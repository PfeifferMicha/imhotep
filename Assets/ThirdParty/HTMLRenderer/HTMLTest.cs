using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class HTMLTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        loadHTML();
	}

    private void loadHTML()
    {
        //string html = "<h1> Hello World </h1><p style=\"color:white; background-color:red\"> This is html rendered text</p>";

        // Open the file to read from.
        string html = File.ReadAllText("IMHOTEP.html");

        System.Drawing.Image image = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImageGdiPlus(html, new System.Drawing.Size(825, 1200));//, System.Drawing.Text.TextRenderingHint.AntiAliasGridFit);
        image.Save("image.png", System.Drawing.Imaging.ImageFormat.Png);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
