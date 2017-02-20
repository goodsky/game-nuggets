#ifndef PICKUP_H
#define PICKUP_H

#include "GlobalHeader.h"
#include "Game_Object.h"

class Pickup : public Game_Object
{
    public:
        Pickup(double initX, double initY, int type);
        void update();
        virtual ~Pickup();

        int powerType;
    protected:
    private:
};

#endif // PICKUP_H
