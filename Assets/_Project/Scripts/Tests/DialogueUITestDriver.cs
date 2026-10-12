using System.Collections;
using System.Text;
using UnityEngine;

public class DialogueUITestDriver : MonoBehaviour
{
    public enum TestMode
    {
        Normal,
        LongReply,
        ConnectionError
    }

    [SerializeField] private DialogueView view;
    [SerializeField] private TestMode mode;
    [SerializeField] private float delaySeconds = 3f;

    private Coroutine pending;

    public void SendTestMessage(string message)
    {
        if (!isActiveAndEnabled || pending != null)
            return;

        pending = StartCoroutine(Respond());
    }

    private IEnumerator Respond()
    {
        yield return new WaitForSecondsRealtime(delaySeconds);

        if (mode == TestMode.ConnectionError)
        {
            view.ShowError(
                "[UI TESTİ] Bağlantı kurulamadı. Tekrar deneyebilirsin."
            );
        }
        else if (mode == TestMode.LongReply)
        {
            StringBuilder text = new StringBuilder();

            text.AppendLine("[UI TESTİ — UZUN METİN]");
            text.AppendLine();

            for (int i = 1; i <= 18; i++)
            {
                text.AppendLine(
                    $"{i}. Bu paragraf kaydırma alanını kontrol eder. " +
                    "Metin panelin dışına taşmamalı ve son satıra " +
                    "fare tekerleğiyle ulaşılabilmelidir."
                );
                text.AppendLine();
            }

            view.ShowReply(text.ToString());
        }
        else
        {
            view.ShowReply(
                "[UI TESTİ] Bu bir arayüz deneme yanıtıdır. " +
                "Gerçek model bağlantısı henüz kullanılmıyor."
            );
        }

        pending = null;
    }

    private void OnDisable()
    {
        if (pending == null)
            return;

        StopCoroutine(pending);
        pending = null;

        if (view != null)
            view.SetBusy(false);
    }
}