#ifndef GLOBALS_H
#define GLOBALS_H

#define MAIN_MENU 0
#define GAME_PLAY 1

// *********************
// Game Variables
// *********************
int SCREEN_WIDTH = 800;
int SCREEN_HEIGHT = 600;
int SCREEN_BPP = 32;

int FRAMES_PER_SECOND = 32;

// MAKE FALSE IF YOU DON'T WANT TO LISTEN TO THE MUSIC FOR NOW
bool playmusic = true;

// Game state says if you are in the main menu, game play or anywhere else
int gamestate = MAIN_MENU;

// Game Scores and other attributes
int gamescore1, gamescore2;

// Global Screen Surface
// This Surface is what is rendered to the screen.
SDL_Surface* screen;

// ******************************
// INITIALIZE THE SDL LAYER!
// ******************************
bool init(string caption)
{
	// Initialize all SDL subsystems
	if (SDL_Init(SDL_INIT_EVERYTHING) == -1)
	{
		cout << "Failed to initialize SDL." << endl;
		return false;
	}

	// Set up the screen
	screen = SDL_SetVideoMode(SCREEN_WIDTH, SCREEN_HEIGHT, SCREEN_BPP, SDL_SWSURFACE);

	if (screen == NULL)
	{
		cout << "Error grabbing pointer to the screen." << endl;
		return false;
	}

    // Initialize the TTF plugin
	if (TTF_Init() == -1)
	{
		cout << "Failed to initialize TTF plugin." << endl;
		return false;
	}

	// Initialize the SDL Mixer plugin
	if (Mix_OpenAudio(22050, MIX_DEFAULT_FORMAT, 2, 4096) == -1)
	{
		cout << "Failed to initialize Mixer plugin." << endl;
		return false;
	}

	// Set the window caption
	SDL_WM_SetCaption(caption.c_str(), NULL);

	// Set the window icon
    SDL_Surface* icon = IMG_Load("media/icon.ico");
    SDL_WM_SetIcon(icon, NULL);
    SDL_FreeSurface(icon);

	return true;
}

// ******************************
// CLEAN UP EVERYTHING
// ******************************
// Close SDL and free all the surfaces that we created
void clean_up()
{
    Mix_CloseAudio();

	TTF_Quit();

	SDL_Quit();
}

// ***************************************************
// LOAD IN AN IMAGE (supports more than just bitmaps)
// ***************************************************
SDL_Surface* load_image(string filename)
{
	// Loaded image and the optimized image
	SDL_Surface* loadedImage = NULL;
	SDL_Surface* optimizedImage = NULL;

	// Load the raw image
	loadedImage = IMG_Load(filename.c_str());

	// Watch for errors
	if (loadedImage == NULL)
	{
		cout << "Could not load Image in load_image. " << filename << endl;
		return NULL;
	}

	// The image loaded, so create the optimized version
	optimizedImage = SDL_DisplayFormat(loadedImage);

	// Free the old image
	SDL_FreeSurface(loadedImage);

	// Return the optimized version
	return optimizedImage;
}

// ***************************************************
// BLIT IN AN IMAGE
// ***************************************************
void apply_surface(int x, int y, SDL_Surface* src, SDL_Surface* dest, SDL_Rect* clip = NULL)
{
	SDL_Rect offset;
	offset.x = x;
	offset.y = y;

	SDL_BlitSurface(src, clip, dest, &offset);
}

#endif
