using UnityEngine;
using System.Collections;

namespace KerbalImprovedSaveSystem
{
	public class KISSDialog : MonoBehaviour
	{
		// 200x300 px window will apear in the center of the screen.
		private Rect windowRect = new Rect((Screen.width - 200) / 2, (Screen.height - 300) / 2, 200, 300);
		// Only show it if needed.
		private string show = "false";
		private string existingSave = "";


		void OnGUI()
		{
			switch (show)
			{
				case "Overwrite":
					windowRect = GUI.Window(0, windowRect, DrawOverwriteCntrls, "Overwrite existing save?");
					break;

				case "Delete":
					windowRect = GUI.Window(0, windowRect, DrawDeleteCntrls, "Delete existing save?");
					break;

				case "Tooltip":
					windowRect = GUI.Window(0, windowRect, DrawOverwriteCntrls, "");
					break;
			}
		}


		//// <summary>
		/// Handles all the GUI drawing/layout.
		/// </summary>
		/// <param name="windowId">Window identifier.</param>
		private void DrawOverwriteCntrls(int windowId)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("A savegame named: '" + existingSave + "' already exists.");
			GUILayout.Label("Overwrite?");

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace(); // moves the following buttons to the right
			if (GUILayout.Button("Yes"))
			{
				show = "";
			}
			if (GUILayout.Button("No"))
			{
				show = "";
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}


		//// <summary>
		/// Handles all the GUI drawing/layout.
		/// </summary>
		/// <param name="windowId">Window identifier.</param>
		private void DrawDeleteCntrls(int windowId)
		{
			GUILayout.BeginVertical();
			GUILayout.Label(existingSave);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace(); // moves the following buttons to the right
			if (GUILayout.Button("Yes"))
			{
				show = "";
			}
			if (GUILayout.Button("No"))
			{
				show = "";
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}


		// To open the dialogue from outside of the script.
		public void ConfirmOverwrite(string fileName)
		{
			existingSave = fileName;
			show = "Overwrite";
		}


		// To open the dialogue from outside of the script.
		public void ConfirmDelete(string fileName)
		{
			existingSave = fileName;
			show = "Delete";
		}
	}
}
