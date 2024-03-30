#ifndef GLSL_H
#define GLSL_H

#include "utils.h"

typedef struct vec2_t {
    float x, y;
} vec2_t;
typedef struct vec3_t {
    float x, y, z;
} vec3_t;
typedef struct vec4_t {
    float x, y, z, w;
} vec4_t;
typedef struct mat2_t{
    vec2_t c1, c2;
} mat2_t;

#define set_xy(_a, _x, _y) { (_a)->x = (_x); (_a)->y = (_y); }
#define set_xz(_a, _x, _z) { (_a)->x = (_x); (_a)->z = (_z); }
#define set_yx(_a, _y, _x) { (_a)->y = (_y); (_a)->x = (_x); }
#define set_yz(_a, _y, _z) { (_a)->y = (_y); (_a)->z = (_z); }
#define set_zx(_a, _z, _x) { (_a)->z = (_z); (_a)->x = (_x); }
#define set_zy(_a, _z, _y) { (_a)->z = (_z); (_a)->y = (_y); }

#undef abs
#define abs fabs

vec2_t* vec2(float, float);
vec3_t* vec3(float, float, float);
vec4_t* vec4(float, float, float, float);
mat2_t* mat2(float, float, float, float);

float spow(float, float);
vec2_t* v2_add(const vec2_t*, const vec2_t*);
vec3_t* v3_add(const vec3_t*, const vec3_t*);
vec4_t* v4_add(const vec4_t*, const vec4_t*);
vec2_t* v2_addf(const vec2_t*, float);
vec3_t* v3_addf(const vec3_t*, float);
vec4_t* v4_addf(const vec4_t*, float);
vec2_t* v2_sub(const vec2_t*, const vec2_t*);
vec3_t* v3_sub(const vec3_t*, const vec3_t*);
vec2_t* v2_subf(const vec2_t*, float);
vec3_t* v3_subf(const vec3_t*, float);
vec2_t* v2_mul(const vec2_t*, const vec2_t*);
vec3_t* v3_mul(const vec3_t*, const vec3_t*);
vec2_t* v2_mulf(const vec2_t*, float);
vec3_t* v3_mulf(const vec3_t*, float);
vec4_t* v4_mulf(const vec4_t*, float);
vec2_t* v2_div(const vec2_t*, const vec2_t*);
vec3_t* v3_div(const vec3_t*, const vec3_t*);
vec2_t* v2_divf(const vec2_t*, float);
vec3_t* v3_divf(const vec3_t*, float);
float v2_dot(const vec2_t*, const vec2_t*);
float v3_dot(const vec3_t*, const vec3_t*);
vec3_t* v3_cross(const vec3_t*, const vec3_t*);
float v2_length(const vec2_t*);
float v3_length(const vec3_t*);
vec2_t* v2_normalize(const vec2_t*);
vec3_t* v3_normalize(const vec3_t*);
vec2_t* v2_pow(const vec2_t*, const vec2_t*);
vec3_t* v3_pow(const vec3_t*, const vec3_t*);
vec2_t* v2_powf(const vec2_t*, float);
vec3_t* v3_powf(const vec3_t*, float);
vec2_t* v2_floor(const vec2_t*);
vec3_t* v3_floor(const vec3_t*);
float fract(float);
vec2_t* v2_fract(const vec2_t*);
vec3_t* v3_fract(const vec3_t*);
vec4_t* v4_fract(const vec4_t*);
vec2_t* v2_abs(const vec2_t*);
vec3_t* v3_abs(const vec3_t*);
float min(float, float);
vec2_t* v2_min(const vec2_t*, const vec2_t*);
vec2_t* v2_minf(const vec2_t*, float);
vec3_t* v3_min(const vec3_t*, const vec3_t*);
vec3_t* v3_minf(const vec3_t*, float);
float max(float, float);
vec2_t* v2_max(const vec2_t*, const vec2_t*);
vec2_t* v2_maxf(const vec2_t*, float);
vec3_t* v3_max(const vec3_t*, const vec3_t*);
vec3_t* v3_maxf(const vec3_t*, float);
float clamp(float, float, float);
float mix(float, float, float);
vec2_t* v2_mix(const vec2_t*, const vec2_t*, float);
vec3_t* v3_mix(const vec3_t*, const vec3_t*, float);
vec4_t* v4_mix(const vec4_t*, const vec4_t*, float);
vec2_t* v2_sin(const vec2_t*);
vec3_t* v3_sin(const vec3_t*);
vec4_t* v4_sin(const vec4_t*);
vec2_t* v2_cos(const vec2_t*);
vec3_t* v3_cos(const vec3_t*);
vec4_t* v4_cos(const vec4_t*);
float sign(float);
vec2_t* v2m2_mul(const vec2_t*, const mat2_t*);

mat2_t* rot(float);
vec3_t* rot_xy(const vec3_t *, float);
vec3_t* rot_xz(const vec3_t *, float);
vec3_t* rot_yz(const vec3_t *, float);

#endif