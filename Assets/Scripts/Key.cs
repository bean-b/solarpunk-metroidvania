using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{

    public List<GameObject> doors;
    public Sprite newsprite;
    


    private void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            foreach (GameObject go in doors)
            {
                go.SendMessage("Unlocked");
            }
            this.GetComponent<SpriteRenderer>().sprite = newsprite;
        }
    }


}
