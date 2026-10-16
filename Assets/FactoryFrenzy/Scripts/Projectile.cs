using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 18f;
    public float pushSpeed = 14f;       // horizontal shove
    public float pushUp = 4f;           // small hop so the player flies off
    public float lifetime = 5f;
    [HideInInspector] public Transform owner;

    void Start() { Destroy(gameObject, lifetime); }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (owner != null && other.transform.IsChildOf(owner)) return;   // ignore the turret itself

        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                Vector3 dir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                pc.Launch(dir * pushSpeed + Vector3.up * pushUp);        // push, not kill
            }
        }
        Destroy(gameObject);
    }
}