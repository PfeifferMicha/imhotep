using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ToolChoise : MonoBehaviour, IPointerEnterHandler {

	public string toolName = "Default Tool Name";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// TODO! This is not called yet:
	public void OnPointerEnter( PointerEventData eventData )
	{
		Debug.Log ("Entered: " + toolName);
	}
}
