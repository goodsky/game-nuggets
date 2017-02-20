#include "Timer.h"

// ctor
Timer::Timer()
{
	startTicks = 0;
	pausedTicks = 0;
	paused = false;
	started = false;
}

// Start the Timer
void Timer::start()
{
	started = true;

	paused = false;

	startTicks = SDL_GetTicks();
}

// Stop the Timer
void Timer::stop()
{
	started = false;
	paused = false;
}

// Pause the Timer
void Timer::pause()
{
	// Make sure the timer is running and not paused
	if (started && !paused)
	{
		paused = true;

		pausedTicks = SDL_GetTicks() - startTicks;
	}
}

// Unpause the Timer
void Timer::unpause()
{
	if (paused)
	{
		paused = false;

		startTicks = SDL_GetTicks() - pausedTicks;
		pausedTicks = 0;
	}
}

// Get the ticks
int Timer::getTicks()
{
	if (started)
	{
		if (paused)
			return pausedTicks;
		else
			return SDL_GetTicks() - startTicks;
	}

	return 0;
}

bool Timer::isStarted()
{
	return started;
}
bool Timer::isPaused()
{
	return paused;
}
