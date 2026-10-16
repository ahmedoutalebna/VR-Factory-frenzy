using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    public FallingHazard hazard;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && hazard != null)
            hazard.Drop();
    }
}