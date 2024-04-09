using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    private PlatformEffector2D effector;

    private PlayerHandler handler;
    private float waitTime = 0;
    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
        handler = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S) && waitTime <= 0) {
            effector.rotationalOffset = 180f;
            handler.addForce(new Vector2(0f, -20f));
            handler.HighGrav();
            waitTime = 1.5f;
        }
        waitTime -= Time.deltaTime;
        if(waitTime <=0) {
            effector.rotationalOffset = 0f;
        }
    }
}
