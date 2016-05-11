using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
    public class UICore : MonoBehaviour
    {
        Dictionary<string, Widget> widgets = new Dictionary<string, Widget>();

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
    }
}
