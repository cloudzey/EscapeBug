
using UnityEngine;

public class RulesCard : MonoBehaviour, IInteractable
{
    [SerializeField]
    private RulesPanelController rulesPanel;

    public string DisplayName => "Kurallar Kartı";

    public void Interact()
    {
        if (rulesPanel == null)
        {
            Debug.LogError(
                "RulesCard: RulesPanel referansı eksik!",
                this
            );

            return;
        }

        rulesPanel.Open();
    }
}
