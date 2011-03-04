/*
Copyright (c) 2010, Raphael Lopes Baldi & Aquiris Game Experience LTDA.

See the document "TERMS OF USE" included in the project folder for licencing details.
*/

using UnityEngine;
using System.Collections.Generic;

public class Decal : MonoBehaviour 
{
	static public int dCount;
	
	//The gameObjects that will be affected by the decal.
	[HideInInspector]
	public GameObject[] affectedObjects;
	
	public float maxAngle = 90.0f;
	
	private float angleCosine;
	
	//Decal bounds. We'll never show it on inspector as user should
	//not modify this value.
	[HideInInspector]
	public Bounds bounds;
	
	#region STATE PROPERTIES
	[HideInInspector]
	public float previousUVAngle;
	
	[HideInInspector]
	public Vector3 previousPosition;
	
	[HideInInspector]
	public Quaternion previousRotation;
	
	[HideInInspector]
	public Vector3 previousScale;
	
	[HideInInspector]
	public float previousMaxAngle;
	
	[HideInInspector]
	public float previousPushDistance = 0.009f;
	
	[HideInInspector]
	public Vector2 previousTiling;
	
	[HideInInspector]
	public Vector2 previousOffset;
	
	[HideInInspector]
	public bool useMeshCollider;
	#endregion
	
	#region UV PROPERTIES
	//[HideInInspector]
	public Vector2 tiling = Vector2.one;
	
	//[HideInInspector]
	public Vector2 offset = Vector2.zero;
	
	[HideInInspector]
	public float uvAngle = 0.0f;
	private float uCos;
	private float vSin;
	#endregion
	
	public Material decalMaterial;
	
	[HideInInspector]
	public DecalMode decalMode;
	
	#region DECAL CREATION PROPERTIES
	private List<DecalPolygon> startPolygons;
	private List<DecalPolygon> clippedPolygons;
	
	[HideInInspector]
	public Vector4 bottomPlane;
	
	[HideInInspector]
	public Vector4 topPlane;
	
	[HideInInspector]
	public Vector4 leftPlane;
	
	[HideInInspector]
	public Vector4 rightPlane;
	
	[HideInInspector]
	public Vector4 frontPlane;
	
	[HideInInspector]
	public Vector4 backPlane;

	[HideInInspector]
	public Vector3 decalNormal;// = transf.MultiplyVector(-transform.forward);
	
	[HideInInspector]
	public Vector3 decalCenter;// = transf.MultiplyPoint(transform.position);
	
	[HideInInspector]
	public Vector3 decalTangent;// = transf.MultiplyVector(transform.right);
	
	[HideInInspector]
	public Vector3 decalBinormal;// = transf.MultiplyVector(transform.up);
	
	[HideInInspector]
	public Vector3 decalSize;// = transf.MultiplyPoint(transform.lossyScale);
	#endregion
	
	public float pushDistance = 0.009f;
	
	private List<MeshCombineUtility.MeshInstance> instancesList;
	
	//Should the system stop checking for objects that will be affected?
	public bool checkAutomatically;
	public LayerMask affectedLayers;
	public bool affectOtherDecals;
	public bool affectInactiveRenderers;
	
	#region INSPECTOR PROPERTIES
	[HideInInspector]
	public bool showAffectedObjectsOptions;
	
	[HideInInspector]
	public bool showObjects;
	#endregion
	
//	private void OnDrawGizmosSelected()
//	{
//		Gizmos.DrawIcon(transform.position, "decal_icon");
//	}
	
	private void OnDrawGizmosSelected()
	{
		//Calculate current decal bounds.
		CalculateBounds();
		
		//Gizmos.DrawRay(decalCenter, decalNormal);
		
		//Draw the helper gizmos with the correct object matrix.
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}
	
	//Method responsible for calculating decal's bounds, used to detect the 
	//affected objects.
 	public void CalculateBounds()
	{
		//Extend decal's bounds a little in order to make sure that everything will
		//still working when the projector is rotated.
		bounds = new Bounds(transform.position, transform.lossyScale * 1.414214f);
	}
	
