using UnityEngine;
using System.Collections;

public class TargetLooker : MonoBehaviour {

	public GameObject targetObject;

	void Update () {
		// Always look at target:
		transform.rotation = Quaternion.LookRotation (
			targetObject.transform.position - transform.position);
	}
}
