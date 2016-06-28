using UnityEngine;
using System.Collections;

public class DoorScript : MonoBehaviour {


    private Transform playerTrans = null;
    public bool doorStatus = false;
    public GameObject door;
    Animator anim;
    private AudioSource audioSource;
    public AudioClip open_door_1;
    public AudioClip close_door_1;

    // Use this for initialization
    void Start () {
        door = GameObject.FindGameObjectWithTag("Door");
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            //Calculate distance between player and door
            if (Vector3.Distance(playerTrans.position, this.transform.position) < 5f)
            {
                if (doorStatus)
                { //close door
                    //StartCoroutine(this.moveDoor(doorClose));
                    anim.Play("DoorClose");
                    audioSource.clip = close_door_1;
                    audioSource.Play();
                    doorStatus = false;
                }
                else
                { //open door
                    anim.Play("DoorOpen");
                    audioSource.clip = open_door_1;
                    audioSource.Play();
                    doorStatus = true;
                }


            }
        }
    }
}
