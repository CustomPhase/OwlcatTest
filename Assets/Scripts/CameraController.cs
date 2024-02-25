using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference m_RotateActionReference;
    [SerializeField] private InputActionReference m_ZoomActionReference;
    [SerializeField] private InputActionReference m_PanActionReference;
    
    [Header("Target")]
    [SerializeField] private PlayerController m_Target;
    [SerializeField] private float m_TargetHeightOffset = 0.5f;
    
    [Header("Pan")]
    [SerializeField] private float m_PanSpeed = 10f;
    [SerializeField, Range(0.01f, 1f)] private float m_CameraBlendSmoothing = 0.2f;
    private SmoothDampFloat m_FreeCameraBlend;
    private float m_FreeCameraBlendTarget = 0;
    private Vector3 m_CameraTargetPosition;
    private Vector3 m_FreeCameraTargetPosition;
    
    [Header("Zoom")]
    [SerializeField] private float m_ZoomDistanceMin = 0.3f;
    [SerializeField] private float m_ZoomDistanceMax = 9.9f;
    [SerializeField] private float m_ZoomSensitivity = 0.08f;
    [SerializeField, Range(0.01f, 2f)] private float m_ZoomSmoothing = 0.1f;
    private float m_ZoomTarget = 1;
    private SmoothDampFloat m_Zoom = new(1);

    [Header("Pitch")]
    [SerializeField] private float m_PitchAngleMin = 20;
    [SerializeField] private float m_PitchAngleMax = 60;
    [SerializeField] private AnimationCurve m_PitchAngleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField, Range(0.01f, 2f)] private float m_PitchSmoothing = 0.3f;
    private SmoothDampFloat m_Pitch = new(1);

    [Header("Yaw")]
    [SerializeField] private float m_YawSensitivity = 0.3f;
    private float m_Yaw;

    private void OnEnable()
    {
        m_Yaw = transform.eulerAngles.y % 360;
        m_CameraTargetPosition = m_Target.transform.position;
        m_FreeCameraTargetPosition = m_CameraTargetPosition;
        m_RotateActionReference.action.Enable();
        m_ZoomActionReference.action.Enable();
        m_PanActionReference.action.Enable();
    }

    private void OnDisable()
    {
        m_RotateActionReference.action.Disable();
        m_ZoomActionReference.action.Disable();
        m_PanActionReference.action.Disable();
    }

    void LateUpdate()
    {
        HandleYaw();
        HandleZoom();
        var pitch = HandlePitch();
        UpdateCameraTargetPosition();
        
        transform.eulerAngles = new Vector3(pitch, m_Yaw, 0);
        var zoomDistance = Mathf.Lerp(m_ZoomDistanceMin, m_ZoomDistanceMax, m_Zoom.Value);
        transform.position = (m_CameraTargetPosition + Vector3.up * m_TargetHeightOffset) - transform.forward * zoomDistance;
        
        HandleCameraCollisions();
    }

    private void HandleYaw()
    {
        float delta = m_RotateActionReference.action.ReadValue<Vector2>().x * m_YawSensitivity;
        m_Yaw = (m_Yaw + delta) % 360;
    }

    private void HandleZoom()
    {
        var zoomDelta = m_ZoomActionReference.action.ReadValue<float>();
        //Mouse scroll returns values multiplied by 120 on Windows. Its fixed in Unity 2023, but not in 2022.
        //https://issuetracker.unity3d.com/issues/windows-inputvalue-of-the-scroll-wheel-changes-with-the-step-of-plus-120-when-scrolling-using-input-system
        if (Mathf.Abs(zoomDelta) > 1) zoomDelta /= 120;
        m_ZoomTarget = Mathf.Clamp01(m_ZoomTarget - zoomDelta * m_ZoomSensitivity);
        m_Zoom.Update(m_ZoomTarget, m_ZoomSmoothing, Time.unscaledDeltaTime);
    }

    private float HandlePitch()
    {
        m_Pitch.Update(m_ZoomTarget, m_PitchSmoothing, Time.unscaledDeltaTime);
        return Mathf.Lerp(m_PitchAngleMin, m_PitchAngleMax, m_PitchAngleCurve.Evaluate(m_Pitch.Value));
    }

    private void SnapFreeCameraTargetPositionToGround()
    {
        m_FreeCameraTargetPosition.y = CameraUtils.GetSmoothedCameraTargetHeight(m_FreeCameraTargetPosition, m_Target.LayerMask);
    }

    private void UpdateCameraTargetPosition()
    {
        var panValue = m_PanActionReference.action.ReadValue<Vector2>();
        if (panValue.sqrMagnitude > 0.001f) m_FreeCameraBlendTarget = 1;
        if (m_Target.IsMovingContinuously) m_FreeCameraBlendTarget = 0;

        bool isFreeCamera = m_FreeCameraBlendTarget > 0.5f;
        if (isFreeCamera)
        {
            if (panValue.sqrMagnitude > 1) panValue.Normalize();
            panValue *= m_PanSpeed;
            m_FreeCameraTargetPosition += 
                Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized * panValue.x * Time.unscaledDeltaTime +
                Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * panValue.y * Time.unscaledDeltaTime;
            SnapFreeCameraTargetPositionToGround();
        }
        else
        {
            m_FreeCameraTargetPosition = Vector3.Lerp(m_FreeCameraTargetPosition, m_Target.transform.position, 1 - Mathf.Exp(-Time.unscaledDeltaTime / m_CameraBlendSmoothing));
        }
        
        m_FreeCameraBlend.Update(m_FreeCameraBlendTarget, m_CameraBlendSmoothing, Time.unscaledDeltaTime);
        m_CameraTargetPosition = Vector3.Lerp(m_Target.transform.position, m_FreeCameraTargetPosition, m_FreeCameraBlend.Value);
    }

    private void HandleCameraCollisions()
    {
        var targetPosition = m_CameraTargetPosition + Vector3.up * m_TargetHeightOffset;
        var ray = new Ray(targetPosition, (transform.position - targetPosition).normalized);
        if (Physics.SphereCast(ray, 0.25f, out var hit, m_ZoomDistanceMax+1, m_Target.LayerMask))
        {
            if (hit.distance <= Vector3.Distance(targetPosition, transform.position))
            {
                transform.position = ray.origin + ray.direction * hit.distance;
            }
        }
    }
}
