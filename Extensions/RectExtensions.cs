using UnityEngine;

namespace KerbalImprovedSaveSystem.Extensions
{
	public static class RectExtensions
	{
		public static Rect CenterScreen(this Rect thisRect)
		{
			if (Screen.width > 0 && Screen.height > 0 && thisRect.width > 0f && thisRect.height > 0f)
			{
				thisRect.x = Screen.width / 2 - thisRect.width / 2;
				thisRect.y = Screen.height/ 2 - thisRect.height / 2;
			}

			return thisRect;
		}
	}
}

