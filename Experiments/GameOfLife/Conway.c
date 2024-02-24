// zcc +zx -lndos -create-app -o Conway Conway.c
#include <stdio.h>
#include <conio.h>
#include <input.h>
#include <spectrum.h>

#define CELL_SET 0
#define CELL_UNSET 56

static uint8_t grid[768];
static uint8_t backup[768];
static unsigned char* attrMem = 22528;

static void advance()
{
    memcpy(backup, grid, 768);

    for (int i = 33; i < 735; ++i)
    {
        const uint8_t sum = backup[i - 33] + backup[i - 32] + backup[i - 31]
                    + backup[i - 1] + backup[i + 1]
                    + backup[i + 31] + backup[i + 32] + backup[i + 33];

        if (!backup[i])
            grid[i] = sum == 3;
        else
            grid[i] = sum == 2 || sum == 3;
    }
}

void main()
{
    //printf("\x01\x20");
    zx_border(6);
    clrscr();
    gotoxy(27, 23);
    textcolor(BLUE);
    printf("Conway's Game of Life (@DeanTheCoder)");
    in_GetKeyReset();

    memset(grid, 0, 768);

    srand(0);
    for (int i = 33; i < 735; i++)
        grid[i] = rand() < (RAND_MAX / 2);

    while (!in_GetKey())
    {
        for (int i = 33; i < 736; ++i)
            attrMem[i] = grid[i] ? CELL_SET : CELL_UNSET;

        advance();
    }
}
