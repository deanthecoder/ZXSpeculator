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
    srand(0);

    // Build color look up tables.
    uint8_t colLUT[256];
    for (int b = 0; b < 256; ++b)
    {
        if (b > 90)
            colLUT[b] = INK_WHITE + PAPER_WHITE;
        else if (b > 80)
            colLUT[b] = INK_YELLOW + PAPER_WHITE;
        else if (b > 70)
            colLUT[b] = INK_WHITE + PAPER_YELLOW;
        else if (b > 60)
            colLUT[b] = INK_YELLOW + PAPER_YELLOW;
        else if (b > 50)
            colLUT[b] = INK_RED + PAPER_YELLOW;
        else if (b > 40)
            colLUT[b] = INK_YELLOW + PAPER_RED;
        else if (b > 30)
            colLUT[b] = INK_RED + PAPER_RED;
        else if (b > 20)
            colLUT[b] = INK_RED + PAPER_BLACK;
        else
            colLUT[b] = INK_BLACK + PAPER_BLACK;
    }

    // Prime the screen with dots.
    for (uint8_t y = 0; y < 10; ++y)
    {
        for (uint8_t x = 0; x < 32; ++x)
            c_plot(x << 1, 27 + (y << 1));
    }

    // Loop.
    int i;
    while (!in_GetKey())
    {
        // Seed the base line.
        for (i = 320; i < 384; ++i)
            grid[i] = rand() % 256;

        // Apply the algorithm.
        for (i = 0; i < 320; ++i)
        {
            int sum = grid[i + 31];
            sum += grid[i + 32];
            sum += grid[i + 33];
            sum += grid[i + 64];

            grid[i] = (uint8_t)((sum * 3) >> 4);
        }

        // Update the screen.
        for (i = 0; i < 320; ++i)
            attrMem[416 + i] = colLUT[grid[i]];
    }
}
