using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
	public GameObject UIcamera;

	public Material UiMeshMaterial;

	GameObject UIMesh = null;

	float initialBaseWidth;
	float initialBaseDepth;

	[Tooltip("Radius of the rounded mesh")]
	public float UIMeshRoundedRadius = 1.2f;
	[Tooltip("Opening angle in degrees")]
	public float UIMeshRoundedAngle = 160f;
	[Tooltip("Distance from platform to bottom of the UI")]
	public float UIMeshRoundedBottom = 0.3f;
	[Tooltip("Height of the UI")]
	public float UIMeshRoundedHeight = 2.0f;
	[Tooltip("Distance from platform to bottom of the UI")]
	public float UIMeshRectangularBottom = 0.5f;
	[Tooltip("Height of the UI")]
	public float UIMeshRectangularHeight = 2.0f;

	// Use this for initialization
	void Start () {
		resetDimensions ();

		// Remember the dimensions of the base:
		initialBaseWidth = rectBase.GetComponent<Renderer>().bounds.size.x;
		initialBaseDepth = rectBase.GetComponent<Renderer>().bounds.size.z;

		setRectangular ();
		setDimensions (2f, 1.5f);

		setRounded ();
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
		generateUIMeshRounded ();
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

	/*! Generate a UI screen mesh for the rectangular platform. */
	void generateUIMeshRectangular( float width, float depth )
	{
		removeUIMesh ();
	}

	/*! Generate a UI screen mesh for the rounded platform. */
	void generateUIMeshRounded()
	{
		removeUIMesh ();

		// First, initialize lists:
		List<Vector3> newVertices = new List<Vector3> ();
		List<Vector2> newUV = new List<Vector2> ();
		List<int> newTriangles = new List<int> ();

		// Fill the lists with vertices and triangles:
		int numSegments = 50;
		float fullAngle = UIMeshRoundedAngle * Mathf.PI / 180f;
		for (int i = 0; i <= numSegments; i++) {
			float currentAmount = (float)i / (float)numSegments;
			float angle = fullAngle * currentAmount - fullAngle * 0.5f;
			float x = UIMeshRoundedRadius * Mathf.Sin (angle);
			float y = UIMeshRoundedRadius * Mathf.Cos (angle);
			newVertices.Add (new Vector3 (x, 0, y));
			newUV.Add (new Vector2 (currentAmount, 0));
			newVertices.Add (new Vector3 (x, UIMeshRoundedHeight, y));
			newUV.Add (new Vector2 (currentAmount, 1));
		}
		for (int i = 0; i <= numSegments - 1; i++) {
			newTriangles.Add (i * 2 + 0);
			newTriangles.Add (i * 2 + 1);
			newTriangles.Add (i * 2 + 2);
			newTriangles.Add (i * 2 + 1);
			newTriangles.Add (i * 2 + 3);
			newTriangles.Add (i * 2 + 2);
		}

		// Generate a mesh from the lists:
		Mesh mesh = new Mesh();
		mesh.name = "UIMesh";
		mesh.vertices = newVertices.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.RecalculateNormals ();


		// Generate a new game object:
		GameObject go = new GameObject("UIMesh");
		go.transform.SetParent( transform, false );
		go.layer = LayerMask.NameToLayer ("MousePlane");
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshCollider> ();
		go.GetComponent<MeshFilter>().mesh = mesh;
		go.GetComponent<MeshCollider> ().sharedMesh = mesh;


		// Set up the render texture:
		float meshWidth = 2f*Mathf.PI * UIMeshRoundedRadius * ( UIMeshRoundedAngle / 360f);		// 2*pi*r
		float meshHeight = UIMeshRoundedHeight;
		float pixelsPerMeter = 500;
		int textureWidth = (int)(meshWidth * pixelsPerMeter);
		int textureHeight = (int)(meshHeight * pixelsPerMeter);
		RenderTexture tex = new RenderTexture (textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32 );
		tex.name = "UI Render Texture";
		UIcamera.GetComponent<Camera>().targetTexture = tex;


		// Set up rendering:
		MeshRenderer renderer = go.AddComponent<MeshRenderer> ();
		renderer.material = UiMeshMaterial;
		renderer.material.mainTexture = tex;

		// Move forward until it reaches the edge of the platform:
		float yPos = rounded.GetComponent<MeshRenderer>().bounds.size.z - UIMeshRoundedRadius;
		go.transform.localPosition = new Vector3 (0f, UIMeshRoundedBottom, yPos);

		// Debug write image to file:
		/* cUIcamera.GetComponent<Camera> ().Render ();
		RenderTexture.active = tex;
		Texture2D virtualPhoto =
			new Texture2D(textureWidth,textureHeight, TextureFormat.ARGB32, false);
		virtualPhoto.ReadPixels ( new Rect( 0,0, textureWidth, textureHeight ), 0, 0 );
		virtualPhoto.Apply ();
		RenderTexture.active = null;

		byte[] bytes;
		bytes = virtualPhoto.EncodeToPNG ();
		System.IO.File.WriteAllBytes("/home/micha/tmp.png", bytes );*/
	}

	/*! Remove UI Mesh, if present. */
	void removeUIMesh()
	{
		if( UIMesh != null )
		{
			Destroy( UIMesh );
		}
	}
}
