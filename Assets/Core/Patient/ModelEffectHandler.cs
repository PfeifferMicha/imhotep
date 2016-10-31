using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ModelEffectHandler : MonoBehaviour {

	private bool loadingEffectActive = false;
	private bool currentlyLoadingNewMeshes = false;

	public GameObject cameraObject;
	public GameObject shaderCuttingPlane;
	public GameObject meshNode;

	class LoadObject
	{
		public GameObject gameObject;
		public float amount;
	};

	List<LoadObject> loadingObjects = new List<LoadObject>();

	void Start () {
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, eventStartLoadingMesh);
		PatientEventSystem.startListening( PatientEventSystem.Event.MESH_LoadedSingle, eventFinishLoadingMesh );
		PatientEventSystem.startListening( PatientEventSystem.Event.MESH_LoadedAll, eventFinishLoadingAllMeshes );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, eventPatientClosed );
	}

	void Update () {
		
		if (loadingEffectActive) {
			bool allMeshesFinishedAnimation = true;
			foreach (LoadObject lObj in loadingObjects) {
				if (lObj.amount >= 0f) {
					lObj.amount += Time.deltaTime * 0.25f;
				}
					
				if (lObj.amount < 1.3) {
					allMeshesFinishedAnimation = false;
				}
				lObj.gameObject.GetComponent<Renderer> ().material.SetFloat ("_amount", lObj.amount);

				/*foreach (Transform child in lObj.gameObject.transform) {
					Material mat = child.gameObject.GetComponent<Renderer> ().material;
					mat.SetFloat ("_amount", amount);
				}*/
			}
			if (!currentlyLoadingNewMeshes && allMeshesFinishedAnimation) {
				loadingEffectActive = false;
				foreach (LoadObject lObj in loadingObjects) {
					lObj.gameObject.GetComponent<Renderer> ().material.SetFloat ("_amount", 1.5f);
					/*foreach (Transform child in lObj.gameObject.transform) {
						Material mat = child.gameObject.GetComponent<Renderer> ().material;
						mat.SetFloat ("_amount", 1.5f);
					}*/
				}
			}
		}
	}

	void eventStartLoadingMesh( object obj )
	{
		loadingObjects = new List<LoadObject>();
		currentlyLoadingNewMeshes = true;
		loadingEffectActive = true;
	}

	// Called when a new mesh-part has been fully loaded.
	// Resets the shader loading animation for the parent of this mesh:
	void eventFinishLoadingMesh( object obj )
	{
		GameObject gameObject = obj as GameObject;
		if (gameObject != null) {

			LoadObject loadObject = new LoadObject ();
			loadObject.gameObject = gameObject;
			loadObject.amount = 0.0f;
			loadingObjects.Add (loadObject);
			/* GameObject parentObject = gameObject.transform.parent.gameObject;
			if( parentObject != null )
			{
				LoadObject loadObject = null;
				// If the object already exists, reset its animation:
				foreach (LoadObject lObj in loadingObjects) {
					if( lObj.gameObject == parentObject )
					{
						loadObject = lObj;
						loadObject.amount = 0.0f;
						break;
					}
				}
				// If the object does not yet exist, add an entry:
				if (loadObject == null) {
					loadObject = new LoadObject ();
					loadObject.gameObject = parentObject;
					loadObject.amount = 0.0f;
					loadingObjects.Add (loadObject);
				}
			}*/
		}
	}
	void eventFinishLoadingAllMeshes( object obj )
	{
		currentlyLoadingNewMeshes = false;
	}

	void eventPatientClosed( object obj )
	{
		loadingObjects.Clear ();
		loadingEffectActive = false;
		currentlyLoadingNewMeshes = false;
	}
}
