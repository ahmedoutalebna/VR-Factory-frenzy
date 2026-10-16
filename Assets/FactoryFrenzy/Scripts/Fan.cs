using UnityEngine;

public class Fan : MonoBehaviour
{
    [Header("Wind")]
    public Collider windZone;              // trigger box in front of the fan
    public float windSpeed = 10f;          // max push speed on the player (m/s)
    public float liftAmount = 0f;          // optional upward component, 0 = pure horizontal

    [Header("Blades (optional visual)")]
    public Transform blades;               // the rotating part of the fan model
    public Vector3 spinAxis = Vector3.forward;
    public float spinSpeed = 720f;         // degrees per second

    Transform player;
    PlayerController pc;
    const float drag = 2.5f;               // must match PlayerController.externalDrag

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { player = p.transform; pc = p.GetComponent<PlayerController>(); }
        if (windZone == null) Debug.LogWarning("Fan: assign the Wind Zone collider.", this);
    }

    void Update()
    {
        if (blades != null)
            blades.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);

        if (pc == null || windZone == null || !pc.enabled) return;

        Vector3 pos = player.position + Vector3.up * 0.9f;
        bool inside = (windZone.ClosestPoint(pos) - pos).sqrMagnitude < 0.0001f;
        if (!inside) return;

        Vector3 dir = windZone.transform.forward;       // blue arrow of the Wind Zone
        dir.y = liftAmount;
        dir.Normalize();

        // acceleration so that the terminal push speed equals windSpeed
        pc.Launch(dir * (windSpeed * drag) * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (windZone == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(windZone.bounds.center, windZone.transform.forward * 3f);
    }
}