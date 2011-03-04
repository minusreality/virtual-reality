using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * ShapeHandles
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
 * This class contains functions for drawing handles to define different shapes
 * for colliders.
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
 * A class for drawing shape handles
 * */
public class ShapeHandles : System.Object
{
	/*
	 * Create a wire cube handle for a specified size at position aligned to orientation
	 * */
	// basic invocation
	public static void WireBox(ref Vector3 size, Vector3 center, Quaternion orientation)
	{
		WireBox(ref size, center, orientation, size.ToString(), Handles.color);
	}
	// manually specify label
	public static void WireBox(ref Vector3 size, Vector3 center, Quaternion orientation, 
		string label)
	{
		WireBox(ref size, center, orientation, label, Handles.color);
	}
	// manually specify color
	public static void WireBox(ref Vector3 size, Vector3 center, Quaternion orientation, 
		Color col)
	{
		WireBox(ref size, center, orientation, size.ToString(), col);
	}
	// manually specify label and color
	public static void WireBox(ref Vector3 size, Vector3 center, Quaternion orientation, 
		string label, Color col)
	{
		// set handle color
		Color oldColor = Handles.color;
		CustomHandleUtilities.SetHandleColor(col);
		
		// TODO: do something with label?
		
		// create linear handles to adjust size
		LinearHandles.ValueSlider(ref size.x, center-orientation*Vector3.right*size.x*0.5f, orientation*Vector3.right);
		LinearHandles.ValueSlider(ref size.x, center+orientation*Vector3.right*size.x*0.5f, orientation*Vector3.left);
		LinearHandles.ValueSlider(ref size.y, center-orientation*Vector3.up*size.y*0.5f, orientation*Vector3.up);
		LinearHandles.ValueSlider(ref size.y, center+orientation*Vector3.up*size.y*0.5f, orientation*Vector3.down);
		LinearHandles.ValueSlider(ref size.z, center-orientation*Vector3.forward*size.z*0.5f, orientation*Vector3.forward);
		LinearHandles.ValueSlider(ref size.z, center+orientation*Vector3.forward*size.z*0.5f, orientation*Vector3.back);
		
		
		// draw the shape
		Vector3[] corners = new Vector3[8];
		corners[0] = center + orientation*(new Vector3( size.x*0.5f, size.y*0.5f, size.z*0.5f));
		corners[1] = center + orientation*(new Vector3( size.x*0.5f,-size.y*0.5f, size.z*0.5f));
		corners[2] = center + orientation*(new Vector3( size.x*0.5f, size.y*0.5f,-size.z*0.5f));
		corners[3] = center + orientation*(new Vector3( size.x*0.5f,-size.y*0.5f,-size.z*0.5f));
		corners[4] = center + orientation*(new Vector3(-size.x*0.5f,-size.y*0.5f,-size.z*0.5f));
		corners[5] = center + orientation*(new Vector3(-size.x*0.5f, size.y*0.5f, size.z*0.5f));
		corners[6] = center + orientation*(new Vector3(-size.x*0.5f,-size.y*0.5f, size.z*0.5f));
		corners[7] = center + orientation*(new Vector3(-size.x*0.5f, size.y*0.5f,-size.z*0.5f));
		Handles.DrawLine(corners[0], corners[1]);
		Handles.DrawLine(corners[0], corners[2]);
		Handles.DrawLine(corners[0], corners[5]);
		Handles.DrawLine(corners[1], corners[3]);
		Handles.DrawLine(corners[1], corners[6]);
		Handles.DrawLine(corners[2], corners[3]);
		Handles.DrawLine(corners[2], corners[7]);
		Handles.DrawLine(corners[3], corners[4]);
		Handles.DrawLine(corners[4], corners[6]);
		Handles.DrawLine(corners[4], corners[7]);
		Handles.DrawLine(corners[5], corners[6]);
		Handles.DrawLine(corners[5], corners[7]);
		
		// reset handle color
		CustomHandleUtilities.SetHandleColor(oldColor);
	}
	
