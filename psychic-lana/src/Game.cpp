#include "Game.h"

// Game Constructor
// Sets Default Values
Game::Game()
{
    // Set Values for the variables
    font = TTF_OpenFont("media/arial.ttf", 32); // I wonder if this is legal...
    fontColor.r = 255; fontColor.g = 255; fontColor.b = 255;
    img_background = load_image("media/background.png");
    img_paddle = load_image("media/paddle.png");
    img_ball = load_image("media/ball.png");

    // Open music
    music = Mix_LoadMUS("media/sexygame.wav"); // lolol sexygame

    // Create Objects
    paddle_1 = new Paddle(1);
    paddle_2 = new Paddle(2);
    ball = new Ball(paddle_1, paddle_2);
    pickup = new Pickup(200, 200, 1);

    // Default value
    entered = false;
}

// Game Deconstructor
Game::~Game()
{
    // Close SDL resources
    TTF_CloseFont(font);

    SDL_FreeSurface(img_background);
    SDL_FreeSurface(img_paddle);
    SDL_FreeSurface(img_ball);

    // Close WAVS
    Mix_FreeMusic(music);

    // deallocate memory
    delete paddle_1;
    delete paddle_2;
    delete ball;
}

void Game::enter_game()
{
    // Music?
    if (playmusic && Mix_PlayingMusic() == 0)
        Mix_PlayMusic(music, -1);

    // Set AI status
    //true = player control, false = computer control
    control_1 = true;
    comp1Level = 3;
    control_2 = false;
    comp2Level = 3;

    // Set the default score
    gamescore1 = 0;
    gamescore2 = 0;

    // We've been entered (bow-chika wow ow)
    entered = true;
}

void Game::leave_game()
{
    // Stop Music
    if (Mix_PlayingMusic() != 0)
        Mix_HaltMusic();

    // We've been left
    entered = false;
}

bool Game::handle_event(SDL_Event* event)
{
    return true;
}

// Update the game state
void Game::update()
{
    // Check the key states
    Uint8* keystates = SDL_GetKeyState(NULL);

    //Left Paddle
    if(control_1)
    {
        // Move Left paddle up and down
        if (keystates[SDLK_a])
            paddle_1->move(-1);
        else if (keystates[SDLK_z])
            paddle_1->move(1);
        else
        {
            if(paddle_1->vy > 0)
                paddle_1->vy--;
            if(paddle_1->vy < 0)
                paddle_1->vy++;
        }
    }else
    {
        Game::ai(paddle_1, comp1Level);
    }

    if(control_2)
    {
        // Move Right paddle up and down
        if (keystates[SDLK_UP])
            paddle_2->move(-1);
        else if (keystates[SDLK_DOWN])
            paddle_2->move(1);
        else
        {
            if(paddle_2->vy > 0)
                paddle_2->vy--;
            if(paddle_2->vy < 0)
                paddle_2->vy++;
        }
    }else
    {
        Game::ai(paddle_2, comp2Level);
    }

    //Update the ball
    paddle_1->update();
    paddle_2->update();
    ball->update();

    //Update powerup
    pickup->update();
    if(pickup->objCollision(ball) && pickup->powerType != 0)
    {
        if(ball->vx > 0)
            paddle_1->power = pickup->powerType;
        else paddle_2->power = pickup->powerType;
        pickup->powerType = noPower;
    }
}

// Render the game
void Game::render(SDL_Surface* screen)
{
    // Draw the background first
    apply_surface(0, 0, img_background, screen, NULL);

    // Draw the paddles and ball
    paddle_1->renderObj(screen);
    paddle_2->renderObj(screen);
    ball->renderObj(screen);
    pickup->renderObj(screen);

    // Draw the Scores
    // Player 1 Score
    stringstream txt;
    txt << ":" << gamescore1;
    text = TTF_RenderText_Solid(font, txt.str().c_str(), fontColor);
    apply_surface(50, 15, text, screen, NULL);
    SDL_FreeSurface(text);

    // Player 2 Score
    txt.str("");
    txt << ":" << gamescore2;
    text = TTF_RenderText_Solid(font, txt.str().c_str(), fontColor);
    apply_surface(SCREEN_WIDTH-(text->w + 50), 15, text, screen, NULL);
    SDL_FreeSurface(text);
}

void Game::ai(Paddle* player, int level)
{
    //ai behaviour:
    //0 = no behaviour
    //1 = track ball when returned, stay until return
    //2 = track ball when returned, move towards center otherwise
    //3 = always track ball
    if(level > 0)
    {
        if(ball->vx * player->getSide() > 0 || level >= 3)
        {
            double difference;
            difference = ball->y - player->y;
            if(difference > player->height / 4)
                player->move(1);
            if(difference < player->height / 4)
                player->move(-1);
        }else if(level >= 2)
        {
            if(player->y > SCREEN_HEIGHT / 2 + player->height ||
               player->y < SCREEN_HEIGHT / 2 - player->height)
            {
                double difference;
                difference = SCREEN_HEIGHT / 2 - player->y;
                if(difference > player->height / 4)
                    player->move(1);
                if(difference < player->height / 4)
                    player->move(-1);
            }else
            {
                if(player->vy > 0)
                    player->vy--;
                if(player->vy < 0)
                    player->vy++;
            }
        }else
        {
            if(player->vy > 0)
                player->vy--;
            if(player->vy < 0)
                player->vy++;
        }
    }
}
