using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ViewControl : MonoBehaviour {

	private GameObject mainPane;
	private GameObject editPane;
	public GameObject viewNameInputField;

	private MeshLoader mMeshLoader;
	public Button saveButton, editButton;
	public Text viewNameText;
	public Button buttoPrev, buttonNext;

	public Shader meshShader, meshShaderTransparent;
	private Transform meshViewerScaleNode, meshViewerRotationNode;

	private int currentViewIndex = 0;

	// Use this for initialization
	void Start () {
		mainPane = transform.FindChild ("Canvas/MainPane").gameObject;
		editPane = transform.FindChild ("Canvas/EditPane").gameObject;

		mMeshLoader = GameObject.Find("GlobalScript").GetComponent<MeshLoader>();

		meshShader = Shader.Find("Custom/MeshShader");
		meshShaderTransparent = Shader.Find("Custom/MeshShaderTransparent");

		meshViewerScaleNode = GameObject.Find ("MeshViewer").transform;
		meshViewerRotationNode = GameObject.Find ("MeshViewer/MeshRotationNode").transform;

		patientClosed ();
		showMainPane ();

		Patient p = Patient.getLoadedPatient ();
		if (p != null) {
			meshLoaded ();
		}
	}

	void OnEnable()
	{
		// Register event callbacks for MESH events:
		PatientEventSystem.startListening(PatientEventSystem.Event.MESH_LoadedAll, meshLoaded);
		PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Closed, patientClosed); 
	}

	void OnDisable()
	{
		// Unregister myself - no longer receives events (until the next OnEnable() call):
		PatientEventSystem.stopListening(PatientEventSystem.Event.MESH_LoadedAll, meshLoaded);
		PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Closed, patientClosed);
	}

	public void meshLoaded( object obj = null )
	{
		Patient p = Patient.getLoadedPatient ();
		List<Patient.View> views = p.getViews ();
		if (views.Count > 0) {
		} else {
			viewNameText.text = "No views configured.";
		}

		saveButton.interactable = true;
		editButton.interactable = true;

		currentViewIndex = 0;
		setView (currentViewIndex);
	}
	public void patientClosed( object obj = null )
	{
		saveButton.interactable = false;
		editButton.interactable = false;
		viewNameText.text = "No patient loaded.";
	}

	public void showMainPane()
	{
		editPane.SetActive (false);
		mainPane.SetActive (true);
	}
	public void showEditPane()
	{
		mainPane.SetActive (false);
		editPane.SetActive (true);
	}
	public void nextView()
	{
		setView (currentViewIndex + 1);
	}
	public void prevView()
	{
		setView (currentViewIndex - 1);
	}

	public void saveNewView()
	{
		Patient p = Patient.getLoadedPatient ();
		if (p != null) {
			string t = viewNameInputField.GetComponent<InputField> ().text;
			if (t.Length > 0) {
				if (mMeshLoader.MeshGameObjectContainers.Count != 0) {
					//createContent();
				}
				Patient.View newView = new Patient.View ();
				newView.name = t;
				newView.orientation = meshViewerRotationNode.localRotation;
				newView.scale = meshViewerScaleNode.localScale;
				newView.opacities = new Dictionary<string,float> ();

				foreach (GameObject g in mMeshLoader.MeshGameObjectContainers) {
					MeshRenderer mr = g.GetComponentInChildren<MeshRenderer> ();
					newView.opacities [g.name] = mr.material.color.a;
				}

				currentViewIndex = p.insertView ( newView, currentViewIndex + 1 );
				setView (currentViewIndex);
		
				showMainPane ();
			}
		}
	}

	void setView( int index )
	{
		Patient p = Patient.getLoadedPatient ();
		if (p != null) {
			Patient.View view = p.getView (index);
			if (view != null) {
				viewNameText.text = (index + 1).ToString();
				viewNameText.text += ": ";
				viewNameText.text += view.name;

				meshViewerRotationNode.localRotation = view.orientation;
				meshViewerScaleNode.localScale = view.scale;

				foreach(KeyValuePair<string, float> entry in view.opacities)
				{
					setObjectOpacity( entry.Key, entry.Value );
				}



				currentViewIndex = index;
			}
		}
	}

	void setObjectOpacity( string name, float opacity )
	{
		// First, find the GameObject which holds the mesh given by "name"
		GameObject gameObjectToChangeOpacity = null;
		foreach (GameObject g in mMeshLoader.MeshGameObjectContainers) {
			if (g.name == name) {
				gameObjectToChangeOpacity = g;
				break;
			}
		}

		// If we found such a GameObject, then set the opacity for all it's children (the meshes):
		if (gameObjectToChangeOpacity != null) {
			if (opacity == 0.0f) {
				gameObjectToChangeOpacity.SetActive (false);
				return;
			} else {
				gameObjectToChangeOpacity.SetActive (true);
			}

			foreach (MeshRenderer mr in gameObjectToChangeOpacity.GetComponentsInChildren<MeshRenderer>()) {
				Material mat = mr.material;
				if(opacity == 1.0f) //Use opaque material
				{
					mat.shader = meshShader;
					mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, opacity);
				}
				else if(opacity > 0.0 && opacity < 1.0f) // Use transparent material
				{
					mat.shader = meshShaderTransparent;
					mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, opacity);
				}
			}
		}
	}
}
