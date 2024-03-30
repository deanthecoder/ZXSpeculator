#include "/Users/dean/Documents/Source/Repos/ZXSpeculator/Experiments/glsl.h"
#include "glsl.h"

#define BUFF_SIZE 32
vec2_t* vec2(float x, float y) {
    static vec2_t buff[BUFF_SIZE];
    static uint8_t i = 0;
    vec2_t* r = &buff[i];
    i = (i + 1) & 0x1F;
    r->x = x;
    r->y = y;
    return r;
}

vec3_t* vec3(float x, float y, float z) {
    static vec3_t buff[BUFF_SIZE];
    static uint8_t i = 0;
    vec3_t* r = &buff[i];
    i = (i + 1) & 0x1F;
    r->x = x;
    r->y = y;
    r->z = z;
    return r;
}

vec4_t* vec4(float x, float y, float z, float w) {
    static vec4_t buff[BUFF_SIZE];
    static uint8_t i = 0;
    vec4_t* r = &buff[i];
    i = (i + 1) & 0x1F;
    r->x = x;
    r->y = y;
    r->z = z;
    r->w = w;
    return r;
}

mat2_t* mat2(float c1x, float c1y, float c2x, float c2y) {
    static mat2_t buff[BUFF_SIZE];
    static uint8_t i = 0;
    mat2_t* r = &buff[i];
    i = (i + 1) & 0x1F;
    r->c1.x = c1x;
    r->c1.y = c1y;
    r->c2.x = c2x;
    r->c2.y = c2y;
    return r;
}

float spow(float a, float b) {
    return a >= 0.0f ? pow(a, b) : 0.0f;
}

vec2_t* v2_add(const vec2_t* a, const vec2_t* b) {
    return vec2(a->x + b->x, a->y + b->y);
}

vec3_t* v3_add(const vec3_t* a, const vec3_t* b) {
    return vec3(a->x + b->x, a->y + b->y, a->z + b->z);
}

vec4_t* v4_add(const vec4_t* a, const vec4_t* b) {
    return vec4(a->x + b->x, a->y + b->y, a->z + b->z, a->w + b->w);
}

vec2_t* v2_addf(const vec2_t* a, float b) {
    return vec2(a->x + b, a->y + b);
}

vec3_t* v3_addf(const vec3_t* a, float b) {
    return vec3(a->x + b, a->y + b, a->z + b);
}

vec4_t* v4_addf(const vec4_t* a, float b) {
    return vec4(a->x + b, a->y + b, a->z + b, a->w + b);
}

vec2_t* v2_sub(const vec2_t* a, const vec2_t* b) {
    return vec2(a->x - b->x, a->y - b->y);
}

vec3_t* v3_sub(const vec3_t* a, const vec3_t* b) {
    return vec3(a->x - b->x, a->y - b->y, a->z - b->z);
}

vec2_t* v2_subf(const vec2_t* a, float b) {
    return vec2(a->x - b, a->y - b);
}

vec3_t* v3_subf(const vec3_t* a, float b) {
    return vec3(a->x - b, a->y - b, a->z - b);
}

vec2_t* v2_mul(const vec2_t* a, const vec2_t* b) {
    return vec2(a->x * b->x, a->y * b->y);
}

vec3_t* v3_mul(const vec3_t* a, const vec3_t* b) {
    return vec3(a->x * b->x, a->y * b->y, a->z * b->z);
}

vec2_t* v2_mulf(const vec2_t* a, float b) {
    return vec2(a->x * b, a->y * b);
}

vec3_t* v3_mulf(const vec3_t* a, float b) {
    return vec3(a->x * b, a->y * b, a->z * b);
}

vec4_t* v4_mulf(const vec4_t* a, float b) {
    return vec4(a->x * b, a->y * b, a->z * b, a->w * b);
}

vec2_t* v2_div(const vec2_t* a, const vec2_t* b) {
    return vec2(a->x / b->x, a->y / b->y);
}

vec3_t* v3_div(const vec3_t* a, const vec3_t* b) {
    return vec3(a->x / b->x, a->y / b->y, a->z / b->z);
}

vec2_t* v2_divf(const vec2_t* a, float b) {
    return vec2(a->x / b, a->y / b);
}

vec3_t* v3_divf(const vec3_t* a, float b) {
    return vec3(a->x / b, a->y / b, a->z / b);
}

float v2_dot(const vec2_t* a, const vec2_t* b) {
    return a->x * b->x + a->y * b->y;
}

float v3_dot(const vec3_t* a, const vec3_t* b) {
    return a->x * b->x + a->y * b->y + a->z * b->z;
}

vec3_t* v3_cross(const vec3_t* a, const vec3_t* b) {
    return vec3(
        a->y * b->z - a->z * b->y,
        a->z * b->x - a->x * b->z,
        a->x * b->y - a->y * b->x
    );
}

float v2_length(const vec2_t* a) {
    return sqrt(a->x * a->x + a->y * a->y);
}

float v3_length(const vec3_t* a) {
    return sqrt(a->x * a->x + a->y * a->y + a->z * a->z);
}

vec2_t* v2_normalize(const vec2_t* a) {
    float l = v2_length(a);
    return l != 0.0f ? v2_divf(a, l) : vec2(0, 0);
}

vec3_t* v3_normalize(const vec3_t* a) {
    float l = v3_length(a);
    return l != 0.0f ? v3_divf(a, l) : vec3(0, 0, 0);
}

vec2_t* v2_pow(const vec2_t* a, const vec2_t* b) {
    return vec2(spow(a->x, b->x), spow(a->y, b->y));
}

