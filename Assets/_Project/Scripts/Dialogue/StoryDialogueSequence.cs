using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StoryDialogueSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryDialogueUI dialogueUI;
    [SerializeField] private Transform playerRoot;

    [Header("Dialogue")]
    [SerializeField] private StoryDialogueLine[] lines;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private bool autoTriggerOnEnter = true;

    private bool hasPlayed;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoTriggerOnEnter || playerRoot == null)
            return;

        if (other.transform == playerRoot ||
            other.transform.IsChildOf(playerRoot))
        {
            Play();
        }
    }

    public void Play()
    {
        TryPlay();
    }

    public bool TryPlay()
    {
        if (playOnce && hasPlayed)
            return false;

        if (dialogueUI == null)
        {
            Debug.LogWarning(
                "StoryDialogueUI bağlantısı eksik.",
                this
            );
            return false;
        }

        if (dialogueUI.Play(lines))
        {
            hasPlayed = true;

            Debug.Log(
                "Hikâye konuşması başladı: " + name,
                this
            );

            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    [ContextMenu("Test/Play Story Dialogue")]
    private void TestPlay()
    {
        if (Application.isPlaying)
            Play();
    }
#endif
}