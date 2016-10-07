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
			return Camera.main;
		}
	}

	public override void Raycast( PointerEventData data, List<RaycastResult> resultAppendList )
	{
		if (canvas == null)
			return;


		Camera cam = Camera.main;
		Vector3 worldPos = canvas.transform.TransformPoint (new Vector3(data.position.x, data.position.y, 0 ));
		Debug.Log ("worldPos: " + worldPos);
		Vector3 screenPosition = cam.WorldToScreenPoint (worldPos);
		Debug.Log ("Pos: " + screenPosition);

		Rect fullCanvas = GetComponent<RectTransform> ().rect;

		if (data.position.x > fullCanvas.size.x || data.position.y > fullCanvas.size.y)
			return;

		List<Graphic> sortedGraphics = new List<Graphic> ();

		var foundGraphics = GraphicRegistry.GetGraphicsForCanvas (canvas);
		for (int i = 0; i < foundGraphics.Count; ++i) {
			Graphic graphic = foundGraphics [i];

			if (graphic.depth == -1 || !graphic.raycastTarget)
				continue;

			Vector2 localPos = positionFromCanvasSpaceToGraphicSpace (graphic, data.position);
			if (!graphic.rectTransform.rect.Contains (localPos))
				continue;

			sortedGraphics.Add (graphic);
		}

		sortedGraphics.Sort ((g1, g2) => g2.depth.CompareTo (g1.depth));


		for (int i = 0; i < sortedGraphics.Count; ++i) {
			Graphic graphic = sortedGraphics [i];
			var castResult = new RaycastResult {
				gameObject = graphic.gameObject,
				module = this,
				distance = i,
				screenPosition = new Vector2( screenPosition.x, screenPosition.y ),//positionFromCanvasSpaceToGraphicSpace (graphic, data.position),
				index = resultAppendList.Count,
				depth = graphic.depth,
				sortingLayer = canvas.sortingLayerID,
				sortingOrder = canvas.sortingOrder
			};
			resultAppendList.Add( castResult );
		}
	}

	public Vector2 positionFromCanvasSpaceToGraphicSpace( Graphic graphic, Vector2 pos )
	{
		// First, convert the position to world space:
		Vector3 worldPos = GetComponent<RectTransform> ().TransformPoint (pos.x, pos.y, 0);

		// Then inverse-transform it back to local space:
		Vector3 result = graphic.rectTransform.InverseTransformPoint( worldPos );
		return new Vector2 (result.x, result.y);
	}
}
