// ---------------------------------------------------------------------------------------------------------------------
// Vertex
// ---------------------------------------------------------------------------------------------------------------------

layout(location = 1) in vec2 PositionOffset;
layout(location = 2) in float Orientation;
layout(location = 3) in vec4 Color;
layout(location = 4) in vec2 Size;
layout(location = 5) in vec4 TexCoords;

out vec2 GeomPosition;
out float GeomOrientation;
out vec4 GeomColor;
out vec2 GeomSize;
out vec4 GeomTexCoords;

void main()
{
	GeomPosition = PositionOffset;
	GeomOrientation = Orientation;
	GeomColor = Color;
	GeomSize = Size;
	GeomTexCoords = TexCoords;

	gl_Position = vec4(PositionOffset, 0, 1);
}

// ---------------------------------------------------------------------------------------------------------------------
// Geometry
// ---------------------------------------------------------------------------------------------------------------------

layout(std140, binding = 0) uniform ProjectionMatrixBuffer
{
	mat4 ProjectionMatrix;
};

layout(std140, binding = 1) uniform CameraPositionBuffer
{
	vec2 CameraPosition;
};

layout(points) in;
layout(triangle_strip) out;
layout(max_vertices = 4) out;

in vec2 GeomPosition[];
in float GeomOrientation[];
in vec4 GeomColor[];
in vec2 GeomSize[];
in vec4 GeomTexCoords[];

out vec2 FragTexCoords;
out vec4 FragColor;

void GenerateVertex(vec2 position, vec2 offset, vec2 texCoords, float cosAngle, float sinAngle)
{
	position = vec2(position.x * cosAngle - position.y * sinAngle, position.x * sinAngle + position.y * cosAngle);
	position += offset;

	gl_Position = ProjectionMatrix * vec4(position, 0, 1);
	FragTexCoords = texCoords;

	EmitVertex();
}

void main()
{
	FragColor = GeomColor[0];

	float width = GeomSize[0].x / 2;
	float height = GeomSize[0].y / 2;
	float cosAngle = cos(GeomOrientation[0]);
	float sinAngle = sin(GeomOrientation[0]);
	vec2 offset = GeomPosition[0] + CameraPosition;

	GenerateVertex(vec2(-width, -height), offset, GeomTexCoords[0].xy, cosAngle, sinAngle);
	GenerateVertex(vec2(-width, height), offset, GeomTexCoords[0].xy + vec2(0, GeomTexCoords[0].w), cosAngle, sinAngle);
	GenerateVertex(vec2(width, -height), offset, GeomTexCoords[0].xy + vec2(GeomTexCoords[0].z, 0), cosAngle, sinAngle);
	GenerateVertex(vec2(width, height), offset, GeomTexCoords[0].xy + GeomTexCoords[0].zw, cosAngle, sinAngle);

	EndPrimitive();
}