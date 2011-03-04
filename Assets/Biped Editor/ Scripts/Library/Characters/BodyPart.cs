using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * BodyPart
 * Version: 1.0
 * Date: 2010.10.31
 * Author: Adam Mechtley (http://adammechtley.com)
 * License: Please don't steal or redistribute this code; I have a family. If
 * you did steal it, at least consider making a donation:
 * http://bit.ly/adammechtley_donate
 * 
 * ----------------------------------------------------------------------------
 * Description
 * ----------------------------------------------------------------------------
 * This component is intended to be used with the Biped component. It serves as
 * a container for storing data related to a body part's physical properties
 * when the biped goes ragdoll.
 * 
 * ----------------------------------------------------------------------------
 * Usage
 * ----------------------------------------------------------------------------
 * In general, you will not want to add and configure this component manually.
 * It is recommended that you instead use the interfaces for character setup
 * included in the Biped and BipedHelpers classes.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations
 * ----------------------------------------------------------------------------
 * There is not yet a generic way to paste a joint axis to an opposite part.
 * There is currently no support for rotated colliders (creating them or
 * reflecting orientation to the opposite part).
 * */

// an enum to describe the type of collider the body part should have
public enum ShapeType { None, Box, Capsule, Sphere }
// an enum to describe possible capsule directions
public enum CapsuleDirection {X, Y, Z}

/*
 * A component for describing a body part on a character
 * */
[AddComponentMenu("Characters/Body Part")]
public class BodyPart : MonoBehaviour
{
	// an accessor for the transform component to cache it, mostly for improved editor performance
	public Transform bone
	{
		get
		{
			// will initialize to null if scripts recompile while bone is selected
			if (_bone==null) _bone=transform;
			return _bone;
		}
		set { _bone = value; }
	}
	private Transform _bone;
	
	// used to snapshot the part's transformations - e.g. in bind pose, before going to ragdoll, etc.
	public Quaternion initialRotation
	{
		get
		{
			// will initialize to zero quaternion if scripts recompile while bone is selected
			if (_initialRotation.x==0f &&
				_initialRotation.y==0f &&
				_initialRotation.z==0f &&
				_initialRotation.w==0f)
			{
				StoreInitialRotation();
			}
			return _initialRotation;
		}
	}
	private Quaternion _initialRotation;
	private Vector3 positionSnapshot;
	
	// the mass of the individual body part
	public float mass = 1f;
	// a flag to determine whether or not to use manual center of mass input
	public bool isCenterOfMassManual = false;
	public Vector3 centerOfMass;
	// indicator to other components (e.g. Biped) that this part should have a rigidbody added
	public bool isRigidbody = true;
	
	// the body part's shape - e.g. use to determine what kind of collider to generate for this part
	public ShapeType shapeType = ShapeType.None;
	public CapsuleDirection capsuleDirection = CapsuleDirection.X; // only used for capsule
	public Vector3 shapeCenter = Vector3.zero;
	public Vector3 shapeSize = Vector3.one;
	public Quaternion shapeRotation = Quaternion.identity; // TODO: not yet implemented
	// indicator to other components (e.g., Biped) that this part should have a collider added
	public bool isCollider = true;
	
	// the joint on this part, if any, as well as its properties
	public ConfigurableJoint joint;
	public Vector3 jointAxis = Vector3.right;
	public Vector3 jointSecondaryAxis = Vector3.up;
	public float xMin;
	public float xMax;
	public float yMax;
	public float zMax;
	// the part to which this part will be connected if no argument is passed in ConnectTo
	public BodyPart parentPart;
	
	// the corresponding part on the other side of the body
	public BodyPart oppositePart;
	// containers for storing the values for axes on the opposite side transformed into this part's space
	public Vector3 oppositeRight = Vector3.right;
	public Vector3 oppositeUp = Vector3.up;
	public Vector3 oppositeForward = Vector3.forward;
	
	/*
	 * Initialize the bone
	 * */
	void Awake()
	{
		StoreInitialRotation();
		StorePositionSnapshot();
	}
	
	/*
	 * Store the initial rotation value
	 * */
	public void StoreInitialRotation()
	{
		_initialRotation = bone.localRotation;
	}
	
	/*
	 * Reset the part to its initial rotation value
	 * */
	public void ResetToInitialRotation()
	{
		bone.localRotation = _initialRotation;
	}
	
	/*
	 * Store a snapshot of the part's current position
	 * */
	public void StorePositionSnapshot()
	{
		positionSnapshot = bone.localPosition;
	}
	
	/*
	 * Reset the part to its position snapshot
	 * */
	public void ResetToPositionSnapshot()
	{
		bone.localPosition = positionSnapshot;
	}
	
	/*
	 * Match this part to another, optionally forcing cardinal axis alignment
	 * */
	public void MatchTo(BodyPart other, bool isCardinalAlignment)
	{
		// match the parts to each other
		oppositePart = other;
		other.oppositePart = this;
		
		// error if the bones are on top of each other
		if ((bone.position-other.bone.position).sqrMagnitude == 0f)
		{
			Debug.LogWarning(string.Format("Could not compute corresponding axes for BodyParts {0} and {1} because they are coincident.", name, other.name));
		}
		
		// compute match axes for each part
		Vector3 reflectionDirection = (bone.position-other.bone.position).normalized;
		other.oppositeForward = other.bone.InverseTransformDirection(VectorHelpers.MirrorPointAcrossPlane(bone.forward, reflectionDirection)).normalized;
		other.oppositeRight = other.bone.InverseTransformDirection(VectorHelpers.MirrorPointAcrossPlane(bone.right, reflectionDirection)).normalized;
		other.oppositeUp = other.bone.InverseTransformDirection(VectorHelpers.MirrorPointAcrossPlane(bone.up, reflectionDirection)).normalized;
		oppositeForward = bone.InverseTransformDirection(VectorHelpers.MirrorPointAcrossPlane(other.bone.forward, reflectionDirection)).normalized;
		oppositeRight = bone.InverseTransformDirection(VectorHelpers.MirrorPointAcrossPlane(other.bone.right, reflectionDirection)).normalized;
		oppositeUp = bone.InverseTransformDirection(VectorHelpers.MirrorPointAcrossPlane(other.bone.up, reflectionDirection)).normalized;
		
		// if isCardinalAlignment, then map the nearest cardinal axis as opposed to the raw axis
		if (isCardinalAlignment)
		{
			other.oppositeForward = VectorHelpers.FindNearestCardinalAxis(other.oppositeForward);
			other.oppositeRight = VectorHelpers.FindNearestCardinalAxis(other.oppositeRight);
			other.oppositeUp = VectorHelpers.FindNearestCardinalAxis(other.oppositeUp);
			oppositeForward = VectorHelpers.FindNearestCardinalAxis(oppositeForward);
			oppositeRight = VectorHelpers.FindNearestCardinalAxis(oppositeRight);
			oppositeUp = VectorHelpers.FindNearestCardinalAxis(oppositeUp);
		}
	}
	
	/*
	 * Transform q into the space of the opposite part (e.g., for shapeRotation)
	 * */
//	public Quaternion TransformRotationToOpposite(Quaternion q)
//	{
//		return q;
//	}
	
	/*
	 * Transform p into the space of the opposite part (e.g., for shapeCenter)
	 * */
	public Vector3 TransformPointToOpposite(Vector3 p, bool applyScale)
	{
		if (applyScale)
			return (p.x*bone.lossyScale.x/oppositePart.bone.lossyScale.x)*oppositeRight + 
				(p.y*bone.lossyScale.y/oppositePart.bone.lossyScale.y)*oppositeUp + 
				(p.z*bone.lossyScale.z/oppositePart.bone.lossyScale.z)*oppositeForward;
		else return p.x*oppositeRight + p.y*oppositeUp + p.z*oppositeForward;
	}
	
	/*
	 * Convert this part's shapeSize into the opposite part's space
	 * */
	public void PasteShapeSizeToOpposite(bool applyScale)
	{
		Vector3 s = TransformPointToOpposite(shapeSize, applyScale);
		if (s.sqrMagnitude == 0f) return; // early out if there is invalid scale
		
		oppositePart.shapeSize = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
		if (oppositePart.shapeType == ShapeType.Sphere)
		{
			oppositePart.shapeSize = Vector3.one*VectorHelpers.MaxValue(oppositePart.shapeSize);
		}
		else if (oppositePart.shapeType == ShapeType.Capsule)
		{
			CapsuleDirection currentDirection = oppositePart.capsuleDirection;
			oppositePart.capsuleDirection = capsuleDirection;
			oppositePart.FlipCapsule(currentDirection);
		}
		
	}
	
	/*
	 * Adjust shapeSize automatically when the capsule direction changes
	 * */
	public void FlipCapsule(CapsuleDirection toDirection)
	{
		// early out if there is no flip
		if (toDirection == capsuleDirection) return;
		// swap around shapeSize components as needed
		switch (capsuleDirection)
		{
		case CapsuleDirection.X:
			if (toDirection == CapsuleDirection.Y)
				shapeSize = new Vector3(shapeSize.y, shapeSize.x, shapeSize.z);
			else
				shapeSize = new Vector3(shapeSize.z, shapeSize.y, shapeSize.x);
			break;
		case CapsuleDirection.Y:
			if (toDirection == CapsuleDirection.X)
				shapeSize = new Vector3(shapeSize.y, shapeSize.x, shapeSize.z);
			else
				shapeSize = new Vector3(shapeSize.x, shapeSize.z, shapeSize.y);
			break;
		case CapsuleDirection.Z:
			if (toDirection == CapsuleDirection.X)
				shapeSize = new Vector3(shapeSize.z, shapeSize.y, shapeSize.x);
			else
				shapeSize = new Vector3(shapeSize.x, shapeSize.z, shapeSize.y);
			break;
		}
		capsuleDirection = toDirection;
	}
	
	/*
	 * Adjust shapeSize vector down the dimension of the capsule direction
	 * */
	public void SetCapsuleLength(float length)
	{
		if (capsuleDirection == CapsuleDirection.X) shapeSize.x = length;
		else if (capsuleDirection == CapsuleDirection.Y) shapeSize.y = length;
		else shapeSize.z = length;
	}
	
	/*
	 * Set the shapeCenter of the parent part to midway down the limb if a parent is defined
	 * */
	public void SetParentShapeCenter()
	{
		if (parentPart==null) return;
		
		parentPart.shapeCenter = 0.5f*parentPart.bone.InverseTransformPoint(bone.position);
	}
	
	/*
	 * Add a Rigidbody to the BodyPart
	 * */
	public Rigidbody AddRigidbody()
	{
		// add a Rigidbody if none yet exists
		Rigidbody rb = (rigidbody==null)?gameObject.AddComponent<Rigidbody>():rigidbody;
		
		// don't permit a mass of 0
		rb.mass = (mass>0f)?mass:1f;
		
		// adjust center of mass if specified
		if (isCenterOfMassManual) rb.centerOfMass = centerOfMass;
		
		return rb;
	}
	
	/*
	 * Add a Collider to the body part
	 * */
	public Collider AddCollider()
	{
		// destroy any existing collider
		if (collider != null) Destroy(collider);
		
		// the collider to return
		Collider col = null;
		switch (shapeType)
		{
		case ShapeType.Box:
			// TODO: account for orientation once support is added
			BoxCollider box = gameObject.AddComponent<BoxCollider>();
			box.center = shapeCenter;
			box.size = shapeSize;
			col = box as Collider;
			break;
		case ShapeType.Capsule:
			// TODO: account for orientation once support is added
			CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
			capsule.center = shapeCenter;
			capsule.direction = (int) capsuleDirection;
			switch (capsuleDirection)
			{
			case CapsuleDirection.X:
				capsule.height = shapeSize.x;
				capsule.radius = Mathf.Max(shapeSize.y, shapeSize.z)*0.5f;
				break;
			case CapsuleDirection.Y:
				capsule.height = shapeSize.y;
				capsule.radius = Mathf.Max(shapeSize.x, shapeSize.z)*0.5f;
				break;
			case CapsuleDirection.Z:
				capsule.height = shapeSize.z;
				capsule.radius = Mathf.Max(shapeSize.x, shapeSize.y)*0.5f;
				break;
			}
			col = capsule as Collider;
			break;
		case ShapeType.Sphere:
			SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
			sphere.center = shapeCenter;
			sphere.radius = Mathf.Max(shapeSize.x, shapeSize.y, shapeSize.z) * 0.5f;
			col = sphere as Collider;
			break;
		}
		return col;
	}
	
	/*
	 * Connect this part to its parent part with a joint
	 * NOTE: If maxforce is 0, the ragdoll will crumple like a chump
	 * */
	public ConfigurableJoint ConnectToParent(float resistance, float maxForce)
	{
		if (parentPart==null)
		{
			Debug.LogError("BodyPart has no parent defined.", this);
			return null;
		}
		return ConnectTo(parentPart, resistance, maxForce);
	}
	
	/*
	 * Create a configurable joint on this BodyPart connected to target applying resistance back to the previous pose
	 * NOTE: If maxforce is 0, the ragdoll will crumple like a chump
	 * */
	public ConfigurableJoint ConnectTo(BodyPart target, float resistance, float maxForce)
	{
		// store the current rotation and enter initialRotation to create the joint
		Quaternion q = bone.localRotation;
		bone.localRotation = _initialRotation;
		
		// ensure there is a rigidbody
		AddRigidbody();
		rigidbody.isKinematic = true;
		
		// create the new joint and configure it
		joint = gameObject.AddComponent<ConfigurableJoint>();
		joint.connectedBody = target.AddRigidbody(); // NOTE: this function takes care of validation internally
		joint.axis = jointAxis;
		joint.secondaryAxis = jointSecondaryAxis;
		joint.xMotion = 
		joint.yMotion = 
		joint.zMotion = ConfigurableJointMotion.Locked;
		joint.angularXMotion = 
		joint.angularYMotion = 
		joint.angularZMotion = ConfigurableJointMotion.Limited;
		SoftJointLimit limit = new SoftJointLimit();
		limit.limit = xMin;
		joint.lowAngularXLimit = limit;
		limit.limit = xMax;
		joint.highAngularXLimit = limit;
		limit.limit = yMax;
		joint.angularYLimit = limit;
		limit.limit = zMax;
		joint.angularZLimit = limit;
		joint.rotationDriveMode = RotationDriveMode.Slerp;
		JointDrive drive = new JointDrive();
		drive.mode = JointDriveMode.Position;
		drive.positionSpring = resistance;
		drive.maximumForce = maxForce;
		joint.slerpDrive = drive;
		joint.targetRotation = q*Quaternion.Inverse(initialRotation);
		
		// disable world-space configuration to ensure settings are applied
		joint.configuredInWorldSpace = false;
		
		// return to the current orientation
		bone.localRotation = q;
		rigidbody.isKinematic = false;
		
		return joint;
	}
}