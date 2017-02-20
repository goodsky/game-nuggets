#ifndef MENU_H
#define MENU_H

#include "GlobalHeader.h"

extern int SCREEN_HEIGHT;
extern int SCREEN_WIDTH;
extern int gamestate;
extern bool playmusic;

void apply_surface(int x, int y, SDL_Surface* src, SDL_Surface* dest, SDL_Rect* clip);

class Menu
{
    public:
        Menu();
        virtual ~Menu();

        bool entered; // know if we have entered the menu
        void enter_menu();
        void leave_menu();
        bool handle_event(SDL_Event* event);
        void update();
        void render(SDL_Surface* screen);

        void intro_screen(SDL_Surface* screen);

        // Fonts
        TTF_Font* titleFont;
        TTF_Font* optionFont;
        SDL_Surface* text;

        // Menu Sounds
        Mix_Music* music;
        Mix_Chunk* beep;

        // Other information we might want
        int menu_items;
        string* menu_titles;

        int selected;
        int colorOffset;

    protected:
    private:
};

#endif // MENU_H
