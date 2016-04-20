using UnityEngine;
using System.Collections;

public class SetupCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Physics2D.IgnoreLayerCollision (8, 9);
	}
}
