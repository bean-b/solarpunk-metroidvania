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


    [SerializeField]
    protected bool isYLocked = false;

    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    private Stack<Vector3> dests;
    private float speed = 10f;
    private float timeElapsed = 0;
    private float maxTime = 2.5f;
    public float laneGizmoHeight = 10f;
    public List<float> lanes = new List<float>();

    private int currentLaneIndex = 0;

    public float followSpeed;
    void Start()
    {
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
            if(Vector2.Distance(transform.position, dests.Peek()) < 2) {
                dests.Pop();
            } else {
                float step = speed * ((timeElapsed/maxTime > 1) ? 1 : easeOutExpo(timeElapsed/maxTime));
                timeElapsed += Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, dests.Peek(), step);
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

                yNew = Mathf.Lerp(transform.position.y, lanes[currentLaneIndex], Time.deltaTime * followSpeed);
            }




            transform.position = new Vector3(xNew, yNew, offset.z);
        }
    }

    public void addDest(Vector2 dest) {
        dests.Push(dest);
        speed = Vector2.Distance(transform.position, dest) * 0.003f/maxTime;
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
        float minDistance = float.MaxValue;
        for (int i = 0; i < lanes.Count; i++)
        {
            float distance = Mathf.Abs(player.position.y - lanes[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                currentLaneIndex = i;
            }
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (float lane in lanes)
        {
            Vector3 start = new Vector3(-1000, lane, transform.position.z);
            Vector3 end = new Vector3(1000, lane, transform.position.z);
            Gizmos.DrawLine(start, end);
        }
    }
}
