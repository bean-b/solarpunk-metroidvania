using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerHandler : MonoBehaviour
{
    [HideInInspector] private float topSpeed3 = 26; //sprinting
    private float topSpeed;
    [HideInInspector] private float topSpeed2 = 18; //running
    [HideInInspector] private float accel = 175; //speed mod
    [HideInInspector] private float accelDefault = 175; //speed mod
    [HideInInspector] private float accelWallJump = 50; //speed mod
    private float walljumpCoyoteTime = 0.125f;
    [HideInInspector] private float jumpingPower = 68; //jump power
    [HideInInspector] private float jumpingPowerSecond = 33; //jump power
    [HideInInspector] private float jumpingSecondGravReduction =0.05f;
    [HideInInspector] public float gravityMod = 6.5f;
    [HideInInspector] private float gravityJumpMod = 1.25f;
    [HideInInspector] private float gravityJumpModSec = 2f;
    [HideInInspector] private float gravRestoreTime = 0.2F; 
    private bool isFacingRight = true; //which direction facing
    [HideInInspector] public Rigidbody2D rb; //our rigid body
    [HideInInspector] public float groundCheckDistance = 2.5f; // How far down we check for ground
    [HideInInspector] public int numberOfRays = 10; // Number of rays to cast
    [HideInInspector] public float width = 0.9f;
    [HideInInspector] public float height = 1f;
    [HideInInspector] public float respawnTime = 1f;

    private float groundedTime  = 0f;
    private float coyoteJump = 0.125f;
    private float right2 = 1;
    private float wallSlideSpeed = -2f;
    private float wallSlideSpeed1 = -2f;
    private float wallSlideSpee2d = -15f;
    private float wallSlideTime = 0f;

    private bool canMove = true;

    public Transform leftCheckStartPoint;
    public Transform rightCheckStartPoint;
    public Transform groundCheckStartPoint; //this is used to check if we are touching the ground, essently a circle below our feet  ||| can be put as getComponenet later
    public LayerMask groundLayer; //this layermask controls what counts as 'ground', add more layers for dif types of ground etc..
    [SerializeField] private Transform gunPivot; //where the gun pivots from. currently set from the center of the player for gameplay reasons, can add sprites later though 
    [SerializeField] public GrapplingRope grapplingRope; // our grappling rope
    [SerializeField] private GrapplingGun grapplingGun; //our grappling gun 
    [HideInInspector] public float maxDeadTime = 20; // max time we can be slow while grapling before it breaks
    [HideInInspector] public float curDeadTime = 0; //how long weve been slow 

    [HideInInspector] public float deadSpeed = 12; // minimum speed to not be considered slow grapling
    [HideInInspector] public Vector2 maxSpeed = new Vector2(50,50); //overall max possible speed
    
    private Transform originalParent;


    private bool wallSliding = false;
    
    private Animator animator;

    public Vector2 respawnPoint;

    private Vector2 lastGrapple; //a vector2 of the speed of the last frame that the grapple contributed to our rigid body, prevents the grappling hook from rocketing the player


    private CameraMotor cam;

    private Boolean hasWallJumped = false;
    private float wallJumpingDirection;
    private float wallJumpingTime;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 12;
    private Vector2 wallJumpingPower = new Vector2(2.2f, 1.7f);



    private void Start()
    {
        wallJumpingTime = -1;
        respawnPoint = transform.position;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();    
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMotor>();
        GetComponent<Renderer>().enabled = true;
        originalParent = transform.parent;
        rb.gravityScale = gravityMod;
        topSpeed = topSpeed2;
    }

    private void Update()
    {
        Jump();
        WallJump();
        Flip();
        if (isGrounded())
        {
            GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("GrapplePoint");
            for (int i = 0; i < gameObjectsWithTag.Length; i++)
            {
                gameObjectsWithTag[i].SendMessage("PlayerGrounded");
            }
            hasWallJumped = false;
            accel = accelDefault;
            groundedTime = coyoteJump;
        }
        else
        {
            groundedTime -= Time.deltaTime;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        if (isTouchingRightWall() && horizontalInput > 0)
        {
            animator.SetBool("IsWallSlide", true);


        }
        else if (isTouchingLeftWall() && horizontalInput < 0)
        {
            animator.SetBool("IsWallSlide", true);
        }
        else
        {
            animator.SetBool("IsWallSlide", false);
        }
        animatorSettings();
    }


    private void animatorSettings()
    {
        animator.SetBool("FacingRight", isFacingRight);
        animator.SetBool("IsGrappling", grapplingRope.enabled);
        animator.SetBool("IsGrounded", isGrounded());

        if (Mathf.Abs(rb.velocity.x) > topSpeed2 / 5f)
        {
            animator.SetBool("isRunning", true);
            if (Mathf.Abs(rb.velocity.x) >= topSpeed2 - topSpeed2 / 50f)
            {
                animator.SetBool("IsSprinting", true);
                topSpeed = topSpeed3;
            }
            else
            {
                animator.SetBool("IsSprinting", false);
                topSpeed = topSpeed2;
            }
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
        if (rb.velocity.y > topSpeed2 / 40f)
        {
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsFalling", false);
        }
        else
        {
            animator.SetBool("IsJumping", false);
            if (rb.velocity.y < -topSpeed2 / 20f)
            {
                animator.SetBool("IsFalling", true);
            }
            else
            {
                animator.SetBool("IsFalling", false);
            }
        }
    }
    private void FixedUpdate()
    {



        if (!grapplingRope.isGrappling)
        {
            Movement();
        }

        float restoreRate = Time.deltaTime / gravRestoreTime; 

        if (rb.gravityScale < gravityMod)
        {
            rb.gravityScale += Mathf.Pow(Mathf.Abs(gravityMod - rb.gravityScale) * restoreRate, 0.5f); // Apply inverted exponential factor

            if (rb.gravityScale > gravityMod)
            {
                rb.gravityScale = gravityMod;
            }
        }
        else if (rb.gravityScale > gravityMod)
        {
            rb.gravityScale -= Mathf.Pow(Mathf.Abs(rb.gravityScale - gravityMod) * restoreRate, 0.5f); // Apply inverted exponential factor

            if (rb.gravityScale < gravityMod)
            {
                rb.gravityScale = gravityMod;
            }
        }

       

        Vector2 grapplingVelocity = grapplingGun.GrappleMovement(new Vector2(Input.GetAxis("Horizontal"), 0f));  //grappling component
        Vector2 curVelocity = new Vector2(rb.velocity.x, rb.velocity.y);

        if (lastGrapple != null && grapplingRope.isGrappling)
        {
            curVelocity = new Vector2(rb.velocity.x - lastGrapple.x, rb.velocity.y - lastGrapple.y); //prevents grapple rocketing maybe theres a better solution
        }

     
        rb.velocity = curVelocity; //add in velocity based on all 2 componenets

        if (grapplingRope.isGrappling)
        {
            rb.velocity += grapplingVelocity;
        }

        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed.x, maxSpeed.x), Mathf.Clamp(rb.velocity.y, -maxSpeed.y, maxSpeed.y)); //clamp based on max speed

        if (rb.velocity.sqrMagnitude < deadSpeed && grapplingRope.isGrappling)
        {

            curDeadTime++; //if slow and grapling increase dead time
        }
        if (curDeadTime > maxDeadTime)
        {
            grapplingGun.Disable(); //if dead time over limit, break rope...prevents just hanging out on ropes 
        }

        lastGrapple = grapplingVelocity; //resets last grapple velocity vector

        if (!canMove)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }

        wallSlide();


        if(wallJumpingTime > 0f && !grapplingRope.isGrappling)
        {
            float initialVelocity = 27f;

            // Exponential decay factor
            // Adjust 'decayRate' to control the speed of the decay. A smaller value (closer to 0) will decay slower.
            float decayRate = -0.18f; // You might need to adjust this based on the desired effect

            // Calculate current velocity using exponential decay based on the remaining wallJumpingTime
            float currentVelocity = initialVelocity * Mathf.Exp(decayRate * (wallJumpingDuration - wallJumpingTime));

            // Apply the calculated velocity
            rb.velocity = new Vector3(currentVelocity * -wallJumpingDirection * wallJumpingPower.x, currentVelocity * wallJumpingPower.y, 0f);

            // Decrease wallJumpingTime
            wallJumpingTime--;
        }

    }
    public bool isGrounded()
    {

        bool isGrounded = false;
        float distanceBetweenRays = width / (numberOfRays - 1);

        for (int i = 0; i < numberOfRays; i++)
        {
            Vector2 rayStart = groundCheckStartPoint.position - Vector3.right * (width / 2) + Vector3.right * (distanceBetweenRays * i);
            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, groundCheckDistance, groundLayer);

            if (hit.collider != null)
            {
                isGrounded = true;
                break;
            }
        }
        return isGrounded;
    }
    public bool isTouchingLeftWall()
    {
        bool isTouchingLeft = false;
        float distanceBetweenRays = height / (numberOfRays - 1);

        for (int i = 0; i < numberOfRays; i++)
        {
            Vector2 rayStart = rightCheckStartPoint.position - Vector3.up * (height / 2) + Vector3.up * (distanceBetweenRays * i);
            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.left, groundCheckDistance, groundLayer);

            if (hit.collider != null)
            {
                isTouchingLeft = true;
                break;
            }
        }
        return isTouchingLeft;
    }

    public bool isTouchingRightWall()
    {
        bool isTouchingRight = false;
        float distanceBetweenRays = height / (numberOfRays - 1);

        for (int i = 0; i < numberOfRays; i++)
        {
            Vector2 rayStart = rightCheckStartPoint.position - Vector3.up * (height / 2) + Vector3.up * (distanceBetweenRays * i);
            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.right, groundCheckDistance, groundLayer);

            if (hit.collider != null)
            {
                isTouchingRight = true;
                break;
            }
        }
        return isTouchingRight;
    }
    private void Movement()
    {
        Transform originalParent = transform.parent;
        transform.parent = null;


        float moveHorizontal = Input.GetAxis("Horizontal");

        Vector2 targetVelocity = new Vector2(moveHorizontal * topSpeed, rb.velocity.y);
        Vector2 velocityDiff = targetVelocity - rb.velocity;
        Vector2 force = velocityDiff.x * Vector3.right * accel;
        if (!(wallJumpingTime > 0f))
        {
            rb.AddForce(force, ForceMode2D.Force);
        }
        transform.parent = originalParent;
    }
    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && !grapplingRope.isGrappling) //can jump?
        {
            if (groundedTime > 0f && !wallSliding)
            {
                if (rb.velocity.y < 0f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpingSecondGravReduction);
                }
                rb.AddForce(new Vector2(0f, jumpingPower), ForceMode2D.Impulse);
                if (rb.gravityScale > gravityJumpMod)
                {
                    rb.gravityScale = gravityJumpMod;
                }
                Invoke("HighGrav", 0.35f);
            }
        }
    }
    public void HighGrav()
    {
        rb.gravityScale = gravityMod * 2.25f;
    }

    private void Flip()
    { //flips our sprite if needed
        float flipDeadzone = 0.5f;
        if ((isFacingRight && rb.velocity.x < -flipDeadzone) || (!isFacingRight && rb.velocity.x > flipDeadzone))
        {
            FlipCharacter();
        }
    }
    private void FlipCharacter()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
        Vector3 localScaleGun = transform.localScale;
        localScaleGun.x *= -1f;
        gunPivot.localScale = localScaleGun;
    }
    public void doubleJump()
    {
        {
            {
                if (rb.velocity.y < 0f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpingSecondGravReduction);
                }
                if (rb.gravityScale > gravityJumpModSec)
                {
                    rb.gravityScale = gravityJumpModSec;
                }
                rb.AddForce(new Vector2(0f, jumpingPowerSecond), ForceMode2D.Impulse); //jump is not using tranform or rb velocity, but rather a force impulse
            }
        }
    }

    public void die()
    {
        grapplingRope.isGrappling = false;
        grapplingRope.enabled = false;
        lastGrapple = new Vector2(0,0);



        wallJumpingTime =0f;
        wallSlideTime = 0f;

  
        GetComponent<Renderer>().enabled = false;
  
        
        Invoke("respawn", respawnTime);

        canMove = false;

    }
    public void respawn()
    {
        canMove = true;
        GetComponent<Renderer>().enabled = true;
        enabled = true;
        lastGrapple = new Vector2(0, 0);
        forceMovement(respawnPoint);
        cam.addDest(respawnPoint);
        rb.velocity = new Vector3(0, 0, 0);
    }
    public void forceMovement(Vector2 snap)
    {
        transform.position = snap;
        if (grapplingRope.isGrappling)
        {
            grapplingRope.OnDisable();
            grapplingRope.enabled = false; //??
        }
    }
    private void wallSlide()
    {

        if(wallSlideTime > 1.5f)
        {
            wallSlideSpeed = wallSlideSpee2d;

        }
        else
        {
            wallSlideSpeed = wallSlideSpeed1;
        }

        if (isTouchingRightWall())
        {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(rb.velocity.y, wallSlideSpeed, float.MaxValue), 0);
            wallSliding = true;
            wallSlideTime += Time.deltaTime;
            right2 = isFacingRight ? 1 : -1;
        }
        else
        {
            wallSliding = false;
            wallSlideTime = 0f;
        }

    }


    private void WallJump()
    {


        if (wallSliding)
        {
            wallJumpingCounter = walljumpCoyoteTime;
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && !grapplingRope.isGrappling && wallJumpingCounter > 0f)
        {
            // Check if the player hasn't wall jumped yet, or if they're jumping off the opposite wall
            if ((wallJumpingCounter > 0 && right2 != wallJumpingDirection) || !hasWallJumped)
            {
                wallJumpingCounter = 0f;
                wallJumpingDirection = right2;
                wallJumpingTime = wallJumpingDuration;
                rb.gravityScale = gravityJumpMod;
                hasWallJumped = true;
                accel = accelWallJump;
            }
        }
    }


    public void setParent(Transform newParent)
    {
        originalParent = transform.parent;
        transform.parent = newParent;
    }
    public void resetParent()
    {
        transform.parent = originalParent;
    }
    public void addForce(Vector2 vel)
    {
        rb.AddForce(vel, ForceMode2D.Impulse);
    }


    public void setRespawnPoint(Vector2 pos)
    {
        respawnPoint = pos;
    }
}