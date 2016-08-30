using System;
using UnityEngine;
using System.Collections.Generic;

namespace UI
{
	public class LayoutSystem
	{
		public static LayoutSystem instance { private set; get; }

		Camera UICamera;
		float UIScale = 0.0025f;
		float aspectRatio = 1f;
		Dictionary<string, Vector2> startupPositions = new Dictionary<string, Vector2>();

		// Maximal dimensions of UI:
		Vector2 max = new Vector2( 1, 1) / 0.0025f;
		Vector2 min = - (new Vector2( 1, 1) / 0.0025f);

		public LayoutSystem ()
		{
			instance = this;

			startupPositions.Add ("Initiate", new Vector2( 0, -0.9f ) );
			startupPositions.Add ("DicomViewer", new Vector2( -1, 0f ) );
			startupPositions.Add ("PatientBriefing", new Vector2( 1, 0f ) );
			startupPositions.Add ("LoadingScreenWidget", new Vector2( 0,0 ) );
			startupPositions.Add ("ViewControl", new Vector2( 0, -0.8f ) );
			startupPositions.Add ("PatientSelector", new Vector2( 0, -0.7f ) );
		}

		public void setCamera( Camera cam, float scale )
		{
			UICamera = cam;
			UIScale = scale;
			aspectRatio = (float)UICamera.targetTexture.width / (float)UICamera.targetTexture.height;
			Debug.Log ("width " + UICamera.targetTexture.width);
			Debug.Log ("height " + UICamera.targetTexture.height);

			max = new Vector2( 1*aspectRatio, 1) / UIScale;
			min = -max;
		}

		public Vector2 getStartupPosForWidget( GameObject widget )
		{
			Vector2 relPos = new Vector2 (0, -1);
			if (startupPositions.ContainsKey (widget.GetComponent<Widget>().uniqueWidgetName)) {
				relPos = startupPositions [widget.GetComponent<Widget> ().uniqueWidgetName];
			}
			Vector2 absPos = rel2Abs (relPos);
			return clampToScreenSize( absPos, widget ); 
		}

		public Vector2 rel2Abs( Vector2 relPos )
		{
			return new Vector2( relPos.x*aspectRatio, relPos.y) / UIScale;
			//UICamera.targetTexture
		}

		public Vector2 clampToScreenSize( Vector2 pos, GameObject widget )
		{
			Transform tf = widget.GetComponentInChildren<Canvas> ().transform;
			Vector2 widgetMax = tf.GetComponent<RectTransform> ().rect.size * 0.5f;
			Vector2 widgetMin = -widgetMax;
			Vector2 clamped = new Vector2 (
								Mathf.Clamp (pos.x, min.x - widgetMin.x, max.x - widgetMax.x),
								Mathf.Clamp (pos.y, min.y - widgetMin.y, max.y - widgetMax.y)
			                  );
			return clamped;
		}
	}
}
