using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class KeyBoardInputControl : MonoBehaviour {
	public InputField keyboardInputField;
	// Use this for initialization
	void Start () {
		
	}
	public void ActivateInputField(){
		keyboardInputField.ActivateInputField ();
		keyboardInputField.Select ();
		Debug.Log ("tessssst");
	}
	// Update is called once per frame
	void Update () {
		
	}
}
