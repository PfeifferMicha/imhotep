using System;
using UnityEngine;

namespace UI
{
	public class LayoutSystem
	{
		public static LayoutSystem instance { private set; get; }

		public LayoutSystem ()
		{
			instance = this;
		}

		public Vector2 getStartupPosForWidget( Widget widget )
		{
			return new Vector2 (-1, 0);
		}
	}
}
