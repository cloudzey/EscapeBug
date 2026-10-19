
using TMPro;
using UnityEngine;

public class SymbolLockView : MonoBehaviour
{
    [Header("Mevcut kilit sistemi")]
    [SerializeField] private SymbolLockController controller;

    [Header("Uc sembol alani")]
    [SerializeField] private TMP_Dropdown firstSlot;
    [SerializeField] private TMP_Dropdown secondSlot;
    [SerializeField] private TMP_Dropdown thirdSlot;

    private readonly int[] indices = { -1, -1, -1 };

    public void NextFirst()
    {
        NextSymbol(0, firstSlot);
    }

    public void NextSecond()
    {
        NextSymbol(1, secondSlot);
    }

    public void NextThird()
    {
        NextSymbol(2, thirdSlot);
    }

    private void NextSymbol(int slotIndex, TMP_Dropdown dropdown)
    {
        if (!gameObject.activeInHierarchy ||
            controller == null ||
            dropdown == null ||
            controller.IsSolved)
        {
            return;
        }

        // 0 = Seciniz, 1-3 = semboller.
        // Mevcut dropdown degerini kaynak kabul ediyoruz.
        indices[slotIndex] = dropdown.value - 1;

        // -1 -> 0 -> 1 -> 2 -> 0
        indices[slotIndex] = (indices[slotIndex] + 1) % 3;

        // Dropdown'da Seciniz secenegi bulundugu icin +1.
        dropdown.SetValueWithoutNotify(indices[slotIndex] + 1);
        dropdown.RefreshShownValue();

        // Mevcut kilit controller'ina secimi bildir.
        controller.SetSymbol(slotIndex, indices[slotIndex]);
    }
}
