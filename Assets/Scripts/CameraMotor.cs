using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraMotor : MonoBehaviour
{
    //https://www.toptal.com/unity-unity3d/2d-camera-in-unity


    protected bool isYLocked = false;


    private float sizeGoal;
    private float changeSpeed = 0.02f;
    private Transform player;
    [SerializeField] private Vector3 offset;
    private Stack<Vector3> dests;
    
    public float laneGizmoHeight = 10f;
    public List<Vector4> lanes = new List<Vector4>();

    public Camera myCam;

    private float totalDist;
    private float distElap;
    private float speed = 5;

    private int currentLaneIndex = 0;

    public float followSpeed;
    void Start()
    {
        sizeGoal = myCam.orthographicSize;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        dests = new Stack<Vector3>();
        transform.position = player.position;
        UpdateCurrentLaneIndex();
        if(lanes.Count > 0)
        {
            isYLocked = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCurrentLaneIndex();

        if (dests.Count != 0) {
            if(distElap < 2) {
                dests.Pop();
            } else {
                float step = easeOutExpo(distElap/totalDist);
                transform.position = Vector3.MoveTowards(transform.position, dests.Peek(), step*speed);
            }
        } else {
            float xNew = Mathf.Lerp(transform.position.x, player.position.x, Time.deltaTime * followSpeed);
            float yNew = transform.position.y;
            if (!isYLocked)
            {
                yNew = Mathf.Lerp(transform.position.y, player.position.y, Time.deltaTime * followSpeed);
            }
            else
            {
                yNew = Mathf.Lerp(transform.position.y, lanes[currentLaneIndex].x, Time.deltaTime * followSpeed);
            }
            transform.position = new Vector3(xNew, yNew, offset.z);
        }

        float currentSize = Camera.main.orthographicSize;

        float newSize = Mathf.SmoothStep(currentSize, sizeGoal, changeSpeed);

        myCam.orthographicSize = newSize;
    }

    public void addDest(Vector2 dest) {
        if(isYLocked) {
            dest = new Vector2 (dest.x, lanes[ClosestLane(dest)].y);
        }
        dests.Push(dest);
        totalDist = Vector2.Distance(transform.position, dest);
    }
    public void clearDest() {
        dests = new Stack<Vector3>();
    }
    private float easeOutExpo(float num) {
        return (num == 1) ? 1 : 1 - math.pow(2, -10 * num);
    }
    void UpdateCurrentLaneIndex()
    {
        // Find the closest lane to the player and update currentLaneIndex
        currentLaneIndex = ClosestLane(player.transform.position);
        sizeGoal = lanes[currentLaneIndex].w;
    }

    int ClosestLane(Vector2 vec) {
        int laneIndex = -1;
        float minDistance = float.MaxValue;
        for (int i = 0; i < lanes.Count; i++)
        {
            if (vec.x <= lanes[i].z && vec.x >= lanes[i].y)
            {
                float distance = Mathf.Abs(vec.y - lanes[i].x);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    laneIndex = i;
                }
            }
        }
        return laneIndex;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 lane in lanes)
        {
            int offset = 2;

            Vector3 start = new Vector3(lane.y, lane.x, transform.position.z);
            Vector3 end = new Vector3(lane.z, lane.x, transform.position.z);
            Gizmos.DrawLine(start, end);
            Vector3 start2 = new Vector3(lane.y, lane.x+ offset, transform.position.z);
            Vector3 end2 = new Vector3(lane.y, lane.x - offset, transform.position.z);
            Gizmos.DrawLine(start2, end2);
            Vector3 start3 = new Vector3(lane.z, lane.x + offset, transform.position.z);
            Vector3 end3 = new Vector3(lane.z, lane.x - offset, transform.position.z);
            Gizmos.DrawLine(start3, end3);

        }
    }
}
