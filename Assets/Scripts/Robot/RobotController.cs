using UnityEngine;
using UnityEngine.AI;

public class RobotController : MonoBehaviour
{
    public static RobotController Instance;

    private NavMeshAgent _agent;
    private Animator _animator;
    private Transform _currentTarget;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    [SerializeField] private float rotationSpeed = 5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    public void MoveTo(Transform target)
    {
        if (target == null || _agent == null) return;

        _currentTarget = target;
        _agent.SetDestination(target.position);

        if (_animator != null)
        {
            _animator.SetBool(IsWalking, true);
        }
    }

    private void Update()
    {
        if (_agent && !_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (_animator)
                {
                    _animator.SetBool(IsWalking, false);
                }

                if (_currentTarget)
                {
                    Vector3 targetEuler = transform.eulerAngles;
                    targetEuler.y = _currentTarget.eulerAngles.y;

                    Quaternion targetRotation = Quaternion.Euler(targetEuler);
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * rotationSpeed
                    );
                }
            }
        }
    }
}