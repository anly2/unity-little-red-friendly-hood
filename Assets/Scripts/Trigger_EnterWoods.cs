using UnityEngine;
using System.Collections;

public class Trigger_EnterWoods : MonoBehaviour {

	private MessageAPI mapi;

	void Start () {
		GameObject player = GameObject.Find ("Player");
		mapi = player.GetComponent<MessageAPI> ();
	}

	void OnTriggerEnter2D(Collider2D collider) {
		mapi.showMessage ("You've entered the woods!");
	}

	void OnTriggerExit2D(Collider2D collider) {
		mapi.hideMessage ();
	}
}
