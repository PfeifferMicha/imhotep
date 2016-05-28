using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DicomDisplayImage : MonoBehaviour, IScrollHandler {

	private Material mMaterial;
	private float mMinValue;
	private float mMaxValue;
	private float mLayer;
	private float initialWidth = 512;
	private float initialHeight = 512;
	private Slider mMinSlider;
	private Slider mMaxSlider;
	private Slider mLayerSlider;

	// Use this for initialization
	void Start () {
		mMinValue = 0.0f;
		mMaxValue = 1.0f;
		mLayer = 0.0f;
	}
	void Awake() {
		mMaterial = GetComponent<RawImage>().material;


		// Remember how large the image was at the beginning:
		initialWidth = GetComponent<RectTransform> ().rect.width;
		initialHeight = GetComponent<RectTransform> ().rect.height;

		Texture3D tex = new Texture3D (4,4,4, TextureFormat.RGBA32, false);
		// Fill with black:
		Color32[] colors = new Color32[4*4*4];
		for( int i = 0; i < 4*4*4; i ++ )
		{
			colors [i] = Color.black;
		}
		tex.SetPixels32 (colors);
		tex.Apply();

		mMaterial.mainTexture = tex;

		// Set up sliders:
		Transform tf = transform.parent.FindChild("SliderMin");
		mMinSlider = transform.parent.FindChild("SliderMin").GetComponent<Slider>();
		mMaxSlider = transform.parent.FindChild("SliderMax").GetComponent<Slider>();
		mLayerSlider = transform.parent.FindChild("SliderLayer").GetComponent<Slider>();
	}

	public void OnScroll(PointerEventData eventData)
	{
		Texture3D tex = (Texture3D)mMaterial.mainTexture;
		int numLayers = tex.depth;

		mLayer = mLayer + Mathf.Ceil( 2.0f*eventData.scrollDelta.y )/ numLayers;

		mLayerSlider.value = mLayer;
	}

	public void MinChanged( float newVal )
	{
		mMinValue = newVal;
		mMaterial.SetFloat ("minValue", mMinValue);
		if (mMinValue > mMaxValue) {
			mMaxValue = mMinValue;
			mMaterial.SetFloat ("maxValue", mMaxValue);
			mMaxSlider.value = mMaxValue;
		}
	}

	public void MaxChanged( float newVal )
	{
		mMaxValue = newVal;
		mMaterial.SetFloat ("maxValue", mMaxValue);
		if (mMaxValue < mMinValue) {
			mMinValue = mMaxValue;
			mMaterial.SetFloat ("minValue", mMinValue);
			mMinSlider.value = mMinValue;
		}
	}

	public void LayerChanged( float newVal )
	{
		mLayer = newVal;
		mLayer = Mathf.Clamp (mLayer, 0.0f, 1.0f);
		mMaterial.SetFloat ("layer", mLayer);
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

		mMaterial.SetFloat ("globalMaximum", (float)dicom.getMaximum ());
		mMaterial.SetFloat ("globalMinimum", (float)dicom.getMinimum ());
		mMaterial.SetFloat ("range", (float)(dicom.getMaximum () - dicom.getMinimum ()));

		mMaterial.mainTexture = tex;

		//mMaterial.SetFloat ("minValue", mMinValue);
		//mMaterial.SetFloat ("maxValue", mMaxValue);
		mMinSlider.value = 0.0f;
		mMaxSlider.value = 1.0f;
		mLayerSlider.value = 0.5f;
	}
}
