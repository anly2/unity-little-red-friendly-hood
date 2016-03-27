using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class QuickWorld : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1"))
            Reload();
	}

    void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        World.I.Invoke("Recreate", 0.1f); //Allow the Scene to reload
    }
}
