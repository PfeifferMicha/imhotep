using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using itk.simple;

/*! This class is respondible for parsing DICOM directories and loading the images.
 * When events happen, other modules are notified using the PatientEventSystem class. */
public class PatientDICOMLoader : MonoBehaviour
{
	//! The singleton instance of the loader (for easier access):
	public static PatientDICOMLoader instance { get; private set; }

	//! DICOM loader instance:
	private DicomLoaderITK mDicomLoader = new DicomLoaderITK ();

	//! Already loaded DICOM:
	private DICOMSlice mCurrentDICOM = null;

	private DICOMVolume mCurrentDICOMVolume = null;


	//! Simple lock, used to prevent loading multiple directory or DICOMs at the same time:
	private bool isLoading = false;

	//! Path which should be searched
	private string PathForThread = "";
	//! ID of the DICOM in list which should be loaded
    private int DicomIDForThread = 0;
	//! Slice number which should be loaded (or -1 for full volume)
	private int DicomSliceForThread = 0;
	//! Temporary object returned by the loader
	private DICOMLoadReturnObjectSlice returnObjectSlice = null;
	//! Temporary object returned by the loader
	private DICOMLoadReturnObjectVolume returnObjectVolume = null;
    private bool loadingFinished = false;
    private bool loadingDirectoryFinished = false;

	public PatientDICOMLoader()
	{
		instance = this;
	}

    public void loadDirectory( string path )
	{
		if (!isLoading) {

			// Lock:
			isLoading = true;

			PathForThread = path;
			ThreadUtil t = new ThreadUtil(setDirectoryWorker, setDirectoryCallback);
            t.Run();

			// Let event system know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_AddLoadingJob,
				"DICOM search");
		}
	}
	private void setDirectoryWorker(object sender, DoWorkEventArgs e)
    {
        // Parse the directory:
		mDicomLoader.setDirectory(PathForThread);
    }
    private void setDirectoryCallback(object sender, RunWorkerCompletedEventArgs e)
    {
        loadingDirectoryFinished = true;
    }

	/*! Start loading the DICOM in the availableSeries list with the given ID.
	 * \return true if we started loading the given series, false if loading was blocked
	 * 		(because something else is still being loaded). */
    public bool loadDicom( int id )
	{
        if (!isLoading) {
			// Lock:
			isLoading = true;

			// Let everyone know we're starting to load a new DICOM:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.DICOM_StartLoading );

            DicomIDForThread = id;
			DicomSliceForThread = -1;

            ThreadUtil t = new ThreadUtil(loadDicomWorker, loadDicomCallback);
            t.Run();

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_AddLoadingJob,
				"DICOM");
			
			return true;
        }
		return false;
	}


	/*! Start loading a new slice of the currently loaded DICOM.
	 * \return true if we started loading the given series, false if loading was blocked
	 * 		(because something else is still being loaded). */
	public bool loadDicomSlice( int slice )
	{
		// Load the slice from the previously loaded DICOM:
		return loadDicomSlice( DicomIDForThread, slice );
	}

	/*! Start loading a new slice of the DICOM given by ID in the availableSeries list.
	 * \return true if we started loading the given series, false if loading was blocked
	 * 		(because something else is still being loaded). */
	public bool loadDicomSlice( int id, int slice )
	{
		if (!isLoading) {
			// Lock:
			isLoading = true;

			// Let everyone know we're starting to load a new DICOM:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.DICOM_StartLoading );

			DicomIDForThread = id;
			DicomSliceForThread = slice;

			ThreadUtil t = new ThreadUtil(loadDicomWorker, loadDicomCallback);
			t.Run();	


			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_AddLoadingJob,
				"DICOM");

			return true;
		}
		return false;
	}

    private void loadDicomWorker(object sender, DoWorkEventArgs e)
    {
		//List<string> series = mDicomLoader.getAvailableSeries ();
		if (DicomSliceForThread >= 0) {
			returnObjectSlice = mDicomLoader.loadSlice (DicomIDForThread, DicomSliceForThread);
			returnObjectVolume = null;
		} else {
			returnObjectVolume = mDicomLoader.load (DicomIDForThread);
			returnObjectSlice = null;
		}
    }

    private void loadDicomCallback(object sender, RunWorkerCompletedEventArgs e)
    {
        loadingFinished = true;
    }

    public List<DICOMHeader> getAvailableSeries()
	{
		return mDicomLoader.getAvailableSeries();
	}

	public DICOMSlice getCurrentDicom()
	{
		return mCurrentDICOM;
	}


	public DICOMVolume getCurrentDicomVolume()
	{
		return mCurrentDICOMVolume;
	}


	//! Checks the threads every frame to see if they're done:
	void Update()
	{
		if (loadingDirectoryFinished)
		{
			loadingDirectoryFinished = false;

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_RemoveLoadingJob,
				"DICOM search");

			PatientEventSystem.triggerEvent(PatientEventSystem.Event.DICOM_NewList);

			// Unlock:
			isLoading = false;

			//loadDicom(0);
		}

		if (loadingFinished)
		{
			loadingFinished = false;

			if(returnObjectSlice != null) {

				DICOMSlice dicom = new DICOMSlice( returnObjectSlice.itkImage );

				Texture2D tex = new Texture2D (returnObjectSlice.texWidth, returnObjectSlice.texHeight, TextureFormat.ARGB32, false, true);
				tex.SetPixels32 (returnObjectSlice.colors);
				tex.Apply ();
				dicom.setTexture2D(tex);

				/*Texture3D tex = new Texture3D (returnObject.texWidth, returnObject.texHeight, returnObject.texDepth, TextureFormat.ARGB32, false);
				//tex.SetPixels (returnObject.colors); //needs around 0.15 sec for a small DICOM, TODO coroutine?
				tex.Apply ();
				dicom.setTexture3D (tex);*/

				dicom.setHeader(returnObjectSlice.header);
				dicom.slice = returnObjectSlice.slice;
				mCurrentDICOM = dicom;


				// Unlock:
				isLoading = false;

				returnObjectSlice = null;

				// If an image was loaded successfully, let listeners know:
				if (mCurrentDICOM != null)
				{
					PatientEventSystem.triggerEvent(PatientEventSystem.Event.DICOM_NewLoaded, mCurrentDICOM);
				}

			}
			if (returnObjectVolume != null) {

				DICOMVolume dicom = new DICOMVolume (returnObjectVolume.itkImage);
				dicom.setHeader(returnObjectVolume.header);

				mCurrentDICOMVolume = dicom;

				// Unlock:
				isLoading = false;

				returnObjectVolume = null;

				// If an image was loaded successfully, let listeners know:
				if (mCurrentDICOMVolume != null)
				{
					PatientEventSystem.triggerEvent(PatientEventSystem.Event.DICOM_NewLoadedVolume, mCurrentDICOMVolume );
				}

			}

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_RemoveLoadingJob,
				"DICOM");
		}
	}
}

