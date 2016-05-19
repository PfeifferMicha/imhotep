using UnityEngine;
using System.Collections;

public class OpacitySlider : MonoBehaviour {

	public OpacityControl.ClippableObject objectToClip { get; set; }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void MoveClippingPlane(float f)
    {
		Vector3 pos = objectToClip.clippingPlane.transform.localPosition;
		objectToClip.clippingPlane.transform.localPosition = new Vector3 (pos.x, pos.y, f * 4 - 2);
    }
}