	//Method responsible for constructing the Decal Mesh, based on the affected objects.
	public void CalculateDecal()
	{
		ClearDecals();
		
		maxAngle = Mathf.Clamp(maxAngle, 0.0f, 180.0f);
		angleCosine = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
		
		uvAngle = Mathf.Clamp(uvAngle, 0.0f, 360.0f);
		uCos = Mathf.Cos(uvAngle * Mathf.Deg2Rad);
		vSin = Mathf.Sin(uvAngle * Mathf.Deg2Rad);
		
		if(affectedObjects == null)
		{
			//Debug.LogWarning("No object will be affected. Decal will not be calculated.");
			return;
		}
		else if(affectedObjects.Length <= 0)
		{
			//Debug.LogWarning("No object will be affected. Decal will not be calculated.");
			return;
		}
		
		//Current transform matrix
		Matrix4x4 myTransform = transform.worldToLocalMatrix;
		
		instancesList = new List<MeshCombineUtility.MeshInstance>();
		
		for(int i = 0; i < affectedObjects.Length; i++)
		{
			if(affectedObjects[i] == null) continue;
			
			CalculateObjectDecal(affectedObjects[i], myTransform);
		}
		
		if(instancesList.Count > 0)
		{
			MeshCombineUtility.MeshInstance[] instances = new MeshCombineUtility.MeshInstance[instancesList.Count];
			for(int i = 0; i < instances.Length; i++)
			{
				instances[i] = instancesList[i];
			}
			
			MeshRenderer r = gameObject.GetComponent<MeshRenderer>();
			if(r == null)
			{
				r = gameObject.AddComponent<MeshRenderer>();
			}
			
			r.material = decalMaterial;
			
			MeshFilter fi = gameObject.GetComponent<MeshFilter>();
			
			if(fi == null)
			{
				fi = gameObject.AddComponent<MeshFilter>();
			}
			else
			{
				DestroyImmediate(fi.sharedMesh);
			}
			
			Mesh finalMesh = MeshCombineUtility.Combine(instances, true);
			
			if(pushDistance > 0.0f)
			{
				List<List<int>> relations = new List<List<int>>();
				Vector3[] vert = finalMesh.vertices;
				Vector3[] normals = finalMesh.normals;
				
				bool[] usedIndex = new bool[vert.Length];
				for(int i = 0; i < usedIndex.Length; i++)
				{
					usedIndex[i] = false;
				}
				
				for(int i = 0; i < vert.Length; i++)
				{
					if(usedIndex[i]) continue;
					
					List<int> c = new List<int>();
					c.Add(i);
					
					usedIndex[i] = true;
					
					for(int j = i + 1; j < vert.Length; j++)
					{
						if(usedIndex[j]) continue;
						
						if(Vector3.Distance(vert[i], vert[j]) < 0.001f)
						{
							c.Add(j);
							
							usedIndex[j] = true;
						}
					}		
					
					relations.Add(c);
				}
				
				foreach(List<int> l in relations)
				{
					Vector3 nNormal = Vector3.zero;
					foreach(int i in l)
					{
						nNormal += normals[i];
					}
					
					nNormal = (nNormal / l.Count).normalized;
					
					foreach(int i in l)
					{
						vert[i] += nNormal * (pushDistance);
					}
				}
				
				finalMesh.vertices = vert;
			}
			
			finalMesh.name = "DecalMesh";
			
			fi.mesh = finalMesh;

			for(int i = 0; i < instancesList.Count; i++)
			{
				DestroyImmediate(instancesList[i].mesh);
			}
		}
		
		instancesList.Clear();
		instancesList = null;
	}
	
	public void ClearDecals()
	{
		MeshFilter auxFilter = gameObject.GetComponent<MeshFilter>();
		
		if(auxFilter != null)
		{
			DestroyImmediate(auxFilter.sharedMesh);
			DestroyImmediate(auxFilter);
		}
		
		MeshRenderer auxRenderer = gameObject.GetComponent<MeshRenderer>();
		
		if(auxRenderer != null)
		{
			DestroyImmediate(auxRenderer);
		}
	}

