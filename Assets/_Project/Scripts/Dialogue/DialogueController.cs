using UnityEngine;
using EscapeBug.Dialogue;

public class DialogueController : MonoBehaviour
{
    [Header("Baglantilar")]
    [SerializeField] private NpcActionExecutor actionExecutor;
    [SerializeField] private OllamaClient ollamaClient;
    [SerializeField] private TextAsset felicePrompt;
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private DialogueView view;

    private bool requestInProgress;

    private void ReportError(string message)
    {
        Debug.LogWarning(message, this);

        if (view != null)
            view.ShowError(message);
    }

    public void HandleResponse(NpcResponse response)
    {
        if (response == null)
        {
            ReportError("NPC yaniti bos.");
            return;
        }

        if (!NpcResponseValidator.TryValidate(response, out string error))
        {
            ReportError("Gecersiz NPC yaniti: " + error);
            return;
        }

        if (response.action == "open_exit")
        {
            if (!response.persuaded ||
                gameProgress == null ||
                !gameProgress.CanOpenExit())
            {
                ReportError(
                    "NPC kapi acma istegi reddedildi: " +
                    "Ikna veya oyun kosullari uygun degil."
                );
                return;
            }

            if (actionExecutor == null)
            {
                ReportError("Action Executor baglanmamis!");
                return;
            }
        }

        if (response.action == "request_evidence" &&
            gameProgress != null &&
            gameProgress.evidenceShown)
        {
            ReportError(
                "NPC zaten gosterilen kaniti tekrar istedi. " +
                "Yanit reddedildi."
            );
            return;
        }

        Debug.Log(
            $"Dogrulanmis NPC yaniti — Eylem: {response.action}, " +
            $"Duygu: {response.emotion}, Ikna: {response.persuaded}",
            this
        );

        Debug.Log("Felice: " + response.dialogue, this);

        if (response.action == "open_exit")
        {
            if (!actionExecutor.isActiveAndEnabled)
            {
                ReportError("Cikis eylemi bileseni etkin degil.");
                return;
            }

            if (!actionExecutor.ExecuteOpenExit())
            {
                ReportError(
                    "Cikis kapisi acilamadi. " +
                    "Kapi baglantisini ve oyun kosullarini kontrol et."
                );
                return;
            }
        }

        if (view != null)
        {
            view.ShowReply(response.dialogue);
            view.ShowEmotion(response.emotion);
        }

        if (gameProgress != null)
        {
            Debug.Log(
                "Yanit sonrasi oyun durumu: exitOpened=" +
                (gameProgress.exitOpened ? "true" : "false"),
                this
            );
        }
    }

    public void HandleRawResponse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            ReportError("NPC yaniti bos.");
            return;
        }

        if (!NpcResponseParser.TryParse(
            json,
            out NpcResponse response,
            out string error))
        {
            ReportError("NPC yaniti reddedildi: " + error);
            return;
        }

        HandleResponse(response);
    }

    public void SendPlayerMessage(string playerText)
    {
        if (!isActiveAndEnabled)
        {
            ReportError("Diyalog baglantisi etkin degil.");
            return;
        }

        if (requestInProgress)
            return;

        if (string.IsNullOrWhiteSpace(playerText))
        {
            if (view != null)
                view.SetBusy(false);

            return;
        }

        if (ollamaClient == null ||
            felicePrompt == null ||
            gameProgress == null ||
            view == null)
        {
            ReportError(
                "DialogueController baglantilari eksik. " +
                "Inspector alanlarini kontrol et."
            );
            return;
        }

        if (!gameProgress.finalDialogueActive)
        {
            ReportError("Final diyalogu henuz etkin degil.");
            return;
        }

        if (gameProgress.exitOpened)
        {
            ReportError("Cikis zaten acildi.");
            return;
        }

        if (gameProgress.evidenceShown && !gameProgress.evidenceCollected)
        {
            ReportError(
                "Kanit durumu tutarsiz: " +
                "Toplanmamis kanit gosterilmis olamaz."
            );
            return;
        }

        string systemPrompt = felicePrompt.text
            + "\n\nGERCEK OYUN DURUMU:\n"
            + "evidenceShown="
            + (gameProgress.evidenceShown ? "true" : "false");

        if (gameProgress.evidenceShown)
        {
            systemPrompt +=
                "\n\nFELICE'YE GOSTERILEN GRETE MEKTUBU:\n" +
                "Kafka,\n" +
                "Gidecegin yerin adini duyunca yazmak istedim. " +
                "Oradaki seyleri yalniz etiketlerine bakarak kullanma. " +
                "Yardim eden biri her zaman seninle ayni seyi " +
                "istemiyor olabilir. Bunu bir davet ya da bulusma " +
                "teklifi sanma; yalnizca dikkatli olmani istiyorum. " +
                "Cikinca haber ver.\nGrete";
        }

        ChatMessage[] messages =
        {
            new ChatMessage("system", systemPrompt),
            new ChatMessage("user", playerText.Trim())
        };

        Debug.Log(
            "Felice mesaj kontrolu: " +
            JsonUtility.ToJson(messages[0]) +
            "\n" +
            JsonUtility.ToJson(messages[1]),
            this
        );

        Debug.Log(
            "Felice istegi gonderiliyor: evidenceShown=" +
            (gameProgress.evidenceShown ? "true" : "false"),
            this
        );

        requestInProgress = true;
        view.SetBusy(true);

        StartCoroutine(ollamaClient.SendChat(messages, result =>
        {
            requestInProgress = false;

            if (result == null)
            {
                ReportError("Modelden sonuc alinamadi.");
                return;
            }

            if (!result.Success)
            {
                Debug.LogWarning("Ollama hatasi: " + result.Error, this);
                ReportError(
                    "Modelden yanit alinamadi. Ollama baglantisini kontrol et."
                );
                return;
            }

            Debug.Log("Ollama ham JSON: " + result.Content, this);
            HandleRawResponse(result.Content);
        }));
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (!requestInProgress)
            return;

        requestInProgress = false;

        if (view != null)
            view.ShowError("Istek iptal edildi. Tekrar deneyebilirsin.");
    }

    [ContextMenu("Test Felice Message")]
    private void TestFeliceMessage()
    {
        SendPlayerMessage(
            "Grete'nin mektubu bir aşk mektubu değildi. " +
            "Beni bu yerin tehlikeleri hakkında uyarmıştı. " +
            "Sana gösterdiğim mektupta da bunu görebilirsin. " +
            "Seni kandırmak istemedim, lütfen açıklamamı dinle."
        );
    }

    [ContextMenu("Test/Valid Exit Response")]
    private void TestValidExitResponse()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Bu testi Play modunda çalıştır.", this);
            return;
        }

        if (requestInProgress)
        {
            Debug.LogWarning("Önce mevcut model isteğinin bitmesini bekle.", this);
            return;
        }

        Debug.Log(
            "TEST: Hazır yanıt kullanılıyor; Ollama çağrılmıyor.",
            this
        );

        HandleRawResponse(
            "{\"dialogue\":\"Açıklaman mektuptaki uyarıyla tutarlı. " +
            "Şimdi ne demek istediğini anlıyorum.\"," +
            "\"emotion\":\"calm\"," +
            "\"action\":\"open_exit\"," +
            "\"persuaded\":true}"
        );
    }
}