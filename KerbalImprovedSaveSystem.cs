using KerbalImprovedSaveSystem.Extensions;
using KSP.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

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

		// savegame directory of the current came
		private string saveGameDir;
		// suggested default filename for savegame
		private string saveFileName = "";
		// list of existing savegames
		private List<string> existingSaveGames;
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
					//Debug.Log(modLogTag + "HighLogic.SaveFolder = " + HighLogic.SaveFolder);
					saveGameDir = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/";
					existingSaveGames = getExistingSaves(saveGameDir);
					//existingSavegames = new List<string>() { "savegame 1", "savegame 2", "savegame 3" };
					saveFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
					RenderingManager.AddToPostDrawQueue(0, OnDraw);
				}
			}
		}

		//		void Awake()
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
			_windowPosition = GUILayout.Window(1337, _windowPosition, OnWindow, "Kerbal Improved Save System", _windowStyle);

			if (_windowPosition.x == 0f && _windowPosition.y == 0f)
			{
				PluginConfiguration config = PluginConfiguration.CreateForType<KerbalImprovedSaveSystem>();
				config.load();
				_windowPosition = config.GetValue<Rect>("Window Position", _windowPosition.CenterScreen());
			}
		}


		/// <summary>
		/// Handles all the GUI drawing/layout.
		/// </summary>
		/// <param name="windowId">Window identifier.</param>
		private void OnWindow(int windowId)
		{

			GUILayout.BeginVertical();
			GUILayout.Label("Existing savegames:", _labelStyle);

			_scrollPos = GUILayout.BeginScrollView(_scrollPos, _listStyle);
			int i = 0;
			if (existingSaveGames == null)
				Debug.Log(modLogTag + "No existing savegames found.");
			else
			{
				for (; i < existingSaveGames.Count; i++)
				{
					var rect = new Rect(5, 20 * i, 285, 20);
					if (rect.yMax < _scrollPos.y || rect.yMin > _scrollPos.y + 500)
					{
						//do not draw items outside the current scrallview
						continue;
					}

					string saveGameName = existingSaveGames[i];
					GUI.Label(rect, saveGameName, _labelStyle);
				}
			}
			GUILayout.Space(20 * i);
			GUILayout.EndScrollView();

			GUILayout.Label("Save game as:", _labelStyle);
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


		/// <summary>
		/// Initialises all required GUIStyles for the plugin.
		/// </summary>
		private void InitStyles()
		{
			_windowStyle = new GUIStyle(HighLogic.Skin.window);
			_windowStyle.fixedWidth = 300f;
			_windowStyle.fixedHeight = 500f;

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


		/// <summary>
		/// Gets the existing savegames (*.sfs) in the specified directory WITHOUT the 
		/// special "persistent.sfs" file.
		/// </summary>
		/// <returns>List of existing savegames without their path.</returns>
		/// <param name="saveFolder">Directory to search.</param>
		private List<string> getExistingSaves(string saveDir)
		{
			List<string> saveGames = null;

			// retrieve all savegames and put them into a list
			if (Directory.Exists(saveDir))
			{
				Debug.Log(modLogTag + "saveDir found!");
				string[] saves = Directory.GetFiles(saveDir, "*.sfs");
				saveGames = new List<string>(saves.Length);
				foreach (string sav in saves)
				{
					// ignore "persistent.sfs" because that is special in KSP
					// (and will be overwritten every time a savegame is loaded anyway)
					if (!sav.Equals(saveDir + "persistent.sfs"))
					{
						Debug.Log(modLogTag + "savegame found: " + sav);
						// remove path and ".sfs" from filenames and add them to list
						saveGames.Add(sav.Substring(sav.LastIndexOf("/") + 1).Replace(".sfs", ""));
					}
				}
				saveGames.Sort();
			}

			return saveGames;
		}


		/// <summary>
		/// Save the current game progress/status into the specified filename.
		/// </summary>
		/// <param name="selectedSaveFileName">File name to save the game into.</param>
		private void Save(string selectedSaveFileName)
		{
			SaveMode s = SaveMode.OVERWRITE; // available SaveModes are: OVERWRITE, APPEND, ABORT
			string filename = GamePersistence.SaveGame(selectedSaveFileName, HighLogic.SaveFolder, s);
			Debug.Log(modLogTag + "Game saved in '" + filename + "'");
		}


		/// <summary>
		/// Closes the KISS window and writes the specified reason into the Debug.Log.
		/// </summary>
		/// <param name="reason">Why/How was the window closed?</param>
		private void Close(string reason)
		{
			// save window position into config file
			PluginConfiguration config = PluginConfiguration.CreateForType<KerbalImprovedSaveSystem>();
			config.SetValue("Window Position", _windowPosition);
			config.save();

			// code to remove window from UI
			_isVisible = false;
			RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
			Debug.Log(modLogTag + reason);	
			FlightDriver.SetPause(false);
		}
	}
}

