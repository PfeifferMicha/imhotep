using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ModelEffectHandler : MonoBehaviour {

	private bool loadingEffectActive = false;
	private float loadingAmount = 0f;

	public GameObject cameraObject;
	public GameObject shaderCuttingPlane;
	public GameObject meshNode;
	public List<GameObject> loadedObjects = new List<GameObject> ();

	void Start () {
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, eventStartLoadingMesh);
		PatientEventSystem.startListening( PatientEventSystem.Event.MESH_LoadedSingle, eventFinishLoadingMesh );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_FinishedLoading, eventFinishLoadingAllMeshes );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, eventPatientClosed );
	}

	void Update () {

		if (loadingEffectActive) {
			loadingAmount = loadingAmount + 0.5f*Time.deltaTime;
			foreach (GameObject o in loadedObjects) {
				MeshMaterialControl matControl = o.gameObject.transform.parent.GetComponent<MeshMaterialControl> ();
				if (matControl != null) {
					matControl.SetLoadingEffectAmount (loadingAmount);
				}
			}
			if (loadingAmount > 1)
				loadingEffectActive = false;
		}
	}

	void eventStartLoadingMesh( object obj )
	{
		loadingEffectActive = false;
		loadedObjects.Clear ();
	}

	// Called when a new mesh-part has been fully loaded.
	// Hides the mesh.
	void eventFinishLoadingMesh( object obj )
	{
		GameObject gameObject = obj as GameObject;
		if (gameObject != null) {
			loadedObjects.Add (gameObject);
		}
	}

	void eventFinishLoadingAllMeshes( object obj )
	{
		loadingEffectActive = true;
		loadingAmount = 0f;

		foreach (GameObject o in loadedObjects) {
			o.SetActive(true);
		}
	}

	void eventPatientClosed( object obj )
	{
		loadedObjects.Clear ();
		loadingEffectActive = false;
	}
}
