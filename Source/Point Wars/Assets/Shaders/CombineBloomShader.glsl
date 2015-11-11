#include "FullscreenVertexShader.glsl"

Fragment
{
	#include "BloomSettings.glsl"

	layout(binding = 0) uniform sampler2D Texture;
	layout(binding = 1) uniform sampler2D BloomTexture;
	layout(location = 1) in vec2 TexCoords;
	layout(location = 0) out vec4 OutColor;

	vec4 AdjustSaturation(vec4 color, float saturation)
	{
		// The constants 0.3, 0.59, and 0.11 are chosen because the
		// human eye is more sensitive to green light, and less to blue
		float grey = dot(color.rgb, vec3(0.3, 0.59, 0.11));
		return mix(vec4(grey), color, vec4(saturation));
	}
	
	void main()
	{
		// Look up the bloom and original base image colors
		vec4 bloom = texture(BloomTexture, TexCoords);
		vec4 tex = texture(Texture, TexCoords);

		// Adjust color saturation and intensity
		bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
		tex = AdjustSaturation(tex, BaseSaturation) * BaseIntensity;

		// Darken down the base image in areas where there is a lot of bloom,
		// to prevent things looking excessively burned-out
		tex *= vec4(1) - clamp(bloom, 0.0f, 1.0f);

		// Combine the two images
		OutColor = tex + bloom;
	}
}