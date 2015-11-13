using UnityEngine;
//using gdcm;

public class DicomLoader : MonoBehaviour {

	// Use this for initialization
	void Start () {

        /*string directory = "E:/IMHOTEPUnity/TestPatient/RetrospectivePatient1";

        Debug.Log("Loading DICOMS from " + directory);

        Tag t = new Tag(0x0020, 0x000e);

        Directory d = new Directory();
        uint nfiles = d.Load(directory);
        if (nfiles == 0) return;

        SmartPtrStrictScan sscan = StrictScanner.New();

        sscan.AddTag(t);
        bool b = sscan.Scan(d.GetFilenames());
        if (!b) return;

        for (int i = 0; i < (int)nfiles; ++i)
        {
            if (!sscan.IsKey(d.GetFilenames()[i]))
            {
                Debug.Log("File is not DICOM or could not be read: " + d.GetFilenames()[i]);
            }
        }

        Debug.Log("Scan:\n" + sscan.toString());

        Debug.Log("success");*/
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}