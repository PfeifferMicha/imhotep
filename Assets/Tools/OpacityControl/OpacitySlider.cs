using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

// TODO Update this class to work like DualSlider?
public class OpacitySlider : MonoBehaviour, IPointerHoverHandler, IPointerDownHandler, IPointerUpHandler
{
	
	public GameObject gameObjectToChangeOpacity;
	private GameObject sliderFill;
	private float dampeningArea = 0.05f;

	private bool sliding = false;

    // Use this for initialization
    void Start()
	{
		sliderFill = transform.Find ("Fill").gameObject;
		updateSlider();
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

	//! Set the slider to the value of f
    public void changeOpacity(float f)
    {
		MeshMaterialControl moc = gameObjectToChangeOpacity.GetComponent<MeshMaterialControl> ();
		if (moc != null) {
			moc.changeOpactiyOfChildren (f);
		}
    }

	//! Called if someone else changed the opacity!
	public void updateSlider(object obj = null){
		if (gameObjectToChangeOpacity != null) {
			float newOpacity = 0f;
			if (gameObjectToChangeOpacity.activeSelf) {
				MeshRenderer mr = gameObjectToChangeOpacity.GetComponentInChildren<MeshRenderer> ();
				if( mr != null )
					newOpacity = mr.material.color.a;
			} else {
				newOpacity = 0f;
			}


			//GetComponent<Slider> ().value = currentOpacity;

			float sliderAmount = newOpacity * (1f - 2f * dampeningArea) + dampeningArea;
			if (newOpacity >= 1)
				sliderAmount = 1;
			else if (newOpacity <= 0)
				sliderAmount = 0;

			//Rect r = transform.GetComponent<RectTransform> ().rect;
			//Debug.Log("sliderFill: " + sliderFill);
			if (sliderFill != null) {
				RectTransform fillRT = sliderFill.GetComponent<RectTransform> ();

				RectTransform rectTF = transform.GetComponent<RectTransform> ();
				Rect r = rectTF.rect;
				fillRT.offsetMax = new Vector2 (-(r.size.x - r.size.x * sliderAmount), fillRT.offsetMax.y);
			}

			//float resultingAmount = Mathf.Clamp ((currentOpacity - dampeningArea)/(1f-2f*dampeningArea), 0f, 1f);
		}
	}


	public void OnPointerDown( PointerEventData data )
	{
		sliding = true;
	}

	public void OnPointerUp( PointerEventData data )
	{
		sliding = false;
	}

	public void OnPointerHover( PointerEventData data )
	{
		if (sliding && InputDeviceManager.instance.currentInputDevice.isLeftButtonDown() ) {
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
