using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingHazard : MonoBehaviour
{
    [Header("Behaviour")]
    public float dropDelay = 0f;              // warning time before it falls
    public Vector3 pushOnDrop = Vector3.zero; // optional shove (world space), e.g. down a ramp
    public float minKillSpeed = 1f;           // only deadly while moving at least this fast
    public float resetDelay = 5f;             // seconds before it goes back (0 = never)

    Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;
    bool dropped;

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

    void OnTriggerEnter(Collider other) { TryKill(other); }
    void OnTriggerStay(Collider other) { TryKill(other); }

    void TryKill(Collider other)
    {
        if (!dropped || rb.isKinematic || Speed < minKillSpeed) return;
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerRespawn>()?.Respawn();
    }
}