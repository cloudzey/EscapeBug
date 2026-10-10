using UnityEngine;

public class CompanionSensors : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sensorOrigin;

    [Header("Detection")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Min(0.01f)]
    private float detectionDistance = 0.5f;

    [SerializeField, Range(1f, 180f)]
    private float maxTargetAngle = 60f;

    [Header("Forward Sensor - Runtime")]
    [SerializeField] private bool obstacleAhead;
    [SerializeField] private float forwardHitDistance;
    [SerializeField] private GameObject forwardHitObject;

    [Header("Target Sensor - Runtime")]
    [SerializeField] private bool targetReachable;
    [SerializeField] private float targetDistance;
    [SerializeField] private float targetAngle;
    [SerializeField] private string targetReason = "Hedef kontrol edilmedi";

    public string TargetReason => targetReason;

    private bool hasTargetPoint;
    private Vector3 lastTargetPoint;

    private void Awake()
    {
        if (sensorOrigin == null || obstacleMask.value == 0)
        {
            Debug.LogError(
                "CompanionSensors: Sensor Origin ve Obstacle Mask doldurulmalý.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        RaycastHit hit;

        obstacleAhead = FindObstacle(
            sensorOrigin.forward,
            detectionDistance,
            null,
            out hit
        );

        forwardHitDistance =
            obstacleAhead ? hit.distance : detectionDistance;

        forwardHitObject =
            obstacleAhead ? hit.collider.gameObject : null;
    }

    public bool CanReachTarget(
        Transform targetPoint,
        Transform targetRoot)
    {
        targetReachable = false;
        hasTargetPoint = false;

        if (!isActiveAndEnabled || sensorOrigin == null)
        {
            targetReason = "Sensör etkin deðil veya baþlangýç noktasý eksik";
            return false;
        }

        if (targetPoint == null || targetRoot == null)
        {
            targetReason = "Hedef referansý eksik";
            return false;
        }

        hasTargetPoint = true;
        lastTargetPoint = targetPoint.position;

        Vector3 offset =
            targetPoint.position - sensorOrigin.position;

        targetDistance = offset.magnitude;
        targetAngle = Vector3.Angle(
            sensorOrigin.forward,
            offset
        );

        if (targetDistance < 0.001f)
        {
            targetReason = "Hedef sensör baþlangýcýyla ayný noktada";
            return false;
        }

        if (targetDistance > detectionDistance)
        {
            targetReason = "Hedef menzil dýþýnda";
            return false;
        }

        if (targetAngle > maxTargetAngle)
        {
            targetReason = "Hedef bakýþ açýsýnýn dýþýnda";
            return false;
        }

        Vector3 direction = offset / targetDistance;

        RaycastHit blocker;

        if (FindObstacle(
            direction,
            targetDistance,
            targetRoot,
            out blocker))
        {
            targetReason =
                "Arada engel var: " + blocker.collider.name;

            return false;
        }

        targetReachable = true;
        targetReason = "Eriþilebilir";
        return true;
    }

    private bool FindObstacle(
        Vector3 direction,
        float distance,
        Transform ignoredTargetRoot,
        out RaycastHit closestHit)
    {
        closestHit = default(RaycastHit);

        RaycastHit[] hits = Physics.RaycastAll(
            sensorOrigin.position,
            direction,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        bool found = false;
        float closestDistance = float.PositiveInfinity;

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;

            if (hitTransform == transform ||
                hitTransform.IsChildOf(transform))
            {
                continue;
            }

            if (ignoredTargetRoot != null &&
                (hitTransform == ignoredTargetRoot ||
                 hitTransform.IsChildOf(ignoredTargetRoot)))
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
                found = true;
            }
        }

        return found;
    }

    private void OnDrawGizmosSelected()
    {
        if (sensorOrigin == null)
        {
            return;
        }

        Gizmos.color = obstacleAhead ? Color.red : Color.green;

        Gizmos.DrawLine(
            sensorOrigin.position,
            sensorOrigin.position +
            sensorOrigin.forward * detectionDistance
        );

        Gizmos.DrawWireSphere(sensorOrigin.position, 0.015f);

        if (hasTargetPoint)
        {
            Gizmos.color =
                targetReachable ? Color.cyan : Color.magenta;

            Gizmos.DrawLine(
                sensorOrigin.position,
                lastTargetPoint
            );
        }
    }
}