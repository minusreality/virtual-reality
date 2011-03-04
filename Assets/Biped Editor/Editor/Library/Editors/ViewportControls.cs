using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * ViewportControls
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
 * This class contains static methods for common viewport controls.
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
 * A class containing helpers for common viewport controls
 * */
public class ViewportControls : System.Object
{
	// a pixel amount to describe padding in from an edge
	public static float viewportPadding = 5f;
	
	public static string[] onOffLabels = new string[2] {"Off", "On"};
	
	/*
	 * A wrapper for GUILayout.BeginArea that automatically handles sizing
	 * */
	public static void BeginArea(float width, float height, GUIAnchor anchor)
	{
		// ensure width does not exceed screen area
		width = Mathf.Min(width, Screen.width-2f*viewportPadding);
		
		// begin the area
		GUILayout.BeginArea(new Rect (
			(anchor==GUIAnchor.LowerLeft||anchor==GUIAnchor.TopLeft)?viewportPadding:Screen.width-viewportPadding-width, 
			(anchor==GUIAnchor.TopLeft||anchor==GUIAnchor.TopRight)?viewportPadding:Screen.height-viewportPadding-height, 
			width, 
			height));
	}
	public static void BeginArea(float width, GUIAnchor anchor)
	{
		BeginArea(width, Screen.height-2f*viewportPadding, anchor);
	}
	
	/*
	 * A wrapper for GUILayout.EndArea()
	 * */
	public static void EndArea()
	{
		GUILayout.EndArea();
	}
	
	/*
	 * Return the value of an on/off switch
	 * */
	public static bool OnOffToggle(string label, bool val)
	{
		GUILayout.BeginHorizontal();
			GUILayout.Label(label);
			val = GUILayout.SelectionGrid((!val)?0:1, onOffLabels, onOffLabels.Length, GUILayout.Width(80f))==1;
		GUILayout.EndHorizontal();
		return val;
	}
}