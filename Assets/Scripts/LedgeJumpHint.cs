using UnityEngine;
using System.Collections;

public class LedgeJumpHint : MonoBehaviour {

    private bool shownAlready = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (shownAlready) return;

        if (other.tag != "Player")
            return;

        other.gameObject.Think("I should be able to jump [Spacebar] over those stones.");
        shownAlready = true;
    }
}
