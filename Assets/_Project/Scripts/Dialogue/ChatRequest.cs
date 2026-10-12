using System;

namespace EscapeBug.Dialogue
{
    [Serializable]
    public class ChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public bool stream = false;
    }
}