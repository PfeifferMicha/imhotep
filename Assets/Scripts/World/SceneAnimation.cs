using UnityEngine;
using System.Collections;

public class SceneAnimation : MonoBehaviour {

	public static SceneAnimation instance;

	public bool skipStartupAnimation;

	public GameObject sphereEmitters;
	public GameObject sphere;

	public SceneAnimation()
	{
		if (instance != null) {
			throw(new System.Exception ("Only one instance of SceneAnimation may be present!"));
		}
		instance = this;
	}

	// Use this for initialization
	void OnEnable () {
		if (skipStartupAnimation) {
			sphereEmitters.GetComponent<Animator> ().Play ("EnableSphereEmitters");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
