#include "Ball.h"

Ball::Ball(Paddle* p1, Paddle* p2) :
Game_Object("media/ball.png", BALL_RADIUS, BALL_RADIUS,
            SCREEN_WIDTH / 2, SCREEN_HEIGHT / 2)
{
    // Seed the random numbers
    srand(SDL_GetTicks());

    // Load Sound effects
    wallhit = Mix_LoadWAV("media/wallbounce.ogg");
    paddlehit = Mix_LoadWAV("media/paddlebounce.ogg");

    //Initialize velocity
    // Start at a random direction
    double ang = rand()%90 + 45;
    vx = 10*sin((ang*3.1415)/180.0);
    vy = 10*cos((ang*3.1415)/180.0);
    xLim = PADDLE_WIDTH + BALL_RADIUS;

    //Point to the paddles
    pad1 = p1;
    pad2 = p2;
    power = 1;
}

Ball::~Ball()
{
    // Close WAVS
    Mix_FreeChunk(wallhit);
    Mix_FreeChunk(paddlehit);
}


//update: Updates ball positons and checks collision
//Input : void
//Result: x (ball) is changed by vx
//        y (ball) is changed by vy
//Output: void
void Ball::update()
{
    int wallCollide;
    Game_Object::update();
    //Wall Collision
    wallCollide = Game_Object::screenEdgeCollide();

    if((wallCollide == wUU && vy < 0) || (wallCollide == wDD && vy > 0))
    {
        Mix_PlayChannel(-1, wallhit, 0);
        vy *= -1;
    }
    if(wallCollide == wRR|| wallCollide == wUR || wallCollide == wDR)
        score(1);
    if(wallCollide == wLL || wallCollide == wUL || wallCollide == wDL)
        score(2);

    //Paddle Collision
    collision(pad1);
    collision(pad2);
}

//collision: Changes velocity based on paddle collision
//Input : padY = y position of colliding paddle
//Result: vy is changed based on padY and y (ball) if collision
//        vx is negated if collision
//Output: void
void Ball::collision(Paddle* pad)
{
    if(objCollision(pad) && vx * pad->getSide() > 0)
    {
        // First play the sound
        Mix_PlayChannel(-1, paddlehit, 0);

        /* Brandon's Method 5/8/2012
        vy = (y - pad->y +
              pad-> vy * (1 + (1 - 1 / power)))
              * (1 + 1 / power)
              / 10;
        vx *= -1;
        power++;

        if(vx > 0) vx += power / 10;
        else vx -= power / 10;
        */

        // Note from Skyler: I just want to try some different methods here.
        // it still doesn't feel completely natural yet... Feel free to revert
        // back or try something different.
        // NOTE: I waste a lot of cycle converting between rads and degrees...
        //       but I don't think we are worried about run-speed right now.
        //       we can change it later if we want.
        // Skyler's Method 5/8/2012

        // Find the normal vector of the point we've hit on the paddle
        // The displacement on the paddle we have hit
        double hity = y - pad->y;

        // The angle can be modeled as linear (because it is small)
        // The angle linearly blends between -maxang and maxang
        //double maxang = 20.0; //Removed and replaced with a defined value
        double ang = (2.0*hity/pad->height)*MAX_ANGLE;
        // Calculate the normal vector
        double ny = sin(ang*RAD);
        double nx = cos(ang*RAD)*pad->getSide()*-1;

        // Use the normal vector and the velocity vector to get the bounce vector
        // The equation for bounce is: V - 2*(V dot n)*n
        double newvy = vy - 2*(vx*nx + vy*ny)*ny;
        double newvx = vx - 2*(vx*nx + vy*ny)*nx;

        // Also we need to increase the velocity slightly
        // Use an exponential that caps at velocity equal to
        // maxvel
        double maxvel = 20.0;
        double newvel = sqrt(newvy*newvy + newvx*newvx);

        // Set the new velocity with a logistic growth model
        newvy += 0.2*newvy * (1 - newvel/maxvel);
        newvx += 0.2*newvx * (1 - newvel/maxvel);

        // Finally cap the angle so that no balls go vertical
        newvel = sqrt(newvy*newvy + newvx*newvx);
        double newang = atan2(newvy, newvx)*DEG;

        // Cap the angles between 75,-75 and 105,105
        if (newvx > 0.0)
        {
            if (newang > 65.0) newang = 65.0;
            if (newang < -65.0) newang = -65.0;
        }
        else
        {
            if (newang > 0.0 && newang < 115.0) newang = 115.0;
            if (newang < 0.0 && newang > -115.0) newang = -115.0;
        }

        // Set the velocity
        vy = newvel*sin(newang*RAD);
        vx = newvel*cos(newang*RAD);
    }
}

void Ball::score(int player)
{
    x = SCREEN_WIDTH / 2;
    y = SCREEN_HEIGHT / 2;

    // Start at a random direction
    double ang = rand()%90 + 45;

    if(player == 1)
    {
        vx = -10*sin((ang*3.1415)/180.0);
        vy = 10*cos((ang*3.1415)/180.0);
        ++gamescore1;
    }else
    {
        vx = 10*sin((ang*3.1415)/180.0);
        vy = 10*cos((ang*3.1415)/180.0);
        ++gamescore2;
    }
    power = 1;
}
