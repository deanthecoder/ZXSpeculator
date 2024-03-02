// zcc +zx -lndos -create-app -o sand sand.c
#include <conio.h>
#include <input.h>
#include <graphics.h>
#include <arch/zx/spectrum.h>

#define W 64
#define H 46
#define C 16

static unsigned char* attrMem = 22528;
static uint8_t i, x, y;
static uint8_t grid[H][W];
static uint8_t grains[C][2];

static void setFixedGrain(uint8_t x, uint8_t y)
{
    grid[y][x] = 2;
    c_plot(x, y);
}

void main()
{
    zx_border(BLUE);
    textbackground(BLUE);
    textcolor(YELLOW);
    clrscr();

    // Prep.
    in_GetKeyReset();
    srand(0);

    // Footer.
    gotoxy(33, 23);
    printf("Sandy Situation (@DeanTheCoder)");
    for (unsigned int j = 752; j < 768; ++j)
        attrMem[j] = PAPER_BLUE + INK_CYAN;
    draw(0, 183, 254, 183);

    // Set the ground layer.
    memset(grid, 0, W * H);
    for (x = 0; x < W; ++x)
        setFixedGrain(x, H - 1);

    // Obstacles.
    textcolor(RED);
    x = 50; y = 8;
    for (i = 0; i < 10; ++i)
        setFixedGrain(1 + x--, y);

    x -= 8; y += 18;
    for (i = 0; i < 12; ++i)
    {
        setFixedGrain(x + i, y);
        setFixedGrain(x + i - 10, y - 10);
    }
    textcolor(YELLOW);

    // Initial falling grains.
    for (i = 0; i < C; ++i)
    {
        grains[i][0] = 1 + (rand() % (W - 1));
        grains[i][1] = 1 + (rand() % (H - 2));
    }

    // Loop.
    while (!in_GetKey())
    {
        // Advance the frame.
        for (i = 0; i < C; ++i)
        {
            x = grains[i][0];
            y = grains[i][1];

            grains[i][1]++;
            if (!grid[y + 1][x])
            {
                // Fall directly below.
                c_plot(x, y + 1);
                c_unplot(x, y);
                continue;
            }
            if (x > 0 && !grid[y + 1][x - 1])
            {
                // Fall to the left.
                grains[i][0]--;
                c_plot(x - 1, y + 1);
                c_unplot(x, y);
                continue;
            }
            if (x < W - 1 && !grid[y + 1][x + 1])
            {
                // Fall to the right.
                grains[i][0]++;
                c_plot(x + 1, y + 1);
                c_unplot(x, y);
                continue;
            }

            // Stop falling.
            grid[y][x] = 1;

            // Spawn a new grain.
            unsigned int r = rand() % 16;
            r *= r;
            r = r * W / 512;
            grains[i][0] = W / 2 + ((rand() % 2) ? 1 : -1) * r;
            grains[i][1] = 0;
        }
    }
}
