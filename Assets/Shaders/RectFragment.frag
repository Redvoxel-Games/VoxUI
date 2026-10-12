#version 330 core

uniform vec2 rectSize;
uniform float rectLineWidth;
uniform vec4 cornerRadii;
uniform vec4 rectColor;

in vec2 localPos;

out vec4 FragColor;

float roundedBoxSDF(vec2 p, vec2 b, vec4 r)
{
    // Select the radius based on the quadrant.
    float radius;

    if (p.x < 0.0)
    radius = (p.y < 0.0) ? r.x : r.z;
    else
    radius = (p.y < 0.0) ? r.y : r.w;

    vec2 q = abs(p) - b + radius;

    return min(max(q.x, q.y), 0.0)
    + length(max(q, 0.0))
    - radius;
}

void main()
{
    // Convert from 0..1 to pixel coordinates centered on the rectangle.
    vec2 p = localPos * rectSize - rectSize * 0.5;

    vec2 halfSize = rectSize * 0.5;

    // Corner radii in pixels.
    vec4 radii = cornerRadii;

    // Prevent radii from extending beyond the rectangle.
    float maxRadius = min(halfSize.x, halfSize.y);

    radii = min(radii, maxRadius);

    // Signed distance to the rounded rectangle.
    float d = roundedBoxSDF(
        p,
        halfSize,
        radii
    );

    // Filled rectangle.
    if (rectLineWidth < 0.0)
    {
        if (d > 0.0)
        discard;

        FragColor = rectColor;
        return;
    }

    // Border.
    float innerD = roundedBoxSDF(
        p,
        halfSize - rectLineWidth,
        max(radii - rectLineWidth, 0.0)
    );

    // Outside outer boundary or inside inner boundary.
    if (d > 0.0 || innerD < 0.0)
    discard;

    FragColor = rectColor;
}