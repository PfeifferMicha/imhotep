using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PatientBriefing : MonoBehaviour {

	public GameObject textObj;
	private Text text;

	// Use this for initialization
	void OnEnable () {
		text = textObj.GetComponent<Text> ();
		string msg = bold( "Patient Information" );
		msg += "\n\nNo patient loaded.";
		text.text = msg;
			
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Loaded, eventNewPatientLoaded );

		Patient loadedPatient = Patient.getLoadedPatient ();
		if (loadedPatient != null) {
			setPatientText( loadedPatient );
		}

	}

	void eventNewPatientLoaded( object obj )
	{
		Patient p = obj as Patient;
		if (p != null) {
			setPatientText (p);
		}
	}

	private void setPatientText( Patient p ) {
		string msg = bold( "Patient Information" );
		msg += "\n\n";

		msg += bold ("Patient Name: ") + p.name + "\n";
		msg += bold ("Date of Birth: ") + p.birthDate + "\n";
		msg += bold ("Date of Operation: ") + p.operationDate + "\n";

		msg += "\n";

		msg += p.getAdditionalInfo() + "\n";

		text.text = msg;
	}

	private string bold( string input )
	{
		return "<b>" + input + "</b>";
	}
}
