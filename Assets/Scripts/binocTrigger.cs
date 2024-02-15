using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class binocTrigger : MonoBehaviour
{


    public PlayableDirector director;


    private bool hasGone = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(!hasGone)
            {
                director.Play();
                hasGone = true; 
            }
        }
    }
}
