#ifndef GAME_OBJECT_H
#define GAME_OBJECT_H

#include "GlobalHeader.h"

extern int SCREEN_HEIGHT;
extern int SCREEN_WIDTH;

//Wall Collision Return Values
enum wallCollisionValues
    {wNONE,
     wUL, wUU, wUR,
     wLL,      wRR,
     wDL, wDD, wDR};

enum powerUp
{
    noPower, power
};

SDL_Surface* load_image(string filename);
void apply_surface(int x, int y, SDL_Surface* src, SDL_Surface* dest, SDL_Rect* clip);

class Game_Object
{
    public:
        Game_Object();
        Game_Object(string filename);
        Game_Object(string filename, int cWidth, int cHeight);
        Game_Object(string filename, int cWidth, int cHeight, double cX, double cY);
        Game_Object(string filename, int cWidth, int cHeight, double cX, double cY, double limitX, double limitY, double limitTotal);
        bool objCollision(Game_Object* obj2);
        int screenCollide();
        int screenEdgeCollide();
        void update();
        void velocityLimit();
        void renderObj(SDL_Surface* screen);
        virtual ~Game_Object();

        SDL_Surface* img_graphic;

        int height, width;
        double x, y;
        double vx, vy;
        double ax, ay;
        double xLim, yLim, totalLim;

    protected:
    private:
};

#endif // GAME_OBJECT_H
