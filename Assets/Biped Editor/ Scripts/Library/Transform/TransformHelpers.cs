using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * TransformHelpers
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
 * This class contains a variety of static methods for working with Transforms.
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
 * A class for working with Transforms
 * */
public class TransformHelpers : System.Object
{
	/*
	 * Locate a Transform by name in a hierarchy starting from root
	 * */
	public static Transform GetTransformInHierarchy(Transform root, string name)
	{
		return GetTransformInHierarchy(root.GetComponentsInChildren<Transform>(), name);
	}
	
	/*
	 * Locate a Transform by name in a hierarchy defined by an array of components
	 * */
	public static Transform GetTransformInHierarchy(Component[] hierarchy, string name)
	{
		foreach (Transform transform in hierarchy)
		{
			if (transform.name == name) return transform;
			// double check to ensure there is not a namespace prefix applied
			else if (transform.name.Contains(":") && 
				transform.name.Substring(transform.name.LastIndexOf(":")+1) == name)
				return transform;
		}
		return null;
	}
}