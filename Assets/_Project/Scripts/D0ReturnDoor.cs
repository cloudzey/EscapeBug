
using System.Collections;
using UnityEngine;

public class D0ReturnDoor : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private Transform doorLeaf;

    [SerializeField] private Vector3 openOffset =
        new Vector3(0f, 2.5f, 0f);

    [SerializeField] private float closeDuration = 1f;

    private Vector3 closedPosition;
    private bool closingStarted;

    private void Awake()
    {
        if (doorLeaf == null)
        {
            Debug.LogError("D0: DoorLeaf atanmadı.", this);
            enabled = false;
            return;
        }

        closedPosition = doorLeaf.localPosition;
        doorLeaf.localPosition = closedPosition + openOffset;
    }

    private void Update()
    {
        if (closingStarted || gameProgress == null)
            return;

        if (!gameProgress.maintenanceEntered)
            return;

        closingStarted = true;
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        Vector3 start = doorLeaf.localPosition;
        float duration = Mathf.Max(0.01f, closeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            doorLeaf.localPosition = Vector3.Lerp(
                start,
                closedPosition,
                Mathf.SmoothStep(0f, 1f, t)
            );

            yield return null;
        }

        doorLeaf.localPosition = closedPosition;

        Debug.Log("D0 kapandı. Geri dönüş kilitlendi.", this);
    }
}
