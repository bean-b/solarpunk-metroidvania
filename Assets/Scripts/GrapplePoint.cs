using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{

    SpriteRenderer spr;
    public Sprite unopened;
    public Sprite opened;

    public bool hasBeenGrappled = false;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }
    public void ClosestGrapple()
    {
        if (!hasBeenGrappled)
        {
            spr.sprite = opened;
        }
    }


    public void NotClosest()
    {
        spr.sprite = unopened;
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
