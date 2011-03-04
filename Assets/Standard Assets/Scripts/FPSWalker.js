var speed = 6.0;
//var speed = 1.0;
var jumpSpeed = 8.0;
//var jumpSpeed = 1.0;
//var gravity = 20.0;
var gravity = 2000000000.0;

private var moveDirection = Vector3.zero;
private var grounded : boolean = false;

function FixedUpdate() {
	var distance;
	
	
	if (grounded) {
		// Get the center of the PhysicalSpace
		physicalSpace = GameObject.Find("PhysicalSpace");
		
		// Get the position of a foot
		leftFoot = GameObject.Find("LeftFoot");
		
		// Calcuate the distance between foot and BOTTOM center
		centerFloor = physicalSpace.transform.position;
		centerFloor.y -= physicalSpace.transform.lossyScale.y / 2;
		distance = Vector3.Distance(centerFloor, leftFoot.transform.position);

		//Debug.Log("distance = " + distance);
		//Debug.Log("x = " + leftFoot.transform.position.x + ", y = " + leftFoot.transform.position.y);
		
		// if the foot is greater than N distance from the center  (along x or z axis) then move that way
		
		
		// We are grounded, so recalculate movedirection directly from axes
		//
		if (distance > .5 ) {
			temp1 = new Vector3(centerFloor.x, 0, centerFloor.z);
			/*if (temp1.x > 0) temp1.x = 1;
			if (temp1.x < 0) temp1.x = -1;
			if (temp1.z > 0) temp1.z = 1;
			if (temp1.z < 0) temp1.z = -1;*/
			temp2 = new Vector3(leftFoot.transform.position.x, 0, leftFoot.transform.position.z);
			/*if (temp2.x > 0) temp2.x = 1;
			if (temp2.x < 0) temp2.x = -1;
			if (temp2.z > 0) temp2.z = 1;
			if (temp2.z < 0) temp2.z = -1;*/
			
			moveDirection = temp2 - temp1; 
			//moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
						
			Debug.Log("Vector x = " + moveDirection.x + ", y = " + moveDirection.y + ", z = " + moveDirection.z);
			moveDirection = transform.TransformDirection(moveDirection);
		    moveDirection *= speed;			
		} else {
			moveDirection = Vector3.zero;
		}
			
		if (Input.GetButton ("Jump")) {
			//moveDirection.y = jumpSpeed;
		}
	}

		Debug.Log("PostTransform x = " + moveDirection.x + ", y = " + moveDirection.y + ", z = " + moveDirection.z);
		// Apply gravity
		moveDirection.y -= gravity * Time.deltaTime;
		//moveDirection *= Time.deltaTime;
		
		Debug.Log("PostGravity x = " + moveDirection.x + ", y = " + moveDirection.y + ", z = " + moveDirection.z);
		
		// Move the controller
		var controller : CharacterController = GetComponent(CharacterController);
		var flags = controller.Move(moveDirection * Time.deltaTime);
		grounded = (flags & CollisionFlags.CollidedBelow) != 0;

	//var fpc = GameObject.Find("First Person Controller Prefab");
	//fpc.transform.position = 
}

@script RequireComponent(CharacterController)