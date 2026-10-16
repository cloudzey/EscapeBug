
using UnityEngine;
using UnityEngine.InputSystem;

public class RulesPanelController : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;
    private void OnEnable()
    {
        PlayerControlLocks.Acquire(this);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        PlayerControlLocks.Release(this);
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Open()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void Close()
{
    if (!gameObject.activeSelf)
        return;

    if (gameProgress != null)
    {
        gameProgress.entranceRulesRead = true;
    }
    else
    {
        Debug.LogWarning(
            "RulesPanelController: GameProgress atanmadı.",
            this
        );
    }

    gameObject.SetActive(false);
}
}
