using UnityEngine;
using System.Collections;

public class StartScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //Recenter oculus rift
        //UnityEngine.VR.InputTracking.Recenter(); //TODO uncomment in release version

        //Hide mouse curser
        //UnityEngine.Cursor.visible = false; //TODO uncomment in release version


		// DEBUG:
		DicomLoaderITK dl = new DicomLoaderITK ();
		dl.load ("../Patients/Patient1/DICOM/");
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
