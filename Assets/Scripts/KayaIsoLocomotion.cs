using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class KayaIsoLocomotion : MonoBehaviour {

	private Animator anim;
	private Vector3 mainCamForward;
	private Vector3 mainCamRight;
	private Vector3 moveInput;
	private float turn;
	private float forward;
	private float animSpeed;
	private float animDirection;
	public LayerMask mask;
	public float groundHeight;				//compensation in y-direction when checking for ground

	private IsometricCamera isoCam;
	private Transform cam;

	private Rigidbody kayaRB;
	private CapsuleCollider kayaCollider;

	public Component[] avatarBones;			//array of bone rigid bodies in player game object



	public bool ragDoll = false;			//condition to enable ragdoll simulation when colliding with game object tagged as obstacle
	public bool jump = false;				//condition indicating user has requested jump

	//on screen debug
	private Text directionText;
	private Text speedText;
	private Text moveText;
	private Text groundedText;

	void Start(){

		kayaRB = GetComponent<Rigidbody> ();					//init player rigid body component\
		kayaCollider = GetComponent<CapsuleCollider>();			//init player collider
		anim = GetComponent<Animator>();						//init player animation component
		cam = Camera.main.transform;							//init main camera transform
		isoCam = Camera.main.GetComponent<IsometricCamera>();	//init isoCam component of main camera

		//debug UI components
		speedText = GameObject.Find("/Canvas/Speed").GetComponent<Text>();
		directionText = GameObject.Find ("/Canvas/Direction").GetComponent<Text> ();
		moveText = GameObject.Find ("/Canvas/MoveVector").GetComponent<Text> ();
		groundedText = GameObject.Find ("/Canvas/Grounded").GetComponent<Text> ();


		//define avatarBones with rigid bodies in player game object

		avatarBones = gameObject.GetComponentsInChildren<Rigidbody>();

		//set avatarBones to kinematic

		foreach(Rigidbody bone in avatarBones){

			bone.isKinematic = true;
		}

		//reset player parent rigid body component to non kinematic 

		kayaRB.isKinematic = false;
		kayaRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

		//debug for iso controller
		//Debug.Log("camera forward vector: " + cam.forward);
		//Debug.Log ("player forward vector: " + transform.forward);
		//turnAssistSpeed = 0.0f;

		mask = ~mask;

	}
		
	void OnCollisionEnter(Collision other){

		//check to see if collision is with an obstacle in the scene
		if (other.gameObject.CompareTag ("Obstacle")) {
			ragDoll = true;
		}

	}

	// Update is called once per frame
	void Update () {
			
		if (!jump){
			
			jump = Input.GetButtonDown ("Jump");
			goForJump (jump);
		}

		if (ragDoll) {

			goForRagdoll ();
		}

	}

	void FixedUpdate(){

		//get user input
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		//set forward and right move vectors for the player relative to the camera 
		mainCamForward = Vector3.Scale (cam.forward, new Vector3 (1.0f, 0.0f, 1.0f)).normalized;
		mainCamRight = cam.right;

		// calculate move direction to pass to character
		moveInput = v * mainCamForward + h * mainCamRight;

		//copied (from milestone 2) input controls for iso cam control
		if (Input.GetKey (KeyCode.Alpha6))
			isoCam.RotateCamera (0);
		if (Input.GetKey (KeyCode.Alpha7))
			isoCam.RotateCamera (1);
		if (Input.GetKey (KeyCode.Alpha8))
			isoCam.RotateCamera (2);
		if (Input.GetKey (KeyCode.Alpha9))
			isoCam.RotateCamera (3);

		//move the player game object
		Move (moveInput);

		//trying to find ground
		//Raycast worked when finally masking ragdoll and player layers
		/*
		RaycastHit hit;

		#if UNITY_EDITOR
		Debug.DrawLine(kayaCollider.bounds.center,new Vector3(kayaCollider.bounds.center.x, kayaCollider.bounds.min.y,kayaCollider.bounds.center.z));
		#endif
	
		if (Physics.Raycast (kayaCollider.bounds.center, -Vector3.up, out hit, 100.0f, mask)) {
			print ("Found an object - distance: " + hit.distance);
			print ("Collider Layer" + LayerMask.LayerToName(hit.transform.gameObject.layer));
		}*/
		playerGrounded ();

	}

	public void Move(Vector3 move){
	
		//normalize the move vector
		if (move.magnitude > 1.0f)
			move = Vector3.Normalize (move);

		//Debug.Log ("updated move vector: " + move);
		//on screen debug 
		moveText.text = "Move: x = "+move.x+" y = "+move.y+" z = "+move.z;

		//convert world relative input move vector to local relative
		move = transform.InverseTransformDirection(move);
		move = Vector3.ProjectOnPlane (move, Vector3.up);	//jumping and incline will need raycast

		//check for player ground condition

		//playerGrounded ();

		//determine player movement - script motion 
		forward = move.z;
		turn = Mathf.Atan2 (move.x, move.z);

		//update mecanim AC parameters
		//move allows for updating the AC using polar coordinates

		animSpeed = Vector3.SqrMagnitude(move); //transition between walk (0.0) and run (1.0)-magnitude not used to improve runtime
		animDirection = turn;			//radians to rotate: +ve - turn right; -ve - turn left

		//on screen debug for what is passed to mecanim
		speedText.text = "Speed: " + forward;
		directionText.text = "Direction: " + turn;

		updateAC(animSpeed,animDirection);
		
	}

	public void goForJump (bool j){

		anim.SetBool ("Jump", j);
		Invoke ("stopJump", 0.1f);
	}

	//transition from anim to ragdoll physics
	public void goForRagdoll(){

		//set avatar Bones to non kinematic objects
		foreach (Rigidbody bone in avatarBones) {
			bone.isKinematic = false;
		}

		//disable animation controller
		anim.enabled = false;

		ragDoll = false;

		StartCoroutine (reloadScene ());

	}

	// update parameters for mecanim AC
	public void updateAC(float s, float d){

		if (Mathf.Abs (d) > 1.5) {
			anim.SetBool ("Pivot", true);
		}
		else {
			anim.SetBool ("Pivot", false);
		}

		anim.SetFloat ("Speed", s, 0.1f, Time.deltaTime);
		anim.SetFloat("Direction", d, 0.1f, Time.deltaTime);
	}

	void stopJump(){

		//anim.SetBool ("Jump", false);
		jump = false;
	}

	bool playerGrounded(){

		bool isGrounded;
	
		Vector3 start = kayaCollider.bounds.center;
		Vector3 end = new Vector3(kayaCollider.bounds.center.x, kayaCollider.bounds.min.y - groundHeight, kayaCollider.bounds.center.z);
		float rad = kayaCollider.radius;

		isGrounded = Physics.CheckCapsule (start, end, rad, mask);

		if(!isGrounded){
			
			Debug.Log("Airborn bitch!");
			groundedText.text = "Grounded: False";

		}else {
			
			groundedText.text = "Grounded: True";

		}
		return isGrounded;
	}

	IEnumerator reloadScene(){
		yield return new WaitForSeconds (3);
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.name);
	}
}
