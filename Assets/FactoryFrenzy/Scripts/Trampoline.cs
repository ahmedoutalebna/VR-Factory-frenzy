using System.Collections;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Bounce")]
    public float bounceHeight = 6f;      // how high the player flies, in metres
    public AudioSource bounceSound;      // optional

    [Header("Animation")]
    public Transform visual;             // the part that moves (Props_4a)
    public float animHeight = 0.5f;      // how far it rises, in metres
    public float animDuration = 0.35f;   // total time, up and back

    Vector3 visualStartPos;
    Coroutine anim;

    void Start()
    {
        if (visual != null) visualStartPos = visual.localPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // launch speed needed to reach bounceHeight under the player's gravity
        pc.SetVerticalVelocity(Mathf.Sqrt(2f * bounceHeight * -pc.gravity));

        if (bounceSound != null) bounceSound.Play();

        if (visual != null)
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(Animate());
        }
    }

    IEnumerator Animate()
    {
        float t = 0f;
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / animDuration);
            float offset = Mathf.Sin(k * Mathf.PI);          // 0 -> 1 -> 0
            visual.localPosition = visualStartPos + Vector3.up * (animHeight * offset);
            yield return null;
        }
        visual.localPosition = visualStartPos;               // back to the original position
        anim = null;
    }
}