using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PatientBriefing : MonoBehaviour {

	public GameObject textObj;
	public GameObject tabButton;
	private Text text;

	// Use this for initialization
	void OnEnable () {
		tabButton.SetActive (false);

		text = textObj.GetComponent<Text> ();
		string msg = bold( "Patient Information" );
		msg += "\n\nNo patient loaded.";
		text.text = msg;

		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Loaded, eventNewPatientLoaded );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, eventPatientClosed );

		Patient loadedPatient = Patient.getLoadedPatient ();
		Debug.Log ("loaded patient:" + loadedPatient);
		if (loadedPatient != null) {
			eventNewPatientLoaded (loadedPatient);
		} else {
			eventPatientClosed ();
		}
	}

	void eventNewPatientLoaded( object obj )
	{
		Patient patient = obj as Patient;
		if (patient != null) {
			
			// Create Tabs according to the loaded tab names:
			List<string> tabNames = patient.getAdditionalInfoTabs();
			for (int i = 0; i < tabNames.Count; i++) {
				GameObject newButton = GameObject.Instantiate (tabButton);
				newButton.SetActive (true);
				newButton.name = tabNames [i];
				newButton.transform.FindChild ("Text").GetComponent<Text> ().text = tabNames [i];
				newButton.transform.SetParent (tabButton.transform.parent, false);

				string capturedTabName = tabNames [i];	// might not be necessary to capture, but just in case the list changes?
				Button b = newButton.GetComponent<Button>();
				b.onClick.AddListener(() => setPatientText( capturedTabName ));
			}

			setPatientText ("General");
		}
	}

	private void setPatientText( string tabName ) {

		Patient loadedPatient = Patient.getLoadedPatient ();
		if (loadedPatient != null) {
			Debug.Log ("Loading tab: " + tabName);
		}

		text.text = loadedPatient.getAdditionalInfo (tabName);
	}



	void eventPatientClosed( object obj = null )
	{
		string msg = bold( "Patient Information" );
		msg += "\n\n";

		msg += bold ("Patient Name: ") + "No patient loaded.";

		text.text = msg;
	}

	private string bold( string input )
	{
		return "<b>" + input + "</b>";
	}
}
