using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EscapeBug.Dialogue
{
    public static class NpcResponseParser
    {
        public static bool TryParse(
            string json,
            out NpcResponse response,
            out string error)
        {
            response = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Model boş yanıt gönderdi.";
                return false;
            }

            try
            {
                JObject obj;

                using (var textReader = new StringReader(json))
                using (var reader = new JsonTextReader(textReader))
                {
                    reader.DateParseHandling = DateParseHandling.None;
                    reader.MaxDepth = 8;

                    var settings = new JsonLoadSettings
                    {
                        DuplicatePropertyNameHandling =
                            DuplicatePropertyNameHandling.Error
                    };

                    obj = JObject.Load(reader, settings);

                    if (reader.Read())
                    {
                        error = "JSON sonrasında fazladan içerik var.";
                        return false;
                    }
                }

                string[] required =
                {
                    "dialogue", "action", "emotion", "persuaded"
                };

                foreach (string field in required)
                {
                    if (obj[field] == null)
                    {
                        error = "Eksik alan: " + field;
                        return false;
                    }
                }

                foreach (JProperty property in obj.Properties())
                {
                    if (Array.IndexOf(required, property.Name) < 0)
                    {
                        error = "Beklenmeyen alan: " + property.Name;
                        return false;
                    }
                }

                if (obj["dialogue"].Type != JTokenType.String ||
                    obj["action"].Type != JTokenType.String ||
                    obj["emotion"].Type != JTokenType.String)
                {
                    error = "Konuşma, eylem ve duygu metin olmalı.";
                    return false;
                }

                if (obj["persuaded"].Type != JTokenType.Boolean)
                {
                    error = "persuaded değeri true veya false olmalı.";
                    return false;
                }

                var candidate = new NpcResponse
                {
                    dialogue = (string)obj["dialogue"],
                    action = (string)obj["action"],
                    emotion = (string)obj["emotion"],
                    persuaded = (bool)obj["persuaded"]
                };

                if (!NpcResponseValidator.TryValidate(candidate, out error))
                    return false;

                response = candidate;
                return true;
            }
            catch (JsonException)
            {
                error = "Model yanıtının JSON yapısı okunamadı.";
                return false;
            }
        }
    }
}