using UnityEngine;

public class CompanionMechanism : MonoBehaviour
{
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private Transform sensorTarget;
    [SerializeField] private DualSwitchDoorController doorPuzzle;

    public Transform InteractionPoint => interactionPoint;
    public Transform SensorTarget => sensorTarget;

    public bool IsReady =>
        isActiveAndEnabled &&
        interactionPoint != null &&
        sensorTarget != null &&
        doorPuzzle != null &&
        doorPuzzle.IsReady;

    public bool IsUsed =>
        doorPuzzle != null &&
        doorPuzzle.CompanionSwitchUsed;

    public bool TryActivate()
    {
        if (!IsReady)
            return false;

        if (IsUsed)
            return true;

        return doorPuzzle.TryActivateCompanionSwitch();
    }
}