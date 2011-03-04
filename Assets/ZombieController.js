function Start()
{
	// Disable ragdoll
	for ( var rb in GetComponentsInChildren(typeof(Rigidbody)) ) { rb.isKinematic = true; }			
}

function Update () {
}

