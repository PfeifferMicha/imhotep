using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel;

public class PatientBriefing : MonoBehaviour
{

	public GameObject textObj;
	public GameObject tabButton;
	public GameObject emptyFiller;
	public GameObject rawImageObj;
	public GameObject scrollView;
    public GameObject viewPort;
	private Text text;

    //True if HTML Renderer has finished
    private bool htmlRendered = false;

    //Variables for thread
    string html = "";
    int imageWidth = 1;
    int imageHeight = 1;
    byte[] imageArray;

    void Update()
    {
        if (htmlRendered)
        {
            htmlRendered = false;

            // Create a new texture ARGB32 (32 bit with alpha) and no mipmaps
            Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.ARGB32, false);           
            texture.LoadImage(imageArray);
            texture.Apply();
            // connect texture to material of GameObject this script is attached to
            rawImageObj.GetComponent<RawImage>().material.mainTexture = texture;

            //Resize rect transform component of raw image
            rawImageObj.GetComponent<RectTransform>().sizeDelta = new Vector2(imageWidth, imageHeight);
            rawImageObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(imageWidth / 2, -imageHeight / 2);

            textObj.SetActive(false);
            rawImageObj.SetActive(true);
            //Set scroll content
            scrollView.GetComponent<ScrollRect>().content = rawImageObj.GetComponent<RectTransform>();
        }
    }

    // Use this for initialization
    void OnEnable()
	{
		tabButton.SetActive(false);
		rawImageObj.SetActive(false);

		text = textObj.GetComponent<Text>();
		string msg = bold("Patient Information");
		msg += "\n\nNo patient loaded.";
		text.text = msg;

		PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Loaded, eventNewPatientLoaded);
		PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Closed, eventPatientClosed);

		Patient loadedPatient = Patient.getLoadedPatient();
		Debug.Log("loaded patient:" + loadedPatient);
		if (loadedPatient != null)
		{
			eventNewPatientLoaded(loadedPatient);
		}
		else
		{
			eventPatientClosed();
		}
	}

	void eventNewPatientLoaded(object obj)
	{
		Patient patient = obj as Patient;
		if (patient != null)
		{

			clearTabs();

			// Create Tabs according to the loaded tab names:
			List<string> tabNames = patient.getAdditionalInfoTabs();
			for (int i = 0; i < tabNames.Count; i++)
			{
				GameObject newButton = GameObject.Instantiate(tabButton);
				newButton.SetActive(true);
				newButton.name = tabNames[i];
				newButton.transform.Find("Text").GetComponent<Text>().text = tabNames[i];
				newButton.transform.SetParent(tabButton.transform.parent, false);

				Button b = newButton.GetComponent<Button>();
				b.onClick.AddListener(() => selectTab(b));
			}

			// Move the empty filler to the back of the list:
			emptyFiller.transform.SetAsLastSibling ();

			if (tabButton.transform.parent.childCount > 2)
			{
				// Select the first tab which is not the tabButton prefab:
				selectTab(tabButton.transform.parent.GetChild(1).GetComponent<Button>());
			}
		}
	}

	private void selectTab(Button b)
	{
		foreach (Transform child in tabButton.transform.parent)
		{
			if( child.gameObject != emptyFiller )
				UI.Core.instance.unselectTab(child.GetComponent<Button>());
		}
		UI.Core.instance.selectTab(b);

		string tabName = b.GetComponentInChildren<Text>().text;
		Patient loadedPatient = Patient.getLoadedPatient();
		if (loadedPatient == null)
		{
			return;

		}
		Debug.Log("Loading tab: " + tabName);



		string result = "";
		bool htmlFound = false;
		foreach (Patient.AdditionalInformation info in loadedPatient.getAdditionalInfo())
		{

			// Add all info for this tab to the string:
			if (info.tabName == tabName)
			{
				// If HTML found, render the html and skip the text
				if (info.type == "HTML")
				{
					htmlFound = true;
					showHTML(info);
					break;
				}

				result += "<b>" + info.name + "</b>" + "\n\n";
				result += info.content + "\n\n";
			}
		}
		if (!htmlFound)
		{
			showText(result);
		}
		return;

	}

	private void showText(string result)
	{
		textObj.SetActive(true);
		rawImageObj.SetActive(false);
		scrollView.GetComponent<ScrollRect>().content = textObj.GetComponent<RectTransform>();
		text.text = result;
		return;
	}

	private void showHTML(Patient.AdditionalInformation info)
	{
        rawImageObj.SetActive(false);
        html = info.content;
        //The width of the viewport (can be resized) 
        int widthOfViewport = (int)viewPort.GetComponent<RectTransform>().rect.width - 17;
        //int widthOfStretchedRawImage = (int)rawImageObj.GetComponent<RawImage>().rectTransform.rect.width;
        widthOfViewport = Math.Max(1, widthOfViewport); //prevent widthOfStrechedRawImage to be less then 1
        int widthBody = findBodyWidth(info.content);

        //If body with in html file is taller the space in the viewport, the body width attribute in html file is set to widthOfStretchedRawImage
        if (widthBody > widthOfViewport)
        {
            html = rewriteBodyWidth(info.content, widthOfViewport);
        }

        ThreadUtil t = new ThreadUtil(this.showHTMLWorker, this.showHTMLCallback);
        t.Run();

		return;
	}

    private void showHTMLCallback(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            Debug.Log("[PatientBriefing.cs] HTML Rendering cancelled");
        }
        else if (e.Error != null)
        {
            Debug.LogError("[PatientBriefing.cs] HTML Rendering Error");
        }
        else
        {
            htmlRendered = true;
        }
        return;
    }

    private void showHTMLWorker(object sender, DoWorkEventArgs e)
    {
        System.Drawing.Bitmap image = (System.Drawing.Bitmap)TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImageGdiPlus(html);
        imageWidth = image.Width;
        imageHeight = image.Height;
        using (MemoryStream ms = new MemoryStream())
        {
            image.Save(ms, image.RawFormat);
            imageArray = ms.ToArray();
        }
    }

    private int findBodyWidth(string input){
		//Find first "width:??px";
		string pattern = "width:\\s*\\d+px\\s*;";
		Regex rgx = new Regex(pattern);
		if(!rgx.IsMatch(input)){
			//If there is no match
			return Int32.MaxValue;
		}
		string width = rgx.Match(input).Value;

		//Get int from string
		string pattern2 = "\\d+";
		Regex rgx2 = new Regex(pattern2);
		int widthInt = Int32.MaxValue; 
		if (!Int32.TryParse (rgx2.Match (width).Value, out widthInt)) {
			Debug.LogError ("HTML parsing error");
		}
		return widthInt;
	}

	private string rewriteBodyWidth(string input, int width)
	{
		string pattern = "width:\\s*\\d+px\\s*;";
		string replacement = "width:"+ width + "px;";
		Regex rgx = new Regex(pattern);
		//If a width attribute is found
		if (rgx.IsMatch(input))
		{
			string result = rgx.Replace(input, replacement);
			return result;
		}

		//Else, add width attribute to body
		string pattern2 = "<body ";
		string replacement2 = "<body width:" + width + "px;";
		Regex rgx2 = new Regex(pattern2);
		string result2 = rgx2.Replace(input, replacement2);
		return result2;
	}

	private void clearTabs()
	{
		foreach (Transform child in tabButton.transform.parent)
		{
			if (child.gameObject != tabButton && child.gameObject != emptyFiller)
			{
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}
	}

	void eventPatientClosed(object obj = null)
	{
		string msg = bold("Patient Information");
		msg += "\n\n";

		msg += bold("Patient Name: ") + "No patient loaded.";

		text.text = msg;
	}

	private string bold(string input)
	{
		return "<b>" + input + "</b>";
	}
}
