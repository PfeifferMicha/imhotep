using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
/*
 * Listen every Frame, if the current selected Gameobject is an InputField. 
 * If it is an InputField, it will activate the Keyboard-Gameobject.
 */
public class KeyboardListener : MonoBehaviour{

	public GameObject keyboard;
	//Script
	private KeyboardControl controller;
	//current selected Gameobject
	private GameObject selected;

	//Identify,if a warning is already active
	private bool oneNotificationIsActive;


	// Use this for initialization
	void Start () {
		if (this.keyboard == null) {
			this.keyboard = GameObject.FindWithTag ("Keyboard");
		}
		this.controller = this.keyboard.GetComponent<KeyboardControl> ();
		this.oneNotificationIsActive = false;
	}
	
	// Update is called once per frame
	void Update () {
		//If the user tries to activate an other tool or if a keyboard is activate from an other Scene-Inputfield, send's a warning
		int active = 0;
		foreach (ToolWidget temp in ToolControl.instance.getExistingTools()) {
			if (temp.gameObject.activeSelf) {
				active++;
			}
		}
		//Show's a warning,if an other tool not by the ToolScene already activated the keyboard
		if (active==1 & this.controller.activatedByWhom == KeyboardControl.State_activation.activated_By_None_Tool & !oneNotificationIsActive) {
			NotificationControl.instance.createNotification ("Please, close first the Tool,before open a new Tool", new System.TimeSpan (0, 0, 5));
			this.oneNotificationIsActive = true;
		}
		this.selected = EventSystem.current.currentSelectedGameObject;
		//Activate's the keyboard, if the current selected Gameobject is an Inputfield
		if (this.selected != null && this.selected.name=="InputField" ) {
			//Doesn't activat the keyboard, if the the inputfield of the keyboard is the current selected Gameobject
			if (this.controller != null && this.selected.GetComponent<InputField> ().tag.CompareTo("Keyboard")!=0  ) {
				this.controller.selectedInputField = this.selected.GetComponent<InputField> ();
				//Replace's the placeholder-Text of the keyboard-InputField with the placeholdertext of the selected InputField 
				if (this.controller.selectedInputField.placeholder.GetComponent<Text> () != null) {
					this.controller.keyboardInputField.placeholder.
						GetComponent<Text>().text = this.controller.
												selectedInputField.placeholder.GetComponent<Text> ().text;
				}
				this.controller.oldText = this.controller.selectedInputField.text;
				this.controller.keyboardInputField.text = this.controller.selectedInputField.text;
				this.controller.keyboardInputField.ActivateInputField ();
				this.controller.activatedByWhom = KeyboardControl.State_activation.activated_By_None_Tool;
				foreach (ToolWidget temp in ToolControl.instance.getExistingTools()) {
					if (temp.gameObject.activeSelf) {
						this.controller.activatedByWhom = KeyboardControl.State_activation.activated_By_Tool;
					}
				}
				this.keyboard.SetActive (true);
				EventSystem.current.SetSelectedGameObject (keyboard);
				InputDeviceManager.instance.shakeLeftController( 0.5f, 0.15f );
			}
		}
	}
}
