// pyschic-lana project - (basic pong video game using C++ and SDL)
// authors: Brandon Scott and Skyler Goodell
// 5/2/2012
#include "GlobalHeader.h"
#include "Globals.h"
#include "Menu.h"
#include "Game.h"
#include <iostream>
#include <string>
using namespace std;

// *****************************
// GAME CLASSES
// *****************************
Menu* menu;
Game* game;

// *****************************
// MAIN FUNCTION
// *****************************
int main(int argc, char** args)
{
    // Try to initialize SDL and the plugins.
    // If init returns false then we have an error.
    if (!init("psychic-lana the game")) return 1;

    // Create the game and menu
    menu = new Menu();
    game = new Game();

    // Fade in the 'created by' screen
    menu->intro_screen(screen);

    // Main Game Loop
    bool running = true;
    SDL_Event myevent;

    // Timer for FPS
    Timer FPSTimer;
    while (running)
    {
        // Start timing this iteration
        FPSTimer.start();

        // Check if we have changed states, and if so, call the initialization
        if (gamestate == MAIN_MENU && !menu->entered) menu->enter_menu();
        if (gamestate == GAME_PLAY && !game->entered) game->enter_game();

        // Poll Events
        while (SDL_PollEvent(&myevent))
        {
            if (gamestate == MAIN_MENU)
                running = menu->handle_event(&myevent);
            else if (gamestate == GAME_PLAY)
                game->handle_event(&myevent);

            // 'X' out of the window
            if (myevent.type == SDL_QUIT) running = false;
        }

        // Update
        if (gamestate == MAIN_MENU)
            menu->update();
        else if (gamestate == GAME_PLAY)
            game->update();

        // Render
        if (gamestate == MAIN_MENU)
            menu->render(screen);
        else if (gamestate == GAME_PLAY)
            game->render(screen);

        // Sync
        SDL_Flip(screen);

        // Regulate FPS
		if (FPSTimer.getTicks() < 1000 / FRAMES_PER_SECOND)
			SDL_Delay((1000/FRAMES_PER_SECOND) - FPSTimer.getTicks());
    }

    // De-Allocate Memory and close the program
    delete menu;
    delete game;
    clean_up();

    return 0;
}

