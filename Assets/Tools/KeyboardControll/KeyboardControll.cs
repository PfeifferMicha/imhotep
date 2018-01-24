using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardControll : MonoBehaviour {
	public Text input;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void clickEvent( string key )
	{
		input.text = input.text+key;
	}

	/*void getText(){
		Component key = this.GetComponentsInChildren;

		input.text = input.text + inputKey;
	}**/
}
