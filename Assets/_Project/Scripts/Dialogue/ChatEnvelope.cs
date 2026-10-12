using System;

namespace EscapeBug.Dialogue
{
    [Serializable]
    public class ChatEnvelope
    {
        public string model;
        public ChatMessage message;
        public bool done;
        public string done_reason;
        public string error;
    }
}