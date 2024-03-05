using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmogGenHandler : MonoBehaviour
{


    private Camera mainCam;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mainCam.transform.position + offset;
    }
}
