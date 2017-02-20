#ifndef PADDLE_H
#define PADDLE_H

#include "GlobalHeader.h"
#include "Game_Object.h"

#define PADDLE_WIDTH 10
#define PADDLE_HEIGHT 100
#define PADDLE_SPEED_LIMIT 16
#define PADDLE_WALL_DISTANCE 32



class Paddle: public Game_Object
{
    public:
        Paddle(int player);
        int getSide();
        void move(int direction);
        void update();
        virtual ~Paddle();

        int paddleSide;
        int power;
    protected:
    private:
};

#endif // PADDLE_H
