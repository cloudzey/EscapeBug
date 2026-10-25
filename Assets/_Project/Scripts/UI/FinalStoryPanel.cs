using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalStoryPanel : MonoBehaviour
{
    [Serializable]
    public class StoryLine
    {
        public string speaker;

        [TextArea(2, 6)]
        public string text;
    }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button nextButton;

    [Header("Yuzlesme metinleri")]
    [SerializeField] private StoryLine[] confrontationLines;

    [Header("D5 sonrasi son soz")]
    [SerializeField] private StoryLine[] endingLines;

    public bool IsPlaying { get; private set; }
    public bool Completed { get; private set; }

    private bool nextRequested;
    private int lineShownFrame;

    private void Awake()
    {
        if (speakerText != null)
            speakerText.richText = false;

        if (bodyText != null)
            bodyText.richText = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(RequestNext);
    }

    private void RequestNext()
    {
        if (!IsPlaying || Time.frameCount <= lineShownFrame)
            return;

        nextRequested = true;
    }

    public IEnumerator PlayConfrontation()
    {
        return PlayLines(confrontationLines);
    }

    public IEnumerator PlayEnding()
    {
        return PlayLines(endingLines);
    }

    private IEnumerator PlayLines(StoryLine[] lines)
    {
        if (IsPlaying)
            yield break;

        Completed = false;

        if (!isActiveAndEnabled ||
            panelRoot == null ||
            speakerText == null ||
            bodyText == null ||
            nextButton == null ||
            lines == null ||
            lines.Length == 0)
        {
            Debug.LogError(
                "FinalStoryPanel: Panel baglantilari veya metinler eksik.",
                this
            );
            yield break;
        }

        foreach (StoryLine line in lines)
        {
            if (line == null ||
                string.IsNullOrWhiteSpace(line.speaker) ||
                string.IsNullOrWhiteSpace(line.text))
            {
                Debug.LogError(
                    "FinalStoryPanel: Bos konusmaci veya metin var.",
                    this
                );
                yield break;
            }
        }

        IsPlaying = true;
        panelRoot.SetActive(true);
        nextButton.interactable = true;

        foreach (StoryLine line in lines)
        {
            speakerText.text = line.speaker;
            bodyText.text = line.text;

            nextRequested = false;
            lineShownFrame = Time.frameCount;

            while (!nextRequested)
            {
                if (!isActiveAndEnabled ||
                    !panelRoot.activeInHierarchy)
                {
                    CancelPlayback();
                    yield break;
                }

                yield return null;
            }

            yield return null;
        }

        IsPlaying = false;
        Completed = true;
        panelRoot.SetActive(false);
    }

    public void CancelPlayback()
    {
        IsPlaying = false;
        Completed = false;
        nextRequested = true;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnDisable()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(RequestNext);

        CancelPlayback();
    }
}