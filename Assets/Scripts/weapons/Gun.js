#pragma strict
#pragma implicit
#pragma downcast

enum FireType
{
	RAYCAST,
	PHYSIC_PROJECTILE,
}

enum FireMode
{
	SEMI_AUTO,
	FULL_AUTO,
	BURST
}

class Gun extends MonoBehaviour
{
	public var gunName : String;
	public var bulletMark : GameObject;
	public var projectilePrefab : GameObject;
	
	public var weaponTransformReference : Transform;
	public var frontSightReference : Transform;
	
	public var hitLayer : LayerMask;
	
	public var woodParticle : GameObject;
	public var metalParticle : GameObject;
	public var concreteParticle : GameObject;
	public var sandParticle : GameObject;
	public var waterParticle : GameObject;

	//How many shots the gun can take in one second
	public var fireRate : float;
	public var useGravity : boolean;
	private var fireType : FireType;
	public var fireMode : FireMode;
	
	//Number of shoots to fire when on burst mode
	public var burstRate : int;
	
	//Range of fire in meters
	public var fireRange : float;
	
	//Speed of the projectile in m/s
	public var projectileSpeed : float;
	
	public var clipSize : int;
	public var totalClips : int;
	
	//Time to reload the weapon in seconds
	public var reloadTime : float;
	public var autoReload : boolean;
	public var currentRounds : int;
	
	public var shootVolume : float = 0.4;
	public var shootSound : AudioClip;
	private var shootSoundSource : AudioSource;
	
	public var reloadSound : AudioClip;
	private var reloadSoundSource : AudioSource;
	
	public var outOfAmmoSound : AudioClip;
	private var outOfAmmoSoundSource : AudioSource;
	
	private var reloadTimer : float;
	
	@HideInInspector
	public var freeToShoot : boolean;
	
	@HideInInspector
	public var reloading : boolean;
	private var lastShootTime : float;
	private var shootDelay : float;
	private var cBurst : int;
	
	@HideInInspector
	public var fire : boolean;
	public var hitParticles : GameObject;
	
	public var shotingEmitter : GunParticles;
	private var shottingParticles : Transform;
	
	public var capsuleEmitter : ParticleEmitter[];
	
	public var shotLight : ShotLight;
	
	public var unlimited : boolean = true;
	
	private var timerToCreateDecal : float;
	
	public var pushPower : float = 3.0;
	
	//public var soldierCamera : SoldierCamera;
	public var cam : Camera;
	
	function OnDisable()
	{
		Debug.Log("Disabled!");
		if(shotingEmitter != null)
		{
			shotingEmitter.ChangeState(false);
		}
		
		if(capsuleEmitter != null)
		{
			for(var i : int = 0; i < capsuleEmitter.Length; i++)
			{
				if (capsuleEmitter[i] != null)
                    capsuleEmitter[i].emit = false;
			}
		}
		
		if(shotLight != null)
		{
			shotLight.enabled = false;
		}
	}
	
	function OnEnable()
	{
		Debug.Log("Enabled");
		//cam = GameObject.Find("Main Camera") as Camera;
		
		reloadTimer = 0.0;
		reloading = false;
		freeToShoot = true;
		shootDelay = 1.0 / fireRate;
		
		cBurst = burstRate;
		
		totalClips--;
		currentRounds = clipSize;
		
		if(projectilePrefab != null)
		{
			fireType = FireType.PHYSIC_PROJECTILE;
		}
		
		if(shotLight != null)
		{
			shotLight.enabled = false;
		}
		
		shottingParticles = null;
		if(shotingEmitter != null)
		{
			for(var i : int = 0; i < shotingEmitter.transform.childCount; i++)
			{
				if(shotingEmitter.transform.GetChild(i).name == "bullet_trace")
				{
					shottingParticles = shotingEmitter.transform.GetChild(i);
					break;
				}
			}
		}
	}
	
