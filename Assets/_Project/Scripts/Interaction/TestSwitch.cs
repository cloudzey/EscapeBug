using UnityEngine;

public class TestSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "Bakim salteri";
    [SerializeField] private DualSwitchDoorController doorPuzzle;

    public string DisplayName =>
        doorPuzzle != null && doorPuzzle.PlayerSwitchUsed
            ? displayName + " (kullanildi)"
            : displayName;

    private void Awake()
    {
        if (doorPuzzle == null)
        {
            Debug.LogError(
                "TestSwitch: Door Puzzle baglantisi eksik.",
                this
            );

            enabled = false;
        }
    }

    public void Interact()
    {
        if (!isActiveAndEnabled)
            return;

        if (doorPuzzle == null || !doorPuzzle.IsReady)
        {
            Debug.LogWarning(
                "TestSwitch: Kapi bulmacasi hazir degil.",
                this
            );
            return;
        }

        doorPuzzle.TryActivatePlayerSwitch();
    }
}