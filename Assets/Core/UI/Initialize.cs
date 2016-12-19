using UnityEngine;
using System.Collections;

public class Initialize : MonoBehaviour {

	void Start () {
		if (Config.instance.skipAnimations)
			gameObject.SetActive (false);
	}
}
