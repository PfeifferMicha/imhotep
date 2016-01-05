using UnityEngine;
using System.Collections;

public class DICOMLoaderDummy : MonoBehaviour {
	
	public string path;

	// Use this for initialization
	void Start () {

		// DEBUG:
		DicomLoaderITK dl = new DicomLoaderITK ();
		dl.load ( path );
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
