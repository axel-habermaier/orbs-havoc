Vertex
{
	layout(std140, binding = 0) uniform ProjectionMatrixBuffer
	{
		mat4 ProjectionMatrix;
	};

	layout(std140, binding = 1) uniform ViewMatrixBuffer
	{
		mat4 ViewMatrix;
	};

	layout(location = 0) in vec2 Position;
	layout(location = 1) in vec2 PositionOffset;
	layout(location = 2) in float Orientation;
	layout(location = 3) in vec4 Color;
	layout(location = 4) in vec2 Size;
	layout(location = 5) in vec4 TexCoords;

	layout(location = 1) out vec2 OutTexCoords;
	layout(location = 2) out vec4 OutColor;

	void main()
	{
		gl_Position = ProjectionMatrix * vec4(Position * vec2(100), 0, 1);
		OutTexCoords = vec2(0,0);
		OutColor = Color;
	}
}