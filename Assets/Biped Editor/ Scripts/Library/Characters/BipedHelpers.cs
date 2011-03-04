using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * BipedHelpers
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
 * This class contains a variety of static methods for working with Bipeds.
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
 * Various helper functions for working with a Biped
 * */
public class BipedHelpers : System.Object
{
	/*
	 * Try to find a character naming prefix for a Biped specified as using 3DS Max Biped naming conventions
	 * */
	public static string FindCharacterNamePrefixOnMaxBiped(Biped biped)
	{
		Component[] hierarchy = biped.GetComponentsInChildren<Transform>();
		foreach (Component c in hierarchy)
			if (c.name.EndsWith("Pelvis"))
				return c.name.Substring(0, c.name.LastIndexOf("Pelvis"));
		return "";
	}
	
	/*
	 * Add a BodyPart to a bone with the specified name in the provided hierarchy
	 * */
	public static BodyPart AddPartByName(string boneName, Component[] hierarchy)
	{
		Transform bone = TransformHelpers.GetTransformInHierarchy(hierarchy, boneName);
		if (bone==null) return null;
		BodyPart p = bone.GetComponent<BodyPart>();
		if (p!=null) GameObject.DestroyImmediate(p);
		return bone.gameObject.AddComponent<BodyPart>();
	}
	
	/*
	 * Find the tip for a limb using the supplied info
	 * */
	public static BodyPart FindLimbTip(BodyPart tipParent, Component[] hierarchy, string tipName)
	{
		// if the tip parent is null, early out
		if (tipParent==null) return null;
		
		// the part that will be returned
		BodyPart ret = null;
		
		// if there is more than one child, find the middle digit using the naming convention
		if (tipParent.bone.childCount>1)
		{
			// search for children with the matching name
			ArrayList children = new ArrayList();
			int matches = 0;
			foreach (Transform child in tipParent.bone)
			{
				children.Add(child.name);
				if (child.name.Contains(tipName)) matches++;				
			}
			if (matches==1)
			{
				foreach (Transform child in tipParent.bone)
				{
					if (child.name.Length<tipName.Length || ret!=null) continue;
					if (child.name.Substring(0, tipName.Length) == tipName)
						ret = BipedHelpers.AddPartByName(child.name, hierarchy);
				}
			}
			
			// if no match was found, this may be a Max Biped, in which case the digits are numbered
			if (ret==null)
			{
				Transform tip = TransformHelpers.GetTransformInHierarchy(hierarchy, tipName+(tipParent.bone.childCount/2).ToString());
				if (tip!=null) ret = BipedHelpers.AddPartByName(tip.name, hierarchy);
			}
		}
		
		// couldn't find the middle digit, so pick the first child
		if (ret==null && tipParent.bone.childCount>0)
		{
			ret = BipedHelpers.AddPartByName(tipParent.bone.GetChild(0).name, hierarchy);
		}
		
		return ret;
	}
}