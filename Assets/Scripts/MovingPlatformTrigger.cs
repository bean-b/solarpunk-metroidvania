using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformTrigger : MonoBehaviour
{
    [SerializeField] private float speed;
  
    public Transform targetWaypoint;

    private bool moving = false;

    private Vector2 lastPos;


    // Start is called before the first frame update
    void Start()
    {
        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        lastPos = transform.position;
        if (moving)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetWaypoint.position,
                speed * Time.deltaTime);
            print(Vector3.Distance(this.transform.position, targetWaypoint.transform.position));
            if (Vector3.Distance(this.transform.position, targetWaypoint.transform.position) < 0.05f)
            {
                moving = false;
            }
        }
       
    }

    private void Unlocked()
    {
        moving = true;
    }

   
    private void OnCollisionEnter2D(Collision2D other)
    { // player standing on platform
        var playerHandler = other.collider.GetComponent<PlayerHandler>();
        if (playerHandler != null)
        {
            if (playerHandler.isGrounded() && transform.position.y < playerHandler.transform.position.y)
            {
                playerHandler.setParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    { // player leaving platform
        var playerHandler = other.collider.GetComponent<PlayerHandler>();
        if (playerHandler != null)
        {
            playerHandler.resetParent();

            /*  Vector2 velocity = new Vector2(transform.position.x, transform.position.y) - lastPos;

              if (velocity.y < 0)
              {
                  playerHandler.addForce(new Vector2(velocity.x, 0f));
              }
              else
              {
                  playerHandler.addForce(velocity);
              }*/


        }
    }
}
