#include "Paddle.h"

Paddle::Paddle(int player)
: Game_Object("media/paddle.png", PADDLE_WIDTH, PADDLE_HEIGHT,
              PADDLE_WALL_DISTANCE, SCREEN_HEIGHT / 2)
{
    paddleSide = player;
    if(player == 2) //Right Player
        x = SCREEN_WIDTH - PADDLE_WALL_DISTANCE;
    //Limits
    xLim = 0;                  //Immobile
    yLim = PADDLE_SPEED_LIMIT; //Limit
    totalLim = -1;             //Ignore
    power = 0;
}

int Paddle::getSide()
{
    if(paddleSide == 1)
    return -1; //LEFT
    if(paddleSide == 2)
    return 1; //RIGHT
    return 0; //ERROR
}

void Paddle::move(int direction)
{
    ay = direction*2;
}

void Paddle::update()
{
    int wallCollide;
    Game_Object::update();
    wallCollide = Game_Object::screenCollide();
    if(wallCollide == wUU)
    {
        ay = vy = 0;
        y = 0;
    }
    if(wallCollide == wDD)
    {
        ay = vy = 0;
        y = SCREEN_HEIGHT;
    }
    if(ay > 0)
        ay--;
    if(ay < 0)
        ay++;
}

Paddle::~Paddle()
{
    //dtor
}
