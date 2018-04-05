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
	//flag for big/little letters; false = big letter
	private bool shift_flag;
	//0:letterField is activated; 1:numberField is activated; 2:specialSignField is activated
	private int activatedField_flag;

	private float keyDeleteTimer = -1;
	private double keyDeleteThreshold = 0.5;
	private bool buttonDeleteLastSymbolPressedDown;

	//Begin of the selected text of the Keyboard-Inputfield
	private int beginTextSelection;
	//End of the selected text of the Keyboard-Inputfield
	private int endTextSelection;
	public GameObject numbersField;
	public GameObject specialSignsField;
	public GameObject lettersField;
	private GameObject[] keyboard_letters;

	// Use this for initialization
	void Start () {
		if (annotationControl == null) {
			annotationControl = GameObject.FindWithTag ("AnnotationControl");
		}
		if (keyboard_letters == null) {
			keyboard_letters = GameObject.FindGameObjectsWithTag ("Keyboard_letter");
		}
		caretPostionKeyboard = 0;
		shift_flag = false;
		activatedField_flag = 0;
		buttonDeleteLastSymbolPressedDown = false;	
	}
	//Save's the current caret Position
	public void updateCaretPosition(){
		caretPostionKeyboard = keyboardInputField.caretPosition;
		Debug.Log ("Caret: " + caretPostionKeyboard);
	}
	// Update is called once per frame
	void Update () {
		//Debug.Log ("End of Selection: "+keyboardInputField.selectionFocusPosition);
		//Debug.Log ("Begin of Selection: "+keyboardInputField.selectionAnchorPosition);
		// Check if we clicked somewhere outside of the 
		if (InputDeviceManager.instance.currentInputDevice.isLeftButtonDown ()) {	
				// Get currently hovered game object:
				HierarchicalInputModule inputModule = EventSystem.current.currentInputModule as HierarchicalInputModule;
				GameObject hover = inputModule.getPointerData ().pointerCurrentRaycast.gameObject;
				if (hover == null || LayerMask.LayerToName (hover.layer).CompareTo("UITool")!=0) {
					this.cancel ();
				}
			}
		//if the DeleteLastSysmbolbutton is pressed down, the last symbols will be continuiously deleted
		if(InputDeviceManager.instance.currentInputDevice.isLeftButtonDown () & buttonDeleteLastSymbolPressedDown) {
			keyDeleteTimer += Time.deltaTime;
			if( keyDeleteTimer > keyDeleteThreshold )
			{
				deleteLastInputSymbol();
				keyDeleteTimer = 0;
				keyDeleteThreshold = 0.08;
			}
		} else {
			keyDeleteThreshold = 0.5;
			keyDeleteTimer = 0;
			this.buttonDeleteLastSymbolPressedDown = false;
		}


	}
	//Verifies, if a text within the Keyboard-Inpufield is selected
	public void wasTextSelected(){
		this.beginTextSelection = Mathf.Min (keyboardInputField.selectionFocusPosition, keyboardInputField.selectionAnchorPosition);
		this.endTextSelection = Mathf.Max (keyboardInputField.selectionFocusPosition, keyboardInputField.selectionAnchorPosition);
	}
	//Remove's currently the whole text, if a text is selected
	private void deleteSelectedText(){
		if (this.beginTextSelection-this.endTextSelection!=0 && this.endTextSelection>0) {
			keyboardInputField.text = "";
			if (selectedInputField != null) {
					selectedInputField.text="";
			}
			this.caretPostionKeyboard = 0;
		}
	}
	public void setShift_Flag(){
		if (shift_flag) {
			shift_flag = false;
			foreach (GameObject buttonletter in keyboard_letters) {
				Text temp = buttonletter.GetComponent<Text> ();
				temp.text = temp.text.ToUpper ();
			}
		} else {
			shift_flag = true;
			foreach (GameObject buttonletter in keyboard_letters) {
				Text temp = buttonletter.GetComponent<Text> ();
				temp.text = temp.text.ToLower();
			}
		}
	}
	//Enter's the text at the given caretPostion in the InputField of the Keyboard
	public void enterTextEvent(string key )	{
		this.deleteSelectedText ();
		this.wasTextSelected ();
		keyboardInputField.text = keyboardInputField.text.Insert(caretPostionKeyboard,key);
		if (selectedInputField != null) {
			selectedInputField.text = selectedInputField.text.Insert (caretPostionKeyboard, key);
		}
		caretPostionKeyboard++;
	}


	//Enter's a given letter
	public void enterLetterEvent(string letter){
		if (shift_flag) {
			this.enterTextEvent (letter.ToLower ());
		} else {
			this.enterTextEvent (letter);
		}
	}

	//Delete's the last Input-Symbol
	public void  deleteLastInputSymbol()
	{
		Debug.Log ("Text: " + keyboardInputField.text);
		Debug.Log ("Textlänge:" + keyboardInputField.text.Length);
		this.deleteSelectedText ();
		this.wasTextSelected ();
		if (caretPostionKeyboard > 0) {			
			string temp = keyboardInputField.text;
			string newText = temp.Substring (0, caretPostionKeyboard - 1) + temp.Substring (caretPostionKeyboard);
			keyboardInputField.text = keyboardInputField.text.Replace (keyboardInputField.text, newText);
			if (selectedInputField != null) {
				selectedInputField.text = selectedInputField.text.Replace (selectedInputField.text, newText);
			}
			caretPostionKeyboard--;
		}
	}
	public void buttonDeleteLastSymbolPressingDown(){
		this.buttonDeleteLastSymbolPressedDown = true;
	}
	//Delete's the whole text
	public void deleteText()
	{
		if (selectedInputField != null) {
			selectedInputField.text = "";
		}
		keyboardInputField.text = "";
		caretPostionKeyboard = 0;
	}

	//Cancel the input's, uses the Savecopy("oldText") and deactivate's the keyboard
	public void cancel()
	{
		caretPostionKeyboard = 0;
		if (selectedInputField != null) {
			selectedInputField.text = oldText;
		}
		keyboardInputField.text = oldText;
		this.gameObject.SetActive (false);
		keyboardInputField.DeactivateInputField ();
		this.setAnnotationControllerPositionBack ();
	}
	//Save's everything and deactivate the keyboard
	public void save()
	{
		caretPostionKeyboard = 0;
		if (selectedInputField != null) {
			oldText = selectedInputField.text;
		}
		keyboardInputField.DeactivateInputField ();
		this.gameObject.SetActive (false);
		this.setAnnotationControllerPositionBack ();
	}

	//Call's by activation
	void OnEnable(){
		this.setAnnotationControllerPosition ();
		this.endTextSelection = keyboardInputField.text.Length;
	}

	public void lineBreak(){		
		keyboardInputField.text += "\n";
		if (selectedInputField != null) {
			selectedInputField.text += "\n";
		}
		this.caretPostionKeyboard++;
		this.deleteSelectedText ();	
		this.wasTextSelected ();
	}
	public void switchToField(int switchToField){
		switch (activatedField_flag) {
		case 0:
		default:
			lettersField.SetActive (false);
			break;
		case 1:
			numbersField.SetActive (false);
			break;
		case 2:
			specialSignsField.SetActive (false);
			break;
		}
		this.switcher (switchToField);
	}
	private void switcher(int switchToField){
		switch (switchToField) {
		case 0:
		default:
			lettersField.SetActive (true);
			break;
		case 1:
			numbersField.SetActive (true);
			break;
		case 2:
			specialSignsField.SetActive (true);
			break;
		}
		this.activatedField_flag = switchToField;
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
