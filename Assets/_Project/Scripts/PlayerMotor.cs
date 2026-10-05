using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedVelocity = -2f;

    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (moveAction == null || moveAction.action == null)
        {
            Debug.LogError(
                "PlayerMotor: Move Action alanýna Gameplay/Move baðlanmalý.",
                this
            );

            enabled = false;
            return;
        }

        verticalVelocity = 0f;
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Disable();
        }
    }

    private void Update()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 direction = new Vector3(input.x, 0f, input.y);

        direction = Vector3.ClampMagnitude(direction, 1f);

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVelocity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = direction * moveSpeed;
        velocity.y = verticalVelocity;

        CollisionFlags collisions =
            controller.Move(velocity * Time.deltaTime);

        if ((collisions & CollisionFlags.Above) != 0 &&
            verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }
    }
}