using UnityEngine;

public class LetterInteractable : MonoBehaviour, IInteractable
{
    [Header("Baglantilar")]
    [SerializeField] private LetterData letter;
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private LetterReader reader;

    [Header("Dunya gorunumu")]
    [SerializeField] private GameObject worldVisual;
    [SerializeField] private Collider interactionCollider;
    [SerializeField] private bool hideAfterCollection = false;

    public string DisplayName =>
        letter == null
            ? "Mektup"
            : letter.title + " - Oku";

    private void OnEnable()
    {
        if (gameProgress != null)
            gameProgress.LettersChanged += RefreshWorldState;
    }

    private void Start()
    {
        RefreshWorldState();
    }

    private void OnDisable()
    {
        if (gameProgress != null)
            gameProgress.LettersChanged -= RefreshWorldState;
    }

    public void Interact()
    {
        if (!isActiveAndEnabled || PlayerControlLocks.IsLocked)
            return;

        if (letter == null ||
            string.IsNullOrWhiteSpace(letter.id) ||
            gameProgress == null ||
            reader == null ||
            !reader.IsReady)
        {
            Debug.LogWarning(
                "LetterInteractable: Mektup, ilerleme veya panel baglantisi eksik.",
                this
            );

            return;
        }

        if (!reader.Open(letter))
            return;

        if (!gameProgress.TryCollectLetter(letter))
            reader.Close();
    }

    private void RefreshWorldState()
    {
        bool collected =
            gameProgress != null &&
            letter != null &&
            gameProgress.HasLetter(letter.id);

        bool visible = !hideAfterCollection || !collected;

        if (worldVisual != null && worldVisual != gameObject)
            worldVisual.SetActive(visible);

        if (interactionCollider != null)
            interactionCollider.enabled = visible;
    }
}