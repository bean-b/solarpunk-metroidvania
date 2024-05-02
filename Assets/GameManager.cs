using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        string scene;
        if (skipCutscene)
        {
            scene = "demo";
        }
        else 
        {
            scene = "Cutscene";
        }
        SceneManager.LoadScene(scene);
    }

    public void SetCutscene()
    {
        skipCutscene = !skipCutscene;
    }
}
