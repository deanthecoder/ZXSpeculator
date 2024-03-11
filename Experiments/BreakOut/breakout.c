// zcc +zx -lndos -create-app -DAMALLOC -o breakout breakout.c
// zcc +zx -lndos -DAMALLOC -o breakout.bin breakout.c
#include <conio.h>
#include <input.h>
#include <graphics.h>
#include <math.h>
#include <arch/zx/spectrum.h>

/** Generic functions */
static unsigned char* attrMem = 22528;
static uint8_t ticks = 0;

static void plot_shade(uint8_t x, uint8_t y, uint8_t density255)
{
    // Plot this pixel based on the density.
    if (density255 == 255 || (uint8_t)rand() < density255)
        plot(x, y);
}

static void c_plot_shade(uint8_t x, uint8_t y, uint8_t density255)
{
    x <<= 2;
    y <<= 2;

    for (uint8_t i = 0; i < 4; ++i)
    {
        const uint8_t py = y + i;
        for (uint8_t j = 0; j < 4; ++j)
            plot_shade(x + i, py, density255);
    }
}

static void drawLogo(uint8_t x, uint8_t y)
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

/** App-specific **/
#define L 22
#define WALL 2
#define DARK_C  INK_BLUE
#define LIGHT_C  INK_CYAN

static uint8_t* grid;

static unsigned int gridToScreen(uint8_t x, uint8_t y)
{
    return ((y + 1) << 5) + 9 + x;
}

static void plotGrid(uint8_t x, uint8_t y)
{
    uint8_t c;
    switch (grid[y * L + x])
    {
        case 0: c = (DARK_C << 3) + DARK_C; break;
        case 1: c = (LIGHT_C << 3) + LIGHT_C; break;
        case WALL: c = (INK_RED << 3) + INK_RED + BRIGHT; break;
    }
    attrMem[gridToScreen(x, y)] = c;
}

static uint8_t peek(uint8_t x, uint8_t y)
{
    return grid[y * L + x];
}

static void flip(uint8_t x, uint8_t y)
{
    uint8_t* p = &grid[y * L + x];
    if (*p == WALL)
        return;
    *p = !(*p);
    plotGrid(x, y);
}

static void flipOneOf(uint8_t x1, uint8_t y1, uint8_t x2, uint8_t y2)
{
    if (rand() & 1)
        flip(x1, y1);
    else
        flip(x2, y2);
}

static void drawBall(uint8_t ballC, uint8_t backC, uint8_t x, uint8_t y, uint8_t d, uint8_t phase)
{
    // Grid -> screen position.
    x = ((x + 9) << 3);
    y = ((y + 1) << 3);

    uint8_t o = phase << 1; // Phase -> offset.
    switch (d)
    {
        case 0:
        {
            y += 6;
            x += o;
            y -= o;
            break;
        }

        case 1:
        {
            x += o;
            y += o;
            break;
        }

        case 2:
        {
            x += 6;
            x -= o;
            y += o;
            break;
        }

        case 3:
        {
            x += 6;
            y += 6;
            x -= o;
            y -= o;
            break;
        }
    }

    for (uint8_t i = 0; i < 6; ++i)
    {
        textcolor(ballC);
        textbackground(backC);
        plot(x, y);
        plot(x + 1, y);
        plot(x, y + 1);
        plot(x + 1, y + 1);
    }
}

static void undrawBall(uint8_t x, uint8_t y)
{
    x = ((x + 9) << 3);
    y = ((y + 1) << 3);

    clga(x, y, 8, 8);
}

