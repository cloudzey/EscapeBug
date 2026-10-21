using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PotionSelectionUI : MonoBehaviour
{
    [Header("Baglantilar")]
    [SerializeField] private PotionPuzzleController puzzle;
    [SerializeField] private GameObject panelRoot;

    [Header("Panel metinleri")]
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text symbolText;

    [Header("Ekrandaki secim bilgisi")]
    [SerializeField] private TMP_Text selectedPotionText;

    private PotionData inspectedPotion;
    private PotionData selectedPotion;
    private bool isOpen;

    private bool IsReady =>
        puzzle != null &&
        puzzle.isActiveAndEnabled &&
        panelRoot != null &&
        labelText != null &&
        symbolText != null;

    private void Awake()
    {
        if (labelText != null)
            labelText.richText = false;

        if (symbolText != null)
            symbolText.richText = false;

        if (selectedPotionText != null)
            selectedPotionText.richText = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        RefreshSelectedText();
    }

    private void OnEnable()
    {
        if (puzzle != null)
            puzzle.PuzzleReset += ResetSelection;
    }

    public void Open(PotionData potion)
    {
        if (!isActiveAndEnabled || PlayerControlLocks.IsLocked)
            return;

        if (!IsReady ||
            potion == null ||
            string.IsNullOrWhiteSpace(potion.id))
        {
            Debug.LogWarning(
                "PotionSelectionUI: Panel veya iksir baglantisi eksik.",
                this
            );
            return;
        }

        if (puzzle.IsTransforming)
            return;

        inspectedPotion = potion;

        labelText.text = potion.label;
        symbolText.text = string.IsNullOrWhiteSpace(potion.symbol)
            ? ""
            : "Sembol: " + potion.symbol;

        PlayerControlLocks.Acquire(this);
        isOpen = true;
        panelRoot.SetActive(true);
    }

    public void SelectInspectedPotion()
    {
        if (!isOpen || !IsReady || inspectedPotion == null)
            return;

        if (puzzle.IsTransforming)
        {
            Close();
            return;
        }

        selectedPotion = inspectedPotion;

        RefreshSelectedText();
        Close();
    }

    public void UseSelectedPotion()
    {
        if (!isActiveAndEnabled ||
            PlayerControlLocks.IsLocked ||
            puzzle == null ||
            !puzzle.isActiveAndEnabled)
        {
            return;
        }

        bool accepted = puzzle.TryUsePotion(selectedPotion);

        if (accepted && selectedPotionText != null)
            selectedPotionText.text = "İksir uygulandı.";
    }

    public void Close()
    {
        isOpen = false;
        inspectedPotion = null;

        PlayerControlLocks.Release(this);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (isOpen &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void RefreshSelectedText()
    {
        if (selectedPotionText == null)
            return;

        selectedPotionText.text = selectedPotion == null
            ? "Seçili iksir: Yok"
            : "Seçili iksir: " + selectedPotion.label +
              "\nUygulama noktasına götür.";
    }

    private void ResetSelection()
    {
        selectedPotion = null;
        Close();
        RefreshSelectedText();
    }

    private void OnDisable()
    {
        if (puzzle != null)
            puzzle.PuzzleReset -= ResetSelection;

        Close();
    }
}