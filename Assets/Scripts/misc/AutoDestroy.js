#pragma strict
#pragma implicit
#pragma downcast

class AutoDestroy extends MonoBehaviour
{
	public var time : float;
	
	function Update()
	{
		time -= Time.deltaTime;
		
		if(time < 0.0)
		{
			Destroy(gameObject);
		}
	}
}