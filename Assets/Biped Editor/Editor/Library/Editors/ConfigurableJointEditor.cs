using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * ConfigurableJointEditor
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
 * This script provides custom handles for working with ConfigurableJoint
 * components.
 * 
 * ----------------------------------------------------------------------------
 * Usage
 * ----------------------------------------------------------------------------
 * Put this script in an Editor folder and it should automatically work when
 * you add a ConfigurableJoint to an object.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations
 * ----------------------------------------------------------------------------
 * See JointHandle.cs
 * */

/*
 * Custom scene GUI and inspector when a MeshRibbon object is selected
 * */
[CustomEditor(typeof(ConfigurableJoint))]
public class ConfigurableJointEditor : Editor
{
	// the target object
	static ConfigurableJoint joint;
	
	// is the handle currently enabled?
	public static bool isHandleEnabled
	{
		get { return _isHandleEnabled; }
		set
		{
			PlayerPrefs.SetInt("Editor - ConfigurableJointEditor - isHandleEnabled", (value)?1:0);
			_isHandleEnabled = value;
		}
	}
	private static bool _isHandleEnabled = PlayerPrefs.GetInt("Editor - ConfigurableJointEditor - isHandleEnabled", 1) == 1;
	
	// the size for the joint handle
	public static float jointHandleSize
	{
		get { return _jointHandleSize; }
		set
		{
			PlayerPrefs.SetFloat("Editor - ConfigurableJointEditor - jointHandleSize", value);
			_jointHandleSize = value;
		}
	}
	private static float _jointHandleSize = PlayerPrefs.GetFloat("Editor - ConfigurableJointEditor - jointHandleSize", 0.2f);
	
	// the width for the on-screen controls
	public static float viewportControlsWidth = 240f;
	
	/*
	 * Initialize
	 * */
	void OnEnable()
	{
		joint = (ConfigurableJoint) target;
	}
	
	/*
	 * Draw the handle if it is enabled
	 * */
	void OnSceneGUI()
	{
		// viewport gui controls
		Handles.BeginGUI();
		{
			ViewportControls.BeginArea(viewportControlsWidth, GUIAnchor.TopLeft);
			{
				GUILayout.BeginVertical();
				{
					JointHandleToggle();
					GUILayout.BeginHorizontal();
					{
						JointHandleSizeSlider(viewportControlsWidth);
					}
					GUILayout.EndHorizontal();
					CustomHandleUtilities.ViewportIntegratorFidelityControls(viewportControlsWidth);
				}
				GUILayout.EndVertical();
			}
			ViewportControls.EndArea();
		}
		Handles.EndGUI();
		
		// handles
		if (!isHandleEnabled) return;
				
		Undo.SetSnapshotTarget(target, "Change Configurable Joint");
		JointHandles.JointLimit(joint, jointHandleSize);
		EditorUtility.SetDirty(target);
	}
	
	/*
	 * A toggle button for the joint handle
	 * */
	public static void JointHandleToggle()
	{
		bool bVal = ViewportControls.OnOffToggle("Joint Limit Handles:", isHandleEnabled);
		if (bVal!=isHandleEnabled) isHandleEnabled = bVal;
	}
	
	/*
	 * A slider to adjust the joint handle size
	 * */
	public static void JointHandleSizeSlider(float width)
	{
		float fVal = jointHandleSize;
		GUILayout.Label(string.Format("Joint Handle Size: {0:0.00}", jointHandleSize), GUILayout.Width(0.65f*width));
		fVal = GUILayout.HorizontalSlider(fVal, 0f, 2f);
		if (fVal != jointHandleSize) jointHandleSize = fVal;
	}
}