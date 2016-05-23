using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using itk.simple;
using LitJson;

public class Patient : PatientMeta
{
	static private Patient loadedPatient = null;

	private List<AdditionalInformation> additionalInformation = new List<AdditionalInformation> ();

	public class AdditionalInformation
	{
		public string name { get; set; }
		public string content { get; set; }
	}

	public Patient( PatientMeta meta ) : base(meta)
    {
		string metaFile = Path.Combine( base.path, "meta.json" );
		string raw = File.ReadAllText(metaFile);
		JsonData data;
		try{
			data = JsonMapper.ToObject(raw);
		} catch {
			throw new System.Exception("Cannot parse meta.json. Invalid syntax?");
		}

		if (data.Keys.Contains("additional information"))
		{
			JsonData infoArray = data ["additional information"];

			//foreach (JsonData elem in infoArray)
			for (int i = 0; i < infoArray.Count; i++) {
				JsonData entry = infoArray [i];
				if (entry.Keys.Contains ("Name") && entry.Keys.Contains ("File")) {

					if( System.IO.File.Exists( base.path + "/" + entry ["File"] ) )
					{
						string content = System.IO.File.ReadAllText ( base.path + "/" + entry ["File"]);
						string name = entry ["Name"].ToString();
						if (name.Length > 0 && content.Length > 0) {
							AdditionalInformation info = new AdditionalInformation {
								name = name,
								content = content
							};
							additionalInformation.Add (info);
						}
					}
				}
			}
		}

		loadedPatient = this;
    }

	public static Patient getLoadedPatient()
	{
		return loadedPatient;
	}

	public string getAdditionalInfo()
	{
		string result = "";
		foreach (AdditionalInformation info in additionalInformation) {
			result += "<b>" + info.name + "</b>" + "\n\n";
			result += info.content + "\n\n";
		}
		return result;
	}
}