	public void CalculateObjectDecal(GameObject obj, Matrix4x4 cTransform)
	{
		Mesh m = null;
		
		if(decalMode == DecalMode.MESH_COLLIDER)
		{
			if(obj.GetComponent<MeshCollider>() != null)
			{	
				m = obj.GetComponent<MeshCollider>().sharedMesh;
			}
			else
			{
				m = null;
			}
		}
		
		if(m == null || decalMode == DecalMode.MESH_FILTER)
		{
			if(obj.GetComponent<MeshFilter>() == null) return;
			
			m = obj.GetComponent<MeshFilter>().sharedMesh;
		}
		
		if(m == null || m.name.ToLower().Contains("combined"))
		{
			return;
		}

		decalNormal = obj.transform.InverseTransformDirection(transform.forward);
		decalCenter = obj.transform.InverseTransformPoint(transform.position);
		decalTangent = obj.transform.InverseTransformDirection(transform.right);
		decalBinormal = obj.transform.InverseTransformDirection(transform.up);
		decalSize = new Vector3(transform.lossyScale.x / obj.transform.lossyScale.x, transform.lossyScale.y / obj.transform.lossyScale.y, transform.lossyScale.z / obj.transform.lossyScale.z);//transf.MultiplyPoint(transform.lossyScale);
		
		bottomPlane = new Vector4(-decalBinormal.x, -decalBinormal.y, -decalBinormal.z, (decalSize.y * 0.5f) + Vector3.Dot(decalCenter, decalBinormal));
		topPlane = new Vector4(decalBinormal.x, decalBinormal.y, decalBinormal.z, (decalSize.y * 0.5f) - Vector3.Dot(decalCenter, decalBinormal));
		rightPlane = new Vector4(-decalTangent.x, -decalTangent.y, -decalTangent.z, (decalSize.x * 0.5f) + Vector3.Dot(decalCenter, decalTangent));
		leftPlane = new Vector4(decalTangent.x, decalTangent.y, decalTangent.z, (decalSize.x * 0.5f) - Vector3.Dot(decalCenter, decalTangent));
		frontPlane = new Vector4(decalNormal.x, decalNormal.y, decalNormal.z, (decalSize.z * 0.5f) - Vector3.Dot(decalCenter, decalNormal));
		backPlane = new Vector4(-decalNormal.x, -decalNormal.y, -decalNormal.z, (decalSize.z * 0.5f) + Vector3.Dot(decalCenter, decalNormal));
		
		int[] triangles = m.triangles;
		Vector3[] vertices = m.vertices;
		Vector3[] normals = m.normals;
		Vector4[] tangents = m.tangents;
		
		float dot;
		int i1, i2, i3;
		Vector3 v1, v2, v3;
		Vector3 n1;
		Vector3 t1, t2, t3;
		
		DecalPolygon t;
		startPolygons = new List<DecalPolygon>();
		
		for(int i = 0; i < triangles.Length; i += 3)
		{
			i1 = triangles[i];
			i2 = triangles[i+1];
			i3 = triangles[i+2];
			
			v1 = vertices[i1];
			v2 = vertices[i2];
			v3 = vertices[i3];

			n1 = normals[i1];
			
			dot = Vector3.Dot(n1, -decalNormal);
			
			if(dot <= angleCosine) continue;

			t1 = tangents[i1];
			t2 = tangents[i2];
			t3 = tangents[i3];
			
			t = new DecalPolygon();
			t.verticeCount = 3;

			t.vertice[0] = v1;
			t.vertice[1] = v2;
			t.vertice[2] = v3;
			
			t.normal[0] = n1;
			t.normal[1] = n1;
			t.normal[2] = n1;
			
			t.tangent[0] = t1;
			t.tangent[1] = t2;
			t.tangent[2] = t3;
			
			startPolygons.Add(t);
		}
		
		Mesh aux = CreateMesh(ClipMesh(), obj.transform);
		
		if(aux != null)
		{
			MeshCombineUtility.MeshInstance instance = new MeshCombineUtility.MeshInstance();
			instance.mesh = aux;
			instance.subMeshIndex = 0;
			instance.transform = transform.worldToLocalMatrix * obj.transform.localToWorldMatrix;
			instancesList.Add(instance);	
		}
		
		aux = null;
		
		startPolygons.Clear();
		startPolygons = null;
		clippedPolygons.Clear();
		clippedPolygons = null;
		
		triangles = null;
		normals = null;
		vertices = null;
		tangents = null;
		
		System.GC.Collect();
	}
	
