private var motor : CharacterMotor;

// Use this for initialization
function Awake () {
	motor = GetComponent(CharacterMotor);
	
	// Disable this script and enable VRPN if connected
	var vrpn = GameObject.Find("Optitrack").GetComponent("VRPNPlugin");	
	var cam = GameObject.Find("Main Camera");
				
	if (vrpn.enabled == true) {					
		Debug.Log("VRPN Plugin enabled.");
			
		Debug.Log("Disabling Character Motor (WASD controls)...");
		cam.GetComponent(CharacterMotor).enabled = false;
			
		Debug.Log("Disabling FPSInput Controller (WASD controls)...");
		cam.GetComponent(FPSInputController).enabled = false;
	} else {
		Debug.Log("VRPN is not enabled. Keeping mouse look active");
	}	
}

// Update is called once per frame
function Update () {
	// Get the input vector from kayboard or analog stick
	
	var directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
	
	if (directionVector != Vector3.zero) {
		// Get the length of the directon vector and then normalize it
		// Dividing by the length is cheaper than normalizing when we already have the length anyway
		var directionLength = directionVector.magnitude;
		directionVector = directionVector / directionLength;
		
		// Make sure the length is no bigger than 1
		directionLength = Mathf.Min(1, directionLength);
		
		// Make the input vector more sensitive towards the extremes and less sensitive in the middle
		// This makes it easier to control slow speeds when using analog sticks
		directionLength = directionLength * directionLength;
		
		// Multiply the normalized direction vector by the modified length
		directionVector = directionVector * directionLength;
	}
	
	// Apply the direction to the CharacterMotor
	motor.inputMoveDirection = transform.rotation * directionVector;
	motor.inputJump = Input.GetButton("Jump");
	
}

// Require a character controller to be attached to the same game object
@script RequireComponent (CharacterMotor)
@script AddComponentMenu ("Character/FPS Input Controller")
