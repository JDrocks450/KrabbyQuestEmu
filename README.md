# KrabbyQuestEmu
A project aiming to restore Spongebob: Krabby Quest by emulating the original game engine using the original game files. (Not distributing copyrighted content!)

## How does it work?
The original game came with seven .DAT files that stored all of the content that the game engine would use to display levels. 
As of now I have reverse engineered the level data format and the tool is able to open and view every level in the game. On top of that, most of the 
objects in each level have been named and their function is obvious. This way, the emulator can use the original game files for the textures,
object-placement, and sound.

## Why is this important?
Because media preservation is something that is important in my eyes. Under no circumstances is this project ever intended to be profitable.
This game is from my childhood and it is important that it can run on modern systems for people who might remember this game to enjoy once again.

## What is completed?
Below is a roadmap:

#### Completed:
- Editor
  - Level Selection
  - Object Database: 
    > This stores information about each object in the game including which *assets* the object should use in-game, its *Name* and *ID*.
  - Asset Editor: 
    > A tool to name each file that has been dumped from the .DAT files and to link it to in-game objects 
  - Asset Database:
    > Stores information about the dumped files from .DAT files to make reimplementing objects easier
#### Under Development:
- Game
  - Opening Levels
  - Reimplementing Objects
  - Adding Music
  - Map Screen with Progression
  - Highscores
  - UI
  - New Features?

## Is this legal?
I am not distributing any copyrighted content, this is only a tool to read the game files that were distributed from another source. 
Additionally, I am doing this only in the name of media preservation - not for profit in any way. I do not plan on accepting any donations.
