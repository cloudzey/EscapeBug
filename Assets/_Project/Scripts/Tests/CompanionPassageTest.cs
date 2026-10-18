using UnityEngine;

public class CompanionPassageTest : MonoBehaviour
{
    [SerializeField] private CompanionMotor motor;
    [SerializeField] private CompanionController controller;
    [SerializeField] private Transform insidePoint;
    [SerializeField] private Transform outsidePoint;

    private bool monitoring;
    private float deadline;
    private string targetName;

    [ContextMenu("Test/Go Inside")]
    private void GoInside()
    {
        SendTo(insidePoint);
    }

    [ContextMenu("Test/Go Outside")]
    private void GoOutside()
    {
        SendTo(outsidePoint);
    }

    private void SendTo(Transform target)
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            Debug.LogWarning("Testi Play modunda çalıştır.", this);
            return;
        }

        if (motor == null || target == null)
        {
            Debug.LogError("Motor veya hedef bağlantısı eksik.", this);
            return;
        }

        if (controller != null && controller.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "Bu test için CompanionController bileşenini kapat.",
                this
            );
            return;
        }

        monitoring = false;
        motor.Stop();
        targetName = target.name;

        if (!motor.MoveTo(target.position, 0.08f))
        {
            Debug.LogWarning(
                "Geçiş testi başlatılamadı: " + motor.FailureReason,
                this
            );
            return;
        }

        deadline = Time.time + 20f;
        monitoring = true;
        Debug.Log("Geçiş testi başladı: " + targetName, this);
    }

    private void Update()
    {
        if (!monitoring)
            return;

        if (motor == null)
        {
            monitoring = false;
            Debug.LogError("Test sırasında motor bağlantısı kayboldu.", this);
            return;
        }

        if (motor.HasFailed)
        {
            monitoring = false;
            Debug.LogWarning(
                "Geçiş testi başarısız: " + motor.FailureReason,
                this
            );
        }
        else if (motor.HasArrived)
        {
            monitoring = false;
            Debug.Log("Geçiş hedefinə ulaşıldı: " + targetName, this);
        }
        else if (Time.time > deadline)
        {
            monitoring = false;
            motor.Stop();
            Debug.LogWarning("Geçiş testi 20 saniyede tamamlanamadı.", this);
        }
    }

    private void OnDisable()
    {
        if (monitoring && motor != null)
            motor.Stop();

        monitoring = false;
    }
}