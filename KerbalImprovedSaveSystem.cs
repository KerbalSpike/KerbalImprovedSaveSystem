using KerbalImprovedSaveSystem.Extensions;
using KSP.IO;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KerbalImprovedSaveSystem
{
	/// <summary>
	/// Used to execute file operations depending of dialog result (yes/no).
	/// </summary>
	/// <param name="filename">The name of the file to be modified.</param>
	internal delegate void FileOpCallback(string filename);

	/// <summary>
	/// Start Kerbal Improved Save System when in a flight scene.
	/// </summary>
	[KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
	public class KerbalImprovedSaveSystem : MonoBehaviour
	{
		// used to identify log entries of this plugin
		public const string modLogTag = "[KISS]: ";

		// stuff to configure the GUI
		private Rect windowPosSize;
		private bool isVisible;
		private KISSDialog _kissDialog;
		private string kissTooltip;
		private GUIStyle _windowStyle, _labelStyle, _whiteLblStyle, _buttonStyle, _altBtnStyle, _delBtnStyle, _listBtnStyle,
			_listSelectionStyle, _txtFieldStyle, _listStyle, _toggleStyle, _selectionGridSytle, _tooltipWindowStyle, _tooltipLblStyle;
		private Texture2D _settingsTexture;
		private bool hasInitStyles;
		// scroll position in the list of existing savegames
		private Vector2 scrollPos;
		private bool showSettings;

		// stuff to detect double clicks
		private bool dblClicked;
		private DateTime lastClickTime;
		private TimeSpan catchTime = TimeSpan.FromMilliseconds(250);

		// savegame directory of the current came
		private string saveGameDir;
		// currently selected filename for savegame
		private string selectedFileName;
		// list of existing savegames
		private List<string> existingSaveGames;

		// reference to the plugin configuration
		private PluginConfiguration config;

		// stuff for controlling the suggested name for the savegame when opening KISS
		private string[] dfltSaveNames;
		private int selectedDfltSaveName;
		private GUIContent[] slctnGridContent;

		// flags  and variables to configure behaviour

		// enable/disable overwrite confirmations
		private bool confirmOverwrite = false;
		// enable/disable delete confirmations
		private bool confirmDelete = false;
		// timestamps in game time instead of system time
		private bool useGameTime = false;
		// reverse sorting of savegames (newest first of only using saves with timestamps)
		private bool reverseOrder = false;
		// enable/disable quicksave mode (quicksave without showing gui)
		private bool quickSaveMode = false;
		// enable/disable detection of pressed key for changing keybinding for KISS
		private bool detectingNewKey = false;
		// holds the currently used keybinding for KISS
		private KeyCode kissKeyCode;
		private String kissKeyCaption;

		/// <summary>
		/// Handles initialization of the plugin
		/// </summary>
		void Start()
		{
			Debug.Log(modLogTag + "Starting ...");

			_kissDialog = gameObject.AddComponent<KISSDialog>();

			InitSettings();

			dfltSaveNames = new string[] { "{Time}_{ActiveVessel}", "{ActiveVessel}_{Time}", "quicksave" };
			slctnGridContent = new GUIContent[] {
				new GUIContent("\"" + dfltSaveNames[0] + "\"","\"{Time}\" is replaced with either the current system or game time, \"{ActiveVessel}\" with the name of the current vessel."),
				new GUIContent("\"" + dfltSaveNames[1] + "\"","\"{Time}\" is replaced with either the current system or game time, \"{ActiveVessel}\" with the name of the current vessel."),
				new GUIContent("\"" + dfltSaveNames[2] + "\"","Use this option if you want to use KISS to quicksave.")
			};

			Debug.Log(modLogTag + "Init GUI ...");
			InitStyles();

			Debug.Log(modLogTag + "Ready for action!");
		}


		/// <summary>
		/// Main part of this Add-on; used to react to keyboard input and do all the magic stuff :)
		/// </summary>
		void Update()
		{
			if (!isVisible)
			{
				// show window on keypress (default key = F8)
				if (Input.GetKey(kissKeyCode)) //GameSettings.QUICKSAVE.GetKey() && GameSettings.MODIFIER_KEY.GetKey())
				{
					// pause game and acquire save directory and filename
					FlightDriver.SetPause(true);
					saveGameDir = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/";
					existingSaveGames = getExistingSaves(saveGameDir);
					selectedFileName = getDfltFileName();

					// launch GUI when not in quicksave mode or when modifier key is pressed (default: ALT)
					if (!quickSaveMode || GameSettings.MODIFIER_KEY.GetKey())
					{

						isVisible = true;
					}
					else
					{
						// quicksave mode: simply save and we are done
						// note: this mode ignores the "confirm overwrite" setting!
						Save(selectedFileName);
					}
				}
			}
			else // if visible...
			{
				// find out if a doubleclick has happened
				checkForDoubleClick();

				// detect the key being pressed when user changes the hotkey for KISS.
				if (detectingNewKey && Event.current.isKey)
					DetectInput();

				// allow aborting window by pressing ESC, but only if _kissDialog is not visible
				if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.Escape)
					&& !_kissDialog.isVisible && !(Event.current.type == EventType.Used))
				{
					Close("SaveDialog aborted by user.");
				}

				// allow saving with currently selected name without moving mouse using Return/Enter
				if ((Event.current.type == EventType.KeyUp) && ((Event.current.keyCode == KeyCode.Return) || (Event.current.keyCode == KeyCode.KeypadEnter)))
				{
					// only if the selectedFileName isn't emtpy and _kissDialog is not visible
					if (selectedFileName != "" && !_kissDialog.isVisible && !(Event.current.type == EventType.Used))
					{
						ConfirmFileOp(confirmOverwrite && existingSaveGames.Contains(selectedFileName), "Overwrite", selectedFileName, Save);
					}
				}
			}
		}

		/// <summary>
		/// Determines if a doubleclick has happened.
		/// </summary>
		private void checkForDoubleClick()
		{
			// detect double clicks
			if (Input.GetMouseButtonDown(0))
			{
				if (DateTime.Now - lastClickTime < catchTime)
				{
					dblClicked = true;
				}
				else
				{
					//normal click
					dblClicked = false;
				}
				lastClickTime = DateTime.Now;
			}
		}

		/// <summary>
		/// Detects a key input to configure the KISS shortcuts.
		/// To be honest this method is a mess but I have not yet found a "proper" way of handling this.
		/// </summary>
		private void DetectInput()
		{
			// check for every possible key if it was pressed.
			foreach (KeyCode vkey in System.Enum.GetValues(typeof(KeyCode)))
			{
				// do not allow the use of modifier keys, because that makes everything way more difficult
				if (Input.GetKeyDown(vkey) && !Event.current.shift && !Event.current.capsLock &&
					!Event.current.control && !Event.current.alt && !Event.current.command)
				{
					// ignore ESC, Return, KeypadEnter and "modifier keys" (SHIFT, CTRL, ALT, Command (Mac))
					if ((vkey != KeyCode.Return) && (vkey != KeyCode.Escape) && (vkey != KeyCode.KeypadEnter) &&
						(vkey != KeyCode.LeftControl) && (vkey != KeyCode.RightControl) &&
						(vkey != KeyCode.LeftAlt) && (vkey != KeyCode.RightAlt) && (vkey != KeyCode.AltGr) &&
						(vkey != KeyCode.LeftCommand) && (vkey != KeyCode.RightCommand) &&
						(vkey != KeyCode.LeftApple) && (vkey != KeyCode.RightApple) &&
						(vkey != KeyCode.LeftWindows) && (vkey != KeyCode.RightWindows) &&
						(vkey != KeyCode.LeftShift) && (vkey != KeyCode.RightShift) && (vkey != KeyCode.CapsLock))
					{
						kissKeyCode = vkey;
						Boolean isFuncKey = Event.current.functionKey;
						Event.current.capsLock = false;
						char kissKeyChar = ' ';
						kissKeyChar = Event.current.character;
						// handle special cases like keys for characters that combine with others like "´" or "^"
						// but ignore the "Pause" key, as that will be handled correctly further below
						if ((kissKeyChar == '\0') && (!isFuncKey) && (vkey != KeyCode.Pause))
						{
							// ignore this key, so user has to press it again (like accents on german keyboard) or press another key.
							continue;
						}
						// set the label for the new key either as the character or its name (for function keys).
						if ((kissKeyChar == '\0') || isNumKey(vkey) || Char.IsDigit(kissKeyChar) || Char.IsWhiteSpace(kissKeyChar))
							kissKeyCaption = Enum.GetName(typeof(KeyCode), kissKeyCode);
						else
							kissKeyCaption = (kissKeyChar + "").ToUpper();

						// stop detection of key input
						detectingNewKey = false;

						// make sure nothing else is activated by this keypress
						Event.current.Use();
						Event.current.keyCode = KeyCode.None;
						Event.current.character = '\0';

						_kissDialog.Hide();
					}
				}
			}
		}

		/// <summary>
		/// Check if the pressed key was on the numeric keyboard to 
		/// label them with their Keypad name.
		/// Unfortunately keypad number keys are not detected when
		/// Numlock is off, then they register as arrow keys etc. .
		/// </summary>
		/// <param name="vkey">The key that has been pressed.</param>
		/// <returns>True - if vkey is on the numeric keyboard.
		/// False - for all other keys.</returns>
		private bool isNumKey(KeyCode vkey)
		{
			// array of all the Keypad Keycodes.
			KeyCode[] numKeys = {KeyCode.Keypad0,
				KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3,
				KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6,
				KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
				KeyCode.KeypadDivide,
				KeyCode.KeypadMultiply,
				KeyCode.KeypadMinus,
				KeyCode.KeypadPlus,
				KeyCode.KeypadPeriod,
				KeyCode.KeypadEquals,
				KeyCode.KeypadEnter,
				KeyCode.Numlock
			};

			return (numKeys.IndexOf(vkey) > -1);
		}

		/// <summary>
		/// Called by Unity to draw the GUI - can be called many times per frame
		/// (once for every event), so event/input related stuff should go here?
		/// Well no because some keyboard detection stuff doens't work when called
		/// in here instead of in Update()
		/// </summary>
		private void OnGUI()
		{
			if (isVisible)
			{
				// finally do all the GUI drawing stuff
				OnDraw();
			}
		}

		/// <summary>
		/// Handles window positioning and tooltip overlay.
		/// </summary>
		private void OnDraw()
		{
			// resize window to show settings panel
			if (showSettings)
			{
				windowPosSize.width = 600;
			}
			else
			{
				windowPosSize.width = 400;
			}

			// create window
			windowPosSize = GUILayout.Window(this.GetInstanceID(), windowPosSize, DrawControls, "Kerbal Improved Save System", _windowStyle);

			// handle display of tooltip
			if (!string.IsNullOrEmpty(kissTooltip))
			{
				// This code is partially borrowed from "[x] Science!" Mod:
				// calculate required height of the label for the given text and width
				float boxHeight = _tooltipLblStyle.CalcHeight(new GUIContent(kissTooltip), 190);
				Rect tooltipPosSize = new Rect(Mouse.screenPos.x + 15, Mouse.screenPos.y + 15, 194, boxHeight + 4);
				// Move tooltip left and/or above of cursor if it would be outside of screen
				if ((tooltipPosSize.x + 15 + tooltipPosSize.width) > Screen.width)
					tooltipPosSize.x = tooltipPosSize.x - 15 - tooltipPosSize.width;
				if ((tooltipPosSize.y + 15 + tooltipPosSize.height) > Screen.height)
					tooltipPosSize.y = tooltipPosSize.y - 15 - tooltipPosSize.height;
				// create window and declare window function inside arguments of constructor
				GUI.Window(1, tooltipPosSize, x =>
				{
					GUI.Label(new Rect(2, 2, 190, boxHeight), kissTooltip, _tooltipLblStyle);
				}, string.Empty, _tooltipWindowStyle);
			}
		}


		/// <summary>
		/// Handles all the GUI drawing/layout.
		/// </summary>
		/// <param name="windowId">Window identifier.</param>
		private void DrawControls(int windowId)
		{

			GUILayout.BeginHorizontal(); // outer container. left: KISS panel, right: settings panel

			GUILayout.BeginVertical(GUILayout.MaxWidth(388), GUILayout.ExpandHeight(true)); // contains content of main KISS window, without settings panel

			GUILayout.BeginHorizontal(); // area above file list
			GUILayout.BeginVertical();
			GUILayout.Space(10); // moves the following label down
			GUILayout.Label("Existing savegames:", _labelStyle);
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace(); // moves the following button to the right
			if (GUILayout.Button(new GUIContent(_settingsTexture, "Toggle Options"), _buttonStyle))
			{
				showSettings = !showSettings;
			}
			GUILayout.EndHorizontal(); // END area above file list

			GUILayout.Space(6);

			scrollPos = GUILayout.BeginScrollView(scrollPos, _listStyle);
			int i = 0;
			if (existingSaveGames == null)
				Debug.Log(modLogTag + "No existing savegames found.");
			else
			{
				// create a Button inside this ScrollView for every existing savegame.
				for (; i < existingSaveGames.Count; i++)
				{
					// code for only rendering currently visible elements in scrollView, 
					// disabled because it causes flickering in KSP 1.1 / Unity 5
					//					var rect = new Rect(5, 20 * i, 285, 20);
					//					if (rect.yMax < _scrollPos.y || rect.yMin > _scrollPos.y + 500)
					//					{
					//						//do not draw items outside the current ScrollView
					//						continue;
					//					}

					string saveGameName;
					if (reverseOrder)
						saveGameName = existingSaveGames[existingSaveGames.Count - 1 - i];
					else
						saveGameName = existingSaveGames[i];

					GUIStyle _renderStyle = _listBtnStyle;
					if (saveGameName == selectedFileName)
					{
						// highlight the list item that is currently selected
						_renderStyle = _listSelectionStyle;
					}
					if (GUILayout.Button(saveGameName, _renderStyle))
					{
						selectedFileName = saveGameName;
						if (dblClicked)
						{
							dblClicked = false;
							ConfirmFileOp(confirmOverwrite && existingSaveGames.Contains(selectedFileName), "Overwrite", selectedFileName, Save);
						}
					}
				}
			}
			GUILayout.Space(40);
			GUILayout.EndScrollView();

			GUILayout.Label("Save game as:", _labelStyle);
			selectedFileName = GUILayout.TextField(selectedFileName, _txtFieldStyle, GUILayout.MaxWidth(388));

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Default", "Use the suggested default filename for the savegame."), _altBtnStyle))
			{
				selectedFileName = getDfltFileName();
			}
			GUILayout.Space(70);
			if (existingSaveGames.Contains(selectedFileName))
			{
				if (GUILayout.Button("Delete", _delBtnStyle))
				{
					ConfirmFileOp(confirmDelete, "Delete", selectedFileName, Delete);
				}
			}
			GUILayout.FlexibleSpace(); // moves the following buttons to the right
			if (GUILayout.Button("Cancel", _buttonStyle))
			{
				Close("SaveDialog aborted by user.");
			}
			if (GUILayout.Button("Save", _buttonStyle))
			{
				ConfirmFileOp(confirmOverwrite && existingSaveGames.Contains(selectedFileName), "Overwrite", selectedFileName, Save);
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical(); // end of main KISS dialog

			if (showSettings)
			{
				GUILayout.BeginVertical(); // start of settings area
				GUILayout.Space(10); // moves the following label down
				GUILayout.Label("General Settings:", _labelStyle);

				confirmOverwrite = GUILayout.Toggle(confirmOverwrite, new GUIContent("Confirm overwrite", "Enable to require confirmation before overwriting existing savegames."), _toggleStyle);
				confirmDelete = GUILayout.Toggle(confirmDelete, new GUIContent("Confirm delete", "Enable to require confirmation before deleting existing savegames."), _toggleStyle);
				useGameTime = GUILayout.Toggle(useGameTime, new GUIContent("Use game time", "If enabled, timestamps created by KISS use the ingame time instead of your system time."), _toggleStyle);
				reverseOrder = GUILayout.Toggle(reverseOrder, new GUIContent("Reverse list", "If enabled, the list of savegames is shown in reverse order."), _toggleStyle);

				GUILayout.Space(10); // moves the following label down
				GUILayout.Label("Default filename:", _labelStyle);
				selectedDfltSaveName = GUILayout.SelectionGrid(selectedDfltSaveName, slctnGridContent, 1, _selectionGridSytle, GUILayout.ExpandWidth(true));

				GUILayout.Space(10); // moves the following label down
				GUILayout.Label("Keybinding:", _labelStyle);
				GUILayout.BeginHorizontal(); // keybindings
				GUILayout.Label("Open KISS:", _whiteLblStyle);
				//GUILayout.Space(10); // moves the following item to the right
				if (GUILayout.Button(new GUIContent(kissKeyCaption, "Click button to change key."), _buttonStyle))
				{
					// show overlay/dialog promting to press a key/button
					_kissDialog.parentWindow = windowPosSize; //update position of main window
					_kissDialog.PromptKeybindingInput();
					detectingNewKey = true;
				}
				GUILayout.EndHorizontal(); // end of keybindings

				GUILayout.Space(10); // moves the following label down
				GUILayout.Label("Remember this option!", _labelStyle);
				quickSaveMode = GUILayout.Toggle(quickSaveMode, new GUIContent("Quicksave mode (no GUI)", "If enabled, pressing > " + kissKeyCaption + " < will directly save the game using the current filename settings. Ignores \"Confirm overwrite\" setting. Press MOD + " + kissKeyCaption + " to show KISS window again."), _toggleStyle);


				GUILayout.EndVertical(); // end of settings area
			}

			GUILayout.EndHorizontal(); // end of KISS window incl. settings

			GUI.DragWindow();

			kissTooltip = GUI.tooltip;
			// This code (and comment) is borrowed from "[x] Science!" mod:
			// If this window gets focus, it pushes the tooltip behind the window, which looks weird.
			// Just hide the tooltip while mouse buttons are held down to avoid this.
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
			{
				kissTooltip = string.Empty;
			}
		}


		/// <summary>
		/// Loads stored configuration from file (and/or initializes default values)
		/// </summary>
		private void InitSettings()
		{
			// default keycode for kiss
			kissKeyCode = KeyCode.F8;
			kissKeyCaption = "F8";

			windowPosSize = new Rect(0, 0, 400, 500);
			isVisible = false;
			hasInitStyles = false;

			showSettings = false;
			kissTooltip = string.Empty;
			dblClicked = false;
			selectedFileName = string.Empty;

			config = PluginConfiguration.CreateForType<KerbalImprovedSaveSystem>();
			config.load();

			windowPosSize = config.GetValue<Rect>("Window Position", windowPosSize.CenterScreen());
			confirmOverwrite = config.GetValue<bool>("confirmOverwrite", false);
			confirmDelete = config.GetValue<bool>("confirmDelete", true);
			useGameTime = config.GetValue<bool>("useGameTime", false);
			reverseOrder = config.GetValue<bool>("reverseOrder", false);
			selectedDfltSaveName = config.GetValue<int>("selectedDfltSaveNameInt", 0);
			quickSaveMode = config.GetValue<bool>("quickSaveMode", false);
			kissKeyCode = config.GetValue<KeyCode>("kissKeyCode", KeyCode.F8);
			kissKeyCaption = config.GetValue<string>("kissKeyCaption", "F8");
		}


		/// <summary>
		/// Initialises all required GUIStyles for the plugin.
		/// </summary>
		private void InitStyles()
		{
			_settingsTexture = new Texture2D(20, 20, TextureFormat.ARGB32, false); // is this line of code really necessary?
			_settingsTexture = GameDatabase.Instance.GetTexture("KerbalImprovedSaveSystem/icons/settings", false);

			Color myYellow = HighLogic.Skin.textField.normal.textColor;
			Color myRed = new Color(0.78f, 0f, 0f);
			Color myOrange = new Color(1f, 0.4f, 0f);

			_windowStyle = new GUIStyle(HighLogic.Skin.window);
			_windowStyle.normal.textColor = myYellow;
			_windowStyle.onNormal.textColor = myYellow;
			_windowStyle.hover.textColor = myYellow;
			_windowStyle.onHover.textColor = myYellow;
			_windowStyle.active.textColor = myYellow;
			_windowStyle.onActive.textColor = myYellow;
			_windowStyle.padding.left = 6;
			_windowStyle.padding.right = 6;

			_labelStyle = new GUIStyle(HighLogic.Skin.label);
			_labelStyle.stretchWidth = true;

			_whiteLblStyle = new GUIStyle(HighLogic.Skin.label);
			_whiteLblStyle.normal.textColor = Color.white;
			_whiteLblStyle.hover.textColor = Color.white;
			_whiteLblStyle.active.textColor = Color.white;

			_buttonStyle = new GUIStyle(HighLogic.Skin.button);

			_altBtnStyle = new GUIStyle(HighLogic.Skin.button);
			_altBtnStyle.normal.textColor = myYellow;
			_altBtnStyle.hover.textColor = myYellow;
			_altBtnStyle.active.textColor = myYellow;

			_delBtnStyle = new GUIStyle(HighLogic.Skin.button);
			_delBtnStyle.normal.textColor = myOrange;
			_delBtnStyle.hover.textColor = myOrange;
			_delBtnStyle.active.textColor = myOrange;

			_listBtnStyle = new GUIStyle(HighLogic.Skin.button);
			_listBtnStyle.alignment = TextAnchor.MiddleLeft;
			_listBtnStyle.hover.background = _listBtnStyle.normal.background;
			_listBtnStyle.normal.background = null;

			_listSelectionStyle = new GUIStyle(HighLogic.Skin.button);
			_listSelectionStyle.alignment = TextAnchor.MiddleLeft;
			_listSelectionStyle.normal.background = _listSelectionStyle.active.background;
			_listSelectionStyle.hover.background = _listSelectionStyle.active.background;
			_listSelectionStyle.normal.textColor = myYellow;
			_listSelectionStyle.hover.textColor = myYellow;

			_txtFieldStyle = new GUIStyle(HighLogic.Skin.textField);
			_txtFieldStyle.stretchWidth = true;

			_listStyle = new GUIStyle(HighLogic.Skin.scrollView);
			_listStyle.padding.left = 6;
			_listStyle.padding.right = 6;
			_listStyle.margin.left = 4;
			_listStyle.margin.right = 4;

			_toggleStyle = new GUIStyle(HighLogic.Skin.toggle);
			_toggleStyle.stretchWidth = true;

			_selectionGridSytle = new GUIStyle(HighLogic.Skin.toggle);
			_selectionGridSytle.stretchWidth = true;

			_tooltipWindowStyle = new GUIStyle(HighLogic.Skin.window);

			_tooltipLblStyle = new GUIStyle(HighLogic.Skin.box);
			_tooltipLblStyle.normal.background = _txtFieldStyle.normal.background;
			_tooltipLblStyle.wordWrap = true;
			_tooltipLblStyle.stretchHeight = true;
			_tooltipLblStyle.padding = new RectOffset(2, 2, 2, 2);

			hasInitStyles = true;

			Debug.Log(modLogTag + "GUI styles initialised.");
		}


		/// <summary>
		/// Gets the existing savegame filenames (*.sfs) in the specified directory WITHOUT the 
		/// special "persistent.sfs" file (that is special in KSP and will be overwritten
		/// every time another savegame is loaded anyway).
		/// </summary>
		/// <returns>List of existing savegames without their paths and extensions.</returns>
		/// <param name="saveDir">Directory to search.</param>
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
					// (it will be overwritten every time a savegame is loaded anyway)
					if (!sav.Equals(saveDir + "persistent.sfs"))
					{
						// remove path and ".sfs" from filenames and add them to list
						saveGames.Add(sav.Substring(sav.LastIndexOf("/") + 1).Replace(".sfs", ""));
					}
				}
				saveGames.Sort();
			}

			return saveGames;
		}


		/// <summary>
		/// Returns the default filename for the savegame according to the current settings
		/// </summary>
		/// <returns></returns>
		private string getDfltFileName()
		{
			string result = dfltSaveNames[selectedDfltSaveName];
			if (useGameTime)
			{
				// Use planetarium universal time
				string timeStamp = KSPUtil.PrintDateCompact(Planetarium.GetUniversalTime(), true, true);
				// PrintDateNew output has format "Y1, D01, 0:24:45"
				// -> change to "Y1_D01_0_24_43"
				timeStamp = timeStamp.Replace(", ", "_").Replace(":", "_");
				result = result.Replace("{Time}", timeStamp);
			}
			else
				result = result.Replace("{Time}", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

			// If we are at the space center, we can't access the name of the active vessel, as there simple is none.
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
				result = result.Replace("{ActiveVessel}", "SpaceCenter");
			else
				result = result.Replace("{ActiveVessel}", FlightGlobals.ActiveVessel.vesselName);


			return result;
		}


		/// <summary>
		/// Gets permission to perform some operation on a file, like deleting or overwriting.
		/// </summary>
		/// <param name="confirmRequired">If confirmation is disabled, the operation is performed directly.</param>
		/// <param name="opType">Type of file operation. Currently available: "Delete" and "Save".</param>
		/// <param name="filename">The name of the file to be manipulated.</param>
		/// <param name="fileOp">Callback for the dialog to execute in case user confirms action.</param>
		private void ConfirmFileOp(bool confirmRequired, string opType, string filename, FileOpCallback fileOp)
		{
			if (confirmRequired)
			{
				_kissDialog.parentWindow = windowPosSize; //update position of main window
				_kissDialog.ConfirmFileOp(opType, selectedFileName, fileOp);
			}
			else
			{
				fileOp(filename);
			}
		}


		/// <summary>
		/// Delete the specified filename.
		/// </summary>
		/// <param name="selectedSaveFileName">Filename to be deleted.</param>
		private void Delete(string selectedSaveFileName)
		{
			string filename = saveGameDir + selectedFileName + ".sfs";
			System.IO.File.Delete(filename);
			existingSaveGames.Remove(selectedFileName);
			selectedFileName = String.Empty;
			Debug.Log(modLogTag + "Savegame '" + filename + "' deleted.");
		}


		/// <summary>
		/// Save the current game progress/status into the specified filename.
		/// </summary>
		/// <param name="selectedSaveFileName">File name to save the game into.</param>
		private void Save(string selectedSaveFileName)
		{
			// First we need to acquire the current game status
			Game currentGame = HighLogic.CurrentGame.Updated();

			// If we are not at the space center, we have to reset the startScene to flight,
			// because calling Updated() sets it to space center.
			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				currentGame.startScene = GameScenes.FLIGHT;
			}

			// Now we can save it
			SaveMode s = SaveMode.OVERWRITE; // available SaveModes are: OVERWRITE, APPEND, ABORT
			string filename = GamePersistence.SaveGame(currentGame, selectedSaveFileName, HighLogic.SaveFolder, s);
			Debug.Log(modLogTag + "Game saved in '" + filename + "'");

			Close("SaveDialog completed.");
		}


		/// <summary>
		/// Closes the KISS window, unpauses the game and writes the specified reason into the Debug.Log.
		/// </summary>
		/// <param name="reason">Why/How was the window closed?</param>
		private void Close(string reason)
		{
			// save window position and current settings into config file
			config.SetValue("Window Position", windowPosSize);
			config.SetValue("confirmOverwrite", confirmOverwrite);
			config.SetValue("confirmDelete", confirmDelete);
			config.SetValue("useGameTime", useGameTime);
			config.SetValue("reverseOrder", reverseOrder);
			config.SetValue("selectedDfltSaveNameInt", selectedDfltSaveName);
			config.SetValue("quickSaveMode", quickSaveMode);
			config.SetValue("kissKeyCode", kissKeyCode);
			config.SetValue("kissKeyCaption", kissKeyCaption);

			config.save();

			// code to remove window from UI
			isVisible = false;
			Debug.Log(modLogTag + reason);
			FlightDriver.SetPause(false);
		}
	}
}

