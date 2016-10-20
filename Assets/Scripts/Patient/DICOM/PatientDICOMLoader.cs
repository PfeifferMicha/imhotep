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
	//! DICOM loader instance:
	private DicomLoaderITK mDicomLoader = new DicomLoaderITK ();

	//! Already loaded DICOM:
	private DICOM mCurrentDICOM = null;


	//! Simple lock, used to prevent loading multiple directory or DICOMs at the same time:
	private bool isLoading = false;

	//! Path which should be searched:
	private string PathForThread = "";
	//! ID of the DICOM in list which should be loaded:
    private int DicomIDForThread = 0;
	private int DicomSliceForThread = 0;
    private DICOMLoadReturnObject returnObject = null;
    private bool loadingFinished = false;
    private bool loadingDirectoryFinished = false;

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

    public void loadDicom( int id )
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

            /*// If there was a series found with the given ID, laod it:
            if (mAvailableSeries.Count > DicomIDForThread)
            {
                mCurrentDICOM = mDicomLoader.load(mPath, mAvailableSeries[DicomIDForThread]);
            }

            if (mCurrentDICOM != null)
            {
                PatientEventSystem.triggerEvent(PatientEventSystem.Event.DICOM_NewLoaded);
            }
            // Unlock:
            isLoading = false;*/

        }
	}


	public void loadDicomSlice( int slice )
	{
		// Load the slice from the previously loaded DICOM:
		loadDicomSlice( DicomIDForThread, slice );
	}

	public void loadDicomSlice( int id, int slice )
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

			/*// If there was a series found with the given ID, laod it:
            if (mAvailableSeries.Count > DicomIDForThread)
            {
                mCurrentDICOM = mDicomLoader.load(mPath, mAvailableSeries[DicomIDForThread]);
            }

            if (mCurrentDICOM != null)
            {
                PatientEventSystem.triggerEvent(PatientEventSystem.Event.DICOM_NewLoaded);
            }
            // Unlock:
            isLoading = false;*/

		}
	}

    private void loadDicomWorker(object sender, DoWorkEventArgs e)
    {
		//List<string> series = mDicomLoader.getAvailableSeries ();
		if (DicomSliceForThread >= 0) {
			returnObject = mDicomLoader.loadSlice (DicomIDForThread, DicomSliceForThread);
		} else {
			returnObject = mDicomLoader.load (DicomIDForThread);
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

	public DICOM getCurrentDicom()
	{
		return mCurrentDICOM;
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

			if(returnObject != null) {

				DICOM dicom = new DICOM();

				if (returnObject.texDepth == 1) {		// depth of 1 voxels means it's just a texture, i.e. 2D!
					Texture2D tex = new Texture2D (returnObject.texWidth, returnObject.texHeight, TextureFormat.ARGB32, false, true);
					tex.SetPixels32 (returnObject.colors);
					tex.Apply ();
					dicom.setTexture2D(tex);

					/*Color32[] cols = tex.GetPixels32 ();
					Color32 col = cols[0];

					Debug.LogError ("Texture: " + col.r + " " + col.g + " " + col.b + " " + col.a);
					Debug.LogError ("Result: " + (col.r + 256*(col.g + 256*(col.b + 256*col.a))));
					Debug.LogError (
						SystemInfo.SupportsTextureFormat (TextureFormat.RGBAFloat));*/

				} else {
					Texture3D tex = new Texture3D (returnObject.texWidth, returnObject.texHeight, returnObject.texDepth, TextureFormat.ARGB32, false);
					//tex.SetPixels (returnObject.colors); //needs around 0.15 sec for a small DICOM, TODO coroutine?
					tex.Apply ();
					dicom.setTexture3D (tex);
				}

				dicom.setHeader(returnObject.header);
				dicom.slice = returnObject.slice;
				//dicom.setMaximum(returnObject.maxCol);
				//dicom.setMinimum(returnObject.minCol);
				//dicom.setMinimum(0);
				mCurrentDICOM = dicom;

				// If a series was loaded successfully, let listeners know:
				if (mCurrentDICOM != null)
				{
					PatientEventSystem.triggerEvent(PatientEventSystem.Event.DICOM_NewLoaded);
				}

				// Unlock:
				isLoading = false;

				returnObject = null;
			}

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_RemoveLoadingJob,
				"DICOM");
		}
	}
}

