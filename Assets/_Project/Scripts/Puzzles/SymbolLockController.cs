using UnityEngine;
using UnityEngine.Events;

public class SymbolLockController : MonoBehaviour
{
    public enum SymbolId
    {
        Key = 0,
        Eye = 1,
        Moth = 2
    }

    private readonly SymbolId[] expectedSequence =
    {
        SymbolId.Key,
        SymbolId.Eye,
        SymbolId.Moth
    };

    private readonly int[] selectedSymbols = { -1, -1, -1 };

    [Header("Cozum olayi")]
    [SerializeField] private UnityEvent onSolved = new UnityEvent();

    [Header("Runtime")]
    [SerializeField] private bool solved;

    public bool IsSolved => solved;

    private void Awake()
    {
        solved = false;

        for (int i = 0; i < selectedSymbols.Length; i++)
        selectedSymbols[i] = -1;
    }

    public void SetSymbol(int slot, int index)
    {
        if (solved)
            return;

        if (slot < 0 || slot >= selectedSymbols.Length)
        {
            Debug.LogWarning("Gecersiz sembol alani: " + slot, this);
            return;
        }

        if (index < -1 || index > (int)SymbolId.Moth)
        {
            Debug.LogWarning("Gecersiz sembol kimligi: " + index, this);
            return;
        }

        selectedSymbols[slot] = index;
    }

    public bool TryUnlock(out string message)
    {
        if (solved)
        {
            message = "Bu kilit zaten çözüldü.";
            return false;
        }

        for (int i = 0; i < selectedSymbols.Length; i++)
        {
            if (selectedSymbols[i] == -1)
            {
                message = "Üç alan için de bir sembol seç.";
                Debug.Log("Sembol kilidi: Eksik secim.", this);
                return false;
            }
        }

        for (int i = 0; i < expectedSequence.Length; i++)
        {
            if (selectedSymbols[i] != (int)expectedSequence[i])
            {
                message = "Sıralama yanlış. Mektuplardaki tarihleri kontrol et.";
                Debug.Log("Sembol kilidi: Yanlis sira.", this);
                return false;
            }
        }

        solved = true;

        message = "Kilit açıldı. İksir odasına geçebilirsin.";

        Debug.Log("Sembol kilidi: Dogru sira. OnSolved cagriliyor.", this);
        onSolved.Invoke();

        return true;
    }
}