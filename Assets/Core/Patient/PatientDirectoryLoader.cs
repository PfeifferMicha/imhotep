using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

public class PatientDirectoryLoader {

	private static string currentPath = "";
	private static List<PatientMeta> mPatientEntries = new List<PatientMeta>();
	private static bool currentlyLoading = false;

    //Variable used for thread
    private static PatientMeta entry;
    private static bool loadingLock = false;


    static PatientDirectoryLoader()
    {
        PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Loaded, loadPatientFinish);
    }

    ~PatientDirectoryLoader()
    {
        PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Loaded, loadPatientFinish);
    }

    /*! Starts processing the new path, by searching the subfolders for patients. */
    public static void setPath(string newPath)
    {
        //Debug.Log( "Current working directory:\n" + Directory.GetCurrentDirectory() );

        if (!Directory.Exists(newPath))
            throw (new System.Exception("Invalid path given: " + newPath));

		// While parsing a directory, don't start parsing another one:
		if (!currentlyLoading) {
			currentlyLoading = true;

			// remove previously found patient entries:
			mPatientEntries.Clear ();

			currentPath = newPath;

			Debug.Log ("Looking for Patients in:\n" + currentPath);

			string[] folders = Directory.GetDirectories (currentPath);

			foreach (string folder in folders) {
				// Attempt to load the directorie's contents as a patient:
				PatientMeta newPatient = PatientMeta.createFromFolder (folder);
				if (newPatient != null) {
					mPatientEntries.Add (newPatient);

					// Let listeners know there's a new patient entry by firing an event:
					PatientEventSystem.triggerEvent (
						PatientEventSystem.Event.PATIENT_NewPatientDirectoryFound
					);
				}
			}

			// Done parsing, unlock:
			currentlyLoading = false;
		}
    }

    public static int getCount()
    {
        return mPatientEntries.Count;
    }

	public static PatientMeta getEntry( int index )
    {
        if (index >= 0 && index < mPatientEntries.Count)
            return mPatientEntries[index];

        throw (new System.Exception("Could not find entry with index " + index.ToString()));
    }

    public static void loadPatient( int index )
    {
        if (loadingLock)
        {
            Debug.LogWarning("[PatientDirectoryLoader.cs] Loading aborted. Still loading other patient");
            return;
        }

        loadingLock = true;
        if (index >= 0 && index < mPatientEntries.Count)
		{
			entry = mPatientEntries[index];

			PatientEventSystem.triggerEvent (PatientEventSystem.Event.PATIENT_StartLoading, entry);

			Patient p = new Patient (entry);

        }
        else
        {
            throw (new System.Exception("Could not find patient with index " + index.ToString()));
        }
    }

    /*
     * loadPatient() starts the loading of a patient. The event Event.PATIENT_Loaded calls this methode and the loading will be finished
     */
    private static void loadPatientFinish(object obj = null)
    {
        // Start parsing the DICOM directory:
        //PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
        //mPatientDICOMLoader.loadDirectory(entry.dicomPath);

		DICOMLoader.instance.setDirectory (entry.dicomPath);

        // Load model in the directory:
        MeshLoader mModelLoader = GameObject.Find("GlobalScript").GetComponent<MeshLoader>();
        mModelLoader.LoadFile(entry.meshPath);      
        loadingLock = false;
    }

    private static PatientDirectoryLoader mInstance;

}
