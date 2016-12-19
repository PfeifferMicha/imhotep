using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawBounds : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool _bIsSelected = true;

	void OnDrawGizmos()
	{
		if (_bIsSelected)
			OnDrawGizmosSelected();
	}


	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, 0.1f);  //center sphere
		if (transform.GetComponent<Renderer> () != null)
			Gizmos.DrawWireCube (transform.GetComponent<Renderer> ().bounds.center, transform.GetComponent<Renderer> ().bounds.size);
		else {
			Bounds bounds = new Bounds ();
			bool boundsInitialized = false;
			Renderer[] renderers = GetComponentsInChildren<Renderer> ();
			foreach (Renderer r in renderers) {
				if (!boundsInitialized) {
					bounds = r.bounds;
					boundsInitialized = true;
				} else {
					bounds.Encapsulate (r.bounds);
				}
			}

			Gizmos.DrawWireCube (bounds.center, bounds.size);
		}
	}
}
