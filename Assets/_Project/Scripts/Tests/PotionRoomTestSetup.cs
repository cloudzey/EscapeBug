using UnityEngine;

public class PotionRoomTestSetup : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;

    [ContextMenu("Test/Prepare Completed Archive")]
    private void PrepareCompletedArchive()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Bu komutu Play modunda kullan.", this);
            return;
        }

        if (gameProgress == null)
        {
            Debug.LogWarning("GameProgress bağlantısı eksik.", this);
            return;
        }

        gameProgress.MarkArchivePuzzleSolved();

        Debug.Log(
            "TEST KURULUMU: Önceki odanın arşiv bulmacası tamamlanmış kabul edildi.",
            this
        );
    }
}