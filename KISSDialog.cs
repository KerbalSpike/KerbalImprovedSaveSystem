using KSP.IO;
using UnityEngine;

namespace KerbalImprovedSaveSystem
{
	public class KISSDialog : MonoBehaviour
	{
		internal Rect parentWindow;
		private Rect windowRect;
		private GUIStyle _dialogStyle, _buttonStyle;
		private int kissDialogHeight;
		private int kissDialogWidth;

		private string dialogType, existingSave;
		internal bool isVisible;

		private FileOpCallback fileOperation;



		/// <summary>
		/// Handles initialization of the dialog
		/// </summary>
		void Start()
		{
			kissDialogHeight = 100;
			kissDialogWidth = 200;

			windowRect = new Rect((Screen.width - kissDialogWidth) / 2, (Screen.height - kissDialogHeight) / 2, kissDialogWidth, kissDialogHeight);

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
				// abort using escape
				if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.Escape))
				{
					// consume event in case ESC was pressed, otherwise the main KISS window will react to ESC key while dialog is visible!
					Event.current.Use();
					Event.current.keyCode = KeyCode.None;
					Hide();
				}
				if ((Event.current.type == EventType.KeyUp) && ((Event.current.keyCode == KeyCode.Return) || (Event.current.keyCode == KeyCode.KeypadEnter)))
				{
					// consume event in case return/enter was pressed, otherwise the main KISS window will react to return/enter keys while dialog is visible!
					Event.current.Use();
					Event.current.keyCode = KeyCode.None;
				}

				if (existingSave.Length > 20)
				{
					// rescale dialog to better fit larger filenames.
					kissDialogWidth = Mathf.Min(Mathf.CeilToInt(200 * (existingSave.Length / 20.0f)), (int)(parentWindow.width - 20));
				}
				else
					kissDialogWidth = 200;

				windowRect = new Rect(parentWindow.x + ((parentWindow.width - kissDialogWidth) / 2), parentWindow.y + ((parentWindow.height - kissDialogHeight) / 2), kissDialogWidth, kissDialogHeight);

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


		/// <summary>
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
				Hide();
			}
			if (GUILayout.Button("No", _buttonStyle))
			{
				// no confirmation, no action, just reset and hide dialog
				Hide();
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		/// <summary>
		/// Resets and hides the dialog;
		/// </summary>
		private void Hide()
		{
			fileOperation = null;
			dialogType = string.Empty;
			existingSave = string.Empty;
			isVisible = false;
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
