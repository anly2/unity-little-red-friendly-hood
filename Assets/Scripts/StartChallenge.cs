using UnityEngine;
using System.Collections;

public class StartChallenge : MonoBehaviour {

	public GameObject trigger;
	private PlayerMovement pm;
	public GameObject player;

	private SteppingStonesChallenge challenge;

	void OnTriggerEnter2D(Collider2D c) {
		if (c.gameObject.tag.Equals ("Player") == true) { 
			challenge = trigger.GetComponent<SteppingStonesChallenge> ();
			pm = challenge.player.GetComponent<PlayerMovement> ();
			challenge.inPosition = true;
			pm.unRestricted = false;
		}
	}
}
