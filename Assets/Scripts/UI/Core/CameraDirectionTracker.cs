using UnityEngine;
using System.Collections;


/*! Notifies the layout system about the position of the UI Mesh which the camera is currently looking at.
 * Attach this to the main camera.
* Updated every frame. */
public class CameraDirectionTracker : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void Update () {
		// Get a ray which looks forward from the camera's position:
		Ray ray = new Ray (transform.position, transform.forward);

		// Shoot the ray only at the UIMesh, ignore everything else:
		LayerMask mask = LayerMask.NameToLayer ("UIMesh");

		RaycastHit result;
		if (Physics.Raycast (ray, out result, Mathf.Infinity)) {
			// Let the Layout System know at what position we're looking (so it can decide which screen to activate):
			UI.Core.instance.layoutSystem.setLookAtPosition (result.textureCoord);
		}
	}
}
