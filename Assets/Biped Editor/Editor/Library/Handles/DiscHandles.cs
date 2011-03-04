using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * DiscHandles
 * Version: 1.02
 * Date: 2010.12.24
 * Author: Adam Mechtley (http://adammechtley.com)
 * License: Please don't steal or redistribute this code; I have a family. If
 * you did steal it, at least consider making a donation:
 * http://bit.ly/adammechtley_donate
 * 
 * ----------------------------------------------------------------------------
 * Description
 * ----------------------------------------------------------------------------
 * This class contains a variety of functions for creating disc handles.
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
 * A class for creating disc handles
 * */
public class DiscHandles : System.Object
{	
	/*
	 * Create a wire disc handle for a target value at position
	 * */
	// basic invocation
	public static void WireDisc(ref float val, Vector3 origin)
	{
		DiscHandle(ref val, origin, Quaternion.identity, val.ToString(), Handles.color, false);
	}
	// manually specify orientation
	public static void WireDisc(ref float val, Vector3 origin, 
		Quaternion orientation)
	{
		DiscHandle(ref val, origin, orientation, val.ToString(), Handles.color, false);
	}
	// manually specify orientation and color
	public static void WireDisc(ref float val, Vector3 origin, 
		Quaternion orientation, Color col)
	{
		DiscHandle(ref val, origin, orientation, val.ToString(), col, false);
	}
	// manually specify orientation and label
	public static void WireDisc(ref float val, Vector3 origin, 
		Quaternion orientation, string label)
	{
		DiscHandle(ref val, origin, orientation, label, Handles.color, false);
	}
	// manually specify label
	public static void WireDisc(ref float val, Vector3 origin, 
		string label)
	{
		DiscHandle(ref val, origin, Quaternion.identity, label, Handles.color, false);
	}
	// manually specify label and color
	public static void WireDisc(ref float val, Vector3 origin, 
		string label, Color col)
	{
		DiscHandle(ref val, origin, Quaternion.identity, label, col, false);
	}
	// manually specify color
	public static void WireDisc(ref float val, Vector3 origin, 
		Color col)
	{
		DiscHandle(ref val, origin, Quaternion.identity, val.ToString(), col, false);
	}
	// manually specify orientation, label, and color
	public static void WireDisc(ref float val, Vector3 origin, 
		Quaternion orientation, string label, Color col)
	{
		DiscHandle(ref val, origin, orientation, label, col, false);
	}
	
	/*
	 * Create a solid disc handle for a target value at position
	 * */
	// basic invocation
	public static void SolidDisc(ref float val, Vector3 origin)
	{
		DiscHandle(ref val, origin, Quaternion.identity, val.ToString(), Handles.color, true);
	}
	// manually specify orientation
	public static void SolidDisc(ref float val, Vector3 origin, 
		Quaternion orientation)
	{
		DiscHandle(ref val, origin, orientation, val.ToString(), Handles.color, true);
	}
	// manually specify orientation and color
	public static void SolidDisc(ref float val, Vector3 origin, 
		Quaternion orientation, Color col)
	{
		DiscHandle(ref val, origin, orientation, val.ToString(), col, true);
	}
	// manually specify orientation and label
	public static void SolidDisc(ref float val, Vector3 origin, 
		Quaternion orientation, string label)
	{
		DiscHandle(ref val, origin, orientation, label, Handles.color, true);
	}
	// manually specify label
	public static void SolidDisc(ref float val, Vector3 origin, 
		string label)
	{
		DiscHandle(ref val, origin, Quaternion.identity, label, Handles.color, true);
	}
	// manually specify label and color
	public static void SolidDisc(ref float val, Vector3 origin, 
		string label, Color col)
	{
		DiscHandle(ref val, origin, Quaternion.identity, label, col, true);
	}
	// manually specify color
	public static void SolidDisc(ref float val, Vector3 origin, 
		Color col)
	{
		DiscHandle(ref val, origin, Quaternion.identity, val.ToString(), col, true);
	}
	// manually specify orientation, label, and color
	public static void SolidDisc(ref float val, Vector3 origin, 
		Quaternion orientation, string label, Color col)
	{
		DiscHandle(ref val, origin, orientation, label, col, true);
	}
		
	/*
	 * Create a disc handle for val using the specified parameters
	 * */
	private static void DiscHandle(ref float val, Vector3 origin, Quaternion orientation, string label, Color col, bool isFilled)
	{		
		// compute handle locations based on disc orientation
		Vector3 right = orientation*Vector3.right;
		Vector3 up = orientation*Vector3.up;
		Vector3 forward = orientation*Vector3.forward;
		
		// set handle color
		Color oldColor = Handles.color;
		CustomHandleUtilities.SetHandleColor(col); // ensure it will still render even if alpha is 0
		
		// create handles
		LinearHandles.ValueSlider(ref val, origin, forward, label, Handles.color);
		LinearHandles.ValueSlider(ref val, origin, right, Handles.color);
		LinearHandles.ValueSlider(ref val, origin, -forward, Handles.color);
		LinearHandles.ValueSlider(ref val, origin, -right, Handles.color);
		
		// draw disc
		CustomHandleUtilities.SetHandleColor(col, col.a*0.5f); // ensure it will still render even if alpha is 0
		Handles.DrawWireDisc(origin, up, val);
		
		// optionally fill the disc
		if (isFilled)
		{
			CustomHandleUtilities.SetHandleColor(col, col.a*0.1f);
			Handles.DrawSolidDisc(origin, up, val);
		}
		
		// reset handle color
		CustomHandleUtilities.SetHandleColor(oldColor);
	}
	
	// the smallest change in an arc handle's angle that will trigger a value change
	// if this number is 0, then the handle risks continually changing a prefab value
	public static float requiredMinAngleChange = 0.00001f;
	
	/*
	 * Create an arc handle for setting an angular value
	 * */
	public static void Arc(ref float val, Vector3 origin, float radius, Quaternion orientation, string label, Color col, bool isFilled, bool isRing)
	{		
		// compute handle locations based on disc orientation
		Vector3 right = orientation*Vector3.right;
		Vector3 up = orientation*Vector3.up;
		Vector3 forward = orientation*Vector3.forward;
		
		// set handle color
		Color oldColor = Handles.color;
		CustomHandleUtilities.SetHandleColor(col);
		
		// create handle
		Vector3 handlePosition = origin + Quaternion.AngleAxis(val, up)*forward*radius;
		Vector3 handleDirection = Quaternion.AngleAxis(val, up)*right;
		Handles.DrawLine(origin, handlePosition);
		Vector3 vDelta = Handles.Slider(handlePosition, handleDirection, HandleUtility.GetHandleSize(handlePosition)*LinearHandles.dotHandleSize, Handles.DotCap, 1f)-handlePosition;
		
		if (vDelta.magnitude > requiredMinAngleChange) val += vDelta.magnitude * Mathf.Sign(Vector3.Dot(vDelta, handleDirection));
		
		// draw disc
		if (isRing)
		{
			CustomHandleUtilities.SetHandleColor(col);
			Handles.DrawWireDisc(origin, up, radius);
		}
		
		// fill arc
		if (isFilled)
		{
			CustomHandleUtilities.SetHandleColor(col, col.a*0.1f);
			Handles.DrawSolidArc(origin, up, forward, val, radius);
		}
		
		// draw the label if requested
		if (label.Length > 0) Handles.Label(origin+Quaternion.AngleAxis(val, up)*forward*radius, label);
		
		// reset handle color
		CustomHandleUtilities.SetHandleColor(oldColor);
	}
	
	/*
	 * Builds a quaternion at position; return value represents a disc oriented to the screen (its normal is screen-facing, its forward is up)
	 * */
	public static Quaternion ScreenSpaceOrientation(Vector3 position)
	{
		// world position in screen space
		Vector2 p = HandleUtility.WorldToGUIPoint(position);
				
		// up orientation is is a ray form the camera through the transform
		Vector3 up = -Camera.current.ScreenPointToRay(p).direction;
		
		// forward orientation is straight up form the transform position
		p.y+=10f;
		Vector3 forward = Camera.current.ScreenToWorldPoint(new Vector3(p.x, p.y, Camera.current.farClipPlane))-position;
		
		return Quaternion.LookRotation(up, forward)*Quaternion.AngleAxis(90f, Vector3.left);
	}
	
	/*
	 * Creates a wire sphere handle for a target value at a transform
	 * */
	// basic invocation
	public static void WireSphere(ref float val, Vector3 origin)
	{
		WireSphere(ref val, origin, "", Handles.color);
	}
	// manually specify color
	public static void WireSphere(ref float val, Vector3 origin, 
		Color col)
	{
		WireSphere(ref val, origin, "", col);
	}
	// manually specify a label
	public static void WireSphere(ref float val, Vector3 origin, 
		string label)
	{
		WireSphere(ref val, origin, label, Handles.color);
	}
	// manually specify a color and a label
	public static void WireSphere(ref float val, Vector3 origin, 
		string label, Color col)
	{
		Quaternion orientation = ScreenSpaceOrientation(origin);
		WireDisc(ref val, origin, orientation, label, col);
	}
	
	/*
	 * Creates a solid sphere handle for a target value at a transform
	 * */
	// basic invocation
	public static void SolidSphere(ref float val, Vector3 origin)
	{
		SolidSphere(ref val, origin, "", Handles.color);
	}
	// manually specify a color
	public static void SolidSphere(ref float val, Vector3 origin, 
		Color col)
	{
		SolidSphere(ref val, origin, "", col);
	}
	// manually specify a label
	public static void SolidSphere(ref float val, Vector3 origin, 
		string label)
	{
		SolidSphere(ref val, origin, label, Handles.color);
	}
	// manually specify a color and a label
	public static void SolidSphere(ref float val, Vector3 origin, 
		string label, Color col)
	{
		Quaternion orientation = ScreenSpaceOrientation(origin);
		SolidDisc(ref val, origin, orientation, label, col);
	}
}