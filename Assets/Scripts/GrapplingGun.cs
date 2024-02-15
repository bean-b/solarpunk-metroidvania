using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GrapplingGun : MonoBehaviour
{

    public bool GrappleTarget = false;


    [SerializeField] private PlayerHandler playerHandler;  
    public GrapplingRope grappleRope;

    public Camera m_camera; 

    

    public Transform gunHolder; // our players tranform
    public Transform gunPivot; //where the gun turns around
    public Transform firePoint;//where we shoot from

    public Rigidbody2D m_rigidbody; //player rigid body


    public float maxDistnace = 10f; //max disstance our rope can shoot
    [HideInInspector] public float curMaxDistance = 10f; // rope distance is calucating when anchoring this prevents rope from sagging out to maxdistance always

    [SerializeField] private float swingForce; //how much swing we
    [SerializeField] private float swingForceTwo; //how much swing we get
    [SerializeField] private float tensionForce; //our much the rope keeps us in an arc
    [SerializeField] private float dampiningForce; //how much the rope loses momentum
    [SerializeField] private int angleImpactReduction; //minimizes effect of anglular momentum higher = less momemntun
    [SerializeField] private int lengthImpactReduction;


    [SerializeField] private float grappleBackSwingStr;

    [SerializeField] private float swingAccel;


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

        findGrapplePoints();

        if (!grappleRope.enabled && Input.GetKeyDown(KeyCode.LeftShift ) && playerHandler.grappleAvailable > 0 && GrappleTarget)
        {
            SetGrapplePoint(); 
        }else if (grappleRope.enabled && (Input.GetKeyDown(KeyCode.LeftShift)))
        {
            Disable();
            playerHandler.doubleJump();
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
        if (m_rigidbody.gravityScale > playerHandler.grappleGravMod)
        {
            m_rigidbody.gravityScale = playerHandler.grappleGravMod;
        }
        
    }

    private void findGrapplePoints()
    {
        Vector2 playerLoc = playerHandler.transform.position;
        float grappleLen = maxDistnace;
        GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("GrapplePoint");
        List<GameObject> gameObjectsWithinBounds = new List<GameObject>();
        for(int i=0; i<gameObjectsWithTag.Length; i++) {
            gameObjectsWithTag[i].SendMessage("NotClosest");
            if (Utility.IsWithinBounds(gameObjectsWithTag[i].transform.position, new Vector2(playerLoc.x - grappleLen, playerLoc.y - grappleLen), new Vector2(playerLoc.x + grappleLen, playerLoc.y + grappleLen))){
                gameObjectsWithinBounds.Add(gameObjectsWithTag[i]);
            }
        }

        float closeset = maxDistnace * 5f;
        int index = -1;

        for(int i=0;i< gameObjectsWithinBounds.Count; i++)
        {
            float dist = Vector2.Distance(playerLoc, gameObjectsWithinBounds[i].transform.position);
                if(dist < closeset)
                {
                    dist = closeset;
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
            GrappleTarget = true;
            grapplePoint = gameObjectsWithinBounds[index].transform.position;
       }

    }


}
