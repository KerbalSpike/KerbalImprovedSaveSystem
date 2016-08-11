using KSP.IO;
using UnityEngine;

namespace KerbalImprovedSaveSystem
{
	public class KISSDialog : MonoBehaviour
	{
		private Rect windowRect;
		private GUIStyle _dialogStyle, _buttonStyle;

		private string dialogType, existingSave;
		private bool isVisible;

		private FileOpCallback fileOperation;



		/// <summary>
		/// Handles initialization of the dialog
		/// </summary>
		void Start()
		{
			windowRect = new Rect((Screen.width - 200) / 2, (Screen.height - 100) / 2, 200, 100);

			_dialogStyle = new GUIStyle(HighLogic.Skin.window);
			_dialogStyle.normal.background = HighLogic.Skin.textField.normal.background;

			_buttonStyle = new GUIStyle(HighLogic.Skin.button);

			fileOperation = null;
			dialogType = string.Empty;
			existingSave = string.Empty;
			isVisible = false;
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
				// user confirmed action -> execute operation on the file.
				fileOperation(existingSave);
				// reset and hide dialog
				fileOperation = null;
				dialogType = string.Empty;
				existingSave = string.Empty;
				isVisible = false;
			}
			if (GUILayout.Button("No", _buttonStyle))
			{
				// no confirmation, no action
				fileOperation = null;
				dialogType = string.Empty;
				existingSave = string.Empty;
				isVisible = false;
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}


		/// <summary>
		/// Request user confirmation for a file operation (delete/overwrite).
		/// </summary>
		/// <param name="opType"></param>
		/// <param name="fileName"></param>
		/// <param name="fileOp"></param>
		internal void ConfirmFileOp(string opType, string fileName, FileOpCallback fileOp)
		{
			existingSave = fileName;
			dialogType = opType;
			isVisible = true;
			fileOperation = fileOp;
		}
	}
}
