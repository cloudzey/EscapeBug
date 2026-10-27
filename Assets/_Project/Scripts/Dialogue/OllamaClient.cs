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
        private string modelName = "qwen2.5:7b";

        [SerializeField, Min(1)]
        private int timeoutSeconds = 180;

        public enum ResultKind
        {
            Success,
            ConnectionError,
            Timeout,
            HttpError,
            InvalidEnvelope,
            InvalidNpcResponse
        }

        public class ChatResult
        {
            public ResultKind Kind = ResultKind.ConnectionError;

            public bool Success => Kind == ResultKind.Success;

            public string Content;
            public string Error;
            public bool ModelNotFound;
            public long ElapsedMilliseconds;
            public long HttpStatus;

            public string UserMessage
            {
                get
                {
                    switch (Kind)
                    {
                        case ResultKind.ConnectionError:
                            return "Diyalog servisine bağlanılamadı. " +
                                   "Bağlantıyı kontrol edip tekrar deneyebilirsin.";

                        case ResultKind.Timeout:
                            return "Yanıt zamanında gelmedi. " +
                                   "Tekrar deneyebilirsin.";

                        case ResultKind.HttpError:
                            return ModelNotFound
                                ? "Seçili model bulunamadı. Model ayarını kontrol et."
                                : "Diyalog servisi isteği tamamlayamadı. " +
                                  "Tekrar deneyebilirsin.";

                        case ResultKind.InvalidEnvelope:
                        case ResultKind.InvalidNpcResponse:
                            return "Geçerli bir yanıt alınamadı. " +
                                   "Tekrar deneyebilirsin.";

                        default:
                            return "";
                    }
                }
            }
        }

        public sealed class RequestHandle
        {
            private UnityWebRequest request;

            public bool IsCancelled { get; private set; }

            internal void Attach(UnityWebRequest value)
            {
                request = value;
            }

            public void Cancel()
            {
                if (IsCancelled)
                    return;

                IsCancelled = true;

                if (request != null)
                    request.Abort();

                Release();
            }

            internal void Release()
            {
                if (request == null)
                    return;

                request.Dispose();
                request = null;
            }
        }

#if UNITY_EDITOR
        public enum TestResponse
        {
            None,
            InvalidEnvelope,
            BrokenNpcJson,
            WrongBooleanType,
            UnknownAction
        }

        [Header("Editor testi - sonraki istekte bir kez kullanilir")]
        [SerializeField]
        private TestResponse nextTestResponse = TestResponse.None;
