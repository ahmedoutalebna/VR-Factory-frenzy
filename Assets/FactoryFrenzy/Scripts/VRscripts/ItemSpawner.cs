using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public Transform head;            // the XR camera (leave empty to use Camera.main)
    public float distance = 1.0f;     // how far in front of you the item appears
    public float heightBelowHead = 0.4f;   // chest height, easy to grab

    void Start()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }

    // Called by the buttons: each button passes its own prefab
    public void Spawn(GameObject prefab)
    {
        if (prefab == null || head == null) return;

        Vector3 fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        Vector3 pos = head.position + fwd * distance + Vector3.down * heightBelowHead;
        Quaternion rot = Quaternion.LookRotation(-fwd);   // faces you

        Instantiate(prefab, pos, rot);
    }
}