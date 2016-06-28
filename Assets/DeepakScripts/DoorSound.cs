using UnityEngine;
using System.Collections;

public class DoorSound : MonoBehaviour
{   
    private AudioSource doorOpen;
    void Start()
    {
        doorOpen = GetComponent<AudioSource>();
    }
    
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            doorOpen.Play();
        }
    }
}
