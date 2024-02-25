using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CharacterCutoutPosition : MonoBehaviour
{
    [SerializeField] private float m_HeightOffset = 0.5f;
    [SerializeField] private float m_Radius = 1f;

    private readonly int m_PositionProperty = Shader.PropertyToID("_CharacterPosition");
    private readonly int m_RadiusProperty = Shader.PropertyToID("_CharacterRadius");

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector(m_PositionProperty, transform.position + Vector3.up * m_HeightOffset);
        Shader.SetGlobalFloat(m_RadiusProperty, m_Radius);
    }
}
