using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour {

	public GameObject rectBase;
	public GameObject front;
	public GameObject left;
	public GameObject right;
	public GameObject frontLeft;
	public GameObject frontRight;
	public GameObject rounded;
	public GameObject chair;

	public GameObject camera;

	float initialBaseWidth;
	float initialBaseDepth;

	// Use this for initialization
	void Start () {
		resetDimensions ();

		// Remember the dimensions of the base:
		initialBaseWidth = rectBase.GetComponent<Renderer>().bounds.size.x;
		initialBaseDepth = rectBase.GetComponent<Renderer>().bounds.size.z;

		setRectangular ();
		setDimensions (2f, 1.5f);
	}

	/*! Set the size of the rectangular platform
	 * (The rounded platform has a fixed size) */
	void setDimensions( float width, float depth )
	{
		float sideWidth = left.GetComponent<Renderer>().bounds.size.x;
		float frontDepth = front.GetComponent<Renderer>().bounds.size.z;

		float newBaseWidth = (width - 2f*sideWidth) / initialBaseWidth;
		float newBaseDepth = (depth - frontDepth) / initialBaseDepth;

		rectBase.transform.localScale = new Vector3 ( newBaseWidth, newBaseDepth, 1f );

		// Figure out where to put the front part:
		Vector3 pos = front.transform.localPosition;
		pos.z = depth - frontDepth - initialBaseDepth;//front.transform.localPosition.y + (depth - currentY);
		front.transform.localPosition = pos;
		// Figure out where to put left and right parts:
		pos = right.transform.localPosition;
		pos.x = (width - initialBaseWidth)*0.5f - sideWidth;//front.transform.localPosition.y + (depth - currentY);
		left.transform.localPosition = -pos;
		right.transform.localPosition = pos;

		left.transform.localScale = new Vector3 ( 1f, newBaseDepth, 1f );
		right.transform.localScale = new Vector3 ( 1f, newBaseDepth, 1f );
		front.transform.localScale = new Vector3 ( newBaseWidth, 1f, 1f );

		frontLeft.transform.localPosition = new Vector3 (-pos.x, 0, depth - frontDepth - initialBaseDepth);
		frontRight.transform.localPosition = new Vector3 (pos.x, 0, depth - frontDepth - initialBaseDepth);

		camera.transform.localPosition = new Vector3 (0f, 0f, depth * 0.5f);
	}
	void resetDimensions()
	{
		Vector3 origScale = new Vector3(1,1,1);
		rectBase.transform.localScale = origScale;
		front.transform.localScale = origScale;
		left.transform.localScale = origScale;
		right.transform.localScale = origScale;
		frontLeft.transform.localScale = origScale;
		frontRight.transform.localScale = origScale;
	}

	//! Enable the rounded platform:
	void setRounded()
	{
		// Deactivate all parts of the rectangular platform:
		front.SetActive (false);
		left.SetActive (false);
		right.SetActive (false);
		frontLeft.SetActive (false);
		frontRight.SetActive (false);

		// Activate the rounded platform:
		rounded.SetActive (true);
		chair.SetActive (true);

		resetDimensions ();
	}

	//! Enable the rectangular platform:
	void setRectangular()
	{
		// Activate all parts of the rectangular platform:
		front.SetActive (true);
		left.SetActive (true);
		right.SetActive (true);
		frontLeft.SetActive (true);
		frontRight.SetActive (true);

		// Deactivate the rounded platform:
		rounded.SetActive (false);
		chair.SetActive (false);
	}
}
