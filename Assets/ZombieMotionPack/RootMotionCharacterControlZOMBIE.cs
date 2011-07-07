		
using UnityEngine;
using System.Collections;


[AddComponentMenu("Mixamo/Demo/Root Motion Character")]
public class RootMotionCharacterControlZOMBIE: MonoBehaviour
{
	public float turningSpeed = 90f;
	public RootMotionComputer computer;
	public CharacterController character;
	public GameObject victim;
	public string mode = "searching";
	private Transform eye;
	private RaycastHit hit;
	private AIFollow followScript;
	public float updateInterval = 10.0F;
	private double lastInterval, variableInterval;
	public AudioClip[] moanTracks;
	public AudioClip[] runTracks;
	public AudioClip[] attackTracks;
	public AudioClip[] deathTracks;

	
	void Start()
	{
		eye = transform.Find("Hips/Spine/Spine1/Spine2/Neck/Neck1/Head/RightEye");
		followScript = GetComponent(typeof(AIFollow)) as AIFollow;
		
		// validate component references
		if (computer == null) computer = GetComponent(typeof(RootMotionComputer)) as RootMotionComputer;
		if (character == null) character = GetComponent(typeof(CharacterController)) as CharacterController;
		if (victim == null) victim = GameObject.Find("Main Camera");
		if (followScript.target == null) followScript.target = GameObject.Find("Main Camera").transform;
		
		lastInterval = Time.realtimeSinceStartup;
		
		// tell the computer to just output values but not apply motion
		computer.applyMotion = false;
		// tell the computer that this script will manage its execution
		computer.isManagedExternally = true;
		// since we are using a character controller, we only want the z translation output
		computer.computationMode = RootMotionComputationMode.ZTranslation;
		// initialize the computer
		computer.Initialize();
		
		// set up properties for the animations
		animation["idle"].layer = 0; animation["idle"].wrapMode = WrapMode.Loop;
		animation["walk01"].layer = 1; animation["walk01"].wrapMode = WrapMode.Loop;
		animation["run"].layer = 1; animation["run"].wrapMode = WrapMode.Loop;
		animation["attack"].layer = 3; animation["attack"].wrapMode = WrapMode.Once;
		animation["headbutt"].layer = 3; animation["headbutt"].wrapMode = WrapMode.Once;
		animation["scratchidle"].layer = 3; animation["scratchidle"].wrapMode = WrapMode.Once;
		animation["walk02"].layer = 3; animation["walk02"].wrapMode = WrapMode.Once;
		animation["standup"].layer = 3; animation["standup"].wrapMode = WrapMode.Once;
		
		// Load audio
		//audio.clip = Resources.Load("zombie_moan1.mp3") as AudioClip;
		//audio.Play();
		variableInterval = updateInterval;
		
		//animation.Play("idle");
		Walk();
		
	}
	
	void Update()
	{
		if (mode != "dead") {
		// Bit shift the index of the layer (8) to get a bit mask
		LayerMask mask = 1 << 10;
	  	// This would cast rays only against colliders in layer 8.
  		// But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
  		//mask = ~mask;	
		if (!Physics.Linecast(eye.position, victim.transform.position, out hit, mask)) {
			CanSeePlayer();
		} else {
			RandomMoan();
		}
		//float targetMovementWeight = 0f;
		//float throttle = 0f;
		
		// turning keys
		//if (Input.GetKey(KeyCode.A)) transform.Rotate(Vector3.down, turningSpeed*Time.deltaTime);
		//if (Input.GetKey(KeyCode.D)) transform.Rotate(Vector3.up, turningSpeed*Time.deltaTime);
		
		// forward movement keys
		// ensure that the locomotion animations always blend from idle to moving at the beginning of their cycles
		/*if (Input.GetKeyDown(KeyCode.W) && 
			(animation["walk01"].weight == 0f || animation["run"].weight == 0f))
		{
			animation["walk01"].normalizedTime = 0f;
			animation["run"].normalizedTime = 0f;
		}
		if (Input.GetKey(KeyCode.W))
		{
			targetMovementWeight = 1f;
		}
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) throttle = 1f;
				
		// blend in the movement

		animation.Blend("run", targetMovementWeight*throttle, 0.5f);
		animation.Blend("walk01", targetMovementWeight*(1f-throttle), 0.5f);
		// synchronize timing of the footsteps
		animation.SyncLayer(1);*/
		
		// all the other animations, such as punch, kick, attach, reaction, etc. go here
		/*if (Input.GetKeyDown(KeyCode.Alpha1)) animation.CrossFade("attack", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha2)) animation.CrossFade("headbutt", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha3)) animation.CrossFade("scratchidle", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha4)) animation.CrossFade("walk02", 0.2f);
		if (Input.GetKeyDown(KeyCode.Alpha5)) animation.CrossFade("standup", 0.2f);*/
		}		
	}
	
	IEnumerator Wait(float delay) {
        float timer = Time.time + delay;
        while (Time.time < timer) {
            yield return null;
        }
    }
    
	void CanSeePlayer() 
	{
		if (mode == "searching")
			mode = "sawplayer";
		float distance = Vector3.Distance (transform.position, victim.transform.position);	
		//Debug.Log("Zombie distance = " + distance);
		if (distance < 15 && mode == "sawplayer") { // can only start running from "Sawplayer"
			mode = "running";
			Run();
		}
//		if (distance < 10)
//			Debug.Log("Close zombie dist = " + distance);
			
		if (distance < 7 && mode == "running") {
			mode = "attacking";
			Debug.Log("Attacking Player");
			AttackPlayer();
		}
		
		
	}
	
