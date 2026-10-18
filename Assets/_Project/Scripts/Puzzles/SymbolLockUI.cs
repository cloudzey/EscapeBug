using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SymbolLockUI : MonoBehaviour
{
    [Header("Baglantilar")]
    [SerializeField] private SymbolLockController controller;
    [SerializeField] private GameObject panelRoot;

    [Header("Secimler")]
    [SerializeField] private TMP_Dropdown firstSlot;
    [SerializeField] private TMP_Dropdown secondSlot;
    [SerializeField] private TMP_Dropdown thirdSlot;

    [Header("Geri bildirim")]
    [SerializeField] private TMP_Text feedbackText;

    private bool isOpen;

    private bool IsReady =>
        controller != null &&
        controller.isActiveAndEnabled &&
        panelRoot != null &&
        firstSlot != null &&
        secondSlot != null &&
        thirdSlot != null &&
        feedbackText != null;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (feedbackText != null)
            feedbackText.richText = false;

        if (firstSlot != null)
            firstSlot.SetValueWithoutNotify(0);

        if (secondSlot != null)
            secondSlot.SetValueWithoutNotify(0);

        if (thirdSlot != null)
            thirdSlot.SetValueWithoutNotify(0);
    }

    public void Open()
    {
        if (!isActiveAndEnabled || PlayerControlLocks.IsLocked)
            return;

        if (!IsReady)
        {
            Debug.LogWarning(
                "SymbolLockUI: Inspector baglantilari eksik.",
                this
            );
            return;
        }

        PlayerControlLocks.Acquire(this);

        isOpen = true;
        panelRoot.SetActive(true);

        feedbackText.text = controller.IsSolved
            ? "Bu kilit zaten çözüldü."
            : "Mektuplardaki tarih sırasına göre sembolleri seç.";
    }

    public void Submit()
    {
        if (!isOpen || !IsReady)
            return;

        controller.SetSymbol(0, firstSlot.value - 1);
        controller.SetSymbol(1, secondSlot.value - 1);
        controller.SetSymbol(2, thirdSlot.value - 1);

        controller.TryUnlock(out string message);
        feedbackText.text = message;
    }

    public void Close()
    {
        isOpen = false;

        PlayerControlLocks.Release(this);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (firstSlot.IsExpanded ||
                secondSlot.IsExpanded ||
                thirdSlot.IsExpanded)
            {
                return;
            }

            Close();
        }
    }

    private void OnDisable()
    {
        Close();
    }
}