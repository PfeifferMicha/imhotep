using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DicomDisplayImage : MonoBehaviour, IScrollHandler {

	// Use this for initialization
	void Start () {
		mMinValue = 0.0f;
		mMaxValue = 1.0f;
		mLayer = 0.0f;
	}
	void Awake() {
		mMaterial = GetComponent<RawImage>().material;

		Transform tf = transform.parent.FindChild("SliderMin");
		Debug.Log (tf);
		mMinSlider = transform.parent.FindChild("SliderMin").GetComponent<Slider>();
		mMaxSlider = transform.parent.FindChild("SliderMax").GetComponent<Slider>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnScroll(PointerEventData eventData)
	{
		Texture3D tex = (Texture3D)mMaterial.mainTexture;
		int numLayers = tex.depth;

		mLayer = mLayer + Mathf.Ceil( 2.0f*eventData.scrollDelta.y )/ numLayers;
		mLayer = Mathf.Clamp (mLayer, 0.0f, 1.0f);
		mMaterial.SetFloat ("layer", mLayer);
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

	private Material mMaterial;
	private float mMinValue;
	private float mMaxValue;
	private float mLayer;
	private Slider mMinSlider;
	private Slider mMaxSlider;
}
