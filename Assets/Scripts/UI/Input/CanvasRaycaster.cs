using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*! Raycaster to determine Object under a 2D position on the canvas.
 * Seems like Unity doesn't supply a "check which UI-Element was hit at this position on the Canvas"
 * without actually sending a ray (which makes little sense in VR), so I wrote my own. */
public class CanvasRaycaster : BaseRaycaster {

	Canvas m_Canvas = null;
	private Canvas canvas
	{
		get {
			if (m_Canvas != null)
				return m_Canvas;

			m_Canvas = GetComponent<Canvas> ();
			return m_Canvas;
		}
	}

	public override Camera eventCamera {
		get {
			return null;
		}
	}

	public override void Raycast( PointerEventData data, List<RaycastResult> resultAppendList )
	{
		if (canvas == null)
			return;

		Rect fullCanvas = GetComponent<RectTransform> ().rect;

		if (data.position.x > fullCanvas.size.x || data.position.y > fullCanvas.size.y)
			return;

		List<Graphic> sortedGraphics = new List<Graphic> ();

		var foundGraphics = GraphicRegistry.GetGraphicsForCanvas (canvas);
		for (int i = 0; i < foundGraphics.Count; ++i) {
			Graphic graphic = foundGraphics [i];

			if (graphic.depth == -1 || !graphic.raycastTarget)
				continue;

			if (!graphic.rectTransform.rect.Contains (data.position))
				continue;

			sortedGraphics.Add (graphic);
		}

		sortedGraphics.Sort ((g1, g2) => g2.depth.CompareTo (g1.depth));

		for (int i = 0; i < sortedGraphics.Count; ++i) {
			Graphic graphic = sortedGraphics [i];
			Debug.Log ("Graphic: " + graphic.name + " + " + graphic.gameObject + " d " + graphic.depth);
			var castResult = new RaycastResult {
				gameObject = graphic.gameObject,
				module = this,
				distance = i,
				screenPosition = data.position,
				index = resultAppendList.Count,
				depth = graphic.depth,
				sortingLayer = canvas.sortingLayerID,
				sortingOrder = canvas.sortingOrder
			};
			resultAppendList.Add( castResult );
		}
	}
}
