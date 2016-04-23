using UnityEngine;
using System.Collections;

public class BrokenBridge : MonoBehaviour {

    private SpeechBubble thought;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;

        thought = other.gameObject.Think("The bridge is broken. I will have to find another way.", -1);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (thought != null)
            Destroy(thought.gameObject);
    }
}
