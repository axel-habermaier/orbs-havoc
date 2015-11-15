#include "VertexShader.glsl"

Fragment
{
	#include "BloomSettings.glsl"

	layout(binding = 0) uniform sampler2D Texture;
	layout(location = 1) in vec2 TexCoords;
	layout(location = 0) out vec4 OutColor;

	void main()
	{
		vec4 color = texture(Texture, TexCoords);
		vec4 subtract = vec4(BloomThreshold);

		// Subtract the bloom threshold from each color component, then scale it back up so that the
		// maximum value is 1 and clamp to the range [0,1].
		OutColor = clamp((color - subtract) / (1.0 - BloomThreshold), 0.0f, 1.0f);
	}
}