using UnityEngine;

public class PotionFirstLever : MonoBehaviour, IInteractable
{
    [SerializeField] private PotionPuzzleController puzzle;
    [SerializeField] private Transform handleVisual;

    [SerializeField]
    private Vector3 latchedLocalEulerOffset = new Vector3(0f, 0f, -55f);

    private Quaternion initialLocalRotation;

    public string DisplayName =>
        puzzle != null && puzzle.FirstLeverLatched
            ? "İlk kol (sabitlendi)"
            : "İlk kolu indir";

    private void Awake()
    {
        if (handleVisual != null)
            initialLocalRotation = handleVisual.localRotation;
    }

    private void OnEnable()
    {
        if (puzzle != null)
            puzzle.PuzzleReset += ResetVisual;
    }

    public void Interact()
    {
        if (!isActiveAndEnabled || PlayerControlLocks.IsLocked)
            return;

        if (puzzle == null)
        {
            Debug.LogWarning("PotionFirstLever: Puzzle bağlantısı eksik.", this);
            return;
        }

        if (!puzzle.TryLatchFirstLever())
            return;

        if (handleVisual != null)
        {
            handleVisual.localRotation =
                initialLocalRotation *
                Quaternion.Euler(latchedLocalEulerOffset);
        }
    }

    private void ResetVisual()
    {
        if (handleVisual != null)
            handleVisual.localRotation = initialLocalRotation;
    }

    private void OnDisable()
    {
        if (puzzle != null)
            puzzle.PuzzleReset -= ResetVisual;
    }
}