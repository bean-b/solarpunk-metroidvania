using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{

    SpriteRenderer spr;

    public bool hasBeenGrappled = false;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }
    public void ClosestGrapple()
    {
        if (!hasBeenGrappled)
        {
            spr.color = Color.yellow;
        }
    }


    public void turnBlue()
    {
        spr.color = Color.blue;
    }
    public void NotClosest()
    {
        spr.color = Color.white;
    }

    public void Used()
    {
        hasBeenGrappled = true;
    }

    public void PlayerGrounded()
    {
        hasBeenGrappled = false;
    }
}
