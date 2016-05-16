using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ModelLoadEffectHandler : MonoBehaviour {

	private bool loadingEffectActive = false;
	private bool currentlyLoadingNewMeshes = false;

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
			}
		}
	}
	void eventFinishLoadingAllMeshes( object obj )
	{
		currentlyLoadingNewMeshes = false;
	}
}
