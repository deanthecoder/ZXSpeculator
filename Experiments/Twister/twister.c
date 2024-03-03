// zcc +zx -lndos -create-app -lmz -DAMALLOC -o twister twister.c
#include <conio.h>
#include <input.h>
#include <graphics.h>
#include <math.h>
#include <arch/zx/spectrum.h>

static unsigned char* attrMem = 22528;
static uint8_t x, y;
static unsigned int i;

static void line(int8_t x1, int8_t x2, uint8_t b, uint8_t attr)
{
    uint8_t c = 0;
    while (x1 < x2)
    {
        attrMem[b * 32 + x1 + 16] = attr + (c++ < 3 ? BRIGHT : 0);
        x1++;
    }
}

void main()
{
    zx_border(BLUE);
    textbackground(BLACK);
    textcolor(WHITE);
    clrscr();

    // Prep.
    in_GetKeyReset();

    // Footer.
    gotoxy(41, 23);
    printf("Twister (@DeanTheCoder)");
    
    // Precalc.
    int8_t* anim = malloc(120 * 24 * 6);
    for (i = 0; i < 120; ++i)
    {
        gotoxy(0, 0);
        printf("%d%%", (int)((i + 1) * 0.833));

        float am = 120.0f + cos(i * 0.0524f) * 100.0f;
        float an = -3.141f + sin(i * 0.0524f) * 3.141f;

        for (y = 0; y < 23; ++y)
        {
            int8_t* p = &anim[24 * 6 * i + y * 6];

            float fv = 8.0f * y / am + an;
            int8_t x1 = 8.0f * sin(fv);
            int8_t x2 = 8.0f * sin(fv + 1.571f);
            int8_t x3 = -x1;
            int8_t x4 = -x2;

            p[0] = x1;
            if (x2 < p[0]) p[0] = x2;
            if (x3 < p[0]) p[0] = x3;
            if (x4 < p[0]) p[0] = x4;
            p[5] = x1;
            if (x2 > p[5]) p[5] = x2;
            if (x3 > p[5]) p[5] = x3;
            if (x4 > p[5]) p[5] = x4;

            if (x1 < x2)
            {
                p[1] = x1;
                p[2] = x2;
            } else if (x3 < x4)
            {
                p[1] = x3;
                p[2] = x4;
            }

            if (x2 < x3)
            {
                p[3] = x2;
                p[4] = x3;
            } else if (x4 < x1)
            {
                p[3] = x4;
                p[4] = x1;
            }
        }
    }

    gotoxy(0, 0);
    printf("    ");

    // Loop.
    while (!in_GetKey())
    {
        for (i = 0; i < 120; ++i)
        {
            for (y = 0; y < 23; ++y)
            {
                int8_t* p = &anim[24 * 6 * i + y * 6];
                line(-8, p[0], y, PAPER_BLACK);
                line(p[1], p[2], y, PAPER_YELLOW);
                line(p[3], p[4], y, PAPER_RED);
                line(p[5], 8, y, PAPER_BLACK);
           }
        }
    }
}
