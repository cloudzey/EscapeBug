using System;

namespace EscapeBug.Dialogue
{
    [Serializable]
    public class NpcResponse
    {
        public string dialogue;
        public string action;
        public string emotion;
        public bool persuaded;
    }
}