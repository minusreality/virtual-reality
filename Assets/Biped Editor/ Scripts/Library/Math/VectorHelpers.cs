using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * VectorHelpers
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
 * This class contains a variety of static methods for working with Vectors.
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
 * A class for working with Vectors
 * */
public class VectorHelpers : System.Object
{
	// an array containing all of the cardinal axes
	public static Vector3[] cardinalAxes
	{
		get
		{
			Vector3[] ret = new Vector3[6];
			ret[0]=Vector3.right; ret[1]=Vector3.left;
			ret[2]=Vector3.up; ret[3]=Vector3.down;
			ret[4]=Vector3.forward; ret[5]=Vector3.back;
			return ret;
		}
	}
	
	/*
	 * Find the cardinal axis that is nearest to testVector
	 * */
	public static Vector3 FindNearestCardinalAxis(Vector3 testVector)
	{
		testVector.Normalize();
		Vector3 nearest = Vector3.forward;
		Vector3[] cardinals = cardinalAxes;
		for (int i=0; i<cardinals.Length; i++)
			if (Vector3.Dot(testVector,cardinals[i])>Vector3.Dot(testVector,nearest))
				nearest = cardinals[i];
		return nearest;
	}
	
	/*
	 * Mirror point p across the plane defined by the normal n
	 * */
	public static Vector3 MirrorPointAcrossPlane(Vector3 p, Vector3 n)
	{
		n.Normalize();
		return p-2f*(Vector3.Dot(p, n)*n);
	}
	
	/*
	 * Return the smallest element in a vector
	 * */
	public static float MinValue(Vector2 v)
	{
		return Mathf.Min(v.x, v.y);
	}
	public static float MinValue(Vector3 v)
	{
		return Mathf.Min(Mathf.Min(v.x, v.y), Mathf.Min(v.y, v.z));
	}
	public static float MinValue(Vector4 v)
	{
		return Mathf.Min(Mathf.Min(v.x, v.y), Mathf.Min(v.z, v.w));
	}
	
	/*
	 * Return the largest element in a vector
	 * */
	public static float MaxValue(Vector2 v)
	{
		return Mathf.Max(v.x, v.y);
	}
	public static float MaxValue(Vector3 v)
	{
		return Mathf.Max(Mathf.Max(v.x, v.y), Mathf.Max(v.y, v.z));
	}
	public static float MaxValue(Vector4 v)
	{
		return Mathf.Max(Mathf.Max(v.x, v.y), Mathf.Max(v.z, v.w));
	}
	
	/*
	 * Scale a vector using another scale vector
	 * */
	public static Vector3 ScaleByVector(Vector3 v, Vector3 scale)
	{
		return new Vector3(v.x*scale.x, v.y*scale.y, v.z*scale.z);
	}
	
	/*
	 * Create an axis tripod from a supplied up-vector
	 * */
	public static AxisTripod AxisTripodFromUp(Vector3 up)
	{
		AxisTripod tripod = new AxisTripod();
		tripod.up = up.normalized;
		tripod.right = Vector3.right;
		float dot = Vector3.Dot(tripod.up, tripod.right);
		if (dot==1f) tripod.right = Vector3.forward;
		else if (dot==-1f) tripod.right = Vector3.back;
		tripod.forward = Vector3.Cross(tripod.right, tripod.up).normalized;
		tripod.right = Vector3.Cross(tripod.up, tripod.forward);
		return tripod;
	}
	
	/*
	 * Create an axis tripod from a supplied forward- and up-axis
	 * */
	public static AxisTripod AxisTripodFromForwardUp(Vector3 forward, Vector3 up)
	{
		AxisTripod tripod = new AxisTripod();
		Quaternion q = Quaternion.LookRotation(forward, up);
		tripod.forward = q*Vector3.forward;
		tripod.up = q*Vector3.up;
		tripod.right = q*Vector3.right;
		return tripod;
	}
	
	public static AxisTripod AxisTripodFromQuaternion(Quaternion quat)
	{
		return AxisTripodFromForwardUp(quat*Vector3.forward, quat*Vector3.up);
	}
}

/*
 * A struct to describe a left-handed axis tripod of three orthonormalized vectors
 * */
public struct AxisTripod
{
	public Vector3 right;
	public Vector3 left
	{
		get { return -right; }
		set { right = -value; }
	}
	public Vector3 up;
	public Vector3 down
	{
		get { return -up; }
		set { up = -value; }
	}
	public Vector3 forward;
	public Vector3 back
	{
		get { return -forward; }
		set { forward = -value; }
	}
}