	function ShotTheTarget()
	{
		if(fire && !reloading)
		{
			if(currentRounds > 0)
			{
				if(Time.time > lastShootTime && freeToShoot && cBurst > 0)
				{
					lastShootTime = Time.time + shootDelay;
			
					switch(fireMode)
					{
						case FireMode.SEMI_AUTO:
							freeToShoot = false;
							break;
						case FireMode.BURST:
							cBurst--;
							break;
					}
					
					if(capsuleEmitter != null)
					{
						for(var i : int = 0; i < capsuleEmitter.Length; i++)
						{
							capsuleEmitter[i].Emit();
						}
					}
					
					PlayShootSound();
					
					if(shotingEmitter != null)
					{
						shotingEmitter.ChangeState(true);
						
					}
					
					if(shotLight != null)
					{
						shotLight.enabled = true;
					}
					
					switch(fireType)
					{
						case FireType.RAYCAST:
							//TrainingStatistics.shootsFired++;
							CheckRaycastHit();
							break;
						case FireType.PHYSIC_PROJECTILE:
							//TrainingStatistics.grenadeFired++;
							//LaunchProjectile();
							break;
					}
					
					currentRounds--;
					
					if(currentRounds <= 0)
					{
						Reload();
					}
				}
			}
			else if(autoReload && freeToShoot)
			{
				if(shotingEmitter != null)
				{
					shotingEmitter.ChangeState(false);
				}
				
				if(shotLight != null)
				{
					shotLight.enabled = false;
				}
				
				if(!reloading)
				{
					Reload();
				}
			}
		}
		else
		{
			if(shotingEmitter != null)
			{
				shotingEmitter.ChangeState(false);
			}
			
			if(shotLight != null)
			{
				shotLight.enabled = false;
			}
		}
		fire = false;
	}
	

	
	/*
	function CheckRaycastHit()
	{
		var hit : RaycastHit;
		var glassHit : RaycastHit;
		var camRay : Ray;
		var origin : Vector3;
		var glassOrigin : Vector3;
		var dir : Vector3;
		var glassDir : Vector3;
		
		if(weaponTransformReference == null)
		{
			Debug.Log("no weapon transform");
			camRay = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5, Screen.height * 0.5, 0));
			origin = camRay.origin;
			dir = camRay.direction;
			origin += dir * 0.1;
		}
		else
		{
			Debug.Log("weapon transform found");
			camRay = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5, Screen.height * 0.5, 0));
			  
			origin = weaponTransformReference.position + (weaponTransformReference.right * 0.2);
			
			if(Physics.Raycast(camRay.origin + camRay.direction * 0.1, camRay.direction, hit, fireRange, hitLayer))
			{
				dir = (hit.point - origin).normalized;
				Debug.Log("hit something");
				if(hit.collider.tag == "glass")
				{
					glassOrigin = hit.point + dir * 0.05;
					
					if(Physics.Raycast(glassOrigin, camRay.direction, glassHit, fireRange - hit.distance, hitLayer))
					{
						glassDir = glassHit.point - glassOrigin;
					}
				}
			}
			else
			{
				Debug.Log("Hit nothing");
				dir = weaponTransformReference.forward;
			}
		}
		
		if(shottingParticles != null)
		{
			shottingParticles.rotation = Quaternion.FromToRotation(Vector3.forward, (cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5, Screen.height * 0.5, cam.farClipPlane)) - weaponTransformReference.position).normalized);
		}
		
		if(Physics.Raycast(origin, dir, hit, fireRange, hitLayer))
		{
			Debug.Log("uh...hit something again?");
			hit.collider.gameObject.SendMessage("Hit", hit, SendMessageOptions.DontRequireReceiver);
			GenerateGraphicStuff(hit);
			
			if(hit.collider.tag == "glass")
			{
				if(Physics.Raycast(glassOrigin, glassDir, glassHit, fireRange - hit.distance, hitLayer))
				{
					glassHit.collider.gameObject.SendMessage("Hit", glassHit, SendMessageOptions.DontRequireReceiver);
					GenerateGraphicStuff(glassHit);
				}
			}
		}
	}*/
	
