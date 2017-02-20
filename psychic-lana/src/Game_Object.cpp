#include "Game_Object.h"

//Constructors
Game_Object::Game_Object()
{
    //Default Constructor
    img_graphic = NULL;
    width = height = 0;
     x = y = vx = vy = ax = ay = 0;
    xLim = yLim = totalLim = -1; //No limit
}
Game_Object::Game_Object(string filename)
{
    //Graphic
    img_graphic = load_image(filename);
    width = height = 0;
    x = y =vx = vy = ax = ay = 0;
    xLim = yLim = totalLim = -1; //No limit
}
Game_Object::Game_Object(string filename, int cWidth, int cHeight)
{
    //"Boundaries
    img_graphic = load_image(filename);
    width = cWidth;
    height = cHeight;
    x = y = vx = vy = ax = ay = 0;
    xLim = yLim = totalLim = -1; //No limit
}
Game_Object::Game_Object(string filename, int cWidth, int cHeight,
                         double cX, double cY)
{
    //"Position
    img_graphic = load_image(filename);
    width = cWidth;
    height = cHeight;
    x = cX;
    y = cY;
    vx = vy = ax = ay = 0;
    xLim = yLim = totalLim = -1; //No limit
}
Game_Object::Game_Object(string filename, int cWidth, int cHeight,
                         double cX, double cY,
                         double limitX, double limitY, double limitTotal)
{
    //"Limits
    img_graphic = load_image(filename);
    height = cHeight;
    width = cWidth;
    x = cX;
    y = cY;
    xLim = limitX;
    yLim = limitY;
    totalLim = limitTotal;
    vx = vy = ax = ay = 0;
}

Game_Object::~Game_Object()
{
    // Don't forget to deallocate EVERYTHING you allocate brandon
    SDL_FreeSurface(img_graphic);
}

bool Game_Object::objCollision(Game_Object* obj2)
{
    if(x + width/2  >= obj2->x - obj2->width/2  &&
       x - width/2  <= obj2->x + obj2->width/2  &&
       y + height/2 >= obj2->y - obj2->height/2 &&
       y - height/2 <= obj2->y + obj2->height/2)
        return true;
    return false;
}

int Game_Object::screenCollide()
{
    //Collision Code
    //1 2 3
    //8 0 4
    //7 6 5
    if(x <= 0) //if LEFT
    {
        if(y <= 0) return wUL;             //if UP LEFT
        if(y >= SCREEN_HEIGHT) return wDL; //if DOWN LEFT
        return wLL;                        //if only LEFT
    }
    if(x >= SCREEN_WIDTH) //if RIGHT
    {
        if(y <= 0) return wUR;             //if UP RIGHT
        if(y >= SCREEN_HEIGHT) return wDR; //if DOWN RIGHT
        return wRR;                        //if only RIGHT
    }
    if(y <= 0) return wUU;                 //if only UP
    if(y >= SCREEN_HEIGHT) return wDD;     //if only DOWN
    return wNONE;                          //if no collision
}
int Game_Object::screenEdgeCollide()
{
    //Collision Code
    //1 2 3
    //8 0 4
    //7 6 5
    if(x - width <= 0) //if LEFT
    {
        if(y - height <= 0) return wUL;             //if UP LEFT
        if(y + height >= SCREEN_HEIGHT) return wDL; //if DOWN LEFT
        return wLL;                                 //if only LEFT
    }
    if(x + width >= SCREEN_WIDTH) //if RIGHT
    {
        if(y - height <= 0) return wUR;             //if UP RIGHT
        if(y + height >= SCREEN_HEIGHT) return wDR; //if DOWN RIGHT
        return wRR;                                 //if only RIGHT
    }
    if(y - width <= 0) return wUU;                 //if only UP
    if(y + height >= SCREEN_HEIGHT) return wDD;    //if only DOWN
    return wNONE;                                  //if no collision
}

void Game_Object::update()
{
    //Acceleration added to velocity
    vx += ax;
    vy += ay;
    //Check and apply speed limits
    velocityLimit();
    //Velocity added to position
    x += vx;
    y += vy;
}

void Game_Object::velocityLimit()
{
    double temp;
    if(xLim >= 0)
    {
        if(fabs(vx) > xLim)
        {
            if(vx > 0) vx = xLim;
            else vx = -xLim;
        }
    }
    if(yLim >= 0)
    {
        if(fabs(vy) > yLim)
        {
            if(vy > 0) vy = yLim;
            else vy = -yLim;
        }
    }
    if(totalLim >= 0)
    {
        temp = pow(vx, 2) + pow(vy, 2);
        if(temp > pow(totalLim, 2))
        {
            temp = totalLim / temp;
            vx *= temp;
            vy *= temp;
        }
    }
}

void Game_Object::renderObj(SDL_Surface* screen)
{
    int xCenter, yCenter;
    xCenter = (int)floor(x - width/2 + 0.5);
    yCenter = (int)floor(y - height/2 + 0.5);
    apply_surface(xCenter, yCenter, img_graphic, screen, NULL);
}
