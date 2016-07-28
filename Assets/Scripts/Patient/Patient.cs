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
		public View( ViewJson vj )
		{
			name = vj.name;
			orientation = new Quaternion( (float)vj.orientation[0],(float) vj.orientation[1], (float)vj.orientation[2], (float)vj.orientation[3] );
			scale = new Vector3( (float)vj.scale[0], (float)vj.scale[1], (float)vj.scale[2] );
			opacities = new Dictionary<string, double>();
			if( vj.opacityKeys.Count == vj.opacityValues.Count )
			{
				int numEntries = vj.opacityKeys.Count;
				for( int i = 0; i < numEntries; i ++ )
				{
					opacities.Add( vj.opacityKeys[i], vj.opacityValues[i] );
				}
			} else {
				throw new System.Exception("Number of opacity values incorrect. Number of opacity keys and number of opacities must match!");
			}
		}
		public View() {
			opacities = new Dictionary<string, double>();
		}
		public string name { get; set; }
		public Quaternion orientation { get; set; }
		public Vector3 scale { get; set; }
		public Dictionary<string, double> opacities { get; set; }
	}
	public class ViewJson
	{
		public ViewJson() {
			orientation = new double[4];
			scale = new double[3];
			//opacityKeys = new string[30];
			//opacities = new double[30];
			//opacities = new Dictionary<string, double>();
			opacityKeys = new List<string>();
			opacityValues = new List<double>();
		}
		public ViewJson( View v )
		{
			name = v.name;
			orientation = new double[4];
			orientation[0] = (double)v.orientation.x;
			orientation[1] = (double)v.orientation.y;
			orientation[2] = (double)v.orientation.z;
			orientation[3] = (double)v.orientation.w;
			scale = new double[3];
			scale[0] = (double)v.scale.x;
			scale[1] = (double)v.scale.y;
			scale[2] = (double)v.scale.z;
			opacityKeys = v.opacities.Keys.ToList();
			opacityValues = v.opacities.Values.ToList();
			/*opacityKeys = new string[v.opacities.Count];
			opacities = new double[v.opacities.Count];
			uint i = 0;
			foreach(KeyValuePair<string, float> entry in v.opacities)
			{
				opacityKeys[i] = entry.Key;
				opacities[i] = (double)entry.Value;
				i ++;
			}*/
		}
		public string name { get; set; }
		public double[] orientation { get; set; }
		public double[] scale { get; set; }
		//public string[] opacityKeys;
		//public double[] opacities;
		public List<string> opacityKeys { get; set; }
		public List<double> opacityValues { get; set; }
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

		readViews ();

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

	private void readViews()
	{
		// Find any views in a seperate view file, if given:
		string viewsFile = Path.Combine (base.path, "views.json");		// TODO: Read from base path?
		mViews.Clear ();
		Debug.Log ("Loading Views: " + viewsFile);
		if (File.Exists (viewsFile)) {
			Debug.Log ("File found.");
			//try {
				// Read the file
				string line;
				System.IO.StreamReader file = new System.IO.StreamReader (viewsFile);
				Debug.Log ("file: " + file);
				while ((line = file.ReadLine ()) != null) {
					Debug.Log ("Line: " + line);
					ViewJson vj = JsonMapper.ToObject<ViewJson> (line);
					View view = new View (vj);
					mViews.Add (view);
				}
				file.Close ();
			//} catch {
			//	mViews.Clear ();
			//}
		}
		Debug.Log ("Loaded: " + mViews.Count);
	}

	public void saveViews()
	{
		Patient p = Patient.getLoadedPatient();
		string path = p.path + "/views.json";

		//Create file if it not exists
		if (!File.Exists(path))
		{
			using (StreamWriter outputFile = new StreamWriter(path,true))
			{
				outputFile.Close();
			}
		}

		//Write annotations in file
		using (StreamWriter outputFile = new StreamWriter(path))
		{
			foreach(View view in mViews)
			{
				ViewJson vj = new ViewJson(view);
				outputFile.WriteLine(JsonMapper.ToJson(vj));
			}
			outputFile.Close();
		}
		return;
	}
}
