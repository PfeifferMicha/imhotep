using UnityEngine;
using System.Collections;

public class Sphere : MonoBehaviour {

	public GameObject logo;

	public void OnEnable()
	{
		//logo.SetActive (false);
	}
	public void activateLogo()
	{
		logo.GetComponent<MeshRenderer> ().material.SetFloat ("_EaseInAmount", 0f);
		logo.SetActive (true);
	}

}
