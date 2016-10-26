using UnityEngine;
using System.Collections;

public class PanToTarget : MonoBehaviour {

	public GameObject target;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 diff = target.transform.position - transform.position;
		float pan = Mathf.Atan2 (diff.x, diff.z);
		transform.rotation = Quaternion.AngleAxis (pan*180f/Mathf.PI + 180, Vector3.up);
	}
}
