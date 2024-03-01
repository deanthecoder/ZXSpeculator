// zcc +zx -lndos -create-app -o matrix matrix.c
#include <stdio.h>
#include <conio.h>
#include <input.h>
#include <graphics.h>
#include <arch/zx/spectrum.h>

static unsigned char* attrMem = 22528;
static uint8_t colLUT[768];
static uint8_t x, y;
static int i;

void main()
{
    zx_border(0);
    textbackground(BLACK);
    clrscr();

    // Footer.
    gotoxy(37, 23);
    printf("The Matrix (@DeanTheCoder)");
    for (i = 754; i < 768; ++i)
        attrMem[i] = PAPER_GREEN;

    // Prep.
    in_GetKeyReset();
    srand(0);

    memset(colLUT, 0, 768);
    i = 0;
    while (i < 768 - 6)
    {
        colLUT[i++] = INK_GREEN;
        colLUT[i++] = INK_GREEN;
        colLUT[i++] = INK_GREEN;
        colLUT[i++] = INK_GREEN + BRIGHT;
        colLUT[i++] = INK_WHITE;
        colLUT[i++] = INK_WHITE + BRIGHT;

        i += rand() % 20;
    }

    // Prime the screen with characters.
    for (y = 0; y < 23; ++y)
    {
        for (x = 0; x < 64; x += 2) {
            gotoxy(x, y);
            putchar(0x3A + (rand() % 33));
        }
    }

    // Loop.
    while (!in_GetKey())
    {
        // Draw the lines.
        i = 0;
        for (x = 0; x < 32; ++x)
        {
            for (y = 0; y < 23; ++y)
                attrMem[(y << 5) + x] = colLUT[i++];
        }

        // Scroll.
        x = colLUT[767];
        memmove(colLUT + 1, colLUT, 767);
        colLUT[0] = x;
    }
}
