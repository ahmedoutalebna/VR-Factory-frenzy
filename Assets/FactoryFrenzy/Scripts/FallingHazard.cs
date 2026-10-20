using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingHazard : MonoBehaviour
{
    [Header("Behaviour")]
    public float dropDelay = 0f;              // warning time before it falls
    public Vector3 pushOnDrop = Vector3.zero; // optional shove (world space), e.g. down a ramp
    public float minKillSpeed = 1f;           // only pushes the player while moving at least this fast
    public float resetDelay = 5f;             // seconds before it goes back (0 = never)

    [Header("Push on the player")]
    public float pushSpeed = 12f;             // horizontal push
    public float pushUp = 4f;                 // small upward kick
    public float pushCooldown = 0.5f;         // avoids pushing every frame while overlapping

    Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;
    bool dropped;
    float nextPushTime;

    float Speed
    {
        get
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity.magnitude;
#else
            return rb.velocity.magnitude;
#endif
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
        rb.isKinematic = true;                // stays put until triggered
    }

    public void Drop()
    {
        if (dropped) return;
        dropped = true;
        StartCoroutine(DropRoutine());
    }

    IEnumerator DropRoutine()
    {
        if (dropDelay > 0f) yield return new WaitForSeconds(dropDelay);

        rb.isKinematic = false;
        if (pushOnDrop != Vector3.zero) rb.AddForce(pushOnDrop, ForceMode.VelocityChange);

        if (resetDelay <= 0f) yield break;
        yield return new WaitForSeconds(resetDelay);

        // reset: stop it, put it back, freeze it, re-arm
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.SetPositionAndRotation(startPos, startRot);
        dropped = false;
    }

    void OnTriggerEnter(Collider other) { TryPush(other); }
    void OnTriggerStay(Collider other) { TryPush(other); }

    void TryPush(Collider other)
    {
        if (!dropped || rb.isKinematic || Speed < minKillSpeed) return;   // only while it is falling
        if (!other.CompareTag("Player") || Time.time < nextPushTime) return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        Vector3 dir = other.transform.position - transform.position;     // away from the hazard
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = transform.forward;           // directly underneath: pick a side
        dir.Normalize();

        pc.Launch(dir * pushSpeed + Vector3.up * pushUp);
        nextPushTime = Time.time + pushCooldown;
    }
}