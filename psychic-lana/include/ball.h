#ifndef BALL_H
#define BALL_H

#include "GlobalHeader.h"
#include "Game_Object.h"
#include "paddle.h"
#include <Math.h>

#define BALL_RADIUS 8

#define MAX_ANGLE 20.0
#define PI 3.14159265
#define RAD 0.0174532925
#define DEG 57.2957795

extern int gamescore1;
extern int gamescore2;

class Ball: public Game_Object
{
    public:
        Ball(Paddle* p1, Paddle* p2);
        virtual ~Ball();
        void update();
        void collision(Paddle* pad);
        void score(int player);

        // Pointers to the paddles for collision
        Paddle* pad1;
        Paddle* pad2;

        // Sound effects for bouncing around
        Mix_Chunk* wallhit;
        Mix_Chunk* paddlehit;

        int power;

    protected:
    private:
};

#endif // BALL_H
