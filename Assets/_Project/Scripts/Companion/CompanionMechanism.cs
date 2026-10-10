using UnityEngine;

public class CompanionMechanism : MonoBehaviour
{
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private Transform sensorTarget;
    [SerializeField] private DoorController targetDoor;

    [Header("Runtime")]
    [SerializeField] private bool used;

    public Transform InteractionPoint => interactionPoint;
    public Transform SensorTarget => sensorTarget;

    public bool IsReady =>
        isActiveAndEnabled &&
        interactionPoint != null &&
        sensorTarget != null &&
        targetDoor != null &&
        targetDoor.isActiveAndEnabled;

    public bool IsUsed =>
        used ||
        (targetDoor != null &&
         (targetDoor.IsOpen || targetDoor.IsOpening));

    public bool TryActivate()
    {
        if (!IsReady)
        {
            return false;
        }

        if (IsUsed)
        {
            return true;
        }

        targetDoor.Open();
        used = true;
        return true;
    }
}