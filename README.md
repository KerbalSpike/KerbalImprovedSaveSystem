KISS - Kerbal Improved Save System
==================================

is an add-on for the game Kerbal Space Program that provides an improved "Save game as..." mechanic while in flight.

The KISS window is activated by pressing **"F8"** and provides the same functionality as the stock "Quicksave as..." dialog (*"MOD"+"F5"*) for creating named savegames. In addition KISS provides the following improvements:

1. It lists all existing savegames for your current game and allows you to select a previous savegame to overwrite it.
2. The suggested name for the new savegame can be configured:
   * the current timestamp and the name of the active vessel (ship) (*"{timestamp}_{vesselName}"*) (default).
   * the name of the active vessel (ship) and the current timestamp (*"{vesselName}_{timestamp}"*).
   * *"quicksave"* (useful for quicksave mode).
3. The timestamp can be either your current system time ("YYYYMMDD_hhmmss") or KSP ingame time ("Y1_D01_0_24_56").
4. QuickSave mode: if enabled pressing **"F8"** will instantly save your current game without showing any GUI (press **"MOD" + "F8"** to show window again).
5. It allows you to delete old savegames directly from within the game.
6. It can create savegames while in a vessel that is moving on the ground (because it pauses the game before saving)!

In other words: **KISS** allows you to manage your quicksaves without the need to memorize quicksave names and without having to type long, meaningful names every time!

### Current version: 2.1.0 ###
for Kerbal Space Program 1.2.1 (also compatible with 1.2.0 and 1.1.3)


See [changelog.txt] (https://github.com/KerbalSpike/KerbalImprovedSaveSystem/blob/develop/changelog.txt) for list of changes.  
Visit the [Forum Thread] (http://forum.kerbalspaceprogram.com/index.php?/topic/138001-113-kiss-kerbal-improved-save-system/) to tell me what you think about KISS or report errors.

#### Planned updates ("soon" :tm: ): ####
* make KISS also available in the space center.

#### Possible future extensions: ####
* make KISS replace the stock quicksave entirely.
* provide a similar functionality for saving crafts in the editor (VAB/SPH). 
