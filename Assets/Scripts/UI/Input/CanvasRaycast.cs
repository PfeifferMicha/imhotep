using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CanvasRaycast : BaseRaycaster {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

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

	public override void Raycast( PointerEventData data, List<RaycastResult> resultAppendList )
	{
		if (canvas == null)
			return;

		Rect fullCanvas = GetComponent<RectTransform> ().rect;

		if (data.position.x > fullCanvas.size.x || data.position.y > fullCanvas.size.y)
			return;

		float hitDistance = float.MaxValue;

		List<Graphic> sortedGraphics = new List<Graphic> ();

		var foundGraphics = GraphicRegistry.GetGraphicsForCanvas (canvas);
		for (int i = 0; i < foundGraphics.Count; ++i) {
			Graphic graphic = foundGraphics [i];

			if (graphic.depth == -1 || !graphic.raycastTarget)
				continue;

			sortedGraphics.Add (graphic);
		}

		sortedGraphics.Sort ((g1, g2) => g2.depth.CompareTo (g1.depth));

		for (int i = 0; i < sortedGraphics.Count; ++i) {

			var castResult = new RaycastResult {
				gameObject 
			};
				results.Add( castResult );
		}
	}
}
