using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ModelEffectHandler : MonoBehaviour {

	private bool loadingEffectActive = false;
	private bool currentlyLoadingNewMeshes = false;
	private bool cutMesh = true;

	public GameObject cameraObject;
	public GameObject shaderCuttingPlane;

	class LoadObject
	{
		public GameObject gameObject;
		public float amount;
	};

	List<LoadObject> loadingObjects = new List<LoadObject>();
	List<Material> loadedMaterials = new List<Material>();

	void Start () {
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, eventStartLoadingMesh);
		PatientEventSystem.startListening( PatientEventSystem.Event.MESH_LoadedSingle, eventFinishLoadingMesh );
		PatientEventSystem.startListening( PatientEventSystem.Event.MESH_LoadedAll, eventFinishLoadingAllMeshes );
	}

	void Update () {
		
		if (loadingEffectActive) {
			bool allMeshesFinishedAnimation = true;
			foreach (LoadObject lObj in loadingObjects) {
				lObj.amount += Time.deltaTime*0.5f;

				float amount;
				if (currentlyLoadingNewMeshes) {
					amount = lObj.amount - (float)Math.Floor (lObj.amount);
				} else {
					amount = lObj.amount;
				}
				if (amount < 1.0) {
					allMeshesFinishedAnimation = false;
				}

				foreach (Transform child in lObj.gameObject.transform) {
					Material mat = child.gameObject.GetComponent<Renderer> ().material;
					mat.SetFloat ("_amount", amount);
				}
			}
			if (!currentlyLoadingNewMeshes && allMeshesFinishedAnimation) {
				loadingEffectActive = false;
				foreach (LoadObject lObj in loadingObjects) {
					foreach (Transform child in lObj.gameObject.transform) {
						Material mat = child.gameObject.GetComponent<Renderer> ().material;
						mat.SetFloat ("_amount", 1.5f);
					}
				}
			}
		}

		// Update the distance of the shaders to the distance between the camera and the camera cutting plane:
		if (cutMesh) {
			foreach (Material mat in loadedMaterials) {
				float distance = Vector3.Distance (cameraObject.transform.position, shaderCuttingPlane.transform.position);
				mat.SetFloat ("_cuttingPlaneDistToCamera", distance);
			}
		} else {
			foreach (Material mat in loadedMaterials) {
				mat.SetFloat ("_cuttingPlaneDistToCamera", 9999);		// High number: Show everything, no clipping!
			}
		}
	}

	void eventStartLoadingMesh( object obj )
	{
		loadingObjects = new List<LoadObject>();
		loadedMaterials = new List<Material>();
		currentlyLoadingNewMeshes = true;
		loadingEffectActive = true;
	}

	// Called when a new mesh-part has been fully loaded.
	// Resets the shader loading animation for the parent of this mesh:
	void eventFinishLoadingMesh( object obj )
	{
		GameObject gameObject = obj as GameObject;
		if (gameObject != null) {

			GameObject parentObject = gameObject.transform.parent.gameObject;
			if( parentObject )
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


				Material newMaterial = gameObject.GetComponent<Renderer> ().material;
				bool found = false;
				// If the material is not yet in the list, add it:
				foreach (Material mat in loadedMaterials) {
					if( mat == newMaterial )
					{
						found = true;
						break;
					}
				}
				if (!found) {
					loadedMaterials.Add( newMaterial );
				}
			}
		}
	}
	void eventFinishLoadingAllMeshes( object obj )
	{
		currentlyLoadingNewMeshes = false;
	}
}
