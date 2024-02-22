using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal p2;
    private Vector2 p2p;
    public float cooldown;
    
    // Start is called before the first frame update
    void Start()
    {
        p2p = p2.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(cooldown > 0) {
            cooldown -= Time.deltaTime;
        }
    }
    public void Cooldown() {
        cooldown = 0.5f;
    }
    private void OnTriggerEnter2D(Collider2D other) {
        print("beep)");
        var playerHandler = other.GetComponent<PlayerHandler>();
        if(cooldown <=0 && playerHandler != null) {
            playerHandler.forceMovement(p2p);
            p2.Cooldown();
        }
    }

}
