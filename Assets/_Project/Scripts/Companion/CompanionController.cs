using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CompanionMotor))]
[RequireComponent(typeof(CompanionSensors))]
public class CompanionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CompanionMotor motor;
    [SerializeField] private CompanionSensors sensors;
    [SerializeField] private Transform player;
    [SerializeField] private CompanionMechanism mechanism;

    [Header("Input")]
    [SerializeField] private InputActionReference commandAction;
    [SerializeField] private InputActionReference waitAction;
    [SerializeField] private InputActionReference followAction;

    [Header("Follow")]
    [SerializeField] private float followStartDistance = 1.5f;
    [SerializeField] private float followStopDistance = 1f;
    [SerializeField] private float repathInterval = 0.25f;
    [SerializeField] private float targetMoveThreshold = 0.2f;

    [Header("Task")]
    [SerializeField] private float mechanismStopDistance = 0.08f;
    [SerializeField] private float taskTimeout = 20f;

    [Header("Runtime")]
    [SerializeField] private CompanionState state = CompanionState.Wait;
    [SerializeField] private string lastMessage = "";

    private CompanionMechanism activeMechanism;
    private float deadline;
    private float nextRepathTime;
    private Vector3 lastPlayerGoal;
    private bool hasPlayerGoal;
    private bool followingMovement;

    private bool IsBusy =>
        state == CompanionState.MoveToTarget ||
        state == CompanionState.Interact ||
        state == CompanionState.Return;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<CompanionMotor>();

        if (sensors == null)
            sensors = GetComponent<CompanionSensors>();
    }

    private void OnEnable()
    {
        if (motor == null ||
            sensors == null ||
            player == null ||
            !HasAction(commandAction) ||
            !HasAction(waitAction) ||
            !HasAction(followAction))
        {
            Debug.LogError(
                "CompanionController: Motor, Sensors, Player ve üç Input Action baðlanmalý.",
                this
            );

            enabled = false;
            return;
        }

        commandAction.action.Enable();
        waitAction.action.Enable();
        followAction.action.Enable();
    }

    private void Start()
    {
        ResumeFollow();
    }

    private void Update()
    {
        if (commandAction.action.WasPressedThisFrame())
        {
            CommandMechanism();
            return;
        }

        if (waitAction.action.WasPressedThisFrame())
        {
            WaitHere();
            return;
        }

        if (followAction.action.WasPressedThisFrame())
        {
            ResumeFollow();
            return;
        }

        if (player == null ||
            !motor.isActiveAndEnabled ||
            !sensors.isActiveAndEnabled)
        {
            if (state != CompanionState.Failed)
                FailTask("Oyuncu, motor veya sensör kullanýlamýyor.");

            return;
        }

        if (IsBusy && Time.time > deadline)
        {
            FailTask("Görev aþamasý için süre sýnýrý aþýldý.");
            return;
        }

        switch (state)
        {
            case CompanionState.Follow:
                TickPlayerMovement(false);
                break;

            case CompanionState.Wait:
                break;

            case CompanionState.MoveToTarget:
                TickMoveToTarget();
                break;

            case CompanionState.Interact:
                TickInteract();
                break;

            case CompanionState.Return:
                TickPlayerMovement(true);
                break;

            case CompanionState.Failed:
                break;
        }
    }

    [ContextMenu("Command Mechanism")]
    public void CommandMechanism()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        if (IsBusy)
        {
            Debug.Log("Böcek zaten görevde; yeni komut alýnmadý.", this);
            return;
        }

        if (mechanism == null || !mechanism.IsReady)
        {
            FailTask("Geçerli mekanizma atanmamýþ.");
            return;
        }

        if (mechanism.IsUsed)
        {
            lastMessage = "Bu mekanizma zaten kullanýldý.";
            Debug.Log(lastMessage, this);
            return;
        }

        activeMechanism = mechanism;
        motor.Stop();
        deadline = Time.time + taskTimeout;
        lastMessage = "";

        ChangeState(CompanionState.MoveToTarget);

        if (!motor.MoveTo(
            activeMechanism.InteractionPoint.position,
            mechanismStopDistance))
        {
            FailTask(motor.FailureReason);
        }
    }

    private void TickMoveToTarget()
    {
        if (!ActiveMechanismValid())
            return;

        if (motor.HasFailed)
        {
            FailTask(motor.FailureReason);
            return;
        }

        if (!motor.HasArrived)
            return;

        if (!motor.FaceTarget(
            activeMechanism.SensorTarget.position))
        {
            return;
        }

        if (!sensors.CanReachTarget(
            activeMechanism.SensorTarget,
            activeMechanism.transform))
        {
            FailTask(sensors.TargetReason);
            return;
        }

        ChangeState(CompanionState.Interact);
    }

    private void TickInteract()
    {
        if (!ActiveMechanismValid())
            return;

        if (!sensors.CanReachTarget(
            activeMechanism.SensorTarget,
            activeMechanism.transform))
        {
            FailTask(sensors.TargetReason);
            return;
        }

        if (!activeMechanism.TryActivate())
        {
            FailTask("Mekanizma çalýþtýrýlamadý.");
            return;
        }

        lastMessage = "Mekanizma çalýþtý; oyuncuya dönülüyor.";
        motor.Stop();
        ResetPlayerTracking();

        deadline = Time.time + taskTimeout;
        ChangeState(CompanionState.Return);
    }

    private void TickPlayerMovement(bool returning)
    {
        if (motor.HasFailed)
        {
            FailTask(motor.FailureReason);
            return;
        }

        float distance =
            Vector3.Distance(transform.position, player.position);

        if (distance <= followStopDistance)
        {
            motor.Stop();
            followingMovement = false;
            hasPlayerGoal = false;

            if (returning)
            {
                lastMessage = "Görev tamamlandý; oyuncuya dönüldü.";
                ChangeState(CompanionState.Follow);
            }

            return;
        }

        if (!returning &&
            !followingMovement &&
            distance < followStartDistance)
        {
            return;
        }

        followingMovement = true;

        if (Time.time < nextRepathTime)
            return;

        nextRepathTime = Time.time + repathInterval;

        if (motor.PathPending)
            return;

        bool playerMoved =
            !hasPlayerGoal ||
            Vector3.Distance(player.position, lastPlayerGoal)
            >= targetMoveThreshold;

        if (motor.IsMoving && !playerMoved)
            return;

        lastPlayerGoal = player.position;
        hasPlayerGoal = true;

        if (!motor.MoveTo(
            player.position,
            followStopDistance * 0.9f))
        {
            FailTask(motor.FailureReason);
        }
    }

    [ContextMenu("Wait Here")]
    public void WaitHere()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        if (IsBusy)
        {
            Debug.Log("Görev sürerken bekleme komutu alýnmadý.", this);
            return;
        }

        motor.Stop();
        ResetPlayerTracking();
        lastMessage = "Bekleniyor.";
        ChangeState(CompanionState.Wait);
    }

    [ContextMenu("Resume Follow")]
    public void ResumeFollow()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        if (IsBusy)
        {
            Debug.Log("Görev sürerken takip komutu alýnmadý.", this);
            return;
        }

        motor.Stop();
        ResetPlayerTracking();
        lastMessage = "";
        ChangeState(CompanionState.Follow);
    }

    private bool ActiveMechanismValid()
    {
        if (activeMechanism != null && activeMechanism.IsReady)
            return true;

        FailTask("Görev sýrasýnda mekanizma kullanýlamaz oldu.");
        return false;
    }

    private void ResetPlayerTracking()
    {
        followingMovement = false;
        hasPlayerGoal = false;
        nextRepathTime = 0f;
    }

    private void FailTask(string reason)
    {
        lastMessage = reason;
        motor.Stop();
        ResetPlayerTracking();

        ChangeState(CompanionState.Failed);
        Debug.LogWarning("Böcek görevi baþarýsýz: " + reason, this);
    }

    private void ChangeState(CompanionState next)
    {
        if (state == next)
            return;

        Debug.Log("Companion: " + state + " -> " + next, this);
        state = next;
    }

    private static bool HasAction(InputActionReference reference)
    {
        return reference != null && reference.action != null;
    }

    private void OnDisable()
    {
        if (HasAction(commandAction))
            commandAction.action.Disable();

        if (HasAction(waitAction))
            waitAction.action.Disable();

        if (HasAction(followAction))
            followAction.action.Disable();

        if (motor != null)
            motor.Stop();
    }

    private void OnGUI()
    {
        if (!Debug.isDebugBuild)
            return;

        GUI.Box(
            new Rect(15f, 15f, 490f, 85f),
            "Companion: " + state +
            "\nF: Görev | Q: Bekle | R: Takip" +
            "\n" + lastMessage
        );
    }
}