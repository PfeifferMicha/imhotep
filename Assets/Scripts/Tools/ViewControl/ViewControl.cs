using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ViewControl : MonoBehaviour {

	private GameObject mainPane;
	private GameObject editPane;
	public GameObject viewNameInputField;
	public GameObject meshViewerRotationNode;
	public GameObject meshViewerScaleNode;

	private MeshLoader mMeshLoader;
	public Button saveButton, editButton;
	public Text viewNameText;
	public Button buttoPrev, buttonNext;

	private int currentViewIndex = 0;

	// Use this for initialization
	void Start () {
		mainPane = transform.FindChild ("Canvas/MainPane").gameObject;
		editPane = transform.FindChild ("Canvas/EditPane").gameObject;

		mMeshLoader = GameObject.Find("GlobalScript").GetComponent<MeshLoader>();

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
				newView.orientation = meshViewerRotationNode.transform.localRotation;
				newView.scale = meshViewerScaleNode.transform.localScale;

				foreach (GameObject g in mMeshLoader.MeshGameObjectContainers) {
					newView.opacities [g.name] = 0.5f;
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

				meshViewerRotationNode.transform.localRotation = view.orientation;
				meshViewerScaleNode.transform.localScale = view.scale;

				currentViewIndex = index;
			}
		}
	}

	void setObjectOpacity( string name, float opacity )
	{
		GameObject gameObjectToChangeOpacity = null;
		foreach (GameObject g in mMeshLoader.MeshGameObjectContainers) {
			if (g.name == name) {
				gameObjectToChangeOpacity = g;
				break;
			}
		}
		if (gameObjectToChangeOpacity != null) {
			if (opacity == 0.0f) {
				gameObjectToChangeOpacity.SetActive (false);
				return;
			} else {
				gameObjectToChangeOpacity.SetActive (true);
			}

			foreach (MeshRenderer mr in gameObjectToChangeOpacity.GetComponentsInChildren<MeshRenderer>()) {
				Color col = mr.material.color;
				col.a = opacity;
				mr.material.color = col;

				/*if (f == 1.0f) { //Use opaque material
					Material mat = Resources.Load ("Materials/DefaultMaterialAfterLoadingOpaque", typeof(Material)) as Material;
					mat.color = new Color (mr.material.color.r, mr.material.color.g, mr.material.color.b, f);

					mr.material = new Material (mat);
				} else if (f > 0.0 && f < 1.0f) { // Use transparent material
					Material mat = Resources.Load ("Materials/DefaultMaterialAfterLoadingTransparent", typeof(Material)) as Material;
					mat.color = new Color (mr.material.color.r, mr.material.color.g, mr.material.color.b, f);

					mr.material = new Material (mat); 
				}*/
			}
		}
	}
}
