using UnityEngine;
using System.Collections;

public class SphereEmitters : MonoBehaviour {

	public GameObject sphere;

	public void OnEnable()
	{
		// Disable the sphere at startup:
		//sphere.SetActive (false);
	}

	public void activateSphere()
	{
		Animator sphereAnimator = sphere.GetComponent<Animator> ();
		sphereAnimator.SetTrigger ("Activate");
		sphere.GetComponent<MeshRenderer> ().material.SetFloat ("_AppearAmount", 0);

		// Make sure to show the sphere:
		sphere.SetActive (true);
	}
}
