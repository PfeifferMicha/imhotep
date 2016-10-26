using UnityEngine.EventSystems;


namespace UI
{
	// Adopted from https://gist.github.com/stramit/76e53efd67a2e1cf3d2f
	// Extends the ExecuteEvents so that it can use our self-defined event handlers, like IPointerMoveHandler:
	public static class CustomEvents
	{
		// call that does the mapping
		private static void Execute(IPointerHoverHandler handler, BaseEventData eventData)
		{
			// The ValidateEventData makes sure the passed event data is of the correct type
			handler.OnPointerHover (ExecuteEvents.ValidateEventData<PointerEventData> (eventData));
		}

		// helper to return the functor that should be invoked
		public static ExecuteEvents.EventFunction<IPointerHoverHandler> pointerHoverHandler
		{
			get { return Execute; }
		}
	}

}