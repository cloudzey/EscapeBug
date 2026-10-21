using UnityEngine;

public class PotionInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private PotionData potion;
    [SerializeField] private PotionSelectionUI selectionUI;

    public string DisplayName => "İksir etiketini incele";

    public void Interact()
    {
        if (!isActiveAndEnabled || PlayerControlLocks.IsLocked)
            return;

        if (potion == null || selectionUI == null)
        {
            Debug.LogWarning(
                "PotionInteractable: Potion veya Selection UI eksik.",
                this
            );
            return;
        }

        selectionUI.Open(potion);
    }
}