using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DicomDisplayImage : MonoBehaviour, IScrollHandler, IPointerDownHandler, IPointerUpHandler {

	private Material mMaterial;
	private float mMinValue;
	private float mMaxValue;
	private float mLayer;
	private uint mNumberOfLayers;
	private float mFilledPartOfTexture;
	private float initialWidth = 512;
	private float initialHeight = 512;
	/*private Slider mMinSlider;
	private Slider mMaxSlider;
	private Slider mLayerSlider;*/

	// When mDragging is true, moving the mouse will modify the windowing:
	private bool mDragging = false;

	// Use this for initialization
	void Awake () {
		mMinValue = 0.0f;
		mMaxValue = 1.0f;
		mLayer = 0.0f;
		mDragging = false;

		mMaterial = GetComponent<RawImage>().material;

		// Remember how large the image was at the beginning:
		initialWidth = GetComponent<RectTransform> ().rect.width;
		initialHeight = GetComponent<RectTransform> ().rect.height;

		//clear ();
	}

	public void OnScroll(PointerEventData eventData)
	{
		Texture3D tex = (Texture3D)mMaterial.mainTexture;
		int numLayers = tex.depth;

		mLayer = Mathf.Clamp (mLayer + Mathf.Ceil (2.0f * eventData.scrollDelta.y) / numLayers, 0, 1);

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
		mLayer = newVal;
		mLayer = Mathf.Clamp (mLayer, 0.0f, 1.0f);
		mMaterial.SetFloat ("layer", mLayer*mFilledPartOfTexture);
	}

	public void SetDicom( DICOM dicom )
	{
		Texture3D tex = dicom.getTexture ();
		float newWidth = initialWidth;
		float newHeight = initialHeight;
		float texWidth = tex.width;
		float texHeight = tex.height;
		if (texWidth > texHeight) {
			newHeight = texHeight * newWidth / texWidth;
		} else {
			newWidth = texWidth * newHeight / texHeight;
		}

		GetComponent<RectTransform> ().sizeDelta = new Vector2 (newWidth, newHeight);

		Debug.Log (mMaterial);
		Debug.Log (dicom);
		mMaterial.SetFloat ("globalMaximum", (float)dicom.getMaximum ());
		mMaterial.SetFloat ("globalMinimum", (float)dicom.getMinimum ());
		mMaterial.SetFloat ("range", (float)(dicom.getMaximum () - dicom.getMinimum ()));

		mMaterial.mainTexture = tex;

		//mMaterial.SetFloat ("minValue", mMinValue);
		//mMaterial.SetFloat ("maxValue", mMaxValue);
		/*mMinSlider.value = 0.0f;
		mMaxSlider.value = 1.0f;
		mLayerSlider.value = 0.5f;*/

		mNumberOfLayers = dicom.getHeader ().numberOfImages;
		// Calculate how much of the texture is actually filled and only display that part:
		mFilledPartOfTexture = (float)mNumberOfLayers/(float)tex.depth;
		LayerChanged( 0.5f );
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
