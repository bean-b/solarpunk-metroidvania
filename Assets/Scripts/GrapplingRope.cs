using Unity.VisualScripting;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    [Header("General Refernces:")]
    public GrapplingGun grapplingGun;
    public LineRenderer m_lineRenderer;

    [Header("General Settings:")]
    [SerializeField] private int precision = 40; //how many 'voxels' (wrong term but yk) the rope is made out of
    [Range(0, 20)][SerializeField] private float straightenLineSpeed = 5; //how quick the rope goes from wavy to taught

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve; //the curve of the wavy of the rope
    [Range(0.01f, 4)][SerializeField] private float StartWaveSize = 2; //size
    float waveSize = 0; //cur size

    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve; 
    [SerializeField][Range(1, 50)] private float ropeProgressionSpeed = 1; 
    [SerializeField] PlayerHandler playerHandler;
    float moveTime = 0;

    [HideInInspector] public bool isGrappling = true;

    bool straightLine = true;

    private LineRenderer lineRenderer;

    [HideInInspector] public float lastEnabled;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    private void OnEnable()
    {
        moveTime = 0;
        m_lineRenderer.positionCount = precision;
        waveSize = StartWaveSize;
        straightLine = false;

        LinePointsToFirePoint();


        m_lineRenderer.enabled = true;

        
    }

    private void OnDisable()
    {
        m_lineRenderer.enabled = false;
        isGrappling = false;
        grapplingGun.grapplePointObj.SendMessage("Used");
    }

    private void LinePointsToFirePoint()
    {
        for (int i = 0; i < precision; i++)
        {
            m_lineRenderer.SetPosition(i, grapplingGun.firePoint.position);
        }
    }

    private void Update()
    {
        //this code makes it more and more red before breaking
        Color color = new Color(playerHandler.curDeadTime * (1f / playerHandler.maxDeadTime), 0, 0);
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        moveTime += Time.deltaTime;
        DrawRope();
    }

    void DrawRope()
    {
        if (!straightLine)
        {
            if (m_lineRenderer.GetPosition(precision - 1).x == grapplingGun.grapplePoint.x)
            {
                straightLine = true;
            }
            else
            {
                DrawRopeWaves();
            }
        }
        else
        {
            if (!isGrappling)
            {
                isGrappling = true;
                grapplingGun.curMaxDistance = Vector2.Distance(grapplingGun.grapplePoint, playerHandler.rb.position);
                lastEnabled = Time.time;
            }
            if (waveSize > 0)
            {
                waveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else
            {
                waveSize = 0;

                if (m_lineRenderer.positionCount != 2) { m_lineRenderer.positionCount = 2; }

                DrawRopeNoWaves();
            }
        }
    }

    void DrawRopeWaves()
    {
        for (int i = 0; i < precision; i++)
        {
            float delta = (float)i / ((float)precision - 1f);
            Vector2 offset = Vector2.Perpendicular(grapplingGun.grappleDistanceVector).normalized * ropeAnimationCurve.Evaluate(delta) * waveSize;
            Vector2 targetPosition = Vector2.Lerp(grapplingGun.firePoint.position, grapplingGun.grapplePoint, delta) + offset;
            Vector2 currentPosition = Vector2.Lerp(grapplingGun.firePoint.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            m_lineRenderer.SetPosition(i, currentPosition);
        }
    }

    void DrawRopeNoWaves()
    {
        m_lineRenderer.SetPosition(0, grapplingGun.firePoint.position);
        m_lineRenderer.SetPosition(1, grapplingGun.grapplePoint);
    }
}
