using System.Collections.Generic;
using UnityEngine;

public class ArchiveMonologueController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private LetterReader letterReader;
    [SerializeField] private StoryDialogueUI dialogueUI;

    [Header("Letter Data")]
    [SerializeField] private LetterData invitationLetter;
    [SerializeField] private LetterData greteLetter;
    [SerializeField] private LetterData feliceLetter;

    [Header("Dialogue Sequences")]
    [SerializeField] private StoryDialogueSequence invitationMonologue;
    [SerializeField] private StoryDialogueSequence greteMonologue;
    [SerializeField] private StoryDialogueSequence feliceMonologue;
    [SerializeField] private StoryDialogueSequence allLettersMonologue;

    private readonly HashSet<LetterData> readLetters =
        new HashSet<LetterData>();

    private readonly Queue<StoryDialogueSequence> pending =
        new Queue<StoryDialogueSequence>();

    private bool allLettersQueued;

    private void OnEnable()
    {
        if (letterReader != null)
            letterReader.LetterClosed += OnLetterClosed;

        if (dialogueUI != null)
            dialogueUI.DialogueFinished += OnDialogueFinished;
    }

    private void OnDisable()
    {
        if (letterReader != null)
            letterReader.LetterClosed -= OnLetterClosed;

        if (dialogueUI != null)
            dialogueUI.DialogueFinished -= OnDialogueFinished;

        pending.Clear();
    }

    private void OnLetterClosed(LetterData letter)
    {
        StoryDialogueSequence sequence = null;

        if (letter == invitationLetter && invitationLetter != null)
            sequence = invitationMonologue;
        else if (letter == greteLetter && greteLetter != null)
            sequence = greteMonologue;
        else if (letter == feliceLetter && feliceLetter != null)
            sequence = feliceMonologue;
        else
            return;

        if (!readLetters.Add(letter))
            return;

        Debug.Log(
            "[Archive] First read: " + letter.id,
            this
        );

        if (sequence != null)
            pending.Enqueue(sequence);

        if (!allLettersQueued &&
            invitationLetter != null &&
            greteLetter != null &&
            feliceLetter != null &&
            readLetters.Contains(invitationLetter) &&
            readLetters.Contains(greteLetter) &&
            readLetters.Contains(feliceLetter))
        {
            allLettersQueued = true;

            if (allLettersMonologue != null)
                pending.Enqueue(allLettersMonologue);
        }

        PlayNext();
    }

    private void OnDialogueFinished()
    {
        PlayNext();
    }

    private void PlayNext()
    {
        if (dialogueUI == null || dialogueUI.IsPlaying)
            return;

        while (pending.Count > 0)
        {
            StoryDialogueSequence next = pending.Peek();

            if (next == null)
            {
                pending.Dequeue();
                continue;
            }

            if (next.TryPlay())
            {
                pending.Dequeue();
                return;
            }

            Debug.LogWarning(
                "[Archive] Monologue could not start: " +
                next.name +
                ". Check StoryDialogueSequence references.",
                this
            );

            return;
        }
    }
}