	private Mesh CreateMesh(int totalVertices, Transform to)
	{
		if(clippedPolygons == null) return null;
		
		if(clippedPolygons.Count <= 0) return null;
		
		if(totalVertices < 3) return null;
		
		int[] newTris = new int[(totalVertices - 2) * 3];

		Vector3[] newVertices = new Vector3[totalVertices];
		Vector3[] newNormals = new Vector3[totalVertices];
		Vector2[] newUv = new Vector2[totalVertices];
		Vector4[] newTangents = new Vector4[totalVertices];
		
		int count = 0;
		int trisCount = 0;
		int oCount = 0;
		
		float u, v, tempU, tempV;
		Vector3 dir;
		
		float one_over_w = 1.0f / decalSize.x;
		float one_over_h = 1.0f / decalSize.y;
		
		foreach(DecalPolygon p in clippedPolygons)
		{	
			for(int i = 0; i < p.verticeCount; i++)
			{
				newVertices[count] = p.vertice[i];
				newNormals[count] = p.normal[i];

				newTangents[count] = p.tangent[i];

				if(i < p.verticeCount - 2)
				{
					newTris[trisCount] = oCount;
					newTris[trisCount+1] = count+1;
					newTris[trisCount+2] = count+2;
					trisCount += 3;
				}
				
				count++;
			}
			oCount = count;
		}
		
		for(int i = 0; i < newVertices.Length; i++)
		{
			dir = newVertices[i] - decalCenter;
				
			tempU = (Vector3.Dot(dir, decalTangent) * one_over_w);// + 0.5f);// * tiling.x;// + offset.x;
			tempV = (Vector3.Dot(dir, decalBinormal) * one_over_h);// + 0.5f);// * tiling.y;// + offset.y;		
			
			u = tempU * uCos - tempV * vSin + 0.5f;
			v = tempU * vSin + tempV * uCos + 0.5f;
			
			u *= tiling.x;
			v *= tiling.y;
			
			u += offset.x;
			v += offset.y;

			newUv[i] = new Vector2(u, v);// * vSin);
		}
		
		Mesh m = new Mesh();
		m.vertices = newVertices;
		m.normals = newNormals;
		m.triangles = newTris;
		m.uv = newUv;
		m.uv1 = newUv;
		m.uv2 = newUv;	

		m.tangents = newTangents;
		
		return m;
	}

	private int ClipMesh()
	{
		DecalPolygon tempFace, face;
		
		int totalVertices = 0;
		
		if(clippedPolygons == null) clippedPolygons = new List<DecalPolygon>();
		else clippedPolygons.Clear();
		
		for(int i = 0; i < startPolygons.Count; i++)
		{
			face = startPolygons[i];

			tempFace = DecalPolygon.ClipPolygonAgainstPlane(face, frontPlane);
			if(tempFace != null)
			{
				tempFace = DecalPolygon.ClipPolygonAgainstPlane(tempFace, backPlane);
				if(tempFace != null)
				{
					tempFace = DecalPolygon.ClipPolygonAgainstPlane(tempFace, rightPlane);
					if(tempFace != null)
					{
						tempFace = DecalPolygon.ClipPolygonAgainstPlane(tempFace, leftPlane);
						if(tempFace != null)
						{
							tempFace = DecalPolygon.ClipPolygonAgainstPlane(tempFace, bottomPlane);
							if(tempFace != null)
							{
								tempFace = DecalPolygon.ClipPolygonAgainstPlane(tempFace, topPlane);
								if(tempFace != null)
								{
									totalVertices += tempFace.verticeCount;
									clippedPolygons.Add(tempFace);
								}
							}
						}
					}
				}
			}
		}
		
		return totalVertices;
	}
	
	//Has the Decal changed since the last call to this method?
	public bool HasChanged()
	{
		Transform t = transform;
		bool changed = false;
		maxAngle = Mathf.Clamp(maxAngle, 0.0f, 180.0f);
		uvAngle = Mathf.Clamp(uvAngle, 0.0f, 360.0f);
		
		if(previousPosition != t.position)
		{
			changed = true;
		}
		else if(previousScale != t.lossyScale)
		{
			changed = true;
		}
		else if(previousRotation != t.rotation)
		{
			changed = true;
		}
		else if(previousPushDistance != pushDistance)
		{
			changed = true;
		}
		else if(previousTiling != tiling)
		{
			changed = true;
		}
		else if(previousOffset != offset)
		{
			changed = true;
		}
		else if(previousMaxAngle != maxAngle)
		{
			changed = true;
		}
		else if(previousUVAngle != uvAngle)
		{
			changed = true;
		}
		
		previousUVAngle = uvAngle;
		previousMaxAngle = maxAngle;
		previousTiling = tiling;
		previousOffset = offset;
		previousPushDistance = pushDistance;
		previousPosition = t.position;
		previousRotation = t.rotation;
		previousScale = t.lossyScale;
		
		return changed;
	}
}