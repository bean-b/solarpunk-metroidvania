using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal p2;
    private Vector2 p2p;
    
    // Start is called before the first frame update
    void Start()
    {
        p2p = p2.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other) {
        print("beep)");
        var playerHandler = other.GetComponent<PlayerHandler>();
        if(playerHandler != null) {
            playerHandler.forceMovement(p2p);
        }
    }

}
