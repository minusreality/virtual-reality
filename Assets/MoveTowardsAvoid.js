var rayLength : float = 10.0;
var lerpSpeed : float = 1.0;
var theTarget : GameObject;
var mult : float = 1.0;
var speed : float = 1.0;
var rotSpeed : float = .15;

private var rayArray : Vector3[];
private var lerpedTargetDir : Vector3;

function Start(){
    rayArray = new Vector3[3];
}

function Update(){

    MoveTowardsAndAvoid(theTarget.transform);

}

function MoveTowardsAndAvoid(target : Transform){

    var targetPos : Vector3 = target.position;
    targetPos.y = transform.position.y; //set y to zero so we only rotate on one plane
    var targetDir : Vector3 = targetPos - transform.position;

    rayArray[0] = transform.TransformDirection(-0.20,0,0.5); //ray pointed slightly left 
    rayArray[2] = transform.TransformDirection(0.20,0,0.5); //ray pointed slightly right 
    rayArray[1] = transform.forward; //ray 1 is pointed straight ahead

    moveIt = false;

    //loop through the rays
    for (i=0; i<3; i++) {
        var hit : RaycastHit;
        // if you hit something with the ray......
        if (Physics.Raycast (transform.position, rayArray[i], hit, rayLength)) {     

            Debug.DrawLine(transform.position, hit.point, Color.magenta);

            targetDir += mult * hit.normal;
        } else {

            moveIt = true;

        }

    }

    // rotation and movement code 

        lerpedTargetDir = Vector3.Lerp(lerpedTargetDir,targetDir,Time.deltaTime * lerpSpeed);
        var targetRotation = Quaternion.LookRotation(lerpedTargetDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed*Time.deltaTime);

        if(moveIt){     transform.Translate(Vector3.forward*Time.deltaTime*speed);  }

}
