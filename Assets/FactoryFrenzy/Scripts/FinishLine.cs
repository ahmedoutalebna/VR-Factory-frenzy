using UnityEngine;

public class FinishLine : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (RaceManager.Instance == null)
        {
            Debug.LogError("No RaceManager in the scene");
            return;
        }
        RaceManager.Instance.OnPlayerFinished();
    }
}