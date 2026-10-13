using System;

namespace EscapeBug.Dialogue
{
    public static class NpcResponseValidator
    {
        public static bool TryValidate(
            NpcResponse response,
            out string error)
        {
            error = null;

            if (response == null)
            {
                error = "NPC yanıtı bulunamadı.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(response.dialogue))
            {
                error = "Konuşma metni boş olamaz.";
                return false;
            }

            if (response.dialogue.Length > 600)
            {
                error = "Konuşma metni 600 karakteri geçemez.";
                return false;
            }

            string[] allowedActions =
            {
                "none", "request_evidence", "open_exit"
            };

            if (Array.IndexOf(allowedActions, response.action) < 0)
            {
                error = "İzin verilmeyen eylem: " + response.action;
                return false;
            }

            string[] allowedEmotions =
            {
                "angry", "suspicious", "calm"
            };

            if (Array.IndexOf(allowedEmotions, response.emotion) < 0)
            {
                error = "İzin verilmeyen duygu: " + response.emotion;
                return false;
            }

            if (response.action == "open_exit" && !response.persuaded)
            {
                error = "İkna olmadan çıkışı açma isteği gönderilemez.";
                return false;
            }

            return true;
        }
    }
}