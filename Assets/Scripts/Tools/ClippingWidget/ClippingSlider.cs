using UnityEngine;
using System.Collections;

public class ClippingSlider : MonoBehaviour {

	public ClippingControl.ClippableObject objectToClip { get; set; }
	public Color defaultColor = new Color( 1.0f,0.7f,0.5f,0.1f);
	public ClippingControl parentControl = null;

	public ClippingSlider()
	{
		objectToClip = null;
	}

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
	
	}

    public void MoveClippingPlane(float f)
    {
		if (objectToClip != null) {
			Vector3 pos = objectToClip.clippingPlane.transform.localPosition;
			objectToClip.clippingPlane.transform.localPosition = new Vector3 (pos.x, pos.y, f * 4 - 2);
			parentControl.plane.transform.localPosition = objectToClip.clippingPlane.transform.localPosition;
			parentControl.plane.GetComponent<Renderer>().material.SetColor ("_Color", defaultColor);
		}
    }
}
