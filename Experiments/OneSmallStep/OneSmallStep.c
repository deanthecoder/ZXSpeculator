// zcc +zx -lndos -create-app -lmz -o OneSmallStep ../utils.c ../glsl.c OneSmallStep.c
// zcc +zx -lndos -lmz -o OneSmallStep.bin ../utils.c ../glsl.c OneSmallStep.c
#include "../utils.h"
#include "../glsl.h"

#define iTime 0.0f
#define iFrame 0
vec2_t iResolution = {256.0f, 192.0f};
#define R iResolution

/********************************/

static float n31(vec3_t p) {
	vec3_t s = {7, 157, 113}, ip;
    ip = *v3_floor(&p);
	p = *v3_fract(&p);
	p = *v3_mul(&p, v3_mul(&p, v3_addf(v3_mulf(&p, -2.0f), 3.0f)));
	vec4_t h = {0};
    h.y = s.y;
    h.z = s.z;
    h.w = 270.0f;
    h = *v4_addf(&h, v3_dot(&ip, &s));
	h = *v4_mix(v4_fract(v4_mulf(v4_sin(&h), 43.5453f)), v4_fract(v4_mulf(v4_sin(v4_addf(&h, s.x)), 43.5453f)), p.x);
    vec2_t hxz, hyw;
    hxz.x = h.x; hxz.y = h.z;
    hyw.x = h.y; hyw.y = h.w;
	vec2_t* r = v2_mix(&hxz, &hyw, p.y);
    set_xy(&h, r->x, r->y);
	return mix(h.x, h.y, p.z);
}

static float box(vec3_t p, vec3_t b) {
	vec3_t q;
    q = *v3_sub(v3_abs(&p), &b);
	return v3_length(v3_maxf(&q, 0.0f)) + min(max(q.x, max(q.y, q.z)), 0.0f);
}

static float map(vec3_t p) {
    // return v3_length(p) - 1.6f;
	float r, k, t, h,
	      bmp = (n31(p) + n31(*v3_mulf(&p, 2.12f)) * .5 + n31(*v3_mulf(&p, 4.42f)) * .25 + n31(*v3_mulf(&p, 8.54f)) * .125 + n31(*v3_mulf(&p, 63.52f)) * .0156) * .5 * (.5 + 2. * exp(-spow(v2_length(v2_sub(vec2(p.x, p.z), vec2(.5, 2.2))), 2.) * .26)),
	      a = p.y - .27 - bmp,
	      b = (bmp * bmp * .5 - .5) * .12;
    set_xy(&p, -p.x, -p.y);
	p.x /= .95 - cos((p.z + 1.2 - sign(p.x)) * .8) * .1;
	vec3_t tp;
    tp = p;
	tp.z = fmod(tp.z - .5, .4) - .2;
	t = max(box(tp, *vec3(2, .16, .12 + tp.y * .25)), box(*v3_sub(&p, vec3(0, 0, 1.1)), *vec3(2, .16, 1.7)));
	tp = p;
	tp.x = abs(p.x) - 1.65;
	tp.z -= 1.1;
	t = min(t, box(tp, *vec3(.53 - .12 * tp.z, .16, 1.6)));
	p.z /= cos(p.z * .1);
	vec2_t q;
    set_xy(&q, p.x, p.z);
	q.x = abs(q.x);
	k = q.x * .12 + q.y;
	if (k < 0.) r = v2_length(&q) - 1.2;
	else if (k > 2.48) r = v2_length(v2_sub(&q, vec2(0, 2.5))) - 1.5;
	else r = v2_dot(&q, vec2(.99, -.12)) - 1.2;

	b -= max(max(r, p.y), -t);
	h = clamp(.5 + .5 * (b - a) / -.8, 0., 1.);
	return mix(b, a, h) + .8 * h * (1. - h);
}

static vec3_t* normal(vec3_t p) {
	vec3_t n = {0}, e;
	for (int i = 0; i < 4; ++i) {
        vec3_t o;
        o.x = ((i + 3) >> 1) & 1;
        o.y = (i >> 1) & 1;
        o.z = i & 1;
		e = *v3_mulf(v3_subf(v3_mulf(&o, 2.0f), 1.0f), 0.5773f);
		n = *v3_add(&n, v3_mulf(&e, map(*v3_add(&p, v3_mulf(&e, 0.01f)))));
	}

	return v3_normalize(&n);
}

static float lights(vec3_t p) {
	vec3_t lp = {6, 3, -10}, ld, n;
    ld = *v3_normalize(v3_sub(&lp, &p));
    n = *normal(p);
	return max(0.0f, 0.1f + 0.9f * v3_dot(&ld, &n));
}

static float march(vec3_t ro, vec3_t rd) {
	vec3_t p;
	float d = .01f;
	for (float i = 0.0f; i < 15.0f; ++i) {
		p = *v3_add(&ro, v3_mulf(&rd, d));
		float h = map(p);
		if (abs(h) < .025f) break;
		d += h;
	}

	return lights(p) * exp(-d * .14);
}

static float mainImage(vec2_t fc) {
    vec2_t q, uv;
    q = *v2_div(&fc, &R);
    uv = *v2_divf(v2_sub(&fc, v2_mulf(&R, 0.5f)), R.y);

    vec3_t f, r, ro = {0, .2, -4};
    ro = *rot_yz(&ro, -0.6f);
    ro = *rot_xz(&ro, 1.1);
    f = *v3_normalize(v3_sub(vec3(0.0f, 0.0f, 0.8f), &ro));
    r = *v3_normalize(v3_cross(vec3(0, 1, 0), &f));
    float c = march(ro, *v3_normalize(v3_add(&f, v3_add(v3_mulf(&r, uv.x), v3_mulf(v3_cross(&f, &r), uv.y)))));
    c *= 2.5;
    c *= c;
    return c;
}

void main()
{
    srand(0);
    zx_border(INK_BLUE);
    textbackground(BLACK);
    textcolor(WHITE);
    clrscr();

    // Footer.
    draw_footer("One Small Step (@DeanTheCoder)");

    // Loop.
    in_GetKeyReset();
    for (uint16_t y = 0; y < 192 - 8; y += 4)
    {
        for (uint16_t x = 0; x < 256; x += 4)
        {
            float c = clamp(mainImage(*vec2(x, 191 - y)), 0.0f, 2.0f);

            // Set the color.
            uint8_t ink, bright = 1 << 6;
            switch ((uint8_t)(floor(c * 0.99f)))
            {
            case 0:
                ink = INK_WHITE;
                break;

            case 1:
                ink = INK_WHITE + bright;
                c *= 0.4;
                break;

            default:
                ink = INK_WHITE + PAPER_RED;
                break;
            }

            // Plot the pixels.
            c_plot_shade((x >> 2), (y >> 2), min(c * 255.0f, 255.0f));
            attrMem[((y >> 3) << 5) + (x >> 3)] = ink;
        }
    }

    in_waitForKey();
}
