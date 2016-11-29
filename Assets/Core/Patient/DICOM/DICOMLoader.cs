using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using itk.simple;

/*! Searches given directories for DICOMs and loads them (all in seperate threads).
 * Usage: Set a directory using setDirectory. Once the directory has been parsed,
 * a PatientEventSystem.Event.DICOM_NewList Event is triggered.
 * Now, you can load one of the series given by DICOMLoader.availableSeries to load
 * a DICOM, by calling the startLoading function. Once the loading process is finished,
 * access the loaded DICOM through DICOMLoader.currentDICOM. */
public class DICOMLoader : MonoBehaviour {

	/*! The singleton instance of the loader (for easier access) */
	public static DICOMLoader instance { private set; get; }

	/*! The DICOM slice which has been loaded */
	public DICOM currentDICOM { private set; get; }
	/*! The DICOM volume which has been loaded */
	public DICOM currentDICOMVolume { private set; get; }
	/*! The DICOM series which has been loaded */
	public DICOMSeries currentDICOMSeries { private set; get; }

	/*! Can be used to check if the loader is currently working on something. */
	public bool isBusy { private set; get; }

	/*! Temporary value to pass the directory to the thread */
	private string directoryToLoad;

	/*! Temporary value to pass the directory to the thread */
	private DICOMSeries seriesToLoad;
	/*! Temporary value to pass the slice number to the thread */
	private int sliceToLoad;

	private DICOM newlyLoadedDICOM;

	/*! The directory which has been set: */
	private string currentDirectory;

	public List<DICOMSeries> availableSeries { private set; get; }

	/*! Set to true when a directory has been parsed. */
	private bool newDirectoryParsed = false;
	/*! Set to true when a DICOM has been loaded. */
	private bool newDICOMLoaded = false;

	public DICOMLoader()
	{
		if (instance != null)
			throw( new System.Exception( "Error: Only one Instance of DICOMLoader may exist!" ) );

		availableSeries = new List<DICOMSeries>();

		instance = this;
		isBusy = false;
	}

	// ===============================================================
	// Directory parsing:

