using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshAgentAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    [SerializeField] private string m_RunParameterName = "Run";
    [SerializeField] private float m_RunAnimationStopDistance = 0.15f;
    
    [SerializeField] private string m_RunSpeedParameterName = "RunSpeed";
    [SerializeField] private float m_RunAnimationRootVelocity = 4;

    private NavMeshAgent m_NavMeshAgent;

    void OnEnable()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        m_Animator.SetFloat(m_RunSpeedParameterName, Vector3.ProjectOnPlane(m_NavMeshAgent.velocity, Vector3.up).magnitude / m_RunAnimationRootVelocity);
        m_Animator.SetBool(m_RunParameterName, m_NavMeshAgent.remainingDistance > m_RunAnimationStopDistance);
    }
}
