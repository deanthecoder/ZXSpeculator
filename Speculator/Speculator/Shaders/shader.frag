#version 330 core

in vec2 texCoord;
out vec4 outputColor;

uniform sampler2D texture0;
uniform float xAspect;
uniform float crt;
uniform float ambientBlur;

// Noise function (-0.5, 0.5)
float h21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(.1031, .11369, .13787));
    p3 += dot(p3, p3.yzx + 19.19);
    return fract((p3.x + p3.y) * p3.z) - 0.5;
}

void main()
{
    vec2 uv = texCoord - 0.5;
    uv.x *= xAspect;
    
    // screen uv is from -0.5 to 0.5
    const vec3 db = vec3(0.00125, 0.0016667, 0); // db.xy = 0.4 / vec2(320.0, 240.0); db.z = 0.0;
    vec2 st = uv - 0.5;
    vec3 col = texture(texture0, st).rgb;
    
    if (crt > 0.5) {
        // Small amount of blur to the main screen.
        col *= 2.0;
        col += texture(texture0, st - db.zy).rgb;
        col += texture(texture0, st + db.zy).rgb;
        col += texture(texture0, st - db.xz).rgb;
        col += texture(texture0, st + db.xz).rgb;
        col /= 6.0;
    }

    float ns = h21(texCoord * 432.234);
    vec3 back = vec3(ns * ambientBlur); // Noise reduces banding.
    
    // onScreen 0 => off screen.  onScreen 1 => on screen.
    float onScreen = step(abs(uv.x), 0.5);
    if (onScreen * crt > 0.0) {
        // Grain.
        col += ns * 0.08;
        
        // Scanlines.
        col *= 1.0 - 0.3 * smoothstep(0.2, 0.0, abs(fract(uv.y * 240.0 + 0.2) - 0.2));

        // Phosphor dots.
        int i = int(fract(uv.x * 320.0) * 3.0);
        const vec2 rgb = vec2(0.9, 1.1);
        if (i == 0) col = pow(col, rgb.xyy);
        else if (i == 1) col = pow(col, rgb.yxy);
        else col = pow(col, rgb.yyx);

        // Vignette.
        float vignette = dot(uv, uv);
        vignette *= vignette * vignette;
        vignette = 1.0 - vignette * 2.5;

        col *= vignette;
    } else if (ambientBlur > 0.5) {
        // Blurred background.
        vec2 xy = vec2(uv.x < 0.0 ? 0.15 : 0.85, st.y * 0.5 - 0.25);
        const float dy = 0.0052083; // 1.0 / 192.0;
        for (float y = -12.0; y < 12.0; y++) {
            back += texture(texture0, xy).rgb;
            xy.y += dy;
        }
        
        back /= 125.0; // 25 samples * 0.2 brightness.
    }

    // Subtle background gradient.
    back += abs(uv.y) * -0.2 + 0.2;
    
    col = mix(back, col, onScreen);
    outputColor = vec4(col, 1.0);
}
