using UnityEngine;
using System.Collections;
using System.IO;

public class QuickLoad : MonoBehaviour {

	public string patientFolderName;
	public ToolWidget toolToLoad;
	public GameObject sphere;
	public GameObject sphereEmitters;
	public GameObject patientSelector;

	// Use this for initialization
	void Start () {

		PatientEventSystem.startListening (
			PatientEventSystem.Event.PATIENT_NewPatientDirectoryFound,
			addPatientEntry
		);

		PatientEventSystem.startListening (
			PatientEventSystem.Event.PATIENT_FinishedLoading,
			loadedPatient
		);

		// Start loading the patient directory:
		PatientDirectoryLoader.setPath("../Patients/");

		// Activate all the objects:
		sphere.SetActive (true);
		sphereEmitters.SetActive (true);

		// Start the animations of the objects, and set their normalized time to 1 (the end)
		sphereEmitters.GetComponent<Animator> ().Play ("EnableSphereEmitters", -1, 1f);
		sphere.GetComponent<Animator> ().Play ("EnableSphere", -1, 1f);
	}

	void addPatientEntry( object obj = null )
	{
		bool found = false;
		for (int index = 0; index < PatientDirectoryLoader.getCount (); index++) {
			PatientMeta patient = PatientDirectoryLoader.getEntry(index);
			string folderName = Path.GetFileName (patient.path);
			if (folderName == patientFolderName) {
				Debug.Log ("Found patient " + patientFolderName);
				PatientDirectoryLoader.loadPatient(index);
				found = true;
				break;
			}
		}
		if( !found )
		{
			Debug.LogWarning ("Patient '" + patientFolderName + "' not found! Make sure to enter valid Patient folder in 'Patient Folder Name'!");
			patientSelector.SetActive (true);
		}
	}

	void loadedPatient( object obj = null )
	{
		if( toolToLoad != null )
			ToolControl.instance.chooseTool (toolToLoad);
	}
}
