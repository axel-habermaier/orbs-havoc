layout(std140, binding = 1) uniform BloomSettings
{
	float BaseIntensity;
	float BaseSaturation;
	float BloomIntensity;
	float BloomSaturation;
	float BloomThreshold;
	int   BlurSampleCount;
};