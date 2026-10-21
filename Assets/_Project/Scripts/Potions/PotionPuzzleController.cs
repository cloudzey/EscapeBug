using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PotionPuzzleController : MonoBehaviour
{
    [Header("Oyun durumu")]
    [SerializeField] private GameProgress gameProgress;

    [Header("Bocek")]
    [SerializeField] private CompanionMotor companionMotor;
    [SerializeField] private CompanionController companionController;
    [SerializeField] private Transform bugInsidePoint;
    [SerializeField] private InputActionReference commandAction;

    [Header("Oyuncu")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform pourPoint;

    [Header("Mesafeler")]
    [SerializeField, Min(0.01f)]
    private float bugStoppingDistance = 0.08f;

    [SerializeField, Min(0.01f)]
    private float bugPositionTolerance = 0.2f;

    [SerializeField, Min(0.01f)]
    private float playerUseDistance = 1f;

    [SerializeField, Min(0.01f)]
    private float verticalTolerance = 0.5f;

    [SerializeField, Min(1f)]
    private float movementTimeout = 20f;

    [Header("Geri bildirim")]
    [SerializeField] private TMP_Text statusText;

    [Header("Donusum istegi")]
    [SerializeField]
    private UnityEvent onTransformationRequested = new UnityEvent();

    private bool firstLeverLatched;
    private bool movingToStation;
    private bool arrivedByCommand;
    private bool isTransforming;
    private int transformationRequestCount;

    private float movementDeadline;

    public bool FirstLeverLatched => firstLeverLatched;
    public bool IsTransforming => isTransforming;
    public int TransformationRequestCount => transformationRequestCount;

    public event System.Action PuzzleReset;

    public bool IsBugReady =>
        arrivedByCommand &&
        !movingToStation &&
        companionMotor != null &&
        companionMotor.isActiveAndEnabled &&
        !companionMotor.IsMoving &&
        !companionMotor.HasFailed &&
        companionMotor.HasArrived &&
        bugInsidePoint != null &&
        IsNear(
            companionMotor.transform.position,
            bugInsidePoint.position,
            bugPositionTolerance
        );

    private void Awake()
    {
        if (statusText != null)
            statusText.richText = false;
    }

    private void OnEnable()
    {
        if (gameProgress != null)
            gameProgress.ProgressReset += ResetPuzzle;

        if (commandAction != null && commandAction.action != null)
            commandAction.action.Enable();
    }

    private void Update()
    {
        if (!PlayerControlLocks.IsLocked &&
            commandAction != null &&
            commandAction.action != null &&
            commandAction.action.WasPressedThisFrame())
        {
            SendBugToStation();
        }

        if (!movingToStation)
            return;

        if (companionController != null &&
            companionController.isActiveAndEnabled)
        {
            FailMovement("Böceği iki kontrolcü aynı anda yönetiyor.");
            return;
        }

        if (companionMotor == null ||
            !companionMotor.isActiveAndEnabled)
        {
            FailMovement("Böceğin hareket sistemi kullanılamıyor.");
            return;
        }

        if (companionMotor.HasFailed)
        {
            FailMovement(companionMotor.FailureReason);
            return;
        }

        if (companionMotor.HasArrived)
        {
            movingToStation = false;

            arrivedByCommand =
                bugInsidePoint != null &&
                IsNear(
                    companionMotor.transform.position,
                    bugInsidePoint.position,
                    bugPositionTolerance
                );

            SetStatus(arrivedByCommand
                ? "Böcek uygulama noktasında bekliyor."
                : "Böcek uygulama noktasına yeterince yaklaşamadı.");

            return;
        }

        if (Time.time > movementDeadline)
        {
            FailMovement("Böcek hedefe zamanında ulaşamadı.");
        }
    }

    public bool TryLatchFirstLever()
    {
        if (!isActiveAndEnabled)
            return false;

        if (firstLeverLatched)
        {
            SetStatus("İlk kol zaten indirilmiş.");
            return true;
        }

        if (isTransforming)
            return false;

        if (gameProgress == null)
        {
            SetStatus("GameProgress bağlantısı eksik.");
            return false;
        }

        if (!gameProgress.archivePuzzleSolved)
        {
            SetStatus("Önce arşivdeki sembol kilidini çözmelisin.");
            return false;
        }

        firstLeverLatched = true;
        SetStatus("İlk kol indirilip sabitlendi. Böceği F ile bölmeye gönder.");

        return true;
    }

    public void SendBugToStation()
    {
        if (!isActiveAndEnabled ||
            PlayerControlLocks.IsLocked ||
            isTransforming)
        {
            return;
        }

        if (movingToStation)
        {
            SetStatus("Böcek zaten uygulama noktasına gidiyor.");
            return;
        }

        if (!firstLeverLatched)
        {
            SetStatus("Önce ilk kolu indir.");
            return;
        }

        if (gameProgress == null || !gameProgress.archivePuzzleSolved)
        {
            SetStatus("Arşiv bulmacası henüz çözülmedi.");
            return;
        }

        if (companionMotor == null || bugInsidePoint == null)
        {
            SetStatus("Böcek veya uygulama noktası bağlantısı eksik.");
            return;
        }

        if (companionController != null &&
            companionController.isActiveAndEnabled)
        {
            SetStatus("PotionRoom'da CompanionController kapalı olmalı.");
            return;
        }

        if (IsBugReady)
        {
            SetStatus("Böcek zaten uygulama noktasında.");
            return;
        }

        arrivedByCommand = false;
        companionMotor.Stop();

        if (!companionMotor.MoveTo(
            bugInsidePoint.position,
            bugStoppingDistance))
        {
            SetStatus("Böcek gönderilemedi: " + companionMotor.FailureReason);
            return;
        }

        movingToStation = true;
        movementDeadline = Time.time + movementTimeout;

        SetStatus("Böcek bölmeye gidiyor...");
    }

    public bool TryUsePotion(PotionData potion)
    {
        if (!isActiveAndEnabled)
            return false;

        if (isTransforming)
        {
            SetStatus("Dönüşüm isteği zaten gönderildi.");
            return false;
        }

        if (potion == null || string.IsNullOrWhiteSpace(potion.id))
        {
            SetStatus("Geçerli bir iksir seçilmemiş.");
            return false;
        }

        if (gameProgress == null || !gameProgress.archivePuzzleSolved)
        {
            SetStatus("Önce arşiv bulmacası çözülmeli.");
            return false;
        }

        if (!firstLeverLatched)
        {
            SetStatus("Önce ilk kolu indir.");
            return false;
        }

        if (companionController != null &&
            companionController.isActiveAndEnabled)
        {
            SetStatus("Böceğin kontrolcü bağlantılarını kontrol et.");
            return false;
        }

        if (!IsBugReady)
        {
            SetStatus("Böcek bölmedeki uygulama noktasında beklemeli.");
            return false;
        }

        if (player == null || pourPoint == null)
        {
            SetStatus("Oyuncu veya PourPoint bağlantısı eksik.");
            return false;
        }

        if (!IsNear(player.position, pourPoint.position, playerUseDistance))
        {
            SetStatus("İksiri kullanmak için uygulama noktasına yaklaş.");
            return false;
        }

        if (!potion.isCorrect)
        {
            SetStatus("Bu iksir uygun değil. Başka bir şişe deneyebilirsin.");
            return false;
        }

        isTransforming = true;
        transformationRequestCount++;

        companionMotor.Stop();

        SetStatus(
            "DÖNÜŞÜM İSTEĞİ OLUŞTU — İstek sayısı: " +
            transformationRequestCount
        );

        onTransformationRequested.Invoke();
        return true;
    }

    private bool IsNear(Vector3 first, Vector3 second, float horizontalLimit)
    {
        float heightDifference = Mathf.Abs(first.y - second.y);

        first.y = 0f;
        second.y = 0f;

        return heightDifference <= verticalTolerance &&
            Vector3.Distance(first, second) <= horizontalLimit;
    }

    private void FailMovement(string message)
    {
        movingToStation = false;
        arrivedByCommand = false;

        if (companionMotor != null)
            companionMotor.Stop();

        SetStatus("Böcek görevi başarısız: " + message);
    }

    public void ResetPuzzle()
    {
        firstLeverLatched = false;
        movingToStation = false;
        arrivedByCommand = false;
        isTransforming = false;
        transformationRequestCount = 0;

        if (companionMotor != null)
            companionMotor.Stop();

        PuzzleReset?.Invoke();

        SetStatus("İksir bulmacası hazır.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log("PotionPuzzle: " + message, this);
    }

    private void OnDisable()
    {
        if (gameProgress != null)
            gameProgress.ProgressReset -= ResetPuzzle;

        if (commandAction != null && commandAction.action != null)
            commandAction.action.Disable();

        movingToStation = false;
        arrivedByCommand = false;

        if (companionMotor != null)
            companionMotor.Stop();
    }
}