using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System.Linq;
public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;
	[SerializeField] private LayerMask m_WhatIsPlatform;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	public bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D m_Rigidbody2D;
	public bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;
	private bool goingDown = false;
	private bool jumpingUp = false;
	private bool inAir = false;
	public bool flip = false;
	public Animator animator;
	private bool land = false;
	public bool onSlope = false;
	private float maxSpeed=-1000000;
	public bool cantWalk = false;
	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	private bool keepJumping = false;
	private bool isJumping = false;
	public float jumpTimeCounter;
	public float jumpTime;
	private int jumpIncrease = 1;


	private float originalGrav;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();
		OnLandEvent.AddListener(PlayLandAnimation);


		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();

		originalGrav = Physics2D.gravity.y;
	}

	private void FixedUpdate()
	{

		maxSpeed = GetComponent<playerController>().runSpeed * Time.fixedDeltaTime * 10f;
		//maxSpeed *=0.9f;
		//Debug.Log("curretn : " + m_Rigidbody2D.velocity.x);
		//Debug.Log("Current speed: " + m_Rigidbody2D.velocity.x);
		if (Mathf.Abs(m_Rigidbody2D.velocity.y) >=0.5f)
        {
			/*Vector2 speed = m_Rigidbody2D.velocity;
			speed.y = -20f;
			m_Rigidbody2D.velocity = speed;
			Debug.Log("Going on slope");*/
		}
		if (Mathf.Abs(m_Rigidbody2D.velocity.x) > maxSpeed)
		{
			/*Debug.Log("Current speed: " + m_Rigidbody2D.velocity.x);
			Vector2 speed = m_Rigidbody2D.velocity;
			speed.x = maxSpeed/3;
			speed.y = 0f;
			m_Rigidbody2D.velocity = speed;
			Debug.Log("Going above max speed");*/
		}

        if (onSlope)
        {
			Physics2D.gravity = new Vector2(Physics2D.gravity.x, 0);
        }
        else
        {
			Physics2D.gravity = new Vector2(Physics2D.gravity.x, originalGrav);
		}
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] platformCol = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsPlatform);
		Collider2D[] groundCol = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		Collider2D[] colliders = groundCol;
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
				if (!wasGrounded)
				{
					Vector2 temp = m_Rigidbody2D.velocity;
					temp.y = 0;
					landRPC();
					m_Rigidbody2D.velocity = temp;
					if ((/*goingDown ||*/ m_Rigidbody2D.velocity.y == 0))
					{
						
						
					}
				}


			}
		}

		for (int i = 0; i < platformCol.Length; i++)
		{
			if (platformCol[i].gameObject != gameObject)
			{
				onSlope = true;
			}
		}

        if (platformCol.Length == 0)
        {
			onSlope = false;
        }
		if (m_Grounded == false && !goingDown)
		{
			if (m_Rigidbody2D.velocity.y < 0)
			{
				jumpDownRPC();
			}
		}

		/*if (m_Grounded == false  && jumpingUp && inAir)
		{
			if (m_Rigidbody2D.velocity.y > 0)
			{
				photonView.RPC("jumpUpRPC", RpcTarget.All);
			}
		}*/


	}


	public void Move(float move, bool crouch, bool jump, bool cantWalk)
	{
        if (cantWalk)
        {
			return;
        }
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			}
			else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				//Flip();
				FlipRPC();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				//Flip();
				FlipRPC();
			}
		}
		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			jumpingUp = true;
			inAir = true;
			isJumping = true;
			jumpTimeCounter = jumpTime;
			jumpIncrease = 1;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}

        /*if (keepJumping)
        {
			jumpingUp = true;
			inAir = true;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}*/

		if (Input.GetButton("Jump") && isJumping)
		{
			Debug.Log("jump");
			if (jumpTimeCounter > 0)
			{
				Debug.Log("RAn");
				m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce/jumpIncrease));
				jumpTimeCounter -= Time.deltaTime;
				jumpIncrease++;
			}
			else
			{
				isJumping = false;
			}
		}

		//if he no longer isjumping
		if (Input.GetButtonUp("Jump"))
		{
			isJumping = false;
		}
	}

	public void FlipRPC()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;

		//flip = true;
	}

	private void PlayLandAnimation()
	{

		goingDown = false;
		//animator.Play("land", 0, 0);
		//animator.Update(0f);

	}
	public bool isGrounded()
	{
		return m_Grounded;
	}

	public bool isJumpingUp()
	{
		return jumpingUp;
	}

	public bool isInAir()
    {
		return inAir;
    }

	public bool shouldFlip(float move)
    {
		if (move > 0 && !m_FacingRight)
		{
			// ... flip the player.
			return true;
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (move < 0 && m_FacingRight)
		{
			// ... flip the player.
			return true;
		}
		return false;
	}


	public void landRPC()
    {
		OnLandEvent.Invoke();
		jumpingUp = false;
		inAir = false;
		land = true;
	}


	public void jumpDownRPC()
    {
		//animator.Play("jumpDown", 0, 0);
		//animator.Update(0);
		goingDown = true;
		jumpingUp = false;
		inAir = true;
		land = false;
	}


	/*public void jumpUpRPC()
    {
		GetComponent<playerController>().jumped = true;
		jumpingUp = true;
		inAir = true;
		goingDown = false;
		land = false;
		if (isGrounded())
		{
			//animator.Play("jumpUp", 0, 0);
			//animator.Update(0f);
		}
	}*/

	public bool isFacingRight()
    {
		return m_FacingRight;
    }
	/*private void OnDrawGizmos()
	{
		var c= 
		Handles.color = Color.red;
		Handles.DrawWireDisc(m_GroundCheck.position,transform.forward,0.3f);								//radius
	}*/
}