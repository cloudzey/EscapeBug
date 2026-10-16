
using UnityEngine;

public class EntranceDoorGate : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private DoorController targetDoor;

    private bool openingTriggered;

    private void Update()
    {
        if (openingTriggered ||
            gameProgress == null ||
            targetDoor == null)
            return;

        if (!gameProgress.CanOpenEntranceDoor())
            return;

        openingTriggered = true;
        targetDoor.Open();
    }
}
