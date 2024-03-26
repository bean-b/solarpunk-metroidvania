using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    [HideInInspector] private float topSpeed3 = 26; //sprinting
    private float topSpeed;
    [HideInInspector] private float topSpeed2 = 18; //running
    [HideInInspector] private float accel = 130; //speed mod
    [HideInInspector] private float jumpingPower = 62; //jump power
    [HideInInspector] private float jumpingPowerSecond = 30; //jump power
    [HideInInspector] private float fastFallMod = 1.5F;
    [HideInInspector] private float jumpingSecondGravReduction =0.05f;
    [HideInInspector] public float gravityMod = 6f;
    [HideInInspector] private float gravityJumpMod = 1.25f;
    [HideInInspector] private float gravityJumpModSec = 2f;
    [HideInInspector] private float gravRestoreTime = 0.25F; 
    [HideInInspector] private float gravRestoreTarget; 
    private bool isFacingRight = true; //which direction facing
    [HideInInspector] public float wallSlideSpeed = 0;
    [HideInInspector] public float wallSlideSpeedactual = 0;
    [HideInInspector] public Rigidbody2D rb; //our rigid body
    [HideInInspector] public float groundCheckDistance = 2.5f; // How far down we check for ground
    [HideInInspector] public int numberOfRays = 10; // Number of rays to cast
    [HideInInspector] public float width = 1f;
    [HideInInspector] public float height = 1f;
    [HideInInspector] public float respawnTime = 1f;

    private float dampening = 0.825f;

    [HideInInspector] public float wallJumpDir = 0;
    [HideInInspector] public Vector2 lastWallJump = new Vector2(0,0);
    [HideInInspector] public float shouldWallJump = 0;
    private float wallJumpDur = 10f;
    public Transform leftCheckStartPoint;
    public Transform rightCheckStartPoint;
    public Transform groundCheckStartPoint; //this is used to check if we are touching the ground, essently a circle below our feet  ||| can be put as getComponenet later
    public LayerMask groundLayer; //this layermask controls what counts as 'ground', add more layers for dif types of ground etc..
    [SerializeField] private Transform gunPivot; //where the gun pivots from. currently set from the center of the player for gameplay reasons, can add sprites later though 
    [SerializeField] public GrapplingRope grapplingRope; // our grappling rope
    [SerializeField] private GrapplingGun grapplingGun; //our grappling gun 
    [HideInInspector] public float maxDeadTime = 15; // max time we can be slow while grapling before it breaks
    [HideInInspector] public float curDeadTime = 0; //how long weve been slow 

    [HideInInspector] public float deadSpeed = 18; // minimum speed to not be considered slow grapling
    [HideInInspector] public Vector2 maxSpeed = new Vector2(50,50); //overall max possible speed
    
    private Transform originalParent;

    [HideInInspector] public float wallSlidingTime = -100f;
    [HideInInspector] public float wallSlideDelay = 0.25f;
    
    private float wallSlideJumpTime = -100f;
    
    private Animator animator;

    public Vector2 respawnPoint;

    private Vector2 lastGrapple; //a vector2 of the speed of the last frame that the grapple contributed to our rigid body, prevents the grappling hook from rocketing the player

    private float wallSlideDuration = 0f;

    private CameraMotor cam;


    private Vector2 wallJumpVecTarget = new Vector2(1.6f, 2.4f);
    private Vector2 wallJumpVecActual;
    private Vector2 wallJumpDecay = new Vector2(0f, 0.2f);
    private Vector2 wallJumpRestore = new Vector2(0f, 0.02f);
    private void Start()
    {
        gravRestoreTarget = gravRestoreTime;
        wallJumpVecActual = wallJumpVecTarget;
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
        wallJump();
        Jump();
        Flip();
        if (isGrounded())
        {
            GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("GrapplePoint");
            for (int i = 0; i < gameObjectsWithTag.Length; i++)
            {
                gameObjectsWithTag[i].SendMessage("PlayerGrounded");
            }
            wallSlideJumpTime = -100f;
            wallSlideDuration = 0f;
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

        if (Mathf.Abs(rb.velocity.x) > topSpeed2 / 10f)
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

        int mod = isGrounded() && !isTouchingLeftWall() && !isTouchingRightWall() ? 5 : 1;
        
        if(wallJumpVecActual.x < wallJumpVecTarget.x)
        {
            wallJumpVecActual.x += wallJumpRestore.x * mod;
        }

        if(wallJumpVecActual.y < wallJumpVecTarget.y){
                wallJumpVecActual.y += wallJumpRestore.y * mod;
        }

        if (!grapplingRope.isGrappling && !(wallSlideJumpTime + wallSlideDelay * 2f > Time.time))
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

        if (!(wallSlidingTime + wallSlideDelay > Time.time))
        {
            if (wallSlideSpeedactual > wallSlideSpeed)
            {
                wallSlideSpeedactual -= (wallSlideSpeedactual - wallSlideSpeed) / gravRestoreTime + 0.01f;
                if (wallSlideSpeedactual < wallSlideSpeed)
                {
                    wallSlideSpeedactual = wallSlideSpeed;
                }
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
            rb.gravityScale = gravityMod * fastFallMod;
            rb.velocity = new Vector2(rb.velocity.x * (1 / fastFallMod), rb.velocity.y);
        }

        lastGrapple = grapplingVelocity; //resets last grapple velocity vector
        wallSlide();
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
        if (shouldWallJump == 0f)
        {
            rb.AddForce(force, ForceMode2D.Force);
        }

        transform.parent = originalParent;
    }
    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && !grapplingRope.isGrappling) //can jump?
        {
            if (isGrounded() && !(wallSlidingTime + wallSlideDelay > Time.time))
            {
                if (rb.velocity.y < 0f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpingSecondGravReduction);
                }
                rb.AddForce(new Vector2(0f, jumpingPower), ForceMode2D.Impulse); //jump is not using tranform or rb velocity, but rather a force impulse
                if (rb.gravityScale > gravityJumpMod)
                {
                    rb.gravityScale = gravityJumpMod;
                }
            }
        }
    }

    private void Flip()
    { //flips our sprite if needed
        float flipDeadzone = 0.1f;
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

  
        GetComponent<Renderer>().enabled = false;
  
        forceMovement(respawnPoint);
        rb.velocity = Vector3.zero;
        Invoke("respawn", respawnTime);

        cam.addDest(respawnPoint);
    }
    public void respawn()
    {
        cam.clearDest();
        GetComponent<Renderer>().enabled = true;
        enabled = true;
        lastGrapple = new Vector2(0, 0);
        wallJumpVecActual = wallJumpVecTarget;
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
        float horizontalInput = Input.GetAxis("Horizontal");

        if (shouldWallJump > 0f)
        {

            float elapsedTimeRatio = 1f - (shouldWallJump / 10f);
            float currentSpeedBonus = Mathf.Lerp(jumpingPower, 0, elapsedTimeRatio);

            Vector2 wallJump = new Vector2(currentSpeedBonus * wallJumpDir, currentSpeedBonus);
            wallJump = wallJump * wallJumpVecActual;

            wallJumpVecActual = wallJumpVecActual - wallJumpDecay;
            rb.velocity = rb.velocity + wallJump;

         
            if (lastWallJump != null)
            {
                lastWallJump = new Vector2(lastWallJump.x, lastWallJump.y * dampening);
                rb.velocity -= lastWallJump;

            }
            lastWallJump = wallJump;
            shouldWallJump--;

        }
        else
        {
            lastWallJump = new Vector2(0f,0f);
            gravRestoreTime = gravRestoreTarget;
        }


        if (isTouchingRightWall() && horizontalInput > 0)
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeedactual);
            }
            wallSlidingTime = Time.time;
            wallSlideDuration++;

            if (wallSlideSpeedactual < gravityMod * 4f)
            {   
                wallSlideSpeedactual += 0.1f;
            }




        }
        else if (isTouchingLeftWall() && horizontalInput < 0)
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeedactual);
            }
            wallSlidingTime = Time.time;
            wallSlideDuration++;

            if (wallSlideSpeedactual < gravityMod * 4f)
            {
                wallSlideSpeedactual += 0.1f;
            }

        }
        else
        {
            wallSlideDuration -= 2;
            if (wallSlideDuration < 0)
            {
                wallSlideDuration = 0;
            }
        }
    }

    public void wallJump()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (isTouchingRightWall() && horizontalInput > 0 || isTouchingLeftWall() && horizontalInput < 0)
        {
            if (Input.GetButtonDown("Jump"))
            {
                shouldWallJump = wallJumpDur;
                rb.gravityScale = gravityJumpMod;
            }

            if (isTouchingRightWall())
            {
                wallJumpDir = -1;
            }
            else
            {
                wallJumpDir = 1;
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