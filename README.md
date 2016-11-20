KISS - Kerbal Improved Save System
==================================

is an add-on for the game Kerbal Space Program that provides an improved "Save game as..." mechanic while in flight.

The KISS window is activated by pressing **"F8"** (can be configured, see notes below) and provides the same functionality as
the stock "Quicksave as..." dialog (*"MOD"+"F5"*) for creating named savegames. In addition KISS provides the following improvements:

1. It lists all existing savegames for your current game and allows you to select a previous savegame to overwrite it.
2. The suggested name for the new savegame can be configured:
   * the current timestamp and the name of the active vessel (ship) (*"{timestamp}_{vesselName}"*) (default).
   * the name of the active vessel (ship) and the current timestamp (*"{vesselName}_{timestamp}"*).
   * *"quicksave"* (useful for quicksave mode).
3. The timestamp can be either your current system time ("YYYYMMDD_hhmmss") or KSP ingame time ("Y1_D01_0_24_56").
4. Quicksave mode: if enabled pressing **"F8"** will instantly save your current game without showing any GUI (press **"MOD" + "F8"** to show window again).
5. It allows you to delete old savegames directly from within the game!
6. It can create savegames while in a vessel that is moving on the ground (because it pauses the game before saving)!

In other words: **KISS** allows you to manage your quicksaves without the need to memorize quicksave names!

#### Known limitations of the keybinding feature: ####
* you **can't** use any modifier keys (Shift, Ctrl, Alt, Command,...) and you also **can't** have any key combinations for KISS, you can only pick a simple, single key.
* you have to disable Capslock before assigning a new key if you want to make sure it is labeled correctly in the UI.
* on international keyboards, you have to hit special keys twice(like the accent keys " ´ " and " ^ " on the German keyboard), because they do not produce a character when hit for the first time, but I need the character to label the key correctly.
* all keys that to not produce characters like Backspace, Arrow Keys, etc. are labeled with their English names, regardless of keyboard language used.
* the "Print" key is for some reason not detected by my code (although it is listed as a viable KeyCode in Unity).

### Current version: 2.1.0 ###
for Kerbal Space Program 1.2.1 (also compatible with 1.2.0 and 1.1.3)


See [changelog.txt] (https://github.com/KerbalSpike/KerbalImprovedSaveSystem/blob/develop/changelog.txt) for list of changes.  
Visit the [Forum Thread] (http://forum.kerbalspaceprogram.com/index.php?/topic/138001-113-kiss-kerbal-improved-save-system/) to tell me what you think about KISS or report errors.
Download KISS on [SpaceDock] (http://spacedock.info/mod/583/Kerbal%20Improved%20Save%20System)

#### Planned updates ("soon" :tm: ): ####
* make KISS also available in the space center.

#### Possible future extensions: ####
* make KISS replace the stock quicksave entirely.
* provide a similar functionality for saving crafts in the editor (VAB/SPH). 
