using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * Root Motion Computer
 * Version: 1.2
 * Date: 2010.12.08
 * Author: Adam Mechtley (http://adammechtley.com)
 * Created for Mixamo, Inc. (http://mixamo.com)
 * 
 * ----------------------------------------------------------------------------
 * Description:
 * ----------------------------------------------------------------------------
 * Like many other game engines, Unity was developed with the intent that
 * character animations be created in-place, as though the characters were
 * moving and acting on a treadmill, and the characters' root nodes would then
 * be moved programmatically using physics, a character controller, or another
 * procedural mechanism. Unfortunately, many character movements, such as a
 * zombie lurching forward, do not move forward with a constant velocity.
 * Consequently, such motions can introduce foot sliding or dramatic bobbing
 * back and forth when moved procedurally.
 * 
 * The Root Motion Computer was designed to solve this problem, particularly by
 * leveraging the fact that motion-capture animations actually capture a moving
 * actor rather than an actor performing in place. Using all of the default
 * settings, the root motion computer uses the movement of the character's
 * pelvis to move move the root node. Thus, in-place motions will play back in-
 * place, while motions captured with forward or sideways movement will
 * actually move the character when looping, rather than snapping back to the
 * starting location. Users can also configure the computer's various settings
 * to instead pipe its output to another script to drive the velocity of a
 * character controller or another movement tool.
 * 
 * ----------------------------------------------------------------------------
 * Usage:
 * ----------------------------------------------------------------------------
 * You can place this script anywhere in your project folder. Because it is
 * written in C#, however, you must put it in your Plugins folder in your
 * project if you are coding in UnityScript.
 * 
 * In many cases, you can simply add the component to your character and it
 * should "just work." Otherwise you can manually specify various properties:
 * 
 * isManagedExternally: Specifies that another script will invoke Initialize()
 *     and ComputeRootMotion(). This is used if you need to manage the
 *     execution order to prevent the computer from interfering with animation
 *     requests you may make in your own Start(), Awake(), or LateUpdate()
 *     functions.
 * rootNode: The transform that is actually moved, whether by the computer or
 *     by another mechanism like a character controller.
 * anim: The animation component from which to process AnimationStates.
 * pelvis: The character's pelvis transform. This object is used to determine
 *     changes in the character's overall position or rotation.
 * pelvisForwardAxis: The axis on the pelvis that points to the character's
 *     front in the bind pose.
 * pelvisRightAxis: The axis on the pelvis that points to the character's right
 *     in the bind pose.
 * computationMode: Specifies whether the computer should compute only forward
 *     translation, all translation (forward-back and side-to-side), or all
 *     translation as well as turning rotation.
 * applyMotion: Specifies whether the computation results should be applied to
 *     rootNode. Set to false if output is going to be read and processed by
 *     another script to move a character controller, for example.
 * deltaPosition: Represents the character's change in translation since the
 *     last frame, given in the space of rootNode.
 * deltaRotation: Represents the character's change in orientation since the
 *     last frame, given in the space of rootNode.
 * deltaEulerAngles: Same as deltaRotation but converted into Euler angles.
 * isDebugMode: Renders pelvis axes in the scene view when the character is
 *     selected. Renders axis tripods to illustrate the position and
 *     orientation of the pelvis and root node when the game is playing.
 * debugGizmoSize: A scalar for the debug gizmos.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations:
 * ----------------------------------------------------------------------------
 * 1. The computer is currently only designed to handle movement of characters
 * forward-back and side-to-side with rotation about their up-axes. As such, it
 * offers no generalized mechanism for adjusting the height of a character
 * (e.g. jumping, going up stairs). Because the computer operates using delta 
 * values in LateUpdate(), however, you can implement your own custom logic for
 * adjusting a character's height in your own movement code and the computer
 * will simply work on top if it.
 * 
 * 2. As of Unity 2.x, there is no way to query the post-normalized weights of
 * AnimationStates. The computer attempts to work around this by rebuilding
 * normalized weights for each state using the same process that Unity uses
 * (applying weights to top-most layers first, and then working down).
 * 
 * 3. The computer currently assumes that the clip for any particular
 * AnimationState will not change. (Generally speaking, it should not once it
 * has been added to the animation component anyway.)
 * 
 * 4. The computer should support adding new clips at run-time, though the
 * feature has been tested only briefly.
 * 
 * 5. Because of how rotation computation works, if a character is on his
 * stomach and then rolls onto his back (or vice-versa), then it is inadvisable
 * to blend in other motions at the time the actual roll occurs (unless it is
 * another synchronized rolling motion).
 * */

// a struct to store information about all of the animation states
public struct AnimInfo
{
	public float currentNormalizedTime;
	public float previousNormalizedTime;
	public float currentWeight; // the actual weight value queried from the AnimationState
	public float contributingWeight; // the weight the AnimationState is actually contributing to the final result based on layers
	public Vector3 currentPosition;
	public Vector3 previousPosition;
	public Vector3 startPosition;
	public Vector3 endPosition;
	public Vector3 currentAxis;
	public Vector3 previousAxis;
	public Vector3 startAxis;
	public Vector3 endAxis;
	public Quaternion totalRotation;
}

// an enum to describe how delta values should be computed
public enum RootMotionComputationMode
{
	ZTranslation,
	XZTranslation,
	TranslationAndRotation
}

[AddComponentMenu("Mixamo/Root Motion Computer")]
public class RootMotionComputer : MonoBehaviour
{
	// the transform to have root motion applied
	public Transform rootNode;
	
	// the animation component where all of the clips for this model exist
	public Animation anim;
	
	// the pelvis joint from which the script obtains x-z motion and y-rotation for the root
	public Transform pelvis;
	public Vector3 pelvisRightAxis = Vector3.right; // its local axis specifying the right direction
	private Vector3 pLocalPosition; // a variable to temporarily store and set the pelvis local position after computation
	
	// parameters for computation and application of result
	public bool isManagedExternally = false; // if the computer is managed externally, then its calls are invoked manually
	public RootMotionComputationMode computationMode = RootMotionComputationMode.TranslationAndRotation;
    public bool applyMotion = true;
	
	// information about the computed root position
	private Vector3 dPosition = Vector3.zero; // local-space delta position since previous frame
	public Vector3 deltaPosition {get{return dPosition;}}
	private Vector3 p; // a simple container to minimize allocations
	
	// information about the computed root rotation
	private Quaternion dRotation = Quaternion.identity; // local-space delta rotation since previous frame
	public Quaternion deltaRotation {get {return dRotation;}}
	public Vector3 deltaEulerAngles {get {return dRotation.eulerAngles;}}
	
	// a hashtable storing information about each AnimationState
	private Hashtable animInfoTable;
	private AnimInfo info; // a simple container to minimize allocations
	
	// specify whether the component should be running in debug mode
	public bool isDebugMode = true;
	public float debugGizmoSize = 0.25f;
	
	// is the computation occuring on the first frame of execution?
	private bool isFirstFrame = true;
	
	// the highest and lowest layers on which there is an AnimationState
	private int highestLayer = 0;
	private int lowestLayer = 0;
	
	/*
	 * Initialize the component if it is not managed externally
	 * */
	void Start()
	{
		if (!isManagedExternally) Initialize();
	}
	
	/*
	 * Initialize all necessary variables and warn user as needed
	 * */
	public void Initialize()
	{
		// validate component references
		if (anim == null)
		{
			anim = gameObject.GetComponentInChildren(typeof(Animation)) as Animation;
			if (anim == null) Debug.LogError("No animation component has been specified.", this);
			else if (isDebugMode) Debug.LogWarning(string.Format("No animation component has been specified. Using the animation component on {0}.", gameObject.name), this);
		}
		if (rootNode == null)
		{
			rootNode = transform;
			if (isDebugMode) Debug.LogWarning(string.Format("No root object has been manually specified. Assuming that {0} is the root object to be moved.", gameObject.name), this);
		}
		if (pelvis == null)
		{
			Component[] hierarchy = GetComponentsInChildren(typeof(Transform));
			// first try to figure out the pelvis based on name
			foreach (Transform joint in hierarchy)
				if (pelvis == null && (joint.name.ToLower() == "hips" || joint.name.ToLower().Contains("pelvis"))) pelvis = joint;
			// if no named pelvis was found, then try to find the first skinned mesh renderer with children
			if (pelvis == null)
			{
				foreach (Transform joint in hierarchy)
				{
					if (joint.GetComponent(typeof(SkinnedMeshRenderer)) == null) continue;
					Component[] children = joint.GetComponentsInChildren(typeof(Transform));
					if (children.Length > 1) pelvis = joint;						
				}
			}
			if (pelvis == null) Debug.LogError("No pelvis transform has been specified.", this);
			else if (isDebugMode) Debug.LogWarning(string.Format("No pelvis object as been manually specified. Assuming that {0} is the pelvis object to track.", pelvis.name));
		}
		
		// store whether or not the animation component is playing
		bool isAnimationPlaying = anim.isPlaying;
		
		// store information about each AnimationState in a hashtable for easy lookup later
		animInfoTable = new Hashtable();
		// first, figure out what all AnimationStates are currently doing
		foreach (AnimationState aState in anim)
		{
			AddAnimInfoToTable(aState);
		}
		anim.Sample(); // BUG: need to call Sample() once up front or AnimationStates in Animation component may reorder during iteration
		anim.Stop(); // call Stop() to ensure that all weights go to 0
		anim.enabled = true; // reenable the animation component to ensure that values will be correct when sampling
		// store properties for each state one at a time
		foreach (AnimationState aState in anim)
		{
			SetupNewAnimInfo(aState);
		}
		
		// revert the animation component to whatever it was doing beforehand
		foreach (AnimationState aState in anim)
		{
			info = (AnimInfo) animInfoTable[aState];
			aState.weight = info.currentWeight;
			aState.normalizedTime = info.currentNormalizedTime;
		}
		if (isAnimationPlaying) anim.Play();
		else anim.Stop();
	}
	
	/*
	 * Add information about the provided state to the hashtable
	 * */
	public void AddAnimInfoToTable(AnimationState aState)
	{
		// create the new info object
		AnimInfo newInfo = new AnimInfo();
		
		// store the current properties
		newInfo.currentNormalizedTime = aState.normalizedTime;
		newInfo.currentWeight = aState.weight;
		
		// add a new hashtable entry for the AnimInfo
		animInfoTable.Add(aState, newInfo);
	}
	
	/*
	 * Set up further properties for a newly-created info object after calling AddAnimInfoToTable()
	 * */
	public void SetupNewAnimInfo(AnimationState aState)
	{				    
		AnimInfo newInfo = (AnimInfo) animInfoTable[aState];
		
		// store information about the animation state up front
		bool isEnabled = aState.enabled;
		WrapMode wrapMode = aState.wrapMode;
		
		// activate the animation state
		aState.weight = 1f;
		aState.enabled = true;
		aState.wrapMode = WrapMode.Clamp; // ensures the value at normalizedTime = 1f is not necessarily the same as normalizedTime = 0f
		
		// scrub to the beginning of the animation state and store initial position and rotation values
		aState.normalizedTime = 0f;
		anim.Sample();
		newInfo.startPosition = GetProjectedPosition(pelvis);
		newInfo.previousPosition = GetProjectedPosition(pelvis);
		newInfo.startAxis = GetProjectedAxis(pelvis, pelvisRightAxis);
		newInfo.previousAxis = GetProjectedAxis(pelvis, pelvisRightAxis);
				
		// scrub to the end of the animation state and store final position and rotation values
		aState.normalizedTime = 1f;
		anim.Sample();
		newInfo.endPosition = GetProjectedPosition(pelvis);
		newInfo.endAxis = GetProjectedAxis(pelvis, pelvisRightAxis);
		
		// store the total rotation over the course of the animation
		newInfo.totalRotation = Quaternion.FromToRotation(newInfo.startAxis, newInfo.endAxis);
				
		// reset the clip to its starting point and scrub it down to 0 weight
		aState.normalizedTime = 0f;
		aState.weight = 0f;
		aState.enabled = isEnabled;
		aState.wrapMode = wrapMode;
		anim.Sample();
		
		animInfoTable[aState] = newInfo;
	}
	
	/*
	 * All motion is applied in LateUpdate() since it is called after all animation states have been set
	 * */
	void LateUpdate()
	{	
		if (!isManagedExternally) ComputeRootMotion();
	}
	
	/*
	 * Compute the root motion variables
	 * */
	public void ComputeRootMotion()
	{
		// early out if no animation is playing
		if (!anim.isPlaying) return;
		
		// store whether or not we should be bothering to compute rotation parameters
		bool isRotationMode = (computationMode == RootMotionComputationMode.TranslationAndRotation);
		
		// an array to store any AnimationStates that have been added to the animation component since the last frame
		ArrayList newlyAddedAnimationStates = null;
		
		// first store current actual weight and time information for all AnimationStates
		foreach (AnimationState aState in anim)
		{
			// store the highest and lowest layers for use in a later iteration
			highestLayer = Mathf.Max(highestLayer, aState.layer);
			lowestLayer = Mathf.Min(lowestLayer, aState.layer);
			
			// if any new animation states have been added, then deal with them in a following iteration
			if (!animInfoTable.ContainsKey(aState))
			{
				AddAnimInfoToTable(aState);
				newlyAddedAnimationStates.Add(aState);
				continue;
			}
			
			info = (AnimInfo) animInfoTable[aState];
			info.currentNormalizedTime = aState.normalizedTime;
			info.currentWeight = aState.weight;
			
			animInfoTable[aState] = info;
			
			// scrub the weight down to 0 for the next iteration
			aState.weight = 0f;
		}
		
		// if any new AnimationStates have been added, add their info to the table
		if (newlyAddedAnimationStates != null && newlyAddedAnimationStates.Count > 0)
		{
			// first set all weights to 0, which will include newly added states
			foreach (AnimationState aState in anim) aState.weight = 0f;
			
			// store all the properties for the new states
			foreach (AnimationState aState in newlyAddedAnimationStates) SetupNewAnimInfo(aState);
		}
		
		// compute normalized AnimationState weights across layers since Unity does not expose them
		float remainingWeight = 1f;
		for (int i=highestLayer; i>=lowestLayer; i--)
		{			
			float weightOnThisLayer = 0f;
			foreach (AnimationState aState in anim)
			{
				if (aState.layer != i) continue;
				
				info = (AnimInfo) animInfoTable[aState];
				
				// find out how much weight the animation state is actually contributing this frame
				if (!aState.enabled || remainingWeight <= 0f)
				{
					info.contributingWeight = 0f;
				}
				else
				{
					info.contributingWeight = remainingWeight * info.currentWeight;
				}
				
				weightOnThisLayer += info.contributingWeight;
				
				animInfoTable[aState] = info;
			}
			// if the weight on this layer is > 1, then normalize it
			// using Blend() or setting weights manually will not affect other weights on the layer, so they must be manually renormalized
			if (weightOnThisLayer > 1f)
			{
				float oneOverWeightOnThisLayer = 1f/weightOnThisLayer;
				foreach (AnimationState aState in anim)
				{
					if (aState.layer != i) continue;
					info = (AnimInfo) animInfoTable[aState];
					info.contributingWeight = info.contributingWeight * oneOverWeightOnThisLayer;
					animInfoTable[aState] = info;
				}
				weightOnThisLayer = 1f;
			}
			remainingWeight -= weightOnThisLayer;
		}
		
		// reset the delta values for this frame
		dPosition = Vector3.zero;
		dRotation = Quaternion.identity;
		
		// compute each AnimationState's individual contribution to the current frame's delta values
		foreach (AnimationState aState in anim)
		{	
			info = (AnimInfo) animInfoTable[aState];
			
			// early out if this state was contributing nothing this frame
			if (info.contributingWeight == 0f) continue;
			
			// early out if aState uses additive blending
			// NOTE: Not entirely sure if this is ideal or not, but it generally should be
			if (aState.blendMode == AnimationBlendMode.Additive) continue;
						
			// scrub the weight up to 1 for sampling values
			aState.weight = 1f;
			
			// sample the values for the projected root configuration back one frame
			// NOTE: cannot simply store these values from one frame to the next since user may manually change the time value at any point
			aState.time = aState.time - Time.deltaTime * aState.speed;
			info.previousNormalizedTime = aState.normalizedTime;
			anim.Sample();
			info.previousAxis = GetProjectedAxis(pelvis, pelvisRightAxis);
			info.previousPosition = GetProjectedPosition(pelvis);
			
			// sample the values for the projected root configuration at the current frame
			aState.normalizedTime = info.currentNormalizedTime;
			anim.Sample();
			info.currentPosition = GetProjectedPosition(pelvis);
			info.currentAxis = GetProjectedAxis(pelvis, pelvisRightAxis);
			
			// ensure both normalizedTime values are positive
			info.previousNormalizedTime = 1f + info.previousNormalizedTime - (int)info.previousNormalizedTime;
			info.currentNormalizedTime = 1f + info.currentNormalizedTime - (int)info.currentNormalizedTime;
			
			// determine the contribution to the root's delta this frame based on whether the animation looped since the previous frame
			if (info.previousNormalizedTime-(int)info.previousNormalizedTime > info.currentNormalizedTime-(int)info.currentNormalizedTime)
			{
				// compute displacement with respect to identity
				p = info.contributingWeight * ((info.endPosition - info.previousPosition) + (info.currentPosition - info.startPosition));
				if (isRotationMode)
				{
					// rotate displacement into current orientation
					p = Quaternion.FromToRotation(info.currentAxis, info.totalRotation*Vector3.right) * p;	
					// compute angular displacement and append to result
					dRotation *= Quaternion.Slerp(Quaternion.identity, 
						Quaternion.FromToRotation(info.previousAxis, info.endAxis) * Quaternion.FromToRotation(info.startAxis, info.currentAxis), 
						info.contributingWeight);
				}
				// append displacement to result
				dPosition += p;
			}
			else
			{
				// compute displacement with respect to identity
				p = info.contributingWeight * (info.currentPosition - info.previousPosition);
				if (isRotationMode)
				{
					// rotate displacement into current orientation
					p = Quaternion.FromToRotation(info.currentAxis, Vector3.right) * p;
					// compute angular displacement and append to result
					dRotation *= Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(info.previousAxis, info.currentAxis), info.contributingWeight);
				}
				// append displacement to result
				dPosition += p;
			}
			
			// scrub the weight back down to 0 so as to not affect sampling of other states
			aState.weight = 0f;
		}
		
		// reset weights to where they were before computation
		foreach (AnimationState aState in anim)
		{	
			info = (AnimInfo) animInfoTable[aState];
			aState.weight = info.currentWeight;
		}
		
		// return the character to its current pose
		anim.Sample();
		
		// delta values for the first frame should simply move from the starting configuration into the current frame of animation
		if (isFirstFrame)
		{
			// simply translate and rotate to the current projected position and orientation
			dPosition = GetProjectedPosition(pelvis);
			dRotation = Quaternion.FromToRotation(Vector3.right, GetProjectedAxis(pelvis, pelvisRightAxis));
			
			// rotate displacement into current orientation
			if (isRotationMode) dPosition = Quaternion.FromToRotation(GetProjectedAxis(pelvis, pelvisRightAxis), Vector3.right) * dPosition;
						
			isFirstFrame = false;
		}
		
		// store the local position of the pelvis before returning it to hover over the root
		pLocalPosition = pelvis.localPosition;
		
		// zero out the local x-component of the position delta if root translation method is z-only
		if (computationMode == RootMotionComputationMode.ZTranslation) dPosition = Vector3.forward * Vector3.Dot(dPosition, Vector3.forward);
		// otherwise zero out the local x-position of the pelvis
		else pLocalPosition.x = 0f;
		
		// return the pelvis to a point hovering over the root
		pLocalPosition.z = 0f;
		pelvis.localPosition = pLocalPosition;
								
		// if computing rotation, then zero out local y-rotation of the pelvis
		if (isRotationMode) pelvis.localRotation = Quaternion.FromToRotation(GetProjectedAxis(pelvis, pelvisRightAxis), Vector3.right) * pelvis.localRotation;
		
		// draw debug lines if requested
		if (isDebugMode) DrawDebug();
		
		// return if root movement is not requested (e.g. a character controller will use delta values)
		if (!applyMotion) return;
		
		// apply rotation if requested
		if (isRotationMode) rootNode.localRotation *= dRotation;
		
		// apply translation
		rootNode.Translate(dPosition, Space.Self);
	}
	
	/*
	 * Obtain the position of t projected onto rootNode's zx plane
	 * */
	private Vector3 GetProjectedPosition(Transform t)
	{
		Vector3 p = rootNode.InverseTransformPoint(t.position);
		p.y = 0f;
		return p;
	}
	
	/*
	 * Obtain the projection of axis on t onto rootNode's zx plane
	 * */
	private Vector3 GetProjectedAxis(Transform t, Vector3 axis)
	{
		Vector3 p = rootNode.InverseTransformDirection(t.TransformDirection(axis));
		p.y = 0f;
		return p;
	}
		
	/*
	 * Draw axis tripods to show how root motion is being determined and applied
	 * */
	private void DrawDebug()
	{	
		// draw pelvis right axis
		Debug.DrawRay(pelvis.position, pelvis.TransformDirection(pelvisRightAxis) * debugGizmoSize, Color.red);
		
		// draw root node axes
		Debug.DrawRay(rootNode.position, rootNode.rotation * Vector3.forward * debugGizmoSize, Color.blue);
		Debug.DrawRay(rootNode.position, rootNode.rotation * Vector3.right * debugGizmoSize, Color.red);
		Debug.DrawRay(rootNode.position, rootNode.rotation * Vector3.up * debugGizmoSize, Color.green);
	}
}