using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LetterArchiveUI : MonoBehaviour
{
    [Serializable]
    private class LetterEntry
    {
        public LetterData letter;
        public Button button;
        public TMP_Text label;

        [NonSerialized]
        public UnityAction clickHandler;
    }

    [Header("Baglantilar")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private LetterReader reader;

    [Header("Mektuplar")]
    [SerializeField] private LetterEntry[] entries;

    private bool previousLockState;

    private void OnEnable()
    {
        if (gameProgress != null)
            gameProgress.LettersChanged += RefreshButtons;

        if (entries != null)
        {
            foreach (LetterEntry entry in entries)
            {
                if (entry == null || entry.button == null)
                    continue;

                LetterData selectedLetter = entry.letter;

                entry.clickHandler = () => OpenCollectedLetter(selectedLetter);
                entry.button.onClick.AddListener(entry.clickHandler);
            }
        }

        previousLockState = PlayerControlLocks.IsLocked;
        RefreshButtons();
    }

    private void Start()
    {
        RefreshButtons();
    }

    private void Update()
    {
        bool locked = PlayerControlLocks.IsLocked;

        if (locked == previousLockState)
            return;

        previousLockState = locked;
        RefreshButtons();
    }

    private void OnDisable()
    {
        if (gameProgress != null)
            gameProgress.LettersChanged -= RefreshButtons;

        if (entries == null)
            return;

        foreach (LetterEntry entry in entries)
        {
            if (entry == null ||
                entry.button == null ||
                entry.clickHandler == null)
            {
                continue;
            }

            entry.button.onClick.RemoveListener(entry.clickHandler);
            entry.clickHandler = null;
        }
    }

    private void OpenCollectedLetter(LetterData letter)
    {
        if (PlayerControlLocks.IsLocked)
            return;

        if (gameProgress == null ||
            reader == null ||
            letter == null ||
            !gameProgress.HasLetter(letter.id))
        {
            return;
        }

        reader.Open(letter);
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        if (entries == null)
            return;

        foreach (LetterEntry entry in entries)
        {
            if (entry == null)
                continue;

            bool collected =
                gameProgress != null &&
                entry.letter != null &&
                gameProgress.HasLetter(entry.letter.id);

            if (entry.button != null)
            {
                entry.button.interactable =
                    collected &&
                    reader != null &&
                    reader.IsReady &&
                    !PlayerControlLocks.IsLocked;
            }

            if (entry.label != null)
            {
                entry.label.text = collected
                    ? entry.letter.title
                    : "Bulunmamis mektup";
            }
        }
    }
}