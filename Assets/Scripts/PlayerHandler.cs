using UnityEngine;

public class PlayerHandler : MonoBehaviour
{



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


    private Vector2 lastGrapple;
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Jump();
        Flip();
       /* GrapplingControl();*/
    }

    private bool IsGrounded(){
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    private void FixedUpdate()
    {


        

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

        Debug.Log(rb.velocity.sqrMagnitude);

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
        if(Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.AddForce(new Vector2(0f, jumpingPower), ForceMode2D.Impulse);
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
            grapplingGun.curMaxDistance -= 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            grapplingGun.curMaxDistance += 0.5f;
        }
    }
}
