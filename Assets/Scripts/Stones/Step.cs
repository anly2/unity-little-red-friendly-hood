using UnityEngine;
using System.Collections;

public class Step : MonoBehaviour {
	
	public GameObject ss;
	private SteppingStonesChallenge challenge;
	
	void Start () {
		challenge = ss.GetComponent<SteppingStonesChallenge> ();
	}
	
	void OnTriggerEnter2D(Collider2D c) {
		if(ss.activeSelf) challenge.PlayerStone (ss);
	}
	
}
