using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProperMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 10f;
    public float groundCheckDistance = 0.1f;
    public float jumpCooldown = 0.5f;

    private bool isGrounded;
    private Transform groundCheck;
    private Rigidbody rb;
    private Animator anim;
    private bool isJumping = false;
    private float lastJumpTime = 0f;

    private MovingPlatform currentMovingPlatform;

    private void Start()
    {
        groundCheck = transform.Find("GroundCheck");
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        currentMovingPlatform = GetComponent<MovingPlatform>();
        // Set the initial rotation
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        anim.SetFloat("Blend", 0f);

        // Ensure rigidbody uses gravity
        rb.useGravity = true;
    }

    private void Update()
    {
        CheckGrounded();
        HandleJumpInput();

        if (isGrounded && CheckIfOnMovingPlatform())
        {
            transform.parent = currentMovingPlatform.transform;
        }
        else
        {
            transform.parent = null;
        }
    }

    private void FixedUpdate()
    {
        HandleMovementInput();
        HandleDirection();
        HandleJump();
    }

    private void CheckGrounded()
    {
        // Check if the player is grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, LayerMask.GetMask("Ground"));
    }

    private void HandleMovementInput()
    {
        // Horizontal movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;

        Vector3 newPosition = transform.position + new Vector3(horizontalInput * moveSpeed * Time.fixedDeltaTime, 0f, 0f);

        transform.position = newPosition;

        float idleBlend = 0f;
        float walkBlend = 0.2f;
        float runBlend = 0.4f;
        float jumpBlend = 0.6f;

        if (isGrounded)
        {
            float smoothBlend = Mathf.Lerp(anim.GetFloat("SmoothBlend"), isMoving ? (Input.GetKey(KeyCode.LeftShift) ? runBlend : walkBlend) : idleBlend, 0.1f);
            anim.SetFloat("SmoothBlend", smoothBlend);

            if (isMoving)
            {
                anim.SetFloat("Blend", anim.GetFloat("SmoothBlend"));
            }
            else
            {
                anim.SetFloat("Blend", 0f);
            }
        }
        else
        {
            anim.SetFloat("Blend", jumpBlend);
        }

        if (Input.GetButtonDown("Jump"))
        {
            anim.SetFloat("Blend", jumpBlend);
        }
    }

    private void HandleDirection()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(1f, 1f, -1f);
        }
        else if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private void HandleJumpInput()
    {
        if (isGrounded && Input.GetKeyDown("space") && Time.time - lastJumpTime > jumpCooldown)
        {
            isJumping = true;
            lastJumpTime = Time.time;
        }
    }

    private void HandleJump()
    {
        if (isJumping)
        {
            anim.SetFloat("Blend", 0.6f);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = false;
        }
        else if (rb.velocity.y < 0 && isGrounded)
        {
            float idleBlend = 0f;
            float walkBlend = 0.2f;
            float runBlend = 0.4f;
            float targetBlend = Input.GetKey(KeyCode.LeftShift) ? runBlend : walkBlend;
            float smoothBlend = Mathf.Lerp(anim.GetFloat("SmoothBlend"), targetBlend, 0.1f);
            anim.SetFloat("SmoothBlend", smoothBlend);

            anim.SetFloat("Blend", anim.GetFloat("SmoothBlend"));
        }
    }
    private bool CheckIfOnMovingPlatform()
    {
        // Check if the player is on the moving platform using a raycast
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, LayerMask.GetMask("MovingPlatform")))
        {
            MovingPlatform platform = hit.collider.GetComponentInParent<MovingPlatform>();
            return (platform != null && platform == currentMovingPlatform);
        }
        return false;
    }

    public void SetAttackAnimation()
    {
        // Set the animation blend value for attack
        anim.SetFloat("Blend", 0.8f);
    }
}
