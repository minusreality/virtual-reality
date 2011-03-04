using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * LinearHandles
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
 * This class contains functions for drawing linear handles.
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
 * A class for drawing linear handles
 * */
public class LinearHandles : System.Object
{
	// the default direction in which linear sliders will be created
	public static Vector3 defaultLinearSliderDirection = Vector3.up;
	
	// the size for little rectangular handles
	public static float dotHandleSize = 0.03f;
	
	// the smallest change in a handle's 3D position that will trigger a value change
	// if this number is 0, then many linear handles must transform along a cardinal axis or risk continually changing a prefab value
	public static float requiredMinHandleChange = 0.00001f;
	
	/*
	 * Create a handle for a linear value slider
	 * */
	// basic invocation
	public static void ValueSlider(ref float val)
	{
		ValueSlider(ref val, Vector3.zero, defaultLinearSliderDirection, "", Handles.color);
	}
	// manually specify an origin and direction
	public static void ValueSlider(ref float val, Vector3 origin, Vector3 direction)
	{
		ValueSlider(ref val, origin, direction, "", Handles.color);
	}
	// manually specify an origin, direction, and label
	public static void ValueSlider(ref float val, Vector3 origin, Vector3 direction, string label)
	{
		ValueSlider(ref val, origin, direction, label, Handles.color);
	}
	// manually specify an origin, direction, and color
	public static void ValueSlider(ref float val, Vector3 origin, Vector3 direction, Color col)
	{
		ValueSlider(ref val, origin, direction, "", col);
	}
	// manually specify a label
	public static void ValueSlider(ref float val, string label)
	{
		ValueSlider(ref val, Vector3.zero, defaultLinearSliderDirection, label, Handles.color);
	}
	// manually specify a label and a color
	public static void ValueSlider(ref float val, string label, Color col)
	{
		ValueSlider(ref val, Vector3.zero, defaultLinearSliderDirection, label, col);
	}
	// manually specify a color
	public static void ValueSlider(ref float val, Color col)
	{
		ValueSlider(ref val, Vector3.zero, defaultLinearSliderDirection, "", col);
	}
	// manually specify an origin, direction, label, and color
	public static void ValueSlider(ref float val, Vector3 origin, Vector3 direction, string label, Color col)
	{
		// set handle color
		Color oldColor = Handles.color;
		CustomHandleUtilities.SetHandleColor(col); // always need handles to be visible
				
		// draw a label if requested
		if (label.Length>0) Handles.Label(origin+direction*val, label);
		
		// create a slider
		direction.Normalize();
		Vector3 initialPosition = origin+direction*val;
		Vector3 vDelta = Handles.Slider(initialPosition, direction, HandleUtility.GetHandleSize(initialPosition)*dotHandleSize, Handles.DotCap, 1f) - initialPosition;
		float delta = vDelta.magnitude;
		if (delta > requiredMinHandleChange) val += delta * Mathf.Sign(Vector3.Dot(vDelta, direction));
		
		// reset handle color
		CustomHandleUtilities.SetHandleColor(oldColor);
	}
	
	/*
	 * Create a linear handle for target's val relative to t
	 * */
	// basic invocation
	public static void Line(ref float val, Vector3 origin)
	{
		Line(ref val, origin, defaultLinearSliderDirection, "", Handles.color);
	}
	// manually specify  direction
	public static void Line(ref float val, Vector3 origin, Vector3 direction)
	{
		Line(ref val, origin, direction, "", Handles.color);
	}
	// manually specify direction and label
	public static void Line(ref float val, Vector3 origin, Vector3 direction, string label)
	{
		Line(ref val, origin, direction, label, Handles.color);
	}
	// manually specify direction and color
	public static void Line(ref float val, Vector3 origin, Vector3 direction, Color col)
	{
		Line(ref val, origin, direction, "", col);
	}
	// manually specify a label
	public static void Line(ref float val, Vector3 origin, string label)
	{
		Line(ref val, origin, defaultLinearSliderDirection, label, Handles.color);
	}
	// manually specify a label and color
	public static void Line(ref float val, Vector3 origin, string label, Color col)
	{
		Line(ref val, origin, defaultLinearSliderDirection, "", col);
	}
	// manually specify a color
	public static void Line(ref float val, Vector3 origin, Color col)
	{
		Line(ref val, origin, defaultLinearSliderDirection, "", col);
	}
	// manually specify direction, label, and color
	public static void Line(ref float val, Vector3 origin, Vector3 direction, string label, Color col)
	{		
		// set handle color
		Color oldColor = Handles.color;
		CustomHandleUtilities.SetHandleColor(col);
		
		// create handle
		ValueSlider(ref val, origin, direction, label, Handles.color);
		
		// draw line
		CustomHandleUtilities.SetHandleColor(col);
		Handles.DrawLine(origin, origin+direction*val);
		
		// reset handle color
		CustomHandleUtilities.SetHandleColor(oldColor);
	}
	
	public static float arrowSize = 0.5f;
	public static float blobSize = 0.1f;
	
