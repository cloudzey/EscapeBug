using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionReference interactAction;

    [Header("Target Selection")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField, Range(1f, 89f)]
    private float viewHalfAngle = 60f;

    [SerializeField] private LayerMask interactionMask;
    [SerializeField] private LayerMask visibilityMask;

    private IInteractable currentTarget;
    private MonoBehaviour currentTargetBehaviour;

    private void OnEnable()
    {
        if (interactionOrigin == null ||
            cameraTransform == null ||
            interactAction == null ||
            interactAction.action == null)
        {
            Debug.LogError(
                "PlayerInteraction: Interaction Origin, Camera Transform " +
                "ve Interact Action alanlarýný doldur.",
                this
            );

            enabled = false;
            return;
        }

        if (interactionMask.value == 0 ||
            visibilityMask.value == 0)
        {
            Debug.LogError(
                "PlayerInteraction: Interaction Mask ve Visibility Mask " +
                "Nothing olmamalý.",
                this
            );

            enabled = false;
            return;
        }

        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.Disable();
        }

        currentTarget = null;
        currentTargetBehaviour = null;
    }

    private void Update()
    {
        FindTarget();

        if (interactAction.action.WasPressedThisFrame() &&
            currentTargetBehaviour != null &&
            currentTargetBehaviour.isActiveAndEnabled)
        {
            currentTarget.Interact();
        }
    }

    private void FindTarget()
    {
        currentTarget = null;
        currentTargetBehaviour = null;

        Vector3 origin = interactionOrigin.position;

        Collider[] candidates = Physics.OverlapSphere(
            origin,
            interactionRange,
            interactionMask,
            QueryTriggerInteraction.Ignore
        );

        float minimumDot =
            Mathf.Cos(viewHalfAngle * Mathf.Deg2Rad);

        float bestScore = float.NegativeInfinity;

        foreach (Collider candidate in candidates)
        {
            IInteractable interactable =
                candidate.GetComponentInParent<IInteractable>();

            MonoBehaviour behaviour =
                interactable as MonoBehaviour;

            if (behaviour == null || !behaviour.isActiveAndEnabled)
            {
                continue;
            }

            Vector3 targetPoint = candidate.bounds.center;
            Vector3 offset = targetPoint - origin;
            float distance = offset.magnitude;

            if (distance < 0.001f || distance > interactionRange)
            {
                continue;
            }

            Vector3 direction = offset / distance;

            float alignment = Vector3.Dot(
                cameraTransform.forward,
                direction
            );

            if (alignment < minimumDot)
            {
                continue;
            }

            bool hasHit = Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                distance + 0.05f,
                visibilityMask,
                QueryTriggerInteraction.Ignore
            );

            if (!hasHit)
            {
                continue;
            }

            IInteractable firstHit =
                hit.collider.GetComponentInParent<IInteractable>();

            if (!object.ReferenceEquals(firstHit, interactable))
            {
                continue;
            }

            float score =
                alignment - (distance / interactionRange) * 0.15f;

            if (score > bestScore)
            {
                bestScore = score;
                currentTarget = interactable;
                currentTargetBehaviour = behaviour;
            }
        }
    }

    private void OnGUI()
    {
        if (currentTargetBehaviour == null ||
            !currentTargetBehaviour.isActiveAndEnabled)
        {
            return;
        }

        Rect promptRect = new Rect(
            Screen.width * 0.5f - 160f,
            Screen.height - 90f,
            320f,
            40f
        );

        GUI.Box(
            promptRect,
            "E - " + currentTarget.DisplayName
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionOrigin == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            interactionOrigin.position,
            interactionRange
        );
    }
}