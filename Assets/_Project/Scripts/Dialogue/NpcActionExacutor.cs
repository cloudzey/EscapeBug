using UnityEngine;

public class NpcActionExecutor : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private DoorController exitDoor;

    public bool ExecuteOpenExit()
    {
        if (!isActiveAndEnabled ||
            gameProgress == null ||
            exitDoor == null ||
            !exitDoor.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "Çıkış bağlantıları eksik veya devre dışı.",
                this
            );
            return false;
        }

        if (!gameProgress.CanOpenExit())
        {
            Debug.LogWarning(
                "Kapı açma koşulları sağlanmadı.",
                this
            );
            return false;
        }

        if (exitDoor.IsOpen || exitDoor.IsOpening)
        {
            Debug.LogWarning(
                "Kapı zaten açık veya açılıyor.",
                this
            );
            return false;
        }

        exitDoor.Open();

        if (!exitDoor.IsOpening && !exitDoor.IsOpen)
            return false;

        gameProgress.exitOpened = true;
        return true;
    }
}