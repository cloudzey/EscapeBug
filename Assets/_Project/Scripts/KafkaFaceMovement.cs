
using UnityEngine;

public class KafkaFaceMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CharacterController playerController;

    [SerializeField]
    private Transform visualRoot;

    [Header("Rotation Settings")]
    [SerializeField, Min(1f)]
    private float rotationSpeed = 540f;

    [SerializeField, Min(0f)]
    private float minimumSpeed = 0.1f;

    [SerializeField]
    private float modelForwardOffset = 0f;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;

        if (playerController == null)
        {
            playerController =
                GetComponentInParent<CharacterController>();
        }
    }

    private void LateUpdate()
    {
        if (playerController == null || visualRoot == null)
            return;

        Vector3 direction = playerController.velocity;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            minimumSpeed * minimumSpeed)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized, Vector3.up);

        targetRotation *= Quaternion.Euler(
            0f,
            modelForwardOffset,
            0f
        );

        visualRoot.rotation = Quaternion.RotateTowards(
            visualRoot.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
