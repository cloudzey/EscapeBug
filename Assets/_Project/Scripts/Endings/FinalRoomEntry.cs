using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class FinalRoomEntry : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Transform finalCameraPoint;
    [SerializeField] private Image fadeImage;

    [SerializeField, Min(0.05f)]
    private float fadeDuration = 0.5f;

    private void Awake()
    {
        PlayerControlLocks.Acquire(this);

        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
            fadeImage.raycastTarget = false;
        }

        if (sceneCamera != null)
        {
            ThirdPersonCamera follow =
                sceneCamera.GetComponent<ThirdPersonCamera>();

            if (follow != null)
                follow.enabled = false;
        }
    }

    private IEnumerator Start()
    {
        if (gameProgress == null ||
            sceneCamera == null ||
            finalCameraPoint == null ||
            fadeImage == null ||
            !sceneCamera.isActiveAndEnabled ||
            !fadeImage.isActiveAndEnabled)
        {
            Debug.LogError(
                "FinalRoomEntry: Kamera, GameProgress veya " +
                "FadeImage baglantisi eksik ya da kapali.",
                this
            );

            if (fadeImage != null)
                fadeImage.color = Color.clear;

            yield break;
        }

        bool transferred = FinalSceneTransfer.TryConsume(
            gameObject.scene.name,
            out GameProgress.TransferData data
        );

        if (transferred)
        {
            gameProgress.RestoreTransferData(data);
        }
        else
        {
            gameProgress.finalDialogueActive = false;

            Debug.LogWarning(
                "FinalRoomTest dogrudan acildi; aktarilmis ilerleme yok. " +
                "Bu oturum yalnizca sahne onizlemesidir.",
                this
            );
        }

        sceneCamera.transform.SetPositionAndRotation(
            finalCameraPoint.position,
            finalCameraPoint.rotation
        );

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;

        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float alpha =
                1f - Mathf.Clamp01(elapsed / duration);

            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImage.color = Color.clear;

        while (HasHeldSubmitInput())
            yield return null;

        yield return null;

        if (transferred)
        {
            gameProgress.finalDialogueActive = true;

            Debug.Log(
                "Final girisi hazir. Ilerleme aktarildi, " +
                "final kamerasi etkin, hareket kilitli.",
                this
            );
        }
    }

    private static bool HasHeldSubmitInput()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        Gamepad gamepad = Gamepad.current;

        return
            (keyboard != null &&
             (keyboard.eKey.isPressed ||
              keyboard.enterKey.isPressed ||
              keyboard.numpadEnterKey.isPressed ||
              keyboard.spaceKey.isPressed)) ||
            (mouse != null && mouse.leftButton.isPressed) ||
            (gamepad != null && gamepad.buttonSouth.isPressed);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        PlayerControlLocks.Release(this);

        if (gameProgress != null)
            gameProgress.finalDialogueActive = false;
    }
}