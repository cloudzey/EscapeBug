using UnityEngine;

public class GameProgress : MonoBehaviour
{
    [Header("Sahne baslangici")]
    [SerializeField] private bool resetOnSceneStart = false;

    [Header("Entrance")]
    public bool entranceRulesRead = false;
    public bool maintenanceEntered = false;

    [Header("Final Dialogue")]
    public bool finalDialogueActive = false;
    public bool evidenceCollected = false;
    public bool evidenceShown = false;
    public bool exitOpened = false;

    private void Start()
    {
        if (resetOnSceneStart)
        {
            ResetForNewGame();
        }
    }

    public void ResetForNewGame()
    {
        entranceRulesRead = false;
        maintenanceEntered = false;

        finalDialogueActive = false;
        evidenceCollected = false;
        evidenceShown = false;
        exitOpened = false;

        Debug.Log("Yeni oyun ilerlemesi sıfırlandı.", this);
    }

    public bool CanOpenEntranceDoor()
    {
        return entranceRulesRead;
    }

    public bool CanOpenExit()
    {
        return finalDialogueActive
            && evidenceCollected
            && evidenceShown
            && !exitOpened;
    }

    [ContextMenu("Test/Reset Progress For New Game")]
    private void TestResetForNewGame()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Bu testi Play modunda çalıştır.",
                this
            );

            return;
        }

        ResetForNewGame();
    }
}