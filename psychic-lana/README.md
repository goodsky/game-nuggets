psychic-lana
============

- Let's make a game... it might take more than 5 hours.

-------------------------------------------------------

SG Changelog:
5/8/2012
* Added any-key-to-skip for splash screen
* Added score text
* Changed the ball to paddle collision routine- check out my changes and let me know what you think. There may be bugs.
* Added random sounds and music (note: I don't like sounds and music... so I picked the first ones I had on my harddrive. We can change them whenever.)
* If you want to turn off music, go to Globals.h and set "playmusic = false".

2/20/2017
* Moved old project to games-nuggets.

-------------------------------------------------------

BS Changelog:
* Additions: ai support, acceleration physics, acceleration based collision physics
* Removals: Nothing removed
* Changes: variable names (user -> paddle_1, comp ->paddle_2, etc), screen resoltion to 800x600, paddle graphics to reflect angled collisions, Bug fixes, ability to collide with ball when it's past paddle, ball getting stuck in walls or paddles

-------------------------------------------------------

SDL Plugins:

http://www.lazyfoo.net/SDL_tutorials/lesson03/index.php
This is the extension for .pngs

http://www.lazyfoo.net/SDL_tutorials/lesson07/index.php
This is the extension for .ttf text

http://www.lazyfoo.net/SDL_tutorials/lesson11/index.php
This is the extension for .wavs
