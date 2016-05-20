﻿using UnityEngine;
using System.Collections;

public class PlayerMovement : Movement {
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer rend;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rend = GetComponent<SpriteRenderer>();
    }


    void Update ()
    {
        if (speed == 0)
        {
            animator.SetBool("Moving", false);
            return;
        }
        
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

        animator.SetBool("Moving", (moveHorizontal != 0 || moveVertical != 0));

        if (rend != null && moveHorizontal != 0)
            rend.flipX = (moveHorizontal < 0);

		Vector3 movement = new Vector3 (moveHorizontal, moveVertical);
		rb.velocity = movement * speed;
    }
}
