using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class KeyboardListener : MonoBehaviour  {

	public GameObject keyboard;
	public KeyboardControll controller;
	private Vector3 oldCameraPosition;
	private GameObject selected;
	 
	// Use this for initialization
	void Start () {
		if (keyboard == null) {
			keyboard = GameObject.FindWithTag ("Keyboard");
		}

	}
	
	// Update is called once per frame
	void Update () {
		selected = EventSystem.current.currentSelectedGameObject;
		if (selected != null && selected.name=="InputField") {
			oldCameraPosition = Camera.main.transform.position;
			Vector3 currentInputFieldPosition = selected.transform.position;
			currentInputFieldPosition.z -= 2; 
			Camera.main.transform.position = currentInputFieldPosition;
			if (controller != null) {
				controller.selectedInputField = selected.GetComponent<InputField> ();
				controller.oldCameraPosition = oldCameraPosition;
				controller.oldText = controller.selectedInputField.text;
			}
			keyboard.SetActive (true);
		}
	}
	/*public void OnSelect (BaseEventData data)
	{
		Debug.Log ("Test");

		GameObject keyboard = GameObject.FindWithTag ("Keyboard");
		if (keyboard != null)
		{
			Debug.Log ("Test-----");
			keyboard.SetActive (true);
		}
	}*/
}
