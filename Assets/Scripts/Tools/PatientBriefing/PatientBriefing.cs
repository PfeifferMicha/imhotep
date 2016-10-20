using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PatientBriefing : MonoBehaviour
{

    public GameObject textObj;
    public GameObject tabButton;
    public GameObject rawImageObj;
    public GameObject scrollView;
    private Text text;

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
                newButton.transform.FindChild("Text").GetComponent<Text>().text = tabNames[i];
                newButton.transform.SetParent(tabButton.transform.parent, false);

                string capturedTabName = tabNames[i];   // might not be necessary to capture, but just in case the list changes?
                Button b = newButton.GetComponent<Button>();
                b.onClick.AddListener(() => selectTab(b));
            }

            if (tabButton.transform.parent.childCount > 1)
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
        textObj.SetActive(false);
        rawImageObj.SetActive(true);
        //Set scroll content
        scrollView.GetComponent<ScrollRect>().content = rawImageObj.GetComponent<RectTransform>();

        int widthOfStrechedRawImage = (int)rawImageObj.GetComponent<RawImage>().rectTransform.rect.width - 17;

        System.Drawing.Bitmap image = (System.Drawing.Bitmap)TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImageGdiPlus(rewriteBodyWidth(info.content, widthOfStrechedRawImage), new System.Drawing.Size(widthOfStrechedRawImage, 1500));//, System.Drawing.Text.TextRenderingHint.AntiAliasGridFit);
        // Create a new widthOfStrechedRawImagex1500 texture ARGB32 (32 bit with alpha) and no mipmaps
        Texture2D texture = new Texture2D(widthOfStrechedRawImage, 1500, TextureFormat.ARGB32, false);

        /*
        // set the pixel values
        Color[] colorArray = new Color[image.Width * image.Height];
        for (int i = image.Height - 1; i >= 0; i--)
        {
            for (int j = image.Width - 1; j >= 0 ; j--)
            {   //Make no debug logs here!
                System.Drawing.Color colPixel = image.GetPixel(j, i);
                //colorArray[i * image.Width + j] = new UnityEngine.Color(colPixel.R, colPixel.G, colPixel.B, colPixel.A);
                texture.SetPixel(j, image.Height - i, new UnityEngine.Color(colPixel.R, colPixel.G, colPixel.B, colPixel.A));
            }
        }
        //texture.SetPixels(colorArray);
        */
    
        //Save image with MemoryStream and load it into texture
        using (MemoryStream ms = new MemoryStream())
        {
            image.Save(ms, image.RawFormat);
            texture.LoadImage(ms.ToArray());
        }
        // Apply all SetPixel calls
        texture.Apply();

        // connect texture to material of GameObject this script is attached to
        rawImageObj.GetComponent<RawImage>().material.mainTexture = texture;
        return;
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
            if (child.gameObject != tabButton)
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
