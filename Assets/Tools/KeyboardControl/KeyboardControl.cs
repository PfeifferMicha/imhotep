using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*!Manages the the following keyboardfeature's. 
 * - Three field's: numberfield, specialletterfield and letterfield. 
 * - Delete the whole text. 
 * - Delete one symbol at a time. 
 * - Delete's faster, if you hold the delete button.
 * - Consider marked text, if you press a button and delete's the text. 
 * - You can jump to a certain caret-position.
 * - You can save and cancel the text.
 * 
 * At that moment, you can't select text.
 * 
 * In order to use the keyboard, also the keyboardListener is requierd.
*/
public class KeyboardControl : MonoBehaviour{
	//Save's the text, before enter anything else
	public string oldText;
	//selected InputField, which the user clicked to activate the keyboard
	public InputField selectedInputField;
	//InputField, which the user see's of the keyboard
	public InputField keyboardInputField;

	//save's current caretPostion in the Keyboard-InputField
	private int caretPostionKeyboard;
	//flag for big/little letters; false = big letter
	private bool shift_flag;
	//0:letterField is activated; 1:numberField is activated; 2:specialSignField is activated
	private int activatedField_flag;

	//keyDeleteTimer, keyDeleteThreshold and buttonDeleteLastSymbolPressedDown are used, if the user hold's the delete Button to delete the text faster
	private float keyDeleteTimer = -1;
	private double keyDeleteThreshold = 0.5;
	private bool buttonDeleteLastSymbolPressedDown;

	//Begin of the selected text of the Keyboard-Inputfield
	private int beginTextSelection;
	//End of the selected text of the Keyboard-Inputfield
	private int endTextSelection;

	//the keyboard has 3 different fields
	public GameObject numbersField;
	public GameObject specialSignsField;
	public GameObject lettersField;

	//represent all letter's on the keyboard
	private GameObject[] keyboard_letters;

	//memorize normalColor of keyboardInputField for ReFocus
	private Color normalColor;

	//Call's by activation
	void OnEnable(){
		this.endTextSelection = keyboardInputField.text.Length;
		//Reset's the position of already active tool, next to the keyboard
		//this.setActiveToolControllerPosition();
		this.setToBigLetters ();

		ToolControl.instance.overrideTool ( this.gameObject );

		InputDeviceManager.instance.shakeLeftController( 0.5f, 0.15f );
	}

	void OnDisable(){
		ToolControl.instance.unoverrideTool ();
		this.cancel ();
	}
	// Use this for initialization
	void Start () {
		if (this.keyboard_letters == null) {
			this.keyboard_letters = GameObject.FindGameObjectsWithTag ("Keyboard_letter");
		}
		this.caretPostionKeyboard = 0;
		this.shift_flag = false;
		this.activatedField_flag = 0;
		this.buttonDeleteLastSymbolPressedDown = false;	
		this.normalColor = this.keyboardInputField.colors.normalColor;
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
		if(InputDeviceManager.instance.currentInputDevice.isLeftButtonDown () & this.buttonDeleteLastSymbolPressedDown) {
			this.keyDeleteTimer += Time.deltaTime;
			if( this.keyDeleteTimer > this.keyDeleteThreshold )
			{
				this.deleteLastInputSymbol();
				this.keyDeleteTimer = 0;
				this.keyDeleteThreshold = 0.08;
			}
		} else {
			this.keyDeleteThreshold = 0.5;
			this.keyDeleteTimer = 0;
			this.buttonDeleteLastSymbolPressedDown = false;
		}
		// If the selected input field is no longer active, cancel input:
		if (selectedInputField != null) {
			if (!selectedInputField.transform.parent.gameObject.activeInHierarchy) {
				this.cancel ();
			}
		}
	}

	//Helps to determ,if the button "DeleteLastSymbol" is pressed down for continously Deletion of the text
	public void buttonDeleteLastSymbolPressingDown(){
		this.buttonDeleteLastSymbolPressedDown = true;
	}

