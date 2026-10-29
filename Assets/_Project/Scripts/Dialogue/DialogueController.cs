using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EscapeBug.Dialogue;

public class DialogueController : MonoBehaviour
{
    [Header("Baglantilar")]
    [SerializeField] private NpcActionExecutor actionExecutor;
    [SerializeField] private OllamaClient ollamaClient;
    [SerializeField] private TextAsset felicePrompt;
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private DialogueView view;

    [Header("LLM Experiment")]
    [SerializeField]
    private LlmTrialRecorder llmTrialRecorder;

    [Header("Kanit mektubu")]
    [SerializeField] private LetterData greteLetter;

    [Header("Final arayuzu")]
    [SerializeField] private CanvasGroup dialogueGroup;
    [SerializeField] private Button evidenceButton;
    [SerializeField] private TMP_Text evidenceStatusText;

    [Header("Final girisi ve test")]
    [SerializeField] private FinalRoomEntry finalRoomEntry;

    [Header("Debug")]
    [SerializeField] private bool logContext = true;

    private readonly DialogueContextBuilder context =
        new DialogueContextBuilder();

    private bool requestInProgress;
    private string pendingTurn;
    private string requestState;
    private int requestVersion;
    private OllamaClient.RequestHandle activeRequest;

    private bool previousDialogueActive;
    private string evidenceHint = "";

    private bool DialogueActive =>
        gameProgress != null &&
        gameProgress.finalDialogueActive;

    private bool CanTalk =>
        isActiveAndEnabled &&
        DialogueActive &&
        !gameProgress.exitOpened;

    private bool ValidGreteData =>
        greteLetter != null &&
        greteLetter.id == "letter_grete" &&
        greteLetter.isEvidence &&
        !string.IsNullOrWhiteSpace(greteLetter.body);

    private bool OwnsGreteLetter =>
        ValidGreteData &&
        gameProgress != null &&
        gameProgress.evidenceCollected &&
        gameProgress.HasLetter(greteLetter.id);

    private void OnEnable()
    {
        if (gameProgress != null)
            gameProgress.ProgressReset += ResetConversation;

        previousDialogueActive = DialogueActive;
        RefreshInterface();
    }

    private void Update()
    {
        bool active = DialogueActive;

        if (requestInProgress &&
            (ollamaClient == null || !ollamaClient.isActiveAndEnabled))
        {
            CancelRequest();

            ReportError(
                "Diyalog bağlantısı kapandı. " +
                "Bağlantıyı açıp tekrar deneyebilirsin."
            );
        }

        if (previousDialogueActive && !active)
        {
            ResetConversation();
        }
        else if (requestInProgress &&
                 requestState != CaptureState())
        {
            CancelRequest();
            ReportError(
                "Oyun durumu değişti. Mesajını yeniden gönderebilirsin."
            );
        }

        previousDialogueActive = active;
        RefreshInterface();
    }

    private string CaptureState()
    {
        if (gameProgress == null)
            return "missing";

        return
            gameProgress.finalDialogueActive + "|" +
            gameProgress.evidenceCollected + "|" +
            gameProgress.evidenceShown + "|" +
            gameProgress.exitOpened + "|" +
            OwnsGreteLetter;
    }

    private bool CheckDialogue(out string error)
    {
        error = null;

        if (!CanTalk)
        {
            error = "Final konuşması etkin değil veya çıkış zaten açıldı.";
            return false;
        }

        if (!ValidGreteData)
        {
            error =
                "Grete Letter bağlantısı geçersiz. " +
                "letter_grete kimlikli kanıt mektubunu bağla.";
            return false;
        }

        if (gameProgress.evidenceShown && !OwnsGreteLetter)
        {
            error =
                "Kanıt durumu tutarsız: Gösterilen Grete mektubu " +
                "toplanan mektuplarda bulunmuyor.";
            return false;
        }

        return true;
    }

