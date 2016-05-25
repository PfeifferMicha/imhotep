using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
	public class UICore : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        Dictionary<string, Widget> widgets = new Dictionary<string, Widget>();

		public bool mouseIsOverUIObject{ private set; get; }

		public static UICore instance { private set; get; }

		public UICore()
		{
			instance = this;
		}

        // Use this for initialization
        void Start()
        {
			// Move the UI rendering far away from the main scene at startup:
			//transform.position = new Vector3 (1000, 0, 0);
        }

		public void OnPointerEnter(PointerEventData dataName)
		{
			mouseIsOverUIObject = true;
		}
		public void OnPointerExit(PointerEventData dataName)
		{
			mouseIsOverUIObject = false;
		}
    }
}
