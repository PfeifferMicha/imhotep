using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DicomVolumeControl : MonoBehaviour {

	public GameObject DICOMVolume;
	public GameObject NoVolumeText;
	public GameObject LoadingText;

	public GameObject TextureListEntry;

	public GameObject SideScreenEmpty;
	public GameObject SideScreenMain;
	public GameObject SideScreenTextureList;

	public Image HistogramImage;

	//private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

	// Use this for initialization
	void Start () {
		TextureListEntry.SetActive (false);
	}

	void OnEnable()
	{
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_CloseVolume, eventDicomClosed );
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewLoadedVolume, eventNewDicom );		
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_StartLoadingVolume, eventLoadingStarted );

		// If a DICOM is already loaded, display the transfer function and histogram:
		if (DICOMLoader.instance.currentDICOMVolume != null) {
			eventNewDicom ();
		} else {
			eventDicomClosed ();
		}
	}

	void OnDisable()
	{
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_CloseVolume, eventDicomClosed );
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewLoadedVolume, eventNewDicom );	
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_StartLoadingVolume, eventLoadingStarted );
	}

	void eventNewDicom( object obj = null )
	{
		NoVolumeText.SetActive (false);
		LoadingText.SetActive (false);
		SideScreenEmpty.SetActive (false);
		displayHistogram ();
		displayMainScreen ();
	}

	void eventDicomClosed( object obj = null )
	{
		NoVolumeText.SetActive (true);
		LoadingText.SetActive (false);
		SideScreenEmpty.SetActive (true);

		SideScreenMain.SetActive (false);
		SideScreenTextureList.SetActive (false);
	}

	void eventLoadingStarted( object obj = null )
	{
		NoVolumeText.SetActive (false);
		LoadingText.SetActive (true);
		SideScreenEmpty.SetActive (true);

		SideScreenMain.SetActive (false);
		SideScreenTextureList.SetActive (false);
	}

	public void setTransferFunctionRange( float min, float max )
	{
		DICOMVolume.GetComponent<MeshRenderer>().material.SetFloat ("minimum", min);
		DICOMVolume.GetComponent<MeshRenderer>().material.SetFloat ("maximum", max);
	}

	void setTransferFunction( Texture2D tex )
	{
		DICOMVolume.GetComponent<MeshRenderer>().material.SetTexture ("_TransferFunction", tex);
	}

	void displayHistogram( object obj = null )
	{
		Debug.Log ("Histogram Event");
		Histogram hist = DICOMLoader.instance.currentDICOMVolume.histogram;
		if (hist != null) {
			Texture2D tex = hist.asTexture ();
			Sprite sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0.5f, 0.5f));
			HistogramImage.sprite = sprite;
		}
	}

	// =========================================================
	// Load and handle transfer function textures:

	Texture2D loadTransferFunctionTexture( string path )
	{
		Texture2D tex = new Texture2D (2, 2);
		if (File.Exists (path)) {
			byte[] data = File.ReadAllBytes (path);
			tex.LoadImage (data);
		}
		return tex;
	}

	List<string> getTextureFiles( string directory )
	{
		string[] files = Directory.GetFiles (directory);
		List<string> list = new List<string>();
		foreach( string f in files )
		{
			list.Add( Path.GetFileNameWithoutExtension(f) );
		}
		return list;
	}

	void generateTextureList()
	{
		List<string> filenames = getTextureFiles ("../TransferFunctions/");
		clearTextureList ();
		foreach (string filename in filenames) {
			Texture2D tex = loadTransferFunctionTexture ("../TransferFunctions/" + filename + ".png");

			GameObject listEntry = Instantiate (TextureListEntry) as GameObject;
			listEntry.SetActive (true);
			listEntry.transform.SetParent (TextureListEntry.transform.parent, false);

			Image img = listEntry.transform.Find("Texture").GetComponent<Image> () as Image;
			Sprite sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0.5f, 0.5f));
			img.sprite = sprite;

			Text text = listEntry.GetComponentInChildren<Text> () as Text;
			text.text = filename;

			listEntry.GetComponent<Button> ().onClick.AddListener(() => setTransferFunction( tex ));
			listEntry.GetComponent<Button> ().onClick.AddListener(() => displayMainScreen());
		}
	}


	void clearTextureList()
	{
		foreach (Transform tf in TextureListEntry.transform.parent) {
			if (tf != TextureListEntry.transform) {
				Destroy (tf.gameObject);
			}
		}
	}

	// =========================================================

	public void displayTransferFunctionList()
	{
		generateTextureList ();

		SideScreenMain.SetActive (false);
		SideScreenTextureList.SetActive (true);
		SideScreenEmpty.SetActive (false);
	}

	void displayMainScreen()
	{

		SideScreenTextureList.SetActive (false);
		SideScreenEmpty.SetActive (false);
		
		if (DICOMLoader.instance.currentDICOMVolume != null) {
			Texture2D currentTex = DICOMVolume.GetComponent<MeshRenderer> ().material.GetTexture ("_TransferFunction") as Texture2D;
			Sprite sprite = Sprite.Create (currentTex, new Rect (0, 0, currentTex.width, currentTex.height), new Vector2 (0.5f, 0.5f));
			SideScreenMain.transform.Find ("Background/Texture").GetComponent<Image> ().sprite = sprite;
			SideScreenMain.SetActive (true);
		} else {
			SideScreenMain.SetActive (false);
			SideScreenEmpty.SetActive (true);
		}
	}
	// =========================================================
}