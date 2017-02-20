#ifndef GAME_H
#define GAME_H

#include "GlobalHeader.h"
#include "Paddle.h"
#include "Ball.h"
#include "Pickup.h"
#include <sstream>

using std::stringstream;

extern int gamestate;
extern int gamescore1;
extern int gamescore2;
extern bool playmusic;

SDL_Surface* load_image(string filename);
void apply_surface(int x, int y, SDL_Surface* src, SDL_Surface* dest, SDL_Rect* clip);

class Game
{
    public:
        Game();
        virtual ~Game();

        bool entered; // know if we have entered the game
        void enter_game(); // we can set the AI parameters and stuff in the function eventually
        void leave_game();
        bool handle_event(SDL_Event* event);
        void update();
        void render(SDL_Surface* screen);
        void ai(Paddle* player, int level);

        // Game Variables
        TTF_Font* font;
        SDL_Color fontColor;
        SDL_Surface* img_background;
        SDL_Surface* img_paddle;
        SDL_Surface* img_ball;
        SDL_Surface* text;

        Paddle* paddle_1;
        Paddle* paddle_2;
        Ball* ball;
        Pickup* pickup;

        // Game Sounds
        Mix_Music* music;

        // Assorted Control variables?
        bool control_1, control_2;
        int comp1Level, comp2Level;

    protected:
    private:
};

#endif // GAME_H
