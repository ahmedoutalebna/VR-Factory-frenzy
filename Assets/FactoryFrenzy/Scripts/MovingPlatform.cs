using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 endOffset = new Vector3(0f, 0f, 6f);  // end = start + offset (world space)
    public float travelTime = 3f;                         // seconds, one way
    public bool smooth = true;                            // ease in/out at the ends

    [Header("Rotation (optional)")]
    public Vector3 rotationAxis = Vector3.up;             // local axis to spin around
    public float rotationSpeed = 0f;                      // degrees per second, 0 = no rotation

    public Vector3 DeltaMove { get; private set; }        // how far it moved this frame

    Vector3 startPos, endPos;
    float startTime;
    bool configured;

    void Start()
    {
        if (!configured)
            Configure(transform.position, transform.position + endOffset, travelTime);
    }

    // The level loader can call this later with the values read from the JSON
    public void Configure(Vector3 start, Vector3 end, float time)
    {
        startPos = start;
        endPos = end;
        travelTime = Mathf.Max(0.01f, time);
        transform.position = start;
        startTime = Time.time;
        configured = true;
    }

    void Update()
    {
        float t = Mathf.PingPong((Time.time - startTime) / travelTime, 1f);
        if (smooth) t = Mathf.SmoothStep(0f, 1f, t);

        Vector3 before = transform.position;
        transform.position = Vector3.Lerp(startPos, endPos, t);
        DeltaMove = transform.position - before;

        if (rotationSpeed != 0f)
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 s = Application.isPlaying ? startPos : transform.position;
        Vector3 e = Application.isPlaying ? endPos : transform.position + endOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(s, e);
        Gizmos.DrawWireSphere(e, 0.3f);
    }
}