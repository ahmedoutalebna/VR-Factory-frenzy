using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FireExtinguisher : MonoBehaviour
{
    [SerializeField] GameObject foamGo;
    [SerializeField] GameObject colliderGo;

    [Tooltip("Number of points per second it removes from a Fire")]
    public float power = 20f;

    [Header("Optional (F6)")]
    [Tooltip("Amount of foam available. Set to 0 or less to disable the limit.")]
    [SerializeField] private float quantity = 100f;
    [Tooltip("Foam consumed per second while the extinguisher is active")]
    [SerializeField] private float consumptionPerSec = 5f;

    XRGrabInteractable xrGrab;
    private bool isActive;
    private bool limitedFoam;

    // Start is called before the first frame update
    void Start()
    {
        limitedFoam = quantity > 0f;

        // Get the XRGrabInteractable component
        xrGrab = GetComponent<XRGrabInteractable>();

        // Register on the activated event (trigger pressed)
        xrGrab.activated.AddListener(_ => setActivation(true));

        // Register on the deactivated event (trigger released)
        xrGrab.deactivated.AddListener(_ => setActivation(false));

        // Start switched off
        setActivation(false);
    }

    private void OnDestroy()
    {
        if (xrGrab != null)
        {
            xrGrab.activated.RemoveAllListeners();
            xrGrab.deactivated.RemoveAllListeners();
        }
    }

    private void Update()
    {
        // Optional: consume foam while active, stop when empty
        if (isActive && limitedFoam)
        {
            quantity -= consumptionPerSec * Time.deltaTime;
            if (quantity <= 0f)
            {
                quantity = 0f;
                setActivation(false);
            }
        }
    }

    private void setActivation(bool active)
    {
        // Can't activate an empty extinguisher
        if (active && limitedFoam && quantity <= 0f)
            active = false;

        isActive = active;

        // Make foam appear / disappear
        foamGo.SetActive(active);

        // Make collider appear / disappear
        colliderGo.SetActive(active);
    }

    private void OnTriggerStay(Collider other)
    {
        // Find the Fire component hit by the extinguisher
        // (InParent so it works if the collider is on a child of the Fire prefab)
        Fire fire = other.GetComponentInParent<Fire>();
        if (fire != null)
        {
            // Reduce the fire health according to the power of the extinguisher
            fire.Health -= power * Time.deltaTime;
        }
    }
}