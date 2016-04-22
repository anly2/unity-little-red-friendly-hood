using UnityEngine;
using System.Collections;

public class SteppingStonesChallenge : MonoBehaviour {

	public GameObject ss1;
	public GameObject ss2;
	public GameObject ss3;
	public GameObject ss4;
	public GameObject ss5;
	public GameObject ss6;

	public GameObject msg;

	public GameObject landing;
	private PlayerMovement pm;

	public GameObject player;

	public bool inPosition = false;

	private GameObject currentStone = null;

	private bool flag = true;

	void Start () {
		StartCoroutine (Challenge());
	}

	void Update () {
		if (Input.GetKeyDown ("space")) {
			if (inPosition) {
				DisableRagdoll();
				Jump(ss1);
				inPosition = false;
			} else if (currentStone != null){

				if (currentStone.Equals(ss1)) {
					Jump(ss2);
				} else if (currentStone.Equals(ss2)) {
					Jump(ss3);
				} else if (currentStone.Equals(ss3)) {
					Jump(ss4);
				} else if (currentStone.Equals(ss4)) {
					Jump(ss5);
				} else if (currentStone.Equals(ss5)) {
					Jump(ss6);
				} else if (currentStone.Equals(ss6)) {
					Jump (landing);
					pm = player.GetComponent<PlayerMovement> ();
					pm.unRestricted = true;
					EnableRagdoll();
				}
			}			
		}
	}

	void EnableRagdoll() {
		Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
		rb.isKinematic = false;
	}

	void DisableRagdoll() {
		Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
		rb.isKinematic = true;
	}

	private void Jump(GameObject stone) {
		if (stone.activeSelf) {
			//Move player to stone
			Vector3 stonePosition = stone.transform.position;
			Vector3 playerPosition = player.transform.position;
			player.transform.position += (stonePosition-playerPosition);
			PlayerStone (stone);
		} else {
			//TODO die
			StartCoroutine (Death());
		}
	}

	IEnumerator Challenge () {

		for (;;) {
			ss1.SetActive(flag);
			ss2.SetActive(flag);
			ss3.SetActive(flag);
			ss4.SetActive(flag);
			ss5.SetActive(flag);
			ss6.SetActive(flag);
			yield return new WaitForSeconds (1);
			ss1.SetActive(!flag);
			ss3.SetActive(!flag);
			ss5.SetActive(!flag);
			if(currentStone != null) if (!currentStone.activeSelf) {
				//TODO die
				StartCoroutine (Death());
			}
			yield return new WaitForSeconds (1);
			ss1.SetActive(flag);
			ss2.SetActive(flag);
			ss3.SetActive(flag);
			ss4.SetActive(flag);
			ss5.SetActive(flag);
			ss6.SetActive(flag);
			yield return new WaitForSeconds (1);
			ss2.SetActive(!flag);
			ss4.SetActive(!flag);
			ss6.SetActive(!flag);
			if(currentStone != null) if (!currentStone.activeSelf) {
				//TODO die
				StartCoroutine (Death());
			}
			yield return new WaitForSeconds (1);
		}

	}

	public void PlayerStone(GameObject stone) {
		currentStone = stone;
	}

	IEnumerator Death () {
		MessageAPI mapi = msg.GetComponent<MessageAPI> ();
		mapi.showMessage ("You fell into the river and drowned");
		yield return new WaitForSeconds (3);
		Reload ();
	}

	void Reload()
	{
        //StartCoroutine (Death());
        //GameObject player = GameObject.FindWithTag("Player");
        //World.I.LogMovement(player, player.transform.position);

        //World.I.Reload();
        currentStone = null;
        SaveManager.Load("stepping stones checkpoint");
        MessageAPI mapi = msg.GetComponent<MessageAPI>();
        mapi.hideMessage();
	}
}
