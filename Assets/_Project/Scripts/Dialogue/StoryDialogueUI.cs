using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[Serializable]
public class StoryDialogueLine
{
    public string speaker;

    [TextArea(2, 5)]
    public string text;
}

public class StoryDialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;

    [Header("Gameplay Lock")]
    [Tooltip("Scripts to disable during dialogue.")]
    [SerializeField]
    private MonoBehaviour[] scriptsToPause;

    [SerializeField]
    private Animator kafkaAnimator;

    private StoryDialogueLine[] activeLines;
    private int currentIndex;

    private bool[] previousScriptStates;
    private bool gameplayPaused;

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    private static readonly int IsMoving =
        Animator.StringToHash("IsMoving");

    public bool IsPlaying => activeLines != null;

    public event Action DialogueFinished;

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(Next);

        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (!IsPlaying || Keyboard.current == null)
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            Next();
        }
    }

    public bool Play(StoryDialogueLine[] lines)
    {
        if (IsPlaying || lines == null ||
            lines.Length == 0 ||
            panel == null ||
            speakerText == null ||
            dialogueText == null)
        {
            return false;
        }

        activeLines = lines;
        currentIndex = 0;

        PauseGameplay();

        panel.SetActive(true);
        ShowCurrentLine();

        return true;
    }

    private void PauseGameplay()
    {
        if (gameplayPaused)
            return;

        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        previousScriptStates =
            new bool[scriptsToPause.Length];

        for (int i = 0; i < scriptsToPause.Length; i++)
        {
            MonoBehaviour script = scriptsToPause[i];

            if (script == null || script == this)
                continue;

            previousScriptStates[i] = script.enabled;
            script.enabled = false;
        }

        gameplayPaused = true;

        if (kafkaAnimator != null)
            kafkaAnimator.SetBool(IsMoving, false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            "[StoryDialogue] Controls paused. Cursor unlocked."
        );
    }

    private void ResumeGameplay()
    {
        if (!gameplayPaused)
            return;

        for (int i = 0; i < scriptsToPause.Length; i++)
        {
            MonoBehaviour script = scriptsToPause[i];

            if (script == null || script == this)
                continue;

            if (previousScriptStates != null &&
                i < previousScriptStates.Length)
            {
                script.enabled = previousScriptStates[i];
            }
        }

        gameplayPaused = false;
        previousScriptStates = null;

        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;

        Debug.Log(
            "[StoryDialogue] Controls restored."
        );
    }

    public void Next()
    {
        if (!IsPlaying)
            return;

        currentIndex++;

        if (currentIndex >= activeLines.Length)
        {
            Close();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        speakerText.text =
            activeLines[currentIndex].speaker;

        dialogueText.text =
            activeLines[currentIndex].text;
    }

    private void Close()
    {
        activeLines = null;
        currentIndex = 0;

        if (panel != null)
            panel.SetActive(false);

        ResumeGameplay();
        DialogueFinished?.Invoke();
    }

    private void OnDisable()
    {
        activeLines = null;

        if (panel != null)
            panel.SetActive(false);

        ResumeGameplay();
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(Next);

        ResumeGameplay();
    }
}