    public void SendPlayerMessage(string playerText)
    {
        if (requestInProgress)
            return;

        if (string.IsNullOrWhiteSpace(playerText))
        {
            if (view != null)
                view.SetBusy(false);

            return;
        }

        if (ollamaClient == null ||
            !ollamaClient.isActiveAndEnabled ||
            felicePrompt == null ||
            string.IsNullOrWhiteSpace(felicePrompt.text) ||
            view == null)
        {
            ReportError("Diyalog bağlantıları veya Felice promptu eksik.");
            return;
        }

        if (!CheckDialogue(out string error))
        {
            ReportError(error);
            return;
        }

        pendingTurn = playerText.Trim();

        ChatMessage[] messages = context.Build(
            felicePrompt.text,
            gameProgress,
            greteLetter,
            pendingTurn
        );

        requestState = CaptureState();
        int version = ++requestVersion;
        string sentMessage = pendingTurn;

        requestInProgress = true;
        view.SetBusy(true);
        RefreshInterface();

        if (logContext)
        {
            var log = new StringBuilder();

            log.AppendLine(
                "Felice bağlamı — tamamlanmış tur: " +
                context.CompletedTurnCount +
                ", mesaj sayısı: " + messages.Length
            );

            foreach (ChatMessage message in messages)
                log.AppendLine(JsonUtility.ToJson(message));

            Debug.Log(log.ToString(), this);
        }

        activeRequest = new OllamaClient.RequestHandle();
        string stateAtSend = requestState;

        llmTrialRecorder?.BeginTrial(gameProgress.evidenceShown);

        StartCoroutine(ollamaClient.SendChat(
            messages,
            result => ReceiveResult(
                version,
                stateAtSend,
                sentMessage,
                result
            ),
            evidenceShown: gameProgress.evidenceShown,
            canOpenExit: gameProgress.CanOpenExit(),
            handle: activeRequest
        ));

        StartCoroutine(ollamaClient.SendChat(
            messages,
            result => ReceiveResult(
                version,
                stateAtSend,
                sentMessage,
                result
            ),
            evidenceShown: gameProgress.evidenceShown,
            canOpenExit: gameProgress.CanOpenExit(),
            handle: activeRequest
        ));
    }

    private void ReceiveResult(
        int version,
        string stateAtSend,
        string sentMessage,
        OllamaClient.ChatResult result)
    {
        if (version != requestVersion || !isActiveAndEnabled)
        {
            Debug.Log(
                "Eski veya iptal edilmiş yanıt yok sayıldı.",
                this
            );
            return;
        }

        activeRequest = null;
        requestInProgress = false;

        if (view != null)
            view.SetBusy(false);

        if (stateAtSend != CaptureState() ||
            !CheckDialogue(out _))
        {
            llmTrialRecorder?.CancelTrial("game_state_changed");

            ReportError(
                "Yanıt beklenirken oyun durumu değişti. " +
                "Mesajını yeniden gönder."
            );
            return;
        }

        if (result == null)
        {
            llmTrialRecorder?.FinishTrial(
                false,
                "unknown",
                "none",
                "null_result",
                "",
                -1,
                -1
            );

            ReportError(
                "Modelden sonuç alınamadı. Tekrar deneyebilirsin."
            );
            return;
        }

        if (!result.Success)
        {
            llmTrialRecorder?.FinishTrial(
                false,
                "unknown",
                "none",
                result.Kind + ": " + (result.Error ?? ""),
                result.Content,
                result.HttpStatus,
                result.ElapsedMilliseconds
            );

            ReportError(result.UserMessage);
            return;
        }

        Debug.Log(
            "Ollama ham JSON: " + result.Content,
            this
        );

        if (!NpcResponseParser.TryParse(
            result.Content,
            out NpcResponse response,
            out string parseError))
        {
            Debug.LogWarning(
                "InvalidNpcResponse: " + parseError,
                this
            );

            llmTrialRecorder?.FinishTrial(
                false,
                "unknown",
                "none",
                "parse_error: " + parseError,
                result.Content,
                result.HttpStatus,
                result.ElapsedMilliseconds
            );

            ReportError(
                "Geçerli bir yanıt alınamadı. " +
                "Tekrar deneyebilirsin."
            );
            return;
        }

        bool applied = TryApplyResponse(
            response,
            out string applyError
        );

        llmTrialRecorder?.FinishTrial(
            true,
            response.action,
            applied ? response.action : "none",
            applied ? "" : applyError,
            result.Content,
            result.HttpStatus,
            result.ElapsedMilliseconds
        );

        if (!applied)
        {
            Debug.LogWarning(
                "NPC yanıtı uygulanmadı: " + applyError,
                this
            );

            ReportError(
                "Yanıt oyun durumuyla uyuşmadı veya " +
                "eylem uygulanamadı. Tekrar deneyebilirsin."
            );
            return;
        }

        context.CompleteTurn(sentMessage, response);
        pendingTurn = null;

        Debug.Log(
            "Konuşma geçmişi: " +
            context.CompletedTurnCount +
            " tamamlanmış tur.",
            this
        );

        RefreshInterface();
    }

