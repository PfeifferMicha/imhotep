using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using itk.simple;

public class DicomCache : MonoBehaviour {

	// All possible Dicom-Related events:
	public enum Event {
		NewDicomList,
		NewDicomLoaded,
		AllCleared
	}

	// Use this for initialization
	void Start () {
		mDicomLoader = new DicomLoaderITK ();
		mPath = "";
	}
	
	// Update is called once per frame
	/*void Update () {
	
	}*/

	public void loadDirectory( string path )
	{
		// Parse the directory:
		mAvailableSeries = mDicomLoader.loadDirectory ( path );
		triggerEvent (Event.NewDicomList);
		mPath = path;

		loadDicom (0);
	}

	public void loadDicom( int id )
	{
		// If at least one DICOM series was found, load it:
		if (mAvailableSeries.Count > id) {
			mCurrentDICOM = mDicomLoader.load ( mPath, mAvailableSeries [id]);
			// If a series was loaded successfully, let listeners know:
			if (mCurrentDICOM != null) {
				triggerEvent (Event.NewDicomLoaded);
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
			mEventDictionary = new Dictionary< Event, UnityEvent >();
		}
	}

	public static void startListening( Event eventType, UnityAction listener )
	{
		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue (eventType, out thisEvent)) {
			thisEvent.AddListener (listener);
		} else {
			thisEvent = new UnityEvent ();
			thisEvent.AddListener (listener);
			instance.mEventDictionary.Add (eventType, thisEvent);
		}
		Debug.Log ("Added event listener for event: " + eventType);
	}
	public static void stopListening( Event eventType, UnityAction listener )
	{
		if (mInstance == null)
			return;
		
		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue (eventType, out thisEvent)) {
			thisEvent.RemoveListener (listener);
		}
		Debug.Log ("Removed event listener for event: " + eventType);
	}
	public static void triggerEvent( Event eventType)
	{
		UnityEvent thisEvent = null;
		// Attempt to get the the UnityEvent from the dictionary. If this succeeds,
		// thisEvent will be filled and the if will evaluate to true:
		if (instance.mEventDictionary.TryGetValue (eventType, out thisEvent)) {
			thisEvent.Invoke ();
			Debug.Log ("Triggered Event:" + eventType);
		}
	}
	public static DICOM getCurrentDicom()
	{
		return instance.mCurrentDICOM;
	}

	public static List<string> getAvailableSeries()
	{
		List<string> list = new List<string>();
		foreach( string s in instance.mAvailableSeries )
		{
			list.Add (s);
		}
		return list;
	}

	private DicomLoaderITK mDicomLoader;
	private static DicomCache mInstance;
	private Dictionary< Event, UnityEvent> mEventDictionary;

	private VectorString mAvailableSeries;
	private DICOM mCurrentDICOM;
	private string mPath;
}