	function CheckRaycastHit()
	{
		var hit : RaycastHit;
		var glassHit : RaycastHit;
		var camRay : Ray;
		var origin : Vector3;
		var glassOrigin : Vector3;
		var dir : Vector3;
		var glassDir : Vector3;
		
		//var wtrTemp : Vector3 = weaponTransformReference.TransformDirection (Vector3.forward);
		//camRay = new Ray(frontSightReference.position, frontSightReference.position - weaponTransformReference.position); 
		camRay = new Ray(weaponTransformReference.position + weaponTransformReference.forward * 0.4, weaponTransformReference.forward);
		origin = weaponTransformReference.position;// + (weaponTransformReference.right * 0.2);
		
		if(Physics.Raycast(camRay.origin/* + camRay.direction * 0.1*/, camRay.direction, hit, fireRange, hitLayer))
		{
			dir = (hit.point - origin).normalized;
			Debug.Log("hit something");
			if(hit.collider.tag == "glass")
			{
				glassOrigin = hit.point + dir * 0.05;
				
				if(Physics.Raycast(glassOrigin, camRay.direction, glassHit, fireRange - hit.distance, hitLayer))
				{
					glassDir = glassHit.point - glassOrigin;
				}
			}
		}
		else
		{
			Debug.Log("Hit nothing");
			dir = weaponTransformReference.forward;
		}
		
		
		if(shottingParticles != null)
		{
			shottingParticles.rotation = weaponTransformReference.rotation;//Quaternion.FromToRotation(Vector3.forward, (cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5, Screen.height * 0.5, cam.farClipPlane)) - weaponTransformReference.position).normalized);
		}
		
		if(Physics.Raycast(camRay.origin, dir, hit, fireRange, hitLayer))//origin, dir, hit, fireRange, hitLayer))
		{
			Debug.Log("uh...hit something again?");
			hit.collider.gameObject.SendMessage("Hit", hit, SendMessageOptions.DontRequireReceiver);
			GenerateGraphicStuff(hit);
			
			if(hit.collider.tag == "glass")
			{
				if(Physics.Raycast(glassOrigin, glassDir, glassHit, fireRange - hit.distance, hitLayer))
				{
					glassHit.collider.gameObject.SendMessage("Hit", glassHit, SendMessageOptions.DontRequireReceiver);
					GenerateGraphicStuff(glassHit);
				}
			}
		}
	}
	