    private bool TryApplyResponse(
        NpcResponse response,
        out string error)
    {
        if (!CheckDialogue(out error))
            return false;

        if (!NpcResponseValidator.TryValidate(response, out error))
            return false;

        if (response.action == "request_evidence" &&
            gameProgress.evidenceShown)
        {
            error = "Gösterilmiş kanıt tekrar istendi; yanıt reddedildi.";
            return false;
        }

        if (response.action == "open_exit")
        {
            if (!response.persuaded ||
                !OwnsGreteLetter ||
                !gameProgress.CanOpenExit())
            {
                error =
                    "Çıkış isteği reddedildi: " +
                    "İkna veya gerçek oyun koşulları uygun değil.";
                return false;
            }

            if (actionExecutor == null ||
                !actionExecutor.isActiveAndEnabled ||
                !actionExecutor.ExecuteOpenExit())
            {
                error = "Çıkış kapısı açılamadı. D5 bağlantısını kontrol et.";
                return false;
            }
        }

        evidenceHint = response.action == "request_evidence"
            ? "Felice mektubu istiyor. Kanıt Göster düğmesini kullan."
            : "";

        if (view != null)
        {
            view.ShowReply(response.dialogue);
            view.ShowEmotion(response.emotion);
        }

        Debug.Log(
            "Doğrulanmış NPC yanıtı: " +
            response.action + ", ikna: " + response.persuaded,
            this
        );

        RefreshInterface();
        return true;
    }

    public void ShowEvidence()
    {
        if (requestInProgress)
            return;

        if (!CheckDialogue(out string error))
        {
            ReportError(error);
            return;
        }

        if (!OwnsGreteLetter)
        {
            evidenceHint = "Mektup bulunmadı.";
            RefreshInterface();
            return;
        }

        if (gameProgress.evidenceShown)
            return;

        gameProgress.evidenceShown = true;
        evidenceHint = "";

        Debug.Log("Gerçek kanıt gösterildi: " + greteLetter.id, this);
        RefreshInterface();
    }

    private void RefreshInterface()
    {
        bool canTalk = CanTalk;

        if (dialogueGroup != null)
        {
            dialogueGroup.alpha = DialogueActive ? 1f : 0f;
            dialogueGroup.interactable =
                canTalk && !requestInProgress;
            dialogueGroup.blocksRaycasts = DialogueActive;
        }

       
if (evidenceButton != null)
{
    evidenceButton.interactable =
        canTalk &&
        !requestInProgress &&
        OwnsGreteLetter &&
        !gameProgress.evidenceShown;
}


        if (evidenceStatusText != null)
        {
            evidenceStatusText.richText = false;

            if (gameProgress != null && gameProgress.evidenceShown)
                evidenceStatusText.text = "Mektup gösterildi.";
            else if (!string.IsNullOrEmpty(evidenceHint))
                evidenceStatusText.text = evidenceHint;
            else
                evidenceStatusText.text = OwnsGreteLetter
                    ? "Grete'nin mektubu sende. Henüz gösterilmedi."
                    : "Grete'nin mektubu sende değil.";
        }
    }

    private void ReportError(string message)
    {
        Debug.LogWarning(message, this);

        if (view != null)
            view.ShowError(message);

        RefreshInterface();
    }

    private void CancelRequest()
    {
        requestVersion++;
        llmTrialRecorder?.CancelTrial("request_cancelled");

        if (activeRequest != null)
        {
            activeRequest.Cancel();
            activeRequest = null;
        }

        StopAllCoroutines();
        requestInProgress = false;

        if (view != null)
            view.SetBusy(false);
    }

    private void ResetConversation()
    {
        CancelRequest();
        context.Clear();
        pendingTurn = null;
        evidenceHint = "";
        RefreshInterface();
    }

    public void HandleRawResponse(string json)
    {
        if (requestInProgress)
            return;

        if (!NpcResponseParser.TryParse(
            json, out NpcResponse response, out string error))
        {
            ReportError(error);
            return;
        }

        HandleResponse(response);
    }

