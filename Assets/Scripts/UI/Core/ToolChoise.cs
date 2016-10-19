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

		// If the input device is a mouse, add a collider so that we can react to mouse clicks:
		// (If the input device is a controller then 
		InputDevice inputDevice = InputDeviceManager.instance.currentInputDevice;
		if (inputDevice.getDeviceType () == InputDeviceManager.InputDeviceType.Mouse) {
			SphereCollider collider = gameObject.AddComponent<SphereCollider> ();
			collider.center = new Vector3 (0f, 0f, 0.06f);
			collider.radius = 0.1f;
		}
	}

	public void OnPointerEnter( PointerEventData eventData )
	{
		Highlight ();
	}

	public void OnPointerExit( PointerEventData eventData )
	{
		UnHighlight ();
	}

	public void OnPointerClick( PointerEventData eventData )
	{
		toolControl.chooseTool (this);
	}

	public void Highlight()
	{
		ToolNameText.gameObject.SetActive (true);
	}
	public void UnHighlight()
	{
		ToolNameText.gameObject.SetActive (false);
	}
}
