using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public bool finalDialogueActive = false;
    public bool evidenceCollected = false;
    public bool evidenceShown = false;
    public bool exitOpened = false;

    public bool CanOpenExit()
{
    return finalDialogueActive
        && evidenceShown
        && !exitOpened;
}
}