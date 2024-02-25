using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask m_LayerMask = Physics.AllLayers;
    public LayerMask LayerMask => m_LayerMask;
    [SerializeField] private InputActionReference m_MoveActionReference;
    
    private NavMeshAgent m_NavMeshAgent;

    private bool m_MoveContinuously = false;
    public bool IsMovingContinuously => m_MoveContinuously;
    private NavMeshPath m_NavMeshPath;
    
    void OnEnable()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_NavMeshPath = new();
        
        m_MoveActionReference.action.Enable();
        m_MoveActionReference.action.performed += MovePerformed;
        m_MoveActionReference.action.canceled += MoveCanceled;
    }

    private void OnDisable()
    {
        m_MoveActionReference.action.Disable();
    }

    private void MovePerformed(InputAction.CallbackContext ctx)
    {
        m_MoveContinuously = true;
    }

    private void MoveCanceled(InputAction.CallbackContext ctx)
    {
        if (m_MoveContinuously)
        {
            m_NavMeshAgent.ResetPath();
            m_MoveContinuously = false;
        }
        else
        {
            MoveToCursor();
        }
    }

    private void MoveToCursor()
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
        if (Physics.Raycast(ray, out var hitInfo, 9999, m_LayerMask))
        {
            //Should be using SetDestination, but it seems to be bugged at the moment -
            //(causes NavMeshAgent's velocity to reset to 0 when targeting unreachable destinations)
            NavMesh.SamplePosition(hitInfo.point, out var navMeshHit, 9999, m_NavMeshAgent.areaMask);
            m_NavMeshAgent.CalculatePath(navMeshHit.position, m_NavMeshPath);
            m_NavMeshAgent.path = m_NavMeshPath;
        }
    }

    private void FixedUpdate()
    {
        if (m_MoveContinuously) MoveToCursor();
    }
}
