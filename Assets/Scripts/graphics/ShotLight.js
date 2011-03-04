#pragma strict
#pragma implicit
#pragma downcast

class ShotLight extends MonoBehaviour
{
	public var time : float = 0.02;
	private var timer : float;
	
	function OnEnable()
	{
		if(light == null)
		{
			Destroy(this);
		}
		else
		{
			timer = time;
			light.enabled = false;
		}
	}
	
	function OnDisable()
	{
		if(light == null)
		{
			Destroy(this);
		}
		else
		{
			timer = time;
			light.enabled = false;
		}
	}
	
	function LateUpdate()
	{
		timer -= Time.deltaTime;
		
		if(timer <= 0.0)
		{
			timer = time;
			light.enabled = !light.enabled;
		}
	}
}