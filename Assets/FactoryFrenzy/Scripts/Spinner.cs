using UnityEngine;

public class Spinner : MonoBehaviour
{
    public Vector3 axis = Vector3.up;        // Y = spins flat, like a sweeper
    public float degreesPerSecond = 120f;

    void Update()
    {
        transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}