using UnityEngine;

public static class FinalSceneTransfer
{
    private static GameProgress.TransferData pendingData;
    private static string targetScene;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Clear();
    }

    public static void Prepare(
        GameProgress progress,
        string sceneName)
    {
        pendingData = progress.CaptureTransferData();
        targetScene = sceneName;
    }

    public static bool TryConsume(
        string sceneName,
        out GameProgress.TransferData data)
    {
        data = null;

        if (pendingData == null || targetScene != sceneName)
            return false;

        data = pendingData;
        Clear();
        return true;
    }

    public static void Clear()
    {
        pendingData = null;
        targetScene = null;
    }
}