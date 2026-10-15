using UnityEngine;

public class DualSwitchDoorController : MonoBehaviour
{
    [SerializeField] private DoorController targetDoor;

    [Header("Runtime - sadece takip icin")]
    [SerializeField] private bool playerSwitchUsed;
    [SerializeField] private bool companionSwitchUsed;

    public bool PlayerSwitchUsed => playerSwitchUsed;
    public bool CompanionSwitchUsed => companionSwitchUsed;

    public bool IsReady =>
        isActiveAndEnabled &&
        targetDoor != null &&
        targetDoor.isActiveAndEnabled;

    private void Awake()
    {
        playerSwitchUsed = false;
        companionSwitchUsed = false;

        if (targetDoor == null)
        {
            Debug.LogError(
                "DualSwitchDoorController: Target Door eksik.",
                this
            );

            enabled = false;
        }
    }

    public bool TryActivatePlayerSwitch()
    {
        if (!IsReady)
            return false;

        if (playerSwitchUsed)
            return true;

        playerSwitchUsed = true;
        Debug.Log("Insan salteri etkin.", this);

        TryOpenDoor();
        return true;
    }

    public bool TryActivateCompanionSwitch()
    {
        if (!IsReady)
            return false;

        if (companionSwitchUsed)
            return true;

        companionSwitchUsed = true;
        Debug.Log("Bocek salteri etkin.", this);

        TryOpenDoor();
        return true;
    }

    private void TryOpenDoor()
    {
        if (!playerSwitchUsed || !companionSwitchUsed)
        {
            Debug.Log(
                "D2 kapali: Diger salter bekleniyor.",
                this
            );
            return;
        }

        if (targetDoor.IsOpen || targetDoor.IsOpening)
            return;

        Debug.Log(
            "Iki salter de etkin. D2 aciliyor.",
            this
        );

        targetDoor.Open();
    }
}