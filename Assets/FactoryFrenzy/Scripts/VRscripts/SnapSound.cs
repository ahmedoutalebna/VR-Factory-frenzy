using UnityEngine;

public class SnapSound : MonoBehaviour
{
    // wired to the socket's "Select Entered" event
    public void Play() { EditorSounds.PlaySnap(transform.position); }
}