using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class PlayerHandler : MonoBehaviour
{
    private const int TimesCanJump = 1;



    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 20f;
    private int numJumps = TimesCanJump;
    private bool isFacingRight = true;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform gunPivot;
    [SerializeField] private GrapplingRope grapplingRope;

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
    }

    private bool IsGrounded(){
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal* speed, rb.velocity.y);
    }

    private void Movement()
    {
        if (!grapplingRope.isGrappling)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }
    }
    private void Jump(){
        if(Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            numJumps--;
        }
        if(Input.GetButtonDown("Jump") && rb.velocity.y > 0f){
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y *0.5f);
        }
    }

    private void Flip(){
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f){
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
