using UnityEngine;
using System.Collections;

public class SampleSpeaker : MonoBehaviour {

    public string speech;
	
	void Start () {
        gameObject.Say(speech);
	}
}
