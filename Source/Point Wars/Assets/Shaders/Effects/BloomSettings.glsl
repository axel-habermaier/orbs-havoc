layout(std140, binding = 2) uniform BloomSettings
{
	float BaseIntensity;
	float BaseSaturation;
	float BloomIntensity;
	float BloomSaturation;
	float BloomThreshold;
	int   BlurSampleCount;
};