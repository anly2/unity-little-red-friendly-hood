using UnityEngine;
using System.Collections;

public class Speaker : MonoBehaviour {
    
	void Start () {
        gameObject.Say("I am speaking some sample text. Something terribly long and long and long which would fit on no less than three lines");
	}
}
