using System;
using itk.simple;
using UnityEngine;
using System.Globalization;

public class DICOMHeader : ICloneable
{
	private string PatientName;
	private DateTime SeriesDateTime;
	public string SeriesUID { get; private set; }
	public string StudyUID { get; private set; }
	public string Modality { get; private set; }
	public string InstitutionName { get; set; }
	public string ImageComments { get; private set; }
	public DateTime AcquisitionDate { get; private set; }
	public uint Dimension{ get; private set; }
	public VectorDouble Origin{ get; private set; }
	public VectorDouble Spacing{ get; private set; }
	public VectorDouble Direction{ get; private set; }
	public uint NumberOfImages{ get; set; }
	public VectorString FileNames{ get; private set; }
	public int RescaleIntercept{ get; private set; }
	public int RescaleSlope{ get; private set; }
	public int MaxPixelValue{ get; private set; }
	public int MinPixelValue{ get; private set; }

	public DICOMHeader ( Image image, VectorString fileNames )
	{

		Modality = "";
		ImageComments = "";
		InstitutionName = "";
		PatientName = "Unknown";
		RescaleSlope = 1;
		RescaleIntercept = 0;
		MinPixelValue = UInt16.MinValue;
		MaxPixelValue = UInt16.MaxValue;

		// Study and Series UID should always be present:
		StudyUID = image.GetMetaData ("0020|000d");
		SeriesUID = image.GetMetaData ("0020|000e");

		// Some of the following tags may not be in the DICOM Header, so catch and ignore "not found" exceptions:
		try {
			ImageComments = image.GetMetaData ("0020|4000");
		} catch { Debug.LogWarning ("Could not find or interpret DICOM tag: (0020|4000)");}
		try {
			string tmp = image.GetMetaData ("0008|0022");
			AcquisitionDate = DICOMDateToDateTime (tmp);
		} catch { Debug.LogWarning ("Could not find or interpret DICOM tag: (0008|0022)");}
		try {
			setPatientName ( image.GetMetaData( "0010|0010" ) );
		} catch { Debug.LogWarning ("Could not find or interpret DICOM tag: (0010|0010)");}
		try {
			setSeriesDate( image.GetMetaData( "0008|0021" ) );
		} catch { Debug.LogWarning ("Could not find or interpret DICOM tag: (0008|0021)");}
		try {
			Modality = image.GetMetaData( "0008|0060" );
		} catch { Debug.LogWarning ("Could not find or interpret DICOM tag: (0008|0060)");}
		try {
			InstitutionName = image.GetMetaData( "0008|0080" );
		} catch(System.Exception e ) { Debug.LogWarning ("Could not find or interpret DICOM tag: (0008|0080) Exception: " + e.Message );}
		try {
			RescaleIntercept = Int32.Parse( image.GetMetaData("0028|1052") );
		} catch(System.Exception e ) { Debug.LogWarning ("Could not find or interpret DICOM tag: (0028|1052) Exception: " + e.Message );}
		try {
			string slope = image.GetMetaData("0028|1053");
			RescaleSlope = Int32.Parse( slope );
		} catch(System.Exception e ) { Debug.LogWarning ("Could not find or interpret DICOM tag: (0028|1053) Exception: " + e.Message );}
		try {
			MinPixelValue = Int32.Parse( image.GetMetaData("0028|0106") );
		} catch(System.Exception e ) { Debug.LogWarning ("Could not find or interpret DICOM tag: (0028|0106) Exception: " + e.Message );}
		try {
			MaxPixelValue = Int32.Parse( image.GetMetaData("0028|0107") );
		} catch(System.Exception e ) { Debug.LogWarning ("Could not find or interpret DICOM tag: (0028|0107) Exception: " + e.Message );}

		if (MaxPixelValue == UInt16.MaxValue) {
			
			int storedBits = 16;
			try {
				storedBits = Int32.Parse( image.GetMetaData("0028|0101") );
			} catch(System.Exception e ) { Debug.LogWarning ("Could not find or interpret DICOM tag: (0028,0101) Exception: " + e.Message );}

			MaxPixelValue = 1 << storedBits;
		}

		Origin = image.GetOrigin ();
		Dimension = image.GetDimension ();
		Spacing = image.GetSpacing();
		Direction = image.GetDirection();

		FileNames = fileNames;
		NumberOfImages = (uint)fileNames.Count;
		/*Debug.Log ("New DICOM Header:");
		Debug.Log (Origin[0] + " " + Origin[1] + " " + Origin[2]);
		Debug.Log (Dimension);
		Debug.Log (Spacing[0] + " " + Spacing[1] + " " + Spacing[2]);
		Debug.Log (Direction[0] + " " + Direction[1] + " " + Direction[2] + " " + Direction[3] + " " + Direction[4] + " " + Direction[5]);
		Debug.Log (image.GetPixelIDTypeAsString ());*/
	}

	public object Clone()
	{
		return this.MemberwiseClone ();
	}

	DateTime DICOMDateToDateTime( string dateString )
	{
		DateTime result = DateTime.ParseExact ( dateString, "yyyyMMdd", CultureInfo.InvariantCulture);
		return result;
	}

	public string getPatientName() {
		return PatientName;
	}
	public void setPatientName( string name ) {
		// TODO: Parse to get rid of ^.
		PatientName = name;
	}

	public DateTime getSeriesDateTime()
	{
		return SeriesDateTime;
	}

	public void setSeriesDate( string date )
	{
		if (date.Length != 8)
			throw(new System.Exception("Date must be of format YYYYMMDD."));
		
		// Set new date:
		int year = Int16.Parse( date.Substring( 0, 4 ) );
		int month = Int16.Parse( date.Substring( 4, 2 ) );
		int day = Int16.Parse( date.Substring( 6, 2 ) );
		// Leave time unchanged:
		int hour = SeriesDateTime.Hour;
		int minute = SeriesDateTime.Minute;
		int second = SeriesDateTime.Second;

		SeriesDateTime = new DateTime( year, month, day, hour, minute, second );
	}

	public void setSeriesTime( string time )
	{
		// Set new time:
		int hour = 0;
		int minute = 0;
		int second = 0;
		// According to DICOM standard, the string may leave out any values towards the right (for example, only hours may be specified),
		// ... so check for string length!
		if( time.Length >= 2 )
			hour = Int16.Parse( time.Substring( 0, 2 ) );
		if( time.Length >= 4 )
			minute = Int16.Parse( time.Substring( 2, 2 ) );
		if( time.Length >= 6 )
			second = Int16.Parse( time.Substring( 4, 2 ) );
		// Leave date unchanged:
		int year = SeriesDateTime.Year;
		int month = SeriesDateTime.Month;
		int day = SeriesDateTime.Day;

		SeriesDateTime = new DateTime( year, month, day, hour, minute, second );
	}

	public override string ToString ()
	{
		return "DICOM Header: " + "\n" +
		"\t" + "(0020, 000d), Study Instance UID: " + StudyUID + "\n" +
		"\t" + "(0020, 000e), Series Instance UID: " + SeriesUID + "\n" +
		"\t" + "(0010, 0010), Patient Name: " + PatientName + "\n" +
		"\t" + "(0008, 0021), Series Date: " + SeriesDateTime.Year + " " + SeriesDateTime.Month + " " + SeriesDateTime.Day + "\n" +
		"\t" + "(0008, 0031), Series Time: " + SeriesDateTime.TimeOfDay + "\n";
	}

	public string toDescription()
	{
		return Modality + " " + ImageComments + " (" + AcquisitionDate + ")";
	}
}

