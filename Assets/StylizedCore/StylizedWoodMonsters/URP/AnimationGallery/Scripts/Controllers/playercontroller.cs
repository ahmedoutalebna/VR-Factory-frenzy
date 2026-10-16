using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 6f;
    public float turnSpeed = 720f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Jump feel")]
    public float coyoteTime = 0.15f;
    public float jumpBuffer = 0.15f;

    [Header("Ground check")]
    public float groundCheckRadius = 0.25f;
    public float groundCheckDistance = 0.2f;

    [Header("Knockback")]
    public float externalDrag = 2.5f;

    [Header("Animation (exact Animator state names)")]
    public Animator animator;
    public string idleState = "Idle";
    public string runState = "Running";
    public float crossFade = 0.15f;

    CharacterController cc;
    float verticalVelocity;
    string currentState;
    float lastGroundedTime = -10f;
    float lastJumpPressedTime = -10f;
    Vector3 externalVelocity;

    public void SetVerticalVelocity(float v) { verticalVelocity = v; }

    // Push the player: horizontal part fades out over time, vertical goes through gravity
    public void Launch(Vector3 velocity)
    {
        externalVelocity += new Vector3(velocity.x, 0f, velocity.z);
        if (velocity.y != 0f) verticalVelocity = velocity.y;
    }

    // Called on respawn so no leftover push carries over
    public void ResetMotion()
    {
        externalVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    bool CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * (groundCheckRadius + 0.1f);
        float dist = 0.1f + groundCheckDistance;
        return Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _, dist, ~0, QueryTriggerInteraction.Ignore);
    }

    void Update()
    {
        float x = 0f, z = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        Vector2 input = Vector2.ClampMagnitude(new Vector2(x, z), 1f);

        Transform cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 move = fwd * input.y + right * input.x;
        bool moving = move.sqrMagnitude > 0.01f;

        if (moving)
        {
            Quaternion target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }

        bool grounded = cc.isGrounded || CheckGround();
        if (grounded && verticalVelocity <= 0f) lastGroundedTime = Time.time;
        if (Input.GetKeyDown(KeyCode.Space)) lastJumpPressedTime = Time.time;
        if (grounded && verticalVelocity < 0f) verticalVelocity = -2f;

        if (Time.time - lastGroundedTime <= coyoteTime && Time.time - lastJumpPressedTime <= jumpBuffer)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpPressedTime = -10f;
            lastGroundedTime = -10f;
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * runSpeed + externalVelocity + Vector3.up * verticalVelocity;
        cc.Move(velocity * Time.deltaTime);
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, externalDrag * Time.deltaTime);

        Play(moving ? runState : idleState);
    }

    void LateUpdate()   // carry the player on moving platforms
    {
        if (!cc.enabled) return;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.6f, ~0, QueryTriggerInteraction.Ignore))
        {
            var platform = hit.collider.GetComponentInParent<MovingPlatform>();
            if (platform != null && platform.DeltaMove != Vector3.zero) cc.Move(platform.DeltaMove);
        }
    }

    void Play(string state)
    {
        if (animator == null || state == currentState) return;
        currentState = state;
        animator.CrossFade(state, crossFade);
    }
}