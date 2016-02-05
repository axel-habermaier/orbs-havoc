#include "Vertices.glsl"

// ---------------------------------------------------------------------------------------------------------------------
// Fragment
// ---------------------------------------------------------------------------------------------------------------------

layout(binding = 0) uniform sampler2D Texture;

in vec2 FragTexCoords;
in vec4 FragColor;

out vec4 OutColor;

void main()
{
	OutColor = vec4(texture(Texture, FragTexCoords).rgb * FragColor.rgb * FragColor.a, 1);
}