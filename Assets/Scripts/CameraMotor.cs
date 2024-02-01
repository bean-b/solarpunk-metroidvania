using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{


    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + offset; //TODO make this better. it should react to mouse post in some ways (i.e. move towards cursor) also it should just be more complex in general lol
    }
}
