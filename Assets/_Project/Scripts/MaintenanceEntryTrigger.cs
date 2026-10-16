
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MaintenanceEntryTrigger : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;

    private bool triggered;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || gameProgress == null)
            return;

        PlayerMotor player = other.GetComponentInParent<PlayerMotor>();

        if (player == null)
            return;

        if (!gameProgress.entranceRulesRead)
            return;

        triggered = true;
        gameProgress.maintenanceEntered = true;

        Debug.Log(
            "Oyuncu bakım odasına geçti. D0 geri dönüşü kilitlenebilir.",
            this
        );
    }
}