void main()
{
    srand(0);
    zx_border(BLUE);
    textbackground(BLACK);
    textcolor(WHITE);
    clrscr();
    ticks = 0;

    // Footer.
    gotoxy(40, 23);
    printf("Breakout (@DeanTheCoder)");

    // Init the grid.
    grid = malloc(L * L);
    memset(grid, 1, L * L); // Clear all.
    for (uint8_t y = 0; y < L; ++y)
    {
        // Half-filled background.
        memset(&grid[y * L], 0, L >> 1);

        // Sides.
        grid[y * L] = grid[y * L + L - 1] = WALL;
    }

    memset(grid, WALL, L);  // Top wall.
    memset(&grid[L * (L - 1)], WALL, L);  // Bottom wall.

    // Draw the grid.
    for (uint8_t y = 0; y < L; ++y)
    {
        for (uint8_t x1 = 0; x1 < L; ++x1)
            plotGrid(x1, y);
    }    

    // Init ballz.
    uint8_t x1 = 2, x2 = L - 2 - 1;
    uint8_t y1 = L / 2, y2 = L / 2;
    uint8_t ox1 = x1, oy1 = y1;
    uint8_t ox2 = x2, oy2 = y2;
    uint8_t d1 = 0, d2 = 2; // 0: Up/right, 1: Down/right, 2: Down/left, 3: Up/left.

    // Loop.
    in_GetKeyReset();
    while (!in_GetKey())
    {
        // Refresh the logo.
        drawLogo(3, 1);

        // Move ballz.
        for (uint8_t phase = 0; phase < 4; ++phase)
        {
            for (uint8_t ballC = 0; ballC < 2; ++ballC)
            {
                uint8_t x, y, d, mask;
                uint8_t* b;
                if (!ballC)
                {
                    x = x1;
                    y = y1;
                    d = d1;
                    undrawBall(ox1, oy1); ox1 = x; oy1 = y;
                    drawBall(CYAN, BLUE, x, y, d, phase);
                }
                else
                {
                    x = x2;
                    y = y2;
                    d = d2;
                    undrawBall(ox2, oy2); ox2 = x; oy2 = y;
                    drawBall(BLUE, CYAN, x, y, d, phase);
                }

                if (phase == 3)
                {
                    switch (d)
                    {
                        case 0:
                            /*
                            32
                            .1
                            */
                            mask = ((peek(x, y - 1) != ballC) << 2) + ((peek(x + 1, y - 1) != ballC) << 1) + (peek(x + 1, y) != ballC);
                                if (mask == 0b000) { ++x; --y; }
                            else if (mask == 0b001) { ++x; --y; }
                            else if (mask == 0b010) { flip(x + 1, y - 1); d += 2; }
                            else if (mask == 0b011) { flip(x + 1, y - 1); --y; d += 3; }
                            else if (mask == 0b100) { ++x; --y; }
                            else if (mask == 0b101) { flipOneOf(x + 1, y, x, y - 1); d += 2; }
                            else if (mask == 0b110) { flip(x + 1, y - 1); ++x; d += 1; }
                            else if (mask == 0b111) { flipOneOf(x + 1, y, x, y - 1); d += 2; }
                            break;
                        case 1:
                            /*
                            .3
                            12
                            */
                            mask = ((peek(x + 1, y) != ballC) << 2) + ((peek(x + 1, y + 1) != ballC) << 1) + (peek(x, y + 1) != ballC);
                                if (mask == 0b000) { ++x; ++y; }
                            else if (mask == 0b001) { ++x; ++y; }
                            else if (mask == 0b010) { flip(x + 1, y + 1); d += 2; }
                            else if (mask == 0b011) { flip(x + 1, y + 1); ++x; d += 3; }
                            else if (mask == 0b100) { ++x; ++y; }
                            else if (mask == 0b101) { flipOneOf(x + 1, y, x, y + 1); d += 2; }
                            else if (mask == 0b110) { flip(x + 1, y + 1); ++y; d += 1; }
                            else if (mask == 0b111) { flipOneOf(x + 1, y, x, y + 1); d += 2; }
                            break;
                        case 2:
                            /*
                            1.
                            23
                            */
                            mask = ((peek(x, y + 1) != ballC) << 2) + ((peek(x - 1, y + 1) != ballC) << 1) + (peek(x - 1, y) != ballC);
                                if (mask == 0b000) { --x; ++y; }
                            else if (mask == 0b001) { --x; ++y; }
                            else if (mask == 0b010) { flip(x - 1, y + 1); d += 2; }
                            else if (mask == 0b011) { flip(x - 1, y + 1); ++y; d += 3; }
                            else if (mask == 0b100) { --x; ++y; }
                            else if (mask == 0b101) { flipOneOf(x - 1, y, x, y + 1); d += 2; }
                            else if (mask == 0b110) { flip(x - 1, y + 1); --x; d += 1; }
                            else if (mask == 0b111) { flipOneOf(x - 1, y, x, y + 1); d += 2; }
                            break;
                        case 3:
                            /*
                            21
                            3.
                            */
                            mask = ((peek(x - 1, y) != ballC) << 2) + ((peek(x - 1, y - 1) != ballC) << 1) + (peek(x, y - 1) != ballC);
                                if (mask == 0b000) { --x; --y; }
                            else if (mask == 0b001) { --x; --y; }
                            else if (mask == 0b010) { flip(x - 1, y - 1); d += 2; }
                            else if (mask == 0b011) { flip(x - 1, y - 1); --x; d += 3; }
                            else if (mask == 0b100) { --x; --y; }
                            else if (mask == 0b101) { flipOneOf(x - 1, y, x, y - 1); d += 2; }
                            else if (mask == 0b110) { flip(x - 1, y - 1); --y; d += 1; }
                            else if (mask == 0b111) { flipOneOf(x - 1, y, x, y - 1); d += 2; }
                            break;
                    }

                    // Move ball to the next position.
                    d = d & 0b11;
                }

                if (!ballC)
                {
                    x1 = x;
                    y1 = y;
                    d1 = d;
                }
                else
                {
                    x2 = x;
                    y2 = y;
                    d2 = d;
                }
            }
        }
    }
}
