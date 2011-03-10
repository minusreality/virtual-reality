public var boundary : Collider;
public var victim : GameObject;
public var levelToLoad : int = -1;

function Update() {
	if(Input.GetKeyDown(KeyCode.T))
	{		
		if ((victim.transform.position.x >= boundary.bounds.min.x && victim.transform.position.x <= boundary.bounds.max.x) &&
    	 	(victim.transform.position.z >= boundary.bounds.min.z && victim.transform.position.z <= boundary.bounds.max.z))
		{
			if (levelToLoad >= 0) {
				LoadLevel();
			}
			Debug.Log("Within portal collider");
    	 	
		} 
	}
}

function LoadLevel() {
	GetComponent(AudioSource).Play();
	yield WaitForSeconds (GetComponent(AudioSource).clip.length);
	Application.LoadLevel(levelToLoad);
}