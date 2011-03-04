using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * CustomHandleUtilities
 * Version: 1.01
 * Date: 2010.11.07
 * Author: Adam Mechtley (http://adammechtley.com)
 * License: Please don't steal or redistribute this code; I have a family. If
 * you did steal it, at least consider making a donation:
 * http://bit.ly/adammechtley_donate
 * 
 * ----------------------------------------------------------------------------
 * Description
 * ----------------------------------------------------------------------------
 * This class contains a variety of static methods for working with Handles.
 * 
 * ----------------------------------------------------------------------------
 * Usage
 * ----------------------------------------------------------------------------
 * Call the functions you need, bro.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations
 * ----------------------------------------------------------------------------
 * N/A
 * */

/*
 * A class for working with Handles
 * */
public class CustomHandleUtilities : System.Object
{
	// attempt to mimic the color of Unity's handles
	public static Color xHandleColor { get { return new Color(0.95f, 0.28f, 0.137f, 1f); } }
	public static Color yHandleColor { get { return new Color(0.733f, 0.95f, 0.337f, 1f); } }
	public static Color zHandleColor { get { return new Color(0.255f, 0.553f, 0.95f, 1f); } }
	
	/*
	 * Set the current handle color with optional alpha
	 * */
	public static void SetHandleColor(Color col)
	{
		SetHandleColor(col, col.a);
	}
	public static void SetHandleColor(Color col, float a)
	{
		col.a = a;
		Handles.color = col;
	}
	
	// static constant for adjusting integrator fidelity
	public static float integratorFidelity
	{
		get { return _integratorFidelity; }
		set
		{
			PlayerPrefs.SetFloat("Editor - CustomHandleUtilities - integratorFidelity", value);
			_integratorFidelity = value;
		}
	}
	private static float _integratorFidelity = PlayerPrefs.GetFloat("Editor - CustomHandleUtilities - integratorFidelity", 0.003f); // lower number is higher fidelity
	
	/*
	 * Compute the fidelity of the integrator for filling gradients and drawing graphs based on the camera's distance to focalPoint
	 * */
	public static float GetIntegratorStep(Vector3 focalPoint, float minDistance)
	{
		// integrator fidelity is only maintained outside the minimum distance in order to improve performance when zoomed in
		return integratorFidelity*Mathf.Max(1f/HandleUtility.GetHandleSize(focalPoint), minDistance);
	}
	
	/*
	 * Display viewport controls for adjusting integrator fidelity
	 * */
	public static void ViewportIntegratorFidelityControls(float width)
	{
		// only update values if they change to prevent constant updating of player pref keys
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(string.Format("Integrator Fidelity: {0:0.####}", integratorFidelity), GUILayout.Width(width*0.65f));
			float fVal = GUILayout.HorizontalSlider(integratorFidelity, 0.001f, 0.005f);
			if (fVal!=integratorFidelity) integratorFidelity = fVal;
		}
		GUILayout.EndHorizontal();
	}
}