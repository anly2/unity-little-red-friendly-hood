using UnityEngine;
using System.Collections;

public class Doll : MonoBehaviour {

    public bool facingRight = true;
    public float runningThreshold = 2f;
    public float movementThreshold = 0.01f;

    private Animator animator;
    private SpriteRenderer rend;

    private Vector3 lastPosition;
    

	void Start () {
        lastPosition = transform.position;

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("The Doll component requires an Animator component to be attached.");
            gameObject.SetActive(false);
        }


        rend = GetComponent<SpriteRenderer>();
	}
	

	void Update () {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(lastPosition, pos);

        if (distance < movementThreshold)
        {
            animator.SetBool("Moving", false);
            return;
        }

        animator.SetBool("Moving", true);

        float speed = distance / Time.deltaTime;
        animator.SetBool("Running", (speed >= runningThreshold));


        if (rend != null)
        {
            if (pos.x >= lastPosition.x)
                rend.flipX = !facingRight;
            else
                rend.flipX = facingRight;
        }

        
        lastPosition = pos;
	}
}
