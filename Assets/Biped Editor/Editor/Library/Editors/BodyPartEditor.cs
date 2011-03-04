using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * BodyPartEditor
 * Version: 1.03
 * Date: 2010.12.24
 * Author: Adam Mechtley (http://adammechtley.com)
 * License: Please don't steal or redistribute this code; I have a family. If
 * you did steal it, at least consider making a donation:
 * http://bit.ly/adammechtley_donate
 * 
 * ----------------------------------------------------------------------------
 * Description
 * ----------------------------------------------------------------------------
 * This script provides custom handles for working with BodyPart components.
 * 
 * ----------------------------------------------------------------------------
 * Usage
 * ----------------------------------------------------------------------------
 * Put this script in an Editor folder and it should automatically work when
 * you add a BodyPart to an object.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations
 * ----------------------------------------------------------------------------
 * Symmetry mode will not automatically reflect joint axes. Trust me, you don't
 * want it to, as there is no generic way to handle it presently.
 * */

/*
 * Custom scene GUI when a single BodyPart object is selected
 * */
[CustomEditor(typeof(BodyPart))]
public class BodyPartEditor : Editor
{
	// the selected object with the component
	private BodyPart part;
	
	// colors for the scene parts
	public static Color shapeColor {get{return _shapeColor;}}
	private static Color _shapeColor = Color.Lerp(Color.cyan, Color.yellow, 0.5f);
	
	// parameters for the handles
	public static float shapeHandleScale {get{return 1f;}}
	private static float changeThreshold = 0.001f;
	
	// enable/disable symmetrical editing
	public static bool isSymmetrical
	{
		get { return _isSymmetrical; }
		set
		{
			PlayerPrefs.SetInt("Editor - BodyPartEditor - isSymmetrical", (value)?1:0);
			_isSymmetrical = value;
		}
	}
	private static bool _isSymmetrical = PlayerPrefs.GetInt("Editor - BodyPartEditor - isSymmetrical", 1)==1;
	// enable/disable scale inheritence in symmetry editing
	public static bool isScaleSymmetrical
	{
		get { return _isScaleSymmetrical; }
		set
		{
			PlayerPrefs.SetInt("Editor - BodyPartEditor - isScaleSymmetrical", (value)?1:0);
			_isScaleSymmetrical = value;
		}
	}
	private static bool _isScaleSymmetrical = PlayerPrefs.GetInt("Editor - BodyPartEditor - isScaleSymmetrical", 0)==1;
	// enable/disable center handles
	public static bool isCenterHandleEnabled
	{
		get { return _isCenterHandleEnabled; }
		set
		{
			PlayerPrefs.SetInt("Editor - BodyPartEditor - isCenterHandleEnabled", (value)?1:0);
			_isCenterHandleEnabled = value;
		}
	}
	private static bool _isCenterHandleEnabled = PlayerPrefs.GetInt("Editor - BodyPartEditor - isCenterHandleEnabled", 0)==1;
	// enable/disable shape handles
	public static bool isShapeHandleEnabled
	{
		get { return _isShapeHandleEnabled; }
		set
		{
			PlayerPrefs.SetInt("Editor - BodyPartEditor - isShapeHandleEnabled", (value)?1:0);
			_isShapeHandleEnabled = value;
		}
	}
	private static bool _isShapeHandleEnabled = PlayerPrefs.GetInt("Editor - BodyPartEditor - isShapeHandleEnabled", 1)==1;
	// resistance amount for the joint
	public static float jointResistance
	{
		get { return _jointResistance; }
		set
		{
			PlayerPrefs.SetFloat("Editor - BodyPartEditor - jointResistance", value);
			_jointResistance = value;
		}
	}
	private static float _jointResistance = PlayerPrefs.GetFloat("Editor - BodyPartEditor - jointResistance", 15f);
	// max force amount for the joint
	public static float maxForce
	{
		get { return _maxForce; }
		set
		{
			PlayerPrefs.SetFloat("Editor - BodyPartEditor - maxForce", value);
			_maxForce = value;
		}
	}
	private static float _maxForce = PlayerPrefs.GetFloat("Editor - BodyPartEditor - maxForce", 85f);
	
	// alpha value for symmetrical part
	public static float symmetryAlpha = 0.5f;
	
	// the width for the on-screen controls
	public static float viewportControlsWidth = 240f;
	
	/*
	 * Initialize
	 * */
	void OnEnable()
	{
		part = (BodyPart) target;
	}
	
	/*
	 * Draw custom viewport render
	 * */
	void OnSceneGUI()
	{
		// draw the shape gizmo
		Handles.color = shapeColor;
		DrawShapeHandle(part, isSymmetrical);
		
		// draw the joint handle
		DrawJointHandle(part, isSymmetrical);
		
		// begin GUI or transform handles will be disabled
		Handles.BeginGUI();
		
		// create the viewport controls
		ViewportControls.BeginArea(viewportControlsWidth, GUIAnchor.TopLeft);
		{
			ViewportCommonControls();
			part.isRigidbody = ViewportControls.OnOffToggle("Is Rigidbody:", part.isRigidbody);
			part.isCollider = ViewportControls.OnOffToggle("Is Collider:", part.isCollider);
			if (isSymmetrical && part.oppositePart!=null)
			{
				part.oppositePart.isRigidbody = part.isRigidbody;
				part.oppositePart.isCollider = part.isCollider;
			}
			ViewportShapeControls(part);
			
			// if the editor is playing, then create testing controls
			if (Application.isPlaying)
			{
				// padding after previous controls
				GUILayout.Space(ViewportControls.viewportPadding*2f);
			
				// create a button to test the joint if it does not currently have one
				if (part.joint == null)
				{
					if (part.isRigidbody && GUILayout.Button("Test Joint"))
					{
						// the BodyPart needs to have a parentPart defined in order to test
						if (part.parentPart==null) Debug.LogError("Cannot test part: its parentPart is null.", this);
						else
						{
							if (part.bone.rigidbody==null) part.AddRigidbody();
							else part.bone.rigidbody.isKinematic = false;
							if (part.parentPart.bone.rigidbody==null)
							{
								part.parentPart.AddRigidbody();
								part.parentPart.bone.rigidbody.isKinematic = true;
							}
							if (part.bone.collider==null) part.AddCollider();
							part.ConnectToParent(jointResistance, maxForce);
						}
					}
				}
				// otherwise create a button to remove the joint
				else
				{
					if (GUILayout.Button("Remove Joint"))
					{
						Destroy(part.joint);
						Destroy(part.rigidbody);
						Destroy(part.collider);
						part.ResetToInitialRotation();
					}
				}
				
				// only update the joint's spring and force values if the slider values have changed
				if (part.isRigidbody)
				{
					float oldResist = jointResistance;
					float oldForce = maxForce;
					ViewportResistanceSlider();
					ViewportMaxForceSlider();
					if ((oldResist!=jointResistance || oldForce!=maxForce) && part.joint!=null)
					{
						JointDrive drive = part.joint.slerpDrive;
						drive.maximumForce = maxForce;
						drive.positionSpring = jointResistance;
						part.joint.slerpDrive = drive;
					}
				}
			}
		}
		ViewportControls.EndArea();
		
		// display symmetry status at the top of the viewport
		ViewportStatus();
		
		// finish GUI
		Handles.EndGUI();
	}
	
	/*
	 * A slider for adjusting joint resistance
	 * */
	public static float ViewportResistanceSlider()
	{
		GUILayout.BeginHorizontal();
			GUILayout.Label(string.Format("Resistance: {0:0.00}", jointResistance), GUILayout.Width(BodyPartEditor.viewportControlsWidth*0.65f));
			jointResistance = GUILayout.HorizontalSlider(jointResistance, 0f, 100f);
		GUILayout.EndHorizontal();
		return jointResistance;
	}
	
	/*
	 * A slider for adjusting joint max force
	 * */
	public static float ViewportMaxForceSlider()
	{
		GUILayout.BeginHorizontal();
			GUILayout.Label(string.Format("Max Force: {0:0.00}", maxForce), GUILayout.Width(BodyPartEditor.viewportControlsWidth*0.65f));
			maxForce = GUILayout.HorizontalSlider(maxForce, 0f, 100f);
		GUILayout.EndHorizontal();
		return maxForce;
	}
	
	/*
	 * Display viewport controls for messing with individual body parts
	 * */
	void ViewportShapeControls(BodyPart part)
	{
		GUILayout.Label("Shape Controls:");
		GUILayout.BeginVertical();
		{
			part.shapeType = (ShapeType)GUILayout.SelectionGrid((int)part.shapeType, System.Enum.GetNames(typeof(ShapeType)), 4);
			if (part.shapeType == ShapeType.Capsule)
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Capsule Direction:", GUILayout.Width(viewportControlsWidth*0.65f));
					CapsuleDirection dir = (CapsuleDirection)GUILayout.SelectionGrid((int)part.capsuleDirection, System.Enum.GetNames(typeof(CapsuleDirection)), 3);
					if (dir != part.capsuleDirection)
					{
						part.FlipCapsule(dir);
						if (isSymmetrical) part.oppositePart.FlipCapsule(dir);
					}
				}	
				GUILayout.EndHorizontal();
			}
							}
		GUILayout.EndVertical();
	}
	
	/*
	 * Display viewport controls for messing with gizmos
	 * */
	public static void ViewportCommonControls()
	{
		GUILayout.BeginVertical();
		{
			// only update values if they change to prevent constant updating of player pref keys
			bool bVal;
			bVal = ViewportControls.OnOffToggle("Symmetry Mode:", isSymmetrical);
			if (bVal!=isSymmetrical) isSymmetrical = bVal;
			bVal = ViewportControls.OnOffToggle("Mirror Scale:", isScaleSymmetrical);
			if (bVal!=isScaleSymmetrical) isScaleSymmetrical = bVal;
			bVal = ViewportControls.OnOffToggle("Shape Handles:", isShapeHandleEnabled);
			if (bVal!=isShapeHandleEnabled) isShapeHandleEnabled = bVal;
			bVal = ViewportControls.OnOffToggle("Center Handles:", isCenterHandleEnabled);
			if (bVal!=isCenterHandleEnabled) isCenterHandleEnabled = bVal;
			ConfigurableJointEditor.JointHandleToggle();
			GUILayout.BeginHorizontal();
			{
				ConfigurableJointEditor.JointHandleSizeSlider(viewportControlsWidth);
			}
			GUILayout.EndHorizontal();
			CustomHandleUtilities.ViewportIntegratorFidelityControls(viewportControlsWidth);
		}
		GUILayout.EndVertical();
	}
	
	/*
	 * Display the current status at the top of the viewport
	 * */
	public static void ViewportStatus()
	{
		GUI.Label(new Rect(Screen.width*0.5f-100f, 5f, 200f, 50f), string.Format("Symmetry Mode: {0}",isSymmetrical?"On":"Off"));
	}
		
	/*
	 * Draw a joint limit gizmo for a specified part
	 * */
	public static void DrawJointHandle(BodyPart part, bool drawOpposite)
	{
		// early out if joint handles are disabled or the part is null
		if (!ConfigurableJointEditor.isHandleEnabled || part==null || !part.isRigidbody) return;
		
		// set undo snapshot
		BodyPart[] parts = new BodyPart[1];
		if(isSymmetrical && part.oppositePart!=null)
		{
			parts = new BodyPart[2];
			parts[1] = part.oppositePart;
			parts[1].oppositePart = part; // BUG: No idea why I need to do this
		}
		parts[0] = part;
		Undo.SetSnapshotTarget(parts, string.Format("Change Joint Limits"));
				
		Quaternion parentRotation = (part.bone.parent==null)?Quaternion.identity:part.bone.parent.rotation;
		JointHandles.JointLimit(ref part.xMin, ref part.xMax, ref part.yMax, ref part.zMax,
			part.bone.position, parentRotation*part.initialRotation, part.jointAxis, part.jointSecondaryAxis,
			ConfigurableJointEditor.jointHandleSize, 1f);
		
		if (isSymmetrical && part.oppositePart!=null)
		{
			// TODO: there may be something math smart to reflect axes in a general way
			// NOTE: this works assuming the axes have been correctly set correspondingly
			parts[1].xMax = part.xMax;
			parts[1].xMin = part.xMin;
			parts[1].yMax = part.yMax;
			parts[1].zMax = part.zMax;
			parentRotation = (parts[1].bone.parent==null)?Quaternion.identity:parts[1].bone.parent.rotation;
			JointHandles.JointLimit(ref parts[1].xMin, ref parts[1].xMax, ref parts[1].yMax, ref parts[1].zMax,
				parts[1].bone.position, parentRotation*parts[1].initialRotation, parts[1].jointAxis, parts[1].jointSecondaryAxis,
				ConfigurableJointEditor.jointHandleSize, 0.5f);
			part.xMax = parts[1].xMax;
			part.xMin = parts[1].xMin;
			part.yMax = parts[1].yMax;
			part.zMax = parts[1].zMax;
		}
	}
	
	/*
	 * Do a threshold test to see if the new scaled shape size should be applied to the shape
	 * */
	private static void DoShapeSizeThresholdTest(BodyPart part, Vector3 shapeHandleSize)
	{
		// early out if there is a possiblity of singularity
		if (part.bone.lossyScale.x<changeThreshold || 
			part.bone.lossyScale.y<changeThreshold || 
			part.bone.lossyScale.z<changeThreshold)
			return;
		// scale the shape handle values down if the scale is not identity
		if (part.bone.lossyScale!=Vector3.one)
		{
			// corner case for capsules
			if (part.shapeType==ShapeType.Capsule)
			{
				switch (part.capsuleDirection)
				{
				case CapsuleDirection.X:
					if ((shapeHandleSize.x-shapeHandleSize.y) < changeThreshold) return;
					break;
				case CapsuleDirection.Y:
					if ((shapeHandleSize.y-shapeHandleSize.z) < changeThreshold) return;
					break;
				case CapsuleDirection.Z:
					if ((shapeHandleSize.z-shapeHandleSize.x) < changeThreshold) return;
					break;
				}
			}
			
			shapeHandleSize = new Vector3(
				shapeHandleSize.x/Mathf.Abs(part.bone.lossyScale.x),
				shapeHandleSize.y/Mathf.Abs(part.bone.lossyScale.y),
				shapeHandleSize.z/Mathf.Abs(part.bone.lossyScale.z));
		}
		if ((part.shapeSize-shapeHandleSize).sqrMagnitude > changeThreshold*changeThreshold)
			part.shapeSize = shapeHandleSize;
	}
	
	/*
	 * Returns a scaled version of part.shapeSize taking localScale and scaleVector into account 
	 * */
	private static Vector3 GetShapeHandleSize(BodyPart part)
	{
		return new Vector3(
			part.shapeSize.x*part.bone.lossyScale.x, 
			part.shapeSize.y*part.bone.lossyScale.y, 
			part.shapeSize.z*part.bone.lossyScale.z);
	}
	
	/*
	 * Get values for a capsule part based on its shapeSize and direction
	 * */
	private static void GetCapsuleProperties(BodyPart part, ref float height, ref float radius, ref Quaternion capsuleOrientation)
	{
		switch (part.capsuleDirection)
		{
		case CapsuleDirection.X:
			height = part.shapeSize.x*part.bone.lossyScale.x;
			radius = Mathf.Max(part.shapeSize.y*part.bone.lossyScale.y*0.5f, part.shapeSize.z*part.bone.lossyScale.z*0.5f);
			capsuleOrientation = Quaternion.AngleAxis(90f, Vector3.left)*Quaternion.AngleAxis(90f, Vector3.forward);					
			break;
		case CapsuleDirection.Y:
			height = part.shapeSize.y*part.bone.lossyScale.y;
			radius = Mathf.Max(part.shapeSize.x*part.bone.lossyScale.x*0.5f, part.shapeSize.z*part.bone.lossyScale.z*0.5f);
			capsuleOrientation = Quaternion.identity;
			break;
		case CapsuleDirection.Z:
			height = part.shapeSize.z*part.bone.lossyScale.z;
			radius = Mathf.Max(part.shapeSize.x*part.bone.lossyScale.x*0.5f, part.shapeSize.y*part.bone.lossyScale.y*0.5f);
			capsuleOrientation = Quaternion.AngleAxis(180f, Vector3.forward)*Quaternion.AngleAxis(90f, Vector3.right);					
			break;
		}
		radius = Mathf.Abs(radius);
		height = Mathf.Max(Mathf.Abs(height), radius*2f);
	}
	
	/*
	 * Convert capsule properties into a shapeSize vector
	 * */
	private static Vector3 CapsuleToSize(BodyPart part, float height, float radius)
	{
		switch (part.capsuleDirection)
		{
		case CapsuleDirection.X:
			return new Vector3(height, radius*2f, radius*2f);
		case CapsuleDirection.Y:
			return new Vector3(radius*2f, height, radius*2f);
		default:
			return new Vector3(radius*2f, radius*2f, height);
		}
	}
	
	/*
	 * Draw a collider gizmo for a specified part
	 * */
	public static void DrawShapeHandle(BodyPart part, bool drawOpposite)
	{
		// early out if the part is null or the part has no shape
		if (part==null || part.shapeType==ShapeType.None || !part.isCollider) return;
		
		// store the current color
		Color oldColor = Handles.color;
		
		// set undo snapshot
		BodyPart[] parts = new BodyPart[1];
		if(isSymmetrical && part.oppositePart!=null)
		{
			parts = new BodyPart[2];
			parts[1] = part.oppositePart;
			parts[1].oppositePart = part; // BUG: No idea why I need to do this
		}
		parts[0] = part;
		Undo.SetSnapshotTarget(parts, string.Format("Change Shape"));
		
		// create shape handles
		if (isShapeHandleEnabled)
		{
			// use radius if shape is capsule or sphere
			float radius = 0f;
			
			// compute the size to draw the shape handle based on the part's scale
			Vector3 shapeHandleSize = GetShapeHandleSize(part);
			
			// draw the correct handle based on shapeType
			switch (part.shapeType)
			{
			case ShapeType.Box:
				// create handles
				ShapeHandles.WireBox(ref shapeHandleSize, part.bone.TransformPoint(part.shapeCenter), part.bone.rotation,"");
				
				// apply the result
				DoShapeSizeThresholdTest(part, shapeHandleSize);
				
				// handle symmetry
				if (parts.Length>1)
				{
					part.PasteShapeSizeToOpposite(isScaleSymmetrical);
					if (drawOpposite)
					{
						// get the opposite part's dimensions in case its local scale is different
						shapeHandleSize = GetShapeHandleSize(parts[1]);
						
						// ghost the opposite part
						CustomHandleUtilities.SetHandleColor(oldColor, symmetryAlpha);
						ShapeHandles.WireBox(ref shapeHandleSize, parts[1].bone.TransformPoint(parts[1].shapeCenter), parts[1].bone.rotation,"");
						
						// apply the result
						DoShapeSizeThresholdTest(parts[1], shapeHandleSize);
						parts[1].PasteShapeSizeToOpposite(isScaleSymmetrical);
					}
				}
				break;
			case ShapeType.Capsule:
				// get handle properties
				float height = 0f;
				Quaternion capsuleOrientation = Quaternion.identity;
				GetCapsuleProperties(part, ref height, ref radius, ref capsuleOrientation);
				
				// draw handle
				ShapeHandles.WireCapsule(ref radius, ref height, part.bone.TransformPoint(part.shapeCenter), part.bone.rotation*part.shapeRotation*capsuleOrientation, "");
				
				// apply result
				DoShapeSizeThresholdTest(part, CapsuleToSize(part, height, radius));
				
				// handle symmetry
				if (parts.Length>1)
				{
					part.PasteShapeSizeToOpposite(isScaleSymmetrical);
					if (drawOpposite)
					{
						// get the opposite part's dimensions in case its local scale is different
						GetCapsuleProperties(parts[1], ref height, ref radius, ref capsuleOrientation);
						
						// ghost the opposite part
						CustomHandleUtilities.SetHandleColor(oldColor, symmetryAlpha);
						ShapeHandles.WireCapsule(ref radius, ref height, parts[1].bone.TransformPoint(parts[1].shapeCenter), parts[1].bone.rotation*parts[1].shapeRotation*capsuleOrientation, "");
						
						// apply the result
						DoShapeSizeThresholdTest(parts[1], CapsuleToSize(parts[1], height, radius));
						parts[1].PasteShapeSizeToOpposite(isScaleSymmetrical);
					}
				}
				break;
			case ShapeType.Sphere:
				// create a simple radius handle
				float oldRadius = Mathf.Max(
					part.shapeSize.x*part.bone.lossyScale.x*0.5f, 
					part.shapeSize.y*part.bone.lossyScale.y*0.5f, 
					part.shapeSize.z*part.bone.lossyScale.z*0.5f);
				radius = Handles.RadiusHandle(part.bone.rotation*part.shapeRotation, part.bone.TransformPoint(part.shapeCenter), oldRadius);
				if (Mathf.Abs(radius-oldRadius)>changeThreshold)
				{
					float scaleFactor = 1f/VectorHelpers.MaxValue(part.bone.lossyScale);
					part.shapeSize.x = 2f*radius*scaleFactor;
					part.shapeSize.y = 2f*radius*scaleFactor;
					part.shapeSize.z = 2f*radius*scaleFactor;
					oldRadius = radius;
				}
				
				// handle symmetry
				if (parts.Length>1)
				{
					part.PasteShapeSizeToOpposite(isScaleSymmetrical);
					if (drawOpposite)
					{
						// ghost the opposite part
						CustomHandleUtilities.SetHandleColor(oldColor, symmetryAlpha);
						oldRadius = Mathf.Max(
							parts[1].shapeSize.x*parts[1].bone.lossyScale.x*0.5f, 
							parts[1].shapeSize.y*parts[1].bone.lossyScale.y*0.5f, 
							parts[1].shapeSize.z*parts[1].bone.lossyScale.z*0.5f);
						radius = Handles.RadiusHandle(parts[1].bone.rotation*part.shapeRotation, parts[1].bone.TransformPoint(parts[1].shapeCenter), oldRadius);
						if (Mathf.Abs(radius-oldRadius)>changeThreshold)
						{
							parts[1].shapeSize.x = 2f*radius;
							parts[1].shapeSize.y = 2f*radius;
							parts[1].shapeSize.z = 2f*radius;
							parts[1].PasteShapeSizeToOpposite(isScaleSymmetrical);
						}
					}
				}
				break;
			}
		}
		
		// center handles
		if (isCenterHandleEnabled)
		{
			// position handle for the center
			Vector3 center = part.bone.InverseTransformPoint(Handles.PositionHandle(part.bone.TransformPoint(part.shapeCenter), part.bone.rotation*part.shapeRotation));
			
			// rotation handle
//			Quaternion rotation = Quaternion.Inverse(part.bone.rotation)*Handles.RotationHandle(part.bone.rotation*part.shapeRotation, part.bone.TransformPoint(part.shapeCenter));
			
			// handle symmetry
			if (parts.Length>1)
			{
				center = part.TransformPointToOpposite(center, isScaleSymmetrical);
//				rotation = part.TransformRotationToOpposite(rotation);
				if (drawOpposite)
				{
					center = parts[1].bone.InverseTransformPoint(Handles.PositionHandle(parts[1].bone.TransformPoint(center), parts[1].bone.rotation*part.shapeRotation));
//					rotation = Quaternion.Inverse(parts[1].bone.rotation)*Handles.RotationHandle(parts[1].bone.rotation*parts[1].shapeRotation, parts[1].bone.TransformPoint(parts[1].shapeCenter));
				}
				center = parts[1].TransformPointToOpposite(center, isScaleSymmetrical);
//				rotation = parts[1].TransformRotationToOpposite(rotation);
			}
			
			// apply results
			if ((part.shapeCenter-center).sqrMagnitude>changeThreshold*changeThreshold)
				part.shapeCenter = center;
//			if (Quaternion.Angle(part.shapeRotation, rotation)>changeThreshold) part.shapeRotation = rotation;
		}
		
		if (parts.Length>1)
		{	
			// update values
			parts[1].shapeType = part.shapeType;
			
			Vector3 oldValue = parts[1].shapeCenter;
			Vector3 newValue = part.TransformPointToOpposite(part.shapeCenter, isScaleSymmetrical);
			if ((oldValue-newValue).sqrMagnitude>changeThreshold*changeThreshold)
				parts[1].shapeCenter = newValue;
			
			oldValue = parts[1].shapeSize;
			part.PasteShapeSizeToOpposite(isScaleSymmetrical);
			if ((oldValue-parts[1].shapeSize).sqrMagnitude<changeThreshold*changeThreshold)
				parts[1].shapeSize = oldValue;
				
			// TODO: mirror rotation if/when collider rotation is implemented
		}
		
		CustomHandleUtilities.SetHandleColor(oldColor);
		
		foreach (BodyPart p in parts) EditorUtility.SetDirty(p);
	}
}