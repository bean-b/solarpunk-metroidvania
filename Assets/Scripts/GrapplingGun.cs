using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GrapplingGun : MonoBehaviour
{


    [SerializeField] private PlayerHandler playerHandler;  
    public GrapplingRope grappleRope;

    [SerializeField] private int grappableLayerNumber; //what we can grapple too, todo should be a layermask to allow for more types of grapple on dif surfances. allows us easily to only grapple to plants

    public Camera m_camera; 

    public Transform gunHolder; // our players tranform
    public Transform gunPivot; //where the gun turns around
    public Transform firePoint;//where we shoot from

    public Rigidbody2D m_rigidbody; //player rigid body


    public const float maxDistnace = 10f; //max disstance our rope can shoot
    public const float minDistnace = 2f; //min distnace (not implemented atm execpt for unimplented rope pulling)
    [HideInInspector] public float curMaxDistance = 10f; // rope distance is calucating when anchoring this prevents rope from sagging out to maxdistance always
    [SerializeField] private float grappleSwingDeduction; //not implemented (and prob shouldnt be)! basicly slowly decreases rope when we are swinging an attempt at swingier movement

    [SerializeField] private float swingForce; //how much swing we get
    [SerializeField] private float tensionForce; //our much the rope keeps us in an arc
    [SerializeField] private float dampiningForce; //how much the rope loses momentum
    [SerializeField] private float swingRatio; // x vs y ratio of swing velocity
    [SerializeField] private int angleImpactReduction; //minimizes effect of anglular momentum higher = less momemntun



    private Vector2 swingDir = Vector2.zero; //keeps track of swinging direction
    private int layerMask; //numbs in this layer mask are ingored. right now just includes player. means that you can shoot through objects in this type


    [HideInInspector] public Vector2 grapplePoint; //where we are grappling too
    [HideInInspector] public Vector2 grappleDistanceVector;

    private void Start()
    {
        layerMask = ~LayerMask.GetMask("Player");
        grappleRope.enabled = false;
        curMaxDistance = maxDistnace;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && playerHandler.grappleAvailable > 0)
        {
            
            SetGrapplePoint(); 

        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            if (grappleRope.enabled)
            {
                RotateGun(grapplePoint, false); 
            }
            else
            {
                Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
                RotateGun(mousePos, true); 
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Disable();
        }
        else
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            RotateGun(mousePos, true);
        }

    }

    private void FixedUpdate()
    {
        //some jank shit
/*        if(grappleRope.isGrappling && playerHandler.rb.velocity.sqrMagnitude > playerHandler.deadSpeed)
        {
            curMaxDistance -= grappleSwingDeduction;
        }*/
    }

    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime) //gun point @ mouse
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void SetGrapplePoint() //set grapple point to nearest object that we can grapple too
    {
        Vector2 distanceVector = m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position;
        if (Physics2D.Raycast(firePoint.position, distanceVector.normalized, maxDistnace, layerMask))
        {
            RaycastHit2D _hit = Physics2D.Raycast(firePoint.position, distanceVector.normalized, maxDistnace, layerMask);
            if (_hit.transform.gameObject.layer == grappableLayerNumber)
            {
                if (Vector2.Distance(_hit.point, firePoint.position) <= maxDistnace)
                {
                    grapplePoint = _hit.point;
                    grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
                    grappleRope.enabled = true;
                    curMaxDistance = Vector2.Distance(grapplePoint, m_rigidbody.position);

                    
                }
            }
        }
    }

    public Vector2 GrappleMovement(Vector2 playerVelocity) //the meat of the grappling velocity
    {
        

        Vector2 toReturn = Vector2.zero; //vector returned to playerhandler for caluations

        float curDistance = Vector2.Distance(grapplePoint, m_rigidbody.position); // distnace from player to grapple point
        Vector2 ropeDirection = grapplePoint - m_rigidbody.position; //rope direction
        ropeDirection = ropeDirection.normalized; 
        float ropeAngle = Vector2.Angle(ropeDirection, Vector2.up); //angle of rope vs cardinal up

        if (curDistance > curMaxDistance && grappleRope.enabled) // this is the tension force, pulling you back into the arc
        {


            float springForceMagnitude = (curDistance - curMaxDistance) * tensionForce;
            Vector2 springForce = ropeDirection * springForceMagnitude;
            Vector2 dampingForce = m_rigidbody.velocity * dampiningForce;

            toReturn = springForce + dampingForce;
        }






        if (grappleRope.isGrappling) //this is the swing force, giving you a swing
        {
            if (m_rigidbody.position.x > grapplePoint.x && playerVelocity.x < 0)
            {
                swingDir = swingDir - new Vector2((swingForce * swingRatio)* (ropeAngle+ angleImpactReduction) / angleImpactReduction, 0f);

            }
            else if (m_rigidbody.position.x > grapplePoint.x && playerVelocity.x > 0)
            {
                swingDir = swingDir + new Vector2((swingForce * swingRatio) * (ropeAngle + angleImpactReduction) / angleImpactReduction, 0f);
            }


            if (m_rigidbody.position.y > grapplePoint.y - curMaxDistance)
            {
                swingDir = swingDir - new Vector2(0f, swingForce * (1 - swingRatio) * (ropeAngle + angleImpactReduction) / angleImpactReduction);

            }
            else
            {
                swingDir = swingDir + new Vector2(0f, swingForce * (1 - swingRatio) * (ropeAngle + angleImpactReduction) / angleImpactReduction);
            }

        }

        return toReturn + swingDir;

    }




    public void Disable() //when we stop grappling
    {
        grappleRope.enabled = false;
        swingDir = Vector2.zero;
        curMaxDistance = maxDistnace;
        playerHandler.curDeadTime = 0;
        playerHandler.graplingLengthMod = 0f;
    }

    private void OnDrawGizmosSelected() //this is just used to show max distance in scene editor, not for gameplay
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistnace);
        }
    }

}
