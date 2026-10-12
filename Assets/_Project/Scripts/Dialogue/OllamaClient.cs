using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace EscapeBug.Dialogue
{
    public class OllamaClient : MonoBehaviour
    {
        [SerializeField]
        private string serverUrl = "http://127.0.0.1:11434";

        [SerializeField]
        private string modelName = "llama3.2:3b";

        [SerializeField, Min(1)]
        private int timeoutSeconds = 60;

        public class ChatResult
        {
            public bool Success;
            public string Content;
            public string Error;
            public long ElapsedMilliseconds;
            public long HttpStatus;
        }

        public IEnumerator SendChat(
            ChatMessage[] messages,
            Action<ChatResult> onCompleted)
        {
            if (string.IsNullOrWhiteSpace(serverUrl) ||
                string.IsNullOrWhiteSpace(modelName))
            {
                onCompleted?.Invoke(new ChatResult
                {
                    Error = "Sunucu adresi veya model adý boþ."
                });
                yield break;
            }

            string endpoint = serverUrl.Trim().TrimEnd('/') + "/api/chat";

            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                onCompleted?.Invoke(new ChatResult
                {
                    Error = "Sunucu adresi geçerli bir HTTP/HTTPS adresi deðil."
                });
                yield break;
            }

            if (messages == null || messages.Length == 0)
            {
                onCompleted?.Invoke(new ChatResult
                {
                    Error = "Gönderilecek mesaj bulunamadý."
                });
                yield break;
            }

            var payload = new ChatRequest
            {
                model = modelName.Trim(),
                messages = messages,
                stream = false
            };

            string json = JsonUtility.ToJson(payload);
            byte[] body = Encoding.UTF8.GetBytes(json);

            var timer = Stopwatch.StartNew();
            var result = new ChatResult();

            using (var request = new UnityWebRequest(
                endpoint, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader(
                    "Content-Type", "application/json; charset=utf-8");
                request.timeout = Mathf.Max(1, timeoutSeconds);

                yield return request.SendWebRequest();

                timer.Stop();

                result.ElapsedMilliseconds = timer.ElapsedMilliseconds;
                result.HttpStatus = request.responseCode;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    result.Error =
                        $"Ýstek baþarýsýz. HTTP: {request.responseCode}. " +
                        $"Hata: {request.error}";
                }
                else
                {
                    string responseBody = request.downloadHandler.text;

                    try
                    {
                        ChatEnvelope envelope =
                            JsonUtility.FromJson<ChatEnvelope>(responseBody);

                        if (envelope == null)
                        {
                            result.Error = "Yanýt zarfý okunamadý.";
                        }
                        else if (!string.IsNullOrWhiteSpace(envelope.error))
                        {
                            result.Error = envelope.error;
                        }
                        else if (!envelope.done)
                        {
                            result.Error = "Tamamlanmamýþ yanýt alýndý.";
                        }
                        else if (envelope.message == null ||
                                 string.IsNullOrWhiteSpace(
                                     envelope.message.content))
                        {
                            result.Error = "Yanýtta message.content boþ.";
                        }
                        else
                        {
                            result.Success = true;
                            result.Content = envelope.message.content;
                        }
                    }
                    catch (Exception exception)
                    {
                        result.Error =
                            "Yanýt zarfý ayrýþtýrýlamadý: " +
                            exception.Message;
                    }
                }
            }

            onCompleted?.Invoke(result);
        }
    }
}