	/*
	 * Create a wire cylinder handle for a specified radius and height at position aligned to orientation
	 * */
	// basic invocation
	public static void WireCylinder(ref float radius, ref float height, Vector3 center, Quaternion orientation)
	{
		CylinderHandle(ref radius, ref height, center, orientation, radius.ToString(), Handles.color, false);
	}
	// manually specify label
	public static void WireCylinder(ref float radius, ref float height, Vector3 center, Quaternion orientation, 
		string label)
	{
		CylinderHandle(ref radius, ref height, center, orientation, label, Handles.color, false);
	}
	// manually specify label and color
	public static void WireCylinder(ref float radius, ref float height, Vector3 center, Quaternion orientation, 
		string label, Color col)
	{
		CylinderHandle(ref radius, ref height, center, orientation, label, col, false);
	}
	// manually specify color
	public static void WireCylinder(ref float radius, ref float height, Vector3 center, Quaternion orientation, 
		Color col)
	{
		CylinderHandle(ref radius, ref height, center, orientation, radius.ToString(), col, false);
	}
	
	/*
	 * Create a wire capsule handle for a specified radius and height at position aligned to orientation
	 * */
	// basic invocation
	public static void WireCapsule(ref float radius, ref float height, Vector3 center, Quaternion orientation)
	{
		CylinderHandle(ref radius, ref height, center, orientation, radius.ToString(), Handles.color, true);
	}
	// manually specify label
	public static void WireCapsule(ref float radius, ref float height, Vector3 center, Quaternion orientation, 
		string label)
	{
		CylinderHandle(ref radius, ref height, center, orientation, label, Handles.color, true);
	}
	// manually specify label and color
	public static void WireCapsule(ref float radius, ref float height, Vector3 center, Quaternion orientation, 
		string label, Color col)
	{
		CylinderHandle(ref radius, ref height, center, orientation, label, col, true);
	}
	// manually specify color
	public static void WireCapsule(ref float radius, ref float height, Vector3 center, Quaternion orientation, 
		Color col)
	{
		CylinderHandle(ref radius, ref height, center, orientation, radius.ToString(), col, true);
	}
	
	/*
	 * Create a disc handle for radius at t using the specified parameters
	 * */
	private static void CylinderHandle(ref float radius, ref float height, Vector3 center, Quaternion orientation, string label, Color col, bool isCapsule)
	{
		// clamp values
		radius = Mathf.Abs(radius);
		height = Mathf.Abs(height);
		
		float oldRadius = radius;
		
		// TODO: do something with label?
		
		// set handle color
		Color oldColor = Handles.color;
		CustomHandleUtilities.SetHandleColor(col);
		
		// compute disc handle locations based on orientation
		Vector3 right = orientation*Vector3.right*radius;
		Vector3 up = orientation*Vector3.up*height*0.5f;
		Vector3 forward = orientation*Vector3.forward*radius;
		
		// create a disc handle at each end of the cylinder
		Vector3 upperPoint = center+(orientation*Vector3.up)*(height*0.5f-(isCapsule?radius:0f));
		Vector3 lowerPoint = center+(orientation*Vector3.up)*(height*-0.5f+(isCapsule?radius:0f));
		if (!isCapsule)
		{
			DiscHandles.WireDisc(ref radius, upperPoint, orientation, label, col);
			DiscHandles.WireDisc(ref radius, lowerPoint, orientation, label, col);
		}
		else
		{
			Handles.DrawWireDisc(upperPoint, up, radius);
			Handles.DrawWireDisc(lowerPoint, up, radius);
			DiscHandles.WireDisc(ref radius, center, orientation, label, col);
		}
				
		// draw a line connecting the handles to visualize the height
		Handles.DrawLine(upperPoint+forward, lowerPoint+forward);
		Handles.DrawLine(upperPoint-forward, lowerPoint-forward);
		Handles.DrawLine(upperPoint+right, lowerPoint+right);
		Handles.DrawLine(upperPoint-right, lowerPoint-right);
		
		// create a linear handles to adjust height
		LinearHandles.ValueSlider(ref height, center-up, up);
		LinearHandles.ValueSlider(ref height, center+up, -up);
		
		// create caps if requested
		if (isCapsule)
		{
			Handles.DrawWireArc(center-up+up.normalized*radius, right, forward, 180f, radius);
			Handles.DrawWireArc(center-up+up.normalized*radius, forward, right, -180f, radius);
			Handles.DrawWireArc(center+up-up.normalized*radius, right, forward, -180f, radius);
			Handles.DrawWireArc(center+up-up.normalized*radius, forward, right, 180f, radius);
			
			// ensure that height and radius values are valid
			if (radius > height*0.5f)
			{
				// user was operating a radius handle
				if (radius > oldRadius) radius = height*0.5f;
				// user was operating a height handle
				else height = radius*2f;
			}
		}
		
		// reset handle color
		CustomHandleUtilities.SetHandleColor(oldColor);
	}
}