using KerbalImprovedSaveSystem.Extensions;
using KSP.IO;
using UnityEngine;

namespace KerbalImprovedSaveSystem
{
	/// <summary>
	/// Start Kerbal improved savegame system as soon as possible and keep loaded.
	/// </summary>
	[KSPAddon(KSPAddon.Startup.EveryScene, false)] 
	public class KerbalImprovedSavegameSystem : MonoBehaviour
	{
		// used to identify log entries of this mod
		public static string modLogTag = "[KISS] ";

		private static Rect _windowPosition = new Rect();
		private GUIStyle _windowStyle, _labelStyle;
		private bool _hasInitStyles = false;

		/// <summary>
		/// Main part of this Add-on; used to react to keyboard input and do all the magic stuff :)
		/// </summary>
		void Update()
		{
			//on Alt-F5
			if (GameSettings.QUICKSAVE.GetKey() && GameSettings.MODIFIER_KEY.GetKey()) {
				
				Debug.Log(modLogTag + "Init GUI.");
				if (!_hasInitStyles)
					InitStyles();
				RenderingManager.AddToPostDrawQueue(0, OnDraw);

				// available SaveModes are: OVERWRITE, APPEND, ABORT
				SaveMode s = SaveMode.OVERWRITE;
				string filename = GamePersistence.SaveGame ("MyOwnSaveFile", HighLogic.SaveFolder, s);
				Debug.Log (modLogTag + "Game saved in '" + filename + "'");
			}
		}

//		public override void OnSave(ConfigNode node)
//		{
//			PluginConfiguration config = PluginConfiguration.CreateForType<KerbalImprovedSavegameSystem>();
//
//			config.SetValue("Window Position", _windowPosition);
//			config.save();
//		}
//
//		public override void OnLoad(ConfigNode node)
//		{
//			PluginConfiguration config = PluginConfiguration.CreateForType<KerbalImprovedSavegameSystem>();
//
//			config.load();
//			_windowPosition = config.GetValue<Rect>("Window Position");
//		}

		private void OnDraw()
		{
			_windowPosition = GUILayout.Window(1337, _windowPosition, OnWindow, "Save current game progress as...", _windowStyle);

			if (_windowPosition.x == 0f && _windowPosition.y == 0f)
			{
				_windowPosition = _windowPosition.CenterScreen();
			}

		}

		private void OnWindow(int windowId)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Filename:", _labelStyle);
			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}

		private void InitStyles()
		{
			_windowStyle = new GUIStyle(HighLogic.Skin.window);
			_windowStyle.fixedWidth = 250f;

			_labelStyle = new GUIStyle(HighLogic.Skin.label);
			_labelStyle.stretchWidth = true;

			_hasInitStyles = true;
		}
	}
}

