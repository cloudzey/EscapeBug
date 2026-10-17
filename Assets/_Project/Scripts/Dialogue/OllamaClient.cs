using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    Error = "Sunucu adresi veya model ad� bo�."
                });
                yield break;
            }

            string endpoint = serverUrl.Trim().TrimEnd('/') + "/api/chat";

            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                onCompleted?.Invoke(new ChatResult
                {
                    Error = "Sunucu adresi ge�erli bir HTTP/HTTPS adresi de�il."
                });
                yield break;
            }

            if (messages == null || messages.Length == 0)
            {
                onCompleted?.Invoke(new ChatResult
                {
                    Error = "G�nderilecek mesaj bulunamad�."
                });
                yield break;
            }

            // Mevcut mesaj serilestirmesini koru.
            var payload = new ChatRequest
            {
                model = modelName.Trim(),
                messages = messages,
                stream = false
            };

            JObject requestBody = JObject.Parse(JsonUtility.ToJson(payload));

            requestBody["format"] = new JObject
            {
                ["type"] = "object",
                ["properties"] = new JObject
                {
                    ["dialogue"] = new JObject
                    {
                        ["type"] = "string",
                        ["minLength"] = 1,
                        ["maxLength"] = 600
                    },
                    ["emotion"] = new JObject
                    {
                        ["type"] = "string",
                        ["enum"] = new JArray(
                            "angry",
                            "suspicious",
                            "calm"
                        )
                    },
                    ["action"] = new JObject
                    {
                        ["type"] = "string",
                        ["enum"] = new JArray(
                        "none",
                            "request_evidence",
                            "open_exit"
                        )
                    },
                    ["persuaded"] = new JObject
                    {
                        ["type"] = "boolean"
                    }
                },
                ["required"] = new JArray(
                    "dialogue",
                    "emotion",
                    "action",
                    "persuaded"
                ),
                ["additionalProperties"] = false
            };

            JObject options = requestBody["options"] as JObject;

            if (options == null)
            {
                options = new JObject();
                requestBody["options"] = options;
            }

            options["temperature"] = 0;

            string json = requestBody.ToString(Formatting.None);
            Debug.Log("OLLAMA SON ISTEK: " + json, this);
            byte[] body = Encoding.UTF8.GetBytes(json);

            Debug.Log(
                $"Ollama istek ayarlari: model={modelName.Trim()}, " +
                "format=NPC JSON semasi, temperature=0",
                this
            );

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
                        $"�stek ba�ar�s�z. HTTP: {request.responseCode}. " +
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
                            result.Error = "Yan�t zarf� okunamad�.";
                        }
                        else if (!string.IsNullOrWhiteSpace(envelope.error))
                        {
                            result.Error = envelope.error;
                        }
                        else if (!envelope.done)
                        {
                            result.Error = "Tamamlanmam�� yan�t al�nd�.";
                        }
                        else if (envelope.message == null ||
                                 string.IsNullOrWhiteSpace(
                                     envelope.message.content))
                        {
                            result.Error = "Yan�tta message.content bo�.";
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
                            "Yan�t zarf� ayr��t�r�lamad�: " +
                            exception.Message;
                    }
                }
            }

            onCompleted?.Invoke(result);
        }
    }
}