using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class DicomDisplayImage : MonoBehaviour, IScrollHandler, IPointerDownHandler, IPointerUpHandler {

	private Material mMaterial;
	private float mMinValue;
	private float mMaxValue;
	private int mLayer;
	private uint mNumberOfLayers;
	private float mFilledPartOfTexture;
	private float initialWidth = 512;
	private float initialHeight = 512;
	/*private Slider mMinSlider;
	private Slider mMaxSlider;
	private Slider mLayerSlider;*/

	// When mDragging is true, moving the mouse will modify the windowing:
	private bool mDragging = false;

	private DICOM currentDICOM;

	// Use this for initialization
	void Awake () {
		mMinValue = 0.0f;
		mMaxValue = 1.0f;
		mLayer = 0;
		mDragging = false;

		mMaterial = GetComponent<RawImage>().material;

		// Remember how large the image was at the beginning:
		initialWidth = GetComponent<RectTransform> ().rect.width;
		initialHeight = GetComponent<RectTransform> ().rect.height;

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
			mDragging = true;
	}
	public void OnPointerUp( PointerEventData eventData )
	{
		if( eventData.button == PointerEventData.InputButton.Left )
			mDragging = false;
	}
	public void Update()
	{
		if (mDragging) {
			InputDeviceManager idm = GameObject.Find ("GlobalScript").GetComponent<InputDeviceManager> ();
			InputDevice inputDevice = idm.currentInputDevice;

			// TODO: Update to new input event system:
			float contrastChange = inputDevice.getTexCoordDelta().x*0.5f;
			float intensityChange = - inputDevice.getTexCoordDelta ().y*0.5f;
			Debug.Log ("Contrast: " + contrastChange + " Intensity: " + intensityChange);

			float newMin = Mathf.Clamp (mMinValue + intensityChange - contrastChange, 0f, 1f);
			float newMax = Mathf.Clamp (mMaxValue + intensityChange + contrastChange, 0f, 1f);
			Debug.Log ("New Intensity: " + newMin + " .. " + newMax );
			MinChanged (newMin);
			MaxChanged (newMax);
		}
	}


	public void MinChanged( float newVal )
	{
		mMinValue = newVal;
		mMaterial.SetFloat ("minValue", mMinValue);
		/*mMinSlider.value = mMinValue;
		if (mMinValue > mMaxValue) {
			mMaxValue = mMinValue;
			mMaterial.SetFloat ("maxValue", mMaxValue);
			mMaxSlider.value = mMaxValue;
		}*/
	}

	public void MaxChanged( float newVal )
	{
		mMaxValue = newVal;
		mMaterial.SetFloat ("maxValue", mMaxValue);
		/*mMaxSlider.value = mMaxValue;
		if (mMaxValue < mMinValue) {
			mMinValue = mMaxValue;
			mMaterial.SetFloat ("minValue", mMinValue);
			mMinSlider.value = mMinValue;
		}*/
	}

	public void LayerChanged( float newVal )
	{
		if (currentDICOM != null) {
			int numLayers = (int)currentDICOM.getHeader ().NumberOfImages;
			//mMaterial.SetFloat ("layer", mLayer*mFilledPartOfTexture);
			mLayer = (int)Mathf.Clamp (newVal, 0, numLayers - 1);
			Debug.Log ("Layer: " + mLayer + " " + (int)currentDICOM.getHeader ().NumberOfImages);

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
		Texture2D tex = dicom.getTexture2D ();
		float newWidth = initialWidth;
		float newHeight = initialHeight;
		float texWidth = tex.width;
		float texHeight = tex.height;
		/*if (texWidth > texHeight) {
			newHeight = texHeight * newWidth / texWidth;
		} else {
			newWidth = texWidth * newHeight / texHeight;
		}*/

		//GetComponent<RectTransform> ().sizeDelta = new Vector2 (newWidth, newHeight);
		Debug.LogWarning("Min, max: " + dicom.getMinimum () + " " + dicom.getMaximum () );
		mMaterial.SetFloat ("globalMaximum", (float)dicom.getMaximum ());
		mMaterial.SetFloat ("globalMinimum", (float)dicom.getMinimum ());
		mMaterial.SetFloat ("range", (float)(dicom.getMaximum () - dicom.getMinimum ()));

		UInt16 value = 1000;
		Debug.Log ("original: " + value);
		float floatValue = value;
		Debug.Log ("floatValue: " + floatValue);

		Color c = new Color ();

		c.r = ((float)value % 256)/256f; value /= 256;
		c.g = ((float)value % 256)/256f; value /= 256;
		c.b = ((float)value % 256)/256f; value /= 256;
		c.a = ((float)value % 256)/256f;

		Debug.Log ("Color: " + c.r + " " + c.g + " " + c.b + " " + c.a);

		const float fromFixed = 256.0f/255f;
		UInt16 result = (UInt16)( 256*(c.r + 256*(c.g + 256*(c.b + 256*c.a))));

		Debug.Log ("reconstructed: " + result);



		GetComponent<RawImage> ().texture = tex;
		//mMaterial.mainTexture = tex;

		//mMaterial.SetFloat ("minValue", mMinValue);
		//mMaterial.SetFloat ("maxValue", mMaxValue);
		/*mMinSlider.value = 0.0f;
		mMaxSlider.value = 1.0f;
		mLayerSlider.value = 0.5f;*/

		mNumberOfLayers = dicom.getHeader ().NumberOfImages;
		// Calculate how much of the texture is actually filled and only display that part:
		mFilledPartOfTexture = (float)mNumberOfLayers/(float)1;
		//LayerChanged( 0.5f );

		Debug.Log ("Size of DICOM: " + tex.width + " " + tex.height);
		currentDICOM = dicom;
	}

	public void FlipHorizontal()
	{
		Rect uvRect = new Rect (GetComponent<RawImage> ().uvRect);
		uvRect.width = -uvRect.width;
		GetComponent<RawImage> ().uvRect = uvRect;
	}
	public void FlipVertical()
	{
		Rect uvRect = new Rect (GetComponent<RawImage> ().uvRect);
		uvRect.height = -uvRect.height;
		GetComponent<RawImage> ().uvRect = uvRect;
	}
}
