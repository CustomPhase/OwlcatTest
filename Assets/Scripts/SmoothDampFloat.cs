using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SmoothDampFloat
{
    public float Value;
    private float m_Velocity;

    public SmoothDampFloat(float value)
    {
        Value = value;
        m_Velocity = 0;
    }

    public void Update(float targetValue, float smoothTime, float deltaTime)
    {
        Value = Mathf.SmoothDamp(Value, targetValue, ref m_Velocity, smoothTime, 99999, deltaTime);
    }
}
