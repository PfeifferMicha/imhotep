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

		float startup;

		Vector2 targetPos;
		Vector2 appearPos;
		public Vector3 targetScale = new Vector3(0.0025f, 0.0025f, 0.0025f);

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

			targetPos = LayoutSystem.instance.getStartupPosForWidget ( this );
			startup = 0f;
			appearPos = new Vector2 (0, -1);
			transform.localScale = startup * targetScale;
			transform.localPosition = Vector3.Lerp (appearPos, targetPos, startup);
			Debug.Log ("start: " + transform.localScale.x);
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

		public void Update()
		{
			if (startup < 1f) {
				startup += Time.deltaTime;
				if (startup > 1f) {
					startup = 1;
				}
				float scale1 = Mathf.Clamp( startup*2, 0f, 1f );
				float scale2 = Mathf.Clamp ( 3*(-0.33f + startup*1.33f), 0f, 1f);;// Update which lags behind
				transform.localScale = new Vector3 ( scale2*targetScale.x, scale1*targetScale.y, scale1*targetScale.z);
				transform.localPosition = Vector3.Lerp (appearPos, targetPos, scale1);
			}
		}

    }
}
