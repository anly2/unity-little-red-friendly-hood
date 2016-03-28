using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            World.I.LogMovement(other.gameObject, transform.position);
    }
}
