using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Transform doorVisual;
    [SerializeField]
    private Vector3 openOffset =
        new Vector3(0f, 2.5f, 0f);

    [SerializeField, Min(0.01f)]
    private float openDuration = 1f;

    public bool IsOpen { get; private set; }
    public bool IsOpening { get; private set; }

    private Vector3 closedLocalPosition;

    private void Awake()
    {
        if (doorVisual == null)
        {
            Debug.LogError(
                "DoorController: Door Visual alanýna kapý panelini baðla.",
                this
            );

            enabled = false;
            return;
        }

        closedLocalPosition = doorVisual.localPosition;
    }

    public void Open()
    {
        if (!isActiveAndEnabled ||
            doorVisual == null ||
            IsOpen ||
            IsOpening)
        {
            return;
        }

        IsOpening = true;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector3 start = doorVisual.localPosition;
        Vector3 end = closedLocalPosition + openOffset;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, openDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            doorVisual.localPosition =
                Vector3.Lerp(start, end, smoothT);

            yield return null;
        }

        doorVisual.localPosition = end;

        IsOpening = false;
        IsOpen = true;

        Debug.Log("Kapý açýldý.", this);
    }
}