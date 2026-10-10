using UnityEngine;

[RequireComponent(typeof(CompanionMotor))]
[RequireComponent(typeof(CompanionSensors))]
public class CompanionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CompanionMotor motor;
    [SerializeField] private CompanionSensors sensors;
    [SerializeField] private Transform mechanismRoot;
    [SerializeField] private Transform sensorTarget;
    [SerializeField] private DoorController targetDoor;

    [Header("Runtime Debug")]
    [SerializeField] private bool sensorAllowsInteraction;
    [SerializeField] private bool canActivate;
    [SerializeField] private bool activated;
    [SerializeField] private string status = "Hazýr";

    private void Awake()
    {
        if (motor == null)
        {
            motor = GetComponent<CompanionMotor>();
        }

        if (sensors == null)
        {
            sensors = GetComponent<CompanionSensors>();
        }

        if (motor == null ||
            sensors == null ||
            mechanismRoot == null ||
            sensorTarget == null ||
            targetDoor == null)
        {
            Debug.LogError(
                "CompanionController: Inspector referanslarýný doldur.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        RefreshAccess();
    }

    private void RefreshAccess()
    {
        sensorAllowsInteraction =
            sensors.CanReachTarget(sensorTarget, mechanismRoot);

        canActivate =
            motor.isActiveAndEnabled &&
            motor.HasArrived &&
            sensorAllowsInteraction &&
            targetDoor.isActiveAndEnabled &&
            !targetDoor.IsOpen &&
            !targetDoor.IsOpening &&
            !activated;

        if (activated)
        {
            status = "Mekanizma çalýþtýrýldý";
        }
        else if (!motor.HasArrived)
        {
            status = "Önce yürüme hedefine ulaþ";
        }
        else if (!sensorAllowsInteraction)
        {
            status = sensors.TargetReason;
        }
        else if (!targetDoor.isActiveAndEnabled)
        {
            status = "Kapý bileþeni etkin deðil";
        }
        else if (targetDoor.IsOpen || targetDoor.IsOpening)
        {
            status = "Kapý zaten açýk veya açýlýyor";
        }
        else
        {
            status = "Etkileþime hazýr";
        }
    }

    [ContextMenu("Go To Mechanism")]
    public void GoToMechanism()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        motor.GoToTarget();
    }

    [ContextMenu("Face Mechanism")]
    public void FaceMechanism()
    {
        if (!Application.isPlaying ||
            !isActiveAndEnabled ||
            !motor.HasArrived)
        {
            return;
        }

        Vector3 direction =
            sensorTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    [ContextMenu("Try Activate Mechanism")]
    public void TryActivateMechanism()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        RefreshAccess();

        if (!canActivate)
        {
            Debug.Log(
                "Mekanizma reddedildi: " + status,
                this
            );

            return;
        }

        targetDoor.Open();
        activated = true;
        canActivate = false;
        status = "Mekanizma çalýþtýrýldý";

        Debug.Log(
            "Sensör eriþimi doðruladý; kapýya Open gönderildi.",
            this
        );
    }
}