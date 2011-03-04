#pragma strict
#pragma implicit
#pragma downcast

enum HitType
{
	CONCRETE,
	WOOD,
	METAL,
	OLD_METAL,
	GLASS,
	GENERIC
}

class BulletMarks extends MonoBehaviour
{
	public var concrete : Texture2D[];
	public var wood : Texture2D[];
	public var metal : Texture2D[];
	public var oldMetal : Texture2D[];
	public var glass : Texture2D[];
	public var generic : Texture2D[];
	
	public function GenerateDecal(type : HitType, go : GameObject) 
	{
		var useTexture : Texture2D;
		var random : int;
		
		switch(type)
		{
			case HitType.CONCRETE:
				if(concrete == null) return;
				if(concrete.Length == 0) return;
				
				random = Random.Range(0, concrete.Length);
				
				useTexture = concrete[random];
				break;
			case HitType.WOOD:
				if(wood == null) return;
				if(wood.Length == 0) return;
				
				random = Random.Range(0, wood.Length);
				
				useTexture = wood[random];
				break;
			case HitType.METAL:
				if(metal == null) return;
				if(metal.Length == 0) return;
				
				random = Random.Range(0, metal.Length);
				
				useTexture = metal[random];
				break;
			case HitType.OLD_METAL:
				if(oldMetal == null) return;
				if(oldMetal.Length == 0) return;
				
				random = Random.Range(0, oldMetal.Length);
				
				useTexture = oldMetal[random];
				break;
			case HitType.GLASS:
				if(glass == null) return;
				if(glass.Length == 0) return;
				
				random = Random.Range(0, glass.Length);
				
				useTexture = glass[random];
				break;
			case HitType.GENERIC:
				if(generic == null) return;
				if(generic.Length == 0) return;
				
				random = Random.Range(0, generic.Length);
				
				useTexture = generic[random];
				break;
			default:
				if(wood == null) return;
				if(wood.Length == 0) return;
				
				random = Random.Range(0, wood.Length);
				
				useTexture = wood[random];
				return;
		}
		
		transform.Rotate(new Vector3(0, 0, Random.Range(-180.0, 180.0)));

		Decal.dCount++;
		var d : Decal = gameObject.GetComponent("Decal");
		d.affectedObjects = new GameObject[1];
		d.affectedObjects[0] = go;
		d.decalMode = DecalMode.MESH_COLLIDER;
		d.pushDistance = 0.009 + BulletMarkManager.Add(gameObject);
		var m : Material = new Material(d.decalMaterial);
		m.mainTexture = useTexture;
		d.decalMaterial = m;
		d.CalculateDecal();
		d.transform.parent = go.transform;
	}
}