	//Save's the current caret Position
	public void updateCaretPosition(){
		this.caretPostionKeyboard = this.keyboardInputField.caretPosition;
	}
	//Verifies, if a text within the Keyboard-Inpufield is selected
	private void updateTextSelected(){
		this.beginTextSelection = Mathf.Min (this.keyboardInputField.selectionFocusPosition, this.keyboardInputField.selectionAnchorPosition);
		this.endTextSelection = Mathf.Max (this.keyboardInputField.selectionFocusPosition, this.keyboardInputField.selectionAnchorPosition);
	}
	//Remove's currently the whole text, if a text is selected
	private void deleteSelectedText(){
		if (this.beginTextSelection-this.endTextSelection!=0 && this.endTextSelection>0) {
			this.keyboardInputField.text = "";
			if (this.selectedInputField != null) {
				this.selectedInputField.text="";
			}
			this.caretPostionKeyboard = 0;
		}
	}


	//################ Method's for Entering Symbol in the Keyboard-Inputfield######################

	//changes letter's from big to small or small to big
	public void setShift_Flag(){
		if (this.shift_flag) {
			this.setToBigLetters ();
		} else {
			this.setToSmallLetters ();
		}
		this.reFoucsKeyboardInputfieldWithoutDeselectionText ();
	}
	//Changes letter's to big Letter's independent of the shift_flag
	private void setToBigLetters(){
		this.shift_flag = false;
		if (this.keyboard_letters != null) {			
			foreach (GameObject buttonletter in this.keyboard_letters) {
				Text temp = buttonletter.GetComponent<Text> ();
				temp.text = temp.text.ToUpper ();
			}
		}
	}
	//Changes letter's to small Letter's independent of the shift_flag
	private void setToSmallLetters(){
		this.shift_flag = true;
		foreach (GameObject buttonletter in keyboard_letters) {
			Text temp = buttonletter.GetComponent<Text> ();
			temp.text = temp.text.ToLower();
		}
	}
	//Enter's the text at the given caretPostion in the InputField of the Keyboard
	public void enterTextEvent(string key )	{
		this.deleteSelectedText ();
		this.updateTextSelected ();
		this.keyboardInputField.text = this.keyboardInputField.text.Insert(this.caretPostionKeyboard,key);
		if (this.selectedInputField != null) {
			this.selectedInputField.text = this.selectedInputField.text.Insert (this.caretPostionKeyboard, key);
		}
		this.caretPostionKeyboard++;
		this.reFocusKeyboardInputfield ();
	}

	//Enter's a given letter
	public void enterLetterEvent(string letter){
		if (this.shift_flag) {
			this.enterTextEvent (letter.ToLower ());
		} else {
			this.enterTextEvent (letter);
			this.setToSmallLetters ();
		}
	}

	//Delete's the last Input-Symbol
	public void deleteLastInputSymbol()
	{
		this.deleteSelectedText ();
		this.updateTextSelected ();
		if (this.caretPostionKeyboard > 0) {			
			string temp = this.keyboardInputField.text;
			string newText = temp.Substring (0, this.caretPostionKeyboard - 1) + temp.Substring (this.caretPostionKeyboard);
			this.keyboardInputField.text = newText;
			if (this.selectedInputField != null){
				this.selectedInputField.text = newText;
			}
			this.caretPostionKeyboard--;
		}
		this.reFocusKeyboardInputfield ();
	}
	//Make's a linebreak in the keyboard-inputfield
	public void lineBreak(){
		//If the caretPosition is at the end of the text, add the the linebreak	
		if (this.caretPostionKeyboard == this.keyboardInputField.text.Length) {			
			this.keyboardInputField.text += System.Text.RegularExpressions.Regex.Unescape ("\n");
			if (this.selectedInputField != null) {
				this.selectedInputField.text += System.Text.RegularExpressions.Regex.Unescape ("\n");
			}
			this.caretPostionKeyboard++;
			this.deleteSelectedText ();	
			this.updateTextSelected ();
		//If the caretPosition is within the text, add the linebreak at that position and move the caretPosition to the end
		} else {
			string temp = this.keyboardInputField.text.Substring (this.caretPostionKeyboard);
			this.keyboardInputField.text = this.keyboardInputField.text.Remove (this.caretPostionKeyboard);
			this.keyboardInputField.text = this.keyboardInputField.text.Insert (this.caretPostionKeyboard, System.Text.RegularExpressions.Regex.Unescape ("\n"));
			this.keyboardInputField.text = this.keyboardInputField.text.Insert (this.keyboardInputField.text.Length, temp);
			//Update the new caretPosition
			this.keyboardInputField.MoveTextEnd (false);
			this.caretPostionKeyboard = this.keyboardInputField.caretPosition;
			//update the changes to selectedInputField
			if (this.selectedInputField != null) {
				this.selectedInputField.text = this.keyboardInputField.text;
			}
		}
		this.reFocusKeyboardInputfield ();
	}

