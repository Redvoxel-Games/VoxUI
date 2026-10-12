#version 330 core

in vec2 textureCoord;

uniform sampler2D fontTexture;
uniform vec4 textColor;

out vec4 FragColor;

void main()
{
    float alpha = texture(fontTexture, textureCoord).r;

    FragColor = vec4(textColor.rgb, textColor.a * alpha);
}