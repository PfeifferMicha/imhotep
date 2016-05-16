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

	// Use this for initialization
	void Start () {
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, eventStartLoadingMesh);
		PatientEventSystem.startListening( PatientEventSystem.Event.MESH_LoadedSingle, eventFinishLoadingMesh );
		PatientEventSystem.startListening( PatientEventSystem.Event.MESH_LoadedAll, eventFinishLoadingAllMeshes );
	}

	// Update is called once per frame
	void Update () {
		
		if (loadingEffectActive) {
			bool allMeshesFinishedAnimation = true;
			foreach (LoadObject obj in loadingObjects) {
				obj.amount += Time.deltaTime;
				Material mat = obj.gameObject.GetComponent<Renderer> ().material;

				float amount;
				if (currentlyLoadingNewMeshes) {
					amount = obj.amount - (float)Math.Floor (obj.amount);
				} else {
					amount = obj.amount;
				}
				if (amount < 1.0) {
					allMeshesFinishedAnimation = false;
				}
				mat.SetFloat ("_amount", amount);
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

	void eventFinishLoadingMesh( object obj )
	{
		GameObject gameObject = obj as GameObject;
		if (gameObject != null) {

			LoadObject newObj = new LoadObject ();
			newObj.gameObject = gameObject;
			newObj.amount = 0.0f;
			loadingObjects.Add (newObj);
		}
	}
	void eventFinishLoadingAllMeshes( object obj )
	{
		currentlyLoadingNewMeshes = false;
	}
}
