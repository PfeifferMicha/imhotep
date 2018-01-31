using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeyboardControll : MonoBehaviour{
	public Text input;
	public SteamVR_TrackedObject tracked;
	public SteamVR_Controller.Device left;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void enterTextEvent( string key )
	{
		input.text = input.text+key;
	}
	public void deleteLastInputSymbol()
	{
		if (input.text.Length >= 1) {
			input.text = input.text.Remove (input.text.Length - 1);
	
		}
	}
	public void deleteText()
	{
		input.text = "";
	}

	public void cancel()
	{
		input.text = "";
		this.gameObject.SetActive (false);
	}
		
}
