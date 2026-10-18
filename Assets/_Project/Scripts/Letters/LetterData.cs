using UnityEngine;

[CreateAssetMenu(
    fileName = "Letter_New",
    menuName = "Escape Bug/Letter Data"
)]
public class LetterData : ScriptableObject
{
    [Header("Kimlik")]
    public string id;

    [Header("Okuma paneli")]
    public string title;

    [TextArea(8, 24)]
    public string body;

    public string displayedDate;

    [Header("Arsiv siralamasi")]
    public int orderIndex;

    public string symbol;

    [Header("Kanit")]
    public bool isEvidence;
}