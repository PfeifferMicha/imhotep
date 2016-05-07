using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*! Loading Screen control.
 * This controls the LoadingScreenWidget and sets the texts on the LoadingScreenWidget.
 * Texts and the Widget must be assigned in the editor.
 * This class reactis to events in the PatientEventSystem.
 * Activate the Loading Screen by triggering "Event.PATIENT_StartLoading".
 * Then add jobs by using Event.LOADING_AddLoadingJob.
 * Each of these should later be followed by an Event.LOADING_RemoveLoadingJob (with the
 * same name passed as argument!)
 * Once all jobs have been removed, the loading screen automatically disappears.
 */
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

	void addLoadingJob( object obj )
	{
		if (!LoadingScreenWidget.activeSelf)
			return;
			
		string msg = obj as string;
		if (msg != null) {
			Debug.Log ("Adding: " + msg);
			activeJobs.Add (msg);
		}
		updateInfo ();
	}
	void removeLoadingJob( object obj )
	{
		if (!LoadingScreenWidget.activeSelf)
			return;
		
		string msg = obj as string;
		if (msg != null) {
			Debug.Log ("Removing: " + msg);
			if (activeJobs.Contains (msg)) {
				activeJobs.Remove (msg);
			}
		}
		updateInfo ();

		// If all jobs have finished, close loading screen:
		if (activeJobs.Count <= 0) {
			LoadingScreenWidget.SetActive (false);
		}
	}
	void updateInfo ()
	{
		string info = "";
		foreach( string s in activeJobs )
		{
			info += s + ": Loading\n";
		}
		mTextLoadingProcess.text = info;
	}

}
