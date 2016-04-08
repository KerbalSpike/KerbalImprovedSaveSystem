using UnityEngine;
using System.Collections;

namespace KerbalImprovedSaveSystem
{
	public class KISSDialog : MonoBehaviour
	{
		// 200x300 px window will apear in the center of the screen.
		private Rect windowRect = new Rect ((Screen.width - 200)/2, (Screen.height - 300)/2, 200, 300);
		// Only show it if needed.
		private bool show = false;
		private string existingSave = "";

		void OnGUI () 
		{
			if(show)
				windowRect = GUI.Window (0, windowRect, DialogWindow, "Overwrite existing save?");
		}

		// This is the actual window.
		void DialogWindow (int windowID)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("A savegame named: '" + existingSave + "' already exists.");
			GUILayout.Label("Overwrite?");

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace(); // moves the following buttons to the right
			if(GUILayout.Button("Yes"))
			{
				show = false;
			}
			if(GUILayout.Button("No"))
			{
				show = false;
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		// To open the dialogue from outside of the script.
		public void Show(string fileName)
		{
			existingSave = fileName;
			show = true;
		}
	}
}
