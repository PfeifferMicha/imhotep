using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ClippingControl : MonoBehaviour {

	public GameObject defaultLine;
	public GameObject mainControlSliderObject = null;
	public Color clippingPlaneColor = new Color (0.3f, 0.7f, 0.3f, 0.1f);

	public GameObject plane { private set; get; }
	private Slider mainControlSlider = null;

	private GameObject meshViewer = null;
	private GameObject meshNode = null;

	public class ClippableObject
	{
		public GameObject meshObject = null;
		public GameObject clippingPlane = null;
		public GameObject slider = null;
	}

	private List<ClippableObject> loadedObjects = new List<ClippableObject>();

    private MeshLoader mMeshLoader;

	void OnEnable()
	{
		// Register event callbacks for MESH events:
		PatientEventSystem.startListening (PatientEventSystem.Event.MESH_LoadedAll, createContent);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, clearContent);

		if (meshViewer == null || meshNode == null) {
			meshViewer = GameObject.Find ("MeshViewer");
			if (meshViewer != null) {

				// Get the node which the objects are attached to:
				meshNode = meshViewer.transform.Find ("MeshRotationNode/MeshPositionNode").gameObject;
			}
		}

		mainControlSlider = mainControlSliderObject.GetComponent<Slider> ();
		mainControlSlider.value = 0.5f;


		plane = GameObject.CreatePrimitive (PrimitiveType.Plane);
		plane.transform.SetParent (meshViewer.transform, false);
		plane.transform.localScale = new Vector3 (0.35f, 0.35f, 0.35f);
		plane.transform.Rotate (new Vector3 (-90f, 0f, 0f));


		Material newMat = Resources.Load("Materials/ShaderCuttingPlane", typeof(Material)) as Material;
		//Material newMat = new Material (mat);	// Duplicate
		newMat.SetColor ("_Color", clippingPlaneColor );
		plane.GetComponent<Renderer>().material =  newMat;
		//setClippingPlaneDistance (0.5f);
	}

	void OnDisable()
	{
		// Unregister myself - no longer receives events (until the next OnEnable() call):
		PatientEventSystem.stopListening( PatientEventSystem.Event.MESH_LoadedAll, createContent);
		clearContent ();
		Destroy (plane);
	}

	public void setClippingPlaneDistance( float newVal )
	{
		//Vector3 pos = clippingPlane.transform.localPosition;
		//clippingPlane.transform.localPosition = new Vector3 (pos.x, pos.y, newVal * 4 - 2);
		foreach (ClippableObject clippable in loadedObjects) {
			clippable.slider.GetComponent<Slider> ().value = newVal;
		}
	}


    // Use this for initialization
    void Start () {
        mMeshLoader = GameObject.Find("GlobalScript").GetComponent<MeshLoader>();
        defaultLine.SetActive(false);
		if (mMeshLoader.MeshGameObjectContainers.Count != 0) {
			createContent ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		foreach( ClippableObject clippable in loadedObjects ) {
			foreach (Transform g in clippable.meshObject.transform) {
				Material mat = g.GetComponent<Renderer> ().material;

				mat.SetVector ("_cuttingPlanePosition", meshNode.transform.InverseTransformPoint (clippable.clippingPlane.transform.position));
				mat.SetVector ("_cuttingPlaneNormal", meshNode.transform.InverseTransformDirection (clippable.clippingPlane.transform.forward));
			}
		}
		Color col = plane.GetComponent<Renderer>().material.GetColor ("_Color");
		col.a = Math.Max (col.a - Time.deltaTime*0.1f, 0.0f);
		plane.GetComponent<Renderer>().material.SetColor ("_Color", col);
    }

	private void createContent( object obj = null )
    {
		clearContent ();

        foreach(GameObject g in mMeshLoader.MeshGameObjectContainers)
        {
			// Remember the object:
			ClippableObject clippable = GenerateClippingPlane( g );
			loadedObjects.Add ( clippable );

            // Create a new instance of the list button:
            GameObject newLine = Instantiate(defaultLine).gameObject;
            newLine.SetActive(true);

            // Attach the new button to the list:
            newLine.transform.SetParent(defaultLine.transform.parent, false);

            //Save game object in slider
			GameObject slider = newLine.transform.Find("Slider").gameObject;
			slider.GetComponent<Slider> ().value = 0.5f;
			slider.GetComponent<ClippingSlider>().objectToClip = clippable;
			slider.GetComponent<ClippingSlider> ().parentControl = this;
			slider.GetComponent<ClippingSlider> ().defaultColor = clippingPlaneColor;

            // Change button text to name of object:
            GameObject textObject = newLine.transform.Find("Text").gameObject;
            Text buttonText = textObject.GetComponent<Text>();
            buttonText.text = g.name;

			clippable.slider = slider;
        }
    }

	void clearContent( object obj = null )
	{
		//Destroy all sliders:
		for(int i = 0; i < defaultLine.transform.parent.childCount; i++)
		{
			if(i != 0) //TODO !=0
			{
				Destroy(defaultLine.transform.parent.GetChild(i).gameObject);
			}
		}

		// Reset shader for each of the loaded objects:
		foreach( ClippableObject clippable in loadedObjects ) {
			foreach (Transform g in clippable.meshObject.transform) {
				Material mat = g.GetComponent<Renderer> ().material;

				mat.SetVector ("_cuttingPlanePosition", new Vector4( 9999f, 0f, 0f, 1f ) );
				mat.SetVector ("_cuttingPlaneNormal", new Vector4( -1f, 0f, 0f, 1f ) );
			}
			Destroy (clippable.clippingPlane);
		}

		// Clear previously loaded objects:
		loadedObjects = new List<ClippableObject>();
	}

	ClippableObject GenerateClippingPlane( GameObject meshObject )
	{
		// Generate the new clipping plane:
		GameObject clippingPlane = new GameObject( "ClippingPlane" );
		clippingPlane.transform.SetParent (meshViewer.transform, false);

		ClippableObject obj = new ClippableObject ();
		obj.meshObject = meshObject;
		obj.clippingPlane = clippingPlane;

		return obj;
	}
}
