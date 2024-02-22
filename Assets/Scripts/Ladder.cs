using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    private PlatformEffector2D effector;
    private float waitTime = 0;
    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S) && waitTime <= 0) {
            effector.rotationalOffset = 180f;
            waitTime = 0.5f;
        }
        waitTime -= Time.deltaTime;
        if(waitTime <=0) {
            effector.rotationalOffset = 0f;
        }
    }
}
