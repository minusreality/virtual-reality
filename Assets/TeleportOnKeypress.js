public var boundary : Collider;
public var victim : GameObject;
public var levelToLoad : int = 1;

function Update() {
	if(Input.GetKeyDown(KeyCode.T))
	{		
		/*if ((victim.transform.position.x >= boundary.bounds.min.x && victim.transform.position.x <= boundary.bounds.max.x) &&
    	 	(victim.transform.position.z >= boundary.bounds.min.z && victim.transform.position.z <= boundary.bounds.max.z))
		{*/
			if (levelToLoad >= 0) {
				Debug.Log("Loading level: " + levelToLoad);
				LoadLevel(levelToLoad);
			}
			Debug.Log("Within portal collider");
    	 	
		//} 
	}
}

function LoadLevel(levelToLoad : int) {
	//GetComponent(AudioSource).Play();
	//yield WaitForSeconds (GetComponent(AudioSource).clip.length);
	// Notify any clients to also load levels
	Debug.Log("Sending RPC to load level " + levelToLoad);
	networkView.RPC ("RPCLoadLevel", RPCMode.All, levelToLoad);
	//Application.LoadLevel(levelToLoad);
}

// All RPC calls need the @RPC attribute!
@RPC function RPCLoadLevel(level : int)
{
	Debug.Log("Received network notification to load level " + level);
	/*var audio = GameObject.Find("GlassPortal").GetComponent(AudioSource);
	audio.Play();
	yield WaitForSeconds (audio.clip.length);*/
	GetComponent(AudioSource).Play();
	yield WaitForSeconds (GetComponent(AudioSource).clip.length);
	
	// disable vrpn updates
	vrpn = GameObject.Find("Optitrack").GetComponent("VRPNPlugin");	
	vrpn.paused = true;
	Debug.Log("VRPN updates paused");
	
	Application.LoadLevel(level);

}