using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;            // the root Player
    public float distance = 5f;
    public float height = 1.6f;         // look-at height above the player's feet
    public float sensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 60f;
    public LayerMask obstacleMask = ~0;

    float yaw;
    float pitch = 15f;

    void Start()
    {
        if (target != null) yaw = target.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Vector3 focus = target.position + Vector3.up * height;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 dir = rot * Vector3.back;

        // pull the camera in if something is between it and the player
        float d = distance;
        if (Physics.SphereCast(focus, 0.2f, dir, out RaycastHit hit, distance,
                               obstacleMask, QueryTriggerInteraction.Ignore)
            && !hit.transform.IsChildOf(target))
            d = hit.distance;

        transform.SetPositionAndRotation(focus + dir * d, rot);
    }
}