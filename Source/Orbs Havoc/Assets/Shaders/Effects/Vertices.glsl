// ---------------------------------------------------------------------------------------------------------------------
// Vertex
// ---------------------------------------------------------------------------------------------------------------------

out vec2 TexCoords;

void main()
{
	// See also http://rauwendaal.net/2014/06/14/rendering-a-screen-covering-triangle-in-opengl/
	float x = -1.0 + float((gl_VertexID & 1) << 2);
	float y = -1.0 + float((gl_VertexID & 2) << 1);

	TexCoords.x = (x + 1.0) * 0.5;
	TexCoords.y = (y + 1.0) * 0.5;
	gl_Position = vec4(x, y, 0, 1);
}