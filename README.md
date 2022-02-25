# Line of Command: The Sweep
A command line interpretation of the classic minesweeper, but a bit fancier. 
It is based on .NET 6 and targets Windows and support for other operating systems might follow later.

## Game objectives
The goal of the game is to flag all the mines on the playing field. Because of that, you must figure out where they are. You want to discover all squares of the board, except for the ones with a mine, since digging in a square with a mine will cause it to explode, which will kill you. 

To help you, each discovered square will give you a bit of information, which you can use to flag the mines. If a square has no number, there are no mines in the area around it. If it has a number, that indicates how many mines there are around the square. If there are as many mines as there are undiscovered squares, all undiscovered squares must have mines, and can thus be flagged. If there already are as many mines as there are flags, all other squares in the area around the number are free of mines and can be discovered.

Once you have marked all mines and discovered all the empty squares, you win the game.

## How to play
You can move the cursor (shown in game by an X) using the arrow keys or W, A, S, and D. Then when you want to dig on a square, press the spacebar or ther Enter key or to flag a square, press F or E.

- To start a new game press N
- To quit the game press Q or Escape

## Current Options

### Support for increased mine counting radius
In addition to the standard radius of 1, thus covering a 3x3 square, you can also increase the search radius of each square. For example, a radius of 2 will look at a larger radius and thus can find more mines and give you bigger numbers.

### Mine counting radius wrapping
When you enable this feature, the squares on one side of the board will also count the mines on one side, as if the two sides of the board are next to each other.

### Enabling / Disabling of flags and question marks
To make it more difficult to track where you saw the mines, you can disable the use of flags. This way, you need to remember or rethink where the mines are every time you revisit a part of the board. 
On the other hand, to make it easier, you can also choose to enable the ability to place a question mark instead of a flag.

### Enabling / Disabling of automatic discovery from empty tiles
By default when you explore a tile without a number, it will automatically explore the tiles around it since it cant have any mines. If you want, this can be disabled.

### Ability to hide all numbers except at the cursor (fog of war)
Another way to make the game more difficult is to hide the progress where your cursor isn't. This way you need to constantly move around to read the numbers, see what areas are discovered and where the flags or mines are.

### Configurable safe zones
To give you a bit more information at the beginning of the game, you can increase the safe zone. In this area around the starting dig, there will be no mines.

### Lives
To improve your chances on the more challenging configurations, you can have multiple lives. This way, if you hit a mine, you will get a second (or third) chance.

## Planned Features and Options
- Hardcore mode with count down timer
- Highscores
- Solvability checker - never get into a coin toss situation again
- Hints - let the computer do it for you
- Cooperative multiplayer
- Campaign
- Undo
