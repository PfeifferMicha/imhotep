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
	private int caretPostionKeyboard;
	//public SteamVR_TrackedObject tracked;
	//public SteamVR_Controller.Device left;

	// Use this for initialization
	void Start () {
		if (annotationControl == null) {
			annotationControl = GameObject.FindWithTag ("AnnotationControl");
		}
		caretPostionKeyboard = 0;
	}
	public void updateCaretPosition(){
		caretPostionKeyboard = keyboardInputField.caretPosition;

	}
	// Update is called once per frame
	void Update () {
		
	}

	public void enterTextEvent( string key )	{
		keyboardInputField.text = createText (keyboardInputField.text, key, caretPostionKeyboard);
		selectedInputField.text = createText (selectedInputField.text, key, caretPostionKeyboard);
		caretPostionKeyboard++;
	}
	private string createText(string text,string key,int caredPosition){
		return text.Insert(caredPosition,key);
	}
	public void deleteLastInputSymbol()
	{
		if (selectedInputField.text.Length >= 1) {
			caretPostionKeyboard--;
			selectedInputField.text = selectedInputField.text.Remove (selectedInputField.text.Length - 1);
			keyboardInputField.text = keyboardInputField.text.Remove (keyboardInputField.text.Length - 1);
		}
	}
	public void deleteText()
	{
		selectedInputField.text = "";
		keyboardInputField.text = "";
		caretPostionKeyboard = 0;
	}

	public void cancel()
	{
		caretPostionKeyboard = 0;
		selectedInputField.text = oldText;
		keyboardInputField.text = oldText;
		this.gameObject.SetActive (false);
		keyboardInputField.DeactivateInputField ();
		this.setAnnotationControllerPositionBack ();
	}

	public void save()
	{
		this.gameObject.SetActive (false);
		keyboardInputField.text = "";
		keyboardInputField.DeactivateInputField ();
		this.setAnnotationControllerPositionBack ();
	}

	void OnEnable(){
		this.setAnnotationControllerPosition ();
	}
	private void setAnnotationControllerPosition(){
		Rect sideScreenRect = this.gameObject.GetComponentInChildren<RectTransform> ().rect;
		annotationControl.transform.Translate (new Vector3 ((sideScreenRect.width/1000)*2, 0));
	}
	private void setAnnotationControllerPositionBack(){
		Rect sideScreenRect = this.gameObject.GetComponentInChildren<RectTransform> ().rect;
		annotationControl.transform.Translate (new Vector3 ((-sideScreenRect.width/1000)*2, 0));
	}
}
