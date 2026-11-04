
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LetterReader : MonoBehaviour
{
    [Serializable]
    private class LetterSymbolEntry
    {
        public LetterData letter;
        public Sprite symbol;
    }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect bodyScroll;

    [Header("Mektup Sembolleri")]
    [SerializeField] private Image symbolImage;
    [SerializeField] private LetterSymbolEntry[] letterSymbols;

    private LetterData currentLetter;

    public event Action<LetterData> LetterClosed;

    public bool IsOpen { get; private set; }

    public bool IsReady =>
        isActiveAndEnabled &&
        panelRoot != null &&
        titleText != null &&
        dateText != null &&
        bodyText != null &&
        closeButton != null;

    private void Awake()
    {
        if (titleText != null)
            titleText.richText = false;

        if (dateText != null)
            dateText.richText = false;

        if (bodyText != null)
            bodyText.richText = false;

        if (symbolImage != null)
        {
            symbolImage.raycastTarget = false;
            symbolImage.gameObject.SetActive(false);
        }

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void Update()
    {
        if (IsOpen &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public bool Open(LetterData letter)
    {
        if (IsOpen)
            return false;

        if (!IsReady || letter == null)
        {
            Debug.LogWarning(
                "LetterReader: Panel baglantilari veya mektup eksik.",
                this
            );

            return false;
        }

        titleText.text = letter.title;
        dateText.text = letter.displayedDate;
        bodyText.text = letter.body;

        dateText.gameObject.SetActive(
            !string.IsNullOrWhiteSpace(letter.displayedDate)
        );

        RefreshSymbol(letter);

        PlayerControlLocks.Acquire(this);
        currentLetter = letter;
        IsOpen = true;

        panelRoot.SetActive(true);

        Canvas.ForceUpdateCanvases();

        if (bodyScroll != null)
            bodyScroll.verticalNormalizedPosition = 1f;

        return true;
    }

    private void RefreshSymbol(LetterData letter)
    {
        if (symbolImage == null)
            return;

        Sprite selectedSymbol = null;

        if (letterSymbols != null)
        {
            foreach (LetterSymbolEntry entry in letterSymbols)
            {
                if (entry != null && entry.letter == letter)
                {
                    selectedSymbol = entry.symbol;
                    break;
                }
            }
        }

        symbolImage.sprite = selectedSymbol;
        symbolImage.preserveAspect = true;
        symbolImage.gameObject.SetActive(selectedSymbol != null);
    }

    public void Close()
    {
        CloseInternal(true);
    }

    private void CloseInternal(bool notifyDialogue)
    {
        bool wasOpen = IsOpen;
        LetterData closedLetter = currentLetter;

        IsOpen = false;
        currentLetter = null;

        PlayerControlLocks.Release(this);

        if (symbolImage != null)
            symbolImage.gameObject.SetActive(false);

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (notifyDialogue && wasOpen && closedLetter != null)
        {
            Debug.Log(
                "[Archive] Letter closed: " + closedLetter.id,
                this
            );

            LetterClosed?.Invoke(closedLetter);
        }
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        CloseInternal(false);
    }

}
