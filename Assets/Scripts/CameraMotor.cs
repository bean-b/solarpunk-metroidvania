using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{


    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    private Stack<Vector3> dests;
    private float speed = 10f;
    private float timeElapsed = 0;
    private float maxTime = 2.5f;
    void Start()
    {
        dests = new Stack<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        if(dests.Count != 0) {
            if(Vector2.Distance(transform.position, dests.Peek()) < 2) {
                dests.Pop();
            } else {
                float step = speed * ((timeElapsed/maxTime > 1) ? 1 : easeOutExpo(timeElapsed/maxTime));
                timeElapsed += Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, dests.Peek(), step);
            }
        } else {
            //dests.Push(player.position + offset);
            transform.position = player.position + offset; //TODO make this better. it should react to mouse post in some ways (i.e. move towards cursor) also it should just be more complex in general lol
        }
    }

    public void addDest(Vector2 dest) {
        dests.Push(dest);
        speed = Vector2.Distance(transform.position, dest) * 0.003f/maxTime;
    }
    public void clearDest() {
        dests = new Stack<Vector3>();
    }
    private float easeOutExpo(float num) {
        return (num == 1) ? 1 : 1 - math.pow(2, -10 * num);
    }
  }
