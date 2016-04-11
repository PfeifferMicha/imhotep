using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using itk.simple;

public class DicomCache : MonoBehaviour {

	// Use this for initialization
	void Start () {
		mDicomLoader = new DicomLoaderITK ();
	}
	
	// Update is called once per frame
	/*void Update () {
	
	}*/

	public void loadDirectory( string path )
	{
		// Parse the directory:
		mLoadedSeries = mDicomLoader.loadDirectory ( path );
		// If at least one DICOM series was found, load it:
		if (mLoadedSeries.Count > 0) {
			mCurrentDICOM = mDicomLoader.load (path, mLoadedSeries [0]);
			// If a series was loaded successfully, let listeners know:
			if (mCurrentDICOM != null) {
				triggerEvent ("NewDicomLoaded");
			}
		}
	}

	public static DicomCache instance
	{
		get {
			if (!mInstance) {
				mInstance = FindObjectOfType (typeof(DicomCache)) as DicomCache;
				if (!mInstance) {
					Debug.LogError ("There needs to be at least one DicomCache active in the project!");
				}
				mInstance.init ();
			}
			return mInstance;
		}
	}

	void init()
	{
		if( mEventDictionary == null )
		{
			mEventDictionary = new Dictionary< string, UnityEvent >();
		}
	}

	public static void startListening( string eventName, UnityAction listener )
	{
		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue (eventName, out thisEvent)) {
			thisEvent.AddListener (listener);
		} else {
			thisEvent = new UnityEvent ();
			thisEvent.AddListener (listener);
			instance.mEventDictionary.Add (eventName, thisEvent);
		}
		Debug.Log ("Added event listener for event: " + eventName);
	}
	public static void stopListening( string eventName, UnityAction listener )
	{
		if (mInstance == null)
			return;
		
		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue (eventName, out thisEvent)) {
			thisEvent.RemoveListener (listener);
		}
		Debug.Log ("Removed event listener for event: " + eventName);
	}
	public static void triggerEvent( string eventName )
	{
		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue (eventName, out thisEvent)) {
			thisEvent.Invoke ();
			Debug.Log ("Triggered Event:" + eventName);
		}
	}
	public static DICOM getCurrentDicom()
	{
		return instance.mCurrentDICOM;
	}

	private DicomLoaderITK mDicomLoader;
	private static DicomCache mInstance;
	private Dictionary< string, UnityEvent> mEventDictionary;

	private VectorString mLoadedSeries;
	private DICOM mCurrentDICOM;
}
