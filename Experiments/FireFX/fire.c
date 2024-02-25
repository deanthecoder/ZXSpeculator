// zcc +zx -lndos -create-app -o fire fire.c
#include <stdio.h>
#include <conio.h>
#include <input.h>
#include <graphics.h>
#include <arch/zx/spectrum.h>

static unsigned char* attrMem = 22528;

void main()
{
    zx_border(2);
    textbackground(BLACK);
    clrscr();

    // Footer.
    gotoxy(37, 23);
    printf("Retro Fire (@DeanTheCoder)");
    for (int i = 736; i < 768; ++i)
        attrMem[i] = 0x39;

    // Prep.
    in_GetKeyReset();
    uint8_t grid[384] = { 0 };
    uint8_t colLUT[256];
    uint8_t backLUT[256];
    srand(0);

    // Build color look up tables.
    for (int b = 0; b < 256; ++b)
    {
        if (b > 90)
        {
            colLUT[b] = WHITE;
            backLUT[b] = WHITE;
        }
        else if (b > 80)
        {
            colLUT[b] = YELLOW;
            backLUT[b] = WHITE;
        }
        else if (b > 70)
        {
            colLUT[b] = WHITE;
            backLUT[b] = YELLOW;
        }
        else if (b > 60)
        {
            colLUT[b] = YELLOW;
            backLUT[b] = YELLOW;
        }
        else if (b > 50)
        {
            colLUT[b] = RED;
            backLUT[b] = YELLOW;
        }
        else if (b > 40)
        {
            colLUT[b] = YELLOW;
            backLUT[b] = RED;
        }
        else if (b > 30)
        {
            colLUT[b] = RED;
            backLUT[b] = RED;
        }
        else if (b > 20)
        {
            colLUT[b] = RED;
            backLUT[b] = BLACK;
        }
        else
        {
            colLUT[b] = BLACK;
            backLUT[b] = BLACK;
        }
    }

    // Loop.
    while (!in_GetKey())
    {
        // Seed the base line.
        for (uint8_t i = 0; i < 64; ++i)
            grid[320 + i] = rand() % 256;

        // Apply the algorithm.
        for (int i = 0; i < 320; ++i)
        {
            int sum = grid[i + 31];
            sum += grid[i + 32];
            sum += grid[i + 33];
            sum += grid[i + 64];

            grid[i] = (uint8_t)((sum * 3) >> 4);
        }

        // Update the screen.
        int o = 0;
        for (uint8_t y = 0; y < 10; ++y)
        {
            for (uint8_t x = 0; x < 32; ++x, ++o)
            {
                const uint8_t b = grid[o];
                textcolor(colLUT[b]);
                textbackground(backLUT[b]);

                c_plot(x << 1, 27 + (y << 1));
            }
        }
    }
}
