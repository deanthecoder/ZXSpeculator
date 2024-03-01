// zcc +zx -lndos -create-app -o 10print 10print.c
#include <stdio.h>
#include <conio.h>
#include <input.h>
#include <graphics.h>
#include <arch/zx/spectrum.h>

static unsigned char *attrMem = 22528;
static unsigned int x, y, i;

void main()
{
    zx_border(BLUE);
    textbackground(CYAN);

    // Prep.
    in_GetKeyReset();
    srand(0);

    // Loop.
    while (!in_GetKey())
    {
        clrscr();

        // Draw.
        for (y = 0; y < 192; y += 8)
        {
            for (x = 0; x < 256; x += 8)
            {
                switch (rand() % 2)
                {
                case 0:
                    plot(x + 1, y);
                    drawr(7, 7);
                    plot(x, y + 1);
                    drawr(7, 7);
                    break;
                case 1:
                    plot(x + 7, y);
                    drawr(-7, 7);
                    plot(x + 8, y + 1);
                    drawr(-7, 7);
                    break;
                }
            }
        }

        // Footer.
        clga(152, 184, 104, 8);
        drawb(151, 183, 106, 10);

        gotoxy(40, 23);
        printf("10PRINT (@DeanTheCoder)");
        for (x = 755; x < 768; ++x)
            attrMem[x] = PAPER_BLUE + INK_WHITE + BRIGHT;

        // Fill.
        i = 0;
        while (i < 8)
        {
            x = rand() % 256;
            y = rand() % 192;

            if (point(x, y) + point(x + 1, y) + point(x, y - 1) + point(x, y + 1))
                continue;

            fill(x, y);
            i++;
        }
    }
}
