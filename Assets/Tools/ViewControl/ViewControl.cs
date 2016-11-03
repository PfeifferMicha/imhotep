using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ViewControl : MonoBehaviour {

	public GameObject mainPane;
	public GameObject editPane;
	public GameObject viewNameInputField;
	public Color colorInactive;
	public Color colorActive;

	public MeshLoader mMeshLoader;
	public Button newButton, deleteButton;
	public Text viewNameText;
	public Button buttoPrev, buttonNext;
	public GameObject viewCountElement;

	public GameObject meshViewerScaleNode, meshViewerRotationNode;

	private int currentViewIndex = 0;

	// Use this for initialization
	void Start () {
		viewCountElement.SetActive (false);

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

		mMeshLoader = GameObject.Find("GlobalScript").GetComponent<MeshLoader>();
	}

	void OnDisable()
	{
		// Unregister myself - no longer receives events (until the next OnEnable() call):
		PatientEventSystem.stopListening(PatientEventSystem.Event.MESH_LoadedAll, meshLoaded);
		PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Closed, patientClosed);
	}

	public void meshLoaded( object obj = null )
	{
		newButton.interactable = true;
		currentViewIndex = 0;
		setView (currentViewIndex);
	}
	public void patientClosed( object obj = null )
	{
		deleteButton.interactable = false;
		newButton.interactable = false;
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

			if (p.getViewCount () == 0) {
				viewNameText.text = "No views configured.";
			} else {

				View view = p.getView (index);
				if (view != null) {
					viewNameText.text = (index + 1).ToString ();
					viewNameText.text += ": ";
					viewNameText.text += view.name;

					// Slowly zoom and rotate towards the target:
					meshViewerScaleNode.GetComponent<ModelZoomer> ().setTargetZoom (view.scale, 0.6f);
					meshViewerRotationNode.GetComponent<ModelRotator> ().setTargetOrientation (view.orientation, 0.6f);

					foreach (KeyValuePair<string, double> entry in view.opacities) {
						setMeshOpacity (entry.Key, (float)entry.Value);
					}

					currentViewIndex = index;
				}
			}

			PatientEventSystem.triggerEvent (PatientEventSystem.Event.MESH_Opacity_Changed);

			updateViewCount ();

			if( p.getViewCount () > 0 )
				deleteButton.interactable = true;
			else
				deleteButton.interactable = false;
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
			MeshMaterialControl moc = gameObjectToChangeOpacity.GetComponent<MeshMaterialControl> ();
			if (moc != null) {
				moc.changeOpactiyOfChildren (opacity);
			}
		}
	}

	void updateViewCount()
	{
		Patient p = Patient.getLoadedPatient ();
		if (p != null) {
			int elementsToShow = Math.Max( p.getViewCount (), 1 );

			if (viewCountElement.transform.parent.childCount < elementsToShow + 1 ) {
				int toCreate = elementsToShow - (viewCountElement.transform.parent.childCount - 1);
				for (int i = 0; i < toCreate; i++) {
					GameObject newElem = Instantiate (viewCountElement);
					newElem.transform.SetParent (viewCountElement.transform.parent, false);
					newElem.SetActive (true);
				}
			} else if( viewCountElement.transform.parent.childCount > elementsToShow + 1 ) {
				// Remove the last element until we have the correct amount of elements:
				int toRemove = (viewCountElement.transform.parent.childCount - 1) - elementsToShow;
				for (int i = 0; i < toRemove; i++) {
					Transform tf = viewCountElement.transform.parent.GetChild (viewCountElement.transform.parent.childCount - 1 - i);
					if( tf != null )
						Destroy( tf.gameObject );
				}
			}

			foreach (Transform tf in viewCountElement.transform.parent) {
				tf.GetComponent<Image> ().color = colorInactive;
			}

			// Highlight the currently active view:
			Transform t = viewCountElement.transform.parent.GetChild( currentViewIndex + 1 );
			t.GetComponent<Image>().color = colorActive;
		}
	}
}
