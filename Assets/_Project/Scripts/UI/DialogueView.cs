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

    [Header("Gonderilecek oyuncu mesaji")]
    [SerializeField] private UnityEvent<string> onMessageSubmitted =
        new UnityEvent<string>();

    private bool isBusy;

    private void Awake()
    {
        // Gelen metni TMP bicimlendirme komutu olarak yorumlama.
        replyText.richText = false;
        errorText.richText = false;

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

        // Yeni cevabi en ustten goster.
        Canvas.ForceUpdateCanvases();
        replyScroll.verticalNormalizedPosition = 1f;
    }

    public void ShowError(string message)
    {
        errorText.text = string.IsNullOrWhiteSpace(message)
            ? "Bir hata oluştu. Tekrar deneyebilirsin."
            : message;

        errorText.gameObject.SetActive(true);
        SetBusy(false);

        // Hata durumunda oyuncunun yazdigi mesaj korunur.
    }
}