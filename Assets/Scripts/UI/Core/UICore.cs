using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
	public class UICore : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        Dictionary<string, Widget> widgets = new Dictionary<string, Widget>();
		private static Transform selectedWidget;

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
			transform.position = new Vector3 (1000, 0, 0);
        }

        public bool RegisterWidget(string uniqueID, Widget widget)
        {
            if (widgets.ContainsKey(uniqueID))
            {
                return false;
            }

            widgets.Add(uniqueID, widget);
            return true;
        }
        public bool UnregisterWidget( string uniqueID )
        {
            if (!widgets.ContainsKey(uniqueID))
            {
                return false;
            }

            widgets.Remove(uniqueID);
            return true;
        }

		public static void HighlightSelectedWidget( Transform widget )
		{
			// Unselect previous:
			if( selectedWidget )
			{
				Vector3 curPos = selectedWidget.localPosition;
				selectedWidget.localPosition = new Vector3 (curPos.x, curPos.y, 0.0f );
			}
			selectedWidget = widget;
			if( selectedWidget )
			{
				Vector3 curPos = selectedWidget.localPosition;
				selectedWidget.localPosition = new Vector3 (curPos.x, curPos.y, -0.5f );
			}
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
