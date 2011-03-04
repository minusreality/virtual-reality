/*
Copyright (c) 2010, Raphael Lopes Baldi & Aquiris Game Experience LTDA.

See the document "TERMS OF USE" included in the project folder for licencing details.
*/

using UnityEngine;

public class DecalPolygon
{
	public int verticeCount;
	public Vector3[] normal;
	public Vector3[] vertice;
	public Vector4[] tangent;
	
	public DecalPolygon()
	{
		verticeCount = 0;
		vertice = new Vector3[9];
		normal = new Vector3[9];
		tangent = new Vector4[9];
	}
	
	static public DecalPolygon ClipPolygonAgainstPlane (DecalPolygon polygon, Vector4 plane)
	{
		bool[] neg = new bool[10];
		int negCount = 0;
		
		Vector3 n = new Vector3(plane.x, plane.y, plane.z);

		for(int i = 0; i < polygon.verticeCount; i++)
		{
			neg[i] = (Vector3.Dot(polygon.vertice[i], n) + plane.w) < 0.0f;
			if(neg[i]) negCount++;
		}
		
		if(negCount == polygon.verticeCount) return null;
		if(negCount == 0) return polygon;

		DecalPolygon tempPolygon = new DecalPolygon();
		tempPolygon.verticeCount = 0;
		Vector3 v1, v2, dir;
		float t;
		
		for(int i = 0; i < polygon.verticeCount; i++)
		{
			int b = (i == 0) ? polygon.verticeCount - 1 : i -1;
			
			if(neg[i])
			{
				if(!neg[b])
				{
					v1 = polygon.vertice[i];
					v2 = polygon.vertice[b];
					dir = (v2 - v1).normalized;
				
					t = -(Vector3.Dot(n, v1) + plane.w) / Vector3.Dot(n, dir);
					
					tempPolygon.tangent[tempPolygon.verticeCount] = polygon.tangent[i] + ((polygon.tangent[b] - polygon.tangent[i]).normalized * t);
					tempPolygon.vertice[tempPolygon.verticeCount] = v1 + ((v2 - v1).normalized * t);
					tempPolygon.normal[tempPolygon.verticeCount] = polygon.normal[i] + ((polygon.normal[b] - polygon.normal[i]).normalized * t);

					tempPolygon.verticeCount++;
				}
			}
			else
			{
				if(neg[b])
				{
					v1 = polygon.vertice[b];
					v2 = polygon.vertice[i];
					dir = (v2 - v1).normalized;
					
					t = -(Vector3.Dot(n, v1) + plane.w) / Vector3.Dot(n, dir);
					
					tempPolygon.tangent[tempPolygon.verticeCount] = polygon.tangent[b] + ((polygon.tangent[i] - polygon.tangent[b]).normalized * t);
					tempPolygon.vertice[tempPolygon.verticeCount] = v1 + ((v2 - v1).normalized * t);
					tempPolygon.normal[tempPolygon.verticeCount] = polygon.normal[b] + ((polygon.normal[i] - polygon.normal[b]).normalized * t);
					
					tempPolygon.verticeCount++;
				}
				
				tempPolygon.tangent[tempPolygon.verticeCount] = polygon.tangent[i];
				tempPolygon.vertice[tempPolygon.verticeCount] = polygon.vertice[i];
				tempPolygon.normal[tempPolygon.verticeCount] = polygon.normal[i];
				
				tempPolygon.verticeCount++;
			}
		}
		
		return tempPolygon;
	}
}