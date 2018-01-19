using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UI
{
	[System.Serializable]
	public class OnDualSliderValuesChanged : UnityEvent<float, float>
	{
	}


	/*! Slider with two sliding parts (left and right) which can be used to specify a range.
	 * To use, attach this to a UI element. The UI Element must have a RectTransform and an
	 * Image component. Then set the OnValuesChanged event in the Unity Editor to point to a function
	 * which accepts two floats (minimum and maximum values). This function will be called whenever
	 * the slider values are changed.
	 * The sliders will automatically be generated and will fill the size of the RectTransform.
	 * */
	public class DualSlider : MonoBehaviour, IPointerHoverHandler, IPointerDownHandler, IPointerUpHandler
	{
		public Sprite SliderSprite;
		public Color SliderColor = new Color32 (101, 131, 161, 220);
		public OnDualSliderValuesChanged OnValuesChanged;

		private GameObject leftSlider;
		private GameObject rightSlider;

		private bool slidingLeft = false;
		private bool slidingRight = false;

		float curLeft = 0f;
		float curRight = 0f;

		// Use this for initialization
		void Start () {
			leftSlider = new GameObject ("LeftSlider");
			Image leftSliderImg = leftSlider.AddComponent<Image> ();
			leftSliderImg.sprite = SliderSprite;
			leftSliderImg.material = GetComponent<Image> ().material;
			leftSliderImg.color = SliderColor;
			leftSlider.transform.SetParent (transform, false);
			RectTransform lRect = leftSlider.GetComponent<RectTransform> ();
			lRect.anchorMin = new Vector2 (0, 0);
			lRect.anchorMax = new Vector2 (1, 1);
			lRect.offsetMin = new Vector2( 0, 0 );
			lRect.offsetMax = new Vector2( 0, 0 );

			rightSlider = new GameObject ("RightSlider");
			Image rightSliderImg = rightSlider.AddComponent<Image> ();
			rightSliderImg.sprite = SliderSprite;
			rightSliderImg.material = GetComponent<Image> ().material;
			rightSliderImg.color = SliderColor;
			rightSlider.transform.SetParent (transform, false);
			RectTransform rRight = rightSlider.GetComponent<RectTransform> ();
			rRight.anchorMin = new Vector2 (0, 0);
			rRight.anchorMax = new Vector2 (1, 1);
			rRight.offsetMin = new Vector2( 0, 0 );
			rRight.offsetMax = new Vector2( 0, 0 );

			setValues (0.01f, 0.99f);
		}

		//! Modifies the slider values.
		public void setValues( float left, float right )
		{
			left = Mathf.Clamp (left, 0f, 1f);
			right = Mathf.Clamp (right, 0f, 1f);
			RectTransform rect = GetComponent<RectTransform> ();
			leftSlider.GetComponent<RectTransform>().offsetMax = new Vector2( -rect.rect.width*(1f-left), 0 );
			rightSlider.GetComponent<RectTransform>().offsetMin = new Vector2( rect.rect.width*right, 0 );

			curLeft = left;
			curRight = right;
			// Activate the callback
			if( OnValuesChanged != null )
				OnValuesChanged.Invoke (curLeft, curRight);
		}

		public void OnPointerDown( PointerEventData data )
		{
			slidingLeft = false;
			slidingRight = false;

			Vector2 localMousePos;
			RectTransform rectTf = GetComponent<RectTransform> ();
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle (rectTf, data.position, data.enterEventCamera, out localMousePos)) {
				Rect r = rectTf.rect;

				float amount = (localMousePos.x + r.size.x * 0.5f) / r.size.x;

				float distToLeftSlider = Mathf.Abs (amount - curLeft);
				float distToRightSlider = Mathf.Abs (amount - curRight);
				if (distToLeftSlider < distToRightSlider) {
					slidingLeft = true;
				} else {
					slidingRight = true;
				}
			}
		}

		public void OnPointerUp( PointerEventData data )
		{
			slidingLeft = false;
			slidingRight = false;
		}

		public void OnPointerHover( PointerEventData data )
		{
			if( InputDeviceManager.instance.currentInputDevice.isLeftButtonDown() && (slidingLeft || slidingRight )) {
				Vector2 localMousePos;
				RectTransform rectTf = GetComponent<RectTransform> ();
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle (rectTf, data.position, data.enterEventCamera, out localMousePos)) {
					Rect r = rectTf.rect;

					float amount = (localMousePos.x + r.size.x * 0.5f) / r.size.x;
					if (slidingLeft) {
						float right = curRight;
						if (amount > right)
							right = amount;
						setValues (amount, right);
					} else if (slidingRight) {
						float left = curLeft;
						if (amount < left)
							left = amount;
						setValues (left, amount);
					}
				}
			}
		}
	}
}