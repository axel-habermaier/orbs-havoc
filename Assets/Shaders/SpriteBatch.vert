layout(std140, binding = 0) uniform DefaultBuffer
{
	mat4 ProjectionMatrix;
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoords;
layout(location = 2) in vec4 Color;

layout(location = 0) out vec4 OutPosition;
layout(location = 1) out vec2 OutTexCoords;
layout(location = 2) out vec4 OutColor;

void main()
{
	OutPosition = ProjectionMatrix * vec4(Position, 0, 1);
	OutTexCoords = TexCoords;
	OutColor = Color;
}
