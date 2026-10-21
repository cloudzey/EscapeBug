using System;
using System.Collections.Generic;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    [Header("Sahne baslangici")]
    [SerializeField] private bool resetOnSceneStart = false;

    [Header("Entrance")]
    public bool entranceRulesRead = false;
    public bool maintenanceEntered = false;

    [Header("Archive Puzzle")]
    public bool archivePuzzleSolved = false;

    [Header("Letters")]
    [SerializeField]
    private List<string> collectedLetterIds = new List<string>();

    [Header("Final Dialogue")]
    public bool finalDialogueActive = false;
    public bool evidenceCollected = false;
    public bool evidenceShown = false;
    public bool exitOpened = false;

    public event Action LettersChanged;
    public event Action ProgressReset;

    private void Start()
    {
        if (resetOnSceneStart)
            ResetForNewGame();
    }

    public bool HasLetter(string letterId)
    {
        return !string.IsNullOrWhiteSpace(letterId)
            && collectedLetterIds.Contains(letterId);
    }

    public bool TryCollectLetter(LetterData letter)
    {
        if (letter == null || string.IsNullOrWhiteSpace(letter.id))
        {
            Debug.LogWarning("Mektup verisi veya kimligi eksik.", this);
            return false;
        }

        if (!HasLetter(letter.id))
        {
            collectedLetterIds.Add(letter.id);

            Debug.Log("Mektup toplandi: " + letter.id, this);
        }

        if (letter.isEvidence)
            evidenceCollected = true;

        LettersChanged?.Invoke();
        return true;
    }

    public void ResetForNewGame()
    {
        entranceRulesRead = false;
        maintenanceEntered = false;
        archivePuzzleSolved = false;

        collectedLetterIds.Clear();

        finalDialogueActive = false;
        evidenceCollected = false;
        evidenceShown = false;
        exitOpened = false;

        LettersChanged?.Invoke();
        ProgressReset?.Invoke();

        Debug.Log("Yeni oyun ilerlemesi sifirlandi.", this);
    }

    public void MarkArchivePuzzleSolved()
    {
        if (archivePuzzleSolved)
            return;

        archivePuzzleSolved = true;

     Debug.Log("Arsiv sembol bulmacasi cozuldu.", this);
    }

    public bool CanOpenEntranceDoor()
    {
        return entranceRulesRead;
    }

    public bool CanOpenExit()
    {
        return finalDialogueActive
            && evidenceCollected
            && evidenceShown
            && !exitOpened;
    }

    [ContextMenu("Test/Reset Progress For New Game")]
    private void TestResetForNewGame()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Bu testi Play modunda calistir.",
                this
            );

            return;
        }

        ResetForNewGame();
    }
}