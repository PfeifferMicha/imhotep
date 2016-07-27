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
	private List<string> additionalInformationTabs = new List<string> ();
	private List<View> mViews = new List<View> ();

	public class AdditionalInformation
	{
		public string name { get; set; }
		public string content { get; set; }
		public string tabName { get; set; }
	}

	public class View
	{
		public string name { get; set; }
		public Quaternion orientation;
		public Vector3 scale;
		public Dictionary<string, float> opacities;
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

			// Set up default tab:
			additionalInformationTabs.Add ("General");
			string c = "";
			c += bold ("Patient Name: ") + name + "\n";
			c += bold ("Date of Birth: ") + birthDate + "\n";
			c += bold ("Date of Operation: ") + operationDate + "\n";
			AdditionalInformation info = new AdditionalInformation {
				name = "Patient Information",
				content = c,
				tabName = "General"
			};
			additionalInformation.Add (info);

			// Find other information in files (which are given in meta.json) and add them to the tabs:
			for (int i = 0; i < infoArray.Count; i++) {
				JsonData entry = infoArray [i];
				if (entry.Keys.Contains ("Name") && entry.Keys.Contains ("File")) {

					if( System.IO.File.Exists( base.path + "/" + entry ["File"] ) )
					{
						string content = System.IO.File.ReadAllText ( base.path + "/" + entry ["File"]);
						string title = entry ["Name"].ToString();
						string tabName = "General";
						if (entry.Keys.Contains ("Tab")) {
							tabName = entry ["Tab"].ToString();
						}
						if (name.Length > 0 && content.Length > 0) {
							info = new AdditionalInformation {
								name = title,
								content = content,
								tabName = tabName
							};
							additionalInformation.Add (info);
							if (!additionalInformationTabs.Contains (tabName)) {
								additionalInformationTabs.Add (tabName);
							}
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

	public List<string> getAdditionalInfoTabs()
	{
		return additionalInformationTabs;
	}
	public string getAdditionalInfo( string tabName )
	{
		string result = "";
		foreach (AdditionalInformation info in additionalInformation) {
			// Add all info for this tab to the string:
			if (info.tabName == tabName) {
				result += "<b>" + info.name + "</b>" + "\n\n";
				result += info.content + "\n\n";
			}
		}
		return result;
	}

	private string bold( string input )
	{
		return "<b>" + input + "</b>";
	}


	// Views:
	public List<View> getViews()
	{
		return mViews;
	}
	public int insertView( View newView, int index )
	{
		index = Mathf.Min (index, mViews.Count);
		mViews.Insert (index, newView);
		return index;
	}
	public View getView( int index )
	{
		if ( index >= 0 && mViews.Count > index) {
			return mViews [index];
		} else {
			return null;
		}
	}
	public void deleteView( int index )
	{
		if (index >= 0 && mViews.Count > index) {
			mViews.RemoveAt (index);
		}
	}
}
