# Line of Command: The Sweep
A command line interpretation of the classic minesweeper, but a bit fancier. 
It is based on .NET 6 and targets Windows. Support for other operating systems might follow later.

## Game objectives
The goal of the game is to flag all the mines on the playing field. Because of that, you must figure out where they are. You want to discover all tiles of the board, except for the ones with a mine, since digging in a tile with a mine will cause it to explode, ouch. 

To help you along, each discovered tile will give you a bit of information, which you can use to flag the mines. If a tile has no number, there are no mines in the area around it. If it has a number, that indicates how many mines there are in the square around the tile. If there are as many mines as there are undiscovered tiles, all undiscovered tiles must have mines in them, so they should be flagged. If there already are as many mines as there are flags, all other tiles in the area around the number are free of mines and can be discovered. You don't always have all the information straight away and sometimes you need to use some logic to progress in more complex boards.

Once you have marked all mines and discovered all the empty squares, you win the game.

### Controls
You can move the cursor (shown in game by an X) using the arrow keys or W, A, S, and D. When you want to dig to discover a tile, press the spacebar or ther Enter key. To put a flag on a tile, press F or E.

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
