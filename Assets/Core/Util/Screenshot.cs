using UnityEngine;
using System.Collections;
using System;

public class Screenshot : MonoBehaviour
{
	public GameObject OVcamera;
	public string path = "";
	private bool takeScreen = false;

	private SteamVR_TrackedController device;

	void Start()
	{
		//OVcamera = Camera.main.gameObject;
		device = GetComponent<SteamVR_TrackedController>();
		device.Gripped += Trigger;
	}

	void Trigger(object sender, ClickedEventArgs e)
	{
		takeScreen = true;
	}

	void LateUpdate()
	{

		if (takeScreen)
		{
			takeScreen = false;
			Debug.LogWarning ("Start Capture Screen");
			StartCoroutine(TakeScreenShot());
		}

	}

	// return file name
	string fileName(int width, int height)
	{
		return string.Format("screen_{0}x{1}_{2}.png",
			width, height,
			System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}

	public IEnumerator TakeScreenShot()
	{
		Debug.LogWarning ("Capture Screen");
		yield return new WaitForEndOfFrame();

		Camera camOV = OVcamera.GetComponent<Camera>();

		RenderTexture currentRT = RenderTexture.active;

		RenderTexture.active = camOV.targetTexture;
		camOV.Render();
		Texture2D imageOverview = new Texture2D(camOV.targetTexture.width, camOV.targetTexture.height, TextureFormat.RGB24, false);
		imageOverview.ReadPixels(new Rect(0, 0, camOV.targetTexture.width, camOV.targetTexture.height), 0, 0);
		imageOverview.Apply();
		RenderTexture.active = currentRT;


		// Encode texture into PNG
		byte[] bytes = imageOverview.EncodeToPNG();

		// save in memory
		string filename = fileName(Convert.ToInt32(imageOverview.width), Convert.ToInt32(imageOverview.height));
		path = filename;

		System.IO.File.WriteAllBytes(path, bytes);
	}
}