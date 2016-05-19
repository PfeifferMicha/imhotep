using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OpacityControl : MonoBehaviour {

	public GameObject defaultLine;
	public GameObject mainControlSliderObject = null;
	private Slider mainControlSlider = null;
	private GameObject clippingPlane = null;
	private GameObject meshNode = null;

	private List<GameObject> loadedObjects = new List<GameObject>();

    private MeshLoader mMeshLoader;

	void OnEnable()
	{
		// Register event callbacks for MESH events:
		PatientEventSystem.startListening (PatientEventSystem.Event.MESH_LoadedAll, createContent);

		// Try to locate the clipping plane:
		if (clippingPlane == null) {
			GameObject meshViewer = GameObject.Find ("MeshViewer");
			if (meshViewer != null) {

				meshNode = meshViewer.transform.Find ("MeshRotationNode/MeshPositionNode").gameObject;

				clippingPlane = new GameObject ();
				clippingPlane.transform.SetParent (meshViewer.transform, false);

				GameObject plane = GameObject.CreatePrimitive (PrimitiveType.Plane);
				plane.transform.SetParent (clippingPlane.transform, false);
				plane.transform.localScale = new Vector3 (0.35f, 0.35f, 0.35f);
				plane.transform.Rotate (new Vector3 (-90f, 0f, 0f));

				Material newMat = Resources.Load("Materials/ShaderCuttingPlane", typeof(Material)) as Material;
				plane.GetComponent<Renderer>().material =  newMat;
				//clippingPlane = meshViewer.transform.FindChild ("ShaderCuttingPlane").gameObject;
			}
		}

		mainControlSlider = mainControlSliderObject.GetComponent<Slider> ();
		mainControlSlider.value = 0.5f;
		//setClippingPlaneDistance (0.5f);
	}

	void OnDisable()
	{
		// Unregister myself - no longer receives events (until the next OnEnable() call):
		PatientEventSystem.stopListening( PatientEventSystem.Event.MESH_LoadedAll, createContent);

		if (clippingPlane != null) {
			Destroy (clippingPlane);
			clippingPlane = null;
		}
		ClearContent ();
	}

	public void setClippingPlaneDistance( float newVal )
	{
		Vector3 pos = clippingPlane.transform.localPosition;
		clippingPlane.transform.localPosition = new Vector3 (pos.x, pos.y, newVal * 4 - 2);
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
		foreach( GameObject container in loadedObjects ) {
			foreach (Transform g in container.transform) {
				Material mat = g.GetComponent<Renderer> ().material;

				mat.SetVector ("_cuttingPlanePosition", meshNode.transform.InverseTransformPoint (clippingPlane.transform.position));
				mat.SetVector ("_cuttingPlaneNormal", meshNode.transform.InverseTransformDirection (clippingPlane.transform.forward));
			}
		}
    }

	private void createContent( object obj = null )
    {
		ClearContent ();

        foreach(GameObject g in mMeshLoader.MeshGameObjectContainers)
        {
            // Create a new instance of the list button:
            GameObject newLine = Instantiate(defaultLine).gameObject;
            newLine.SetActive(true);

            // Attach the new button to the list:
            newLine.transform.SetParent(defaultLine.transform.parent, false);

            //Save game object in slider
            GameObject slider = newLine.transform.Find("Slider").gameObject;
            slider.GetComponent<OpacitySlider>().gameObjectToChangeOpacity = g;

            // Change button text to name of tool:
            GameObject textObject = newLine.transform.Find("Text").gameObject;
            Text buttonText = textObject.GetComponent<Text>();
            buttonText.text = g.name;

			loadedObjects.Add (g);
        }
    }

	void ClearContent()
	{
		//Destroy all object except for default line
		for(int i = 0; i < defaultLine.transform.parent.childCount; i++)
		{
			if(i != 0) //TODO !=0
			{
				Destroy(defaultLine.transform.parent.GetChild(i).gameObject);
			}
		}

		// Reset shader for each of the loaded objects:
		foreach( GameObject container in loadedObjects ) {
			foreach (Transform g in container.transform) {
				Material mat = g.GetComponent<Renderer> ().material;

				mat.SetVector ("_cuttingPlanePosition", new Vector4( 9999f, 0f, 0f, 1f ) );
				mat.SetVector ("_cuttingPlaneNormal", new Vector4( -1f, 0f, 0f, 1f ) );
			}
		}

		// Clear previously loaded objects:
		loadedObjects = new List<GameObject>();
	}
}
