using System;

namespace EscapeBug.Dialogue
{
    [Serializable]
    public class NpcResponseSchema
    {
        public string type = "object";
        public NpcResponseProperties properties = new NpcResponseProperties();

        public string[] required =
        {
            "dialogue", "action", "emotion", "persuaded"
        };

        public bool additionalProperties = false;
    }

    [Serializable]
    public class NpcResponseProperties
    {
        public DialogueTextSchema dialogue = new DialogueTextSchema();

        public ChoiceSchema action = new ChoiceSchema
        {
            @enum = new[] { "none", "request_evidence", "open_exit" }
        };

        public ChoiceSchema emotion = new ChoiceSchema
        {
            @enum = new[] { "angry", "suspicious", "calm" }
        };

        public BooleanSchema persuaded = new BooleanSchema();
    }

    [Serializable]
    public class DialogueTextSchema
    {
        public string type = "string";
        public int minLength = 1;
        public int maxLength = 600;
    }

    [Serializable]
    public class ChoiceSchema
    {
        public string type = "string";
        public string[] @enum;
    }

    [Serializable]
    public class BooleanSchema
    {
        public string type = "boolean";
    }
}