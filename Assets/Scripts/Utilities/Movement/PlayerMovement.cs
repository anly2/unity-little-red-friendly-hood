using UnityEngine;
using System.Collections;

public class PlayerMovement : Movement {
    
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void Update ()
    {
        if (speed == 0)
            return;
        
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (moveHorizontal, moveVertical);

		rb.velocity = movement * speed;
    }
}
