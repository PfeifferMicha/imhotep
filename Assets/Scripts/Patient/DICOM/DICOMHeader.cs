using System;

public class DICOMHeader
{
	public DICOMHeader ( string studyInstanceUID, string seriesInstanceUID )
	{
		mStudyUID = studyInstanceUID;
		mSeriesUID = seriesInstanceUID;
		mPatientName = "Unknown";
	}

	public string getPatientName() {
		return mPatientName;
	}
	public void setPatientName( string name ) {
		// TODO: Parse to get rid of ^.
		mPatientName = name;
	}

	public DateTime getSeriesDateTime()
	{
		return mSeriesDateTime;
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
		int hour = mSeriesDateTime.Hour;
		int minute = mSeriesDateTime.Minute;
		int second = mSeriesDateTime.Second;

		mSeriesDateTime = new DateTime( year, month, day, hour, minute, second );
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
		int year = mSeriesDateTime.Year;
		int month = mSeriesDateTime.Month;
		int day = mSeriesDateTime.Day;

		mSeriesDateTime = new DateTime( year, month, day, hour, minute, second );
	}

	private string mPatientName;
	private DateTime mSeriesDateTime;
	public string mSeriesUID { get; set; }
	public string mStudyUID { get; set; }
	public string mModality { get; set; }
	public string mInstitutionName { get; set; }

	public override string ToString ()
	{
		return "DICOM Header: " + "\n" +
		"\t" + "(0020, 000d), Study Instance UID: " + mStudyUID + "\n" +
		"\t" + "(0020, 000e), Series Instance UID: " + mSeriesUID + "\n" +
		"\t" + "(0010, 0010), Patient Name: " + mPatientName + "\n" +
		"\t" + "(0008, 0021), Series Date: " + mSeriesDateTime.Year + " " + mSeriesDateTime.Month + " " + mSeriesDateTime.Day + "\n" +
		"\t" + "(0008, 0031), Series Time: " + mSeriesDateTime.TimeOfDay + "\n";
	}
}

