using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Patient
{
    public Patient( string directory )
    {
        Debug.Log("Path: " + directory);
        //mDicomLoader = new DicomLoader(directory + "/DICOM" );
    }

    
    DicomLoader mDicomLoader;
    List<Dicom> mLoadedDicoms;
}
