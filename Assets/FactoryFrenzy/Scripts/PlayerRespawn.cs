using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerRespawn : MonoBehaviour
{
    Vector3 respawnPos;
    Quaternion respawnRot;
    CharacterController cc;
    PlayerController pc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController>();
        // first checkpoint = where the player starts (on the Start platform)
        respawnPos = transform.position;
        respawnRot = transform.rotation;
    }

    public void SetCheckpoint(Vector3 pos, Quaternion rot)
    {
        respawnPos = pos;
        respawnRot = rot;
    }

    public void Respawn()
    {
        ResultsScreen.DeathCount++;
        cc.enabled = false;                       // a CharacterController blocks direct teleporting
        transform.SetPositionAndRotation(respawnPos, respawnRot);
        cc.enabled = true;
        if (pc != null) pc.ResetMotion();
    }
}