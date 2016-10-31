using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UI {

	public enum ButtonType
	{
		Left,
		Right,
		Middle,
		Trigger
	}

	public class ButtonInfo {

		public Dictionary<ButtonType, PointerEventData.FramePressState> buttonStates = new Dictionary<ButtonType, PointerEventData.FramePressState>();


		public ButtonInfo()
		{
			// Set all buttons to "Not changed" per default:
			buttonStates[ButtonType.Left] = PointerEventData.FramePressState.NotChanged;
			buttonStates[ButtonType.Right] = PointerEventData.FramePressState.NotChanged;
			buttonStates[ButtonType.Middle] = PointerEventData.FramePressState.NotChanged;
			buttonStates[ButtonType.Trigger] = PointerEventData.FramePressState.NotChanged;
		}
	}
}
