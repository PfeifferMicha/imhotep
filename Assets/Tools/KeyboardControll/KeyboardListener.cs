using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class KeyboardListener : MonoBehaviour  {

	public GameObject keyboard;
	public KeyboardControll controller;
	private GameObject selected;
	public GameObject annotationControl;
	// Use this for initialization
	void Start () {
		if (keyboard == null) {
			keyboard = GameObject.FindWithTag ("Keyboard");
		}
		if (annotationControl == null) {
			annotationControl = GameObject.FindWithTag("AnnotationControl");
		}
	}
	
	// Update is called once per frame
	void Update () {
		selected = EventSystem.current.currentSelectedGameObject;
		if (selected != null && selected.name=="InputField") {
			if (controller != null) {
				controller.selectedInputField = selected.GetComponent<InputField> ();
				controller.oldText = controller.selectedInputField.text;
				controller.keyboardInputField.text = controller.oldText;
			}
			keyboard.SetActive (true);
		}
	}
}
