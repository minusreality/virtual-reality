var speed = 10.0;
var gravity = 10.0;
var maxVelocityChange = 10.0;
var canJump = true;
var jumpHeight = 2.0;
var lastCenter = Vector3.zero;
private var grounded = false;
private var resetCenter = true;
private var physicalSpace;
private var leftFoot;
private var lastLeft;
private var rightFoot;
private var lastRight;
private var footCenter;

@script RequireComponent(Rigidbody, CapsuleCollider)

function Awake ()
{
    rigidbody.freezeRotation = true;
    rigidbody.useGravity = false;
}

function Start () 
{
	// Hide mouse cursor
	Screen.showCursor = false;
	// Get the center of the PhysicalSpace
	physicalSpace = GameObject.Find("PhysicalSpace");
	
	// Get the position of left foot
	leftFoot = GameObject.Find("LeftFoot");
	lastLeft = GameObject.Find("LastLeft");
	
	// Get the position of right foot
	rightFoot = GameObject.Find("RightFoot");
	lastRight = GameObject.Find("LastRight");
	
	// Get the foot center marker
	footCenter = GameObject.Find("FootCenter");
}

function FixedUpdate ()
{
    if (grounded)
    {
		var targetVelocity = Vector3.zero;

		// Calculate the distance between each foot
		distance = Vector3.Distance(leftFoot.transform.position, rightFoot.transform.position);
		
		// When the distance falls below a certain range, stop walking and persist the center point of both feet
		//Debug.Log("Distance between feet = " + distance);
		
		if (distance <= .9) 
		{
			//if (resetCenter == true)
			if (distance <= .7)
			{
				// Set the last center relative to physical space				
				footCenter.transform.localPosition.x = (leftFoot.transform.localPosition.x + rightFoot.transform.localPosition.x) / 2;
				footCenter.transform.localPosition.y = (leftFoot.transform.localPosition.y + rightFoot.transform.localPosition.y) / 2;
				footCenter.transform.localPosition.z = (leftFoot.transform.localPosition.z + rightFoot.transform.localPosition.z) / 2;
				//lastCenter.y = 0;
				
				//resetCenter = false;
				//Debug.Log("Setting new Center");
				
				// store each foot position
				lastLeft.transform.localPosition = leftFoot.transform.localPosition;
				lastRight.transform.localPosition = rightFoot.transform.localPosition;
			}
			// Stop all movement
			targetVelocity = Vector3.zero;
		} else {
			//resetCenter = true;
			// update the center based on movement 
			
			// get left foot distance from center
			var temp1 = new Vector3(leftFoot.transform.localPosition.x, 0, leftFoot.transform.localPosition.z);
			var leftDistance = Vector3.Distance(temp1, footCenter.transform.localPosition);
			//Debug.Log("left distance = " + leftDistance);
			// get right foot distance from center
			var temp2 = new Vector3(rightFoot.transform.localPosition.x, 0, rightFoot.transform.localPosition.z);
			var rightDistance = Vector3.Distance(temp2, footCenter.transform.localPosition);
			//Debug.Log("right distance = " + rightDistance);
			// Move in the direction created by the vector of the foot farthest from the last center
			if (leftDistance > rightDistance)
			{
			   //targetVelocity = leftFoot.transform.position - lastLeft.transform.position;
			  
			  Debug.Log("Moving from left foot");
			} else {
				//targetVelocity = rightFoot.transform.position - lastRight.transform.position;
				Debug.Log("Moving from Right foot");
			}
			
		}
		
		// Calcuate the distance between foot and BOTTOM center
		//centerFloor = physicalSpace.transform.position;
		//centerFloor.y -= physicalSpace.transform.lossyScale.y / 2;
		//distance = Vector3.Distance(centerFloor, leftFoot.transform.position);
		
		//temp1 = new Vector3(centerFloor.x, 0, centerFloor.z);
		//temp2 = new Vector3(leftFoot.transform.position.x, 0, leftFoot.transform.position.z);
		//var targetVelocity = temp2 - temp1; 
		
        // Calculate how fast we should be moving
        //targetVelocity = transform.TransformDirection(targetVelocity);	// transforms from local space to world space					
        targetVelocity *= speed;
		
       
        // Apply a force that attempts to reach our target velocity
        var velocity = rigidbody.velocity;
        var velocityChange = (targetVelocity - velocity);
        //velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        //velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
		//Debug.Log("velocityChange.x = " + velocityChange.x + ", velocityChange.z = " + velocityChange.z);
        velocityChange.y = 0;
        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
   
        // Jump
        if (canJump && Input.GetButton("Jump"))
        {
            rigidbody.velocity = Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
        }
    }
   
    // We apply gravity manually for more tuning control
    rigidbody.AddForce(Vector3 (0, -gravity * rigidbody.mass, 0));
   
    grounded = false;
}

function OnCollisionStay ()
{
    grounded = true;   
}

function CalculateJumpVerticalSpeed ()
{
    // From the jump height and gravity we deduce the upwards speed
    // for the character to reach at the apex.
    return Mathf.Sqrt(2 * jumpHeight * gravity);
}