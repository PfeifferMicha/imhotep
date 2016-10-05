using System;
using UnityEngine;
using System.Collections.Generic;

namespace UI
{
	public enum Screen {
		left,
		right,
		center
	}
	public enum AlignmentH {
		left,
		right,
		center,
		stretch
	}
	public enum AlignmentV {
		top,
		bottom,
		center,
		stretch
	}

	public class LayoutPosition
	{
		public Screen screen;
		public AlignmentH alignHorizontal;
		public AlignmentV alignVertical;
		public LayoutPosition()
		{
			screen = Screen.center;
			alignHorizontal = AlignmentH.stretch;
			alignVertical = AlignmentV.stretch;
		}
	}


	public class LayoutSystem
	{
		Camera UICamera;

		// Maximal dimensions of UI:
		public Rect fullScreenSize { private set; get; }

		public Vector3 activeScale = new Vector3 (1.05f, 1.05f, 1.05f);
		public Vector3 inactiveScale = new Vector3 (1f, 1f, 1f);

		Rect leftScreen;
		Rect centerScreen;
		Rect rightScreen;

		Screen activeScreen = Screen.center;

		private List<Widget> widgets = new List<Widget>();

		public void setCamera( Camera cam )
		{
			UICamera = cam;

			Vector2 max = new Vector2 (1 * UI.Core.instance.aspectRatio, 1) / UI.Core.instance.UIScale;
			Vector2 min = -max;
			fullScreenSize = new Rect (min, max - min);

			leftScreen = Platform.instance.getScreenDimensions ( Screen.left );
			rightScreen = Platform.instance.getScreenDimensions ( Screen.right );
			centerScreen = Platform.instance.getScreenDimensions ( Screen.center );
		}

		/*public Vector2 getStartupPosForWidget( GameObject widget )
		{
			Vector2 relPos = new Vector2 (0, -1);
			Vector2 absPos = rel2Abs (relPos);
			return clampToScreenSize( absPos, widget ); 
		}

		public Vector2 rel2Abs( Vector2 relPos )
		{
			return new Vector2( relPos.x*UI., relPos.y) / UIScale;
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
		}*/

		public void addWidget( Widget newWidget )
		{
			if (!widgets.Contains (newWidget)) {
				widgets.Add (newWidget);
			}
			setWidgetPosition (newWidget, newWidget.layoutPosition);
		}

		public void setWidgetPosition( Widget widget, LayoutPosition newPosition )
		{
			if (!widgets.Contains (widget)) {
				return;
			}
			RectTransform widgetRect = widget.GetComponent<RectTransform> ();

			Rect parentRect;
			if (newPosition.screen == Screen.left)
				parentRect = leftScreen;
			else if (newPosition.screen == Screen.right)
				parentRect = rightScreen;
			else
				parentRect = centerScreen;

			Vector2 newPos = new Vector2();
			Vector2 newSize = new Vector2 ();
			if (newPosition.alignHorizontal == AlignmentH.stretch) {
				newPos.x = parentRect.center.x;
				newSize.x = parentRect.width;
			} else if (newPosition.alignHorizontal == AlignmentH.left) {
				newPos.x = parentRect.min.x + widgetRect.rect.width * 0.5f;
				newSize.x = widgetRect.rect.width;
			} else if (newPosition.alignHorizontal == AlignmentH.right) {
				newPos.x = parentRect.max.x - widgetRect.rect.width * 0.5f;
				newSize.x = widgetRect.rect.width;
			} else {
				newPos.x = parentRect.center.x;
				newSize.x = widgetRect.rect.width;
			}
			if (newPosition.alignVertical == AlignmentV.stretch) {
				newPos.y = parentRect.center.y;
				newSize.y = parentRect.height;
			} else if (newPosition.alignVertical == AlignmentV.bottom) {
				newPos.y = parentRect.min.y + widgetRect.rect.height * 0.5f;
				newSize.y = widgetRect.rect.height;
			} else if (newPosition.alignVertical == AlignmentV.top) {
				newPos.y = parentRect.max.y - widgetRect.rect.height * 0.5f;
				newSize.y = widgetRect.rect.height;
			} else {
				newPos.y = parentRect.center.y;
				newSize.y = widgetRect.rect.height;
			}

			//widgetRect.rect = new Rect (newPos, newSize);
			widgetRect.anchoredPosition = newPos;
			widgetRect.sizeDelta = newSize;
		}


		/*! Set the position which the camera's center is currently facing */
		public void setLookAtPosition( Vector2 pos )
		{
			Vector2 pixelPos = fullScreenSize.min + new Vector2 (pos.x * fullScreenSize.width, pos.y * fullScreenSize.height);

			bool isInLeftScreen = false;
			bool isInCenterScreen = false;
			bool isInRightScreen = false;

			// Check into which screens the targeted point falls:
			if (pixelPos.x >= leftScreen.min.x && pixelPos.x <= leftScreen.max.x) {
				isInLeftScreen = true;
			}
			if (pixelPos.x >= centerScreen.min.x && pixelPos.x <= centerScreen.max.x) {
				isInCenterScreen = true;
			}
			if (pixelPos.x >= rightScreen.min.x && pixelPos.x <= rightScreen.max.x) {
				isInRightScreen = true;
			}

			// If the point only falls into one single screen, activate that screen:
			if (isInLeftScreen && ! isInCenterScreen && ! isInRightScreen) {
				setActiveScreen ( Screen.left );
			} else if (isInCenterScreen && ! isInLeftScreen && ! isInRightScreen) {
				setActiveScreen (Screen.center);
			} else if (isInRightScreen && ! isInLeftScreen && ! isInCenterScreen) {
				setActiveScreen( Screen.right );
			}
		}

		public void setActiveScreen( Screen s )
		{
			activeScreen = s;
			foreach( Widget w in widgets )
			{
				if (w.layoutScreen == s) {
					w.GetComponent<RectTransform> ().localScale = activeScale;
					Vector3 pos = w.transform.localPosition;
					pos.z = 0;
					w.transform.localPosition = pos;
				} else {
					w.GetComponent<RectTransform> ().localScale = inactiveScale;
					Vector3 pos = w.transform.localPosition;
					pos.z = 100;	// Move backwards
					w.transform.localPosition = pos;
				}
			}
		}
	}

}
