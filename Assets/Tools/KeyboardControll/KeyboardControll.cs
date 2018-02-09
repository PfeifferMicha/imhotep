using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeyboardControll : MonoBehaviour{
	public Text inputText; 
	public InputField selectedInputField;
	//public SteamVR_TrackedObject tracked;
	//public SteamVR_Controller.Device left;

	// Use this for initialization
	void Start () {
		//inputText = input.text;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void enterTextEvent( string key )
	{
		inputText.text = inputText.text+key;
	}
	public void deleteLastInputSymbol()
	{
		if (inputText.text.Length >= 1) {
			inputText.text = inputText.text.Remove (inputText.text.Length - 1);
	
		}
	}
	public void deleteText()
	{
		inputText.text = "";
	}

	public void cancel()
	{
		inputText.text = selectedInputField.text;
		this.gameObject.SetActive (false);
	}

	public void save()
	{
		selectedInputField.text = inputText.text;
		this.gameObject.SetActive (false);
	}
}
