Vertex
{
	layout(std140, binding = 0) uniform ProjectionMatrixBuffer
	{
		mat4 ProjectionMatrix;
	};

	layout(std140, binding = 1) uniform CameraPositionBuffer
	{
		vec2 CameraPosition;
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
		float cosAngle = cos( Orientation);
		float sinAngle = -sin(Orientation);

		vec2 offset = PositionOffset + CameraPosition;
		vec2 position = Position * Size;
		position = vec2(position.x * cosAngle + position.y * sinAngle, -position.x * sinAngle + position.y * cosAngle);
		position = position + offset;

		gl_Position = ProjectionMatrix * vec4(position, 0, 1);

		OutTexCoords.x = (gl_VertexID < 2) ? TexCoords.x : TexCoords.x + TexCoords.z;
		OutTexCoords.y = (gl_VertexID == 0 || gl_VertexID == 2) ? TexCoords.y : TexCoords.y + TexCoords.w;

		OutColor = Color;
	}
}

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