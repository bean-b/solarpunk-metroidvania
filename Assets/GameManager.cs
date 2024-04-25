using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool skipCutscene = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartGame()
    {
        if (skipCutscene)
        {
            
        }
    }

    public void SetCutscene()
    {
        skipCutscene = !skipCutscene;
    }
}
