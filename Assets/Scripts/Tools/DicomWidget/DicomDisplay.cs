using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class DicomDisplay : MonoBehaviour {

    private RawImage mDicomImage;
	private Dropdown mDicomList;

	void Awake()
	{
		mDicomList = transform.Find ("Canvas/DicomList").GetComponent<Dropdown>();
		mDicomImage = transform.Find ("Canvas/DicomImage").GetComponent<RawImage>();
		mDicomImage.material = new Material (mDicomImage.material);
		mDicomImage.material.mainTexture = new Texture3D (4,4,4, TextureFormat.RGBA32, false);
	}

	void OnEnable()
	{
		// Register event callbacks for all DICOM events:
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewList, eventNewDicomList );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewLoaded, eventDisplayCurrentDicom );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_AllCleared, eventClear );
		eventDisplayCurrentDicom ();
		eventNewDicomList ();
	}

	void OnDisable()
	{
		// Unregister myself - no longer receives events (until the next OnEnable() call):
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
			mDicomImage.material.mainTexture = dicom.getTexture();
			mDicomImage.material.SetFloat ("globalMaximum", (float)dicom.getMaximum ());
			mDicomImage.material.SetFloat ("globalMinimum", (float)dicom.getMinimum ());
			mDicomImage.material.SetFloat ("range", (float)(dicom.getMaximum () - dicom.getMinimum ()));
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
	}
	public void selectedNewDicom( int id )
	{
        PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
        Debug.Log (id);
		mPatientDICOMLoader.loadDicom ( id );
	}

}
