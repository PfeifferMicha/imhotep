using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class DicomDisplay : MonoBehaviour {

	private DicomDisplayImage mDicomImage;
	private Dropdown mDicomList;
	void Awake()
	{
		mDicomList = transform.Find ("Canvas/DicomList").GetComponent<Dropdown>();
		mDicomImage = transform.Find ("Canvas/DicomImage").gameObject.GetComponent<DicomDisplayImage>();
	}

	void Start()
	{
		// Register event callbacks for all DICOM events:
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewList, eventNewDicomList );
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
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewLoaded, eventDisplayCurrentDicom );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_AllCleared, eventClear );
	}

	// Called when a new DICOM was loaded:
	void eventDisplayCurrentDicom( object obj = null )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();        
        DICOM dicom = mPatientDICOMLoader.getCurrentDicom();
		if( dicom != null )
		{
			mDicomImage.SetDicom (dicom);
		}
	}

	void eventNewDicomList( object obj = null )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
        mDicomList.ClearOptions ();
		mDicomList.AddOptions (mPatientDICOMLoader.getAvailableSeries());
	}
	void eventClear( object obj = null )
	{
		mDicomList.ClearOptions ();
		mDicomImage.clear ();
	}
	public void selectedNewDicom( int id )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
        Debug.Log (id);
		mPatientDICOMLoader.loadDicom ( id );
	}

}
