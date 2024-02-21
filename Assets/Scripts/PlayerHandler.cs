using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{


    [SerializeField] private float topSpeed;

    [SerializeField] private float accel; //speed mod
    [SerializeField]  private float jumpingPower; //jump power
    [SerializeField]  private float jumpingPowerSecond; //jump power

    [SerializeField] private float fastFallMod;
    [SerializeField] private float jumpingSecondGravReduction;

    public float gravityMod;
    [SerializeField] private float gravityJumpMod;
    [SerializeField] private float gravRestoreTime; // higher  = slower

    private bool isFacingRight = true; //which direction facing



    public Rigidbody2D rb; //our rigid body

    public float groundCheckDistance = 1.5f; // How far down we check for ground
    public int numberOfRays = 5; // Number of rays to cast
    public float width = 3f;
    public float height = 1f;


    public Transform leftCheckStartPoint;
    public Transform rightCheckStartPoint;
    public Transform groundCheckStartPoint; //this is used to check if we are touching the ground, essently a circle below our feet  ||| can be put as getComponenet later
    public LayerMask groundLayer; //this layermask controls what counts as 'ground', add more layers for dif types of ground etc..
    [SerializeField] private Transform gunPivot; //where the gun pivots from. currently set from the center of the player for gameplay reasons, can add sprites later though 
    [SerializeField] private GrapplingRope grapplingRope; // our grappling rope
    [SerializeField] private GrapplingGun grapplingGun; //our grappling gun 

    public float maxDeadTime; // max time we can be slow while grapling before it breaks
    public float curDeadTime = 0; //how long weve been slow 
    public float deadSpeed; // minimum speed to not be considered slow grapling
    public Vector2 maxSpeed; //overall max possible speed, janky solution to rocketing around places lol

    private Transform originalParent;
    
    


    private Vector2 lastGrapple; //a vector2 of the speed of the last frame that the grapple contributed to our rigid body, prevents the grappling hook from rocketing the player
    private Vector2 lastMovement;
    private void Start()
    {
        originalParent = transform.parent;
        rb.gravityScale = gravityMod;

    }

    private void Update()
    {
        Jump();
        Flip();
        if (isGrounded())
        {
            GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("GrapplePoint");
            for (int i = 0; i < gameObjectsWithTag.Length; i++)
            {
                gameObjectsWithTag[i].SendMessage("PlayerGrounded");
            }
        }
   

       



    }
    private void FixedUpdate()
    {
        /*     wallSlide();*/
        if (!grapplingRope.isGrappling)
        {
            Movement();
        }

        if (rb.gravityScale < gravityMod)
        {

            rb.gravityScale += ((gravityMod - rb.gravityScale) / gravRestoreTime) + 0.01f;
            if(rb.gravityScale > gravityMod)
            {
                rb.gravityScale = gravityMod;
            }

        }else if (rb.gravityScale > gravityMod)
        {
            rb.gravityScale -= ((gravityMod - rb.gravityScale / gravRestoreTime)) + 0.01f;
            if(rb.gravityScale < gravityMod)
            {
                rb.gravityScale = gravityMod;
            }
        }



        Vector2 grapplingVelocity = grapplingGun.GrappleMovement(new Vector2(Input.GetAxis("Horizontal"), 0f));  //grappling component
        Vector2 curVelocity = new Vector2(rb.velocity.x, rb.velocity.y); 
        
        if (lastGrapple!=null && grapplingRope.isGrappling)
        {
            curVelocity = new Vector2(rb.velocity.x - lastGrapple.x, rb.velocity.y - lastGrapple.y); //prevents grapple rocketing maybe theres a better solution
        }


        curVelocity = new Vector2(curVelocity.x, curVelocity.y);

        rb.velocity = grapplingVelocity + curVelocity; //add in velocity based on all 2 componenets


        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed.x, maxSpeed.x), Mathf.Clamp(rb.velocity.y, -maxSpeed.y, maxSpeed.y)); //clamp based on max speed
        
        if(rb.velocity.sqrMagnitude < deadSpeed && grapplingRope.isGrappling) {
            curDeadTime++; //if slow and grapling increase dead time
        }
        if(curDeadTime > maxDeadTime) {
            grapplingGun.Disable(); //if dead time over limit, break rope...prevents just hanging out on ropes 
            rb.gravityScale = gravityMod * fastFallMod;
            rb.velocity = new Vector2(rb.velocity.x*(1/fastFallMod), rb.velocity.y);
        }

        lastGrapple = grapplingVelocity; //resets last grapple velocity vector


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
                // Optionally, handle ground angle here based on hit.normal
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
            Vector2 rayStart = leftCheckStartPoint.position - Vector3.up * (height / 2) + Vector3.up * (distanceBetweenRays * i);
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

        rb.AddForce(force, ForceMode2D.Force);



        transform.parent = originalParent;

      

    }
    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && !grapplingRope.isGrappling) //can jump?
        {
            if (isGrounded())
            {
                rb.AddForce(new Vector2(0f, jumpingPower), ForceMode2D.Impulse); //jump is not using tranform or rb velocity, but rather a force impulse
                if (rb.gravityScale > gravityJumpMod)
                {
                    rb.gravityScale = gravityJumpMod;
                }
            }
        }
    }


            private void Flip(){ //flips our sprite if needed
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
                if (rb.gravityScale > gravityJumpMod)
                {
                    rb.gravityScale = gravityJumpMod;
                }
                rb.AddForce(new Vector2(0f, jumpingPowerSecond), ForceMode2D.Impulse); //jump is not using tranform or rb velocity, but rather a force impulse
            }
        }
    }

    public void die()
    {
        transform.position = new Vector3 (-11f, -4.8f, 0f);
        rb.velocity = Vector3.zero;
        if(grapplingRope.isGrappling)
        {
            grapplingRope.OnDisable();
        }
    }


   /* private void wallSlide()
    {
        if(isTouchingRightWall() && horizontalInput > 0 && !isGrounded())
        {
            rb.AddForce(new Vector2(0f, wallSlideForceCur), ForceMode2D.Impulse);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, 0));
            wallSlideForceCur -= (wallSlideForceCur / wallSlideTime);
            wallSlideForceCur = Mathf.Max(wallSlideForceCur, 0);


            if (Input.GetButtonDown("Jump"))
            {
                rb.AddForce(new Vector2(0, jumpingPower), ForceMode2D.Impulse);
            *//*    wallSlideJumpDir = -1;
                timeLastWallSLideJumped = Time.time;*//*
            }

        }
        else if(isTouchingLeftWall() && horizontalInput < 0 && !isGrounded() ) {

            rb.AddForce(new Vector2(0f, wallSlideForceCur), ForceMode2D.Impulse);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, 0));
            wallSlideForceCur -= (wallSlideForceCur / wallSlideTime);
            wallSlideForceCur = Mathf.Max(wallSlideForceCur, 0);

            if (Input.GetButtonDown("Jump"))
            {
                rb.AddForce(new Vector2(0, jumpingPower), ForceMode2D.Impulse);
  *//*              wallSlideJumpDir = 1;
                timeLastWallSLideJumped = Time.time;*//*
            }

        }
        else
        {
            wallSlideForceCur += (wallSlideForce / wallSlideTime);
            wallSlideForceCur = Mathf.Min(wallSlideForceCur, wallSlideForce);
        }
    }
*/
    public void setParent(Transform newParent) {
        originalParent = transform.parent;
        transform.parent = newParent;
    }

    public void resetParent() {
        transform.parent = originalParent;
    }
    public void addVelocity(Vector2 vel)
    {
        rb.velocity += vel;
    }
}

