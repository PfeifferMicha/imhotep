using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Responsible for every feature within in the keyboard
public class KeyboardControll : MonoBehaviour{
	//Save's the text, before enter anything else
	public string oldText;
	//selected InputField, which the user clicked to activate the keyboard
	public InputField selectedInputField;
	//InputField, which the user see's of the keyboard
	public InputField keyboardInputField;
	public GameObject annotationControl;
	//save's current caretPostion in the Keyboard-InputField
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
	//Save's the current caret Position
	public void updateCaretPosition(){
		caretPostionKeyboard = keyboardInputField.caretPosition;

	}
	// Update is called once per frame
	void Update () {
		
	}

	//Enter's the text in the InputField of the Keyboard
	public void enterTextEvent( string key )	{
		keyboardInputField.text = createText (keyboardInputField.text, key, caretPostionKeyboard);
		selectedInputField.text = createText (selectedInputField.text, key, caretPostionKeyboard);
		caretPostionKeyboard++;
	}
	//return a String with the inserted "key" at the given caretPostion
	private string createText(string text,string key,int caretPosition){
		return text.Insert(caretPosition,key);
	}
	//Delete's the last Input-Symbol
	public void  deleteLastInputSymbol()
	{
		if (selectedInputField.text.Length >= 1) {
			caretPostionKeyboard--;
			selectedInputField.text = selectedInputField.text.Remove (selectedInputField.text.Length - 1);
			keyboardInputField.text = keyboardInputField.text.Remove (keyboardInputField.text.Length - 1);
		}
	}
	//Delete's the whole text
	public void deleteText()
	{
		selectedInputField.text = "";
		keyboardInputField.text = "";
		caretPostionKeyboard = 0;
	}

	//Cancel the input's, uses the Savecopy("oldText") and deactivate's the keyboard
	public void cancel()
	{
		caretPostionKeyboard = 0;
		selectedInputField.text = oldText;
		keyboardInputField.text = oldText;
		this.gameObject.SetActive (false);
		keyboardInputField.DeactivateInputField ();
		this.setAnnotationControllerPositionBack ();
	}
	//Save's everything and deactivate the keyboard
	public void save()
	{
		this.gameObject.SetActive (false);
		oldText = "";
		keyboardInputField.DeactivateInputField ();
		this.setAnnotationControllerPositionBack ();
	}

	//Call's by activation
	void OnEnable(){
		this.setAnnotationControllerPosition ();
	}

	//Reset's the Annotation-Position
	private void setAnnotationControllerPosition(){
		Rect sideScreenRect = this.gameObject.GetComponentInChildren<RectTransform> ().rect;
		annotationControl.transform.Translate (new Vector3 ((sideScreenRect.width/1000)*2, 0));
	}
	//Reset's the Annotation-Position to the original position
	private void setAnnotationControllerPositionBack(){
		Rect sideScreenRect = this.gameObject.GetComponentInChildren<RectTransform> ().rect;
		annotationControl.transform.Translate (new Vector3 ((-sideScreenRect.width/1000)*2, 0));
	}
}
