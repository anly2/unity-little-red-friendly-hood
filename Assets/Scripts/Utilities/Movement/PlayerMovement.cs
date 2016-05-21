using UnityEngine;
using System.Collections.Generic;

public class PlayerMovement : Movement {

    [Header("Listeners")]
    public List<GameObject> jumpListeners;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer rend;

    private bool jumping = false;


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


        if (jumping)
            return;

        if (Input.GetButtonDown("Jump"))
            Jump();


        float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

        animator.SetBool("Moving", (moveHorizontal != 0 || moveVertical != 0));

        if (rend != null && moveHorizontal != 0)
            rend.flipX = (moveHorizontal < 0);

		Vector3 movement = new Vector3 (moveHorizontal, moveVertical);
		rb.velocity = movement * speed;
    }


    public void Jump()
    {
        animator.SetTrigger("Jump");

        JumpStart();
        //JumpLand is called as an animation event
    }

    void JumpStart()
    {
        jumping = true;
        foreach (var l in jumpListeners)
            l.SendMessage("JumpStart");
    }

    void JumpLand()
    {
        jumping = false;
        foreach (var l in jumpListeners)
            l.SendMessage("JumpLand");
    }
}
