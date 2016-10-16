using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class DicomDisplayImage : MonoBehaviour, IScrollHandler, IPointerDownHandler, IPointerUpHandler {

	private Material mMaterial;
	//private float mMinValue;
	//private float mMaxValue;
	private float mLevel;
	private float mWindow;
	private int mLayer;
	private bool flipHorizontal = true;
	private bool flipVertical = true;

	// Positioning:
	private float panX = 0;
	private float panY = 0;
	private float zoom = 1;
	/*private Slider mMinSlider;
	private Slider mMaxSlider;
	private Slider mLayerSlider;*/

	// When dragLevelWindow is true, moving the mouse will modify the windowing:
	private bool dragLevelWindow = false;
	// When dragPan is true, moving the mouse will modify the position:
	private bool dragPan = false;
	// When dragZoom is true, moving the mouse will modify the position:
	private bool dragZoom = false;

	private DICOM currentDICOM;

	// Use this for initialization
	void Awake () {
		//mMinValue = 0.0f;
		//mMaxValue = 1.0f;
		mLevel = 0.5f;
		mWindow = 1f;
		mLayer = 0;
		dragLevelWindow = false;

		mMaterial = new Material (Shader.Find ("Unlit/DICOM2D"));
		GetComponent<RawImage> ().material = mMaterial;

		//clear ();
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (currentDICOM != null) {
			//int numLayers = (int)currentDICOM.getHeader ().NumberOfImages;

			Debug.Log ("ScrollDelta:" + eventData.scrollDelta.y + " " + eventData.scrollDelta.x);
			LayerChanged (mLayer + Mathf.Sign( eventData.scrollDelta.y ));
		}
		//mLayerSlider.value = mLayer;
	}
	public void OnPointerDown( PointerEventData eventData )
	{
		if( eventData.button == PointerEventData.InputButton.Left )
			dragLevelWindow = true;
		else if( eventData.button == PointerEventData.InputButton.Right )
			dragPan = true;
		else if( eventData.button == PointerEventData.InputButton.Middle )
			dragZoom = true;
	}
	public void OnPointerUp( PointerEventData eventData )
	{
		if( eventData.button == PointerEventData.InputButton.Left )
			dragLevelWindow = false;
		else if( eventData.button == PointerEventData.InputButton.Right )
			dragPan = false;
		else if( eventData.button == PointerEventData.InputButton.Middle )
			dragZoom = false;
	}
	public void Update()
	{
		if (dragLevelWindow) {
			InputDeviceManager idm = GameObject.Find ("GlobalScript").GetComponent<InputDeviceManager> ();
			InputDevice inputDevice = idm.currentInputDevice;

			// TODO: Update to new input event system:
			float intensityChange = -inputDevice.getTexCoordDelta ().y * 0.25f;
			float contrastChange = inputDevice.getTexCoordDelta ().x * 0.5f;

			setLevel (mLevel + intensityChange);
			setWindow (mWindow + contrastChange);
			//MinChanged (
			//MaxChanged (newMax);
		}
		if (dragPan) {
			InputDeviceManager idm = GameObject.Find ("GlobalScript").GetComponent<InputDeviceManager> ();
			InputDevice inputDevice = idm.currentInputDevice;

			float dX = -inputDevice.getTexCoordDelta ().x;
			float dY = -inputDevice.getTexCoordDelta ().y;
			if (flipHorizontal)
				dX = -dX;
			if (flipVertical)
				dY = -dY;

			panX += dX*zoom;
			panY += dY*zoom;

			ApplyScaleAndPosition ();
		}
		if (dragZoom) {
			InputDeviceManager idm = GameObject.Find ("GlobalScript").GetComponent<InputDeviceManager> ();
			InputDevice inputDevice = idm.currentInputDevice;

			float dY = -inputDevice.getTexCoordDelta ().y * 0.5f;

			zoom = Mathf.Clamp (zoom + dY, 0.1f, 5f);

			ApplyScaleAndPosition ();
		}
	}

	public void setLevel( float newLevel )
	{
		if (mMaterial == null)
			return;

		mLevel = Mathf.Clamp (newLevel, -0.5f, 1.5f);
		mMaterial.SetFloat ("level", mLevel);
	}

	public void setWindow( float newWindow )
	{
		if (mMaterial == null)
			return;

		mWindow = Mathf.Clamp (newWindow, 0f, 1f);
		mMaterial.SetFloat ("window", mWindow);
	}


	/*public void MinChanged( float newVal )
	{
		if (mMaterial == null)
			return;
		mMinValue = newVal;
		mMaterial.SetFloat ("minValue", mMinValue);
	}

	public void MaxChanged( float newVal )
	{
		if (mMaterial == null)
			return;
		mMaxValue = newVal;
		mMaterial.SetFloat ("maxValue", mMaxValue);
	}*/

	public void LayerChanged( float newVal )
	{
		if (currentDICOM != null) {
			int numLayers = (int)currentDICOM.getHeader ().NumberOfImages;
			//mMaterial.SetFloat ("layer", mLayer*mFilledPartOfTexture);
			mLayer = (int)Mathf.Clamp (newVal, 0, numLayers - 1);
			Debug.Log ("Layer: " + mLayer + "/" + (int)currentDICOM.getHeader ().NumberOfImages);

			PatientDICOMLoader mPatientDICOMLoader = GameObject.Find("GlobalScript").GetComponent<PatientDICOMLoader>();
			mPatientDICOMLoader.loadDicomSlice ( mLayer );
		}
	}

	public float frac( float val )
	{
		return val - Mathf.Floor (val);
	}

	public void SetDicom( DICOM dicom )
	{
		if (mMaterial == null)
			return;
		Texture2D tex = dicom.getTexture2D ();

		mLayer = dicom.slice;
		//GetComponent<RectTransform> ().sizeDelta = new Vector2 (newWidth, newHeight);
		/*Debug.LogWarning("Min, max: " + dicom.getMinimum () + " " + dicom.getMaximum () );
		mMaterial.SetFloat ("globalMaximum", (float)dicom.getMaximum ());
		mMaterial.SetFloat ("globalMinimum", (float)dicom.getMinimum ());
		mMaterial.SetFloat ("range", (float)(dicom.getMaximum () - dicom.getMinimum ()));*/

		mMaterial.SetFloat ("globalMinimum", (float)dicom.getHeader().MinPixelValue);
		mMaterial.SetFloat ("globalMaximum", (float)dicom.getHeader().MaxPixelValue);

		GetComponent<RawImage> ().texture = tex;

		currentDICOM = dicom;

		ApplyScaleAndPosition ();
	}

	public void ApplyScaleAndPosition()
	{
		Texture2D tex = GetComponent<RawImage> ().texture as Texture2D;

		float scaleW = 1f;
		float scaleH = 1f;
		if (tex.width > tex.height) {
			scaleH = (float)tex.width / (float)tex.height;
		} else {
			scaleW = (float)tex.height / (float)tex.width;
		}
		if (flipHorizontal)
			scaleW = scaleW * -1;
		if (flipVertical)
			scaleH = scaleH * -1;

		float oX = panX;
		float oY = panY;

		Rect uvRect = GetComponent<RawImage> ().uvRect;
		uvRect.size = new Vector2 (scaleW*zoom, scaleH*zoom);
		uvRect.center = new Vector2 (0.5f + oX, 0.5f + oY);
		GetComponent<RawImage> ().uvRect = uvRect;
	}

	public void FlipHorizontal()
	{
		Rect uvRect = GetComponent<RawImage> ().uvRect;
		uvRect.width = -uvRect.width;
		GetComponent<RawImage> ().uvRect = uvRect;
	}
	public void FlipVertical()
	{
		Rect uvRect = GetComponent<RawImage> ().uvRect;
		uvRect.height = -uvRect.height;
		GetComponent<RawImage> ().uvRect = uvRect;
	}
}
