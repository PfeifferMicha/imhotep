using UnityEngine;
using System.Collections;

public class SceneAnimation : MonoBehaviour {

	public static SceneAnimation instance;

	public GameObject sphereEmitters;
	public GameObject sphere;
	public GameObject logo;
	public GameObject PatientSelector;

	public SceneAnimation()
	{
		if (instance != null) {
			throw(new System.Exception ("Error: Cannot create more than one instance of SceneAnimation!"));
		}
		instance = this;
	}

	// Use this for initialization
	void OnEnable () {

		// If we're skipping the startup animations...
		if (Config.instance.skipAnimations) {

			// Activate all the objects:
			sphere.SetActive (true);
			logo.SetActive (false);
			PatientSelector.SetActive (true);
			sphereEmitters.SetActive (true);

			// Start the animations of the objects, and set their normalized time to 1 (the end)
			sphereEmitters.GetComponent<Animator> ().Play ("EnableSphereEmitters", -1, 1f);
			sphere.GetComponent<Animator> ().Play ("EnableSphere", -1, 1f);
			logo.GetComponent<Animator> ().Play ("LogoActivate", -1, 1f);

		} else {
			sphere.SetActive (false);
			logo.SetActive (false);
			PatientSelector.SetActive (false);
		}

	}
}
