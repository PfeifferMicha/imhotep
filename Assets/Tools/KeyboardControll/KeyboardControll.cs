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

	private List<ToolWidget> listofGameObjectsUIToolLayer;

	//Call's by activation
	void OnEnable(){
		this.endTextSelection = keyboardInputField.text.Length;
		//Reset's the position of already active tool, next to the keyboard
		List<ToolWidget> temp = ToolControl.instance.getExistingTools();
		if (temp != null) {	
			for (int i = 0; i < temp.Count; i++) {
				if (temp [i].gameObject.activeSelf) {
					setActiveToolControllerPosition (temp [i].gameObject.GetComponent<Transform>());
				}
			}
		}
		setToBigLetters ();
	}
	// Use this for initialization
	void Start () {
		if (keyboard_letters == null) {
			keyboard_letters = GameObject.FindGameObjectsWithTag ("Keyboard_letter");
		}
		caretPostionKeyboard = 0;
		shift_flag = false;
		activatedField_flag = 0;
		buttonDeleteLastSymbolPressedDown = false;	
		normalColor = keyboardInputField.colors.normalColor;
		this.listofGameObjectsUIToolLayer = ToolControl.instance.getExistingTools ();
	}


	// Update is called once per frame
	void Update () {
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
			setToBigLetters ();
		} else {
			setToSmallLetters ();
		}
		this.reFocusKeyboardInputfield ();
	}
	//Changes letter's to big Letter's independent of the shift_flag
	private void setToBigLetters(){
		shift_flag = false;
		foreach (GameObject buttonletter in keyboard_letters) {
			Text temp = buttonletter.GetComponent<Text> ();
			temp.text = temp.text.ToUpper ();
		}
	}
	//Changes letter's to small Letter's independent of the shift_flag
	private void setToSmallLetters(){
		shift_flag = true;
		foreach (GameObject buttonletter in keyboard_letters) {
			Text temp = buttonletter.GetComponent<Text> ();
			temp.text = temp.text.ToLower();
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
		this.reFocusKeyboardInputfield ();
	}

	//Enter's a given letter
	public void enterLetterEvent(string letter){
		if (shift_flag) {
			this.enterTextEvent (letter.ToLower ());
		} else {
			this.enterTextEvent (letter);
			setToSmallLetters ();
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
	//Make's a linebreak in the keyboard-inputfield
	public void lineBreak(){
		//If the caretPosition is at the end of the text, add the the linebreak	
		if (caretPostionKeyboard == keyboardInputField.text.Length) {			
			keyboardInputField.text += System.Text.RegularExpressions.Regex.Unescape ("\n");
			if (selectedInputField != null) {
				selectedInputField.text += System.Text.RegularExpressions.Regex.Unescape ("\n");
			}
			this.caretPostionKeyboard++;
			this.deleteSelectedText ();	
			this.wasTextSelected ();
		//If the caretPosition is within the text, add the linebreak at that position and move the caretPosition to the end
		} else {
			string temp = keyboardInputField.text.Substring (caretPostionKeyboard);
			keyboardInputField.text = keyboardInputField.text.Remove (caretPostionKeyboard);
			keyboardInputField.text = keyboardInputField.text.Insert (caretPostionKeyboard, System.Text.RegularExpressions.Regex.Unescape ("\n"));
			keyboardInputField.text = keyboardInputField.text.Insert (keyboardInputField.text.Length, temp);
			//Update the new caretPosition
			keyboardInputField.MoveTextEnd (false);
			this.caretPostionKeyboard = keyboardInputField.caretPosition;
			//update the changes to selectedInputField
			if (selectedInputField != null) {
				selectedInputField.text = keyboardInputField.text;
			}
		}
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
		this.setToolControllerPositionBack ();
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
		this.setToolControllerPositionBack ();
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
	//Reset's an active Tool-Position
	private void setActiveToolControllerPosition(Transform tool){
		Rect sideScreenRect = this.gameObject.GetComponentInChildren<RectTransform> ().rect;
		tool.Translate (new Vector3 ((sideScreenRect.width/1000)*2, 0));
	}
	//Reset's an active-Position to the original position
	private void setToolControllerPositionBack(){
		Rect sideScreenRect = this.gameObject.GetComponentInChildren<RectTransform> ().rect;
		for (int i = 0; i < listofGameObjectsUIToolLayer.Count; i++) {
			if (listofGameObjectsUIToolLayer [i].gameObject.activeSelf) {
				listofGameObjectsUIToolLayer [i].gameObject.GetComponent<Transform>().Translate (new Vector3 ((-sideScreenRect.width / 1000) * 2, 0));
			}
		}		
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
