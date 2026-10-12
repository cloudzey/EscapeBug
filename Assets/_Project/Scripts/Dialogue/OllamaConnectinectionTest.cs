using System.Collections;
using UnityEngine;

namespace EscapeBug.Dialogue
{
    [RequireComponent(typeof(OllamaClient))]
    public class OllamaConnectionTest : MonoBehaviour
    {
        [SerializeField]
        private Transform rotatingObject;

        [SerializeField, TextArea(2, 5)]
        private string playerText =
            "Merhaba, baðlantý testi yapýyoruz. Kýsaca kendini tanýt.";

        private OllamaClient client;
        private bool busy;
        private int framesWhileWaiting;
        private float requestStartedAt;

        private string status = "Hazýr";
        private string lastResponse = "";

        private void Awake()
        {
            client = GetComponent<OllamaClient>();
        }

        private void Update()
        {
            if (rotatingObject != null)
            {
                rotatingObject.Rotate(
                    new Vector3(20f, 60f, 10f) * Time.deltaTime,
                    Space.Self);
            }

            if (busy)
            {
                framesWhileWaiting++;
            }
        }

        private void SendTest()
        {
            if (busy || string.IsNullOrWhiteSpace(playerText))
            {
                return;
            }

            StartCoroutine(RunTest());
        }

        private IEnumerator RunTest()
        {
            busy = true;
            framesWhileWaiting = 0;
            requestStartedAt = Time.realtimeSinceStartup;

            status = "Yanýt bekleniyor...";
            lastResponse = "";

            var messages = new[]
            {
                new ChatMessage(
                    "system",
                    "Türkçe cevap ver. Yanýtýn en fazla iki cümle olsun."),

                new ChatMessage("user", playerText)
            };

            Debug.Log("[OllamaTest] Ýstek gönderiliyor.");

            yield return client.SendChat(messages, result =>
            {
                busy = false;

                string measurement =
                    $"Süre: {result.ElapsedMilliseconds} ms | " +
                    $"Beklerken ilerleyen kare: {framesWhileWaiting} | " +
                    $"HTTP: {result.HttpStatus}";

                if (result.Success)
                {
                    status = "Baþarýlý — " + measurement;
                    lastResponse = result.Content;

                    Debug.Log("[OllamaTest] " + measurement);
                    Debug.Log(
                        "[OllamaTest] Model yanýtý:\n" + result.Content);
                }
                else
                {
                    status = "Baþarýsýz — " + measurement;
                    lastResponse = result.Error;

                    Debug.LogWarning(
                        "[OllamaTest] " + measurement +
                        "\n" + result.Error);
                }
            });

            busy = false;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            busy = false;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(
                new Rect(15, 15, 460, 360),
                GUI.skin.box);

            GUILayout.Label("Ollama baðlantý testi");
            GUILayout.Label(status);

            if (busy)
            {
                float elapsed =
                    Time.realtimeSinceStartup - requestStartedAt;

                GUILayout.Label(
                    $"Bekleme: {elapsed:F1} saniye | " +
                    $"Ýlerleyen kare: {framesWhileWaiting}");
            }

            playerText = GUILayout.TextArea(
                playerText, GUILayout.Height(70));

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !busy;

            if (GUILayout.Button("Mesaj gönder"))
            {
                SendTest();
            }

            GUI.enabled = previousEnabled;

            GUILayout.Label(
                lastResponse,
                new GUIStyle(GUI.skin.label) { wordWrap = true });

            GUILayout.EndArea();
        }
    }
}