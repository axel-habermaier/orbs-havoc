#include "Vertices.glsl"

// ---------------------------------------------------------------------------------------------------------------------
// Fragment
// ---------------------------------------------------------------------------------------------------------------------

layout(binding = 0) uniform sampler2D Texture;
in vec2 TexCoords;
out vec4 OutColor;

void main()
{
	OutColor = texture(Texture, TexCoords);
}