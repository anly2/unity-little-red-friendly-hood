using UnityEngine;
using System.Collections;

public class StartChallenge : MonoBehaviour {

	public GameObject trigger;
	private PlayerMovement pm;
	public GameObject player;

	private SteppingStonesChallenge challenge;

	void OnTriggerEnter2D(Collider2D c) {
        if (!c.gameObject.tag.Equals("Player"))
            return;

        SaveManager.Save("stepping stones checkpoint");

        player.Think("I think I can jump [Spacebar] between these stepping stones");
		challenge = trigger.GetComponent<SteppingStonesChallenge> ();
		pm = challenge.player.GetComponent<PlayerMovement> ();
		challenge.inPosition = true;
		pm.unRestricted = false;
	}
}
