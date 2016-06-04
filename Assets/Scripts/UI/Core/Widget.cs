using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace UI
{
    public class Widget : MonoBehaviour
    {
		public Sprite ToolIcon;

		public string uniqueWidgetName = "";
		public bool unique = false;

		public Widget()
		{
			uniqueWidgetName = "";
		}

        public void OnEnable()
        {
        }

		public void initialize( string name )
		{
			uniqueWidgetName = name;
			WidgetEventSystem.triggerEvent (WidgetEventSystem.Event.WIDGET_Opened,
				gameObject.GetComponent<Widget> ());
		}

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void Close()
        {
            Destroy(gameObject);
        }

		public void OnDestroy()
		{
			WidgetEventSystem.triggerEvent (WidgetEventSystem.Event.WIDGET_Closed,
				gameObject.GetComponent<Widget> ());
		}
    }
}
