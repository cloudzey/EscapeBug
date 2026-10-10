using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CompanionMotor : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private bool moveOnStart = false;

    [Header("Checks")]
    [SerializeField, Min(0.01f)]
    private float targetSampleRadius = 0.25f;

    [SerializeField, Min(0.001f)]
    private float arrivalTolerance = 0.03f;

    [SerializeField, Min(0.001f)]
    private float stoppedSpeed = 0.05f;

    [SerializeField, Min(1f)]
    private float taskTimeout = 30f;

    [Header("Runtime Debug")]
    [SerializeField] private string state = "Idle";

    private NavMeshAgent agent;
    private Vector3 destination;
    private bool taskActive;
    private float requestTime;
    private int requestFrame;

    public bool HasArrived { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (moveOnStart)
        {
            GoToTarget();
        }
    }

    [ContextMenu("Go To Target")]
    public void GoToTarget()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "CompanionMotor: Önce Play moduna gir.",
                this
            );
            return;
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        HasArrived = false;
        taskActive = false;

        if (!isActiveAndEnabled ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
        {
            Fail("Böcek etkin deðil veya NavMesh üzerinde deðil.");
            return;
        }

        // Önceki görevi temizle.
        agent.isStopped = true;
        agent.ResetPath();

        if (interactionPoint == null)
        {
            Fail("Interaction Point alanýna hedef baðlanmamýþ.");
            return;
        }

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        bool foundTarget = NavMesh.SamplePosition(
            interactionPoint.position,
            out NavMeshHit hit,
            targetSampleRadius,
            filter
        );

        if (!foundTarget)
        {
            Fail("Hedefin yakýnýnda uygun NavMesh bulunamadý.");
            return;
        }

        destination = hit.position;
        agent.isStopped = false;

        if (!agent.SetDestination(destination))
        {
            Fail("Yol isteði kabul edilmedi.");
            return;
        }

        requestTime = Time.time;
        requestFrame = Time.frameCount;
        taskActive = true;
        state = "Calculating";

        Debug.Log(
            "Böcek: Hedefe gitme komutu verildi.",
            this
        );
    }

    private void Update()
    {
        if (!taskActive)
        {
            return;
        }

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            Fail("Hareket sýrasýnda NavMesh baðlantýsý kayboldu.");
            return;
        }

        if (Time.time - requestTime > taskTimeout)
        {
            Fail("Süre doldu; hedefe varýþ doðrulanamadý.");
            return;
        }

        if (Time.frameCount <= requestFrame)
        {
            return;
        }

        if (agent.pathPending)
        {
            state = "Calculating";
            return;
        }

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            Fail("Hedefe tam yol yok: " + agent.pathStatus);
            return;
        }

        float remaining = agent.remainingDistance;

        if (float.IsInfinity(remaining) || float.IsNaN(remaining))
        {
            state = "WaitingForDistance";
            return;
        }

        float threshold =
            agent.stoppingDistance + arrivalTolerance;

        bool closeOnPath = remaining <= threshold;

        bool closeToDestination =
            Vector3.Distance(agent.nextPosition, destination)
            <= threshold;

        bool slowEnough =
            agent.velocity.sqrMagnitude
            <= stoppedSpeed * stoppedSpeed;

        if (closeOnPath && closeToDestination && slowEnough)
        {
            Complete();
            return;
        }

        if (!agent.hasPath && !closeToDestination)
        {
            Fail("Hedefe ulaþmadan yol kayboldu.");
            return;
        }

        state = "Moving";
    }

    private void Complete()
    {
        taskActive = false;
        HasArrived = true;
        state = "Arrived";

        agent.isStopped = true;
        agent.ResetPath();

        Debug.Log(
            "Böcek: Hedefe baþarýyla ulaþtý.",
            this
        );
    }

    private void Fail(string reason)
    {
        taskActive = false;
        HasArrived = false;
        state = "Failed";

        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Debug.LogWarning(
            "Böcek görevi baþarýsýz: " + reason,
            this
        );
    }

    private void OnDisable()
    {
        taskActive = false;
        HasArrived = false;
        state = "Disabled";

        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                interactionPoint.position,
                0.1f
            );
        }

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