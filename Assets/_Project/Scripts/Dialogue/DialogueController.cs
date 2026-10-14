using UnityEngine;
using System.Collections;
using EscapeBug.Dialogue;

public class DialogueController : MonoBehaviour
{
    [SerializeField]
    private NpcActionExecutor actionExecutor;
    [SerializeField]
     private OllamaClient ollamaClient;
    [SerializeField]
    private TextAsset felicePrompt;
    [SerializeField]
    private GameProgress gameProgress;

    public void HandleResponse(NpcResponse response)
    {
        if (!NpcResponseValidator.TryValidate(
            response, out string error))
        {
            Debug.LogWarning("Gecersiz NPC yaniti: " + error);
            return;
        }
        if (response.action == "open_exit" &&
    (gameProgress == null || !gameProgress.CanOpenExit()))
{
    Debug.LogWarning(
        "NPC kapi acma istegi reddedildi: Oyun kosullari uygun degil."
    );
    return;
}
if (response.action == "request_evidence" &&
    gameProgress != null &&
    gameProgress.evidenceShown)
{
    Debug.LogWarning(
        "NPC zaten gosterilen kaniti tekrar istedi. Yanit reddedildi."
    );
    return;
}
        Debug.Log("Felice: " + response.dialogue);
        if (response.action == "open_exit")
{
    if (actionExecutor == null)
    {
        Debug.LogError("Action Executor baglanmamis!");
        return;
    }

    actionExecutor.ExecuteOpenExit();
}
    }
    public void HandleRawResponse(string json)
{
    if (string.IsNullOrWhiteSpace(json))
    {
        Debug.LogWarning("NPC yaniti bos.");
        return;
    }

    try
    {
        NpcResponse response =
            JsonUtility.FromJson<NpcResponse>(json);

        HandleResponse(response);
    }
    catch (System.Exception exception)
    {
        Debug.LogWarning(
            "NPC JSON hatasi: " + exception.Message);
    }
}
public void SendPlayerMessage(string playerText)
{
    if (ollamaClient == null || felicePrompt == null)
    {
        Debug.LogError("OllamaClient veya FelicePrompt eksik!");
        return;
    }

    if (string.IsNullOrWhiteSpace(playerText))
        return;

    if (gameProgress == null)
{
    Debug.LogError("GameProgress baglanmamis!");
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

    StartCoroutine(ollamaClient.SendChat(messages, result =>
    {
        if (!result.Success)
        {
            Debug.LogError("Ollama hatasi: " + result.Error);
            return;
        }
        Debug.Log("Ollama ham JSON: " + result.Content);

        HandleRawResponse(result.Content);
    }));
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

}