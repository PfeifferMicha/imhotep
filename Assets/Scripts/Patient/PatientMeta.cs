using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

public class PatientMeta
{
	public PatientMeta ( string folder )
	{
		path = folder;
		Debug.Log (path);

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
			}
			if (metaData.Keys.Contains ("DateOfOperation")) {
				operationDate = metaData ["DateOfOperation"].ToString ();
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

	public string firstName { get; private set; }
	public string lastName { get; private set; }
	public string name { 
		get { return this.firstName + " " + this.lastName; }
		private set { }
	}
	public string birthDate { get; private set; }
	public string operationDate { get; private set; }
	public string path { get; private set; }

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

