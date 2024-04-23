using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//attribution: https://youtu.be/hH0OYz7YtKk?feature=shared
public class movihgHzard : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed;
    [SerializeField] private bool isCircuit; // whether it will treat waypoints[]
                                             //as a circle or a line (will be the same for two)
    private Transform targetWaypoint;
    private int currentWaypointIndex = 0;
    private float checkDistance = 0.05f;
    private bool frontwards = true; //for linear movement

    private Vector2 lastPos;


    // Start is called before the first frame update
    void Start()
    {
        targetWaypoint = waypoints[0];
        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        lastPos = transform.position;

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetWaypoint.position,
            speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetWaypoint.position) < checkDistance)
        {
            if (isCircuit)
            {
                targetWaypoint = getNextWPCircuit();
            }
            else
            {
                targetWaypoint = getNextWPLinear();
            }

        }



    }

    private Transform getNextWPCircuit()
    { // waypoints are in a circuit (a,b,c,a,b,c)
        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0;
        }
        return waypoints[currentWaypointIndex];
    }
    private Transform getNextWPLinear()
    { // waypoints are in a line (a,b,c,b,a,b)
        if (frontwards)
        {
            currentWaypointIndex++;
        }
        else
        {
            currentWaypointIndex--;
        }
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex -= 2;
            frontwards = false;
        }
        if (currentWaypointIndex < 0)
        {
            currentWaypointIndex += 2;
            frontwards = true;
        }
        return waypoints[currentWaypointIndex];
    }

}