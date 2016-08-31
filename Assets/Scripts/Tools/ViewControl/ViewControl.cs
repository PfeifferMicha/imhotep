using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ViewControl : MonoBehaviour {

	private GameObject mainPane;
	private GameObject editPane;
	public GameObject viewNameInputField;

	private MeshLoader mMeshLoader;
	public Button saveButton, newButton;
	public Text viewNameText;
	public Button buttoPrev, buttonNext;
	public Text viewNumberText;

	private Shader meshShader, meshShaderTransparent;
	public GameObject meshViewerScaleNode, meshViewerRotationNode;

	private int currentViewIndex = 0;

	// Use this for initialization
	void Start () {
		mainPane = transform.FindChild ("Canvas/MainPane").gameObject;
		editPane = transform.FindChild ("Canvas/EditPane").gameObject;

		mMeshLoader = GameObject.Find("GlobalScript").GetComponent<MeshLoader>();

		meshShader = Shader.Find("Custom/MeshShader");
		meshShaderTransparent = Shader.Find("Custom/MeshShaderTransparent");

		//meshViewerScaleNode = GameObject.Find ("MeshViewerBase/MeshViewerScale");
		//meshViewerRotationNode = GameObject.Find ("MeshViewerBase/MeshViewerScale/MeshRotationNode");

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
		currentViewIndex = 0;
		setView (currentViewIndex);

		saveButton.interactable = true;
		newButton.interactable = true;

		setView (currentViewIndex);
	}
	public void patientClosed( object obj = null )
	{
		saveButton.interactable = false;
		newButton.interactable = false;
		viewNameText.text = "No patient loaded.";
		viewNumberText.text = "";
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
				View newView = new View ();
				newView.name = t;
				newView.orientation = meshViewerRotationNode.transform.localRotation;
				newView.scale = meshViewerScaleNode.transform.localScale;
				//newView.opacities = new Dictionary<string,double> ();

				foreach (GameObject g in mMeshLoader.MeshGameObjectContainers) {
					MeshRenderer mr = g.GetComponentInChildren<MeshRenderer> ();
					if (g.activeSelf) {
						newView.opacities [g.name] = mr.material.color.a;
					} else {
						newView.opacities [g.name] = 0.0f;
					}
				}

				currentViewIndex = p.insertView ( newView, currentViewIndex + 1 );
				setView (currentViewIndex);

				p.saveViews ();
		
				showMainPane ();
			}
		}
	}

	public void deleteView()
	{
		Patient p = Patient.getLoadedPatient ();
		if (p != null) {
			p.deleteView (currentViewIndex);
			p.saveViews ();

			if (p.getViewCount() <= currentViewIndex) {
				currentViewIndex = p.getViewCount() - 1;
			}
		}
		setView (currentViewIndex);
	}

	void setView( int index )
	{
		Patient p = Patient.getLoadedPatient ();
		if (p != null) {

			if (p.getViewCount() == 0) {
				viewNameText.text = "No views configured.";
				return;
			}

			View view = p.getView (index);
			if (view != null) {
				viewNameText.text = (index + 1).ToString();
				viewNameText.text += ": ";
				viewNameText.text += view.name;

				// Slowly zoom and rotate towards the target:
				meshViewerScaleNode.GetComponent<ModelZoomer> ().setTargetZoom (view.scale, 0.6f);
				meshViewerRotationNode.GetComponent<ModelRotator> ().setTargetOrientation (view.orientation, 0.6f );

				foreach(KeyValuePair<string, double> entry in view.opacities)
				{
					setMeshOpacity( entry.Key, (float)entry.Value );
				}

				currentViewIndex = index;
			}
			viewNumberText.text = (currentViewIndex+1).ToString () + "/" + p.getViewCount ().ToString ();

			PatientEventSystem.triggerEvent (PatientEventSystem.Event.MESH_Opacity_Changed);
		}
	}

	void setMeshOpacity( string name, float opacity )
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
			MeshOpacityChanger moc = gameObjectToChangeOpacity.GetComponent<MeshOpacityChanger> ();
			if (moc != null) {
				moc.changeOpactiyOfChildren (opacity);
			}
		}
	}
}
