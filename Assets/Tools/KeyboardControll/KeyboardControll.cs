using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeyboardControll : MonoBehaviour{
	public string oldText;
	public InputField selectedInputField;
	public InputField keyboardInputField;
	public GameObject annotationControl;
	//public SteamVR_TrackedObject tracked;
	//public SteamVR_Controller.Device left;

	// Use this for initialization
	void Start () {
		if (annotationControl == null) {
			annotationControl = GameObject.FindWithTag ("AnnotationControl");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void enterTextEvent( string key )
	{
		selectedInputField.text += key;
		keyboardInputField.text += key;
	}
	public void deleteLastInputSymbol()
	{
		if (selectedInputField.text.Length >= 1) {
			selectedInputField.text = selectedInputField.text.Remove (selectedInputField.text.Length - 1);
			keyboardInputField.text = keyboardInputField.text.Remove (keyboardInputField.text.Length - 1);
		}
	}
	public void deleteText()
	{
		selectedInputField.text = "";
		keyboardInputField.text = "";
	}

	public void cancel()
	{
		selectedInputField.text = oldText;
		keyboardInputField.text = oldText;
		this.gameObject.SetActive (false);
		this.setAnnotationControllerPositionBack ();
	}

	public void save()
	{
		this.gameObject.SetActive (false);
		keyboardInputField.text = "";
		this.setAnnotationControllerPositionBack ();
	}

	void OnEnable(){
		this.setAnnotationControllerPosition ();
	}
	private void setAnnotationControllerPosition(){
		Rect sideScreenRect = this.gameObject.GetComponentInChildren<RectTransform> ().rect;
		annotationControl.transform.Translate (new Vector3 (sideScreenRect.width/1000, 0));
	}
	private void setAnnotationControllerPositionBack(){
		Rect sideScreenRect = this.gameObject.GetComponentInChildren<RectTransform> ().rect;
		annotationControl.transform.Translate (new Vector3 (-sideScreenRect.width/1000, 0));
	}
}
