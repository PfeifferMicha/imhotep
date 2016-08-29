using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
	public class UICore : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		// Color which all buttons will turn when clicked:
		public Color ButtonPressedColor = Color.white;

		public bool mouseIsOverUIObject{ private set; get; }

		public static UICore instance { private set; get; }

		public UICore()
		{
			instance = this;
			new LayoutSystem ();
		}

		public void OnPointerEnter(PointerEventData dataName)
		{
			mouseIsOverUIObject = true;
		}
		public void OnPointerExit(PointerEventData dataName)
		{
			mouseIsOverUIObject = false;
		}

		public Color getHighlightColorFor( Color baseCol )
		{
			return baseCol * 1.1f;
		}

		public Color getPressedColorFor( Color baseCol )
		{
			return ButtonPressedColor;
		}
    }
}
