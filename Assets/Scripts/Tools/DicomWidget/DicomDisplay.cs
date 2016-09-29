using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DicomDisplay : MonoBehaviour {

	private DicomDisplayImage mDicomImage;
	private Dropdown mDicomList;

	private Text mStatusText;

	void Awake()
	{
		mDicomList = transform.Find ("Canvas/DicomList").GetComponent<Dropdown>();
		mDicomImage = transform.Find ("Canvas/DicomImage").gameObject.GetComponent<DicomDisplayImage>();

		GameObject go = GameObject.Find ("StatusText");
		if (go != null) {
			mStatusText = go.GetComponent<Text> ();
			mStatusText.text = "Searching for DICOMs ...";
		}
	}

	void Start()
	{
		// Register event callbacks for all DICOM events:
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewList, eventNewDicomList );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_StartLoading, eventHideDICOM );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewLoaded, eventDisplayCurrentDicom );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_AllCleared, eventClear );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, eventClear );
		eventDisplayCurrentDicom ();
		eventNewDicomList ();
	}

	void OnDisable()
	{
		// Unregister myself - no longer receive events (until the next OnEnable() call):
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewList, eventNewDicomList );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_StartLoading, eventHideDICOM );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewLoaded, eventDisplayCurrentDicom );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_AllCleared, eventClear );
		PatientEventSystem.stopListening( PatientEventSystem.Event.PATIENT_Closed, eventClear );
	}

	// Called when a new DICOM was loaded:
	void eventDisplayCurrentDicom( object obj = null )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();        
        DICOM dicom = mPatientDICOMLoader.getCurrentDicom();
		if( dicom != null )
		{
			mDicomImage.gameObject.SetActive (true);
			mDicomImage.SetDicom (dicom);
		}
		mStatusText.gameObject.SetActive (false);
	}

	void eventNewDicomList( object obj = null )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
        mDicomList.ClearOptions ();
		List<string> seriesUIDs = mPatientDICOMLoader.getAvailableSeries ();
		List<string> customNames = new List<string> ();
		Patient p = Patient.getLoadedPatient ();
		if( p != null )
		{
			foreach (string uid in seriesUIDs) {
				customNames.Add (p.getDICOMNameForSeriesUID (uid));
			}
		}
		mDicomList.AddOptions ( customNames );
		if (customNames.Count == 0) {
			mStatusText.gameObject.SetActive (true);
			mStatusText.text = "No DICOM series found.";
		}
	}
	void eventClear( object obj = null )
	{
		mDicomList.ClearOptions ();
		mDicomImage.clear ();
	}
	void eventHideDICOM( object obj = null )
	{
		mDicomImage.gameObject.SetActive (false);
	}
	public void selectedNewDicom( int id )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
		mPatientDICOMLoader.loadDicom ( id );

		mStatusText.gameObject.SetActive (true);
		mStatusText.text = "Loading DICOM ...";
	}

}
