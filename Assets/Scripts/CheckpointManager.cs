using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private PlayerHandler playerHandler;

    public List<Vector3> checkPoints = new List<Vector3>();


    private void Start()
    {
        playerHandler = GetComponent<PlayerHandler>();
    }


    private void Update()
    {
        Vector2 playerPos = transform.position;

        for (int i = 0; i < checkPoints.Count; i++)
        {

            if(Vector2.Distance(playerPos, new Vector2(checkPoints[i].x, checkPoints[i].y)) < checkPoints[i].z)
            {
                playerHandler.setRespawnPoint(new Vector2(checkPoints[i].x, checkPoints[i].y));
            }

        }

    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < checkPoints.Count; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(checkPoints[i].x, checkPoints[i].y, 0), checkPoints[i].z);


        }
    }
}
