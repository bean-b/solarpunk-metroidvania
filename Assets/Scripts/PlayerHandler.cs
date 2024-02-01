using UnityEngine;

public class PlayerHandler : MonoBehaviour
{

    private int jumpAvailable = 0;
    [HideInInspector] public int grappleAvailable = 0;
    private float lastGrounded;
    [SerializeField] private float groundDelay;

    [HideInInspector]public float graplingLengthMod = 0f;

    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 25f;
    private bool isFacingRight = true;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform gunPivot;
    [SerializeField] private GrapplingRope grapplingRope;
    [SerializeField] private GrapplingGun grapplingGun;
    [SerializeField] private float graplingSpeedBonus;


    public float maxDeadTime;
    public float curDeadTime = 0;
    [SerializeField] private float deadSpeed;
    [SerializeField] private Vector2 maxSpeed;
    [SerializeField] private float grapplePullSpeed;

    private Vector2 lastGrapple;
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame

    private void Update()
    {
        Jump();
        Movement();
        Flip();
  /*      GrapplingControl();*/
        IsGrounded();
    }
    private void IsGrounded(){
        if(Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer) && Time.time - lastGrounded > groundDelay)
        {
            jumpAvailable = 1;
            grappleAvailable = 1;
            grappleAvailable = 1;
            lastGrounded = Time.time;
        }
        
    }
    private void FixedUpdate()
    {

        grapplingGun.curMaxDistance += graplingLengthMod;

        


        float curSpeed = 1f * speed;
        if (grapplingRope.isGrappling)
        {
            curSpeed *= graplingSpeedBonus;
        }
        Vector2 playerVelocity = new Vector2(horizontal * curSpeed, 0f);
        Vector2 graplingVelocity = grapplingGun.GrappleMovement(playerVelocity);
        Vector2 curVelocity = new Vector2(0f, rb.velocity.y);
        
        
        if (lastGrapple!=null && grapplingRope.isGrappling)
        {
            curVelocity = new Vector2(0f, rb.velocity.y - lastGrapple.y);
        }


        rb.velocity = graplingVelocity + playerVelocity + curVelocity;


        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed.x, maxSpeed.x), Mathf.Clamp(rb.velocity.y, -maxSpeed.y, maxSpeed.y));
        
        if(rb.velocity.sqrMagnitude < deadSpeed && grapplingRope.isGrappling) {
            curDeadTime++;
        }
        if(curDeadTime > maxDeadTime) {
            grapplingGun.Disable();
        }

        lastGrapple = graplingVelocity;
        
    }

    private void Movement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
    }
    private void Jump(){



        if(Input.GetButtonDown("Jump") && jumpAvailable > 0)
        {
            rb.AddForce(new Vector2(0f, jumpingPower), ForceMode2D.Impulse);
            jumpAvailable--;
            

        }
    }

    private void Flip(){
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

    private void GrapplingControl()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            graplingLengthMod = -grapplePullSpeed;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            graplingLengthMod = grapplePullSpeed;
        }
        if(Input.GetKeyUp(KeyCode.S))
        {
            graplingLengthMod = 0f;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            graplingLengthMod = 0f;
        }


        grapplingGun.curMaxDistance += graplingLengthMod;
    }
}
