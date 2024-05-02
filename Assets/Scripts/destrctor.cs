using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destrctor : MonoBehaviour
{

    public AudioSource audioSource;
    public Camera main_camera;

    public CameraMotor cameraMotor;


    private void OnCollisionEnter2D(Collision2D collider)
    {
       main_camera.transform.position = new Vector3(-100f, -100f, -10f);
       cameraMotor.enabled = false;
       audioSource.Play();
    }

}
