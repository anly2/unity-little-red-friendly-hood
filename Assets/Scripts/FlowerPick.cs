using UnityEngine;
using System.Collections;

public class FlowerPick : MonoBehaviour {

	public GameObject fl;

	void OnTriggerEnter2D(Collider2D collider) {
		DestroyObject (fl);
	}
}