vec3_t* v3_pow(const vec3_t* a, const vec3_t* b) {
    return vec3(spow(a->x, b->x), spow(a->y, b->y), spow(a->z, b->z));
}

vec2_t* v2_powf(const vec2_t* a, float b) {
    return vec2(spow(a->x, b), spow(a->y, b));
}

vec3_t* v3_powf(const vec3_t* a, float b) {
    return vec3(spow(a->x, b), spow(a->y, b), spow(a->z, b));
}

vec2_t* v2_floor(const vec2_t* a) {
    return vec2(floor(a->x), floor(a->y));
}

vec3_t* v3_floor(const vec3_t* a) {
    return vec3(floor(a->x), floor(a->y), floor(a->z));
}

float fract(float a) {
    return a - floor(a);
}

vec2_t* v2_fract(const vec2_t* a) {
    return vec2(fract(a->x), fract(a->y));
}

vec3_t* v3_fract(const vec3_t* a) {
    return vec3(fract(a->x), fract(a->y), fract(a->z));
}

vec4_t* v4_fract(const vec4_t* a) {
    return vec4(fract(a->x), fract(a->y), fract(a->z), fract(a->w));
}

vec2_t* v2_abs(const vec2_t* a) {
    return vec2(fabs(a->x), fabs(a->y));
}

vec3_t* v3_abs(const vec3_t* a) {
    return vec3(fabs(a->x), fabs(a->y), fabs(a->z));
}

float min(float a, float b) {
    return a < b ? a : b;
}

vec2_t* v2_min(const vec2_t* a, const vec2_t* b) {
    return vec2(min(a->x, b->x), min(a->y, b->y));
}

vec2_t* v2_minf(const vec2_t* a, float b) {
    return vec2(min(a->x, b), min(a->y, b));
}

vec3_t* v3_min(const vec3_t* a, const vec3_t* b) {
    return vec3(min(a->x, b->x), min(a->y, b->y), min(a->z, b->z));
}

vec3_t* v3_minf(const vec3_t* a, float b) {
    return vec3(min(a->x, b), min(a->y, b), min(a->z, b));
}

float max(float a, float b) {
    return a > b ? a : b;
}

vec2_t* v2_max(const vec2_t* a, const vec2_t* b) {
    return vec2(max(a->x, b->x), max(a->y, b->y));
}

vec2_t* v2_maxf(const vec2_t* a, float b) {
    return vec2(max(a->x, b), max(a->y, b));
}

vec3_t* v3_max(const vec3_t* a, const vec3_t* b) {
    return vec3(max(a->x, b->x), max(a->y, b->y), max(a->z, b->z));
}

vec3_t* v3_maxf(const vec3_t* a, float b) {
    return vec3(max(a->x, b), max(a->y, b), max(a->z, b));
}

float clamp(float a, float mn, float mx) {
    return min(max(a, mn), mx);
}

float mix(float a, float b, float c) {
    return a * (1.0f - c) + b * c;
}

vec2_t* v2_mix(const vec2_t* a, const vec2_t* b, float c) {
    return vec2(mix(a->x, b->x, c), mix(a->y, b->y, c));
}

vec3_t* v3_mix(const vec3_t* a, const vec3_t* b, float c) {
    return vec3(mix(a->x, b->x, c), mix(a->y, b->y, c), mix(a->z, b->z, c));
}

vec4_t* v4_mix(const vec4_t* a, const vec4_t* b, float c) {
    return vec4(mix(a->x, b->x, c), mix(a->y, b->y, c), mix(a->z, b->z, c), mix(a->w, b->w, c));
}

vec2_t* v2_sin(const vec2_t* a) {
    return vec2(sin(a->x), sin(a->y));
}

vec3_t* v3_sin(const vec3_t* a) {
    return vec3(sin(a->x), sin(a->y), sin(a->z));
}

vec4_t* v4_sin(const vec4_t* a) {
    return vec4(sin(a->x), sin(a->y), sin(a->z), sin(a->w));
}

vec2_t* v2_cos(const vec2_t* a) {
    return vec2(cos(a->x), cos(a->y));
}

vec3_t* v3_cos(const vec3_t* a) {
    return vec3(cos(a->x), cos(a->y), cos(a->z));
}

vec4_t* v4_cos(const vec4_t* a) {
    return vec4(cos(a->x), cos(a->y), cos(a->z), cos(a->w));
}

float sign(float a) {
    return a > 0.0f ? 1.0f : (a < 0.0f ? -1.0f : 0.0f);
}

vec2_t* v2m2_mul(const vec2_t* v, const mat2_t* m) {
    return vec2(v2_dot(v, &(m->c1)), v2_dot(v, &(m->c2)));
}

/********************************/

mat2_t* rot(float a) {
    float c = cos(a),
          s = sin(a);
    return mat2(c, s, -s, c);
}

vec3_t* rot_xy(const vec3_t* v, float a) {
    vec2_t* res = v2m2_mul(vec2(v->x, v->y), rot(a));
    return vec3(res->x, res->y, v->z);
}

vec3_t* rot_xz(const vec3_t* v, float a) {
    vec2_t* res = v2m2_mul(vec2(v->x, v->z), rot(a));
    return vec3(res->x, v->y, res->y);
}

vec3_t* rot_yz(const vec3_t* v, float a) {
    vec2_t* res = v2m2_mul(vec2(v->y, v->z), rot(a));
    return vec3(v->x, res->x, res->y);
}