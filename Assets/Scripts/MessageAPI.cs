using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageAPI : MonoBehaviour {

	public Text messageText;
	public GameObject fader;

	void Awake () {
		toggleVisibility (false);
	}

	public void showMessage(string msg) {
		messageText.text = msg;
		toggleVisibility (true);
	}

	public void hideMessage() {
		toggleVisibility (false);
	}

	private void toggleVisibility(bool tof) {
		fader.SetActive (tof);
	}
}
