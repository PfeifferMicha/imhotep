using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ToolChoise : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	public string toolName = "Default Tool Name";

	public ToolControl toolControl;

	public Text ToolNameText;

	void OnEnable () {
		ToolNameText.text = toolName;
		ToolNameText.gameObject.SetActive (false);
	}

	public void OnPointerEnter( PointerEventData eventData )
	{
		ToolNameText.gameObject.SetActive (true);
	}

	public void OnPointerExit( PointerEventData eventData )
	{
		ToolNameText.gameObject.SetActive (false);
	}

	public void OnPointerClick( PointerEventData eventData )
	{
		toolControl.chooseTool (this);
	}
}
