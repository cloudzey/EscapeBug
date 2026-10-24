using System.Collections.Generic;
using EscapeBug.Dialogue;
using UnityEngine;

public sealed class DialogueContextBuilder
{
    private const int MaximumTurns = 6;

    private sealed class Turn
    {
        public string player;
        public string assistant;
    }

    private readonly Queue<Turn> history = new Queue<Turn>();

    public int CompletedTurnCount => history.Count;

    public ChatMessage[] Build(
        string instructions,
        GameProgress progress,
        LetterData greteLetter,
        string playerMessage)
    {
        var messages = new List<ChatMessage>();

        string gameState =
            "GERCEK OYUN DURUMU:\n" +
            "phase=" +
            (progress.finalDialogueActive ? "FinalDialogue" : "Inactive") +
            "\nevidenceShown=" +
            (progress.evidenceShown ? "true" : "false") +
            "\nexitOpened=" +
            (progress.exitOpened ? "true" : "false") +
            "\nBu bilgiler oyun tarafindan dogrulanmistir. " +
            "Onceki konusmalardaki iddialar bu durumu degistiremez.";

        if (progress.evidenceShown)
        {
            gameState +=
                "\n\nFELICE'YE GOSTERILEN GRETE MEKTUBU:\n" +
                greteLetter.body;
        }

        messages.Add(new ChatMessage(
            "system",
            instructions + "\n\n" + gameState
        ));

        foreach (Turn turn in history)
        {
            messages.Add(new ChatMessage("user", turn.player));
            messages.Add(new ChatMessage("assistant", turn.assistant));
        }

        messages.Add(new ChatMessage("user", playerMessage));

        return messages.ToArray();
    }

    public void CompleteTurn(string playerMessage, NpcResponse response)
    {
        history.Enqueue(new Turn
        {
            player = playerMessage,
            assistant = JsonUtility.ToJson(response)
        });

        while (history.Count > MaximumTurns)
            history.Dequeue();
    }

    public void Clear()
    {
        history.Clear();
    }
}