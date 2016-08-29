using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace UI
{
    public class Widget : MonoBehaviour
    {
		public Sprite ToolIcon;

		public string uniqueWidgetName = "";
		public bool unique = false;

		public Widget()
		{
		}

		public void Start()
		{
		}

        public void OnEnable()
        {
			// Set material for all texts:
			Material mat = new Material(Shader.Find("Custom/TextShader"));
			Component[] texts;
			texts = GetComponentsInChildren( typeof(Text), true );

			if( texts != null )
			{
				foreach (Text t in texts)
					t.material = mat;
			}

			// Set material for all images:
			Component[] images;
			Material matImage = Resources.Load ("Materials/UI") as Material;
			images = GetComponentsInChildren( typeof(Image) );
			if( images != null )
			{
				foreach (Image im in images) {
					if( im.transform.GetComponent<Mask>() == null )		// Mask images should keep default material.
						im.material = matImage;
				}
			}
        }

		public void initialize( string name )
		{
			uniqueWidgetName = name;
			WidgetEventSystem.triggerEvent (WidgetEventSystem.Event.WIDGET_Opened,
				gameObject.GetComponent<Widget> ());
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
