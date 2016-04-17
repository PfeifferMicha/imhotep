using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DicomDisplayImage : MonoBehaviour, IScrollHandler {

	// Use this for initialization
	void Start () {
		mMaterial = GetComponent<RawImage> ().material;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnScroll(PointerEventData eventData)
	{
		Texture3D tex = (Texture3D)mMaterial.mainTexture;
		int numLayers = tex.depth;
		float layer = mMaterial.GetFloat ("layer");
		layer = layer + Mathf.Ceil( 2.0f*eventData.scrollDelta.y )/ numLayers;
		layer = Mathf.Clamp (layer, 0.0f, 1.0f);
		Debug.Log ("layer: " + layer);
		mMaterial.SetFloat ("layer", layer);
	}

	private Material mMaterial;
}
