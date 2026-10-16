
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    [Header("Entrance")]
    public bool entranceRulesRead = false;
    public bool maintenanceEntered = false;

    [Header("Final Dialogue")]
    public bool finalDialogueActive = false;
    public bool evidenceCollected = false;
    public bool evidenceShown = false;
    public bool exitOpened = false;

    public bool CanOpenEntranceDoor()
    {
        return entranceRulesRead;
    }

    public bool CanOpenExit()
    {
        return finalDialogueActive
            && evidenceShown
            && !exitOpened;
    }
}
