# KrabbyQuestEmu
A project aiming to restore Spongebob: Krabby Quest by emulating the original game engine using the original game files.

## How does it work?
The original game came with seven .DAT files that stored all of the content that the game engine would use to display levels. 
### File Summary
 - *res1.dat* contains the byte offsets, filenames, and file sizes of all the assets stored in *res2.dat*.
 - *res2.dat* contains Assets such as level data, 3D models, textures, and sound effects.

The *ManifestViewer* tool can open and extract these files to a Destination Directory, which is required to play the game.

### Databases
Hardcoding is the worst strategy here, thus there are 2 databases that the Editor creates and the Game Engine read to create the gameplay experience. 
Some aspects have to be hard-coded such as player movement, button behavior, gates, conveyors, etc. 
- *BlockDB*: Levels are stored in blocks, each block represents an object in-game. The BlockDB is a way for developers to tie textures, parameters, names, etc.
to these blocks since all this information was hardcoded in the original game. 

  - You can edit this by loading a level in the Krabby Quest Tools program.
- *AssetDB*: This ties Assets/Sounds to the *BlockDB entries* so that the game can load these assets and apply them in accordance to the parameters set in the *BlockDB*
  - You can edit this by clicking: "Edit AssetDB for Selected Directory" in the Krabby Quest Tools program.

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
#### Under Development:
- Game
  - Opening Levels
  - Reimplementing Objects (~50% complete)
  - Adding Music
  - Map Screen with Progression
  - Highscores
  - UI
  - New Features?

## Is this legal?
I am not distributing any copyrighted content, this is only a tool to read the game files that were distributed from another source. 
Additionally, I am doing this only in the name of media preservation - not for profit in any way. I do not plan on accepting any donations.
