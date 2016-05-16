using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using itk.simple;
using System;


/*! This class is respondible for parsing DICOM directories and loading the images.
 * When events happen, other modules are notified using the PatientEventSystem class. */
public class PatientDICOMLoader : MonoBehaviour
{
	//! DICOM loader instance:
	private DicomLoaderITK mDicomLoader = new DicomLoaderITK ();

	//! Available series found in the folder:
	private VectorString mAvailableSeries = new VectorString ();

	//! Already loaded DICOM:
	private DICOM mCurrentDICOM = null;

	//! Path which should be searched:
	private string mPath = "";

	//! Simple lock, used to prevent loading multiple directory or DICOMs at the same time:
	private bool isLoading = false;

    private int DicomIDForThread = 0;
    private DICOMLoadReturnObject returnObject = null;
    private bool loadingFinished = false;
    private bool loadingDirectoryFinished = false;

    // Update is called once per frame
    void Update()
    {
        if (loadingFinished)
        {
            loadingFinished = false;

            if(returnObject != null) {
                DateTime begin = DateTime.Now;                
                Texture3D tex = new Texture3D(returnObject.texWidth, returnObject.texHeight, returnObject.texDepth, TextureFormat.RGBA32, false);
                tex.SetPixels(returnObject.colors); //needs around 0.15 sec for a small DICOM, TODO coroutine?
                tex.Apply();

                DICOM dicom = new DICOM();
                dicom.setTexture(tex);
                dicom.setHeader(returnObject.header);
                dicom.setMaximum((UInt32)returnObject.maxCol);
                dicom.setMinimum((UInt32)returnObject.minCol);
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

        if (loadingDirectoryFinished)
        {
            loadingDirectoryFinished = false;

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_RemoveLoadingJob,
				"DICOM search");

            PatientEventSystem.triggerEvent(PatientEventSystem.Event.DICOM_NewList);

            // Unlock:
            isLoading = false;

            loadDicom(0);
        }

    }



    public void loadDirectory( string path )
	{
		if (!isLoading) {

			// Lock:
			isLoading = true;

			mPath = path;

            ThreadUtil t = new ThreadUtil(loadDirectoryWorker, loadDirectoryCallback);
            t.Run();


			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_AddLoadingJob,
				"DICOM search");

            /*
            // Parse the directory:
            mAvailableSeries = mDicomLoader.loadDirectory (path);
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.DICOM_NewList);

			// Unlock:
			isLoading = false;

			loadDicom (0);
            */
		}
	}

    private void loadDirectoryWorker(object sender, DoWorkEventArgs e)
    {
        // Parse the directory:
        mAvailableSeries = mDicomLoader.loadDirectory(mPath);
    }

    private void loadDirectoryCallback(object sender, RunWorkerCompletedEventArgs e)
    {
        loadingDirectoryFinished = true;
    }

    public void loadDicom( int id )
	{

        if (!isLoading) {
			// Lock:
			isLoading = true;

            DicomIDForThread = id;

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
        // If there was a series found with the given ID, laod it:
        if (mAvailableSeries.Count > DicomIDForThread)
        {
            returnObject = mDicomLoader.load(mPath, mAvailableSeries[DicomIDForThread]);
        }
    }

    private void loadDicomCallback(object sender, RunWorkerCompletedEventArgs e)
    {
        loadingFinished = true;
    }



    public List<string> getAvailableSeries()
	{
		List<string> list = new List<string>();
		foreach( string s in mAvailableSeries )
		{
			list.Add (s);
		}
		return list;
	}

	public DICOM getCurrentDicom()
	{
		return mCurrentDICOM;
	}
}

