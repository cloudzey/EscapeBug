using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class FinalExitTrigger : MonoBehaviour
{
    [SerializeField] private FinalEndingController ending;

    private void Awake()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        zone.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ending != null)
            ending.TryFinish(other);
    }
}