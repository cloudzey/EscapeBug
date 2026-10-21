using UnityEngine;

public class PotionPourInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private PotionSelectionUI selectionUI;

    public string DisplayName => "Seçili iksiri uygula";

    public void Interact()
    {
        if (!isActiveAndEnabled || PlayerControlLocks.IsLocked)
            return;

        if (selectionUI == null)
        {
            Debug.LogWarning(
                "PotionPourInteractable: Selection UI eksik.",
                this
            );
            return;
        }

        selectionUI.UseSelectedPotion();
    }
}