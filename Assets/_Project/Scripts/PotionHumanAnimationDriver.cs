using UnityEngine;

public class PotionHumanAnimationDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform movementRoot;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Animator animator;

    [Header("Movement Detection")]
    [SerializeField, Min(0f)]
    private float minimumSpeed = 0.05f;

    [SerializeField, Min(0.1f)]
    private float teleportDistance = 1.5f;

    [Header("Rotation")]
    [SerializeField, Min(1f)]
    private float rotationSpeed = 540f;

    [SerializeField]
    private float modelForwardOffset = 0f;

    private Vector3 previousPosition;
    private bool initialized;

    private static readonly int IsMoving =
        Animator.StringToHash("IsMoving");

    private void OnEnable()
    {
        initialized = false;

        if (animator != null)
            animator.SetBool(IsMoving, false);
    }

    private void LateUpdate()
    {
        if (movementRoot == null ||
            visualRoot == null ||
            animator == null)
        {
            return;
        }

        Vector3 currentPosition = movementRoot.position;

        if (!initialized)
        {
            previousPosition = currentPosition;
            initialized = true;
            animator.SetBool(IsMoving, false);
            return;
        }

        Vector3 delta = currentPosition - previousPosition;
        previousPosition = currentPosition;

        delta.y = 0f;

        if (delta.magnitude > teleportDistance)
        {
            animator.SetBool(IsMoving, false);
            return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        float horizontalSpeed = delta.magnitude / dt;

        bool moving = horizontalSpeed > minimumSpeed;

        animator.SetBool(IsMoving, moving);

        if (!moving)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(delta.normalized, Vector3.up);

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