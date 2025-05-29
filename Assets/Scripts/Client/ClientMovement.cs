using UnityEngine;
using UnityEngine.AI;

public class ClientMovement : MonoBehaviour
{
    private NavMeshAgent _agent;
    private ClientController _client;

    public Transform CurrentTarget { get; private set; }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _client = GetComponent<ClientController>();
    }

    public void MoveTo(Transform target)
    {
        CurrentTarget = target;
        _agent.SetDestination(target.position);
    }

    private void Update()
    {
        if (CurrentTarget && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _client.OnDestinationReached(CurrentTarget);
            CurrentTarget = null;
        }
    }
}