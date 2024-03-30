#ifndef UTILS_H
#define UTILS_H

#include <conio.h>
#include <input.h>
#include <graphics.h>
#include <math.h>
#include <arch/zx/spectrum.h>

extern unsigned char* attrMem;

/// @brief Plot a single pixel using a random dither.
void plot_shade(uint8_t x, uint8_t y, uint8_t density255);

/// @brief Plot a single chunky pixel using a random dither.
void c_plot_shade(uint8_t x, uint8_t y, uint8_t density255);

/// @brief Draw a frame from the DTC logo. (Call once per game loop.)
void drawLogo(uint8_t x, uint8_t y);

/// @brief Draw the footer text at the bottom of the screen.
void draw_footer(const char* s);

/// @brief Block until a key is pressed.
void in_waitForKey();

#endif