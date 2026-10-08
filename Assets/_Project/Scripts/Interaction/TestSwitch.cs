using UnityEngine;

public class TestSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "Test þalteri";
    [SerializeField] private DoorController targetDoor;

    private bool used;

    public string DisplayName =>
        used ? displayName + " (kullanýldý)" : displayName;

    private void Awake()
    {
        if (targetDoor == null)
        {
            Debug.LogError(
                "TestSwitch: Target Door alanýna kapýyý baðla.",
                this
            );

            enabled = false;
        }
    }

    public void Interact()
    {
        if (!isActiveAndEnabled || used)
        {
            return;
        }

        if (targetDoor == null || !targetDoor.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "TestSwitch: Hedef kapý eksik veya devre dýþý.",
                this
            );

            return;
        }

        used = true;
        targetDoor.Open();

        Debug.Log(
            "Þalter kullanýldý; kapýya Open çaðrýsý gönderildi.",
            this
        );
    }
}