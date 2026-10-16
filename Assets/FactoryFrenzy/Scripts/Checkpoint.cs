using System.Collections;
using UnityEngine;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint;          // where the player reappears

    [Header("Bulb")]
    public Renderer bulb;                   // the bulb's Mesh Renderer
    public int materialIndex = 0;           // which material slot is the bulb
    public Material inactiveMaterial;       // red
    public Material activeMaterial;         // green (Bulb_Green)

    [Header("Popup (optional)")]
    public TMP_Text messageText;
    public float messageDuration = 1.5f;

    bool activated;

    void Start()
    {
        if (respawnPoint == null) respawnPoint = transform;
        SetBulb(inactiveMaterial);
        if (messageText != null) messageText.text = "";
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated || !other.CompareTag("Player")) return;

        var respawn = other.GetComponent<PlayerRespawn>();
        if (respawn == null) return;

        activated = true;
        Debug.Log("Checkpoint activated: " + name);
        respawn.SetCheckpoint(respawnPoint.position, respawnPoint.rotation);
        SetBulb(activeMaterial);

        if (messageText != null) StartCoroutine(ShowMessage());
    }

    void SetBulb(Material m)
    {
        if (bulb == null || m == null) return;
        var mats = bulb.sharedMaterials;
        if (materialIndex < 0 || materialIndex >= mats.Length) return;
        mats[materialIndex] = m;
        bulb.sharedMaterials = mats;        // only affects this checkpoint's renderer
    }

    IEnumerator ShowMessage()
    {
        messageText.text = "Checkpoint!";
        yield return new WaitForSeconds(messageDuration);
        messageText.text = "";
    }
}