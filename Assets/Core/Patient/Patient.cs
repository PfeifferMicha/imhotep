using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using itk.simple;
using LitJson;
using UI;
using System.ComponentModel;

public class Patient : PatientMeta
{
	static private Patient loadedPatient = null;

	private List<AdditionalInformation> additionalInformation = new List<AdditionalInformation> ();
	private List<string> additionalInformationTabs = new List<string> ();
	private List<View> mViews = new List<View> ();
	private List<GameObject> mAnnotations = new List<GameObject> ();

	public class AdditionalInformation
	{
		public string name { get; set; }
        public string type { get; set; }
		public string content { get; set; } //TODO save path, not content of the file
		public string tabName { get; set; }
	}

	public Patient( PatientMeta meta ) : base(meta)
	{
		PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_FinishedLoading, finishedLoading);	// if we were listening already, stop
        PatientEventSystem.startListening (PatientEventSystem.Event.PATIENT_FinishedLoading, finishedLoading);

        ThreadUtil t = new ThreadUtil(this.PatientLoaderWorker, this.PatientLoaderCallback);
        t.Run();
    }
    ~Patient()
	{
		stopListeningToEvents ();
    }

	private void stopListeningToEvents()
	{
		PatientEventSystem.stopListening (PatientEventSystem.Event.PATIENT_FinishedLoading, finishedLoading);
	}

    private string rewritePathInHTML(string content, string filePath)
    {
        //
        string folderHTMLSources = ""; 
        string[] substrings = filePath.Split('/');
        for (int i = 0; i < substrings.Length - 1; i++)
        {
            folderHTMLSources += substrings[i];
        }

        //Rewrite src and href in HTML, because relative paths are wrong
        string c = content;
        c = c.Replace("src=\".", "src=\"./" + path + "/" + folderHTMLSources);
        c = c.Replace("href=\".", "href=\"./" + path + "/" + folderHTMLSources);
        return c;
    }


	public static Patient getLoadedPatient()
	{
		return loadedPatient;
	}

	public void save()
	{
		saveViews ();
		saveAnnotation();
	}

	public static void close()
	{
		if (loadedPatient != null)
			loadedPatient.stopListeningToEvents ();
		loadedPatient = null;
	}

	public List<string> getAdditionalInfoTabs()
	{
		return additionalInformationTabs;
	}
	public List<AdditionalInformation> getAdditionalInfo()
	{
        return additionalInformation;

        /*
        string result = "";
		foreach (AdditionalInformation info in additionalInformation) {
			// Add all info for this tab to the string:
			if (info.tabName == tabName) {
				result += "<b>" + info.name + "</b>" + "\n\n";
				result += info.content + "\n\n";
			}
		}
		return result;  */
	}

	private string bold( string input )
	{
		return "<b>" + input + "</b>";
	}

	///////////////////////////////////////////////////////
	// Views:

	public int getViewCount()
	{
		return mViews.Count;
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
		//Debug.Log ("Loading Views: " + viewsFile);
		if (File.Exists (viewsFile)) {
			try {
				// Read the file
				string line;
				System.IO.StreamReader file = new System.IO.StreamReader (viewsFile);
				while ((line = file.ReadLine ()) != null) {
					ViewJson vj = JsonMapper.ToObject<ViewJson> (line);
					View view = new View (vj);
					mViews.Add (view);
				}
				file.Close ();
			} catch {
				mViews.Clear ();
			}
		}
		Debug.Log ("Loaded: " + mViews.Count + " views.");
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

	///////////////////////////////////////////////////////
	// UI:

	public void setupDefaultWidgets()
	{
		// TODO: Get info about default UI widgets from patient's json:

		Widget w = UI.Core.instance.getWidgetByName ("ViewControl");
		if (w)
			w.gameObject.SetActive (true);

		w = UI.Core.instance.getWidgetByName ("PatientBriefing");
		if (w)
			w.gameObject.SetActive (true);
		
		w = UI.Core.instance.getWidgetByName ("DICOMViewer");
		if (w)
			w.gameObject.SetActive (true);
	}




    private void PatientLoaderWorker(object sender, DoWorkEventArgs e)
    {
        string metaFile = Path.Combine(base.path, "meta.json");
        string raw = File.ReadAllText(metaFile);
        JsonData data;
        try
        {
            data = JsonMapper.ToObject(raw);
        }
        catch
        {
            throw new System.Exception("Cannot parse meta.json. Invalid syntax?");
        }

        if (data.Keys.Contains("additional information"))
        {
            JsonData infoArray = data["additional information"];

            // Set up default tab:
            additionalInformationTabs.Add("General");
            string c = "";
            c += bold("Patient Name: ") + name + "\n";
            c += bold("Date of Birth: ") + birthDate + "\n";
            c += bold("Date of Operation: ") + operationDate + "\n";
            AdditionalInformation info = new AdditionalInformation
            {
                name = "Patient Information",
                type = "Plaintext",
                content = c,
                tabName = "General"
            };
            additionalInformation.Add(info);

            // Find other information in files (which are given in meta.json) and add them to the tabs:
            for (int i = 0; i < infoArray.Count; i++)
            {
                JsonData entry = infoArray[i];
                if (entry.Keys.Contains("Name") && entry.Keys.Contains("File"))
                {  //TODO use JsonMapper.ToObject to load this

                    if (System.IO.File.Exists(base.path + "/" + entry["File"]))
                    {
                        string content = System.IO.File.ReadAllText(base.path + "/" + entry["File"]);
                        string title = entry["Name"].ToString();
                        string type = "plainText";
                        if (entry.Keys.Contains("Type"))
                            type = entry["Type"].ToString();
                        string tabName = "General";
                        if (entry.Keys.Contains("Tab"))
                        {
                            tabName = entry["Tab"].ToString();
                        }
                        if (name.Length > 0 && content.Length > 0)
                        {
                            info = new AdditionalInformation
                            {
                                name = title,
                                type = type,
                                content = content,
                                tabName = tabName
                            };
                            if (info.type == "HTML")
                            {
                                info.content = rewritePathInHTML(info.content, entry["File"].ToString());

                            }
                            additionalInformation.Add(info);
                            if (!additionalInformationTabs.Contains(tabName))
                            {
                                additionalInformationTabs.Add(tabName);
                            }
                        }
                    }
                }
            }
        }

        readViews();
    }

    private void PatientLoaderCallback(object sender, RunWorkerCompletedEventArgs e)
    {
        //BackgroundWorker worker = sender as BackgroundWorker;
        if (e.Cancelled)
        {
            Debug.Log("[Patient.cs] Patient Loading cancelled"); //Not implemented in worker yet
        }
        else if (e.Error != null)
        {
            Debug.LogError("[Patient.cs] Error while loading the patient");
        }
        else
        {
            loadedPatient = this;

            // Let other widgets know the patient information is now available:
            PatientEventSystem.triggerEvent(PatientEventSystem.Event.PATIENT_Loaded, this);
        }
        return;
    }


	///////////////////////////////////////////////////////
	// Annotations:

	public void updateAnnotationList( List<GameObject> list )
	{
		mAnnotations = list;
	}

	//Saves all annotations in a file
	public void saveAnnotation ()
	{

		if (loadedPatient == null) {
			return;
		}

		string path = loadedPatient.path + "/annotation.json";

		//Create file if it not exists
		if (!File.Exists (path)) {
			using (StreamWriter outputFile = new StreamWriter (path, true)) {
				outputFile.Close ();
			}
		}

		//Write annotations in file
		using (StreamWriter outputFile = new StreamWriter (path)) {
			foreach (GameObject apListEntry in mAnnotations) {
				GameObject ap = apListEntry.GetComponent<AnnotationListEntry> ().getAnnotation ();
				AnnotationJson apj = new AnnotationJson ();
				apj.Text = ap.GetComponent<Annotation> ().getLabelText ();
				apj.ColorR = ap.GetComponent<Annotation> ().getColor ().r;
				apj.ColorG = ap.GetComponent<Annotation> ().getColor ().g;
				apj.ColorB = ap.GetComponent<Annotation> ().getColor ().b;
				apj.PositionX = ap.transform.localPosition.x;
				apj.PositionY = ap.transform.localPosition.y;
				apj.PositionZ = ap.transform.localPosition.z;

				apj.RotationW = ap.transform.localRotation.w;
				apj.RotationX = ap.transform.localRotation.x;
				apj.RotationY = ap.transform.localRotation.y;
				apj.RotationZ = ap.transform.localRotation.z;

				apj.Creator = ap.GetComponent<Annotation> ().creator;
				apj.CreationDate = ap.GetComponent<Annotation> ().creationDate;
				outputFile.WriteLine (JsonUtility.ToJson (apj));
			}
			outputFile.Close ();
		}
		return;
	}
    
    ///////////////////////////////////////////////////////
    // Misc:

    public void finishedLoading(object obj = null)
    {
        setupDefaultWidgets();
		NotificationControl.instance.createNotification ("Patient loaded.", new TimeSpan (0, 0, 5));
    }
}