	/*
	function CheckRaycastHit()
	{
		var hit : RaycastHit;
		var glassHit : RaycastHit;
		var camRay : Ray;
		var origin : Vector3;
		var glassOrigin : Vector3;
		var dir : Vector3;
		var glassDir : Vector3;
		
		if(weaponTransformReference == null)
		{
			Debug.Log("No weapon transform reference!");
			camRay = new Ray(weaponTransformReference.position, weaponTransformReference.position.forward); //cam.ScreenPointToRay(new Vector3(Screen.width * 0.5, Screen.height * 0.5, 0));
			origin = camRay.origin;
			dir = camRay.direction;
			origin += dir * 0.1;
		}
		else
		{
			Debug.Log("Weapon transform reference found");
			camRay = new Ray(weaponTransformReference.position, weaponTransformReference.position.forward); //cam.ScreenPointToRay(new Vector3(Screen.width * 0.5, Screen.height * 0.5, 0));
			  
			origin = weaponTransformReference.position;// + (weaponTransformReference.right * 0.2);
			
			if(Physics.Raycast(camRay.origin + camRay.direction * 0.1, camRay.direction, hit, fireRange, hitLayer)) // tip of gun, along direction, hit seems to be pass by ref, range, selective ignore layers
			{
				Debug.Log("Raycast found SOMETHING");
				dir = (hit.point - origin).normalized;
				
				if(hit.collider.tag == "glass")
				{
					glassOrigin = hit.point + dir * 0.05;
					
					if(Physics.Raycast(glassOrigin, camRay.direction, glassHit, fireRange - hit.distance, hitLayer))
					{
						glassDir = glassHit.point - glassOrigin;
					}
				}
			}
			else
			{
				Debug.Log("Gun shooting forward");
				dir = weaponTransformReference.forward;
			}
		}
		
		if(shottingParticles != null)
		{
			Debug.Log("Shoting particles");
			shottingParticles.rotation = Quaternion.FromToRotation(Vector3.forward, weaponTransformReference.position.normalized);
		}
		
		if(Physics.Raycast(origin, dir, hit, fireRange, hitLayer))
		{
			Debug.Log("Hit something");
			hit.collider.gameObject.SendMessage("Hit", hit, SendMessageOptions.DontRequireReceiver);
			GenerateGraphicStuff(hit);
			
			if(hit.collider.tag == "glass")
			{
				if(Physics.Raycast(glassOrigin, glassDir, glassHit, fireRange - hit.distance, hitLayer))
				{
					glassHit.collider.gameObject.SendMessage("Hit", glassHit, SendMessageOptions.DontRequireReceiver);
					GenerateGraphicStuff(glassHit);
				}
			}
		}
	}
	*/
	function GenerateGraphicStuff(hit : RaycastHit)
	{
		var hitType : HitType;				
		var body : Rigidbody = hit.collider.rigidbody;
		
		// Send a message that it was hit
		if (hit.collider.gameObject.transform.root.tag == "zombie"){			
			Debug.Log("hit a zombie!");
			hit.collider.gameObject.transform.root.SendMessage("WasHit", 5.0);//, direction.normalized * pushPower, hit.point);		
		}
		
		if(body == null)
		{
			Debug.Log("body==null");
			if(hit.collider.transform.parent != null)
			{
				body = hit.collider.transform.parent.rigidbody;
			}
		}
		
		if(body != null)
		{
			if(body.gameObject.layer != 10 && !body.gameObject.name.ToLower().Contains("door"))
			{
				Debug.Log("not a door, not layer 10");
				//body.isKinematic = false;
			}
		
			if(!body.isKinematic)
			{
				Debug.Log("not kinematic");
    				var direction : Vector3 = hit.collider.transform.position - weaponTransformReference.position;
				body.AddForceAtPosition(direction.normalized * pushPower, hit.point, ForceMode.Impulse);
			}
		}
		
		var go : GameObject;
		
		var delta : float = -0.02;
		var hitUpDir : Vector3 = hit.normal;
		var hitPoint : Vector3 = hit.point + hit.normal * delta;
		
		Debug.Log("Object hit = " + hit.collider.name + ", material=" + hit.collider.tag);
		
		
		switch(hit.collider.tag)
		{
			case "wood":
				Debug.Log("made of wood");
				hitType = HitType.WOOD;
				go = GameObject.Instantiate(woodParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
				break;
			case "metal":
				hitType = HitType.METAL;
				go = GameObject.Instantiate(metalParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
				break;
			case "car":
				hitType = HitType.METAL;
				go = GameObject.Instantiate(metalParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
				break;
			case "concrete":
				hitType = HitType.CONCRETE;
				go = GameObject.Instantiate(concreteParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
				break;
			case "dirt":
				hitType = HitType.CONCRETE;
				go = GameObject.Instantiate(sandParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
				break;
			case "sand":
				hitType = HitType.CONCRETE;
				go = GameObject.Instantiate(sandParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
				break;
			case "water":
			case "zombie":
				go = GameObject.Instantiate(waterParticle, hitPoint, Quaternion.FromToRotation(Vector3.up, hitUpDir)) as GameObject;
				break;
			default:
				return;
		}
		
		go.layer = hit.collider.gameObject.layer;
		
		if(hit.collider.renderer == null) return;
		
		if(timerToCreateDecal < 0.0 && hit.collider.tag != "water" && hit.collider.tag != "zombie" )
		{
			go = GameObject.Instantiate(bulletMark, hit.point, Quaternion.FromToRotation(Vector3.forward, -hit.normal));
			var bm : BulletMarks = go.GetComponent("BulletMarks");
			bm.GenerateDecal(hitType, hit.collider.gameObject);
			timerToCreateDecal = 0.02;
		}
	}
	
	function Update()
	{
		timerToCreateDecal -= Time.deltaTime;
		
		if(Input.GetKeyDown(KeyCode.F) && currentRounds == 0 && !reloading && freeToShoot)
		{
			PlayOutOfAmmoSound();
		}
		
		if(Input.GetKeyDown(KeyCode.F))
		{
			Debug.Log("Sending shootSemi command");
			networkView.RPC ("shootSemi", RPCMode.All);
			/*freeToShoot = true;
			cBurst = burstRate;
			fire = true;*/
		}
		
		if(Input.GetKeyDown(KeyCode.L))
		{
			if (shotLight.enabled == true)
				shotLight.enabled = false;
			else
				shotLight.enabled = false;
		}
		
		HandleReloading();
		
		ShotTheTarget();
	}
	
	@RPC function shootSemi()
	{
		Debug.Log("Received network notification to shoot in semi mode");
		freeToShoot = true;
	    cBurst = burstRate;
		fire = true;
	}
	
	function HandleReloading()
	{
		if(Input.GetKeyDown(KeyCode.R) && !reloading)
		{
			Reload();
		}
		
		if(reloading)
		{
			reloadTimer -= Time.deltaTime;
			
			if(reloadTimer <= 0.0)
			{
				reloading = false;
				if(!unlimited)
				{
					totalClips--;
				}
				currentRounds = clipSize;
			}
		}
	}
	
	function Reload()
	{
		if(totalClips > 0 && currentRounds < clipSize)
		{
			PlayReloadSound();
			reloading = true;
			reloadTimer = reloadTime;
		}
	}
	
	//---------------AUDIO METHODS--------
	function PlayOutOfAmmoSound()
	{
		audio.PlayOneShot(outOfAmmoSound, 1.5);
	}
	
	function PlayReloadSound()
	{
		audio.PlayOneShot(reloadSound, 1.5);
	}
	
	function PlayShootSound()
	{
		audio.PlayOneShot(shootSound);
	}
}