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
		Image startupOverlay = null;

		Vector2 targetPos;
		Vector2 appearPos;
		//public Vector3 targetScale = new Vector3(0.0025f, 0.0025f, 0.0025f);

        public void OnEnable()
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

			targetPos = LayoutSystem.instance.getStartupPosForWidget ( gameObject );
			Debug.Log ("Opening Widget " + name + " at " + targetPos);
			startup = 0f;
			appearPos = new Vector2 (0, -1);
			transform.localScale = new Vector3 (0, 0, 0);
			transform.localPosition = Vector3.Lerp (appearPos, targetPos, startup);

			startupOverlay = transform.Find("Canvas").gameObject.AddComponent<Image> ();
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
				float scale1 = Mathf.Clamp( startup*2, 0f, 1f );
				float scale2 = 0.2f + 0.8f*Mathf.Clamp ( 3*(-0.33f + startup*1.33f), 0f, 1f);// Update which lags behind
				startupOverlay.color = new Color (1, 1, 1, 1-scale2);
				if (startup > 1f) {
					startup = 1;
					Object.Destroy (startupOverlay);
				}
				transform.localScale = new Vector3 ( scale2, scale1, scale1);
				transform.localPosition = Vector3.Lerp (appearPos, targetPos, scale1);
			}
		}

    }
}
