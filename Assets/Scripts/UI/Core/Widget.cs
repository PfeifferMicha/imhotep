using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


namespace UI
{
	public class Widget : MonoBehaviour, IPointerEnterHandler, IScrollHandler, IPointerClickHandler
    {
		public LayoutPosition layoutPosition{ private set; get; }
		public Screen layoutScreen = Screen.center;
		public AlignmentH layoutAlignHorizontal = AlignmentH.stretch;
		public AlignmentV layoutAlignVertical = AlignmentV.stretch;

		public void Start()
		{
			// Make sure my canvas is centered:
			Canvas cv = GetComponentInChildren( typeof( Canvas ), true ) as Canvas;
			if (cv != null) {
				cv.transform.localPosition = Vector3.zero;
			}

			// Set material for all texts:
			Material mat = new Material(Shader.Find("Custom/TextShader"));
			Component[] texts;
			texts = GetComponentsInChildren( typeof(Text), true );

			if( texts != null )
			{
				foreach (Text t in texts)
					t.material = mat;
			}


			layoutPosition = new LayoutPosition();
			layoutPosition.screen = layoutScreen;
			layoutPosition.alignHorizontal = layoutAlignHorizontal;
			layoutPosition.alignVertical = layoutAlignVertical;

			UI.Core.instance.layoutSystem.addWidget( this );
		}

        public void Close()
        {
            Destroy(gameObject);
        }

		public void setPosition( LayoutPosition newPosition )
		{
			layoutPosition = newPosition;
			UI.Core.instance.layoutSystem.setWidgetPosition( this, newPosition );
		}

		public void OnPointerEnter( PointerEventData eventData )
		{
			Debug.Log ("Entered Widget: " + name);
		}

		public void OnScroll( PointerEventData eventData )
		{
			Debug.Log ("\tscrolled: " + eventData.scrollDelta);
		}

		public void OnPointerClick( PointerEventData eventData )
		{
			Debug.Log ("\tclicked: " + eventData.button);
		}
    }
}
