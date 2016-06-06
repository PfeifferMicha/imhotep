using UnityEngine;
using System.Collections;

public class HideOnPatient : MonoBehaviour {

	// Use this for initialization
	void Start () {

		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading, show);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_Closed, hide);
	}
	
	public void show( object obj = null )
	{
		GetComponent<MeshRenderer> ().enabled = false;
	}
	public void hide( object obj = null )
	{
		GetComponent<MeshRenderer> ().enabled = true;
	}
}
