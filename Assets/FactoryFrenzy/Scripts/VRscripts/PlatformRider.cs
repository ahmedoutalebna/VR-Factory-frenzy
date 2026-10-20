using UnityEngine;

public class PlatformRider : MonoBehaviour
{
    public Transform head;             // leave empty to use Camera.main
    public float checkDistance = 0.6f; // how far below the feet we look for a platform
    public float maxJump = 2f;         // ignore huge jumps (platform restarting)

    Transform current;
    Vector3 lastPos;

    void LateUpdate()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;
        if (head == null) return;

        // ray from under the head, at floor level
        Vector3 origin = new Vector3(head.position.x, transform.position.y + 0.1f, head.position.z);
        Transform platform = null;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, checkDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponentInParent<PlatformSettings>();
            if (p != null) platform = p.transform;
        }

        if (platform != null && platform == current)
        {
            Vector3 delta = platform.position - lastPos;
            if (delta.sqrMagnitude < maxJump * maxJump) transform.position += delta;
        }

        current = platform;
        if (platform != null) lastPos = platform.position;
    }
}