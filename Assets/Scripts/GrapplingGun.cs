using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GrapplingGun : MonoBehaviour
{


    [SerializeField] private PlayerHandler playerHandler;  
    public GrapplingRope grappleRope;

    [SerializeField] private bool grappleToAll = false;
    [SerializeField] private int grappableLayerNumber = 9;

    public Camera m_camera;

    public Transform gunHolder;
    public Transform gunPivot;
    public Transform firePoint;

    public Rigidbody2D m_rigidbody;


    [SerializeField] private const float maxDistnace = 10f;
    [HideInInspector] public float curMaxDistance = 10f;

    [SerializeField] private float swingForce;
    [SerializeField] private float tensionForce;
    [SerializeField] private float dampiningForce;
    [SerializeField] private float swingRatio;




    private Vector2 swingDir = Vector2.zero;
    private int layerMask;

    private enum LaunchType
    {
        Transform_Launch,
        Physics_Launch
    }

    [HideInInspector] public Vector2 grapplePoint;
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


    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void SetGrapplePoint()
    {
        Vector2 distanceVector = m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position;
        if (Physics2D.Raycast(firePoint.position, distanceVector.normalized, maxDistnace, layerMask))
        {
            RaycastHit2D _hit = Physics2D.Raycast(firePoint.position, distanceVector.normalized, maxDistnace, layerMask);
            if (_hit.transform.gameObject.layer == grappableLayerNumber || grappleToAll)
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

    public Vector2 GrappleMovement(Vector2 playerVelocity)
    {
        

        Vector2 toReturn = Vector2.zero;

        float curDistance = Vector2.Distance(grapplePoint, m_rigidbody.position);
        Vector2 ropeDirection = grapplePoint - m_rigidbody.position;
        ropeDirection = ropeDirection.normalized;
        float ropeAngle = Vector2.Angle(ropeDirection, Vector2.up);

        if (curDistance > curMaxDistance && grappleRope.enabled)
        {


            float springForceMagnitude = (curDistance - curMaxDistance) * tensionForce;
            Vector2 springForce = ropeDirection * springForceMagnitude;
            Vector2 dampingForce = m_rigidbody.velocity * dampiningForce;

            toReturn = springForce + dampingForce;
        }






        if (grappleRope.isGrappling)
        {
            if (m_rigidbody.position.x > grapplePoint.x && playerVelocity.x < 0)
            {
                swingDir = swingDir - new Vector2((swingForce * swingRatio)* (ropeAngle+ 1000) / 1000, 0f);

            }
            else if (m_rigidbody.position.x > grapplePoint.x && playerVelocity.x > 0)
            {
                swingDir = swingDir + new Vector2((swingForce * swingRatio) * (ropeAngle + 1000) / 1000, 0f);
            }


            if (m_rigidbody.position.y > grapplePoint.y - curMaxDistance)
            {
                swingDir = swingDir - new Vector2(0f, swingForce * (1 - swingRatio) * (ropeAngle + 1000) / 500);

            }
            else
            {
                swingDir = swingDir + new Vector2(0f, swingForce * (1 - swingRatio) * (ropeAngle + 1000) / 500);
            }

        }

        return toReturn + swingDir;

    }




    public void Disable()
    {
        grappleRope.enabled = false;
        swingDir = Vector2.zero;
        curMaxDistance = maxDistnace;
        playerHandler.curDeadTime = 0;
        playerHandler.graplingLengthMod = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistnace);
        }
    }

}
