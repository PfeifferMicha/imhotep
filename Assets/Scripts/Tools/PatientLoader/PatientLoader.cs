using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

public class PatientLoader {

    private string mPath;
    private List<PatientMeta> mPatientEntries;

    public PatientLoader()
    {
        mPath = "";
        mPatientEntries = new List<PatientMeta>();
    }

    public void setPath(string newPath)
    {
        Debug.Log( "Current working directory:\n" + Directory.GetCurrentDirectory() );
        if (!Directory.Exists(newPath))
            throw (new System.Exception("Invalid path given: " + newPath));

        mPath = newPath;

        parsePath();
    }

    public int getCount()
    {
        return mPatientEntries.Count;
    }

	public PatientMeta getEntry( int index )
    {
        if (index >= 0 && index < mPatientEntries.Count)
            return mPatientEntries[index];

        throw (new System.Exception("Could not find entry with index " + index.ToString()));
    }

    private void parsePath()
    {
		Debug.Log ("Looking for Patients in:\n" + mPath);

		string[] folders = Directory.GetDirectories (mPath);

		foreach( string folder in folders )
		{
			PatientMeta newPatient = PatientMeta.createFromFolder (folder);
			if (newPatient != null) {
				Debug.Log (newPatient.ToString ());
				mPatientEntries.Add(newPatient);
			}
		}

        /*string patientsDirectoryFile = Path.Combine(mPath, "Patients.json");
        if (File.Exists(patientsDirectoryFile))
        {
            msg = msg + "\tFound Patients.json.";
        } else {
            msg = msg + "\tNo Patients.json found - invalid Patients folder.";
            return;
        }

        mPatientEntries.Clear();
        
		string raw = File.ReadAllText(patientsDirectoryFile);

        JsonData data = JsonMapper.ToObject(raw);

        if (data.Keys.Contains("Patients"))
        {
            JsonData patientArray = data["Patients"];
            if( patientArray.IsArray )
            {
                for( int i = 0; i < patientArray.Count; i++ )
                {
                    JsonData patient = patientArray[i];

                    if (patient.Keys.Contains("Path"))
					{
                        string subfolder = patient["Path"].ToString();
                        string patientFolder = Path.Combine(mPath, subfolder);
                        string patientMetaFile = Path.Combine( patientFolder, "meta.json" );

                        if (Directory.Exists(patientFolder) && File.Exists(patientMetaFile))
                        {
                            PatientEntry patientEntry;
                            patientEntry.name = "Anonymous";
                            patientEntry.operationDate = "?";
                            patientEntry.birthDate = "?";
                            patientEntry.path = subfolder;
                            try {
                                patientEntry.name = patient["Name"].ToString();
                                patientEntry.birthDate = patient["Date of Birth"].ToString();
                                patientEntry.operationDate = patient["Date of Operation"].ToString();
                                patientEntry.path = patientFolder;      // save the absolute path
                            } catch(System.Exception e) {
                                Debug.LogWarning(e.ToString());
                            }
                            mPatientEntries.Add(patientEntry);
                        } else
                        {
                            Debug.LogWarning("Patient found which points to invalid folder or is missing meta.json file.");
                        }
                    }
                }
            }
        }*/
    }

    public Patient loadPatient( int index )
    {
        if (index >= 0 && index < mPatientEntries.Count)
        {
			PatientMeta entry = mPatientEntries[index];
			Patient p = new Patient (entry);
            return p;
        }
        else
        {
            throw (new System.Exception("Could not find patient with index " + index.ToString()));
        }
    }

}
