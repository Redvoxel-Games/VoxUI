#version 330 core

in vec2 textureCoord;

uniform sampler2D image;

out vec4 FragColor;

void main()
{
    vec4 col = texture(image, textureCoord);

    FragColor = col;
}