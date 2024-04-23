using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningBlade : MonoBehaviour
{
    private bool running = false;
    public float speed = 500f;

    // Start is called before the first frame update
    void Start()
    {
        running = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime, Space.Self);
    }

    public void stop()
    {
        running = false;
    }
}
