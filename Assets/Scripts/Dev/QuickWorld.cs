using UnityEngine;
using System.Collections;

public class QuickWorld : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
       // if (Input.GetButtonDown("Submit"))
        //    Reload();
	}

    void Reload()
    {
        GameObject player = GameObject.FindWithTag("Player");
        World.I.LogMovement(player, player.transform.position);

        World.I.Reload();
    }
}
