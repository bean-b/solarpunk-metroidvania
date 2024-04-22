using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SimpleCutsceneManager : MonoBehaviour
{

    public List<Sprite> sprites = new List<Sprite> ();
    public List<float> delayTime = new List<float>();
     public string nextSceneName;
    private SpriteRenderer spriteRenderer;
    private float lastTime;
    private int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastTime = Time.time;
        spriteRenderer.sprite = sprites[index];
    }

    // Update is called once per frame
void Update()
{
    if (index < sprites.Count && Time.time - lastTime >= delayTime[index])
    {
        lastTime = Time.time; // Reset the time
        index++; // Move to the next index

        if (index >= sprites.Count)
        {
            progressScene(); // Handle the end of the sequence
        }
        else
        {
            spriteRenderer.sprite = sprites[index]; // Update the sprite
        }
    }
}


    void progressScene(){
        SceneManager.LoadScene(nextSceneName);
    }
}
