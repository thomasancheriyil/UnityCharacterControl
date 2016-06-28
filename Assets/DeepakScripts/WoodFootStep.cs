using UnityEngine;
using System.Collections;

public class WoodFootStep : MonoBehaviour {

    public GameObject player;
    private Animator anim;
    public AudioSource footstep;
    private GameObject dirtPS;

    private bool step = true;

    // Use this for initialization
    void Start()
    {
        // Get access to player animator and audio sources
        anim = player.GetComponent<Animator>();
        dirtPS = GameObject.Find("MudPart");
    }

    void OnTriggerStay(Collider other)
    {


        // Check if player has entered a footstep trigger
        if (other.gameObject.tag == "Player")
        {
           
            if (anim.GetFloat("Speed") > 0.5 && step)
            {
                StartCoroutine(PlaySound());
            }
        }
    }

    IEnumerator PlaySound()
    {
        step = false;
        // Plays a walk step and waits ample time before the next on
        ParticleSystem ps = dirtPS.GetComponent<ParticleSystem>();
        ps.Play();
        footstep.Play();
        yield return new WaitForSeconds(0.3f);
        step = true;
        ps.Stop();
    }
}
