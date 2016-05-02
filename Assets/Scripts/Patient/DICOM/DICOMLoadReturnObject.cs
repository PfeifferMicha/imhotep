using UnityEngine;
using System.Collections;

public class DICOMLoadReturnObject {

    public int texWidth { get; set; }
    public int texHeight { get; set; }
    public int texDepth { get; set; }
    public Color[] colors { get; set; }
    public DICOMHeader header { get; set; }
    public int maxCol { get; set; }
    public int minCol { get; set; }

    public DICOMLoadReturnObject(int texWidth, int texHeight, int texDepth, Color[] colors, DICOMHeader header, int maxCol, int minCol)
    {
        this.texWidth = texWidth;
        this.texHeight = texHeight;
        this.texDepth = texDepth;
        this.colors = colors;
        this.header = header;
        this.maxCol = maxCol;
        this.minCol = minCol;
    }
}
