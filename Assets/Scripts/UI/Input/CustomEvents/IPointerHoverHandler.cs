using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace UI {
	public interface IPointerHoverHandler : IEventSystemHandler
	{
		void OnPointerHover(PointerEventData eventData);
	}
}