	/*! Starts searching the given path for DICOMs.
	 * \return true if loading process is started, false if the loader is currently busy. */
	public bool setDirectory( string path )
	{
		if (!isBusy) {

			// Lock:
			isBusy = true;

			directoryToLoad = path;
			ThreadUtil t = new ThreadUtil (parseDirectory, parseDirectoryCallback);
			t.Run ();

			// Let event system know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_AddLoadingJob,
				"DICOM directory parsing");
			
			return true;
		} else {
			return false;
		}
	}

	/*! Searches the directoryToLoad for DICOMs. Called in thread!*/
	public void parseDirectory(object sender, DoWorkEventArgs e)
	{
		// Parse the directory and return the seriesUIDs of all the DICOM series:
		try {
			availableSeries.Clear();

			VectorString series = ImageSeriesReader.GetGDCMSeriesIDs (directoryToLoad);
			if (series.Count > 0) {

				foreach (string s in series) {
					DICOMSeries info = new DICOMSeries ( directoryToLoad, s );
					availableSeries.Add(info);
				}
			}

			// Debug log:
			string str = "[DICOM] Found " + series.Count + " series.";
			for (int i = 0; i < series.Count; i++)
				str += "\n\t" + series [i];
			Debug.Log (str);

		} catch( System.Exception err ) {
			Debug.LogError( err.Message );
		}
	}

	/*! Called when loader has finished parsing a directory. */
	public void parseDirectoryCallback(object sender, RunWorkerCompletedEventArgs e)
	{
		// Unlock:
		isBusy = false;
		newDirectoryParsed = true;
	}

	// ===============================================================
	// DICOM Loading:

	/*! Starts loading the given DICOM, if available.
	 * \note If slice is negative, this loads the entire volume!
	 * \return true if loading process is started, false if the loader is currently busy. */
	public bool startLoading (DICOMSeries toLoad, int slice = 0 )
	{
		if (!isBusy && toLoad != null) {

			// Lock:
			isBusy = true;

			seriesToLoad = toLoad;
			sliceToLoad = slice;
			ThreadUtil t = new ThreadUtil (load, loadCallback);
			t.Run ();

			// Let event system know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_AddLoadingJob,
				"DICOM directory parsing");

			return true;
		} else {
			return false;
		}
	}
	/*! Starts loading the given DICOM slice, if available (overload for convenience)
	* \return true if loading process is started, false if the loader is currently busy
	* 		or if no series with the given index is available. */
	public bool startLoadingSlice ( int index, int slice = 0 )
	{
		if (index >= 0 && index < availableSeries.Count) {
			return startLoading (availableSeries [index], slice);
		} else {
			return false;
		}
	}
	/*! Starts loading the given DICOM slice, if available (overload for convenience)
	* \return true if loading process is started, false if the loader is currently busy
	* 		or if no series with the given series UID is available. */
	public bool startLoadingSlice ( string seriesUID, int slice = 0 )
	{
		foreach (DICOMSeries i in availableSeries) {
			if (i.seriesUID == seriesUID) {
				return startLoading (i, slice);
			}
		}
		return false;
	}

	/*! Starts loading the given DICOM volume, if available.
	* \return true if loading process is started, false if the loader is currently busy. */
	public bool startLoadingVolume( DICOMSeries toLoad )
	{
		return startLoading (toLoad, -1);
	}

	/*! Starts loading the given DICOM volume, if available (overload for convenience)
	* \return true if loading process is started, false if the loader is currently busy
	* 		or if no series with the given index is available. */
	public bool startLoadingVolume ( int index )
	{
		if (index >= 0 && index < availableSeries.Count) {
			return startLoading (availableSeries [index], -1);
		} else {
			return false;
		}
	}
	/*! Starts loading the given DICOM volume, if available (overload for convenience)
	* \return true if loading process is started, false if the loader is currently busy
	* 		or if no series with the given series UID is available. */
	public bool startLoadingVolume ( string seriesUID )
	{
		foreach (DICOMSeries i in availableSeries) {
			if (i.seriesUID == seriesUID) {
				return startLoading (i, -1);
			}
		}
		return false;
	}



	/*! Loads a DICOM. Called in thread!
	 * \note if sliceToLoad is negative, this will load the whole volume. */
	public void load(object sender, DoWorkEventArgs e)
	{
		try {
			DICOM newDICOM = new DICOM( seriesToLoad, sliceToLoad );
			newlyLoadedDICOM = newDICOM;
		} catch( System.Exception err ) {
			Debug.LogError( err.Message );
		}
	}
	/*! Called when loader has finished parsing a directory. */
	public void loadCallback(object sender, RunWorkerCompletedEventArgs e)
	{
		// Unlock:
		isBusy = false;
		newDICOMLoaded = true;
	}


	// ===============================================================
	// Update loop:
	public void Update()
	{
		if (newDirectoryParsed) {
			PatientEventSystem.triggerEvent(PatientEventSystem.Event.DICOM_NewList);
			newDirectoryParsed = false;

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_RemoveLoadingJob,
				"DICOM directory parsing");
		}
		if (newDICOMLoaded) {
			newDICOMLoaded = false;
			if (newlyLoadedDICOM.dimensions == 2) {
				currentDICOM = newlyLoadedDICOM;
				// Let Listeners know that we've loaded a new DICOM:
				PatientEventSystem.triggerEvent (PatientEventSystem.Event.DICOM_NewLoaded, currentDICOM);
			} else {
				currentDICOMVolume = newlyLoadedDICOM;
				// Let Listeners know that we've loaded a new DICOM:
				PatientEventSystem.triggerEvent (PatientEventSystem.Event.DICOM_NewLoadedVolume, currentDICOMVolume);
			}
		}
	}

}