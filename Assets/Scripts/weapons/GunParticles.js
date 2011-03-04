#pragma strict
#pragma implicit
#pragma downcast

class GunParticles extends MonoBehaviour
{
	private var cState : boolean;
	private var emitters : Array;
	
	function Start()
	{
		cState = true;
		
		emitters = GetComponentsInChildren(ParticleEmitter);
		
		ChangeState(false);
	}
	
	public function ChangeState(p_newState : boolean)
	{
		if(cState == p_newState) return;
		
		cState = p_newState;
		
		if(emitters != null)
		{
			for(var i : int = 0; i < emitters.length; i++)
			{
				(emitters[i] as ParticleEmitter).emit = p_newState;
			}
		}
	}
}