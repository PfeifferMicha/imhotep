using UnityEngine;
using System.Collections;

public class CopyPosition : MonoBehaviour {

	public Transform target;
	public bool copyX = false;
	public bool copyY = false;
	public bool copyZ = false;
	public float offsetX = 0f;
	public float offsetY = 0f;
	public float offsetZ = 0f;

	void Update () {
		Vector3 pos = transform.position;
		if (copyX)
			pos.x = target.position.x + offsetX;
		if (copyY)
			pos.y = target.position.y + offsetY;
		if (copyZ)
			pos.z = target.position.z + offsetZ;
		transform.position = pos;
	}
}
