using UnityEngine;
using System.Collections;

/*
 * ----------------------------------------------------------------------------
 * Creation Info
 * ----------------------------------------------------------------------------
 * BipedNamingConvention
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
 * This class contains the required parts for specifying the naming convention
 * of a Biped, including appropriate prefixes and bone names.
 * 
 * ----------------------------------------------------------------------------
 * Usage
 * ----------------------------------------------------------------------------
 * Call the functions you need, bro.
 * 
 * ----------------------------------------------------------------------------
 * Notes and Limitations
 * ----------------------------------------------------------------------------
 * The naming convention currently assumes a prefix rather than suffix for
 * defining the body side. Not only is this standard practice in most
 * commercial DCC applications, but it also makes more sense (you more often
 * need to select parts on the basis of side rather than on the basis of part).
 * */

// an enum for the a character skeleton's naming convention
public enum DCCApplication { Max, Maya, HumanIK, Custom };

/*
 * A class to contain a definition for a Biped naming convention
 * */
[System.Serializable]
public class BipedNamingConvention : System.Object
{
	public string characterNamePrefix = "";
	public string centerPrefix = "";
	public string leftPrefix = "Left";
	public string rightPrefix = "Right";
	public string head = "Head";
	public string neck = "Neck";
	public string spine = "Spine";
	public string pelvis = "Hips";
	public string upperLeg = "UpLeg";
	public string lowerLeg = "Leg";
	public string foot = "Foot";
	public string toe = "FootMiddle"; // toe, not toe base
	public string collar = "Shoulder";
	public string upperArm = "Arm";
	public string forearm = "ForeArm";
	public string hand = "Hand";
	public string handTip = "HandMiddle"; // finger, not finger base
	
	/*
	 * Return a new BipedNamingConvention using Max Biped names
	 * */
	public static BipedNamingConvention Max(string characterNamePrefix)
	{
		BipedNamingConvention names = new BipedNamingConvention();
		names.characterNamePrefix = characterNamePrefix;
		names.centerPrefix = "";
		names.leftPrefix = "L ";
		names.rightPrefix = "R ";
		names.head = "Head";
		names.neck = "Neck";
		names.spine = "Spine";
		names.pelvis = "Pelvis";
		names.upperLeg = "Thigh";
		names.lowerLeg = "Calf";
		names.foot = "Foot";
		names.toe = "Toe"; // toe, not toe base
		names.collar = "Clavicle";
		names.upperArm = "UpperArm";
		names.forearm = "Forearm";
		names.hand = "Hand";
		names.handTip = "Finger"; // finger, not finger base
		return names;
	}
	
	/*
	 * Return a new BipedNamingConvention using Maya names
	 * */
	public static BipedNamingConvention Maya()
	{
		BipedNamingConvention names = new BipedNamingConvention();
		names.characterNamePrefix = "";
		names.centerPrefix = "Center";
		names.leftPrefix = "Left";
		names.rightPrefix = "Right";
		names.head = "Head";
		names.neck = "Neck";
		names.spine = "Spine";
		names.pelvis = "Root";
		names.upperLeg = "Hip";
		names.lowerLeg = "Knee";
		names.foot = "Foot";
		names.toe = "Toe"; // toe, not toe base
		names.collar = "Collar";
		names.upperArm = "Shoulder";
		names.forearm = "Elbow";
		names.hand = "Hand";
		names.handTip = "MiddleFinger"; // finger, not finger base
		return names;
	}
	
	/*
	 * Return a new biped naming convention using HumanIK names
	 * */
	public static BipedNamingConvention HumanIK()
	{
		BipedNamingConvention names = new BipedNamingConvention();
		names.characterNamePrefix = "";
		names.centerPrefix = "";
		names.leftPrefix = "Left";
		names.rightPrefix = "Right";
		names.head = "Head";
		names.neck = "Neck";
		names.spine = "Spine";
		names.pelvis = "Hips";
		names.upperLeg = "UpLeg";
		names.lowerLeg = "Leg";
		names.foot = "Foot";
		names.toe = "FootMiddle"; // toe, not toe base
		names.collar = "Shoulder";
		names.upperArm = "Arm";
		names.forearm = "ForeArm";
		names.hand = "Hand";
		names.handTip = "HandMiddle"; // finger, not finger base
		return names;
	}
}