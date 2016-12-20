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
		Vector3 pos = transform.localPosition;
		if (copyX)
			pos.x = target.localPosition.x + offsetX;
		if (copyY)
			pos.y = target.localPosition.y + offsetY;
		if (copyZ)
			pos.z = target.localPosition.z + offsetZ;
		transform.localPosition = pos;
	}
}
