using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RobotHead : MonoBehaviour
{
    [SerializeField] private Fire fire;
    [SerializeField] private RobotFireTrigger robot;
    [SerializeField] private Transform neckPoint;        // empty object where the head belongs
    [SerializeField] private float snapDistance = 0.3f;  // how close counts as "on the neck"

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private InteractionLayerMask originalLayers;
    private bool fireSeen;

    // original pose on the neck
    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        originalLayers = grab.interactionLayers;

        originalParent = transform.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;

        grab.interactionLayers = 0; // locked at start
    }

    private void OnEnable() => grab.selectExited.AddListener(OnReleased);
    private void OnDisable() => grab.selectExited.RemoveListener(OnReleased);

    public void OnPopped() { }

    private void Update()
    {
        bool burning = fire != null && fire.gameObject.activeInHierarchy && fire.Health > 0f;
        if (burning) fireSeen = true;

        bool canGrab = fireSeen && !burning;
        grab.interactionLayers = canGrab ? originalLayers : (InteractionLayerMask)0;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (!fireSeen) return;

        // Released close enough to the neck? Put it back.
        if (Vector3.Distance(transform.position, neckPoint.position) <= snapDistance)
            Reattach();
    }

    private void Reattach()
    {
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        transform.SetParent(originalParent);
        transform.localPosition = originalLocalPos;
        transform.localRotation = originalLocalRot;

        fireSeen = false;   // locked again until the next fire
        robot.Repair();
    }
}