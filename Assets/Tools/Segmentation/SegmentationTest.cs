using UnityEngine;
using System.Collections;
using itk.simple;

public class SegmentationTest : MonoBehaviour {
	void OnEnable() {
		PatientEventSystem.startListening (PatientEventSystem.Event.DICOM_NewLoadedVolume, OnDICOMLoaded );
	}

	void OnDisable() {
		PatientEventSystem.stopListening (PatientEventSystem.Event.DICOM_NewLoadedVolume, OnDICOMLoaded );
	}

	public void OnDICOMLoaded( object obj = null )
	{
		DICOM dicom = obj as DICOM;
		if (dicom == null)
			return;

		Image volume = dicom.image;
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
