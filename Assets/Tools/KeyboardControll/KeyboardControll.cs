using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardControll : MonoBehaviour {
	public InputField input;
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
		input.text.Remove (input.text.Length - 1);
	}
		

}