#endif

        public IEnumerator SendChat(
            ChatMessage[] messages,
            Action<ChatResult> onCompleted,
            bool evidenceShown = false,
            bool canOpenExit = false,
            RequestHandle handle = null)
        {
            if (handle == null)
                handle = new RequestHandle();

            if (handle.IsCancelled)
                yield break;

#if UNITY_EDITOR
            TestResponse test = nextTestResponse;
            nextTestResponse = TestResponse.None;

            if (test != TestResponse.None)
            {
                yield return null;

                if (handle.IsCancelled)
                    yield break;

                ChatResult simulated =
                    ParseEnvelope(BuildTestEnvelope(test));

                simulated.HttpStatus = 200;

                LogResult(simulated, true);
                onCompleted?.Invoke(simulated);
                yield break;
            }
#endif

            UnityWebRequest request;
            UnityWebRequestAsyncOperation operation;
            string setupError;

            var timer = Stopwatch.StartNew();

            if (!TryStartRequest(
                messages,
                evidenceShown,
                canOpenExit,
                out request,
                out operation,
                out setupError))
            {
                timer.Stop();

                var failure = new ChatResult
                {
                    Kind = ResultKind.ConnectionError,
                    Error = setupError,
                    ElapsedMilliseconds = timer.ElapsedMilliseconds
                };

                LogResult(failure, false);
                onCompleted?.Invoke(failure);
                yield break;
            }

            handle.Attach(request);

            ChatResult result;
            long timeoutMilliseconds =
                Math.Max(1, timeoutSeconds) * 1000L;

            try
            {
                while (!operation.isDone)
                {
                    if (handle.IsCancelled)
                        yield break;

                    if (timer.ElapsedMilliseconds >= timeoutMilliseconds)
                        break;

                    yield return null;
                }

                if (handle.IsCancelled)
                    yield break;

                if (timer.ElapsedMilliseconds >= timeoutMilliseconds)
                {
                    request.Abort();

                    result = new ChatResult
                    {
                        Kind = ResultKind.Timeout,
                        Error = "İstek süre sınırını aştı; Abort çağrıldı.",
                        HttpStatus = request.responseCode
                    };
                }
                else
                {
                    result = ReadResponse(request);
                }

                timer.Stop();
                result.ElapsedMilliseconds = timer.ElapsedMilliseconds;
            }
            finally
            {
                timer.Stop();
                handle.Release();
            }

            if (handle.IsCancelled)
                yield break;

            LogResult(result, false);
            onCompleted?.Invoke(result);
        }

        private bool TryStartRequest(
            ChatMessage[] messages,
            bool evidenceShown,
            bool canOpenExit,
            out UnityWebRequest request,
            out UnityWebRequestAsyncOperation operation,
            out string error)
        {
            request = null;
            operation = null;
            error = null;

            try
            {
                if (!isActiveAndEnabled)
                    throw new InvalidOperationException(
                        "OllamaClient etkin değil.");

                if (string.IsNullOrWhiteSpace(serverUrl) ||
                    string.IsNullOrWhiteSpace(modelName))
                {
                    throw new InvalidOperationException(
                        "Sunucu adresi veya model adı boş.");
                }

                if (messages == null || messages.Length == 0)
                    throw new InvalidOperationException(
                        "Gönderilecek mesaj bulunamadı.");

                string endpoint =
                    serverUrl.Trim().TrimEnd('/') + "/api/chat";

                if (!Uri.TryCreate(
                    endpoint, UriKind.Absolute, out Uri uri) ||
                    (uri.Scheme != "http" && uri.Scheme != "https"))
                {
                    throw new InvalidOperationException(
                        "Sunucu adresi geçerli bir HTTP/HTTPS adresi değil.");
                }

                string[] allowedActions = !evidenceShown
                    ? new[] { "none", "request_evidence" }
                    : canOpenExit
                        ? new[] { "none", "open_exit" }
                        : new[] { "none" };

                var payload = new ChatRequest
                {
                    model = modelName.Trim(),
                    messages = messages,
                    stream = false
                };

                payload.format.properties.action.@enum = allowedActions;

                JObject requestBody =
                    JObject.Parse(JsonUtility.ToJson(payload));

                requestBody["options"] = new JObject
                {
                    ["temperature"] = 0
                };

                string json = requestBody.ToString(Formatting.None);

                Debug.Log(
                    "Ollama isteği: model=" + modelName.Trim() +
                    ", timeout=" + timeoutSeconds +
                    ", izinli eylemler=" +
                    string.Join(", ", allowedActions),
                    this
                );

                request = new UnityWebRequest(
                    endpoint, UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler =
                    new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type", "application/json; charset=utf-8");

                request.timeout = 0;

                operation = request.SendWebRequest();
                return true;
            }
            catch (Exception exception)
            {
                error = exception.ToString();

                if (request != null)
                {
                    request.Dispose();
                    request = null;
                }

                return false;
            }
        }

        private static ChatResult ReadResponse(UnityWebRequest request)
        {
            string body = request.downloadHandler != null
                ? request.downloadHandler.text
                : "";

            long status = request.responseCode;

            if (request.result == UnityWebRequest.Result.ProtocolError ||
                status >= 400)
            {
                string serverError = ReadServerError(body);

                bool missingModel =
                    status == 404 &&
                    serverError.IndexOf(
                        "model", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    serverError.IndexOf(
                        "not found", StringComparison.OrdinalIgnoreCase) >= 0;

                return new ChatResult
                {
                    Kind = ResultKind.HttpError,
                    HttpStatus = status,
                    ModelNotFound = missingModel,
                    Error = "HTTP " + status + ": " +
                            request.error + "\n" + serverError
                };
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                return new ChatResult
                {
                    Kind = ResultKind.ConnectionError,
                    HttpStatus = status,
                    Error = request.error
                };
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                return new ChatResult
                {
                    Kind = ResultKind.InvalidEnvelope,
                    HttpStatus = status,
                    Error = request.error
                };
            }

            ChatResult result = ParseEnvelope(body);
            result.HttpStatus = status;
            return result;
        }

        private static string ReadServerError(string body)
        {
            try
            {
                JObject obj = JObject.Parse(body);
                return obj["error"]?.ToString() ?? body;
            }
            catch (JsonException)
            {
                return body ?? "";
            }
        }

        private static ChatResult ParseEnvelope(string body)
        {
            JObject envelope;

            try
            {
                envelope = JObject.Parse(body ?? "");
            }
            catch (JsonException exception)
            {
                return new ChatResult
                {
                    Kind = ResultKind.InvalidEnvelope,
                    Error = "Yanıt zarfı okunamadı: " + exception.Message
                };
            }

            if (envelope["error"] != null ||
                envelope["done"]?.Type != JTokenType.Boolean ||
                !(bool)envelope["done"] ||
                !(envelope["message"] is JObject message) ||
                message["content"]?.Type != JTokenType.String)
            {
                return new ChatResult
                {
                    Kind = ResultKind.InvalidEnvelope,
                    Error = "Yanıt zarfında error, done veya " +
                            "message.content alanı geçersiz.\n" + body
                };
            }

            string content = (string)message["content"];

            if (!NpcResponseParser.TryParse(
                content, out NpcResponse parsed, out string error))
            {
                return new ChatResult
                {
                    Kind = ResultKind.InvalidNpcResponse,
                    Content = content,
                    Error = error
                };
            }

            return new ChatResult
            {
                Kind = ResultKind.Success,
                Content = content
            };
        }

        private void LogResult(ChatResult result, bool simulated)
        {
            string log =
                (simulated ? "TEST SONUCU" : "Ollama sonucu") +
                ": " + result.Kind +
                " | HTTP=" + result.HttpStatus +
                " | süre=" + result.ElapsedMilliseconds + " ms";

            if (result.Success)
                Debug.Log(log, this);
            else
                Debug.LogWarning(log + "\n" + result.Error, this);
        }

#if UNITY_EDITOR
        private static string BuildTestEnvelope(TestResponse test)
        {
            if (test == TestResponse.InvalidEnvelope)
                return "{\"done\":true,\"message\":{}}";

            string content;

            switch (test)
            {
                case TestResponse.BrokenNpcJson:
                    content = "{\"dialogue\":\"Dinliyorum.\"";
                    break;

                case TestResponse.WrongBooleanType:
                    content =
                        "{\"dialogue\":\"Dinliyorum.\"," +
                        "\"emotion\":\"suspicious\"," +
                        "\"action\":\"none\"," +
                        "\"persuaded\":\"evet\"}";
                    break;

                default:
                    content =
                        "{\"dialogue\":\"Dinliyorum.\"," +
                        "\"emotion\":\"suspicious\"," +
                        "\"action\":\"teleport_player\"," +
                        "\"persuaded\":false}";
                    break;
            }

            return new JObject
            {
                ["done"] = true,
                ["message"] = new JObject
                {
                    ["role"] = "assistant",
                    ["content"] = content
                }
            }.ToString(Formatting.None);
        }
#endif
    }
}