using KSP.IO;
using UnityEngine;

namespace KerbalImprovedSaveSystem
{
	public class KISSDialog : MonoBehaviour
	{
		// 200x300 px window will apear in the center of the screen.
		private Rect windowRect;
		private GUIStyle _dialogStyle, _buttonStyle;
		// Only show it if needed.
		private string dialogType = "";
		private bool isVisible = false;
		private string existingSave = "";



		/// <summary>
		/// Handles initialization of the dialog
		/// </summary>
		void Start()
		{
			windowRect = new Rect((Screen.width - 200) / 2, (Screen.height - 100) / 2, 200, 100);

			_dialogStyle = new GUIStyle(HighLogic.Skin.window);
			_dialogStyle.normal.background = HighLogic.Skin.textField.normal.background;

			_buttonStyle = new GUIStyle(HighLogic.Skin.button);

		}


		/// <summary>
		/// 
		/// </summary>
		void OnGUI()
		{
			if (isVisible)
			{
				switch (dialogType)
				{
					case "Overwrite":
						windowRect = GUI.ModalWindow(0, windowRect, DrawControls, "Overwrite existing save?", _dialogStyle);
						break;

					case "Delete":
						windowRect = GUI.ModalWindow(0, windowRect, DrawControls, "Delete existing save?", _dialogStyle);
						break;
				}
			}
		}


		//// <summary>
		/// Handles all the GUI drawing/layout.
		/// </summary>
		/// <param name="windowId">Window identifier.</param>
		private void DrawControls(int windowId)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("'" + existingSave + "'", GUILayout.ExpandWidth(true));

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace(); // moves the following buttons to the right
			if (GUILayout.Button("Yes", _buttonStyle))
			{
				dialogType = "";
				isVisible = false;
			}
			if (GUILayout.Button("No", _buttonStyle))
			{
				dialogType = "";
				isVisible = false;
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}
		

		/// <summary>
		/// Show a dialog that asks if you want to overwrite a file.
		/// </summary>
		/// <param name="fileName"></param>
		public void ConfirmOverwrite(string fileName)
		{
			existingSave = fileName;
			dialogType = "Overwrite";
			isVisible = true;
		}


		/// <summary>
		/// Show a dialog that asks if you want to delete a file.
		/// </summary>
		/// <param name="fileName"></param>
		public void ConfirmDelete(string fileName)
		{
			existingSave = fileName;
			dialogType = "Delete";
			isVisible = true;
		}
	}
}
