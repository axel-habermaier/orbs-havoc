Vertex
{
	layout(std140, binding = 0) uniform ProjectionMatrixBuffer
	{
		mat4 ProjectionMatrix;
	};

	layout(std140, binding = 1) uniform PositionOffsetBuffer
	{
		vec2 PositionOffset;
	};

	layout(location = 0) in vec2 Position;
	layout(location = 1) in vec2 TexCoords;
	layout(location = 2) in vec4 Color;

	layout(location = 1) out vec2 OutTexCoords;
	layout(location = 2) out vec4 OutColor;

	void main()
	{
		gl_Position = ProjectionMatrix * vec4(Position + PositionOffset, 0, 1);
		OutTexCoords = TexCoords;
		OutColor = Color;
	}
}