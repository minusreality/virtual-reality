using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * Biped
 * Version: 1.01
 * Date: 2010.12.24
 * Author: Adam Mechtley (http://adammechtley.com)
 * License: Please don't steal or redistribute this code; I have a family. If
 * you did steal it, at least consider making a donation:
 * http://bit.ly/adammechtley_donate
 * 
 * ----------------------------------------------------------------------------
 * Description
 * ----------------------------------------------------------------------------
 * This component stores references to BodyParts on a bipedal character. It
 * provides interfaces for quickly entering and exiting ragdoll without
 * requiring any swapping of prefabs.
 * 
 * ----------------------------------------------------------------------------
 * Usage
 * ----------------------------------------------------------------------------
 * In general, simply add the component to a bipedal character you have made
 * and use the editor GUI controls to quickly set up the character. When you
 * have set up a character properly, simply use the CreateRagdoll() and
 * RemoveRagdoll() methods as needed during gameplay.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations
 * ----------------------------------------------------------------------------
 * Automated setup features assume character is generally in a T- or A-Pose
 * with its feet on the ground, facing the z-axis of the transform with the
 * Biped component added to it.
 * 
 * Biped minimum requirements are:
 	* pelvis
 	* one or more spine bones
 	* head
 	* left and right hip
 	* left and right lower leg
 	* left and right foot
 	* left and right upper arm
 	* left and right forearm
 	* left and right hand
 * Biped optional parts are:
 	* one or more neck bones
 	* head tip
 	* left and right hand tip
 	* left and right toe
 	* left and right toe tip
 * */

/*
 * A component for a biped to collect body parts and facilitate ragdoll creation and destruction
 * */
[AddComponentMenu("Characters/Biped")]
public class Biped : MonoBehaviour
{
	// the character's total mass - used to compute mass of each body part based on human distribution
	public float mass = 80f;
	public bool autoDistributeMass = true;
	
	// allows other scripts to know if the biped is currently in a ragdoll state
	public bool isRagdoll {get{return _isRagdoll;}}
	private bool _isRagdoll = false;
	
	// the animation component, if any, that controls this character
	public Animation anim;
	
	// the naming convention for this character (for automated setup features)
	public BipedNamingConvention namingConvention;
	
	// axial skeleton
	public BodyPart head;
	public BodyPart headTip;
	public BodyPart[] neck = new BodyPart[0];
	public BodyPart[] spine = new BodyPart[1];
	public BodyPart pelvis;
	
	// left arm
	public BodyPart leftCollar;
	public BodyPart leftUpperArm;
	public BodyPart leftForearm;
	public BodyPart leftHand;
	public BodyPart leftHandTip;
	
	// right arm
	public BodyPart rightCollar;
	public BodyPart rightUpperArm;
	public BodyPart rightForearm;
	public BodyPart rightHand;
	public BodyPart rightHandTip;
	
	// left leg
	public BodyPart leftHip;
	public BodyPart leftLowerLeg;
	public BodyPart leftFoot;
	public BodyPart leftToe;
	public BodyPart leftToeTip;
	
	// right leg
	public BodyPart rightHip;
	public BodyPart rightLowerLeg;
	public BodyPart rightFoot;
	public BodyPart rightToe;
	public BodyPart rightToeTip;
	
	// get all the parts
	public BodyPart[] allParts
	{
		get
		{
			// return an array containing all of the parts
			BodyPart[] ret = new BodyPart[24+neck.Length+Mathf.Max(spine.Length, 1)];
			int i=0;
			ret[i]=headTip; i++;
			ret[i]=head; i++;

			foreach (BodyPart part in neck)
			{
				ret[i]=part; i++;
			}
			foreach (BodyPart part in spine)
			{
				ret[i]=part; i++;
			}
			ret[i]=pelvis; i++;
			ret[i]=leftCollar; i++;
			ret[i]=leftUpperArm; i++;
			ret[i]=leftForearm; i++;
			ret[i]=leftHand; i++;
			ret[i]=leftHandTip; i++;
			ret[i]=rightCollar; i++;
			ret[i]=rightUpperArm; i++;
			ret[i]=rightForearm; i++;
			ret[i]=rightHand; i++;
			ret[i]=rightHandTip; i++;
			ret[i]=leftHip; i++;
			ret[i]=leftLowerLeg; i++;
			ret[i]=leftFoot; i++;
			ret[i]=leftToe; i++;
			ret[i]=leftToeTip; i++;
			ret[i]=rightHip; i++;
			ret[i]=rightLowerLeg; i++;
			ret[i]=rightFoot; i++;
			ret[i]=rightToe; i++;
			ret[i]=rightToeTip; i++;
			
			return ret;
		}
	}
	
	// get the length of the spine
	public float spineLength
	{
		get
		{
			float len = 0f;
			// compute length from first spine bone
			for (int i=1; i<spine.Length; i++)
			{
				len += (spine[i].bone.position-spine[i-1].bone.position).magnitude;
			}
			// add length of final segment
			if (neck.Length>0) len += (neck[0].bone.position-spine[spine.Length-1].bone.position).magnitude;
			else if (head!=null) len += (head.bone.position-spine[spine.Length-1].bone.position).magnitude;
			return len;
		}
	}
	
	// get the length of the neck
	public float neckLength
	{
		get
		{
			float len = 0f;
			// compute length from first neck bone
			for (int i=1; i<neck.Length; i++)
			{
				len += (neck[i].bone.position-neck[i-1].bone.position).magnitude;
			}
			// add length of final segment
			len += (head.bone.position-neck[neck.Length-1].bone.position).magnitude;
			return len;
		}
	}
	
	/*
	 * Initialize the biped's parts
	 * */
	void Start()
	{
		// validate minimum requirements
		ValidateMinimumRequirements();
		
		if (autoDistributeMass) DistributeMass();
	}
	
	/*
	 * Ensure that the user has defined the minimum requirents
	 * */
	public bool ValidateMinimumRequirements()
	{
		// validate the minimally required parts
		bool ret = (head!=null && 
			spine!=null && spine.Length > 0 && 
			pelvis!=null && 
			leftUpperArm!=null && 
			leftForearm!=null && 
			leftHand!=null && 
			rightUpperArm!=null && 
			rightForearm!=null && 
			rightHand!=null && 
			leftHip!=null && 
			leftLowerLeg!=null && 
			leftFoot!=null && 
			rightHip!=null && 
			rightLowerLeg!=null && 
			rightFoot!=null);
		
		// if there is a problem, construct a meaningful error message
		if (!ret)
		{
			string missingBones = "";
			if (head==null) missingBones = string.Format("{0} Head", missingBones);
			if (spine==null) missingBones = string.Format("{0} Spine", missingBones);
			if (pelvis==null) missingBones = string.Format("{0} Pelvis", missingBones);
			if (leftUpperArm==null) missingBones = string.Format("{0} LeftUpperArm", missingBones);
			if (leftForearm==null) missingBones = string.Format("{0} LeftForearm", missingBones);
			if (leftHand==null) missingBones = string.Format("{0} LeftHand", missingBones);
			if (rightUpperArm==null) missingBones = string.Format("{0} RightUpperArm", missingBones);
			if (rightForearm==null) missingBones = string.Format("{0} RightForearm", missingBones);
			if (rightHand==null) missingBones = string.Format("{0} RightHand", missingBones);
			if (leftHip==null) missingBones = string.Format("{0} LeftHip", missingBones);
			if (leftLowerLeg==null) missingBones = string.Format("{0} LeftLowerLeg", missingBones);
			if (leftFoot==null) missingBones = string.Format("{0} LeftFoot", missingBones);
			if (rightHip==null) missingBones = string.Format("{0} RightHip", missingBones);
			if (rightLowerLeg==null) missingBones = string.Format("{0} RightLowerLeg", missingBones);
			if (rightFoot==null) missingBones = string.Format("{0} RightFoot", missingBones);
			Debug.LogError(string.Format("The following required parts are missing:{0}", missingBones), this);
		}
		
		// return the status
		return ret;
	}
	
	/*
	 * A convenience function to automatically locate bones based on the Biped's naming convention
	 * */
	public void FindBonesUsingNamingConvention()
	{
		// cache the hierarchy
		Component[] hierarchy = GetComponentsInChildren<Transform>();
		
		// locate all straightforward bones
		head = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.centerPrefix+
			namingConvention.head, hierarchy);
		if (head!=null && head.bone.childCount>0)
		{
			// assume the head tip is the topmost child in the center
			Transform hTip = null;
			foreach (Transform child in head.bone)
			{
				Vector3 pos = transform.InverseTransformPoint(child.position);
				if (pos.y > transform.InverseTransformPoint(head.bone.position).y && 
					Mathf.Abs(pos.x) < 0.01f && 
					(hTip == null || 
					(hTip!=null && (transform.InverseTransformPoint(hTip.position)).y < (transform.InverseTransformPoint(child.position)).y)))
					hTip = child;
			}
			if (hTip!=null) headTip = BipedHelpers.AddPartByName(hTip.name, hierarchy);
		}
		pelvis = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.centerPrefix+
			namingConvention.pelvis, hierarchy);
		leftHip = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.leftPrefix+
			namingConvention.upperLeg, hierarchy);
		leftLowerLeg = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.leftPrefix+
			namingConvention.lowerLeg, hierarchy);
		leftFoot = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.leftPrefix+
			namingConvention.foot, hierarchy);
		rightHip = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.rightPrefix+
			namingConvention.upperLeg, hierarchy);
		rightLowerLeg = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.rightPrefix+
			namingConvention.lowerLeg, hierarchy);
		rightFoot = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.rightPrefix+
			namingConvention.foot, hierarchy);
		leftCollar = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.leftPrefix+
			namingConvention.collar, hierarchy);
		leftUpperArm = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.leftPrefix+
			namingConvention.upperArm, hierarchy);
		leftForearm = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.leftPrefix+
			namingConvention.forearm, hierarchy);
		leftHand = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.leftPrefix+
			namingConvention.hand, hierarchy);
		rightCollar = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.rightPrefix+
			namingConvention.collar, hierarchy);
		rightUpperArm = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.rightPrefix+
			namingConvention.upperArm, hierarchy);
		rightForearm = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.rightPrefix+
			namingConvention.forearm, hierarchy);
		rightHand = BipedHelpers.AddPartByName(namingConvention.characterNamePrefix+
			namingConvention.rightPrefix+
			namingConvention.hand, hierarchy);
		
		// locate bones that need voodoo magic
		Transform currentBone;
		// neck
		if (head!=null)
		{
			ArrayList neckBones = new ArrayList();
			currentBone = head.bone.parent;
			while (currentBone.name.Contains(namingConvention.characterNamePrefix+
				namingConvention.centerPrefix+
				namingConvention.neck))
			{
				neckBones.Add(currentBone.gameObject);
				currentBone = currentBone.parent;
			}
			neckBones.Reverse();
			neck = new BodyPart[neckBones.Count];
			for (int j=0; j<neck.Length; j++)
			{
				BodyPart p = (neckBones[j] as GameObject).GetComponent<BodyPart>();
				neck[j] = (p==null)?(neckBones[j] as GameObject).AddComponent<BodyPart>():p;
			}
		}
		// spine
		if (head!=null)
		{
			ArrayList spineBones = new ArrayList();
			currentBone = head.bone.parent;
			while (!currentBone.name.Contains(namingConvention.characterNamePrefix+
				namingConvention.centerPrefix+
				namingConvention.spine))
				currentBone = currentBone.parent;
			while (currentBone.name.Contains(namingConvention.characterNamePrefix+
				namingConvention.centerPrefix+
				namingConvention.spine))
			{
				spineBones.Add(currentBone.gameObject);
				currentBone = currentBone.parent;
			}
			spineBones.Reverse();
			spine = new BodyPart[spineBones.Count];
			for (int j=0; j<spine.Length; j++)
			{
				BodyPart p = (spineBones[j] as GameObject).GetComponent<BodyPart>();
				spine[j] = (p==null)?(spineBones[j] as GameObject).AddComponent<BodyPart>():p;
			}
		}
		// left toe and tip
		leftToe = BipedHelpers.FindLimbTip(leftFoot, hierarchy, 
			namingConvention.characterNamePrefix+namingConvention.leftPrefix+namingConvention.toe);
		if (leftToe!=null && leftToe.bone.childCount>0)
			leftToeTip = BipedHelpers.AddPartByName(leftToe.bone.GetChild(0).name, hierarchy);
		// right toe and tip
		rightToe = BipedHelpers.FindLimbTip(rightFoot, hierarchy, 
			namingConvention.characterNamePrefix+namingConvention.rightPrefix+namingConvention.toe);
		if (rightToe!=null && rightToe.bone.childCount>0)
			rightToeTip = BipedHelpers.AddPartByName(rightToe.bone.GetChild(0).name, hierarchy);
		// left hand tip
		leftHandTip = BipedHelpers.FindLimbTip(leftHand, hierarchy, 
			namingConvention.characterNamePrefix+namingConvention.leftPrefix+namingConvention.handTip);
		// right hand tip
		rightHandTip = BipedHelpers.FindLimbTip(rightHand, hierarchy, 
			namingConvention.characterNamePrefix+namingConvention.rightPrefix+namingConvention.handTip);
	}
	
	/*
	 * Computes the paramater values for each of the parts in a spine or neck with an optional per-link homogenous offset
	 * e.g., paramaterOffset=0.5f finds the parameter value for the midpoint of each part
	 * parameterOffset=0f finds the paramater value at each transform's pivot point
	 * */
	private float[] ComputeChainParameterValues(ref BodyPart[] parts, float chainLength, float parameterOffset)
	{
		float[] ret = new float[parts.Length];
		float oneOverLen = 1f/chainLength;
		float currentParam = 0f;
		for (int i=0; i<parts.Length; i++)
		{
			float distance = 0f;
			if (i<parts.Length-1)
				distance = (parts[i+1].bone.position-parts[i].bone.position).magnitude*oneOverLen*parameterOffset;
			else
				distance = parameterOffset*(1f-currentParam);
			currentParam += distance;
			ret[i] = currentParam;
		}
		return ret;
	}
	
	/*
	 * Perform a normal human mass distribution and set center of mass for different parts
	 * If neck, feet, toes, or hands have isRigidbody flag set to false, their mass will be
	 * redistributed to their parents, allowing for more simplified ragdolls
	 * */
	public void DistributeMass()
	{
		// validate minimum requirements
		ValidateMinimumRequirements();
		
		// first set all mass values to 0
		foreach (BodyPart part in allParts) if (part!=null) part.mass = 0f;
		
		// axial skeleton
		head.mass += mass * 0.073f;
		// distribute mass up the length of the neck
		float length = neckLength;
		float totalNeckMass = mass * 0.02f;
		// if there is no neck, or if the neck bone is not to be part of the ragdoll, distribute its mass to the final spine bone
		if (neck==null || neck.Length<1 || !neck[neck.Length-1].isRigidbody)
			spine[spine.Length-1].mass += totalNeckMass;
		else
		{
			for (int i=0; i<neck.Length; i++)
			{
				// determine what percentage of the neck this part contributes based on the neck length
				float pctOfNeck = ((i==neck.Length-1) ? 
					((head.bone.position-neck[i].bone.position).magnitude): 
					(neck[i+1].bone.position-neck[i].bone.position).magnitude) / length;
				neck[i].mass += pctOfNeck*totalNeckMass;
			}
		}
		// distribute mass up the length of the spine
		// TODO: take torso center of mass into account
		length = spineLength;
		float totalSpineMass = mass * 0.335f;
		for (int i=0; i<spine.Length; i++)
		{
			// determine what percentage of the torso this part contributes based on the spine length
			float pctOfTorso = ((i==spine.Length-1) ? 
				((neck.Length==0)?(head.bone.position-spine[i].bone.position).magnitude:(neck[0].bone.position-spine[i].bone.position).magnitude) : 
				(spine[i+1].bone.position-spine[i].bone.position).magnitude) / length;
			spine[i].mass += pctOfTorso*totalSpineMass;
		}
		// pelvis
		pelvis.mass += mass * 0.15f;
		
		// legs
		leftHip.mass += mass * 0.103f;
		leftLowerLeg.mass += mass * 0.043f;
		if (leftFoot.isRigidbody) leftFoot.mass += mass * 0.0125f;
		else leftLowerLeg.mass += mass * 0.0125f;
		if (leftToe.isRigidbody) leftToe.mass += mass * 0.0025f;
		else if (leftFoot.isRigidbody) leftFoot.mass += mass * 0.0025f;
		else leftLowerLeg.mass += mass * 0.0025f;
		if (leftToeTip!=null) leftToeTip.mass += 0f;
		rightHip.mass += mass * 0.103f;
		rightLowerLeg.mass += mass * 0.043f;
		if (rightFoot.isRigidbody) rightFoot.mass += mass * 0.0125f;
		else rightLowerLeg.mass += mass * 0.0125f;
		if (rightToe.isRigidbody) rightToe.mass += mass * 0.0025f;
		else if (rightFoot.isRigidbody) rightFoot.mass += mass * 0.0025f;
		else rightLowerLeg.mass += mass * 0.0025f;
		if (rightToeTip!=null) rightToeTip.mass += 0f;
		
		// arms
		leftUpperArm.mass += mass * 0.026f;
		leftForearm.mass += mass * 0.016f;
		if (leftHand.isRigidbody) leftHand.mass += mass * 0.007f;
		else leftForearm.mass += mass * 0.007f;
		if (leftHandTip!=null) leftHandTip.mass += 0f;
		rightUpperArm.mass += mass * 0.026f;
		rightForearm.mass += mass * 0.016f;
		if (rightHand.isRigidbody) rightHand.mass += mass * 0.007f;
		else rightForearm.mass += mass * 0.007f;
		if (rightHandTip!=null) rightHandTip.mass += 0f;
		
		// adjust center of mass on each part
		if (leftCollar!=null) leftCollar.centerOfMass = leftCollar.bone.InverseTransformPoint(leftUpperArm.bone.position)*0.5f;
		leftUpperArm.centerOfMass = leftUpperArm.bone.InverseTransformPoint(leftForearm.bone.position)*0.513f;
		leftForearm.centerOfMass = leftForearm.bone.InverseTransformPoint(leftHand.bone.position)*0.039f;
		if (rightCollar!=null) rightCollar.centerOfMass = rightCollar.bone.InverseTransformPoint(rightUpperArm.bone.position)*0.5f;
		rightUpperArm.centerOfMass = rightUpperArm.bone.InverseTransformPoint(rightForearm.bone.position)*0.513f;
		rightForearm.centerOfMass = rightForearm.bone.InverseTransformPoint(rightHand.bone.position)*0.039f;
		leftHip.centerOfMass = leftHip.bone.InverseTransformPoint(leftLowerLeg.bone.position)*0.372f;
		leftLowerLeg.centerOfMass = leftLowerLeg.bone.InverseTransformPoint(leftFoot.bone.position)*0.0371f;
		rightHip.centerOfMass = rightHip.bone.InverseTransformPoint(rightLowerLeg.bone.position)*0.372f;
		rightLowerLeg.centerOfMass = rightLowerLeg.bone.InverseTransformPoint(rightFoot.bone.position)*0.0371f;
		
		// mark each part that needs to use its specificed center of mass
		if (leftCollar) leftCollar.isCenterOfMassManual = true;
		if (rightCollar) rightCollar.isCenterOfMassManual = true;
		leftUpperArm.isCenterOfMassManual = 
			leftForearm.isCenterOfMassManual = 
			rightUpperArm.isCenterOfMassManual = 
			rightForearm.isCenterOfMassManual = 
			leftHip.isCenterOfMassManual = 
			leftLowerLeg.isCenterOfMassManual = 
			leftFoot.isCenterOfMassManual = 
			rightHip.isCenterOfMassManual = 
			rightLowerLeg.isCenterOfMassManual = 
			rightFoot.isCenterOfMassManual = true;
	}
	
	/*
	 * Match parts to their corresponding parts on the opposite side
	 * */
	public void MapSymmetry(bool isCardinalAlignment)
	{
		// validate minimum requirements
		ValidateMinimumRequirements();
		
		if (leftCollar!=null) leftCollar.MatchTo(rightCollar, isCardinalAlignment);
		leftUpperArm.MatchTo(rightUpperArm, isCardinalAlignment);
		leftForearm.MatchTo(rightForearm, isCardinalAlignment);
		leftHand.MatchTo(rightHand, isCardinalAlignment);
		if (leftHandTip!=null) leftHandTip.MatchTo(rightHandTip, isCardinalAlignment);
		leftHip.MatchTo(rightHip, isCardinalAlignment);
		leftLowerLeg.MatchTo(rightLowerLeg, isCardinalAlignment);
		leftFoot.MatchTo(rightFoot, isCardinalAlignment);
		if (leftToe!=null) leftToe.MatchTo(rightToe, isCardinalAlignment);
		if (leftToeTip!=null) leftToeTip.MatchTo(rightToeTip, isCardinalAlignment);
	}
	
	/*
	 * Set default collider shapes and sizes
	 * */
	public void SetDefaultColliders()
	{
		SetDefaultColliderShapes();
		SetDefaultColliderSizes();
	}
	
	/*
	 * Set default collider shapes for a body parts component
	 * */
	public void SetDefaultColliderShapes()
	{
		// validate minimum requirements
		ValidateMinimumRequirements();
		
		foreach (BodyPart part in allParts) if (part!=null) part.shapeType = ShapeType.Capsule;
		head.shapeType = ShapeType.Sphere;
		if (neck.Length>0) foreach (BodyPart part in neck) part.shapeType = ShapeType.Sphere;
		foreach (BodyPart part in spine) if (part!=null) part.shapeType = ShapeType.Sphere;
		pelvis.shapeType = ShapeType.Sphere;
		if (leftCollar!=null) leftCollar.shapeType = ShapeType.None;
		leftHand.shapeType = ShapeType.Box;
		if (rightCollar!=null) rightCollar.shapeType = ShapeType.None;
		rightHand.shapeType = ShapeType.Box;
		leftFoot.shapeType = ShapeType.Box;
		if (leftToe!=null) leftToe.shapeType = ShapeType.Box;
		rightFoot.shapeType = ShapeType.Box;
		if (rightToe!=null) rightToe.shapeType = ShapeType.Box;
		if (headTip!=null) headTip.shapeType = ShapeType.None;
		if (leftHandTip!=null) leftHandTip.shapeType = ShapeType.None;
		if (rightHandTip!=null) rightHandTip.shapeType = ShapeType.None;
		if (leftToeTip!=null) leftToeTip.shapeType = ShapeType.None;
		if (rightToeTip!=null) rightToeTip.shapeType = ShapeType.None;
	}
	
	/*
	 * Attempt to set default collider sizes
	 * */
	public void SetDefaultColliderSizes()
	{
		SetParentParts();
		
		// cache the biped's scale
		Vector3 scale = transform.localScale;
		transform.localScale = Vector3.one;
		
		// first determine capsule directions
		foreach (BodyPart part in allParts)
		{
			if (part==null || part.parentPart==null) continue;
			
			// determine the direction vector of the part's parent
			Vector3 nearestCardinal = VectorHelpers.FindNearestCardinalAxis(
				part.parentPart.bone.InverseTransformPoint(part.bone.position));
			if (Mathf.Abs(Vector3.Dot(nearestCardinal, Vector3.forward)) == 1f)
				part.parentPart.capsuleDirection = CapsuleDirection.Z;
			else if (Mathf.Abs(Vector3.Dot(nearestCardinal, Vector3.up)) == 1f)
				part.parentPart.capsuleDirection = CapsuleDirection.Y;
			else
				part.parentPart.capsuleDirection = CapsuleDirection.X;
		}
		
		// determine approximate height and thickness
		Component[] skins = GetComponentsInChildren<SkinnedMeshRenderer>();
		float height = 0f;
		foreach (SkinnedMeshRenderer skin in skins)
			if (skin.bounds.max.y>height) height = skin.bounds.max.y;
		if (skins.Length < 1)
			height = 1.85f*transform.InverseTransformPoint(pelvis.bone.position).magnitude; // assume that pelvis is roughly halfway point
		
		// set defaults based on normal human proportions
		// axial skeleton
		head.shapeSize = Vector3.one*height*0.13f;
		head.shapeCenter = 0.3f*head.bone.InverseTransformPoint(new Vector3(head.bone.position.x, height, head.bone.position.z));
		head.shapeCenter = VectorHelpers.FindNearestCardinalAxis(head.shapeCenter) * head.shapeCenter.magnitude;
		foreach (BodyPart part in neck)
		{
			part.shapeSize = Vector3.one*height*0.075f/neck.Length;
			part.SetParentShapeCenter();
			part.isCollider = false;
		}
		// a curve to decsribe how the width changes down the parameter of the spine
		AnimationCurve spineCountour = new AnimationCurve(
			new Keyframe[2] {new Keyframe(0f,0.7f), new Keyframe(1f,1f)});
		float[] parameterValues = ComputeChainParameterValues(ref spine, spineLength, 0.7f);
		for (int i=0; i<spine.Length; i++)
		{
			spine[i].shapeSize = Vector3.one*height*0.2f*spineCountour.Evaluate(parameterValues[i]);
			spine[i].SetParentShapeCenter();
			// scoot the center back a little
			if (i>0) spine[i-1].shapeCenter *= 0.75f; // previous part
			spine[i].shapeCenter *= 0.75f; // current part - for last link
		}
		pelvis.shapeSize = Vector3.one*height*0.18f;
		pelvis.shapeCenter = Vector3.zero;
		
		// legs
		leftHip.shapeSize = Vector3.one*height*0.085f;
		leftHip.SetCapsuleLength(1.15f*Vector3.Distance(leftLowerLeg.bone.position, leftHip.bone.position));
		leftLowerLeg.SetParentShapeCenter();
		leftLowerLeg.shapeSize = Vector3.one*height*0.0675f;
		leftLowerLeg.SetCapsuleLength(1.15f*Vector3.Distance(leftFoot.bone.position, leftLowerLeg.bone.position));
		leftFoot.SetParentShapeCenter();
		leftFoot.shapeSize = Vector3.one*height*0.05f;
		leftFoot.SetCapsuleLength(1.2f*leftFoot.shapeSize.magnitude);
		if (leftToe!=null)
		{
			leftToe.SetParentShapeCenter();
			leftToe.shapeSize = Vector3.one*height*0.05f;
			leftToe.isCollider = false;
			if (leftToeTip!=null) leftToeTip.SetParentShapeCenter();
		}
		
		rightHip.shapeSize = Vector3.one*height*0.085f;
		rightHip.SetCapsuleLength(1.15f*Vector3.Distance(rightLowerLeg.bone.position, rightHip.bone.position));
		rightLowerLeg.SetParentShapeCenter();
		rightLowerLeg.shapeSize = Vector3.one*height*0.0675f;
		rightLowerLeg.SetCapsuleLength(1.15f*Vector3.Distance(rightFoot.bone.position, rightLowerLeg.bone.position));
		rightFoot.SetParentShapeCenter();
		rightFoot.shapeSize = Vector3.one*height*0.05f;
		rightFoot.SetCapsuleLength(1.2f*rightFoot.shapeSize.magnitude);
		if (rightToe!=null)
		{
			rightToe.SetParentShapeCenter();
			rightToe.shapeSize = Vector3.one*height*0.05f;
			rightToe.isCollider = false;
			if (rightToeTip!=null) rightToeTip.SetParentShapeCenter();
		}
		
		// arms
		if (leftCollar!=null)
		{
			leftCollar.shapeSize = Vector3.one*height*0.08f;
			leftCollar.SetCapsuleLength(0.8f*leftCollar.shapeSize.magnitude);
			leftUpperArm.SetParentShapeCenter();
		}
		leftUpperArm.shapeSize = Vector3.one*height*0.0675f;
		leftUpperArm.SetCapsuleLength(1.15f*Vector3.Distance(leftForearm.bone.position, leftUpperArm.bone.position));
		leftForearm.SetParentShapeCenter();
		leftForearm.shapeSize = Vector3.one*height*0.0575f;
		leftForearm.SetCapsuleLength(1.15f*Vector3.Distance(leftHand.bone.position, leftForearm.bone.position));
		leftHand.SetParentShapeCenter();
		leftHand.shapeSize = Vector3.one*height*0.0675f;
		leftHand.shapeCenter = VectorHelpers.FindNearestCardinalAxis(
			leftHand.bone.InverseTransformDirection(leftHand.bone.position-leftForearm.bone.position)
			)*leftHand.shapeSize.x*0.5f;
		Vector3 thickness = Vector3.Cross(
			VectorHelpers.FindNearestCardinalAxis(leftHand.bone.InverseTransformDirection(transform.forward)),
			VectorHelpers.FindNearestCardinalAxis(leftHand.bone.InverseTransformDirection(leftHand.bone.position-leftForearm.bone.position))
		);
		thickness.x = Mathf.Abs(thickness.x);
		thickness.y = Mathf.Abs(thickness.y);
		thickness.z = Mathf.Abs(thickness.z);
		leftHand.shapeSize -= 0.5f*leftHand.shapeSize.x*thickness;
		
		if (rightCollar!=null)
		{
			rightCollar.shapeSize = Vector3.one*height*0.08f;
			rightCollar.SetCapsuleLength(0.8f*rightCollar.shapeSize.magnitude);
			rightUpperArm.SetParentShapeCenter();
		}
		rightUpperArm.shapeSize = Vector3.one*height*0.0675f;
		rightUpperArm.SetCapsuleLength(1.15f*Vector3.Distance(rightForearm.bone.position, rightUpperArm.bone.position));
		rightForearm.SetParentShapeCenter();
		rightForearm.shapeSize = Vector3.one*height*0.0575f;
		rightForearm.SetCapsuleLength(1.15f*Vector3.Distance(rightHand.bone.position, rightForearm.bone.position));
		rightHand.SetParentShapeCenter();
		rightHand.shapeSize = Vector3.one*height*0.0675f;
		rightHand.shapeCenter = VectorHelpers.FindNearestCardinalAxis(
			rightHand.bone.InverseTransformDirection(rightHand.bone.position-rightForearm.bone.position)
			)*rightHand.shapeSize.x*0.5f;
		thickness = Vector3.Cross(
			VectorHelpers.FindNearestCardinalAxis(rightHand.bone.InverseTransformDirection(transform.forward)),
			VectorHelpers.FindNearestCardinalAxis(rightHand.bone.InverseTransformDirection(rightHand.bone.position-rightForearm.bone.position))
		);
		thickness.x = Mathf.Abs(thickness.x);
		thickness.y = Mathf.Abs(thickness.y);
		thickness.z = Mathf.Abs(thickness.z);
		rightHand.shapeSize -= 0.5f*rightHand.shapeSize.x*thickness;
		
		// try to clean up dense spine colliders
		float overlap = ComputeSphereColliderOverlap(head, spine[spine.Length-1]);
		if (neck.Length>0 && overlap>0f)
		{
			// if the overlap is minimal, then just scale down the collider
			if (overlap<height*0.01f)
				spine[spine.Length-1].shapeSize *= 1f-4f*overlap/spine[spine.Length-1].shapeSize.x;
			// otherwise disable the collider
			else
				spine[spine.Length-1].isCollider = false;
		}
		overlap = ComputeSphereColliderOverlap(spine[0], pelvis);
		// disable the first spine collider if more than 80 percent of its diameter overlaps the pelvis collider
		if (overlap/spine[0].shapeSize.x > 0.8f)
			spine[0].isCollider = false;
		/*for (int i=1; i<spine.Length; i++)
		{
			if (!spine[i].isCollider) continue;
			BodyPart currentParent = spine[i].parentPart.parentPart;
			while (currentParent!=null && spine[i].isCollider)
			{
				if (currentParent.isCollider)
				{
					// search back to see if there is overlap
					overlap = ComputeSphereColliderOverlap(spine[i], currentParent);
					Debug.Log(overlap);
					// if the overlap is minimal, then just scale down the collider
					if (overlap<height*0.01f)
						spine[i].shapeSize *= 1f-4f*overlap/spine[i].shapeSize.x;
					// otherwise disable the collider
					else
						spine[i].isCollider = false;
				}
				// examine next parent to look for overlap
				currentParent = currentParent.parentPart;
			}
		}*/
		
		// reset the scale
		transform.localScale = scale;
	}
	
	/*
	 * Return the amount of overlap between two sphere colliders - used to determine if they are too dense
	 * */
	private float ComputeSphereColliderOverlap(BodyPart p1, BodyPart p2)
	{
		float distance = Vector3.Distance(p1.bone.TransformPoint(p1.shapeCenter), p2.bone.TransformPoint(p2.shapeCenter));
		return (p1.shapeSize.x+p2.shapeSize.x)*0.5f-distance;
	}
	
	/*
	 * Set default joint axes and limits
	 * */
	public void SetDefaultJoints(bool useCardinalAxes)
	{
		SetDefaultJointAxes(useCardinalAxes);
		SetDefaultJointLimits();
	}
	
	/*
	 * Attempt to determine the jointAxis on each BodyPart
	 * */
	public void SetDefaultJointAxes(bool useCardinalAxes)
	{
		SetParentParts();
		
		// container for working
		AxisTripod tripod;
		
		// head axis is front-back, secondary is side-to-side
		head.jointAxis = head.bone.InverseTransformDirection(-transform.right);
		head.jointSecondaryAxis = head.bone.InverseTransformDirection(transform.forward);
		// neck axis is front-back, secondary is side-to-side
		foreach (BodyPart part in neck)
		{
			part.jointAxis = part.bone.InverseTransformDirection(-transform.right);
			part.jointSecondaryAxis = part.bone.InverseTransformDirection(transform.forward);
		}
		// spine axis is front-back, secondary is side-to-side
		foreach (BodyPart part in spine)
		{
			part.jointAxis = part.bone.InverseTransformDirection(-transform.right);
			part.jointSecondaryAxis = part.bone.InverseTransformDirection(transform.forward);
		}
		// left collar
		if (leftCollar!=null)
		{
			leftCollar.jointAxis = leftCollar.bone.InverseTransformDirection(Vector3.forward);
			leftCollar.jointSecondaryAxis = leftCollar.bone.InverseTransformDirection(Vector3.up);
		}
		// left upper arm axis is pectoral flexion, secondary is adduction/abduction
		tripod = VectorHelpers.AxisTripodFromForwardUp(leftForearm.bone.position-leftUpperArm.bone.position, transform.forward);
		leftUpperArm.jointAxis = leftUpperArm.bone.InverseTransformDirection(tripod.left);
		leftUpperArm.jointSecondaryAxis = leftUpperArm.bone.InverseTransformDirection(tripod.down);
		// left forearm axis is flexion
		tripod = VectorHelpers.AxisTripodFromForwardUp(leftHand.bone.position-leftForearm.bone.position, transform.forward);
		leftForearm.jointAxis = leftForearm.bone.InverseTransformDirection(tripod.left);
		leftForearm.jointSecondaryAxis = leftForearm.bone.InverseTransformDirection(tripod.down);
		// left hand axis is flexion, secondary is lateral motion
		tripod = VectorHelpers.AxisTripodFromForwardUp(leftHand.bone.position-leftForearm.bone.position, transform.forward);
		leftHand.jointAxis = leftHand.bone.InverseTransformDirection(tripod.up);
		leftHand.jointSecondaryAxis = leftHand.bone.InverseTransformDirection(tripod.left);
		// right collar
		if (rightCollar!=null)
		{
			rightCollar.jointAxis = rightCollar.bone.InverseTransformDirection(Vector3.back);
			rightCollar.jointSecondaryAxis = rightCollar.bone.InverseTransformDirection(Vector3.up);
		}
		// right upper arm axis is pectoral flexion, secondary is adduction/abduction
		tripod = VectorHelpers.AxisTripodFromForwardUp(rightForearm.bone.position-rightUpperArm.bone.position, transform.forward);
		rightUpperArm.jointAxis = rightUpperArm.bone.InverseTransformDirection(tripod.left);
		rightUpperArm.jointSecondaryAxis = rightUpperArm.bone.InverseTransformDirection(tripod.down);
		// right forearm axis is flexion
		tripod = VectorHelpers.AxisTripodFromForwardUp(rightHand.bone.position-rightForearm.bone.position, transform.forward);
		rightForearm.jointAxis = rightForearm.bone.InverseTransformDirection(tripod.left);
		rightForearm.jointSecondaryAxis = rightForearm.bone.InverseTransformDirection(tripod.down);
		// right hand axis is flexion, secondary is lateral motion
		tripod = VectorHelpers.AxisTripodFromForwardUp(rightHand.bone.position-rightForearm.bone.position, transform.forward);
		rightHand.jointAxis = rightHand.bone.InverseTransformDirection(tripod.down);
		rightHand.jointSecondaryAxis = rightHand.bone.InverseTransformDirection(tripod.right);
		// left upper leg axis is gluteal flexion, secondary is adduction/abduction
		tripod = VectorHelpers.AxisTripodFromForwardUp(leftLowerLeg.bone.position-leftHip.bone.position, transform.right);
		leftHip.jointAxis = leftHip.bone.InverseTransformDirection(tripod.up);
		leftHip.jointSecondaryAxis = leftHip.bone.InverseTransformDirection(tripod.left);
		// left lower leg axis is flexion
		tripod = VectorHelpers.AxisTripodFromForwardUp(leftFoot.bone.position-leftLowerLeg.bone.position, transform.right);
		leftLowerLeg.jointAxis = leftLowerLeg.bone.InverseTransformDirection(tripod.up);
		leftLowerLeg.jointSecondaryAxis = leftLowerLeg.bone.InverseTransformDirection(tripod.left);
		// left foot
		tripod = VectorHelpers.AxisTripodFromForwardUp(transform.forward, transform.right);
		leftFoot.jointAxis = leftFoot.bone.InverseTransformDirection(tripod.up);
		leftFoot.jointSecondaryAxis = leftFoot.bone.InverseTransformDirection(tripod.left);
		// left toe
		if (leftToe!=null)
		{
			leftToe.jointAxis = leftToe.bone.InverseTransformDirection(tripod.up);
			leftToe.jointSecondaryAxis = leftToe.bone.InverseTransformDirection(tripod.left);
		}
		// right upper leg axis is gluteal flexion, secondary is adduction/abduction
		tripod = VectorHelpers.AxisTripodFromForwardUp(rightLowerLeg.bone.position-rightHip.bone.position, transform.right);
		rightHip.jointAxis = rightHip.bone.InverseTransformDirection(tripod.up);
		rightHip.jointSecondaryAxis = rightHip.bone.InverseTransformDirection(tripod.left);
		// right lower leg axis is flexion
		tripod = VectorHelpers.AxisTripodFromForwardUp(rightFoot.bone.position-rightLowerLeg.bone.position, transform.right);
		rightLowerLeg.jointAxis = rightLowerLeg.bone.InverseTransformDirection(tripod.up);
		rightLowerLeg.jointSecondaryAxis = rightLowerLeg.bone.InverseTransformDirection(tripod.left);
		// right foot
		tripod = VectorHelpers.AxisTripodFromForwardUp(transform.forward, transform.right);
		rightFoot.jointAxis = rightFoot.bone.InverseTransformDirection(tripod.up);
		rightFoot.jointSecondaryAxis = rightFoot.bone.InverseTransformDirection(tripod.left);
		// right toe
		if (rightToe!=null)
		{
			rightToe.jointAxis = rightToe.bone.InverseTransformDirection(tripod.up);
			rightToe.jointSecondaryAxis = rightToe.bone.InverseTransformDirection(tripod.left);
		}
		
		// cardinalize the axes if requested
		if (useCardinalAxes)
		{
			foreach (BodyPart part in allParts)
			{
				if (part!=null)
				{
					part.jointAxis = VectorHelpers.FindNearestCardinalAxis(part.jointAxis);
					part.jointSecondaryAxis = VectorHelpers.FindNearestCardinalAxis(part.jointSecondaryAxis);
				}
			}
		}
	}
	
	/*
	 * Determine the limits on the body parts based on their joint properties and a normal joint limit
	 * */
	public void SetDefaultJointLimits()
	{
		// validate minimum requirements
		ValidateMinimumRequirements();
		
		// set the initial rotation
		foreach (BodyPart part in allParts) if (part!=null) part.StoreInitialRotation();
		
		// head
		head.xMin=-5f; head.xMax=10f; head.yMax=15f; head.zMax=10f;
		
		// neck
		float[] parameterValues;
		float param = 0f;
		if (neck.Length>0)
		{
			parameterValues = ComputeChainParameterValues(ref neck, neckLength, 1f);
			param = parameterValues[0];
			for (int i=0; i< neck.Length; i++)
			{
				if (i>0) param = parameterValues[i]-parameterValues[i-1];
				// TODO: There may be better values for these
				neck[i].xMin = -2.5f*param;
				neck[i].xMax = 2.5f*param;
				neck[i].yMax = 2.5f*param;
				neck[i].zMax = 2.5f*param;
			}
		}
		
		// spine
		parameterValues = ComputeChainParameterValues(ref spine, spineLength, 1f);
		param = parameterValues[0];
		for (int i=0; i< spine.Length; i++)
		{
			if (i>0) param = parameterValues[i]-parameterValues[i-1];
			spine[i].xMin = -40f*param;
			spine[i].xMax = 120f*param;
			spine[i].yMax = 40f*param;
			spine[i].zMax = 60f*param;
		}
		
		// collar bones
		if (leftCollar!=null)
		{
			leftCollar.xMin=0f; leftCollar.xMax=20f; leftCollar.yMax=15f; leftCollar.zMax=0f;
		}
		if (rightCollar!=null)
		{
			rightCollar.xMin=0f; rightCollar.xMax=20f; rightCollar.yMax=15f; rightCollar.zMax=0f;
		}
		
		// upper arms
		leftUpperArm.xMin=-100f; leftUpperArm.xMax=50f; leftUpperArm.yMax=100f; leftUpperArm.zMax=100f;
		rightUpperArm.xMin=-100f; rightUpperArm.xMax=50f; rightUpperArm.yMax=100f; rightUpperArm.zMax=100f;
		
		// forearms
		float angle = Vector3.Angle(
			leftHand.bone.position-leftForearm.bone.position, 
			leftForearm.bone.position-leftUpperArm.bone.position);
		leftForearm.xMin=angle-100f; leftForearm.xMax=angle; leftForearm.yMax=0f; leftForearm.zMax=0f;
		angle = Vector3.Angle(
			rightHand.bone.position-rightForearm.bone.position, 
			rightForearm.bone.position-rightUpperArm.bone.position);
		rightForearm.xMin=angle-100f; rightForearm.xMax=angle; rightForearm.yMax=0f; rightForearm.zMax=0f;
		
		// hands
		leftHand.xMin=-90f; leftHand.xMax=30f; leftHand.yMax=10f; leftHand.zMax=45f;
		rightHand.xMin=-90f; rightHand.xMax=30f; rightHand.yMax=10f; rightHand.zMax=45f;
		
		// upper legs
		leftHip.xMin=-40f; leftHip.xMax=100f; leftHip.yMax=30f; leftHip.zMax=45f;
		rightHip.xMin=-40f; rightHip.xMax=100f; rightHip.yMax=30f; rightHip.zMax=45f;
		
		// lower legs
		angle = Vector3.Angle(
			leftFoot.bone.position-leftLowerLeg.bone.position, 
			leftLowerLeg.bone.position-leftHip.bone.position);
		leftLowerLeg.xMin=angle-100f; leftLowerLeg.xMax=angle; leftLowerLeg.yMax=1f; leftLowerLeg.zMax=1f;
		angle = Vector3.Angle(
			rightFoot.bone.position-rightLowerLeg.bone.position, 
			rightLowerLeg.bone.position-rightHip.bone.position);
		rightLowerLeg.xMin=angle-100f; rightLowerLeg.xMax=angle; rightLowerLeg.yMax=1f; rightLowerLeg.zMax=1f;
		
		// feet
		leftFoot.xMin=-45f; leftFoot.xMax=15f; leftFoot.yMax=5f; leftFoot.zMax=5f;
		rightFoot.xMin=-45f; rightFoot.xMax=15f; rightFoot.yMax=5f; rightFoot.zMax=5f;
		
		// toes
		if (leftToe!=null)
		{
			leftToe.xMin=-20f; leftToe.xMax=20f; leftToe.yMax=0f; leftToe.zMax=0f;
		}
		if (rightToe!=null)
		{
			rightToe.xMin=-20f; rightToe.xMax=20f; rightToe.yMax=0f; rightToe.zMax=0f;
		}
		
		// disable parts that don't use joints
		if (headTip!=null) headTip.isRigidbody = false;
		if (leftHandTip!=null) leftHandTip.isRigidbody = false;
		if (rightHandTip!=null) rightHandTip.isRigidbody = false;
		if (leftToeTip!=null) leftToeTip.isRigidbody = false;
		if (rightToeTip!=null) rightToeTip.isRigidbody = false;
		
		// turn the toes off by default
		if (leftToe!=null) leftToe.isRigidbody = false;
		if (rightToe!=null) rightToe.isRigidbody = false;
	}
	
	/*
	 * Set the parent part for each BodyPart
	 * */
	public void SetParentParts()
	{
		// validate minimum requirements
		ValidateMinimumRequirements();
		
		if (headTip!=null) headTip.parentPart = head;
		if (neck.Length>0)
		{
			head.parentPart = neck[neck.Length-1];
			for (int i=1; i<neck.Length; i++) neck[i].parentPart=neck[i-1];
			neck[0].parentPart = spine[spine.Length-1];
		}
		else head.parentPart = spine[spine.Length-1];
		for (int i=1; i<spine.Length; i++) spine[i].parentPart=spine[i-1];
		spine[0].parentPart = pelvis;
		if (leftCollar!=null)
		{
			leftCollar.parentPart = spine[spine.Length-1];
			leftUpperArm.parentPart = (leftCollar.isRigidbody)?leftCollar:spine[spine.Length-1];
		}
		else leftUpperArm.parentPart = spine[spine.Length-1];
		leftForearm.parentPart = leftUpperArm;
		leftHand.parentPart = leftForearm;
		if (leftHandTip!=null) leftHandTip.parentPart = leftHand;
		if (rightCollar!=null)
		{
			rightCollar.parentPart = spine[spine.Length-1];
			rightUpperArm.parentPart = (rightCollar.isRigidbody)?rightCollar:spine[spine.Length-1];
		}
		else rightUpperArm.parentPart = spine[spine.Length-1];
		rightForearm.parentPart = rightUpperArm;
		rightHand.parentPart = rightForearm;
		if (rightHandTip!=null) rightHandTip.parentPart = rightHand;
		leftHip.parentPart = pelvis;
		leftLowerLeg.parentPart = leftHip;
		leftFoot.parentPart = leftLowerLeg;
		if (leftToe!=null)
		{
			leftToe.parentPart = leftFoot;
			if (leftToeTip!=null) leftToeTip.parentPart = leftToe;
		}
		rightHip.parentPart = pelvis;
		rightLowerLeg.parentPart = rightHip;
		rightFoot.parentPart = rightLowerLeg;
		if (rightToe!=null)
		{
			rightToe.parentPart = rightFoot;
			if (rightToeTip!=null) rightToeTip.parentPart = rightToe;
		}
	}
	
	/*
	 * Fully automate setup using the current namingConvention and mass
	 * */
	public void AutomateSetup()
	{
		AutomateSetup(DCCApplication.Custom);
	}
	
	/*
	 * Automate setup using the naming convention of the specified DCC application
	 * Pass in DCCApplication.Custom if you have pre-defined a namingConvention on this Biped
	 * */
	public void AutomateSetup(DCCApplication fromApplication)
	{
		switch (fromApplication)
		{
		case DCCApplication.Max:
			namingConvention = BipedNamingConvention.Max(BipedHelpers.FindCharacterNamePrefixOnMaxBiped(this));
			break;
		case DCCApplication.Maya:
			namingConvention = BipedNamingConvention.Maya();
			break;
		case DCCApplication.HumanIK:
			namingConvention = BipedNamingConvention.HumanIK();
			break;
		}
		FindBonesUsingNamingConvention();
		if (ValidateMinimumRequirements())
		{
			if (anim==null) anim = GetComponentInChildren<Animation>();
			MapSymmetry(true);
			SetParentParts();
			SetDefaultColliders();
			SetDefaultJoints(true);
			DistributeMass();
		}
	}
	
	/*
	 * Store snapshots for all the body parts
	 * */
	public void SnapshotPosition()
	{
		foreach (BodyPart part in allParts) if (part!=null) part.StorePositionSnapshot();
	}
	
	/*
	 * Create a ragdoll with reasonable resistance and force values for a normal human
	 * */
	public void CreateRagdoll()
	{
		CreateRagdoll(15f, 85f);
	}
	
	/*
	 * Turn the biped into a ragdoll with optional resistence on each joint
	 * NOTE: If maxforce is 0, the ragdoll will crumple like a chump
	 * */
	public void CreateRagdoll(float resistence, float maxForce)
	{
		// validate minimum requirements
		ValidateMinimumRequirements();
		
		// early out if the character is already a ragdoll
		if (_isRagdoll) return;
		
		// set the flag and disable animation
		_isRagdoll = true;
		if (anim!=null)
		{
			anim.Stop();
			anim.enabled = false;
		}
		
		// add colliders
		foreach (BodyPart part in allParts) if (part!=null && part.isCollider) part.AddCollider();
		
		// connect the joints
		// axial skeleton
		if (neck.Length>0)
		{
			head.ConnectTo(neck[neck.Length-1], resistence, maxForce);
			for (int i=neck.Length-1; i>-1; i--)
			{
				if (i==0) neck[i].ConnectTo(spine[spine.Length-1], resistence, maxForce);
				else neck[i].ConnectTo(neck[i-1], resistence, maxForce);
			}
		}
		else
		{
			head.ConnectTo(spine[spine.Length-1], resistence, maxForce);
		}
		for (int i=spine.Length-1; i>-1; i--)
		{
			if (i==0) spine[i].ConnectTo(pelvis, resistence, maxForce);
			else spine[i].ConnectTo(spine[i-1], resistence, maxForce);
		}
		// arms
		if (leftCollar!=null && leftCollar.isRigidbody)
		{
			leftCollar.ConnectTo(spine[spine.Length-1], resistence, maxForce);
			leftUpperArm.ConnectTo(leftCollar, resistence, maxForce);
		}
		if (leftUpperArm.joint==null) leftUpperArm.ConnectTo(spine[spine.Length-1], resistence, maxForce);
		leftForearm.ConnectTo(leftUpperArm, resistence, maxForce);
		if (leftHand!=null && leftHand.isRigidbody) leftHand.ConnectTo(leftForearm, resistence, maxForce);
		if (rightCollar!=null && rightCollar.isRigidbody)
		{
			rightCollar.ConnectTo(spine[spine.Length-1], resistence, maxForce);
			rightUpperArm.ConnectTo(rightCollar, resistence, maxForce);
		}
		if (rightUpperArm.joint==null) rightUpperArm.ConnectTo(spine[spine.Length-1], resistence, maxForce);
		rightForearm.ConnectTo(rightUpperArm, resistence, maxForce);
		if (rightHand!=null && rightHand.isRigidbody) rightHand.ConnectTo(rightForearm, resistence, maxForce);
		// legs
		leftHip.ConnectTo(pelvis, resistence, maxForce);
		leftLowerLeg.ConnectTo(leftHip, resistence, maxForce);
		if (leftFoot.isRigidbody)
		{
			leftFoot.ConnectTo(leftLowerLeg, resistence, maxForce);
			if (leftToe.isRigidbody) leftToe.ConnectTo(leftFoot, resistence, maxForce);
		}
		rightHip.ConnectTo(pelvis, resistence, maxForce);
		rightLowerLeg.ConnectTo(rightHip, resistence, maxForce);
		if (rightFoot.isRigidbody)
		{
			rightFoot.ConnectTo(rightLowerLeg, resistence, maxForce);
			if (rightToe.isRigidbody) rightToe.ConnectTo(rightFoot, resistence, maxForce);
		}
	}
	
	/*
	 * Return the biped to a non-ragdoll state
	 * */
	public void RemoveRagdoll()
	{
		if (!_isRagdoll) return;
		
		Vector3 currentPosition = pelvis.bone.position;
		foreach (BodyPart part in allParts)
		{
			// early out if a part does not exist
			if (part==null) continue;
			
			// remove the ragdoll components
			if (part.joint!=null) Destroy(part.joint);
			if (part.rigidbody!=null) Destroy(part.rigidbody);
			if (part.collider!=null) Destroy(part.collider);
			
			// snap parts back to their snapshotted position values in case they jittered out of place
			part.ResetToPositionSnapshot();
		}
		pelvis.bone.position = currentPosition;
		
		// set the flag and reenable animation
		_isRagdoll = false;
		if(anim!=null) anim.enabled = true;
	}
}