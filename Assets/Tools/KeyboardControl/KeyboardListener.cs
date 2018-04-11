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
	// Use this for initialization
	void Start () {
		if (keyboard == null) {
			keyboard = GameObject.FindWithTag ("Keyboard");
		}
		controller = keyboard.GetComponent<KeyboardControl> ();
	}
	
	// Update is called once per frame
	void Update () {
		selected = EventSystem.current.currentSelectedGameObject;
		if (selected != null && selected.name=="InputField" ) {
			if (controller != null && selected.GetComponent<InputField> ().tag.CompareTo("Keyboard")!=0  ) {
				controller.selectedInputField = selected.GetComponent<InputField> ();
				//Replace's the placeholder-Text of the keyboard-InputField with the placeholdertext of the selected InputField 
				if (controller.selectedInputField.placeholder.GetComponent<Text> () != null) {
					controller.keyboardInputField.placeholder.
							GetComponent<Text>().text = controller.
												selectedInputField.placeholder.GetComponent<Text> ().text;
				}
				controller.oldText = controller.selectedInputField.text;
				controller.keyboardInputField.text = controller.selectedInputField.text;
				controller.keyboardInputField.ActivateInputField ();
				keyboard.SetActive (true);
				EventSystem.current.SetSelectedGameObject (keyboard);
				InputDeviceManager.instance.shakeLeftController( 0.5f, 0.15f );
			}
		}
	}
}
