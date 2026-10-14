using UnityEngine;

public class NpcActionExecutor : MonoBehaviour
{
    [SerializeField]
    private GameProgress gameProgress;

    [SerializeField]
    private DoorController exitDoor;

    public bool ExecuteOpenExit()
{
    if (gameProgress == null || exitDoor == null)
    {
        Debug.LogError("GameProgress veya ExitDoor eksik!");
        return false;
    }

    if (!gameProgress.CanOpenExit())
    {
        Debug.LogWarning("Kapi acma kosullari saglanmadi.");
        return false;
    }

    exitDoor.Open();

    if (!exitDoor.IsOpening && !exitDoor.IsOpen)
        return false;

    gameProgress.exitOpened = true;
    return true;
}
[ContextMenu("Test Exit Action")]
private void TestExitAction()
{
    bool result = ExecuteOpenExit();
    Debug.Log("Kapi acma sonucu: " + result);
}
}