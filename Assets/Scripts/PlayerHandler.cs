using UnityEngine;

public class PlayerHandler : MonoBehaviour
{

    [HideInInspector] public int grappleAvailable = 0; //this controls how many graples per time touching the ground

    [SerializeField] private float topSpeed;
    private float horizontalInput; //horizontal movement 
    private float horizontalMod = 0; //horizontal movement 
    private float horizontalSpeed = 0; //horizontal movement 
    [SerializeField] private float accel; //speed mod
    [SerializeField]  private float jumpingPower; //jump power
    
    private bool isFacingRight = true; //which direction facing
    [SerializeField] private float graplingSpeedBonus; //the amount our speed increases when we grapple. Essentialy the grapple boost %. 1.5 = 50% boost etc


    public Rigidbody2D rb; //our rigid body
    public Transform groundCheck; //this is used to check if we are touching the ground, essently a circle below our feet  ||| can be put as getComponenet later
    public LayerMask groundLayer; //this layermask controls what counts as 'ground', add more layers for dif types of ground etc..
    [SerializeField] private Transform gunPivot; //where the gun pivots from. currently set from the center of the player for gameplay reasons, can add sprites later though 
    [SerializeField] private GrapplingRope grapplingRope; // our grappling rope
    [SerializeField] private GrapplingGun grapplingGun; //our grappling gun 

    public float maxDeadTime; // max time we can be slow while grapling before it breaks
    public float curDeadTime = 0; //how long weve been slow 
    public float deadSpeed; // minimum speed to not be considered slow grapling
    public Vector2 maxSpeed; //overall max possible speed, janky solution to rocketing around places lol
    

    private Vector2 lastGrapple; //a vector2 of the speed of the last frame that the grapple contributed to our rigid body, prevents the grappling hook from rocketing the player


    private void Update()
    {
        Jump();
        Movement();
        Flip();
        IsGrounded();
    }
    private void IsGrounded(){
        //update lastgrounded and jump availible if grounded and delay true
        if(Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer))
        {
            grappleAvailable = 1;
        }
        
    }
    private void FixedUpdate()
    {
        Vector2 playerVelocity = new Vector2(horizontalSpeed, 0f); //player control componenet
        Vector2 graplingVelocity = grapplingGun.GrappleMovement(playerVelocity);  //grappling component
        Vector2 curVelocity = new Vector2(0f, rb.velocity.y); //gravity/jumping component
        
        if (lastGrapple!=null && grapplingRope.isGrappling)
        {
            curVelocity = new Vector2(0f, rb.velocity.y - lastGrapple.y); //prevents grapple rocketing maybe theres a better solution
        }


        rb.velocity = graplingVelocity + playerVelocity + curVelocity; //add in velocity based on all 3 componenets


        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed.x, maxSpeed.x), Mathf.Clamp(rb.velocity.y, -maxSpeed.y, maxSpeed.y)); //clamp based on max speed
        
        if(rb.velocity.sqrMagnitude < deadSpeed && grapplingRope.isGrappling) {
            curDeadTime++; //if slow and grapling increase dead time
        }
        if(curDeadTime > maxDeadTime) {
            grapplingGun.Disable(); //if dead time over limit, break rope...prevents just hanging out on ropes 
        }

        lastGrapple = graplingVelocity; //resets last grapple velocity vector
        
    }

    private void Movement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");//using unity input system, left right (a,d) = horiziontal velocity

        if(horizontalInput == 0f)
        {
            horizontalMod *= 0.8f;
        }

        if(horizontalMod  < 0 && horizontalInput > 0 || horizontalMod > 0 && horizontalInput < 0)
        {
            horizontalMod *= 0.8f;
        }

      
        horizontalMod += horizontalInput * accel;

        float adjustedAccel = Mathf.Abs(horizontalMod); 
        

        float speedBonus = (Mathf.Log(1f + Mathf.Abs(horizontalMod), 5f + adjustedAccel) * topSpeed); 

        if (horizontalMod > 0)
            {
                horizontalSpeed = speedBonus;
            }
            else
            {
                horizontalSpeed = -speedBonus;
            }


    }
    private void Jump(){
        if(Input.GetButtonDown("Jump") && Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer)) //can jump?
        {
            rb.AddForce(new Vector2(0f, jumpingPower), ForceMode2D.Impulse); //jump is not using tranform or rb velocity, but rather a force impulse
        }
    }

    private void Flip(){//flips our sprite if needed
        if (isFacingRight && rb.velocity.x < 0f || !isFacingRight && rb.velocity.x > 0f){ 
            isFacingRight = !isFacingRight;
            Vector3 localScale =  transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
            Vector3 localScaleGun = transform.localScale;
            localScaleGun.x *= -1f;
            gunPivot.localScale = localScaleGun;
        } 
        
    }

}
