using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] private float jumpForce = 1300f;
	[Range(0, .3f)] [SerializeField] private float movementSmoothing = .05f;
	[SerializeField] private bool airControl = false;
	[SerializeField] private LayerMask whatIsGround;
	[SerializeField] private BoxCollider2D groundCheckCollider;
    [SerializeField] private SpriteRenderer sRenderer;

    private bool grounded;
	private Rigidbody2D rb;
	private bool facingRight = true;
	private Vector3 currentVelocity = Vector3.zero;

	[Header("Events")]
	public UnityEvent OnLandEvent;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();

        OnLandEvent ??= new UnityEvent();
	}

    private void FixedUpdate()
	{
		CheckGround();
    }

    public GameObject CheckGround()
	{
        bool wasGrounded = grounded;
        grounded = false;

        if (groundCheckCollider != null)
        {
            List<Collider2D> colliders = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(whatIsGround);
            filter.useLayerMask = true;
            groundCheckCollider.OverlapCollider(filter, colliders);
            if (colliders.Count > 0)
            {
                grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
				return colliders[0].gameObject;
            }
        }
		return null;
    }

	public void Move(float move, bool jump)
	{
		//only control the player if grounded or airControl is turned on
		if (grounded || airControl)
		{
			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, rb.velocity.y);
			// And then smoothing it out and applying it to the character
			rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref currentVelocity, movementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !facingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && facingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if (grounded && jump)
		{
			// Add a vertical force to the player.
			grounded = false;
			rb.AddForce(new Vector2(0f, jumpForce));
		}
	}

	public bool IsGrounded()
	{
		return grounded;
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		sRenderer.flipX = !facingRight;
	}
}
