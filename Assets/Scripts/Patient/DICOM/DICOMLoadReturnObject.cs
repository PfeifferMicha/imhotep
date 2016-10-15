using UnityEngine;
using System.Collections;
using System;


public class DICOMLoadReturnObject {

    public int texWidth { get; set; }
    public int texHeight { get; set; }
    public int texDepth { get; set; }
    public Color32[] colors { get; set; }
    public DICOMHeader header { get; set; }
	public UInt16 minCol;
	public UInt16 maxCol;

	public DICOMLoadReturnObject (int texWidth, int texHeight, int texDepth, Color32[] colors, DICOMHeader header, UInt16 minCol, UInt16 maxCol)
    {
        this.texWidth = texWidth;
        this.texHeight = texHeight;
        this.texDepth = texDepth;
		this.colors = colors;
		this.header = header;
		this.minCol = minCol;
		this.maxCol = maxCol;
    }
}
