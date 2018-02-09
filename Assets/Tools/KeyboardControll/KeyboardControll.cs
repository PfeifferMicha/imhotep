using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeyboardControll : MonoBehaviour{
	public string oldText;
	public InputField selectedInputField;
	public Vector3 oldCameraPosition;
	//public SteamVR_TrackedObject tracked;
	//public SteamVR_Controller.Device left;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void enterTextEvent( string key )
	{
		selectedInputField.text +=key;
	}
	public void deleteLastInputSymbol()
	{
		if (selectedInputField.text.Length >= 1) {
			selectedInputField.text = selectedInputField.text.Remove (selectedInputField.text.Length - 1);	
		}
	}
	public void deleteText()
	{
		selectedInputField.text = "";
	}

	public void cancel()
	{
		selectedInputField.text = oldText;
		this.gameObject.SetActive (false);
		Camera.main.transform.position = oldCameraPosition;
	}

	public void save()
	{
		this.gameObject.SetActive (false);
		Camera.main.transform.position = oldCameraPosition;
	}
}
