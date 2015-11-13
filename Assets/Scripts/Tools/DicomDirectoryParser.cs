using UnityEngine;
using System.Collections;
using System;
//using gdcm;

public class DicomDirectoryParser {

    /*bool mysort(DataSet ds1, DataSet ds2)
    {
        return false;
    }

    bool mysortByStudyInstanceUID(DataSet ds1, DataSet ds2)
    {
        Tag t = new Tag(0x0020, 0x00d);
        DataElement elem1 = ds1.GetDataElement(t);
        DataElement elem2 = ds1.GetDataElement(t);
        return String.Compare(elem1.toString(), elem2.toString()) < 0;
    }

    bool mysortBySeriesInstanceUID(DataSet ds1, DataSet ds2)
    {
        Tag t = new Tag(0x0020, 0x000e);
        DataElement elem1 = ds1.GetDataElement(t);
        DataElement elem2 = ds1.GetDataElement(t);
        return String.Compare(elem1.toString(), elem2.toString()) < 0;
    }

    bool mysortByImageOrientationPatient(DataSet ds1, DataSet ds2)
    {
        Tag t = new Tag(0x0020, 0x0037);
        DataElement elem1 = ds1.GetDataElement(t);
        DataElement elem2 = ds1.GetDataElement(t);
        return String.Compare(elem1.toString(), elem2.toString()) < 0;
    }*/


    // Use this for initialization
    void Start () {

        /*string directory = "E:/IMHOTEPUnity/TestPatient/RetrospectivePatient1";
        Debug.Log("Loading DICOMS from " + directory);
        Directory d = new Directory();
        uint nfiles = d.Load(directory);

        public delegate bool Del(DataSet ds1, DataSet ds2);
        Del = mysort;

        Sorter sorter = new Sorter();
        sorter.SetSortFunction(mysort);
        sorter.Sort(d.GetFilenames());

        sorter.SetSortFunction(mysort);
        sorter.Sort(sorter.GetFilenames());

        FilenamesType filenames = sorter.GetFilenames();*/
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    


    /*bool mysortImagePositionPatient(DataSet ds1, DataSet ds2)
    {
        // Do the IPP sorting here
        gdcm::Attribute < 0x0020,0x0032 > ipp1;
        gdcm::Attribute < 0x0020,0x0037 > iop1;
        ipp1.Set(ds1);
        iop1.Set(ds1);
        gdcm::Attribute < 0x0020,0x0032 > ipp2;
        gdcm::Attribute < 0x0020,0x0037 > iop2;
        ipp2.Set(ds2);
        iop2.Set(ds2);
        if (iop1 != iop2)
        {
            return false;
        }

        // else
        double normal[3];
        normal[0] = iop1[1] * iop1[5] - iop1[2] * iop1[4];
        normal[1] = iop1[2] * iop1[3] - iop1[0] * iop1[5];
        normal[2] = iop1[0] * iop1[4] - iop1[1] * iop1[3];
        double dist1 = 0;
        for (int i = 0; i < 3; ++i) dist1 += normal[i] * ipp1[i];
        double dist2 = 0;
        for (int i = 0; i < 3; ++i) dist2 += normal[i] * ipp2[i];

        std::cout << dist1 << "," << dist2 << std::endl;
        return dist1 < dist2;
    }*/

}


