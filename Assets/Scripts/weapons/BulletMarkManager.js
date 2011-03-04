#pragma strict
#pragma implicit
#pragma downcast

class BulletMarkManager extends MonoBehaviour
{
	static private var instance : BulletMarkManager;

	public var maxMarks : int;
	public var marks : Array;
	public var pushDistances : Array;
	
	function Start()
	{
		if(instance == null)
		{
			instance = this;
		}
	}
	
	static function Add(go : GameObject) : float
	{
		//if(instance == null)
		//{
			var aux : GameObject = new GameObject("BulletMarkManager");
			instance = aux.AddComponent("BulletMarkManager") as BulletMarkManager;
			instance.marks = new Array();
			instance.pushDistances = new Array();
			instance.maxMarks = 2000;
		//}
		
		var auxGO : GameObject;
		var auxT : Transform;
		var currentT : Transform = go.transform;
		var currentPos = currentT.position;
		var radius : float = (currentT.localScale.x * currentT.localScale.x * 0.25) + (currentT.localScale.y * currentT.localScale.y * 0.25) + (currentT.localScale.z * currentT.localScale.z * 0.25);
		radius = Mathf.Sqrt(radius);
		var realRadius : float = radius * 2;
		radius *= 0.2;
		
		var distance : float;
		
		if(instance.marks.length == instance.maxMarks)
		{
			auxGO = instance.marks[0] as GameObject;
			Destroy(auxGO);
			instance.marks.RemoveAt(0);
			instance.pushDistances.RemoveAt(0);
		}
		
		var pushDistance : float = 0.0001;
		var length : int = instance.marks.length;
		var sideMarks : int = 0;
		for(var i : int = 0; i < length; i++)
		{
			auxGO = instance.marks[i] as GameObject;
			
			if(auxGO != null)
			{
				auxT = auxGO.transform;
				distance = (auxT.position - currentPos).magnitude;
				if(distance < radius)
				{
					Destroy(auxGO);
					instance.marks.RemoveAt(i);
					instance.pushDistances.RemoveAt(i);
					i--;
					length--;
				}
				else if(distance < realRadius)
				{
					var cDist : float = instance.pushDistances[i];
					
					pushDistance = Mathf.Max(pushDistance, cDist);
				}
			}
			else
			{
				instance.marks.RemoveAt(i);
				instance.pushDistances.RemoveAt(i);
				i--;
				length--;
			}
		}
		pushDistance += 0.0005;
		
		instance.marks.Add(go);
		instance.pushDistances.Add(pushDistance);
		
		return pushDistance;
	}
	
	static function ClearMarks()
	{
		var go : GameObject;
		
		if(instance.marks.length > 0)
		{
			for(var i : int = 0; i < instance.marks.length; i++)
			{
				go = instance.marks[i] as GameObject;
				
				Destroy(go);
			}	
			
			instance.marks.Clear();
		}
	}
}