	void AttackPlayer() 
	{
		float targetMovementWeight = 0f;
		float throttle = 0f;
		
		if (animation["attack"].weight == 0f)
		{
			animation["attack"].normalizedTime = 0f;
		}				
		targetMovementWeight = 1f;		
		throttle = 1f;
		// blend in the movement
		//animation.Blend("attack", targetMovementWeight*throttle, 1f);
		animation.CrossFade("attack",1f);	
		//animation.SyncLayer(1);
		
		// Zombie attack sound
		audio.Stop();
        audio.PlayOneShot(attackTracks[Random.Range(0,attackTracks.Length)]);        
                
        MultiConnect multi = GameObject.Find("Multiplayer").GetComponent<MultiConnect>();	
        if (multi.mode == "server") {
			networkView.RPC ("killPlayer", RPCMode.All);	
        }
	}
	
	[RPC]
	void killPlayer() 	
	{
		// The player dies
		Debug.Log("Player killed by zombie!");
		Death deathScript = GameObject.Find("ZombieOverlord").GetComponent<Death>();
		deathScript.enabled = true;		
	}
	
	
	void LateUpdate()
	{
		if (mode != "dead")
			computer.ComputeRootMotion();
		
		// move the character using the computer's output
		//character.SimpleMove(transform.TransformDirection(computer.deltaPosition)/Time.deltaTime);
	}
	
	void Walk() {
		float targetMovementWeight = 0f;
		float throttle = 0f;
		
		if ((animation["walk02"].weight == 0f || animation["run"].weight == 0f))
		{
			animation["walk02"].normalizedTime = 0f;
			animation["run"].normalizedTime = 0f;
		}				
		targetMovementWeight = 1f;		
		//throttle = 1f;
		// blend in the movement
		//animation.Blend("run", targetMovementWeight*throttle, 0.5f);	
		animation.CrossFade("walk01",0.5f);	
		//animation.Blend("walk01", targetMovementWeight*(1f-throttle), 0.5f);
		// synchronize timing of the footsteps
		animation.SyncLayer(1);

	}
	
	void Run() {
		float targetMovementWeight = 0f;
		float throttle = 0f;
		
		if ((animation["walk01"].weight == 0f || animation["run"].weight == 0f))
		{
			animation["walk01"].normalizedTime = 0f;
			animation["run"].normalizedTime = 0f;
		}				
		targetMovementWeight = 1f;		
		throttle = 1f;
		followScript.speed = 3;
		// blend in the movement
		animation.Blend("run", targetMovementWeight*throttle, 1f);
		//animation.CrossFade("run",0.5f);	
		
		// Play aggro sound
		audio.Stop();
        audio.PlayOneShot(runTracks[Random.Range(0,runTracks.Length)]);
		
		// synchronize timing of the footsteps
		animation.SyncLayer(1);

	}
	
	void RandomMoan() { 
		double timeNow = Time.realtimeSinceStartup;
		if( timeNow > lastInterval + variableInterval )
		{
			// play random moan
			audio.Stop();
            audio.PlayOneShot(moanTracks[Random.Range(0,moanTracks.Length)]);
			lastInterval = timeNow;
			variableInterval = updateInterval + Random.Range(0,10);
		}	
	}
	
	public IEnumerator WasHit (float damage) {
		mode = "dead";		
		print ("ouch: " + damage);
		GetComponent<AIFollow>().speed = 0;
		//GetComponent<Seeker>().enabled = false;
		animation.CrossFade("standup", 0.3f);
		//yield return StartCoroutine(Wait(1.0f));		
		audio.Stop();
        audio.PlayOneShot(deathTracks[Random.Range(0,deathTracks.Length)]);
        yield return StartCoroutine(Wait(1.0f));		
        animation.Stop();
		//yield return new WaitForSeconds(3f);
		yield return StartCoroutine(Wait(3.0f));
		Die();
		
		//Walk();
		//animation.CrossFade("walk02",0.5f);
		//yield return new WaitForSeconds(animation["walk01"].clip.length);
		//GetComponent<AIFollow>().enabled = true;

		
		
		// Disable animation
		//animation.Stop();		
		
		// Enable ragdoll		
		/*Component[] rbComponents = GetComponentsInChildren(typeof(Rigidbody)) ;		
		for(int i = 0; i < rbComponents.Length; i++)
		{		    
		    ((Rigidbody)rbComponents[i]).isKinematic = false;
		}*/
		

/*		
		// JavaScript version
		Rigidbody rbs = GetComponentInChildren(typeof(Rigidbody)) as Rigidbody;
		foreach ( Rigidbody rb in rbs ) {
			rb.isKinematic = false; 
		}*/
	}
	
	private void Die() {
		networkView.RPC ("updateOverlord", RPCMode.All);
		Network.Destroy (gameObject);
	}
	
	[RPC]
	void updateOverlord() 	
	{
		Debug.Log("Received network notification of zombie death");
		ZombieOverlordScript overlordScript = GameObject.Find("ZombieOverlord").GetComponent<ZombieOverlordScript>();		
		overlordScript.deadZombie();
	}
}