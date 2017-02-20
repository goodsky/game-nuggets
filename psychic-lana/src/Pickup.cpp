#include "Pickup.h"

Pickup::Pickup(double initX, double initY, int type) :
Game_Object("media/PickupBase.png", 50, 50)
{
    x = initX;
    y = initY;
    powerType = type;
}

void Pickup::update()
{
    if(powerType == noPower)
    {
        y = x = -500.0;
    }
}

Pickup::~Pickup()
{
    //dtor
}