    public void HandleResponse(NpcResponse response)
    {
        if (requestInProgress)
            return;

        if (!TryApplyResponse(response, out string error))
            ReportError(error);
    }

    private void OnDisable()
    {
        if (gameProgress != null)
            gameProgress.ProgressReset -= ResetConversation;

        ResetConversation();
    }

#if UNITY_EDITOR
    [ContextMenu("Test/1 Begin Final Without Evidence")]
    private void TestBeginFinal()
    {
        if (!Application.isPlaying ||
            finalRoomEntry == null ||
            !finalRoomEntry.IsReady ||
            gameProgress == null)
        {
            Debug.LogWarning(
                "FinalRoomTest'i Play'de aç; karartmanın bitmesini bekle " +
                "ve Final Room Entry bağlantısını kontrol et.",
                this
            );
            return;
        }

        if (gameProgress.exitOpened)
        {
            Debug.LogWarning(
                "Kapı zaten açılmış. Yeni test için Play'i yeniden başlat.",
                this
            );
            return;
        }

        gameProgress.ResetForNewGame();
        gameProgress.finalDialogueActive = true;
        previousDialogueActive = true;
        RefreshInterface();

        Debug.Log("TEST: Kanıtsız final konuşması başlatıldı.", this);
    }

    [ContextMenu("Test/2 Collect Grete Letter")]
    private void TestCollectGrete()
    {
        if (!Application.isPlaying ||
            requestInProgress ||
            !CanTalk ||
            !ValidGreteData)
        {
            Debug.LogWarning(
                "Önce final testini başlat ve Grete Letter alanını bağla.",
                this
            );
            return;
        }

        gameProgress.TryCollectLetter(greteLetter);
        evidenceHint = "";
        RefreshInterface();
    }

    [ContextMenu("Test/3 Valid Exit Response")]
    private void TestValidExitResponse()
    {
        if (!Application.isPlaying || requestInProgress)
            return;

        Debug.Log("TEST: Hazır yanıt; Ollama çağrılmıyor.", this);

        HandleResponse(new NpcResponse
        {
            dialogue = "Açıklaman mektuptaki uyarıyla tutarlı.",
            emotion = "calm",
            action = "open_exit",
            persuaded = true
        });
    }

    [ContextMenu("Test/4 Cancel Pending Request")]
    private void TestCancelPendingRequest()
    {
        if (!Application.isPlaying || !requestInProgress)
        {
            Debug.LogWarning(
                "Önce bir mesaj gönder; yanıt beklenirken bu testi çalıştır.",
                this
            );
            return;
        }

        CancelRequest();

        ReportError(
            "İstek test amacıyla iptal edildi. Tekrar deneyebilirsin."
        );
    }

    [ContextMenu("Test/5 Cancel And Inject Stale Exit")]
    private void TestCancelAndInjectStaleExit()
    {
        if (!Application.isPlaying || !requestInProgress)
        {
            Debug.LogWarning(
                "Önce bir mesaj gönder; yanıt beklenirken bu testi çalıştır.",
                this
            );
            return;
        }

        int oldVersion = requestVersion;
        string oldState = requestState;
        string oldMessage = pendingTurn;

        string stateBefore = CaptureState();
        int turnsBefore = context.CompletedTurnCount;

        CancelRequest();

        var staleResult = new OllamaClient.ChatResult
        {
            Kind = OllamaClient.ResultKind.Success,
            Content =
                "{\"dialogue\":\"Açıklaman mektupla tutarlı.\"," +
                "\"emotion\":\"calm\"," +
                "\"action\":\"open_exit\"," +
                "\"persuaded\":true}"
        };

        ReceiveResult(
            oldVersion,
            oldState,
            oldMessage,
            staleResult
        );

        bool unchanged =
            stateBefore == CaptureState() &&
            turnsBefore == context.CompletedTurnCount;

        if (unchanged)
        {
            Debug.Log(
                "TEST GEÇTİ: Eski yanıt oyun durumunu ve geçmişi değiştirmedi.",
                this
            );
        }
        else
        {
            Debug.LogError(
                "TEST BAŞARISIZ: Eski yanıt durumu değiştirdi.",
                this
            );
        }

        ReportError(
            "İstek iptal edildi; eski yanıt testi tamamlandı. " +
            "Tekrar deneyebilirsin."
        );
    }

#endif
}