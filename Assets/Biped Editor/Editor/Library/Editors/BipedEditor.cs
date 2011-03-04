using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * BipedEditor
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
 * This script provides custom handles for working with Biped components.
 * 
 * ----------------------------------------------------------------------------
 * Usage
 * ----------------------------------------------------------------------------
 * Put this script in an Editor folder and it should automatically work when
 * you add a Biped to a character.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations
 * ----------------------------------------------------------------------------
 * See BodyPartEditor.cs
 * */

/*
 * Custom scene GUI when a Biped object is selected
 * NOTE: This inspector uses several methods defined in BodyPartEditor
 * */
[CustomEditor(typeof(Biped))]
public class BipedEditor : Editor
{
	// the selected object with the component
	private Biped biped;
	
	// colors for the scene biped
	private Color ballsColor = new Color(1f, 0f, 0f, 0.5f);
	private Color linesColor = new Color(1f, 0f, 0f, 0.5f);
	private Color shapeColor = BodyPartEditor.shapeColor;
	
	// parameters for the handles
	// display size for BodyPart transforms spheres
	public static float ballSize
	{
		get { return _ballSize; }
		set
		{
			PlayerPrefs.SetFloat("Editor - BipedEditor - ballSize", value);
			_ballSize = value;
		}
	}
	private static float _ballSize = PlayerPrefs.GetFloat("Editor - BipedEditor - ballSize", 0.025f);
	
	// preferred naming convention
	public static DCCApplication namingConvention
	{
		get { return _namingConvention; }
		set
		{
			PlayerPrefs.SetInt("Editor - BipedEditor - namingConvention", (int)value);
			_namingConvention = value;
		}
	}
	private static DCCApplication _namingConvention = (DCCApplication)PlayerPrefs.GetInt("Editor - BipedEditor - namingConvention", 2);
	
	/*
	 * Initialize
	 * */
	void OnEnable()
	{
		biped = (Biped) target;
	}
	
