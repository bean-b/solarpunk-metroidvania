using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{

    SpriteRenderer spr;

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }
    public void ClosestGrapple()
    {
        spr.color = Color.red;
    }
    public void NotClosest()
    {
        spr.color = Color.white;
    }
}
