using UnityEngine;

public class SymbolLockInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private SymbolLockUI lockUI;

    public string DisplayName => "Sembol kilidini incele";

    public void Interact()
    {
        if (!isActiveAndEnabled || PlayerControlLocks.IsLocked)
            return;

        if (lockUI == null || !lockUI.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "SymbolLockInteractable: Lock UI baglantisi eksik.",
                this
            );
            return;
        }

        lockUI.Open();
    }
}