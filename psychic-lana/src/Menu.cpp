#include "Menu.h"

Menu::Menu()
{
    // Open the questionably legal .ttf file
    titleFont = TTF_OpenFont("media/arial.ttf", 42);
    optionFont = TTF_OpenFont("media/arial.ttf", 24);

    // Open music and sounds
    music = Mix_LoadMUS("media/menu.wav");
    beep = Mix_LoadWAV("media/beep.wav");

    // Create the menu options
    menu_items = 2;
    menu_titles = new string[menu_items];
    menu_titles[0] = "Play Now";
    menu_titles[1] = "Quit";

    // Default
    entered = false;
}

Menu::~Menu()
{
    TTF_CloseFont(titleFont);
    TTF_CloseFont(optionFont);

    Mix_FreeMusic(music);
    Mix_FreeChunk(beep);

    delete[] menu_titles;
}

// This will start music, set defaults and start the menu
void Menu::enter_menu()
{
    // Set game state and default selected item in the menu
    gamestate = 0; // MAIN_MENU;

    // Set up the other parameters
    text = NULL;
    selected = 0;
    colorOffset = 0; // static text is lame. colorful text is sexy.

    // Start the music if desired
    if (playmusic && Mix_PlayingMusic() == 0)
        Mix_PlayMusic(music, -1);

    // We've been entered
    entered = true;
}

void Menu::leave_menu()
{
    // Go to the game
    gamestate = 1; // GAME_PLAY;

    // Stop music
    if (Mix_PlayingMusic() != 0)
        Mix_HaltMusic();

    // We've been abandoned!
    entered = false;
}

bool Menu::handle_event(SDL_Event* event)
{
    // Key Presses
    if (event->type == SDL_KEYDOWN)
    {
        // Up Key
        if (event->key.keysym.sym == SDLK_UP)
        {
            selected--;
            Mix_PlayChannel(-1, beep, 0);

            if (selected < 0) selected = menu_items-1;
        }
        // Down Key
        else if (event->key.keysym.sym == SDLK_DOWN)
        {
            selected++;

            Mix_PlayChannel(-1, beep, 0);
            if (selected == menu_items) selected = 0;
        }
        // Enter Key
        else if (event->key.keysym.sym == SDLK_RETURN)
        {
            // Do selected action
            if (selected == 0)
                leave_menu();
            else if (selected == 1)
                return false;
        }
    }

    return true;
}

void Menu::update()
{

}

void Menu::render(SDL_Surface* screen)
{
    // Draw the screen
    // Set the screen to be black
    SDL_FillRect(screen, &screen->clip_rect, SDL_MapRGB(screen->format, 0, 0, 0));

    // Draw Title 'Phycic-Lana'
    SDL_Color color = {255,255,255};
    text = TTF_RenderText_Solid(titleFont, "Psychic-Lana: the game", color);
    apply_surface((SCREEN_WIDTH - text->w)/2, 200, text, screen, NULL);
    SDL_FreeSurface(text);

    // Set the fluctuating colors
    SDL_Color select = {255, colorOffset < 64 ? 128-colorOffset : colorOffset, colorOffset < 64 ? 128-colorOffset : colorOffset};

    // Draw all the menu items
    for (int i = 0; i < menu_items; ++i)
    {
        if (selected == i)
            text = TTF_RenderText_Solid(optionFont, menu_titles[i].c_str(), select);
        else
            text = TTF_RenderText_Solid(optionFont, menu_titles[i].c_str(), color);

        apply_surface((SCREEN_WIDTH - text->w)/2, 300 + i*50, text, screen, NULL);
        SDL_FreeSurface(text);
    }

    // Update the new color
    colorOffset -= 3;
    if (colorOffset < 0) colorOffset = 128;
}

// This is the most dirty code I've written all day.
// Look past the ugly, and embrace the fact that all it needs to do is display our names for 3 seconds.
void Menu::intro_screen(SDL_Surface* screen)
{
    SDL_Event myevent;

    // Fade in words
    SDL_Color color = {0xff, 0xff, 0xff};
    text = TTF_RenderText_Solid(optionFont, "by Brandon Scott and Skyler Goodell", color);
    int startTick = SDL_GetTicks();

    // This outer for-loop goes through the fade-in, display, and fade-out phases
    for (int i = 0; i < 3; ++i)
    {
        // do the appropriate phase
        for (int alpha = 0; alpha <= 255; alpha += 1)
        {
            // Set the screen to be black
            SDL_FillRect(screen, &screen->clip_rect, SDL_MapRGB(screen->format, 0, 0, 0));

            // Set the alpha if we are in phase 0 or 2
            if (i == 0)
                SDL_SetAlpha(text, SDL_SRCALPHA, alpha);
            else if (i == 2)
                SDL_SetAlpha(text, SDL_SRCALPHA, (255-alpha));

            // Draw Text
            apply_surface((SCREEN_WIDTH - text->w)/2, (SCREEN_HEIGHT-text->h)/2, text, screen, NULL);

            // Sync to the clock
            int wait = SDL_GetTicks() - startTick;
            if ((wait - 8) > 0) SDL_Delay(wait-8);
            startTick = SDL_GetTicks();

            // Poll Events
            if (SDL_PollEvent(&myevent)){
                if (myevent.type == SDL_KEYDOWN)
                {
                    SDL_FreeSurface(text);
                    return;
                }
                else if (myevent.type == SDL_QUIT) break;
            }

            // Sync
            SDL_Flip(screen);
        }
    }

    SDL_FreeSurface(text);
}
