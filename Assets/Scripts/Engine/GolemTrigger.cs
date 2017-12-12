using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GolemTrigger : MonoBehaviour {

    public AudioSource Mix;

    void OnTringgerEnter2D(Collider2D coll)
    {
        if(coll.transform.tag == "Player")
            Mix.Play();
    }
	
}
