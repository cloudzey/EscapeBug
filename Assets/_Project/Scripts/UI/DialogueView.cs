using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour
{
    [Header("UI referanslari")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text replyText;
    [SerializeField] private TMP_Text busyText;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private ScrollRect replyScroll;

    [Header("Duygu gostergesi")]
    [SerializeField] private TMP_Text emotionText;

    [Header("Gonderilecek oyuncu mesaji")]
    [SerializeField] private UnityEvent<string> onMessageSubmitted =
        new UnityEvent<string>();

    private bool isBusy;

    private void Awake()
    {
        replyText.richText = false;
        errorText.richText = false;

        ShowEmotion("");
        busyText.gameObject.SetActive(false);
        errorText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        sendButton.onClick.AddListener(Submit);
        messageInput.onValueChanged.AddListener(OnInputChanged);
        RefreshSendButton();
    }

    private void OnDisable()
    {
        sendButton.onClick.RemoveListener(Submit);
        messageInput.onValueChanged.RemoveListener(OnInputChanged);
    }

    private void OnInputChanged(string value)
    {
        RefreshSendButton();
    }

    private void RefreshSendButton()
    {
        sendButton.interactable =
            !isBusy &&
            !string.IsNullOrWhiteSpace(messageInput.text);
    }

    public void Submit()
    {
        if (isBusy)
            return;

        string message = messageInput.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
            return;

        errorText.text = "";
        errorText.gameObject.SetActive(false);

        SetBusy(true);
        onMessageSubmitted.Invoke(message);
    }

    public void SetBusy(bool busy)
    {
        isBusy = busy;
        busyText.gameObject.SetActive(busy);
        messageInput.interactable = !busy;
        RefreshSendButton();
    }

    public void ShowReply(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
        {
            ShowError("Boş yanıt alındı. Tekrar deneyebilirsin.");
            return;
        }

        replyText.text = reply;
        errorText.text = "";
        errorText.gameObject.SetActive(false);

        messageInput.SetTextWithoutNotify("");
        SetBusy(false);

        Canvas.ForceUpdateCanvases();
        replyScroll.verticalNormalizedPosition = 1f;
    }

    public void ShowEmotion(string emotion)
    {
        if (emotionText == null)
            return;

        switch (emotion)
        {
            case "angry":
                emotionText.text = "Duygu: Kızgın";
                emotionText.color = new Color32(255, 140, 120, 255);
            break;

            case "suspicious":
                emotionText.text = "Duygu: Şüpheci";
                emotionText.color = new Color32(255, 210, 100, 255);
            break;

            case "calm":
                emotionText.text = "Duygu: Sakin";
                emotionText.color = new Color32(230, 230, 230, 255);
            break;

            default:
                emotionText.text = "Duygu: —";
                emotionText.color = Color.white;
            break;
        }
    }

    public void ShowError(string message)
    {
        errorText.text = string.IsNullOrWhiteSpace(message)
            ? "Bir hata oluştu. Tekrar deneyebilirsin."
            : message;

        errorText.gameObject.SetActive(true);
        SetBusy(false);
    }
}