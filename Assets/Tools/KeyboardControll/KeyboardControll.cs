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

	//memorize normalColor of keyboardInputField for ReFocus
	private Color normalColor;


	//Call's by activation
	void OnEnable(){
		this.setAnnotationControllerPosition ();
		this.endTextSelection = keyboardInputField.text.Length;
	}
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
		normalColor = keyboardInputField.colors.normalColor;
	}

	// Update is called once per frame
	void Update () {
		Debug.Log (keyboardInputField.caretPosition);
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
	//Save's the current caret Position
	public void updateCaretPosition(){
		caretPostionKeyboard = keyboardInputField.caretPosition;
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


	//=========Method's for Entering Symbol in the Keyboard-Inputfield===========

	//changes letter's from big to small or small to big
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
		this.reFocusKeyboardInputfield ();
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
		this.reFocusKeyboardInputfield ();
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
		this.reFocusKeyboardInputfield ();
	}
	//Make's linebreak in the keyboard-inputfield
	public void lineBreak(){		
		keyboardInputField.text += System.Text.RegularExpressions.Regex.Unescape ("\n");
		if (selectedInputField != null) {
			selectedInputField.text += System.Text.RegularExpressions.Regex.Unescape ("\n");
		}
		this.caretPostionKeyboard++;
		this.deleteSelectedText ();	
		this.wasTextSelected ();
		this.reFocusKeyboardInputfield ();
	}

	//Delete's the whole text
	public void deleteText()
	{
		if (selectedInputField != null) {
			selectedInputField.text = "";
		}
		keyboardInputField.text = "";
		caretPostionKeyboard = 0;
		this.reFocusKeyboardInputfield ();
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

	//Switches between the different Fields (for numbers/letter/special signs) of the keyboard
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
		this.reFocusKeyboardInputfield ();
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
	//Helps to determ,if the button "DeleteLastSymbol" is pressed down for continously Deletion of the text
	public void buttonDeleteLastSymbolPressingDown(){
		this.buttonDeleteLastSymbolPressedDown = true;
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

	void OnCollisionEnter(Collision collisionInfo){
		Debug.Log ("Collision:"+collisionInfo.collider.name);
	}


	//================Method's for Refocus to the Keyboard-InputField after a Button is clicked=========
	//Set the focus back to the keyboard-Inputfield
	private void reFocusKeyboardInputfield(){
		EventSystem.current.SetSelectedGameObject (keyboardInputField.gameObject);
		keyboardInputField.selectionFocusPosition = 0;
		keyboardInputField.selectionAnchorPosition = 0;
		beginTextSelection = 0;
		endTextSelection = 0;
		keyboardInputField.caretPosition = caretPostionKeyboard;
		StartCoroutine (waitForFrame ());
	}
	public void changeNormalColorIntoHighlightedColorForRefocus(){
		ColorBlock tempBlock = keyboardInputField.colors;
		tempBlock.normalColor = keyboardInputField.colors.highlightedColor;
		keyboardInputField.colors = tempBlock;
		Debug.Log (keyboardInputField.colors.ToString ());
	}
	public void changeNormalColorBackAfterRefocus(){
		ColorBlock tempBlock = keyboardInputField.colors;
		tempBlock.normalColor = normalColor;
		keyboardInputField.colors = tempBlock;
	}
	//used to deselect the text of keyboard-Inputfield
	private IEnumerator waitForFrame(){
		yield return 0;
		keyboardInputField.MoveTextStart (false);
		keyboardInputField.caretPosition = caretPostionKeyboard;
	}
}
