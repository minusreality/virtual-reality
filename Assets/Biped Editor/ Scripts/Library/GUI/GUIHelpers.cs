using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * GUIHelpers
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
 * This class contains a variety of static methods for working with GUI.
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

// describes the corner of the screen to which a rect is anchored
public enum GUIAnchor { LowerLeft, LowerRight, TopLeft, TopRight }

/*
 * A class containing various helper functionsfor working with GUI
 * */
public class GUIHelpers : System.Object
{
	/*
	 * Remaps GUI to mouse position and vice versa
	 * */
	public static Vector3 MouseToGUIPosition(Vector3 v)
	{
		return new Vector3(v.x, Screen.height-v.y, v.z);
	}
	
	/*
	 * Returns the height of one pixel at depth in frustum as world space distance
	 * */
	public static float OnePixelInWorld(Camera camera, float depth)
	{
		return (camera.ScreenToWorldPoint(Vector3.up + Vector3.forward*depth) - camera.ScreenToWorldPoint(Vector3.zero + Vector3.forward*depth)).y;
	}
}