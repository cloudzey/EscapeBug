
using UnityEngine;
using UnityEngine.InputSystem;

public class LetterArchiveToggle : MonoBehaviour
{
    [SerializeField] private GameObject archivePanel;
    [SerializeField] private GameObject letterPanel;

    private bool archiveOpen;

    private void Start()
    {
        if (archivePanel == null)
        {
            Debug.LogError("LetterArchiveToggle: Archive Panel eksik!", this);
            enabled = false;
            return;
        }

        archivePanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.iKey.wasPressedThisFrame)
            return;

        if (archiveOpen)
        {
            CloseArchive();
            return;
        }

        // Başka bir panel açıkken arşivi açma.
        if ((letterPanel != null && letterPanel.activeInHierarchy) ||
            PlayerControlLocks.IsLocked)
            return;

        OpenArchive();
    }

    public void OpenArchive()
    {
        if (archiveOpen || archivePanel == null)
            return;

        archiveOpen = true;
        PlayerControlLocks.Acquire(this);
        archivePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseArchive()
    {
        if (!archiveOpen)
            return;

        archiveOpen = false;
        archivePanel.SetActive(false);
        PlayerControlLocks.Release(this);
    }

    private void OnDisable()
    {
        if (archivePanel != null)
            archivePanel.SetActive(false);

        archiveOpen = false;
        PlayerControlLocks.Release(this);
    }
}
