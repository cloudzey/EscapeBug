using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FinalEndingController : MonoBehaviour
{
    public enum FinalStage
    {
        WaitingForExit,
        ReadingFeliceReply,
        FinalWords,
        ReadyForReveal,
        Revealing,
        Reveal,
        End,
        Failed
    }

    [Header("Baglantilar")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private NpcActionExecutor actionExecutor;
    [SerializeField] private FinalStoryPanel storyPanel;
    [SerializeField] private DoorController exitDoor;
    
    [Header("Reveal")]
    [SerializeField] private FinalRoomEntry finalRoomEntry;
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Transform revealCameraAnchor;
    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject humanVisualRoot;
    [SerializeField] private PlayerMotor finalKafkaMotor;
    [SerializeField] private GameObject endPanel;

    [SerializeField, Min(0.05f)]
    private float fadeDuration = 0.5f;

    [Header("Sureler")]
    [SerializeField, Min(1f)]
    private float lastReplyDuration = 5f;

    [SerializeField, Min(1f)]
    private float doorTimeout = 10f;

    [Header("Runtime")]
    [SerializeField]
    private FinalStage stage = FinalStage.WaitingForExit;

    public FinalStage Stage => stage;

    private bool started;

    private void Awake()
    {
        if (finalKafkaMotor != null)
            finalKafkaMotor.gameObject.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (actionExecutor != null)
            actionExecutor.ExitOpened += HandleExitOpened;
    }

    private void HandleExitOpened()
    {
        if (!isActiveAndEnabled || started)
            return;

        if (gameProgress == null ||
            storyPanel == null ||
            !storyPanel.isActiveAndEnabled ||
            exitDoor == null ||
            !exitDoor.isActiveAndEnabled ||
            finalRoomEntry == null ||
            sceneCamera == null ||
            !sceneCamera.isActiveAndEnabled ||
            revealCameraAnchor == null ||
            fadeImage == null ||
            !fadeImage.isActiveAndEnabled ||
            humanVisualRoot == null ||
            finalKafkaMotor == null ||
            !finalKafkaMotor.enabled ||
            endPanel == null)
        {
            Fail("Final baglantilari eksik veya devre disi.");
            return;
        }

        CharacterController character =
            finalKafkaMotor.GetComponent<CharacterController>();

        if (character == null || !character.enabled)
        {
            Fail("Final boceginin CharacterController bileseni etkin olmali.");
            return;
        }

        if (!gameProgress.finalDialogueActive ||
            !gameProgress.exitOpened ||
            !gameProgress.evidenceCollected ||
            !gameProgress.evidenceShown)
        {
            Fail("Finali baslatmak icin oyun kosullari uygun degil.");
            return;
        }

        started = true;
        PlayerControlLocks.Acquire(this);

        StartCoroutine(FinalRoutine());
    }

    private IEnumerator FinalRoutine()
    {
        stage = FinalStage.ReadingFeliceReply;

        yield return null;

        yield return new WaitForSecondsRealtime(lastReplyDuration);

        float deadline = Time.unscaledTime + doorTimeout;

        while (!exitDoor.IsOpen)
        {
            if (!exitDoor.isActiveAndEnabled ||
                Time.unscaledTime > deadline)
            {
                Fail("D5 acilma animasyonu tamamlanamadi.");
                yield break;
            }

            yield return null;
        }

        gameProgress.finalDialogueActive = false;

        yield return null;

        if (storyPanel.IsPlaying)
        {
            Fail("Baska bir yazili konusma halen oynuyor.");
            yield break;
        }

        stage = FinalStage.FinalWords;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return storyPanel.PlayEnding();

        if (!storyPanel.Completed)
        {
            Fail("Final repligi tamamlanamadi.");
            yield break;
        }

        stage = FinalStage.ReadyForReveal;
        yield return RevealRoutine();
    }

    private IEnumerator RevealRoutine()
    {
        stage = FinalStage.Revealing;

        fadeImage.raycastTarget = true;

        yield return FadeTo(1f);

        humanVisualRoot.SetActive(false);

        ThirdPersonCamera follow =
            sceneCamera.GetComponent<ThirdPersonCamera>();

        if (follow != null)
            follow.enabled = false;

        sceneCamera.transform.SetPositionAndRotation(
            revealCameraAnchor.position,
            revealCameraAnchor.rotation
        );

        finalKafkaMotor.gameObject.SetActive(true);

        yield return null;

        if (!finalKafkaMotor.isActiveAndEnabled)
        {
            yield return FadeTo(0f);
            fadeImage.raycastTarget = false;

            Fail("Bocek motoru acilamadi. Move Action baglantisini kontrol et.");
            yield break;
        }

        yield return FadeTo(0f);
        fadeImage.raycastTarget = false;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current
                .SetSelectedGameObject(null);
        }

        PlayerControlLocks.Release(finalRoomEntry);
        PlayerControlLocks.Release(this);

        stage = FinalStage.Reveal;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            "Final: Gercek Kafka gorunur. Bocegi WASD ile cikisa gotur.",
            this
        );
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / duration)
            );

            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0f, 0f, 0f, targetAlpha);
    }

    public bool TryFinish(Collider entrant)
    {
        if (!isActiveAndEnabled ||
            stage != FinalStage.Reveal ||
            entrant == null ||
            finalKafkaMotor == null ||
            !finalKafkaMotor.isActiveAndEnabled ||
            gameProgress == null ||
            !gameProgress.exitOpened ||
            exitDoor == null ||
            !exitDoor.IsOpen ||
            endPanel == null)
        {
            return false;
        }

        PlayerMotor enteringMotor =
            entrant.GetComponentInParent<PlayerMotor>();

        if (enteringMotor != finalKafkaMotor)
            return false;

        stage = FinalStage.End;

        PlayerControlLocks.Acquire(this);
        finalKafkaMotor.enabled = false;

        endPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Final: Kafka bocek olarak cikti. Oyun tamamlandi.", this);
        return true;
    }

    private void Fail(string message)
    {
        stage = FinalStage.Failed;
        Debug.LogError("Final: " + message, this);
    }

    private void OnDisable()
    {
        if (actionExecutor != null)
            actionExecutor.ExitOpened -= HandleExitOpened;

        StopAllCoroutines();

        if (started && storyPanel != null)
            storyPanel.CancelPlayback();

        PlayerControlLocks.Release(this);
    }
}