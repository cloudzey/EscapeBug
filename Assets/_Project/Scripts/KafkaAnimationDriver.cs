using UnityEngine;

[RequireComponent(typeof(Animator))]
public class KafkaAnimationDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CharacterController playerController;

    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float movementThreshold = 0.08f;

    private Animator animator;

    private static readonly int IsMoving =
        Animator.StringToHash("IsMoving");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (playerController == null)
        {
            playerController =
                GetComponentInParent<CharacterController>();
        }

        animator.applyRootMotion = false;
    }

    private void Update()
    {
        if (playerController == null)
            return;

        Vector3 velocity = playerController.velocity;

        velocity.y = 0f;

        bool moving =
            velocity.sqrMagnitude >
            movementThreshold * movementThreshold;

        animator.SetBool(IsMoving, moving);
    }
}