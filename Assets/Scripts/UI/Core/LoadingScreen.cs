using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour {

	public GameObject LoadingScreenWidget;
	public GameObject TextPatientName;
	public GameObject TextLoadingProcess;
	private Text mTextPatientName;
	private Text mTextLoadingProcess;

	// Use this for initialization
	void Start () {
		// Get the text components to be modified later:
		mTextLoadingProcess = TextLoadingProcess.GetComponent<Text> ();
		mTextPatientName = TextPatientName.GetComponent<Text> ();

		// Start listening to events which are called during the loading process:
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_StartLoading,
			loadingStarted);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_FinishLoading,
			loadingFinished);
		PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_LoadingProcess,
			newProcessInfo);
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

	// Called when all parts of the patient are done loading:
	void loadingFinished( object obj = null )
	{
		LoadingScreenWidget.SetActive (false);
	}

	// Called when some new event happend that should be displayed on the loading screen:
	void newProcessInfo( object obj = null )
	{
		string msg = obj as string;
		if (msg != null) {
			mTextLoadingProcess.text += msg;
		}

	}

}
