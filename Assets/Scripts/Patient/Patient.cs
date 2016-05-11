﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using itk.simple;

public class Patient : PatientMeta
{
	static private Patient loadedPatient = null;

	public Patient( PatientMeta meta ) : base(meta)
    {

        /*
        Debug.Log("Path: " + directory);

		Loader mModelLoader = GameObject.Find ("GlobalScript").GetComponent<Loader> ();
		// Load all models in the directory:
		if (Directory.Exists (directory + "/Models")) {
			string [] modelFiles = Directory.GetFiles (directory + "/Models");
			foreach (string fileName in modelFiles) {
				mModelLoader.LoadFile (fileName);
			}
		}
			
		DicomCache dicomCache = DicomCache.instance;
		dicomCache.loadDirectory (directory + "/DICOM");
        */

		loadedPatient = this;
    }

	public static Patient getLoadedPatient()
	{
		return loadedPatient;
	}
}
