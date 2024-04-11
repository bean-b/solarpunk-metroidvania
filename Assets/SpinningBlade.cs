using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningBlade : MonoBehaviour
{
    private bool running = false;
    private float speed = 0.75f;
    // Start is called before the first frame update
    void Start()
    {
        running = true;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, speed, Space.Self);
    }

    public void stop()
    {
        running = false;
    }
}
