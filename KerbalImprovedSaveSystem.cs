using KerbalImprovedSaveSystem.Extensions;
using KSP.IO;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace KerbalImprovedSaveSystem
{
	/// <summary>
	/// Start Kerbal Improved Save System as soon as possible and keep loaded.
	/// </summary>
	[KSPAddon(KSPAddon.Startup.Flight, false)] 
	public class KerbalImprovedSaveSystem : MonoBehaviour
	{
		// used to identify log entries of this mod
		public static string modLogTag = "[KISS]: ";

		// stuff to configure the GUI
		private static Rect _windowPosition = new Rect();
		private static bool _isVisible = false;
		private GUIStyle _windowStyle, _labelStyle, _buttonStyle, _txtFieldStyle, _listStyle;
		private bool _hasInitStyles = false;

		// suggested default filename for savegame
		private string saveFileName = "";
		// list of existing savegames
		private List<string> existingSavegames;
		// scroll position of the list of existing savegames
		private Vector2 _scrollPos;


		/// <summary>
		/// Main part of this Add-on; used to react to keyboard input and do all the magic stuff :)
		/// </summary>
		void Update()
		{
			//on Alt-F5
			if (Input.GetKey("f8")) //GameSettings.QUICKSAVE.GetKey() && GameSettings.MODIFIER_KEY.GetKey())
			{
				if (!_hasInitStyles)
				{
					Debug.Log(modLogTag + "Init GUI.");
					InitStyles();
				}

				if (!_isVisible)
				{
					FlightDriver.SetPause(true);
					_isVisible = true;
					// get lsit of filenames()
					existingSavegames = new List<string>() { "savegame 1", "savegame 2", "savegame 3" };
					saveFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
					RenderingManager.AddToPostDrawQueue(0, OnDraw);
				}
			}
		}

//		void Start()
//		{
//			PluginConfiguration config = PluginConfiguration.CreateForType<KerbalImprovedSaveSystem>();
//
//			config.SetValue("Window Position", _windowPosition);
//			config.save();
//		}
//
//		void OnDestroy()
//		{
//			PluginConfiguration config = PluginConfiguration.CreateForType<KerbalImprovedSaveSystem>();
//
//			config.load();
//			_windowPosition = config.GetValue<Rect>("Window Position");
//		}


		private void OnDraw()
		{
			_windowPosition = GUILayout.Window(1337, _windowPosition, OnWindow, "Save current game progress as...", _windowStyle);

			if (_windowPosition.x == 0f && _windowPosition.y == 0f)
			{
				PluginConfiguration config = PluginConfiguration.CreateForType<KerbalImprovedSaveSystem>();
				config.load();
				_windowPosition = config.GetValue<Rect>("Window Position", _windowPosition.CenterScreen());
			}
		}


		private void OnWindow(int windowId)
		{

			GUILayout.BeginVertical();
			GUILayout.Label("Previous savegames:", _labelStyle);

			_scrollPos = GUILayout.BeginScrollView(_scrollPos, _listStyle);
			int i = 0;
			if (existingSavegames == null)
				Debug.Log (modLogTag + "No existing savegames found.");
			else
			{
				for ( ; i < existingSavegames.Count; i++)
				{
					var rect = new Rect(5, 20 * i, 300, 20);
					if (rect.yMax < _scrollPos.y || rect.yMin > _scrollPos.y + 500)
					{
						continue;
					}

					string saveGameName = existingSavegames[i];
					GUI.Label(rect, saveGameName,_labelStyle);
				}
			}
			GUILayout.Space(20 * i);
			GUILayout.EndScrollView();

			GUILayout.Label("Filename:", _labelStyle);
			saveFileName = GUILayout.TextField(saveFileName, _txtFieldStyle);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace(); // moves the following buttons to the right
			if (GUILayout.Button("Save", _buttonStyle))
			{
				Save(saveFileName);
				Close("SaveDialog completed.");
			}
			if (GUILayout.Button("Cancel", _buttonStyle))
			{
				Close("SaveDialog aborted by user.");
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUI.DragWindow();
		}


		private void InitStyles()
		{
			_windowStyle = new GUIStyle(HighLogic.Skin.window);
			_windowStyle.fixedWidth = 310f;
			_windowStyle.fixedHeight = 600f;

			_labelStyle = new GUIStyle(HighLogic.Skin.label);
			_labelStyle.stretchWidth = true;

			_buttonStyle = new GUIStyle(HighLogic.Skin.button);

			_txtFieldStyle = new GUIStyle(HighLogic.Skin.textField);
			_txtFieldStyle.stretchWidth = true;

			_listStyle = new GUIStyle(HighLogic.Skin.scrollView);
			//_listStyle.fixedHeight = 600f;
			//_listStyle.fixedWidth = 300f;

			_hasInitStyles = true;
		}


		private void Save(string selectedSaveFileName)
		{
			SaveMode s = SaveMode.OVERWRITE; // available SaveModes are: OVERWRITE, APPEND, ABORT
			string filename = GamePersistence.SaveGame (selectedSaveFileName, HighLogic.SaveFolder, s);
			Debug.Log (modLogTag + "Game saved in '" + filename + "'");
		}


		private void Close(string reason)
		{
			// save window position into config file
			PluginConfiguration config = PluginConfiguration.CreateForType<KerbalImprovedSaveSystem>();
			config.SetValue("Window Position", _windowPosition);
			config.save();

			// code to remove window from UI
			_isVisible = false;
			RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
			Debug.Log (modLogTag + reason);	
			FlightDriver.SetPause(false);
		}
	}
}

