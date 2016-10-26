using UnityEngine;
using System.Collections;
using itk.simple;

public class SegmentationTest : MonoBehaviour {
	void OnEnable() {
		PatientEventSystem.startListening (PatientEventSystem.Event.DICOM_NewLoadedVolume, OnDICOMLoaded );

		// Start loading the first volume, if possible:
		PatientDICOMLoader.instance.loadDicom( 0 );
	}

	void OnDisable() {
		PatientEventSystem.stopListening (PatientEventSystem.Event.DICOM_NewLoadedVolume, OnDICOMLoaded );
	}

	public void OnDICOMLoaded( object obj = null )
	{
		DICOMVolume dicom = obj as DICOMVolume;
		if (dicom == null)
			return;

		DICOMHeader header = dicom.getHeader ();

		Debug.Log ("DICOM Volume loaded for: " + header.getPatientName ());

		Image volume = dicom.getImage ();
		Debug.Log ("Width: " + volume.GetWidth () + ", height: " + volume.GetHeight () + ", depth: " + volume.GetDepth ());

		VectorUInt32 position = new VectorUInt32 {
			volume.GetWidth() / 2,
			volume.GetHeight() / 2,
			volume.GetDepth() / 2
		};
		// Asumes that the pixel type stored in the image is grayscale int32:
		int value = volume.GetPixelAsInt32 (position);
		Debug.Log ("Value of center pixel: " + value);
	}
}
