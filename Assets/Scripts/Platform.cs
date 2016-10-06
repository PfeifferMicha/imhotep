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

	public GameObject mainCamera;
	public Camera UICamera;

	public Material UiMeshMaterial;

	public static Platform instance { private set; get; }

	public GameObject UIMesh { private set; get; }

	float initialBaseWidth;
	float initialBaseDepth;

	[Tooltip("Radius of the rounded mesh")]
	public float UIMeshRoundedRadius = 1.2f;
	[Tooltip("Opening angle in degrees")]
	public float UIMeshRoundedAngle = 160f;
	[Tooltip("Distance from platform to bottom of the UI")]
	public float UIMeshRoundedBottom = 0.35f;
	[Tooltip("Height of the UI")]
	public float UIMeshRoundedHeight = 2.0f;
	[Tooltip("Distance from platform to bottom of the UI")]
	public float UIMeshRectangularBottom = 0.5f;
	[Tooltip("Height of the UI")]
	public float UIMeshRectangularHeight = 2.0f;
	[Tooltip("Radius of corners of rectangular mesh")]
	public float UIMeshRectangularRadius = 0.425f;

	public GameObject riftCamera;
	public GameObject viveCamera;


	// Portion of the screen which is rounded (for rectangular setup):
	private float ratioRounded;
	// Portion of the screen which makes up a side (for rectangular setup):
	private float ratioSide;
	// Portion of the screen which makes up front (for rectangular setup):
	private float ratioFront;

	public Platform()
	{
		instance = this;
	}

	void Awake () {
		resetDimensions ();

		// Remember the dimensions of the base:
		initialBaseWidth = rectBase.GetComponent<Renderer>().bounds.size.x;
		initialBaseDepth = rectBase.GetComponent<Renderer>().bounds.size.z;

		//setRectangular (3f, 2f);

		if (! viveCamera.activeInHierarchy) {
			setRectangular ( 3f, 2f );
		} else {
			setRounded ();
		}
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

		mainCamera.transform.localPosition = new Vector3 (0f, 0f, depth * 0.5f);
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
		chair.transform.localPosition = new Vector3 (0f, 0f, initialBaseDepth * 0.75f);
		mainCamera.transform.localPosition = new Vector3 (0f, 0f, initialBaseDepth * 0.75f);

		resetDimensions ();		// make sure base platform has the correct size
		generateUIMeshRounded ();
	}

	//! Enable the rectangular platform:
	void setRectangular( float width, float depth )
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

		setDimensions (width, depth);
		generateUIMeshRectangular (width, depth);
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
		go.layer = LayerMask.NameToLayer ("UIMesh");
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshCollider> ();
		go.GetComponent<MeshFilter>().mesh = mesh;
		go.GetComponent<MeshCollider> ().sharedMesh = mesh;


		// Set up the render texture:
		float meshWidth = 2f*Mathf.PI * UIMeshRoundedRadius * ( UIMeshRoundedAngle / 360f);		// 2*pi*r
		float meshHeight = UIMeshRoundedHeight;
		float pixelsPerMeter = UI.Core.instance.pixelsPerMeter;
		int textureWidth = (int)(meshWidth * pixelsPerMeter);
		int textureHeight = (int)(meshHeight * pixelsPerMeter);
		RenderTexture tex = new RenderTexture (textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32 );
		tex.name = "UI Render Texture";
		UICamera.targetTexture = tex;


		// Set up rendering:
		MeshRenderer renderer = go.AddComponent<MeshRenderer> ();
		renderer.material = UiMeshMaterial;
		renderer.material.mainTexture = tex;

		// Move forward until it reaches the edge of the platform:
		float yPos = rounded.GetComponent<MeshRenderer>().bounds.size.z - UIMeshRoundedRadius;
		go.transform.localPosition = new Vector3 (0f, UIMeshRoundedBottom, yPos);

		UIMesh = go;

		// Let the layout system know about the new aspect ratio:
		UI.Core.instance.setCamera( UICamera );

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

	/*! Generate a UI screen mesh for the rectangular platform. */
	void generateUIMeshRectangular( float width, float depth )
	{
		removeUIMesh ();

		// First, initialize lists:
		List<Vector3> newVertices = new List<Vector3> ();
		List<Vector2> newUV = new List<Vector2> ();
		List<int> newTriangles = new List<int> ();

		float radius = UIMeshRectangularRadius;
		float centerSize = width - 2*left.GetComponent<Renderer> ().bounds.size.x;
		float roundedSize = 2f * Mathf.PI * radius * 0.5f;
		float depthSize = depth - front.GetComponent<Renderer> ().bounds.size.z;
		float fullSize = centerSize + roundedSize + 2f * depthSize;

		// what percentage of the whole mesh is rounded (as opposed to straight):
		ratioRounded = roundedSize / fullSize;
		ratioSide = depthSize / fullSize;
		ratioFront = centerSize / fullSize;

		// Fill the lists with vertices and triangles:
		int numRoundedSegments = 50;
		float fullAngle = Mathf.PI;
		// Add left side mesh:

		newVertices.Add (new Vector3 (-width*0.5f, 0, -depthSize));
		newUV.Add (new Vector2 (0, 0));
		newVertices.Add (new Vector3 (-width*0.5f, UIMeshRectangularHeight, -depthSize));
		newUV.Add (new Vector2 (0, 1));

		// Add first rounded corner:
		for (int i = 0; i <= numRoundedSegments / 2; i++) {
			float currentAmount = (float)i / (float)numRoundedSegments;
			float angle = fullAngle * currentAmount - fullAngle * 0.5f;
			float x = radius * Mathf.Sin (angle) - centerSize*0.5f;
			float y = radius * Mathf.Cos (angle);
			newVertices.Add (new Vector3 (x, 0, y));
			newUV.Add (new Vector2 (currentAmount*ratioRounded + ratioSide, 0));
			newVertices.Add (new Vector3 (x, UIMeshRectangularHeight, y));
			newUV.Add (new Vector2 (currentAmount*ratioRounded + ratioSide, 1));
		}
		// Add second rounded corner (and automatically add center piece):
		for (int i = numRoundedSegments / 2; i <= numRoundedSegments; i++) {
			float currentAmount = (float)i / (float)numRoundedSegments;
			float angle = fullAngle * currentAmount - fullAngle * 0.5f;
			float x = radius * Mathf.Sin (angle) + centerSize*0.5f;
			float y = radius * Mathf.Cos (angle);
			newVertices.Add (new Vector3 (x, 0, y));
			newUV.Add (new Vector2 (currentAmount*ratioRounded + ratioSide + ratioFront, 0));
			newVertices.Add (new Vector3 (x, UIMeshRectangularHeight, y));
			newUV.Add (new Vector2 (currentAmount*ratioRounded + ratioSide + ratioFront, 1));
		}
		// Add right side mesh:
		newVertices.Add (new Vector3 (width*0.5f, 0, -depthSize));
		newUV.Add (new Vector2 (1, 0));
		newVertices.Add (new Vector3 (width*0.5f, UIMeshRectangularHeight, -depthSize));
		newUV.Add (new Vector2 (1, 1));

		for (int i = 0; i <= numRoundedSegments + 2; i++) {
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
		go.layer = LayerMask.NameToLayer ("UIMesh");
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshCollider> ();
		go.GetComponent<MeshFilter>().mesh = mesh;
		go.GetComponent<MeshCollider> ().sharedMesh = mesh;


		// Set up the render texture:
		float meshWidth = fullSize;		// 2*pi*r
		float meshHeight = UIMeshRectangularHeight;
		float pixelsPerMeter = UI.Core.instance.pixelsPerMeter;
		int textureWidth = (int)(meshWidth * pixelsPerMeter);
		int textureHeight = (int)(meshHeight * pixelsPerMeter);
		RenderTexture tex = new RenderTexture (textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32 );
		tex.name = "UI Render Texture";
		UICamera.targetTexture = tex;
		UICamera.orthographicSize = 1f;


		// Set up rendering:
		MeshRenderer renderer = go.AddComponent<MeshRenderer> ();
		renderer.material = UiMeshMaterial;
		renderer.material.mainTexture = tex;


		// Move forward until it reaches the edge of the platform:
		float yPos = depth - front.GetComponent<Renderer>().bounds.size.z;
		go.transform.localPosition = new Vector3 (0f, UIMeshRectangularBottom, yPos);

		UIMesh = go;

		// Let the layout system know about the new aspect ratio:
		UI.Core.instance.setCamera( UICamera );
	}

	/*! Remove UI Mesh, if present. */
	void removeUIMesh()
	{
		if( UIMesh != null )
		{
			Destroy( UIMesh );
		}
	}


	/*! Return a new GameObject into which a new toolstand can be placed.
	 * numberOfToolStands is the total number of toolstands that will be used.
	 * number is the toolstand to generate, which must be between
	 * 0 and numberOfToolStands. */
	public GameObject toolStandPosition( uint number, uint numberOfToolStands )
	{
		GameObject go = new GameObject ("ToolStand Anchor");

		// If the rounded platform is active...
		if (rounded.activeSelf) {
			float radius = 0.6f;//1f;
			float angleStep = 19;
			float startAngle = (numberOfToolStands-1) * angleStep * 0.5f;
			float angle = number * angleStep - startAngle;
			Vector3 center = new Vector3 (0f, 0f, 1.4f);
			Vector3 offset = Quaternion.AngleAxis( angle, Vector3.up) * new Vector3 (0f, 0f, radius );

			go.transform.localPosition = offset + center;
			go.transform.localRotation = Quaternion.AngleAxis ( angle, Vector3.up);
		} else {	// Rectangular platform is active
			float z = front.GetComponent<Renderer>().bounds.min.z - front.transform.position.z;
			float distBetweenStands = 0.3f;
			float startX = (numberOfToolStands-1) * distBetweenStands * 0.5f;
			float x = number * distBetweenStands - startX;

			go.transform.localPosition = new Vector3 (x, 0f, z);
		}
		go.transform.SetParent (transform, false);
		return go;
	}

	public bool getIsRounded()
	{
		if (rounded.activeSelf) {
			return true;
		}
		return false;
	}

	public bool getIsRectangular()
	{
		return !getIsRounded ();
	}

	public Rect getScreenDimensions( UI.Screen screen )
	{
		Rect rect = new Rect ();
		Rect fullScreen = UI.Core.instance.layoutSystem.sizeOfUIScene;

		if (rounded.activeSelf) {
			if (screen == UI.Screen.left) {
				rect.min = fullScreen.min;
				rect.max = new Vector2 (fullScreen.min.x + fullScreen.width * 0.33f, fullScreen.max.y);
			} else if (screen == UI.Screen.right) {
				rect.max = fullScreen.max;
				rect.min = new Vector2 (fullScreen.max.x - fullScreen.width * 0.33f, fullScreen.min.y);
			} else {
				rect.min = new Vector2 (fullScreen.center.x - fullScreen.width * 0.25f, fullScreen.min.y);
				rect.max = new Vector2 (fullScreen.center.x + fullScreen.width * 0.25f, fullScreen.max.y);
			}
		} else {
			if (screen == UI.Screen.left) {
				rect.min = fullScreen.min;
				rect.max = new Vector2 (fullScreen.min.x + fullScreen.width * ratioSide, fullScreen.max.y);
			} else if (screen == UI.Screen.right) {
				rect.max = fullScreen.max;
				rect.min = new Vector2 (fullScreen.max.x - fullScreen.width * ratioSide, fullScreen.min.y);
			} else {
				rect.min = new Vector2 (fullScreen.center.x - fullScreen.width * ratioFront*0.5f, fullScreen.min.y);
				rect.max = new Vector2 (fullScreen.center.x + fullScreen.width * ratioFront*0.5f, fullScreen.max.y);
			}
		}
		return rect;
	}
}
