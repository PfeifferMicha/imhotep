using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScreenshotControl : MonoBehaviour {
	//Show's the timer value, which you set
	public Text showtimerValue;
	//A Slider to change the value of the timer ("showTimerValue")
	public Slider timerSlider;

	//Gameobject of the background, where you can press a button to take a screenshot and set the timer
	public GameObject background;

	//Count's the taken screenshot's: part of the pathname to ensure,that every screenshot has a unique name
	private int imageCounter;
	//Has a Screenshot been taken?
	private bool tookPicture;
	//Gameobject for Showing the Countdown
	public GameObject countdown;

	// Use this for initialization
	void Start () {
		timerSlider.onValueChanged.AddListener(delegate {ValueChangeCheck(); });
		timerSlider.value = 3;
		showtimerValue.text = timerSlider.value.ToString ();
		imageCounter = 1;
		if (this.background != null) {
			this.background = GameObject.Find ("BackgroundScreenshot");
		}
		this.tookPicture = false;
		if (this.countdown != null) {
			//this.countdown = GameObject.Find ("TimerCountdownShowBackground");
		}
	}
	void OnEnable(){
		this.background.SetActive (true);
		this.countdown.SetActive (false);
	}
	// Update is called once per frame
	void Update () {
		
	}
	// Invoked when the value of the slider changes.
	public void ValueChangeCheck()
	{
		showtimerValue.text = timerSlider.value.ToString();
	}

	private void takePicture()
	{
		/*RenderTexture.active = Camera.main.targetTexture;
		Camera.main.Render();
		screenshotImage = new Texture2D(Camera.main.targetTexture.width, Camera.main.targetTexture.height);
		screenshotImage.ReadPixels(new Rect(0, 0, Camera.main.targetTexture.width, Camera.main.targetTexture.height), 0, 0);
		screenshotImage.Apply();*/
		ScreenCapture.CaptureScreenshot ("Assets/Tools/Screenshot/ScreenshotImages/screenshot"+imageCounter+".png");
		imageCounter++;
		InputDeviceManager.instance.shakeLeftController( 0.5f, 0.15f );
	}

	private IEnumerator waitForSecondsAndTakePicture(){
		this.tookPicture = true;
		yield return WaitForSecondsAndShowEverySecond();
		this.takePicture ();

		if (!background.activeSelf) {
			this.gameObject.SetActive (false);
			this.background.SetActive (true);
		}
	}

	public void startCoroutineTakePicture(){
		StartCoroutine (waitForSecondsAndTakePicture ());
	}

	//Wait's the entered Seconds and show's every second the countdown
	private IEnumerator WaitForSecondsAndShowEverySecond(){
		this.countdown.SetActive (true);
		Text show = this.countdown.transform.GetChild (0).gameObject.GetComponent<Text>();
		show.text = timerSlider.value.ToString ();
		Vector3 temp = new Vector3 ();
		temp.Set (Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1f);
		this.countdown.gameObject.transform.position = temp;
		for (int i = (int)timerSlider.value; i >=0 ; i--) {
			show.text = i.ToString ();
			yield return new WaitForSeconds (1);
		}
		this.countdown.SetActive (false);
	}
	public void close(){
		//I never deactivate the whole gameobject, just the background (shows the button to take screenshot) and/or the countdown (shown, when you took a screenshot)
		if (this.tookPicture) {
			this.background.SetActive (false);
			this.tookPicture = false;
		} else {
			this.gameObject.SetActive (false);
		}
	}
}
