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
	public GameObject PhysicalSpace;
	public Vector3 basePos;
	/*public GameObject leftFoot;
	public GameObject rightFoot;*/
	public GameObject gun;
	public CharacterController charController;
	private Vector3 moveDirection = Vector3.zero;
	
	void Loop()
	{
	}

	void Update()
	{
		VRPNTick();
		/*VRPNTick();
		VRPNTick();
		VRPNTick();*/
		//Debug.Log(PosX(0));
		
		//cam = GameObject.Find("Main Camera");
		
		// Get current HMD position and orientation
		Vector3 pos = cam.transform.position;
		Quaternion ori = cam.transform.rotation;
		
		// Get current foot position and orientation
		/*Vector3 leftFootPos = leftFoot.transform.position;
		Quaternion leftFootOri = leftFoot.transform.rotation;
		Vector3 rightFootPos = rightFoot.transform.position;
		Quaternion rightFootOri = rightFoot.transform.rotation;*/
		
		// Get current gun position and orientation
		Vector3 gunPos = gun.transform.position;
		Quaternion gunOri = gun.transform.rotation;
		
		// Trackables move relative to PhysicalSpace's floor which moves relative to the character controller
		basePos = PhysicalSpace.transform.position;
		basePos.y -= PhysicalSpace.transform.lossyScale.y / 2;
		
		// Move/Orient head
		pos.x = PosX(0)*2+basePos.x;
		pos.y = PosY(0)*2+basePos.y;
		pos.z = -PosZ(0)*2+basePos.z;
		//cam.transform.position = pos; // This will move through walls
		//moveDirection = transform.TransformDirection(pos);
		charController.Move(pos - cam.transform.position);
		
		ori.x  = OriX(0);
		ori.y  = OriY(0);
		ori.z  = -OriZ(0); 
		ori.w = -OriW(0); 
		cam.transform.rotation = ori;
		//transform.localRotation = ori;
		
		// Move/Orient left foot
		/*leftFootPos.x = PosX(1)*2+basePos.x;
		leftFootPos.y = PosY(1)*2+basePos.y;
		leftFootPos.z = -PosZ(1)*2+basePos.z;
		leftFoot.transform.position = leftFootPos;
		
		leftFootOri.x  = OriX(1);
		leftFootOri.y  = OriY(1);
		leftFootOri.z  = -OriZ(1);
		leftFootOri.w = -OriW(1);
		leftFoot.transform.rotation = leftFootOri;
		
		// Move/Orient right foot
		rightFootPos.x = PosX(2)*2+basePos.x;
		rightFootPos.y = PosY(2)*2+basePos.y;
		rightFootPos.z = -PosZ(2)*2+basePos.z;
		rightFoot.transform.position = rightFootPos;
		
		rightFootOri.x  = OriX(2);
		rightFootOri.y  = OriY(2);
		rightFootOri.z  = -OriZ(2);
		rightFootOri.w = -OriW(2);
		rightFoot.transform.rotation = rightFootOri;*/
		
		// Move/Orient gun
		gunPos.x = PosX(3)*2+basePos.x;
		gunPos.y = PosY(3)*2+basePos.y;
		gunPos.z = -PosZ(3)*2+basePos.z;
		gun.transform.position = gunPos;		
		
		gunOri.x  = OriX(3);
		gunOri.y  = OriY(3);
		gunOri.z  = -OriZ(3); // was -
		gunOri.w = -OriW(3); // was -
		gun.transform.rotation = gunOri;
	}

    void Start()
    {
    	// TODO: If you can't connect to VRPN within x seconds then activate keyboard and mouse control
        VRPNStartup();

		PhysicalSpace = GameObject.Find("PhysicalSpace");
		basePos = PhysicalSpace.transform.position;
		basePos.y -= PhysicalSpace.transform.lossyScale.y / 2;
		cam = GameObject.Find("Main Camera");				
		charController = cam.GetComponent<CharacterController>();
		/*leftFoot = GameObject.Find("LeftFoot");
		rightFoot = GameObject.Find("RightFoot");*/
		gun = GameObject.Find("M4");
    }
	
    void Tick()
    {
        VRPNTick();
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

