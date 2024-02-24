using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PlayerController), typeof(NavMeshAgent))]
public class PlayerPathTargetMarker : MonoBehaviour
{
    [SerializeField] private GameObject m_PathMarkerPrefab;

    [SerializeField] private bool m_ShowWhenMovingContinuously = false;
    
    private PlayerController m_PlayerController;
    private NavMeshAgent m_NavMeshAgent;
    private MeshRenderer m_PathMarker;

    private void OnEnable()
    {
        m_PlayerController = GetComponent<PlayerController>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_PathMarker = Instantiate(m_PathMarkerPrefab).GetComponent<MeshRenderer>();
    }

    void LateUpdate()
    {
        m_PathMarker.transform.position = m_NavMeshAgent.destination;
        bool shouldShow = !m_PlayerController.IsMovingContinuously || m_ShowWhenMovingContinuously;
        m_PathMarker.enabled = m_NavMeshAgent.hasPath && shouldShow;
    }
}
