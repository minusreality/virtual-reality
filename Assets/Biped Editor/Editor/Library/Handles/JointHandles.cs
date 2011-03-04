using UnityEditor;
using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * JointHandles
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
 * This class contains functions for drawing ConfigurableJoint handles.
 * 
 * ----------------------------------------------------------------------------
 * Usage
 * ----------------------------------------------------------------------------
 * Call the functions you need, bro.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations
 * ----------------------------------------------------------------------------
 * Requires Interpolate class from unifycommunity wiki for joint handle:
 * http://www.unifycommunity.com/wiki/index.php?title=Interpolate
 * */

/*
 * A class for drawing joint handles
 * */
public class JointHandles : System.Object
{
	/*
	 * Creates a joint limit handle
	 * */
	// basic invocation using a joint
	public static void JointLimit(ConfigurableJoint joint, float scale)
	{
		JointLimit(joint, scale, 1f);
	}
	// invocation using a joint and specifying an alpha value
	public static void JointLimit(ConfigurableJoint joint, float scale, float alpha)
	{
		float xMin = joint.lowAngularXLimit.limit;
		float xMax = joint.highAngularXLimit.limit;
		float yMax = joint.angularYLimit.limit;
		float zMax = joint.angularZLimit.limit;
		JointLimit(ref xMin, ref xMax, ref yMax, ref zMax, joint.transform.TransformPoint(joint.anchor), joint.transform.rotation, joint.axis, joint.secondaryAxis, scale, alpha);
		SoftJointLimit limit = joint.lowAngularXLimit; limit.limit = xMin; joint.lowAngularXLimit = limit;
		limit = joint.highAngularXLimit; limit.limit = xMax; joint.highAngularXLimit = limit;
		limit = joint.angularYLimit; limit.limit = yMax; joint.angularYLimit = limit;
		limit = joint.angularZLimit; limit.limit = zMax; joint.angularZLimit = limit;
	}
	// basic invocation using float values
	public static void JointLimit(ref float xMin, ref float xMax, ref float yMax, ref float zMax, 
		Vector3 origin, Quaternion orientation, Vector3 axis, Vector3 secondaryAxis, float scale)
	{
		JointLimit(ref xMin, ref xMax, ref yMax, ref zMax, origin, orientation, axis, secondaryAxis, scale, 1f);
	}
	// invocation using float values and specifying an alpha value
	public static void JointLimit(ref float xMin, ref float xMax, ref float yMax, ref float zMax, 
		Vector3 origin, Quaternion orientation, Vector3 axis, Vector3 secondaryAxis, 
		float scale, float alpha)
	{	
		// ConfigurableJoint defaults to Vector3.right if axis is Vector3.zero - contrary to documentation
		axis = (axis.sqrMagnitude>0f)?axis:Vector3.right;
		
		// if secondaryAxis is Vector3.zero, then it defaults to Vector.up
		secondaryAxis = (secondaryAxis.sqrMagnitude>0f)?secondaryAxis:Vector3.up;
		
		// if both secondaryAxis and axis are the same
		secondaryAxis = (Mathf.Abs(Vector3.Dot(axis,secondaryAxis))==1f)?Vector3.right:secondaryAxis;
		
		// normalize axes
		axis.Normalize();
		secondaryAxis.Normalize();
		
		// on a ConfigurableJoint, secondary axis is used for nothing if it the same as primary axis
		bool isSecondaryAxisValid = !(Mathf.Abs(Vector3.Dot(axis,secondaryAxis))==1f);
		// on a ConfigurableJoint, secondary axis is re-orthogonalized from Vector3.up or Vector3.forward (if axis is Vector3.up)
		if (!isSecondaryAxisValid) secondaryAxis = (Mathf.Abs(Vector3.Dot(axis,Vector3.up))==1f)?Vector3.forward:Vector3.up;
		// compute the third axis
		Vector3 tertiaryAxis = Vector3.Cross(axis, secondaryAxis);
		// orthogonalize secondary axis
		secondaryAxis = Vector3.Cross(tertiaryAxis, axis);
		
		// colors for each handle
		Color xLimitColor = Color.red; xLimitColor.a = alpha;
		Color yLimitColor = Color.green; yLimitColor.a = alpha;
		Color zLimitColor = Color.blue; zLimitColor.a = alpha;
		
		CustomHandleUtilities.SetHandleColor(xLimitColor);
		Handles.ArrowCap(0, origin, Quaternion.LookRotation(orientation*axis), scale*0.2f);
		CustomHandleUtilities.SetHandleColor(yLimitColor);
		Handles.ArrowCap(0, origin, Quaternion.LookRotation(orientation*secondaryAxis), scale*0.2f);
		CustomHandleUtilities.SetHandleColor(zLimitColor);
		Handles.ArrowCap(0, origin, Quaternion.LookRotation(orientation*tertiaryAxis), scale*0.2f);
		
		// xMin/xMax Handles
		Quaternion handleOffset = Quaternion.LookRotation(tertiaryAxis, axis); // offset from orientation into handle's plane
		Quaternion handleOrientation = orientation*handleOffset; // composite orientation of the handle
		float val = -xMin;
		DiscHandles.Arc(ref val, origin, scale, handleOrientation, "", xLimitColor, false, false);
		xMin = Mathf.Min(-val, xMax);
		val = -xMax;
		DiscHandles.Arc(ref val, origin, scale, handleOrientation, "", xLimitColor, false, false);
		xMax = Mathf.Max(-val, xMin);
		CustomHandleUtilities.SetHandleColor(xLimitColor, xLimitColor.a*0.1f);
		Vector3 xHandle1 = orientation*Quaternion.AngleAxis(-xMin, axis)*handleOffset*Vector3.forward;
		Vector3 xHandle2 = orientation*Quaternion.AngleAxis(-xMax, axis)*handleOffset*Vector3.forward;
		
		// yMax Handles
		handleOffset = Quaternion.LookRotation(tertiaryAxis, secondaryAxis);
		handleOrientation = orientation*handleOffset;
		val = yMax;
		DiscHandles.Arc(ref val, origin, scale, handleOrientation, "", yLimitColor, false, false);
		yMax = Mathf.Max(val, 0f);
		val *= -1f;
		DiscHandles.Arc(ref val, origin, scale, handleOrientation, "", yLimitColor, false, false);
		yMax = Mathf.Max(-val, 0f);
		Vector3 yHandle1 = orientation*Quaternion.AngleAxis(-yMax, secondaryAxis)*handleOffset*Vector3.forward;
		Vector3 yHandle2 = orientation*Quaternion.AngleAxis(yMax, secondaryAxis)*handleOffset*Vector3.forward;
		
		// a quaternion to describe the orientation of each handle
		Quaternion qX1 = Quaternion.LookRotation(xHandle1, tertiaryAxis);
		Quaternion qX2 = Quaternion.LookRotation(xHandle2, tertiaryAxis);
		Quaternion qY1 = Quaternion.LookRotation(yHandle1, tertiaryAxis);
		Quaternion qY2 = Quaternion.LookRotation(yHandle2, tertiaryAxis);
		
		// draw lines to shade the cone
		Vector3[] pts = new Vector3[5];
		pts[0] = qX1*Vector3.forward*scale;
		pts[1] = qY1*Vector3.forward*scale;
		pts[2] = qX2*Vector3.forward*scale;
		pts[3] = qY2*Vector3.forward*scale;
		pts[4] = qX1*Vector3.forward*scale;
		
		// use a catmull-rom spline to define the cone
		int last = pts.Length-1;
		for (int current = 0; current < last; current++)
		{	
			int previous = (current==0)?last:current-1;
			int start = current;
			int end = (current==last)?0:current + 1;
			int next = (end==last)?0:end + 1;
			
			// determine slice count based on arc length between points
			int slices = (int)(CustomHandleUtilities.GetIntegratorStep(origin, scale)*50f*Vector3.Angle(pts[start],pts[end]));
						
			// adding one guarantees yielding at least the end point
			int stepCount = slices+1;
			float oneOverStepCount = 1f/stepCount;
			Vector3 currentPt = pts[current];
			Vector3 previousPt = currentPt;
			for (int step=1; step<=stepCount; step++)
			{
				// compute current color
				Color col = Color.Lerp(xLimitColor, yLimitColor, (current==1||current==3)?1f-step*oneOverStepCount:step*oneOverStepCount);
				// lines to fill cone
				CustomHandleUtilities.SetHandleColor(col, col.a*0.25f);
				currentPt = Interpolate.CatmullRom(pts[previous], pts[start], pts[end], pts[next], step, stepCount).normalized*scale;
				Handles.DrawLine(origin, origin+Interpolate.CatmullRom(pts[previous], pts[start], pts[end], pts[next], step, stepCount).normalized*scale);
				// lines to draw outer arc
				CustomHandleUtilities.SetHandleColor(col);
				Handles.DrawLine(origin+previousPt, origin+currentPt);
				// increment
				previousPt = currentPt;
			}
		}
		
		// zMax Handles
		handleOrientation = orientation*Quaternion.LookRotation(axis, tertiaryAxis);
		Quaternion oppositeHandleOrientation = orientation*Quaternion.AngleAxis(180f, tertiaryAxis)*Quaternion.LookRotation(axis, tertiaryAxis);
		val = zMax;
		DiscHandles.Arc(ref val, origin, scale*0.5f, handleOrientation, "", zLimitColor, true, false);
		zMax = Mathf.Max(val, 0f);
		val *= -1f;
		DiscHandles.Arc(ref val, origin, scale*0.5f, handleOrientation, "", zLimitColor, true, false);
		zMax = Mathf.Max(-val, 0f);
		val = zMax;
		DiscHandles.Arc(ref val, origin, scale*0.5f, oppositeHandleOrientation, "", zLimitColor, true, false);
		zMax = Mathf.Max(val, 0f);
		val *= -1f;
		DiscHandles.Arc(ref val, origin, scale*0.5f, oppositeHandleOrientation, "", zLimitColor, true, false);
		zMax = Mathf.Max(-val, 0f);
	}
}