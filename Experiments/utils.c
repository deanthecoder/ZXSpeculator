#include "utils.h"

unsigned char* attrMem = 22528;
static uint8_t ticks = 0;

void plot_shade(uint8_t x, uint8_t y, uint8_t density255)
{
    if (!density255)
        return;

    // Plot this pixel based on the density.
    if (density255 == 255 || (uint8_t)rand() < density255)
        plot(x, y);
}

/// @brief Plot a chunky pixel with a certain shade.
/// @param x Range 0 to 64
/// @param y Range 0 to 48
/// @param density255 
void c_plot_shade(uint8_t x, uint8_t y, uint8_t density255)
{
    if (!density255)
        return;

    x <<= 2;
    y <<= 2;

    for (uint8_t i = 0; i < 4; ++i)
    {
        const uint8_t py = y + i;
        for (uint8_t j = 0; j < 4; ++j)
            plot_shade(x + j, py, density255);
    }
}

void drawLogo(uint8_t x, uint8_t y)
{
    if (ticks > 30)
        return;

    if (ticks == 0)
    {
        // Seed the pixels - Initially all black.
        const char* logo[23] =
        {
            " **********",
            "..***....***",
            " .***   ..***",
            " .***    .***",
            " .***    ***",
            " **********",
            "..........",
            "",
            " ***********",
            ".*...***...*",
            ".   .***  .",
            "    .***",
            "    .***",
            "    *****",
            "   .....",
            "",
            "   *********",
            "  ***.....***",
            " ***     ...",
            ".***",
            "..***     ***",
            " ..*********",
            "  ........."
        };

        for (uint8_t j = 0; j < 23 * 2; ++j)
        {
            textcolor(BLACK);
            uint8_t* l = logo[j >> 1];
            const uint8_t w = strlen((const char*)l);
            const uint8_t py = j + y;
            for (uint8_t px = x; px < x + w; ++px)
            {
                switch (*l++)
                {
                    case '*':
                        c_plot(px, py);
                        break;
                    case '.':
                        c_plot_shade(px, py, 40);
                        break;
                }
            }
        }

        ++ticks;
        return;
    }

    // Apply the color over time.
    y >>= 1;
    x >>= 1;

    static const uint8_t logoC[] = { INK_WHITE + BRIGHT, INK_WHITE, INK_CYAN, INK_GREEN, INK_MAGENTA, INK_MAGENTA, INK_RED, INK_RED };
    uint8_t j = ticks, i = 0;
    while (j >= 0 && i < 7)
    {
        attrMem[((j + y) << 5) + x + i] = logoC[j & 0x07];
        --j; ++i;
    }

    ++ticks;
}

void draw_footer(const char* s)
{
    gotoxy(64 - strlen(s), 23);
    printf(s);
}

void in_waitForKey() {
    while (!in_GetKey())
        ;
    while (in_GetKey())
        ;
}