	/*
	 * Create a linear slider arrow with an optional blob
	 * */
	// basic invocation
	public static void Arrow(ref float val, Vector3 origin, bool isBlob)
	{
		Arrow(ref val, origin, isBlob, defaultLinearSliderDirection, "", Handles.color);
	}
	// manually specify  direction
	public static void Arrow(ref float val, Vector3 origin, bool isBlob, Vector3 direction)
	{
		Arrow(ref val, origin, isBlob, direction, "", Handles.color);
	}
	// manually specify direction and label
	public static void Arrow(ref float val, Vector3 origin, bool isBlob, Vector3 direction, string label)
	{
		Arrow(ref val, origin, isBlob, direction, label, Handles.color);
	}
	// manually specify direction and color
	public static void Arrow(ref float val, Vector3 origin, bool isBlob, Vector3 direction, Color col)
	{
		Arrow(ref val, origin, isBlob, direction, "", col);
	}
	// manually specify a label
	public static void Arrow(ref float val, Vector3 origin, bool isBlob, string label)
	{
		Arrow(ref val, origin, isBlob, defaultLinearSliderDirection, label, Handles.color);
	}
	// manually specify a label and color
	public static void Arrow(ref float val, Vector3 origin, bool isBlob, string label, Color col)
	{
		Arrow(ref val, origin, isBlob, defaultLinearSliderDirection, "", col);
	}
	// manually specify a color
	public static void Arrow(ref float val, Vector3 origin, bool isBlob, Color col)
	{
		Arrow(ref val, origin, isBlob, defaultLinearSliderDirection, "", col);
	}
	// manually specify direction, label, and color
	public static void Arrow(ref float val, Vector3 origin, bool isBlob, Vector3 direction, string label, Color col)
	{
		// set handle color
		Color oldColor = Handles.color;
		CustomHandleUtilities.SetHandleColor(col);
		
		// draw a label if requested
		if (label.Length>0) Handles.Label(origin+direction*val, label);
		
		// create a slider
		direction.Normalize();
		Vector3 initialPosition = origin+direction*val;
		Vector3 vDelta = Handles.Slider(initialPosition, direction, HandleUtility.GetHandleSize(initialPosition)*arrowSize, Handles.ArrowCap, 1f) - initialPosition;
		float delta = vDelta.magnitude;
		if (delta > requiredMinHandleChange) val += delta * Mathf.Sign(Vector3.Dot(vDelta, direction));
		
		// draw a blob
		if (isBlob) Handles.SphereCap(-1, origin+direction*val, Quaternion.identity, blobSize);
		
		// reset handle color
		CustomHandleUtilities.SetHandleColor(oldColor);
	}
		
	/*
	 * Create a linear slider cone
	 * */
	// basic invocation
	public static void Cone(ref float val, Vector3 origin, float scale)
	{
		Cone(ref val, origin, scale, defaultLinearSliderDirection, "", Handles.color);
	}
	// manually specify  direction
	public static void Cone(ref float val, Vector3 origin, float scale, Vector3 direction)
	{
		Cone(ref val, origin, scale, direction, "", Handles.color);
	}
	// manually specify direction and label
	public static void Cone(ref float val, Vector3 origin, float scale, Vector3 direction, string label)
	{
		Cone(ref val, origin, scale, direction, label, Handles.color);
	}
	// manually specify direction and color
	public static void Cone(ref float val, Vector3 origin, float scale, Vector3 direction, Color col)
	{
		Cone(ref val, origin, scale, direction, "", col);
	}
	// manually specify a label
	public static void Cone(ref float val, Vector3 origin, float scale, string label)
	{
		Cone(ref val, origin, scale, defaultLinearSliderDirection, label, Handles.color);
	}
	// manually specify a label and color
	public static void Cone(ref float val, Vector3 origin, float scale, string label, Color col)
	{
		Cone(ref val, origin, scale, defaultLinearSliderDirection, "", col);
	}
	// manually specify a color
	public static void Cone(ref float val, Vector3 origin, float scale, Color col)
	{
		Cone(ref val, origin, scale, defaultLinearSliderDirection, "", col);
	}
	// manually specify direction, label, and color
	public static void Cone(ref float val, Vector3 origin, float scale, Vector3 direction, string label, Color col)
	{	
		// set handle color
		Color oldColor = Handles.color;
		CustomHandleUtilities.SetHandleColor(col);
		
		// draw a label if requested
		if (label.Length>0) Handles.Label(origin+direction*val, label);
		
		// create a slider
		direction.Normalize();
		Vector3 initialPosition = origin+direction*val;
		Vector3 vDelta = Handles.Slider(initialPosition, direction, HandleUtility.GetHandleSize(initialPosition)*scale, Handles.ConeCap, 1f) - initialPosition;
		float delta = vDelta.magnitude;
		if (delta > requiredMinHandleChange) val += delta * Mathf.Sign(Vector3.Dot(vDelta, direction));
		
		// reset handle color
		CustomHandleUtilities.SetHandleColor(oldColor);
	}
}