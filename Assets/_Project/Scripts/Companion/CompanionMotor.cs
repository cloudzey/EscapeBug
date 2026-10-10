using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CompanionMotor : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField, Min(0.01f)]
    private float sampleRadius = 0.25f;

    [SerializeField, Min(0.001f)]
    private float arrivalTolerance = 0.03f;

    [SerializeField, Min(0.001f)]
    private float stoppedSpeed = 0.05f;

    [SerializeField, Min(0.1f)]
    private float pathCalculationTimeout = 5f;

    private NavMeshAgent agent;
    private Vector3 destination;
    private float requestTime;
    private int requestFrame;

    public bool IsMoving { get; private set; }
    public bool HasArrived { get; private set; }
    public bool HasFailed { get; private set; }
    public string FailureReason { get; private set; }

    public bool PathPending =>
        agent != null &&
        agent.isActiveAndEnabled &&
        agent.isOnNavMesh &&
        agent.pathPending;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public bool MoveTo(Vector3 target, float stoppingDistance)
    {
        HasArrived = false;
        HasFailed = false;
        FailureReason = "";

        if (!isActiveAndEnabled ||
            agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
        {
            Fail("Böcek NavMesh üzerinde deðil veya motor etkin deðil.");
            return false;
        }

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (!NavMesh.SamplePosition(
            target,
            out NavMeshHit hit,
            sampleRadius,
            filter))
        {
            Fail("Hedefin yakýnýnda uygun NavMesh bulunamadý.");
            return false;
        }

        destination = hit.position;
        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;

        if (!agent.SetDestination(destination))
        {
            Fail("Yol isteði kabul edilmedi.");
            return false;
        }

        requestTime = Time.time;
        requestFrame = Time.frameCount;
        IsMoving = true;
        return true;
    }

    private void Update()
    {
        if (!IsMoving)
        {
            return;
        }

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            Fail("NavMesh baðlantýsý kayboldu.");
            return;
        }

        if (Time.frameCount <= requestFrame)
        {
            return;
        }

        if (agent.pathPending)
        {
            if (Time.time - requestTime > pathCalculationTimeout)
            {
                Fail("Yol hesaplama süresi aþýldý.");
            }

            return;
        }

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            Fail("Tam yol bulunamadý: " + agent.pathStatus);
            return;
        }

        float remaining = agent.remainingDistance;

        if (float.IsInfinity(remaining) || float.IsNaN(remaining))
        {
            if (Time.time - requestTime > pathCalculationTimeout)
            {
                Fail("Kalan yol mesafesi hesaplanamadý.");
            }

            return;
        }

        float threshold =
            agent.stoppingDistance + arrivalTolerance;

        bool closeOnPath = remaining <= threshold;

        bool closeToGoal =
            Vector3.Distance(agent.nextPosition, destination)
            <= threshold;

        bool slowEnough =
            agent.velocity.sqrMagnitude
            <= stoppedSpeed * stoppedSpeed;

        if (closeOnPath && closeToGoal && slowEnough)
        {
            HaltAgent();
            IsMoving = false;
            HasArrived = true;
            return;
        }

        if (!agent.hasPath && !closeToGoal)
        {
            Fail("Hedefe ulaþmadan yol kayboldu.");
        }
    }

    public bool FaceTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        Quaternion desiredRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRotation,
            360f * Time.deltaTime
        );

        return Quaternion.Angle(
            transform.rotation,
            desiredRotation
        ) <= 2f;
    }

    public void Stop()
    {
        HaltAgent();
        IsMoving = false;
        HasArrived = false;
        HasFailed = false;
        FailureReason = "";
    }

    private void Fail(string reason)
    {
        HaltAgent();
        IsMoving = false;
        HasArrived = false;
        HasFailed = true;
        FailureReason = reason;
    }

    private void HaltAgent()
    {
        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnDisable()
    {
        Stop();
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying ||
            agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh ||
            !agent.hasPath)
        {
            return;
        }

        Vector3[] corners = agent.path.corners;
        Gizmos.color = Color.cyan;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Gizmos.DrawLine(
                corners[i] + Vector3.up * 0.03f,
                corners[i + 1] + Vector3.up * 0.03f
            );
        }
    }
}