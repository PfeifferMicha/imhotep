using UnityEngine;
using System.Collections;

public class SceneAnimation : MonoBehaviour {

	public static SceneAnimation instance;

	public GameObject sphereEmitters;
	public GameObject sphere;

	public SceneAnimation()
	{
		if (instance != null) {
			throw(new System.Exception ("Error: Cannot create more than one instance of SceneAnimation!"));
		}
		instance = this;
	}

	// Use this for initialization
	void OnEnable () {
		if (Config.instance.skipAnimations) {
			sphereEmitters.GetComponent<Animator> ().Play ("EnableSphereEmitters");
		}
	}
}
