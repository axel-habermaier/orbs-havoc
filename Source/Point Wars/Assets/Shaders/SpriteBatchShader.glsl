#include VertexShader.glsl

Fragment
{
	layout(binding = 0) uniform sampler2D Texture;
	layout(location = 1) in vec2 TexCoords;
	layout(location = 2) in vec4 Color;
	layout(location = 0) out vec4 OutColor;

	void main()
	{
		OutColor = texture(Texture, TexCoords) * Color;
	}
}