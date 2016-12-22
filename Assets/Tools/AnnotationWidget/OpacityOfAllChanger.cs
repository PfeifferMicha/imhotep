using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OpacityOfAllChanger : MonoBehaviour {

	private List<GameObject> organs = new List<GameObject>();


	void OnEnable () {
		// Register event callbacks:
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_FinishedLoading, openPatient);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, closePatient);
	}

	void OnDisable () {
		// Unregister myself:
		PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_FinishedLoading, openPatient);
		PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_Closed, closePatient);
	}

	public void addOrgan (GameObject newOrgan) {
		organs.Add (newOrgan);
	}

	public void changeOpacityofAll (float opacity) {
		foreach(GameObject o in organs) {
			o.GetComponent<MeshMaterialControl> ().changeOpactiyOfChildren (opacity);
		}
	}

	public void closePatient(object obj = null) {
		organs = new List<GameObject> ();
	}

	public void openPatient(object obj = null) {
		foreach(Transform child in transform) {
			if(child.gameObject.GetComponent<MeshMaterialControl>() != null) {
				organs.Add (child.gameObject);
			}
		}
	}
}
