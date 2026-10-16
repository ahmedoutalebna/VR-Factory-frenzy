using UnityEngine;

public class Launcher : MonoBehaviour
{
    public enum LState { Search, Orient, Fire }
    public LState state = LState.Search;

    [Header("Parts")]
    public Transform pivot;                 // the rotating head (leave empty to rotate the whole object)
    public Transform muzzle;                // empty child at the barrel tip, blue arrow = shooting direction
    public Projectile projectilePrefab;

    [Header("Detection")]
    public Collider detectionZone;          // trigger box over the bridge
    public float detectionRadius = 20f;     // used only if no zone is assigned

    [Header("Search")]
    public float minYaw = -50f;
    public float maxYaw = 50f;
    public float searchSpeed = 40f;         // sweep speed, degrees per second

    [Header("Orient / Fire")]
    public float turnSpeed = 120f;
    public float fireAngle = 30f;           // fires when the player is within this angle of the barrel
    public float fireCooldown = 1.5f;
    public float projectileSpeed = 18f;

    Quaternion baseRot;
    float searchPhase;
    float nextFireTime;
    Transform player;

    void Start()
    {
        if (pivot == null) pivot = transform;
        if (muzzle == null) muzzle = pivot;
        baseRot = pivot.rotation;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    bool PlayerInZone()
    {
        if (player == null) return false;

        Vector3 pos = player.position + Vector3.up * 0.9f;
        if (detectionZone != null)
            return (detectionZone.ClosestPoint(pos) - pos).sqrMagnitude < 0.0001f;

        return (player.position - transform.position).sqrMagnitude <= detectionRadius * detectionRadius;
    }

    float SignedAngleToPlayer()
    {
        Vector3 fwd = Vector3.ProjectOnPlane(muzzle.forward, Vector3.up);
        Vector3 to = Vector3.ProjectOnPlane(player.position - pivot.position, Vector3.up);
        return Vector3.SignedAngle(fwd, to, Vector3.up);
    }

    void Update()
    {
        if (PlayerInZone())
        {
            float angle = SignedAngleToPlayer();
            TurnBy(angle);                                   // keep tracking the player

            if (Mathf.Abs(angle) > fireAngle) state = LState.Orient;
            else { state = LState.Fire; TryFire(); }
        }
        else
        {
            state = LState.Search;
            Sweep();
        }
    }

    void TurnBy(float angle)
    {
        float max = turnSpeed * Time.deltaTime;
        pivot.Rotate(Vector3.up, Mathf.Clamp(angle, -max, max), Space.World);
    }

    void Sweep()
    {
        searchPhase += Time.deltaTime * searchSpeed;
        float range = Mathf.Max(1f, maxYaw - minYaw);
        float yaw = minYaw + Mathf.PingPong(searchPhase, range);

        Quaternion desired = Quaternion.AngleAxis(yaw, Vector3.up) * baseRot;
        pivot.rotation = Quaternion.RotateTowards(pivot.rotation, desired, turnSpeed * Time.deltaTime);
    }

    void TryFire()
    {
        if (Time.time < nextFireTime || projectilePrefab == null) return;
        nextFireTime = Time.time + fireCooldown;

        Vector3 aimPoint = player.position + Vector3.up * 1f;               // chest height
        Vector3 dir = (aimPoint - muzzle.position).normalized;
        var p = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir));
        p.speed = projectileSpeed;
        p.owner = transform;
    }

    void OnDrawGizmosSelected()
    {
        Transform m = muzzle != null ? muzzle : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(m.position, m.forward * 3f);
    }
}