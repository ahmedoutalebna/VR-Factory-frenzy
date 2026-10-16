using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RobotFireTrigger : MonoBehaviour
{
    [Header("Head")]
    [SerializeField] private RobotHead head;
    [SerializeField] private Rigidbody headRigidbody;
    [SerializeField] private float airTime = 1f;
    [SerializeField] private float throwDistance = 1.5f;
    [SerializeField] private float popTorque = 0.5f;

    [Header("Fire")]
    [SerializeField] private float fireDelay = 2f;

    // The menu commands (Call / Patrol / GoTo) check this
    public static bool RobotBroken { get; private set; }

    private GameObject fireObject;
    private NavMeshAgent robotAgent;
    private Animator robotAnimator;
    private Rigidbody robotBody;
    private readonly List<MonoBehaviour> stoppedScripts = new List<MonoBehaviour>();

    private InputAction startFireAction;
    private bool fireStarted;

    private void Awake()
    {
        RobotBroken = false;

        // Found automatically
        var fire = FindObjectOfType<Fire>(true);
        fireObject = fire != null ? fire.gameObject : null;

        robotAgent = GetComponentInParent<NavMeshAgent>();
        if (robotAgent == null) robotAgent = GetComponentInChildren<NavMeshAgent>();
        if (robotAgent == null) robotAgent = FindObjectOfType<NavMeshAgent>();

        Transform robotRoot = robotAgent != null ? robotAgent.transform : transform;
        robotAnimator = robotRoot.GetComponentInChildren<Animator>();
        robotBody = robotRoot.GetComponent<Rigidbody>();

        if (fireObject == null) Debug.LogError("RobotFireTrigger: no Fire found in the scene");
        if (head == null || headRigidbody == null) Debug.LogError("RobotFireTrigger: Head or Head Rigidbody is not assigned");

        startFireAction = new InputAction("StartFire", InputActionType.Button, "<Keyboard>/f");
        if (fireObject != null) fireObject.SetActive(false);
    }

    private void OnEnable()
    {
        startFireAction.performed += OnStartFire;
        startFireAction.Enable();
    }

    private void OnDisable()
    {
        startFireAction.performed -= OnStartFire;
        startFireAction.Disable();
    }

    private void OnStartFire(InputAction.CallbackContext ctx)
    {
        if (fireStarted || headRigidbody == null) return;
        fireStarted = true;
        StartCoroutine(FireSequence());
    }

    private IEnumerator FireSequence()
    {
        StopRobot();
        if (head != null) head.OnPopped();

        // 1. Throw the head
        headRigidbody.transform.SetParent(null);
        headRigidbody.transform.position += Vector3.up * 0.1f;
        headRigidbody.isKinematic = false;
        headRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        headRigidbody.WakeUp();
        yield return new WaitForFixedUpdate();

        float upSpeed = Mathf.Abs(Physics.gravity.y) * airTime * 0.5f;
        Vector2 dir2D = Random.insideUnitCircle.normalized;
        Vector3 horizontal = new Vector3(dir2D.x, 0f, dir2D.y) * (throwDistance / airTime);

        headRigidbody.AddForce(horizontal + Vector3.up * upSpeed, ForceMode.VelocityChange);
        headRigidbody.AddTorque(Random.insideUnitSphere * popTorque, ForceMode.Impulse);

        // 2. Wait, then start the fire
        yield return new WaitForSeconds(fireDelay);

        if (fireObject != null)
        {
            fireObject.SetActive(true);
            foreach (var ps in fireObject.GetComponentsInChildren<ParticleSystem>(true))
                ps.Play(true);
        }
    }

    private void StopRobot()
    {
        RobotBroken = true;
        stoppedScripts.Clear();

        // Scripts first, so nothing calls SetDestination on a disabled agent
        var owners = new HashSet<GameObject> { gameObject };
        if (robotAgent != null) owners.Add(robotAgent.gameObject);

        foreach (var owner in owners)
        {
            foreach (var script in owner.GetComponents<MonoBehaviour>())
            {
                if (script == null || script == this || !script.enabled) continue;
                if (script is RobotHead || script is Fire || script is XRBaseInteractable) continue;

                script.enabled = false;
                stoppedScripts.Add(script);
            }
        }

        if (robotAgent != null && robotAgent.isActiveAndEnabled)
        {
            robotAgent.isStopped = true;
            robotAgent.ResetPath();
            robotAgent.velocity = Vector3.zero;
            robotAgent.enabled = false;
        }

        if (robotBody != null && !robotBody.isKinematic)
        {
            robotBody.velocity = Vector3.zero;
            robotBody.angularVelocity = Vector3.zero;
        }
        if (robotAnimator != null) robotAnimator.enabled = false;
    }

    // Called by RobotHead when the head is put back on the neck
    public void Repair()
    {
        RobotBroken = false;
        fireStarted = false;

        if (fireObject != null) fireObject.SetActive(false);

        // Agent first, then the scripts
        if (robotAgent != null) robotAgent.enabled = true;
        foreach (var script in stoppedScripts)
            if (script != null) script.enabled = true;
        stoppedScripts.Clear();

        if (robotAnimator != null) robotAnimator.enabled = true;
    }
}