	/*
	 * Draw custom viewport render
	 * */
	void OnSceneGUI()
	{
		// only draw handles if array parts are not null, otherwise console spams crap when component is added while object is selected
		if (biped.spine!=null && biped.spine.Length>0)
		{
			// get the current scaleFactor
			Matrix4x4 matrix = biped.transform.localToWorldMatrix;
			float scaleFactor = Mathf.Sqrt(VectorHelpers.MaxValue(new Vector3(
				new Vector3(matrix.m00, matrix.m01, matrix.m02).sqrMagnitude, 
				new Vector3(matrix.m10, matrix.m11, matrix.m12).sqrMagnitude, 
				new Vector3(matrix.m20, matrix.m21, matrix.m22).sqrMagnitude)));
			
			// draw balls and lines for all defined parts
			Handles.color = ballsColor;
			foreach (BodyPart part in biped.allParts) DrawBall(part, scaleFactor);
			
			// draw a shape gizmo for each body part
			Handles.color = shapeColor;
			foreach (BodyPart part in biped.allParts) BodyPartEditor.DrawShapeHandle(part, false);
			
			// draw joint gizmos for all the body parts
			foreach (BodyPart part in biped.allParts) BodyPartEditor.DrawJointHandle(part, false);
		}
		
		// begin GUI or transform handles will be disabled
		Handles.BeginGUI();
		
		// create the viewport controls
		ViewportControls.BeginArea(BodyPartEditor.viewportControlsWidth, GUIAnchor.TopLeft);
		{
			// common controls for BodyParts (shape, center, joints)
			BodyPartEditor.ViewportCommonControls();
			// only update values if they change to prevent constant updating of player pref keys
			float fVal;
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(string.Format("Transform Display Size: {0:0.00}", ballSize), GUILayout.Width(BodyPartEditor.viewportControlsWidth*0.65f));
				fVal = GUILayout.HorizontalSlider(ballSize, 0f, 1f);
				if (fVal!=ballSize) ballSize = fVal;
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(string.Format("Body Mass: {0:0.00}", biped.mass), GUILayout.Width(BodyPartEditor.viewportControlsWidth*0.65f));
				fVal = GUILayout.HorizontalSlider(biped.mass, 1f, 500f);
				if (fVal!=biped.mass)
				{
					biped.mass = fVal;
					biped.DistributeMass();
				}
			}
			GUILayout.EndHorizontal();
			
			// if the editor is playing, then create testing controls
			if (Application.isPlaying)
			{
				// padding after previous controls
				GUILayout.Space(ViewportControls.viewportPadding*2f);
				
				// only display ragdoll controls if minimum requirements have been met
				if (biped.ValidateMinimumRequirements())
				{
					// if the biped is currently ragdoll, then create a button to remove the ragdoll
					if (biped.isRagdoll)
					{
						if (GUILayout.Button("Remove Ragdoll")) biped.RemoveRagdoll();
					}
					// otherwise create a button to turn the biped into a ragdoll
					else
					{
						if (GUILayout.Button("Create Ragdoll")) biped.CreateRagdoll(BodyPartEditor.jointResistance, 1f);
					}
					
					// only update an active ragdoll's spring value if the resistance slider changes
					float oldResist = BodyPartEditor.jointResistance;
					float oldForce = BodyPartEditor.maxForce;
					BodyPartEditor.ViewportResistanceSlider();
					BodyPartEditor.ViewportMaxForceSlider();
					if ((oldResist!=BodyPartEditor.jointResistance || oldForce!=BodyPartEditor.maxForce) && biped.isRagdoll)
					{
						JointDrive drive;
						foreach(BodyPart part in biped.allParts)
						{
							if (part==null || part.joint==null) continue;
							drive = part.joint.slerpDrive;
							drive.maximumForce = BodyPartEditor.maxForce;
							drive.positionSpring = BodyPartEditor.jointResistance;
							part.joint.slerpDrive = drive;
						}
					}
					
					// if the biped is out of ragdoll, include a button to return to the snapshot
					if (!biped.isRagdoll)
					{
						if (GUILayout.Button("Restore Pose to Snapshot"))
						{
							foreach (BodyPart part in biped.allParts)
							{
								if (part==null) continue;
								part.ResetToInitialRotation();
								part.ResetToPositionSnapshot();
							}
						}
					}
				}
				else
				{
					GUILayout.Label("Minimum biped definition not specified. Please stop the game and ensure that biped minimum requirements have been met.");
				}
			}
			// otherwise, create additional setup controls
			else
			{
				GUILayout.Label("Bone Naming Convention:");
				GUILayout.BeginVertical();
				{
					namingConvention = (DCCApplication)GUILayout.SelectionGrid((int)namingConvention, System.Enum.GetNames(typeof(DCCApplication)), 2);
					if (GUILayout.Button(string.Format("Set Up Biped Using {0} Names", namingConvention)))
					{
						biped.AutomateSetup(namingConvention);
					}
				}
				GUILayout.EndVertical();
			}
		}
		ViewportControls.EndArea();
		
		// display symmetry status at the top of the viewport
		BodyPartEditor.ViewportStatus();
		
		// finish GUI
		Handles.EndGUI();
	}
	
	/*
	 * Draw a ball at a part's location
	 * */
	void DrawBall(BodyPart part, float scaleFactor)
	{
		if (part==null) return;
		
		Handles.SphereCap(0, part.bone.position, part.bone.rotation, _ballSize*VectorHelpers.MaxValue(part.bone.root.localScale));
		
		// draw a line to the parent part if it is defined
		Handles.color = linesColor;
		if (part.parentPart!=null)
		{
			Handles.DrawLine(part.bone.position, part.parentPart.bone.position);
		}
	}
	/*
	 * Draw a line from a part to its most immediately-defined ancestory
	 * */
	void DrawLine(BodyPart part, params BodyPart[] ancestors)
	{
		if (part==null) return;
		
		foreach (BodyPart ancestor in ancestors)
		{
			if (ancestor==null) return;
			Handles.DrawLine(part.bone.position, ancestor.bone.position);
			return;
		}
	}
}