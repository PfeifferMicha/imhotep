using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

public class Patient
{
    public Patient( string directory )
    {
        Debug.Log("Path: " + directory);
        //mDicomLoader = new DicomLoader(directory + "/DICOM" );
		//DicomLoaderITK loader = new DicomLoaderITK();
		//loader.loadDirectory (directory + "/DICOM");

		mModelLoader = GameObject.Find ("GlobalScript").GetComponent<Loader> ();

		if (Directory.Exists (directory + "/Models")) {
			string [] modelFiles = Directory.GetFiles (directory + "/Models");
			foreach (string fileName in modelFiles) {
				mModelLoader.LoadFile (fileName);
			}
		}

    }
    
    private DicomLoader mDicomLoader;
    //List<Dicom> mLoadedDicoms;

	Loader mModelLoader;
}
