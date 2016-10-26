using UnityEngine;
using System.Collections;
using System.IO;

public class SaveRenderTexture : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log ("'SaveRenderTexture' active. Press T to save the UI render texture.");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.T))
		{
			Save ();
		}
	}

	void Save() {
		Camera cam = GameObject.Find ("UICamera").GetComponent<Camera> ();
		RenderTexture renderTex = cam.targetTexture;
		RenderTexture.active = renderTex;
		Texture2D tex = new Texture2D (renderTex.width, renderTex.height, TextureFormat.ARGB32, false);
		tex.ReadPixels (new Rect (0, 0, renderTex.width, renderTex.height), 0, 0);
		tex.Apply ();
		RenderTexture.active = null;

		byte[] data = tex.EncodeToPNG ();
		File.WriteAllBytes("renderTexture.png", data);
		Debug.Log ("Saved texture to: 'renderTexture.png'");
	}
}
