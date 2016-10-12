using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

public class PatientMeta
{
	public string firstName { get; private set; }
	public string lastName { get; private set; }
	public string name { 
		get { return this.firstName + " " + this.lastName; }
		private set { }
	}
	public string birthDate { get; private set; }
	public DateTime birthDateDT { get; private set; }
	public string operationDate { get; private set; }
	public string indication { get; private set; }
	public string details { get; private set; }
	public string sex { get; private set; }
	public string path { get; private set; }
	public string dicomPath { get; private set; }
	public string meshPath { get; private set; }
	public int age { get; private set; }

	public PatientMeta ( string folder )
	{
		path = folder;

		string metaFile = Path.Combine( path, "meta.json" );
		string raw = File.ReadAllText(metaFile);
		JsonData data;
		try{
			data = JsonMapper.ToObject(raw);
		} catch {
			throw new System.Exception("Cannot parse meta.json. Invalid syntax?");
		}

		if (data.Keys.Contains ("meta")) {
			JsonData metaData = data["meta"];
			if (metaData.Keys.Contains ("FirstName")) {
				firstName = metaData ["FirstName"].ToString ();
			}
			if (metaData.Keys.Contains ("LastName")) {
				lastName = metaData ["LastName"].ToString ();
			}
			if (metaData.Keys.Contains ("DateOfBirth")) {
				birthDate = metaData ["DateOfBirth"].ToString ();

				try {
					IFormatProvider culture = System.Threading.Thread.CurrentThread.CurrentCulture;
					DateTime dt = DateTime.Parse(birthDate, culture, System.Globalization.DateTimeStyles.AssumeLocal);
					age = DateTime.Now.Year - dt.Year;
					birthDate = dt.Day + " " + dt.ToString("MMMM") + " " + dt.Year;
				} catch (System.Exception ex) {
					age = -1;
				}
			}
			if (metaData.Keys.Contains ("Indication")) {
				indication = metaData ["Indication"].ToString ();
			}
			if (metaData.Keys.Contains ("Details")) {
				details = metaData ["Details"].ToString ();
			}
			if (metaData.Keys.Contains ("DateOfOperation")) {
				operationDate = metaData ["DateOfOperation"].ToString ();
			}
			if (metaData.Keys.Contains ("Sex")) {
				sex = metaData ["Sex"].ToString ();
			}
		}

        if (data.Keys.Contains("mesh"))
        {
            JsonData metaData = data["mesh"];
            if (metaData.Keys.Contains("path"))
            {
                meshPath = path + "/" + metaData["path"].ToString();
            }
        }

        if (data.Keys.Contains("dicom"))
        {
            JsonData metaData = data["dicom"];
            if (metaData.Keys.Contains("path"))
            {
                dicomPath = path + "/" + metaData["path"].ToString(); ;
            }
        }
	}

	// Copy constructor:
	public PatientMeta( PatientMeta toCopy )
	{
		firstName = toCopy.firstName;
		lastName = toCopy.lastName;
		birthDate = toCopy.birthDate;
		operationDate = toCopy.operationDate;
		path = toCopy.path;
	}


	public static PatientMeta createFromFolder( string folder )
	{
		string patientsDirectoryFile = Path.Combine( folder, "meta.json" );
		if (File.Exists(patientsDirectoryFile))
		{
			return new PatientMeta (folder);
		} else {
			Debug.LogWarning ("No meta.json found in\n\t" + folder + "\n\tInvalid patient folder.");
			return null;
		}
	}

	public override string ToString()
	{
		return base.ToString () + " (Name: " + name + ")";
	}
}

