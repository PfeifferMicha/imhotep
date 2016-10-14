using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class PatientBriefing : MonoBehaviour
{

    public GameObject textObj;
    public GameObject tabButton;
    public GameObject rawImageObj;
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
        text.text = result;
        return;
    }

    private void showHTML(Patient.AdditionalInformation info)
    {
        textObj.SetActive(false);
        rawImageObj.SetActive(true);
        System.Drawing.Bitmap image = (System.Drawing.Bitmap)TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImageGdiPlus(info.content, new System.Drawing.Size(829, 940));//, System.Drawing.Text.TextRenderingHint.AntiAliasGridFit);
        image.Save("image2.png", System.Drawing.Imaging.ImageFormat.Png);
        // Create a new 829x940 texture ARGB32 (32 bit with alpha) and no mipmaps
        Texture2D texture = new Texture2D(829, 940, TextureFormat.ARGB32, false);

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

        // Apply all SetPixel calls
        texture.Apply();

        // connect texture to material of GameObject this script is attached to
        rawImageObj.GetComponent<RawImage>().material.mainTexture = texture;
        return;
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
