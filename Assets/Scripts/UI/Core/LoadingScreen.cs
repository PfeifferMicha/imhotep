using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreen : MonoBehaviour {

	public GameObject LoadingScreenWidget;
	public GameObject TextPatientName;
	public GameObject TextLoadingProcess;
	private Text mTextPatientName;
	private Text mTextLoadingProcess;
	private List<string> activeJobs = new List<string>();

	// Use this for initialization
	void Start () {
		// Get the text components to be modified later:
		mTextLoadingProcess = TextLoadingProcess.GetComponent<Text> ();
		mTextPatientName = TextPatientName.GetComponent<Text> ();

		// Start listening to events which are called during the loading process:
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading,
			loadingStarted);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_LoadingProcess,
			newProcessInfo);
		PatientEventSystem.startListening (PatientEventSystem.Event.LOADING_AddLoadingJob,
			addLoadingJob);
		PatientEventSystem.startListening (PatientEventSystem.Event.LOADING_RemoveLoadingJob,
			removeLoadingJob);
	}

	// Called when a new patient is being loaded:
	void loadingStarted( object obj )
	{
		// The passed object should be the patient entry of the patient to be loaded:
		PatientMeta patientEntry = obj as PatientMeta;
		if (patientEntry != null) {
			mTextPatientName.text = patientEntry.name;
		}

		mTextLoadingProcess.text = "Started Loading\n";
		LoadingScreenWidget.SetActive (true);
	}

	// Called when some new event happend that should be displayed on the loading screen:
	void newProcessInfo( object obj = null )
	{
		string msg = obj as string;
		if (msg != null) {
			mTextLoadingProcess.text += msg;
		}
	}

	void addLoadingJob( object obj )
	{
		if (!LoadingScreenWidget.active)
			return;
			
		string msg = obj as string;
		if (msg != null) {
			Debug.Log ("Adding: " + msg);
			activeJobs.Add (msg);
		}
	}
	void removeLoadingJob( object obj )
	{
		if (!LoadingScreenWidget.active)
			return;
		
		string msg = obj as string;
		if (msg != null) {
			Debug.Log ("Removing: " + msg);
			if (activeJobs.Contains (msg)) {
				activeJobs.Remove (msg);
			}
		}

		// If all jobs have finished, close loading screen:
		if (activeJobs.Count <= 0) {
			LoadingScreenWidget.SetActive (false);
		}
	}

}
