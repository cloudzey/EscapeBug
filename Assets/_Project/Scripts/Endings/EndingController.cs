using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingController : MonoBehaviour
{
    public enum EndingState
    {
        Exploration,
        Transforming,
        FinalDialogue,
        Reveal,
        End
    }

    [Header("Oyun")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private PotionPuzzleController puzzle;
    [SerializeField] private PotionSelectionUI potionUI;

    [Header("Mevcut oyuncu")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private PlayerMotor playerMotor;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private CharacterController playerCharacter;

    [Header("Mevcut bocek")]
    [SerializeField] private CompanionMotor companionMotor;
    [SerializeField] private NavMeshAgent companionAgent;
    [SerializeField] private Transform bugInsidePoint;

    [Header("Donusum sonrasi bedenler")]
    [SerializeField] private GameObject transformedHumanActor;
    [SerializeField] private GameObject smallKafkaActor;

    [Header("Kamera")]
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private ThirdPersonCamera cameraFollow;
    [SerializeField] private Transform humanRevealCameraPoint;
    [SerializeField] private Transform roomExitCameraPoint;

    [Header("Kararma")]
    [SerializeField] private Image fadeImage;
    [SerializeField, Min(0.05f)] private float fadeDuration = 0.5f;
    [SerializeField, Min(0f)] private float blackHoldDuration = 0.15f;

    [Header("Ikinci kol sekansi")]
    [SerializeField] private CharacterController humanCharacter;
    [SerializeField] private Transform leverBStandPoint;
    [SerializeField] private Transform leverBHandle;

    [SerializeField] private DoorController chamberDoor;
    [SerializeField] private DoorController exitDoor;

    [SerializeField, Min(0.1f)]
    private float humanWalkSpeed = 1.2f;

    [SerializeField, Min(1f)]
    private float walkTimeout = 15f;

    [SerializeField, Min(0.1f)]
    private float leverDuration = 0.5f;

    [SerializeField]
    private Vector3 leverRotationOffset = new Vector3(0f, 0f, -55f);

    [Header("Bolmeden cikis")]
    [SerializeField] private Transform beforeChamberPoint;
    [SerializeField] private Transform chamberExitPoint;
    [SerializeField] private Transform afterChamberPoint;

    [Header("Final sahnesine gecis")]
    [SerializeField] private Transform[] finalApproachPoints;

    [SerializeField]
    private string finalSceneName = "FinalRoomTest";

    private bool loadingFinalScene;

    private bool exitPointReached;
    private bool chamberExitCompleted;

    private Collider[] originalPlayerColliders;
    private bool[] originalPlayerColliderStates;

    private Quaternion initialLeverRotation;
    private bool secondLeverCompleted;

    private Renderer[] playerRenderers;
    private Renderer[] bugRenderers;
    private bool[] playerRendererStates;
    private bool[] bugRendererStates;

    private bool playerMotorWasEnabled;
    private bool playerInteractionWasEnabled;
    private bool companionMotorWasEnabled;
    private bool companionAgentWasEnabled;
    private bool cameraFollowWasEnabled;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private CursorLockMode originalCursorLock;
    private bool originalCursorVisible;

    private bool capturedState;
    private Coroutine sequence;

    public EndingState State { get; private set; }
        = EndingState.Exploration;

    public bool BodySwapCompleted { get; private set; }

    private void Awake()
    {
        if (leverBHandle != null)
            initialLeverRotation = leverBHandle.localRotation;

        if (transformedHumanActor != null)
            transformedHumanActor.SetActive(false);

        if (smallKafkaActor != null)
            smallKafkaActor.SetActive(false);

        SetFade(0f);

        if (fadeImage != null)
            fadeImage.raycastTarget = false;
    }

    private void OnEnable()
    {
        if (gameProgress != null)
            gameProgress.ProgressReset += ResetSequence;
    }

    public bool CanStartTransformation(out string error)
    {
        error = null;

        if (!isActiveAndEnabled)
        {
            error = "EndingController etkin degil.";
            return false;
        }

        if (State != EndingState.Exploration || capturedState)
        {
            error = "Donusum zaten basladi.";
            return false;
        }

        if (gameProgress == null || puzzle == null || potionUI == null ||
            playerRoot == null || playerMotor == null ||
            playerInteraction == null || playerCharacter == null ||
            companionMotor == null || companionAgent == null ||
            bugInsidePoint == null ||
            transformedHumanActor == null || smallKafkaActor == null ||
            sceneCamera == null || cameraFollow == null ||
            humanRevealCameraPoint == null || fadeImage == null)
        {
            error = "EndingController Inspector baglantilari eksik.";
            return false;
        }

        if (!sceneCamera.isActiveAndEnabled ||
            !fadeImage.isActiveAndEnabled)
        {
            error = "Kamera ve FadeImage etkin olmali.";
            return false;
        }

        if (cameraFollow.gameObject != sceneCamera.gameObject)
        {
            error = "Camera Follow alanina Main Camera'daki " +
                "ThirdPersonCamera bilesenini bagla.";
            return false;
        }

        if (transformedHumanActor == smallKafkaActor ||
            transformedHumanActor.transform.IsChildOf(playerRoot) ||
            transformedHumanActor.transform.IsChildOf(
                companionMotor.transform) ||
            smallKafkaActor.transform.IsChildOf(playerRoot) ||
            smallKafkaActor.transform.IsChildOf(
                companionMotor.transform))
        {
            error = "Yeni bedenler Player veya Companion altinda olmamali.";
            return false;
        }
    if (humanCharacter == null ||
        leverBStandPoint == null ||
        leverBHandle == null ||
        chamberDoor == null ||
        exitDoor == null)
    {
        error = "Ikinci kol veya kapi baglantilari eksik.";
        return false;
    }

    if (humanCharacter.gameObject != transformedHumanActor)
    {
        error = "Human Character alanina TransformedHumanActor " +
            "uzerindeki CharacterController baglanmali.";
        return false;
    }

    if (!humanCharacter.enabled ||
        !chamberDoor.isActiveAndEnabled ||
        !exitDoor.isActiveAndEnabled ||
        !leverBHandle.gameObject.activeInHierarchy)
    {
        error = "Insan CharacterController, kapilar ve kol etkin olmali.";
        return false;
    }

    if (chamberDoor == exitDoor)
    {
        error = "Chamber Door ve Exit Door farkli kapilar olmali.";
        return false;
    }

    if (beforeChamberPoint == null ||
        chamberExitPoint == null ||
        afterChamberPoint == null)
    {
        error = "Bolmeden cikis noktalarini bagla.";
        return false;
    }

    if (finalApproachPoints == null ||
        finalApproachPoints.Length == 0)
    {
        error = "Final cikis rotasi baglanmamis.";
        return false;
    }

    foreach (Transform point in finalApproachPoints)
    {
        if (point == null)
        {
            error = "Final cikis rotasinda bos nokta var.";
            return false;
        }
    }

    if (string.IsNullOrWhiteSpace(finalSceneName) ||
        !Application.CanStreamedLevelBeLoaded(finalSceneName))
    {
        error = "Final sahnesi Scene List'e eklenmemis veya adi yanlis.";
        return false;
    }

    return true;
}

    public void BeginTransformation()
    {
        if (!CanStartTransformation(out string error))
        {
            Debug.LogWarning(error, this);
            return;
        }

        if (!puzzle.IsTransforming ||
            puzzle.TransformationRequestCount != 1 ||
            !puzzle.FirstLeverLatched ||
            !gameProgress.archivePuzzleSolved)
        {
            Debug.LogWarning(
                "Donusum icin kabul edilmis bir iksir istegi yok.",
                this
            );
            return;
        }

        CaptureState();

        PlayerControlLocks.Acquire(this);
        potionUI.Close();

        playerMotor.enabled = false;
        playerInteraction.enabled = false;
        cameraFollow.enabled = false;

        companionMotor.Stop();
        companionMotor.enabled = false;
        companionAgent.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ChangeState(EndingState.Transforming);
        sequence = StartCoroutine(TransformationRoutine());
    }

    private void CaptureState()
    {
        playerMotorWasEnabled = playerMotor.enabled;
        playerInteractionWasEnabled = playerInteraction.enabled;
        companionMotorWasEnabled = companionMotor.enabled;
        companionAgentWasEnabled = companionAgent.enabled;
        cameraFollowWasEnabled = cameraFollow.enabled;

        originalCameraPosition = sceneCamera.transform.position;
        originalCameraRotation = sceneCamera.transform.rotation;
        originalCursorLock = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        playerRenderers =
            playerRoot.GetComponentsInChildren<Renderer>(true);

        bugRenderers =
            companionMotor.GetComponentsInChildren<Renderer>(true);

        playerRendererStates = CaptureRenderers(playerRenderers);
        bugRendererStates = CaptureRenderers(bugRenderers);

        originalPlayerColliders =
            playerRoot.GetComponentsInChildren<Collider>(true);

        originalPlayerColliderStates =
            new bool[originalPlayerColliders.Length];

        for (int i = 0; i < originalPlayerColliders.Length; i++)
        {
            originalPlayerColliderStates[i] =
                originalPlayerColliders[i] != null &&
                originalPlayerColliders[i].enabled;
        }

        capturedState = true;
    }

    private IEnumerator TransformationRoutine()
    {
        Debug.Log("Ending: Ekran karariyor.", this);
        yield return FadeTo(1f);

        yield return null;

        SwapBodies();

        sceneCamera.transform.SetPositionAndRotation(
            humanRevealCameraPoint.position,
            humanRevealCameraPoint.rotation
        );

        if (blackHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(blackHoldDuration);

        Debug.Log("Ending: Insan gorunumu gosteriliyor.", this);
        yield return FadeTo(0f);

        Debug.Log(
            "Ending: Beden degisimi tamamlandi. Ikinci kola geciliyor.",
            this
        );

        yield return new WaitForSecondsRealtime(0.75f);
        yield return UseSecondLeverRoutine();

        sequence = null;
    }

    private IEnumerator UseSecondLeverRoutine()
{
    if (State != EndingState.Transforming ||
        !BodySwapCompleted ||
        !puzzle.FirstLeverLatched ||
        secondLeverCompleted)
    {
        Debug.LogError(
            "Ending: Ikinci kol icin donusum kosullari uygun degil.",
            this
        );
        yield break;
    }

    if (!humanCharacter.gameObject.activeInHierarchy ||
        !humanCharacter.enabled)
    {
        Debug.LogError("Ending: Insan bedeni etkin degil.", this);
        yield break;
    }

    Debug.Log("Ending: Insan ikinci kola yuruyor.", this);

    float deadline = Time.unscaledTime + walkTimeout;
    float verticalSpeed = -2f;

    while (true)
    {
        Vector3 offset =
            leverBStandPoint.position -
            humanCharacter.transform.position;

        float heightDifference = Mathf.Abs(offset.y);
        offset.y = 0f;

        if (offset.magnitude <= 0.08f &&
            heightDifference <= 0.2f)
        {
            break;
        }

        if (Time.unscaledTime > deadline)
        {
            Debug.LogError(
                "Ending: Insan LeverB noktasina ulasamadi. " +
                "Hedefin konumunu ve aradaki collider'lari kontrol et.",
                this
            );
            yield break;
        }

        float dt = Mathf.Min(Time.deltaTime, 0.05f);

        if (humanCharacter.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -2f;
        else
            verticalSpeed += -20f * dt;

        Vector3 movement = Vector3.ClampMagnitude(
            offset,
            humanWalkSpeed * dt
        );

        movement.y = verticalSpeed * dt;
        humanCharacter.Move(movement);

        if (offset.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(offset);

            humanCharacter.transform.rotation =
                Quaternion.RotateTowards(
                    humanCharacter.transform.rotation,
                    targetRotation,
                    240f * dt
                );
        }

        yield return null;
    }

    float turnDeadline = Time.unscaledTime + 3f;

    while (true)
    {
        Vector3 direction =
            leverBHandle.position -
            humanCharacter.transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            break;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        humanCharacter.transform.rotation =
            Quaternion.RotateTowards(
                humanCharacter.transform.rotation,
                targetRotation,
                240f * Time.unscaledDeltaTime
            );

        if (Quaternion.Angle(
            humanCharacter.transform.rotation,
            targetRotation) <= 2f)
        {
            break;
        }

        if (Time.unscaledTime > turnDeadline)
        {
            Debug.LogError("Ending: Kola donus tamamlanamadi.", this);
            yield break;
        }

        yield return null;
    }

    Debug.Log("Ending: Ikinci kol indiriliyor.", this);

    Quaternion startRotation = leverBHandle.localRotation;
    Quaternion endRotation =
        initialLeverRotation * Quaternion.Euler(leverRotationOffset);

    float elapsed = 0f;
    float duration = Mathf.Max(0.1f, leverDuration);

    while (elapsed < duration)
    {
        elapsed += Time.unscaledDeltaTime;

        float t = Mathf.SmoothStep(
            0f,
            1f,
            Mathf.Clamp01(elapsed / duration)
        );

        leverBHandle.localRotation =
            Quaternion.Slerp(startRotation, endRotation, t);

        yield return null;
    }

    leverBHandle.localRotation = endRotation;
    secondLeverCompleted = true;

    chamberDoor.Open();
    exitDoor.Open();

    float doorDeadline = Time.unscaledTime + 10f;

    while (!chamberDoor.IsOpen || !exitDoor.IsOpen)
    {
        if (!chamberDoor.isActiveAndEnabled ||
            !exitDoor.isActiveAndEnabled ||
            Time.unscaledTime > doorDeadline)
        {
            Debug.LogError(
                "Ending: Kapilarin acilmasi tamamlanamadi. " +
                "Door Visual baglantilarini kontrol et.",
                this
            );
            yield break;
        }

        yield return null;
    }

    Debug.Log(
        "Ending: Ikinci kol tamamlandi. ChamberDoor ve D4 acildi.",
        this
    );

    yield return new WaitForSecondsRealtime(0.3f);
    if (sceneCamera != null && roomExitCameraPoint != null)
    {
    sceneCamera.transform.SetPositionAndRotation(
            roomExitCameraPoint.position,
            roomExitCameraPoint.rotation
        );
    }

    yield return WalkOutOfChamber();
}

private IEnumerator WalkOutOfChamber()
{
    chamberExitCompleted = false;

    if (!BodySwapCompleted ||
        !secondLeverCompleted ||
        !chamberDoor.IsOpen ||
        !exitDoor.IsOpen)
    {
        Debug.LogError(
            "Ending: Donusum, ikinci kol ve kapilar " +
            "tamamlanmadan bolmeden cikilamaz.",
            this
        );
        yield break;
    }

    Transform[] route =
    {
        beforeChamberPoint,
        chamberExitPoint,
        afterChamberPoint
    };

    Debug.Log("Ending: Insan bolmeden cikiyor.", this);

    foreach (Transform point in route)
    {
        yield return WalkHumanToPoint(point);

        if (!exitPointReached)
            yield break;
    }

    chamberExitCompleted = true;

    Debug.Log(
        "Ending: Insan ChamberDoor'dan cikti. " +
        "Final sahnesine gecis noktasi hazir.",
        this
    );

    yield return GoToFinalScene();
}

private IEnumerator WalkHumanToPoint(Transform point)
{
    exitPointReached = false;

    if (point == null ||
        humanCharacter == null ||
        !humanCharacter.enabled ||
        !humanCharacter.gameObject.activeInHierarchy)
    {
        Debug.LogError(
            "Ending: Yuruyus hedefi veya insan bedeni kullanilamiyor.",
            this
        );
        yield break;
    }

    float deadline = Time.unscaledTime + walkTimeout;
    float verticalSpeed = -2f;

    while (true)
    {
        Vector3 offset =
            point.position - humanCharacter.transform.position;

        float heightDifference = Mathf.Abs(offset.y);
        offset.y = 0f;

        if (offset.magnitude <= 0.08f &&
            heightDifference <= 0.2f)
        {
            exitPointReached = true;

            Debug.Log(
                "Ending: Cikis noktasina ulasildi: " + point.name,
                this
            );
            yield break;
        }

        if (Time.unscaledTime > deadline)
        {
            Debug.LogError(
                "Ending: Cikis noktasina ulasilamadi: " +
                point.name +
                ". Noktanin yuksekligini ve yolu kontrol et.",
                this
            );
            yield break;
        }

        float dt = Mathf.Min(Time.deltaTime, 0.05f);

        if (humanCharacter.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -2f;
        else
            verticalSpeed += -20f * dt;

        Vector3 movement = Vector3.ClampMagnitude(
            offset,
            humanWalkSpeed * dt
        );

        movement.y = verticalSpeed * dt;
        humanCharacter.Move(movement);

        if (offset.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(offset);

            humanCharacter.transform.rotation =
                Quaternion.RotateTowards(
                    humanCharacter.transform.rotation,
                    desiredRotation,
                    240f * dt
                );
        }

        yield return null;
    }
}

private void ResetSecondLever()
{
    secondLeverCompleted = false;

    if (leverBHandle != null)
        leverBHandle.localRotation = initialLeverRotation;

    if (chamberDoor != null)
        chamberDoor.ResetToClosed();

    if (exitDoor != null)
        exitDoor.ResetToClosed();
}

    private void SwapBodies()
    {
        Vector3 humanPosition = companionMotor.transform.position;
        humanPosition.y = bugInsidePoint.position.y;

        Vector3 smallPosition = playerRoot.position;
        smallPosition.y = playerCharacter.bounds.min.y;

        foreach (Collider item in originalPlayerColliders)
        {
            if (item != null)
                item.enabled = false;
        }

        transformedHumanActor.transform.SetPositionAndRotation(
            humanPosition,
            Quaternion.Euler(
                0f,
                companionMotor.transform.eulerAngles.y,
                0f
            )
        );

        smallKafkaActor.transform.SetPositionAndRotation(
            smallPosition,
            Quaternion.Euler(0f, playerRoot.eulerAngles.y, 0f)
        );

        SetRenderers(playerRenderers, false);
        SetRenderers(bugRenderers, false);

        transformedHumanActor.SetActive(true);
        smallKafkaActor.SetActive(true);

        BodySwapCompleted = true;

        Debug.Log(
            "Ending: Bedenler bulunduklari yerde degisti.",
            this
        );
    }

    private IEnumerator FadeTo(float target)
    {
        float start = fadeImage.color.a;
        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            SetFade(Mathf.Lerp(
                start,
                target,
                Mathf.Clamp01(elapsed / duration)
            ));

            yield return null;
        }

        SetFade(target);
    }

    private void SetFade(float alpha)
    {
        if (fadeImage == null)
            return;

        fadeImage.color = new Color(0f, 0f, 0f, alpha);
    }

    private void ChangeState(EndingState next)
    {
        State = next;

        if (gameProgress != null)
        {
            gameProgress.finalDialogueActive =
                next == EndingState.FinalDialogue;
        }

        Debug.Log("Ending asamasi: " + next, this);
    }

    private static bool[] CaptureRenderers(Renderer[] renderers)
    {
        bool[] states = new bool[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            states[i] = renderers[i] != null && renderers[i].enabled;

        return states;
    }

    private static void SetRenderers(
        Renderer[] renderers,
        bool visible)
    {
        if (renderers == null)
            return;

        foreach (Renderer item in renderers)
        {
            if (item != null)
                item.enabled = visible;
        }
    }

    private static void RestoreRenderers(
        Renderer[] renderers,
        bool[] states)
    {
        if (renderers == null || states == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = states[i];
        }
    }

    private void ResetSequence()
    {
        if (sequence != null)
        {
            StopCoroutine(sequence);
            sequence = null;
        }

        if (capturedState)
        {
            ResetSecondLever();
            if (originalPlayerColliders != null &&
                originalPlayerColliderStates != null)
            {
                for (int i = 0; i < originalPlayerColliders.Length; i++)
                {
                    if (originalPlayerColliders[i] != null)
                    {
                        originalPlayerColliders[i].enabled =
                            originalPlayerColliderStates[i];
                    }
                }
            }

            RestoreRenderers(playerRenderers, playerRendererStates);
            RestoreRenderers(bugRenderers, bugRendererStates);

            if (sceneCamera != null)
            {
                sceneCamera.transform.SetPositionAndRotation(
                    originalCameraPosition,
                    originalCameraRotation
                );
            }

            if (playerMotor != null)
                playerMotor.enabled = playerMotorWasEnabled;

            if (playerInteraction != null)
                playerInteraction.enabled = playerInteractionWasEnabled;

            if (companionAgent != null)
                companionAgent.enabled = companionAgentWasEnabled;

            if (companionMotor != null)
                companionMotor.enabled = companionMotorWasEnabled;

            if (cameraFollow != null)
                cameraFollow.enabled = cameraFollowWasEnabled;

            Cursor.lockState = originalCursorLock;
            Cursor.visible = originalCursorVisible;
        }

        if (transformedHumanActor != null)
            transformedHumanActor.SetActive(false);

        if (smallKafkaActor != null)
            smallKafkaActor.SetActive(false);

        capturedState = false;
        BodySwapCompleted = false;

        exitPointReached = false;
        chamberExitCompleted = false;

        SetFade(0f);
        PlayerControlLocks.Release(this);
        ChangeState(EndingState.Exploration);
    }

    private void OnDisable()
    {
        if (gameProgress != null)
            gameProgress.ProgressReset -= ResetSequence;

        if (loadingFinalScene)
        {
            StopAllCoroutines();
            PlayerControlLocks.Release(this);
            return;
        }

        ResetSequence();
    }

    private IEnumerator GoToFinalScene()
    {
        if (!chamberExitCompleted ||
            !secondLeverCompleted ||
            !BodySwapCompleted ||
            loadingFinalScene)
        {
            yield break;
        }

        foreach (Transform point in finalApproachPoints)
        {
            yield return WalkHumanToPoint(point);

            if (!exitPointReached)
                yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(finalSceneName))
        {
            Debug.LogError("Ending: Final sahnesi yuklenemiyor.", this);
            yield break;
        }

        Debug.Log("Ending: Final sahnesine geciliyor.", this);

        yield return FadeTo(1f);
        yield return null;

        FinalSceneTransfer.Prepare(gameProgress, finalSceneName);

        AsyncOperation operation = null;

        try
        {
            operation = SceneManager.LoadSceneAsync(
                finalSceneName,
                LoadSceneMode.Single
            );
        }
        catch (System.Exception exception)
        {
            Debug.LogError(
                "Ending: Sahne gecisi basarisiz: " + exception.Message,
                this
            );
        }

        if (operation == null)
        {
            FinalSceneTransfer.Clear();
            yield return FadeTo(0f);
            yield break;
        }

        loadingFinalScene = true;

        while (!operation.isDone)
            yield return null;
    }

}