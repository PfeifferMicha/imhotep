using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

public class OpacitySlider : MonoBehaviour, IPointerHoverHandler
{
	
	public GameObject gameObjectToChangeOpacity;
	private GameObject sliderFill;
	private float dampeningArea = 0.05f;

    // Use this for initialization
    void Start()
	{
		updateSlider();
		sliderFill = transform.FindChild ("Fill").gameObject;
    }

	void OnEnable()
	{
		// Register event callbacks:
		PatientEventSystem.startListening(PatientEventSystem.Event.MESH_Opacity_Changed, updateSlider);
	}

	void OnDisable()
	{
		// Unregister myself:
		PatientEventSystem.stopListening(PatientEventSystem.Event.MESH_Opacity_Changed, updateSlider);
	}

	//Set the slider to the value of f
    public void changeOpacity(float f)
    {
		MeshMaterialControl moc = gameObjectToChangeOpacity.GetComponent<MeshMaterialControl> ();
		if (moc != null) {
			moc.changeOpactiyOfChildren (f);
		}
    }

	// Called if Silder value changed from external tool by event system.
	private void updateSlider(object obj = null){		
		if (gameObjectToChangeOpacity != null) {
			float currentOpacity = 0f;
			if (gameObjectToChangeOpacity.activeSelf) {
				MeshRenderer mr = gameObjectToChangeOpacity.GetComponentInChildren<MeshRenderer> ();
				if( mr != null )
					currentOpacity = mr.material.color.a;
			} else {
				currentOpacity = 0f;
			}

			//GetComponent<Slider> ().value = currentOpacity;


			//Rect r = transform.GetComponent<RectTransform> ().rect;
			//Debug.Log("sliderFill: " + sliderFill);
			if (sliderFill != null) {
				RectTransform fillRT = sliderFill.GetComponent<RectTransform> ();
				Debug.Log ("fillRT: " + fillRT);

				RectTransform rectTF = transform.GetComponent<RectTransform> ();
				Rect r = rectTF.rect;
				fillRT.offsetMax = new Vector2 (-(r.size.x - r.size.x * currentOpacity), fillRT.offsetMax.y);
			}

			//float resultingAmount = Mathf.Clamp ((currentOpacity - dampeningArea)/(1f-2f*dampeningArea), 0f, 1f);
		}
	}

	public void OnPointerHover( PointerEventData data )
	{
		if (InputDeviceManager.instance.currentInputDevice.isLeftButtonDown() ) {
			Vector2 localMousePos;
			RectTransform rectTF = transform.GetComponent<RectTransform> ();
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle (rectTF, data.position, data.enterEventCamera, out localMousePos)) {
				Rect r = rectTF.rect;

				float amount = (localMousePos.x + r.size.x * 0.5f) / r.size.x;
				float scaledAmount = amount;
				if (amount > 1f - dampeningArea)
					scaledAmount = 1f;
				else if (amount < dampeningArea)
					scaledAmount = 0f;

				RectTransform fillRT = sliderFill.GetComponent<RectTransform> ();
				fillRT.offsetMax = new Vector2 ( -(r.size.x - r.size.x*scaledAmount), fillRT.offsetMax.y);

				float resultingAmount = Mathf.Clamp ((amount - dampeningArea)/(1f-2f*dampeningArea), 0f, 1f);

				changeOpacity (resultingAmount);
			}
		}
	}
}
