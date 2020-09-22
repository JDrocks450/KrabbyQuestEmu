# KrabbyQuestEmu
A project aiming to restore Spongebob: Krabby Quest by emulating the original game engine using the original game files.

## How does it work?
The original game came with seven .DAT files that stored all of the content that the game engine would use to display levels. 
### File Summary
 - *res1.dat* contains the byte offsets, filenames, and file sizes of all the assets stored in *res2.dat*.
 - *res2.dat* contains Assets such as level data, 3D models, textures, and sound effects.
 - *res3,4,5,6.dat* music tracks
 - *res7.dat* a duplicate asset, likely compiler error that this file is even included.

The *ManifestViewer* tool can open and extract these files to a Destination Directory, which is required to play the game.

### Databases
Hardcoding is the worst strategy here, thus there are 2 databases that the Editor creates and the Game Engine read to create the gameplay experience. 
Some aspects have to be hard-coded such as player movement, button behavior, gates, conveyors, etc. 
- *BlockDB*: Levels are stored in blocks, each block represents an object in-game. The BlockDB is a way for developers to tie textures, parameters, names, etc.
to these blocks since all this information was hardcoded in the original game. 

  > You can edit this by loading a level in the Krabby Quest Tools program.
- *AssetDB*: This ties Assets/Sounds to the *BlockDB entries* so that the game can load these assets and apply them in accordance to the parameters set in the *BlockDB*
  > You can edit this by clicking: "Edit AssetDB for Selected Directory" in the Krabby Quest Tools program.

## Why is this important?
Because media preservation is something that is important in my eyes. Under no circumstances is this project ever intended to be profitable.
This game is from my childhood and it is important that it can run on modern systems for people who might remember this game to enjoy once again.

## What is completed?
Below is a roadmap:

#### Completed:
- Editor
  - Level Selection
    > All levels are openable
  - Block Database Editor: 
    > This stores information about each object in the game including name, parameters, editor color, rotation, and whether its *Integral* or *Decorative*
  - Asset Database Editor: 
    > A tool to link extracted Assets to BlockDB entries.
  - Manifest Viewer
    > Handles the extraction of Assets to a directory *Vital*
  - Map Screen Editor
    > Allows creators to add and remove waypoints on the map screen to any level including custom ones.
- Game
  - Loading Levels
    > All levels are loadable, some with incomplete objects
  - Start Screen
    > Menus in the game were bland, so they are being replaced with newer menu styles that are less abrasive.
  - Map Screen
    > This menu has also gotten a visual improvement.
  - Basic UI
    > Menus, Bonus/Patty collection UI is implemented
#### Under Development:
- Setup Procedure
  - Make automated setup procedure
- Game
  - Reimplementing Objects (~80% complete)
  - Highscores
  - New Features?

## Custom Content?
This project largely aims to captivate creators. The editor is designed to allow you to have all the tools that I used to fully restore the game.
Custom maps will be able to use all of the existing functionality from any level in the game already. 
Creators will be able to place markers to their maps anywhere on the existing map in the game. On top of that,
the textures the game uses are completely extracted so they can be patched with ease - including the map textures meaning custom Map-Screens are 
entirely possible.
This means that you will able to create full adventures through the editor in the future.
#### However
This is not supported yet. In future it is on my to-do list but as of now the editor is not able to officially edit
levels at all. 

## How Do I Play?
Right now, the setup is difficult as this project is still incomplete. However, it is possible.

*You must have Blender installed!*
 
 1. Extract an original copy of the game to a directory.
 2. Build and Run *KrabbyQuestTools*
 3. Open the ManifestViewer tool.
 4. Drag and Drop *res1.dat* into the *Manifest Path Box*, *res2.dat* into the *Data Path Box*, and Input a Directory where it all should go. (Remember this directory!)
 5. Click Open, numerous file paths should appear on the left. If they have, press "Extract All" at the top.
 6. Create a new tab and click "Export Models"
 7. The workspace you have open should be in the box. Input where you have blender installed into the box.
 8. Click Export and be patient!
 9. Create another new tab, now the *AssetDB Directory* box should be filled with your directory. Click *Open AssetDB Editor*
 10. Finally, Create one more new tab, click *Database Options* and PUSH all the changes you made to where Krabby Quest Game is installed on your pc.
 11. Build and Run Krabby Quest Game

## Is this legal?
I am not distributing any copyrighted content, this is only a tool to read the game files that were distributed from another source. 
Additionally, I am doing this only in the name of media preservation - not for profit in any way. I do not plan on accepting any donations.
