using UnityEngine;
using System.Collections;
using System;


public class DICOMLoadReturnObject {

    public int texWidth { get; set; }
    public int texHeight { get; set; }
    public int texDepth { get; set; }
    public Color32[] colors { get; set; }
    public DICOMHeader header { get; set; }

	public DICOMLoadReturnObject (int texWidth, int texHeight, int texDepth, Color32[] colors, DICOMHeader header)
    {
        this.texWidth = texWidth;
        this.texHeight = texHeight;
        this.texDepth = texDepth;
		this.colors = colors;
		this.header = header;
    }
}
