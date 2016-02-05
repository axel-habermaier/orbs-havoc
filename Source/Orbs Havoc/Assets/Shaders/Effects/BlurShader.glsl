#include "VertexShader.glsl"

Fragment
{
	#include "BloomSettings.glsl"

	layout(std140, binding = 3) uniform BlurSettings
	{
		vec2 SampleOffsets[15];
		float SampleWeights[15];
	};
	
	layout(binding = 0) uniform sampler2D Texture;
	in vec2 TexCoords;
	out vec4 OutColor;

	void main()
	{
		OutColor = vec4(0);

		for (int i = 0; i < BlurSampleCount; i++)
			OutColor += texture(Texture, TexCoords + SampleOffsets[i]) * SampleWeights[i];
	}
}