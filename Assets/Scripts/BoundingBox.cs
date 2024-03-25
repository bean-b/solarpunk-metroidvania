using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox : MonoBehaviour
{
    [SerializeField] private PlayerHandler player;
    [SerializeField] private float floor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player.transform.position.y < floor) {
            player.SendMessage("die");
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = new Vector3(-1000, floor, transform.position.z);
        Vector3 end = new Vector3(1000, floor, transform.position.z);
        Gizmos.DrawLine(start, end);
    }
}
