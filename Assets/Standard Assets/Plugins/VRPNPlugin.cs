using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class VRPNPlugin : MonoBehaviour
{
    //Lets make our calls from the Plugin
    [DllImport("OptiTrackPlugin")]
    private static extern void VRPNStartup();
	
    [DllImport("OptiTrackPlugin")]
    private static extern void VRPNTick();
	
    [DllImport("OptiTrackPlugin")]
    private static extern float VRPNPositionX(int n);
	
    [DllImport("OptiTrackPlugin")]
    private static extern float VRPNPositionY(int n);
	
    [DllImport("OptiTrackPlugin")]
    private static extern float VRPNPositionZ(int n);
	
    [DllImport("OptiTrackPlugin")]
    private static extern float VRPNOrientX(int n);
	
    [DllImport("OptiTrackPlugin")]
    private static extern float VRPNOrientY(int n);
	
    [DllImport("OptiTrackPlugin")]
    private static extern float VRPNOrientZ(int n);
	
    [DllImport("OptiTrackPlugin")]
    private static extern float VRPNOrientW(int n);

	public GameObject cam;
	public GameObject gun;	
	public GameObject PhysicalSpace;
	public GameObject MoveDirection;
	public CharacterController charController;	

	public Vector3 basePos;
	public Vector3 runDirection;
	public Quaternion baseOri;
	
	public float YOffset = 0;
	public float YatLastFrame = 0;	
	public float runSpeed = 0;
	public bool paused = false;
	
	private bool freshKeyDown = true;
	private Vector3 rotationPoint;
	
	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
	}

	void Update()
	{
	   if (!paused) {
		VRPNTick();
								
		// Get current HMD position and orientation
		Vector3 pos = cam.transform.position;
		Quaternion ori = cam.transform.rotation;
		Vector3 rot = Vector3.zero;
		Vector3 temp = Vector3.zero;
		Vector3 camOffset = Vector3.zero;
				
		// Get current gun position and orientation
		Vector3 gunPos = gun.transform.position;
		Quaternion gunOri = gun.transform.rotation;
		
		// Trackables move relative to PhysicalSpace's floor which moves relative to the character controller
		basePos = PhysicalSpace.transform.position;
		baseOri = PhysicalSpace.transform.rotation;
		basePos.y -= PhysicalSpace.transform.lossyScale.y / 2; // set the floor as the base Y position, not the center
		
		// Set values for moving/orienting head
		pos.x = PosX(0)*2+basePos.x;
		pos.y = PosY(0)*2+basePos.y;
		pos.z = -PosZ(0)*2+basePos.z;		
		
		ori.x  = OriX(0);						
		ori.y  = OriY(0);		
		ori.z  = -OriZ(0); 
		ori.w = -OriW(0); 		

		// Get resulting euler angles
		rot = ori.eulerAngles;
		
		// Set the move direction		
		MoveDirection.transform.rotation = Quaternion.Euler(0, rot.y, 0);
		
		// Run
		if (Input.GetButton("Fire2")) {
			//YOffset += (rot.y - YatLastFrame);			
			runSpeed = Math.Abs(YatLastFrame - pos.y);
			
			// Get the camera's offset from physical space
			// Move camera in the direction that you would have moved the physical space.
			// Record camera's world position after moving
			// Set the physical space's position to the camera's position (plus offset)
			// Reset the camera to it's position as recorded before moving physical space
			
			// Store the camera's offset from PhysicalSpace
			//camOffset = cam.transform.localPosition;
			camOffset = cam.transform.position - PhysicalSpace.transform.position;
						
			// Move the camera in the run direction
			//charController.Move( charController.transform.position);// + (MoveDirection.transform.forward * runSpeed * 5) );
			charController.Move( MoveDirection.transform.forward * runSpeed * 15 );
			temp = cam.transform.position;
			
			// Set the physical space to the cameras location factoring in the offset of the camera within the physical space.			
		    PhysicalSpace.transform.position = cam.transform.position - camOffset; // blown to space
		    //TODO: FORCE NETWORK UPDATE
			//PhysicalSpace.transform.position = camOffset - cam.transform.position; //slightly off
			//PhysicalSpace.transform.position = camOffset + cam.transform.position; // blown to space
			
			// Reset the camera to it's position as recorded before moving physical space
			cam.transform.position = temp;
			
			// Move the physical space directly
			// PhysicalSpace.transform.position += MoveDirection.transform.forward * runSpeed * 5;			
			
			/*if (freshKeyDown) {
				freshKeyDown = false;				 				
			}*/
			
			
			
			// Adjust physical space depending on rotation
			// This would move the camera and gun but this gets undone because the cam and gun are positioned in world coordinates/rotation afterwards
			// The camera should rotate twice as fast, but the physical space should rotate in the opposite direction at the same rate		
			//PhysicalSpace.transform.RotateAround(rotationPoint, Vector3.up, ((rot.y + (YOffset))*-1) - baseOri.eulerAngles.y ); // right direction, too fast
			//PhysicalSpace.transform.RotateAround(rotationPoint, Vector3.up,(rot.y + (YOffset)) - baseOri.eulerAngles.y);  // wrong direction, right speed
			//PhysicalSpace.transform.RotateAround(rotationPoint, Vector3.up, ((rot.y + (YOffset))*-1) + baseOri.eulerAngles.y ); // wtf
			
		} else {
			//freshKeyDown = true; 
			runSpeed = 0;
		}
		
		// TODO: Align the physical space with it's starting coordinates to correct for rotation errors
		
		YatLastFrame = pos.y;
		//rot.y += YOffset;	
		
		// Set the camera's position
		// This will actually use world coordinates from the tracker so is not relative to the physical space			
		//cam.transform.position = pos; // This will move through walls
	    charController.Move(pos - cam.transform.position); // This will not move through walls
		// TODO: if the camera doesnt move then set the walkable area back to it's last position when it did move
					
		// Set the camera's orientation
		// This will actually use world coordinates from the tracker so is not relative to the physical space
		cam.transform.rotation = ori; // This does not support adjusting the orientation of the volume		
		//cam.transform.eulerAngles = rot;
				
		// Move/Orient gun
		// This will actually use world coordinates from the tracker so is not relative to the physical space
		gunPos.x = PosX(3)*2+basePos.x;
		gunPos.y = PosY(3)*2+basePos.y;
		gunPos.z = -PosZ(3)*2+basePos.z;
		gun.transform.position = gunPos; // Moves through walls	
		
		// This will actually use world coordinates from the tracker so is not relative to the physical space
		gunOri.x  = OriX(3);
		gunOri.y  = OriY(3);
		gunOri.z  = -OriZ(3); 
		gunOri.w = -OriW(3);
		gun.transform.rotation = gunOri;
	   }
	}

    void Start()
    {
        VRPNStartup();
        DontDestroyOnLoad(transform.gameObject);
		LoadSceneObjects();
    }
    
    // Must reload scene objects when scene changes or the references will be to orphans
    void OnLevelWasLoaded()
    {
    	LoadSceneObjects();    	
    	paused = false;
    	Debug.Log("Level load complete. VRPN updates unpaused.");    	
		
		// TODO: fix this ugly hack that makes the gun visible on scene load	
		/*Vector3 temp = Vector3.zero;
		Vector3 camOffset = Vector3.zero;			
		camOffset = cam.transform.position - PhysicalSpace.transform.position;
		charController.Move( MoveDirection.transform.forward * runSpeed * 15 );
		temp = cam.transform.position;			
		PhysicalSpace.transform.position = cam.transform.position - camOffset; // blown to space
		cam.transform.position = temp;*/
    }
    
    void LoadSceneObjects()
    {
    	YOffset = 0;
		YatLastFrame = 0;	
		
		PhysicalSpace = GameObject.Find("PhysicalSpace");
		MoveDirection = GameObject.Find("MoveDirection");
		basePos = PhysicalSpace.transform.position;
		basePos.y -= PhysicalSpace.transform.lossyScale.y / 2;
		cam = GameObject.Find("Main Camera");				
		charController = cam.GetComponent<CharacterController>();
		gun = GameObject.Find("M4");    	
    }
	
    void Tick()
    {
        //VRPNTick();
    }
	
    float PosX(int n)
    {
        return VRPNPositionX(n);
    }
	
    float PosY(int n)
    {
        return VRPNPositionY(n);
    }
	
    float PosZ(int n)
    {
        return VRPNPositionZ(n);
    }

    float OriX(int n)
    {
        return VRPNOrientX(n);
    }
	
    float OriY(int n)
    {
        return VRPNOrientY(n);
    }
	
    float OriZ(int n)
    {
        return VRPNOrientZ(n);
    }
	
    float OriW(int n)
    {
        return VRPNOrientW(n);
    }
}

