#version 330 core

layout (location = 0) in vec3 vertPos;

uniform vec2 rectPos;
uniform vec2 rectSize;
uniform vec2 screenSize;

uniform vec2 UVMin;
uniform vec2 UVMax;

out vec2 localPos;
out vec2 textureCoord;

void main()
{
    localPos = vertPos.xy;

    vec2 pos = rectPos + vertPos.xy * rectSize;
    vec2 normalizedPos = pos / screenSize;
    vec2 ndcPos = normalizedPos * 2.0 - 1.0;

    ndcPos.y = -ndcPos.y;

    gl_Position = vec4(ndcPos, vertPos.z, 1.0);

    textureCoord = mix(
        UVMin,
        UVMax,
        vertPos.xy
    );
}