	//######### Management Method's like save,cancel, delete-wholetext,switch-keyboard-fields ###########################################

	//Delete's the whole text
	public void deleteText()
	{
		if (this.selectedInputField != null) {
			this.selectedInputField.text = "";
		}
		this.keyboardInputField.text = "";
		this.caretPostionKeyboard = 0;
		this.reFocusKeyboardInputfield ();
	}

	//!Cancel everything and reset the text to the old value. And deactivate the keyboard and his Inputfield.
	public void cancel()
	{
		this.caretPostionKeyboard = 0;
		if (this.selectedInputField != null) {
			this.selectedInputField.text = oldText;
		}
		this.keyboardInputField.text = oldText;
		this.gameObject.SetActive (false);
		this.keyboardInputField.DeactivateInputField ();
		//this.setToolControllerPositionBack ();
	}
	//!Save's everything and deactivate the keyboard including his Inputfield.
	public void save()
	{
		this.caretPostionKeyboard = 0;
		if (this.selectedInputField != null) {
			oldText = this.selectedInputField.text;
		}
		this.keyboardInputField.DeactivateInputField ();
		this.gameObject.SetActive (false);
		//this.setToolControllerPositionBack ();
	}

	//Switches between the different Fields (for numbers/letter/special signs) of the keyboard
	public void switchToField(int switchToField){
		switch (this.activatedField_flag) {
		case 0:
		default:
			this.lettersField.SetActive (false);
			break;
		case 1:
			this.numbersField.SetActive (false);
			break;
		case 2:
			this.specialSignsField.SetActive (false);
			break;
		}
		this.switcher (switchToField);
		this.reFoucsKeyboardInputfieldWithoutDeselectionText ();
	}
	private void switcher(int switchToField){
		switch (switchToField) {
		case 0:
		default:
			this.lettersField.SetActive (true);
			break;
		case 1:
			this.numbersField.SetActive (true);
			break;
		case 2:
			this.specialSignsField.SetActive (true);
			break;
		}
		this.activatedField_flag = switchToField;
	}


	//####################### Method's for Refocus to the Keyboard-InputField after a Button is clicked #######################
	//Set the focus back to the keyboard-Inputfield and deselect's the text
	private void reFocusKeyboardInputfield(){
		EventSystem.current.SetSelectedGameObject (this.keyboardInputField.gameObject);
		this.keyboardInputField.caretPosition = this.caretPostionKeyboard;
		this.keyboardInputField.selectionFocusPosition = 0;
		this.keyboardInputField.selectionAnchorPosition = 0;
		this.beginTextSelection = 0;
		this.endTextSelection = 0;
		StartCoroutine (this.waitForFrame ());
	}
	//Set the focus back to the keyboard-Inputfield without Deselection of the text
	private void reFoucsKeyboardInputfieldWithoutDeselectionText(){
		if ((this.beginTextSelection - this.endTextSelection) != 0) {
			EventSystem.current.SetSelectedGameObject (this.keyboardInputField.gameObject);
			this.keyboardInputField.caretPosition = this.caretPostionKeyboard;
		} else {
			this.reFocusKeyboardInputfield ();
		}
	}
	//Necessary for refocusing to the Inputfield, after the user clicked a button. It change's normal color into highlighted color.
	public void changeNormalColorIntoHighlightedColorForRefocus(){
		ColorBlock tempBlock = this.keyboardInputField.colors;
		tempBlock.normalColor = this.keyboardInputField.colors.highlightedColor;
		this.keyboardInputField.colors = tempBlock;
	}
	//Necessary for refocusing to the Inputfield, after the user clicked a button. It change's normal color back into original color.
	public void changeNormalColorBackAfterRefocus(){
		ColorBlock tempBlock = this.keyboardInputField.colors;
		tempBlock.normalColor = normalColor;
		this.keyboardInputField.colors = tempBlock;
	}
	//used to deselect the text of keyboard-Inputfield
	private IEnumerator waitForFrame(){
		yield return 0;
		this.keyboardInputField.MoveTextStart (false);
		this.keyboardInputField.caretPosition = this.caretPostionKeyboard;
	}

	//Method, for development purpose: Enables's to enter text via physical keyboard, if you are not in the virtual reality-mode
	public void keyboardTextEvent(string key )	{
		this.selectedInputField.text = this.keyboardInputField.text;
	}

}
