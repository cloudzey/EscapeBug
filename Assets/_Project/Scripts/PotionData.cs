using UnityEngine;

[CreateAssetMenu(
    fileName = "Potion_New",
    menuName = "Escape Bug/Potion Data"
)]
public class PotionData : ScriptableObject
{
    [Header("Kimlik")]
    public string id;

    [Header("Oyuncunun gorecegi etiket")]
    [TextArea(2, 5)]
    public string label;

    public string symbol;

    [Header("Bulmaca")]
    public bool isCorrect;
}