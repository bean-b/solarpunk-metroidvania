using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GrapplingGun : MonoBehaviour
{

    public bool GrappleTarget = false;


    private PlayerHandler playerHandler;  
    public GrapplingRope grappleRope;

    public Camera m_camera; 

    

    public Transform gunHolder; // our players tranform
    public Transform gunPivot; //where the gun turns around
    public Transform firePoint;//where we shoot from

    public Rigidbody2D m_rigidbody; //player rigid body


    [HideInInspector]  public float maxDistnace = 21f; //max disstance our rope can shoot
    [HideInInspector] public float curMaxDistance; // rope distance is calucating when anchoring this prevents rope from sagging out to maxdistance always

    [SerializeField] private float swingForce; //how much swing we
    [SerializeField] private float swingForceTwo; //how much swing we get
    [SerializeField] private float tensionForce; //our much the rope keeps us in an arc
    [SerializeField] private float dampiningForce; //how much the rope loses momentum
    [SerializeField] private int angleImpactReduction; //minimizes effect of anglular momentum higher = less momemntun
    [SerializeField] private int lengthImpactReduction;



    [SerializeField] private float swingAccel;


    private Vector2 swingDir = Vector2.zero; //keeps track of swinging direction
    private int layerMask; //numbs in this layer mask are ingored. right now just includes player. means that you can shoot through objects in this type


    [HideInInspector] public Vector2 grapplePoint; //where we are grappling too
    [HideInInspector] public Vector2 grappleDistanceVector;
    [HideInInspector]  public GameObject grapplePointObj;


    private GrapplePoint[] grapplePoints;
    private GameObject[] grapplePointsOBJ;

    private void Start()
    {
        playerHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHandler>();
        layerMask = ~LayerMask.GetMask("Player");
        grappleRope.enabled = false;
        curMaxDistance = maxDistnace;
        grapplePointObj = null;
        grapplePointsOBJ = GameObject.FindGameObjectsWithTag("GrapplePoint"); //this should use quadtrees for preformance if that becoms an issue

        grapplePoints = new GrapplePoint[grapplePointsOBJ.Length];
        int i = 0;
        foreach (GameObject go in grapplePointsOBJ)
        {
            grapplePoints[i] = go.GetComponent<GrapplePoint>();
            i = i + 1;
        }

    }


    private void Update()
    {
        findGrapplePoints();

        if (!grappleRope.enabled && Input.GetKeyDown(KeyCode.LeftShift ) && GrappleTarget)
        {
            SetGrapplePoint(); 
        }else if (grappleRope.enabled && (Input.GetKeyDown(KeyCode.LeftShift)))
        {
            Disable();
        }else if (grappleRope.enabled && (Input.GetKeyDown(KeyCode.Space))){
            if (grappleRope.isGrappling)
            {
                playerHandler.doubleJump();
            }
            Disable();
        }

    }



    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime) //gun point @ mouse
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void SetGrapplePoint() //set grapple point to nearest object that we can grapple too
    {
        grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
        grappleRope.enabled = true;
    }

    public Vector2 GrappleMovement(Vector2 playerVelocity) //the meat of the grappling velocity
    {
        Vector2 toReturn = Vector2.zero; //vector returned to playerhandler for caluations

        float curDistance = Vector2.Distance(grapplePoint, m_rigidbody.position); // distnace from player to grapple point
        Vector2 ropeDirection = grapplePoint - m_rigidbody.position; //rope direction
        ropeDirection = ropeDirection.normalized; 
        float ropeAngle = Vector2.Angle(ropeDirection, Vector2.up); //angle of rope vs cardinal up

        if (curDistance > curMaxDistance && grappleRope.isGrappling) // this is the tension force, pulling you back into the arc
        {
            float springForceMagnitude = (curDistance - curMaxDistance) * tensionForce;
            Vector2 springForce = ropeDirection * springForceMagnitude;
            Vector2 dampingForce = m_rigidbody.velocity * dampiningForce;

            toReturn = springForce + dampingForce;
        }






        if (grappleRope.isGrappling && !playerHandler.isGrounded()) //this is the swing force, giving you a swing
        {

            if (playerVelocity.x < 0 || (m_rigidbody.position.x > grapplePoint.x && Mathf.Abs(playerHandler.rb.velocity.x) < 3 ))
            {
                float grappleMod = 1f;
                if ((playerVelocity.x > 0))
                {
                   grappleMod = 0.9f;
                }

                    if (m_rigidbody.position.x > grapplePoint.x)
                {

                    swingDir = swingDir - ((new Vector2((swingForce) * (ropeAngle + angleImpactReduction) / angleImpactReduction, 0f) * ((lengthImpactReduction + curMaxDistance) / (maxDistnace + lengthImpactReduction)) * grappleMod));
                }
                else
                {
                    swingDir = swingDir - (new Vector2((swingForceTwo) * (ropeAngle + angleImpactReduction) / angleImpactReduction, 0f) * ((lengthImpactReduction + curMaxDistance) / (maxDistnace + lengthImpactReduction))* grappleMod);
                }
            }
            if (playerVelocity.x > 0 || (m_rigidbody.position.x < grapplePoint.x && Mathf.Abs(playerHandler.rb.velocity.x) < 3))
            {

                float grappleMod = 1f;
                if ((playerVelocity.x < 0))
                {
                    grappleMod = 0.9f;
                }

                if (m_rigidbody.position.x < grapplePoint.x)
                {
                    swingDir = swingDir + (new Vector2((swingForce) * (ropeAngle + angleImpactReduction) / angleImpactReduction, 0f) * ((lengthImpactReduction + curMaxDistance) / (maxDistnace + lengthImpactReduction))* grappleMod);
                }
                else
                {
                    swingDir = swingDir + (new Vector2((swingForceTwo) * (ropeAngle + angleImpactReduction) / angleImpactReduction, 0f) * ((lengthImpactReduction + curMaxDistance) / (maxDistnace + lengthImpactReduction)) * grappleMod);
                }
            }
        }
        float elapsedTime = Time.time - grappleRope.lastEnabled;
        float scaleFactor = Mathf.Min(1.0f, elapsedTime * swingAccel);
        swingDir *= scaleFactor;

        return toReturn + swingDir;

    }




    public void Disable() //when we stop grappling
    {

        grappleRope.enabled = false;
        swingDir = Vector2.zero;
        curMaxDistance = maxDistnace;
        playerHandler.curDeadTime = 0;
        
    }

    private void findGrapplePoints()
    {

        if(grappleRope.enabled)
        {
            return;
        }

        Vector2 playerLoc = playerHandler.transform.position;
        List<GameObject> gameObjectsWithinBounds = new List<GameObject>();

        for(int i=0; i<grapplePoints.Length; i++) {
            grapplePoints[i].NotClosest();
            if (Utility.IsWithinBounds(grapplePointsOBJ[i].transform.position, new Vector2(playerLoc.x - maxDistnace, playerLoc.y - maxDistnace), new Vector2(playerLoc.x + maxDistnace, playerLoc.y + maxDistnace))){ //this should be replaced with a quadtree for preformance reasons

                if (!grapplePoints[i].hasBeenGrappled)
                {

                    Vector2 direction = grapplePointsOBJ[i].transform.position - firePoint.position;

                    // Calculate the distance to the target for the raycast
                    float distance = Vector2.Distance(firePoint.position, grapplePointsOBJ[i].transform.position);


                    // Perform the raycast
                    RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction.normalized, distance, layerMask);

                    if (hit.collider != null)
                    {
                        if (hit.collider.CompareTag("GrapplePoint"))
                        {
                            gameObjectsWithinBounds.Add(grapplePointsOBJ[i]);
                        }
                    
                    }


                        
                }
            }
        }

        float closeset = maxDistnace * 5f;
        int index = -1;

        for(int i=0;i< gameObjectsWithinBounds.Count; i++)
        {

            float dist = Vector2.Distance(firePoint.position, gameObjectsWithinBounds[i].transform.position);
                if(dist < closeset)
                {
                    closeset = dist;
                    index = i;
                }
        }
        if(index == -1)
        {
            GrappleTarget = false;
        }
       else 
       {
           gameObjectsWithinBounds[index].SendMessage("ClosestGrapple");
           grapplePointObj = gameObjectsWithinBounds[index];
           GrappleTarget = true;
           grapplePoint = gameObjectsWithinBounds[index].transform.position;
       }


       }

    }



