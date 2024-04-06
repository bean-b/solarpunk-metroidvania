using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{

    public GameObject door;
    public Sprite newsprite;
    
    private void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            door.SendMessage("Unlocked");
            this.GetComponent<SpriteRenderer>().sprite = newsprite;
        }
    }


}
