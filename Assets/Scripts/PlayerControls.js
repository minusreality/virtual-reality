var cam : GameObject;
var optiTrack : GameObject;

function Start() {
	// Hide mouse cursor
	Screen.showCursor = false;			
}

// Quits the player when the user hits escape
function Update () {
     if (Input.GetKey ("escape") || Input.GetKeyDown(KeyCode.Q)) {
          Application.Quit();
     }
}