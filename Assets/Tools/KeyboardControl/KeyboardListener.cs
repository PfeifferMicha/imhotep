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

	// Use this for initialization
	void Start () {
		if (this.keyboard == null) {
			this.keyboard = GameObject.FindWithTag ("Keyboard");
		}
		this.controller = this.keyboard.GetComponent<KeyboardControl> ();
	}
	
	// Update is called once per frame
	void Update () {
		GameObject selected = EventSystem.current.currentSelectedGameObject;
		//Activate's the keyboard, if the current selected Gameobject is an Inputfield
		if (selected != null) { 
			InputField inputField = selected.GetComponent<InputField> ();
			if (inputField != null) {
				//Doesn't activat the keyboard, if the the inputfield of the keyboard is the current selected Gameobject
				if (this.controller != null && inputField.tag.CompareTo ("Keyboard") != 0) {
					this.controller.selectedInputField = inputField;
					//Replace's the placeholder-Text of the keyboard-InputField with the placeholdertext of the selected InputField 
					if (inputField.placeholder.GetComponent<Text> () != null) {
						this.controller.keyboardInputField.placeholder.
						GetComponent<Text> ().text = inputField.placeholder.GetComponent<Text> ().text;
					}
					this.controller.oldText = inputField.text;
					this.controller.keyboardInputField.text = inputField.text;
					this.controller.keyboardInputField.ActivateInputField ();
					EventSystem.current.SetSelectedGameObject (keyboard);
					this.keyboard.SetActive (true);
				}
			}
		}
	}
}
