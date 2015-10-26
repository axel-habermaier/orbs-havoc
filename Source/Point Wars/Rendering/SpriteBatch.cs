// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace PointWars.Rendering
{
	using System;
	using System.Collections.Generic;
	using System.Numerics;
	using Platform.Graphics;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;
	using static Platform.Graphics.OpenGL3;
	using Buffer = Platform.Graphics.Buffer;

	/// <summary>
	///   Efficiently draws large amounts of 2D sprites by batching together quads with the same texture.
	/// </summary>
	public sealed class SpriteBatch : DisposableObject
	{
		/// <summary>
		///   The maximum number of quads that can be queued.
		/// </summary>
		private const int MaxQuads = UInt16.MaxValue;

		/// <summary>
		///   The maximum number of position offsets that can be used.
		/// </summary>
		private const int MaxPositionOffsets = 64;

		/// <summary>
		///   The index buffer that is used for drawing.
		/// </summary>
		private readonly Buffer _indexBuffer;

		/// <summary>
		///   The uniform buffer providing the shader with the position offsets.
		/// </summary>
		private readonly DynamicBuffer _positionOffsetBuffer;

		/// <summary>
		///   The position offsets used by the sprite batch.
		/// </summary>
		private readonly Vector2[] _positionOffsets = new Vector2[MaxPositionOffsets];

		/// <summary>
		///   The uniform buffer providing the shader with the projection matrix.
		/// </summary>
		private readonly Buffer _projectionMatrixBuffer;

		/// <summary>
		///   The list of all quads.
		/// </summary>
		private readonly Quad[] _quads = new Quad[MaxQuads];

		/// <summary>
		///   The vertex buffer that is used for drawing.
		/// </summary>
		private readonly DynamicBuffer _vertexBuffer;

		/// <summary>
		///   The vertex input layout used by the sprite batch.
		/// </summary>
		private readonly VertexLayout _vertexLayout;

		/// <summary>
		///   A 1x1 pixels fully white texture.
		/// </summary>
		private readonly Texture _whiteTexture;

		/// <summary>
		///   The blend state that should be used for drawing.
		/// </summary>
		private BlendState _blendState = BlendState.Premultiplied;

		/// <summary>
		///   The index of the section that is currently in use.
		/// </summary>
		private int _currentSection = -1;

		/// <summary>
		///   The index of the section list that is currently being used.
		/// </summary>
		private int _currentSectionList = -1;

		/// <summary>
		///   The number of position offsets that are currently used.
		/// </summary>
		private int _numPositionOffsets;

		/// <summary>
		///   The number of quads that are currently queued.
		/// </summary>
		private int _numQuads;

		/// <summary>
		///   The number of section lists that are currently used.
		/// </summary>
		private int _numSectionLists;

		/// <summary>
		///   The number of sections that are currently used.
		/// </summary>
		private int _numSections;

		/// <summary>
		///   The projection matrix used by the sprite batch.
		/// </summary>
		private Matrix4x4 _projectionMatrix;

		/// <summary>
		///   The sampler state that should be used for drawing.
		/// </summary>
		private SamplerState _samplerState = SamplerState.Bilinear;

		/// <summary>
		///   A mapping from a texture to its corresponding section list.
		/// </summary>
		private SectionList[] _sectionLists = new SectionList[4];

		/// <summary>
		///   The list of all sections.
		/// </summary>
		private Section[] _sections = new Section[16];

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public unsafe SpriteBatch()
		{
			// Initialize the indices; this can be done once, so after the indices are copied to the index buffer,
			// we never have to change the index buffer again
			const int numIndices = MaxQuads * 6;
			var indices = new int[numIndices];
			ushort index = 0;

			for (var i = 0; i < numIndices; index += 4)
			{
				// Indices for the first triangle of the quad
				indices[i++] = index;
				indices[i++] = index + 1;
				indices[i++] = index + 2;

				// Indices for the second triangle of the quad
				indices[i++] = index + 3;
				indices[i++] = index + 1;
				indices[i++] = index + 2;
			}

			// Initialize the graphics objects
			fixed (void* data = indices)
			{
				_vertexBuffer = new DynamicBuffer(GL_ARRAY_BUFFER, MaxQuads * 4, sizeof(Vertex));
				_indexBuffer = new Buffer(GL_ELEMENT_ARRAY_BUFFER, GL_STATIC_DRAW, numIndices * sizeof(int), data);
				_vertexLayout = new VertexLayout(_vertexBuffer.Buffer, _indexBuffer);
				_projectionMatrixBuffer = new Buffer(GL_UNIFORM_BUFFER, GL_STATIC_DRAW, sizeof(Matrix4x4), null);
				// Position offsets must be of type Vector4 due to OpenGL uniform buffer alignment requirements
				_positionOffsetBuffer = new DynamicBuffer(GL_UNIFORM_BUFFER, MaxPositionOffsets, sizeof(Vector4));
			}

			using (var pointer = new BufferPointer(new byte[] { 255, 255, 255, 255 }))
				_whiteTexture = new Texture(new Size(1, 1), GL_RGBA, pointer);

			Reset();
		}

		/// <summary>
		///   Gets or sets the position offset used by the sprite batch.
		/// </summary>
		public Vector2 PositionOffset { get; set; }

		/// <summary>
		///   Gets or sets the projection matrix used by the sprite batch.
		/// </summary>
		public unsafe Matrix4x4 ProjectionMatrix
		{
			get { return _projectionMatrix; }
			set
			{
				Assert.NotDisposed(this);

				_projectionMatrix = value;
				_projectionMatrixBuffer.Copy(&value);
			}
		}

		/// <summary>
		///   Gets or sets the sampler state that should be used for drawing.
		/// </summary>
		public SamplerState SamplerState
		{
			get { return _samplerState; }
			set
			{
				Assert.ArgumentNotNull(value, nameof(value));
				Assert.NotDisposed(this);

				_samplerState = value;
			}
		}

		/// <summary>
		///   Gets or sets the blend state that should be used for drawing.
		/// </summary>
		public BlendState BlendState
		{
			get { return _blendState; }
			set
			{
				Assert.ArgumentInRange(value, nameof(value));
				Assert.NotDisposed(this);

				_blendState = value;
			}
		}

		/// <summary>
		///   Gets or sets the scissor area that should be used for the scissor test.
		/// </summary>
		public Rectangle ScissorArea { get; set; }

		/// <summary>
		///   Gets or sets a value indicating whether a scissor test should be performed during rendering.
		/// </summary>
		public bool UseScissorTest { get; set; }

		/// <summary>
		///   Gets or sets the layer of all subsequent drawing operations. All sprites within the same layer are drawn in some
		///   unspecified order. Layers, on the other hand, are drawn from lowest to highest, such that sprites in a higher layer
		///   overlap or hide sprites in a lower layer at the same position, depending on the blend state settings, obviously.
		/// </summary>
		public int Layer { get; set; }

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_whiteTexture.SafeDispose();
			_vertexBuffer.SafeDispose();
			_indexBuffer.SafeDispose();
			_vertexLayout.SafeDispose();
			_projectionMatrixBuffer.SafeDispose();
			_positionOffsetBuffer.SafeDispose();
		}

		/// <summary>
		///   Draws the outline of a rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle that should be drawn.</param>
		/// <param name="color">The color of the outline.</param>
		/// <param name="thickness">The thickness of the outline.</param>
		public void DrawOutline(Rectangle rectangle, Color color, float thickness = 1)
		{
			// Make sure there is no overdraw at the corners that would be visible depending on the opacity and blend mode
			DrawLine(rectangle.TopLeft, rectangle.TopRight, color, thickness);
			DrawLine(rectangle.BottomLeft + new Vector2(1, -1), rectangle.TopLeft + new Vector2(1, 1), color, thickness);
			DrawLine(rectangle.TopRight + new Vector2(0, 1), rectangle.BottomRight - new Vector2(0, 1), color, thickness);
			DrawLine(rectangle.BottomLeft - new Vector2(0, 1), rectangle.BottomRight - new Vector2(0, 1), color, thickness);
		}

		/// <summary>
		///   Draws a textured rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle that should be drawn.</param>
		/// <param name="texture">The texture that should be used to draw the rectangle.</param>
		public void Draw(Rectangle rectangle, Texture texture)
		{
			Draw(rectangle, texture, Colors.White);
		}

		/// <summary>
		///   Draws a colored rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle that should be drawn.</param>
		/// <param name="color">The color that should be used to draw the rectangle.</param>
		public void Draw(Rectangle rectangle, Color color)
		{
			Draw(rectangle, _whiteTexture, color);
		}

		/// <summary>
		///   Draws a textured rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle that should be drawn.</param>
		/// <param name="texture">The texture that should be used to draw the rectangle.</param>
		/// <param name="color">The color of the quad.</param>
		/// <param name="texCoords">The texture coordinates that should be used.</param>
		public void Draw(Rectangle rectangle, Texture texture, Color color, Rectangle? texCoords = null)
		{
			var quad = new Quad(rectangle, color, texCoords);
			Draw(ref quad, texture);
		}

		/// <summary>
		///   Draws the given rectangle.
		/// </summary>
		/// <param name="position">The position of the quad that should be drawn.</param>
		/// <param name="size">The size of the quad that should be drawn.</param>
		/// <param name="texture">The texture that should be used to draw the quad.</param>
		/// <param name="color">The color of the quad.</param>
		/// <param name="rotation">The rotation (in radians) that should be applied to the quad before it is drawn.</param>
		/// <param name="textureArea">The texture coordinates that should be used to draw the quad.</param>
		public void Draw(Vector2 position, Size size, Texture texture, Color color, float rotation, Rectangle? textureArea = null)
		{
			Assert.ArgumentNotNull(texture, nameof(texture));

			var rectangle = new Rectangle(-size.Width / 2.0f, -size.Height / 2.0f, size.Width, size.Height);
			var quad = new Quad(rectangle, color, textureArea);

			var rotationMatrix = Matrix4x4.CreateRotationZ(rotation);
			var unrotatedPosition = new Vector3(position.X, position.Y, 0);
			var offset = new Vector2(unrotatedPosition.X, unrotatedPosition.Y);

			Quad.Transform(ref quad, ref rotationMatrix);
			Quad.Offset(ref quad, ref offset);

			Draw(ref quad, texture);
		}

		/// <summary>
		///   Draws a textured rectangle at the given position with the texture's size.
		/// </summary>
		/// <param name="texture">The texture that should be used to draw the quad.</param>
		/// <param name="position">The position of the quad.</param>
		/// <param name="color">The color of the quad.</param>
		public void Draw(Texture texture, Vector2 position, Color color)
		{
			Assert.ArgumentNotNull(texture, nameof(texture));

			var quad = new Quad(new Rectangle(position, texture.Size), color);
			Draw(ref quad, texture);
		}

		/// <summary>
		///   Draws a textured rectangle at the given position with the texture's size and rotation.
		/// </summary>
		/// <param name="texture">The texture that should be used to draw the quad.</param>
		/// <param name="angle">The rotation of the quad.</param>
		/// <param name="position">The position of the quad.</param>
		/// <param name="color">The color of the quad.</param>
		public void Draw(Texture texture, Vector2 position, float angle, Color color)
		{
			Assert.ArgumentNotNull(texture, nameof(texture));

			var size = new Size(texture.Width, texture.Height);
			var shift = new Vector2(-size.Width, -size.Height) * 0.5f;
			var quad = new Quad(new Rectangle(shift, size), color);

			var rotation = Matrix4x4.CreateRotationZ(angle);
			Quad.Transform(ref quad, ref rotation);

			var translation = Matrix4x4.CreateTranslation(position.X, position.Y, 0);
			Quad.Transform(ref quad, ref translation);

			Draw(ref quad, texture);
		}

		/// <summary>
		///   Draws the given quad.
		/// </summary>
		/// <param name="quad">The quad that should be added.</param>
		/// <param name="texture">The texture that should be used to draw the quad.</param>
		public void Draw(ref Quad quad, Texture texture)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(texture, nameof(texture));

			if (!CheckQuadCount(1))
				return;

			ChangeSection(texture);

			// Add the quad to the list
			_quads[_numQuads++] = quad;
			++_sections[_currentSection].NumQuads;
		}

		/// <summary>
		///   Draws a single line of text.
		/// </summary>
		/// <param name="font">The font that should be used to draw the text.</param>
		/// <param name="text">The text that should be drawn.</param>
		/// <param name="color">The color that should be used to draw the text.</param>
		/// <param name="position">The position of the text's top left corner.</param>
		public void DrawText(Font font, string text, Color color, Vector2 position)
		{
			using (var textString = new TextString(text))
				DrawText(font, textString, color, position);
		}

		/// <summary>
		///   Draws a single line of text.
		/// </summary>
		/// <param name="font">The font that should be used to draw the text.</param>
		/// <param name="text">The text that should be drawn.</param>
		/// <param name="color">The color that should be used to draw the text.</param>
		/// <param name="position">The position of the text's top left corner.</param>
		public void DrawText(Font font, TextString text, Color color, Vector2 position)
		{
			Assert.ArgumentNotNull(font, nameof(font));

			if (text.IsWhitespaceOnly)
				return;

			for (var i = 0; i < text.Length; ++i)
			{
				Quad quad;
				var area = font.GetGlyphArea(text, 0, i, ref position);

				if (font.CreateGlyphQuad(text, i, ref area, color, out quad))
					Draw(ref quad, font.Texture);
			}
		}

		/// <summary>
		///   Draws a line.
		/// </summary>
		/// <param name="start">The start of the line.</param>
		/// <param name="end">The end of the line.</param>
		/// <param name="color">The color of the line.</param>
		/// <param name="width">The width of the line.</param>
		public void DrawLine(Vector2 start, Vector2 end, Color color, float width)
		{
			if (MathUtils.Equals(width, 0) || MathUtils.Equals((start - end).LengthSquared(), 0))
				return;

			var quad = Quad.FromLine(start, end, color, width);
			Draw(ref quad, _whiteTexture);
		}

		/// <summary>
		///   Draws the given quads.
		/// </summary>
		/// <param name="quads">The quads that should be added.</param>
		/// <param name="count">The number of quads to draw.</param>
		/// <param name="texture">The texture that should be used to draw the quads.</param>
		internal void Draw(Quad[] quads, int count, Texture texture)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(quads, nameof(quads));
			Assert.ArgumentNotNull(texture, nameof(texture));
			Assert.ArgumentInRange(count, 0, quads.Length, nameof(count));

			if (count == 0 || !CheckQuadCount(count))
				return;

			ChangeSection(texture);

			// Add the quads to the list and update the quad counts
			Array.Copy(quads, 0, _quads, _numQuads, count);
			_numQuads += count;
			_sections[_currentSection].NumQuads += count;
		}

		/// <summary>
		///   Draws all batched sprites.
		/// </summary>
		/// <param name="renderTarget">The render target the sprite batch should draw to.</param>
		public void DrawBatch(RenderTarget renderTarget)
		{
			Assert.That(SamplerState != null, "No sampler state has been set.");
			Assert.NotDisposed(this);

			// Quit early if there's nothing to draw
			if (_numQuads == 0)
				return;

			// Sort the section lists by layer
			Array.Sort(_sectionLists, 0, _numSectionLists, SectionList.LayerComparer.Instance);

			// Prepare the vertex buffer and the position offset buffer
			UpdateBuffers();

			// Set the shared GPU state
			Assets.SpriteBatchShader.Bind();
			_vertexLayout.Bind();
			_projectionMatrixBuffer.Bind(0);

			// Draw the quads, starting with the lowest layer
			var offset = 0;
			for (var i = 0; i < _numSectionLists; ++i)
			{
				var sectionList = _sectionLists[i];

				// Set the section-specific GPU state
				sectionList.Texture.Bind();
				sectionList.SamplerState.Bind();
				sectionList.BlendState.Bind();

				// Bind the correct position offset
				_positionOffsetBuffer.Bind(1, sectionList.PostitionOffsetIndex, 1);

				if (!sectionList.UseScissorTest)
					glDisable(GL_SCISSOR_TEST);
				else
				{
					glEnable(GL_SCISSOR_TEST);
					glScissor(
						MathUtils.RoundIntegral(sectionList.ScissorArea.Left),
						MathUtils.RoundIntegral(renderTarget.Size.Height - sectionList.ScissorArea.Height - sectionList.ScissorArea.Top),
						MathUtils.RoundIntegral(sectionList.ScissorArea.Width),
						MathUtils.RoundIntegral(sectionList.ScissorArea.Height));
				}

				// Draw and increase the offset
				var numIndices = sectionList.NumQuads * 6;
				renderTarget.DrawIndexed(numIndices, offset, _vertexBuffer.VertexOffset);
				offset += numIndices;
			}

			// Reset the internal state
			Reset();

			// Make sure we don't "leak out" the scissor rasterizer state
			glDisable(GL_SCISSOR_TEST);
		}

		/// <summary>
		///   Resets the internal state.
		/// </summary>
		private void Reset()
		{
			_numQuads = 0;
			_numPositionOffsets = 1;
			_numSections = 0;
			_numSectionLists = 0;
			_currentSectionList = -1;
			_currentSection = -1;
		}

		/// <summary>
		///   Check whether adding the given number of quads would overflow the internal quad buffer. Returns true
		///   if the quads can be batched.
		/// </summary>
		/// <param name="quadCount">The additional number of quads that should be drawn.</param>
		private bool CheckQuadCount(int quadCount)
		{
			Assert.ArgumentInRange(quadCount, 0, MaxQuads, nameof(quadCount));

			// Check whether we would overflow if we added the given batch.
			var tooManyQuads = _numQuads + quadCount >= _quads.Length;

			if (tooManyQuads)
				Log.Warn("Sprite batch buffer overflow: {0} out of {1} allocated quads in use (could not add {2} quad(s)).",
					_numQuads, MaxQuads, quadCount);

			return !tooManyQuads;
		}

		/// <summary>
		///   Copies the quads to the vertex buffer, sorted by texture.
		/// </summary>
		private unsafe void UpdateBuffers()
		{
			var vertexData = (Quad*)_vertexBuffer.Map();
			fixed (Quad* quads = _quads)
			{
				for (var i = 0; i < _numSectionLists; ++i)
				{
					var section = _sectionLists[i].FirstSection;
					while (section != -1)
					{
						// Calculate the offsets into the arrays and the amount of bytes to copy
						var quadOffset = quads + _sections[section].Offset;
						var bytes = _sections[section].NumQuads * sizeof(Quad);

						// Copy the entire section to the vertex buffer
						MemCopy.Copy(vertexData, quadOffset, bytes);

						// Update the section list's total quad count
						_sectionLists[i].NumQuads += _sections[section].NumQuads;

						// Update the offset and advance to the next section
						vertexData += _sections[section].NumQuads;
						section = _sections[section].Next;
					}
				}
			}

			fixed (Vector2* offsets = _positionOffsets)
			{
				var buffer = (Vector4*)_positionOffsetBuffer.Map();
				for (var i = 0; i < _numPositionOffsets; ++i)
					buffer[i] = new Vector4(offsets[i], 0, 0);
			}
		}

		/// <summary>
		///   Checks whether the current section has to be changed, and if so, creates a new one and possibly
		///   a new section list.
		/// </summary>
		/// <param name="texture">The texture that should be used for newly added quads.</param>
		private void ChangeSection(Texture texture)
		{
			// If nothing has changed, just continue to append to the current section
			if (_currentSectionList != -1 && _currentSection != -1 && SectionListMatches(_currentSectionList, texture))
				return;

			// Add a new section
			AddSection();

			// Depending on whether we've already seen this texture and these rendering settings, add it to the corresponding 
			// section list or add a new one
			var known = false;
			for (var i = 0; i < _numSectionLists; ++i)
			{
				if (!SectionListMatches(i, texture))
					continue;

				// We've already seen this texture and these rendering settings before, so add the new section to the list by setting the
				// list's tail section's next pointer to the newly allocated section
				_sections[_sectionLists[i].LastSection].Next = _numSections;
				// Set the section list's tail pointer to the newly allocated section
				_sectionLists[i].LastSection = _numSections;

				known = true;
				_currentSectionList = i;
				break;
			}

			if (!known)
			{
				var positionOffsetIndex = 0;
				if (PositionOffset != Vector2.Zero)
				{
					_positionOffsets[_numPositionOffsets] = PositionOffset;
					positionOffsetIndex = _numPositionOffsets;
					++_numPositionOffsets;
				}

				_currentSectionList = _numSectionLists;
				AddSectionList(new SectionList
				{
					BlendState = BlendState,
					SamplerState = SamplerState,
					Texture = texture,
					ScissorArea = ScissorArea,
					UseScissorTest = UseScissorTest,
					Layer = Layer,
					FirstSection = _numSections,
					LastSection = _numSections,
					NumQuads = 0,
					PositionOffset = PositionOffset,
					PostitionOffsetIndex = positionOffsetIndex
				});
			}

			_currentSection = _numSections;

			// Mark the newly allocated section as allocated
			++_numSections;
		}

		/// <summary>
		///   Checks whether the section list with the specified index matches the given texture and the current rendering
		///   settings.
		/// </summary>
		/// <param name="sectionList">The index of the section list that should be checked.</param>
		/// <param name="texture">The texture that should be used to draw the quads.</param>
		private bool SectionListMatches(int sectionList, Texture texture)
		{
			var list = _sectionLists[sectionList];

			return list.Texture == texture && list.BlendState == BlendState && list.Layer == Layer &&
				   list.SamplerState == SamplerState && list.UseScissorTest == UseScissorTest &&
				   list.ScissorArea == ScissorArea && list.PositionOffset == PositionOffset;
		}

		/// <summary>
		///   Adds a new section, allocating more space for the new section if required.
		/// </summary>
		private void AddSection()
		{
			if (_numSections >= _sections.Length)
			{
				var sections = new Section[_sections.Length * 2];
				Array.Copy(_sections, sections, _sections.Length);
				_sections = sections;
			}

			_sections[_numSections] = new Section(_numQuads);
		}

		/// <summary>
		///   Adds the given section list, allocating more space for the new section list if required.
		/// </summary>
		/// <param name="sectionList">The section list that should be added.</param>
		private void AddSectionList(SectionList sectionList)
		{
			if (_numSectionLists >= _sectionLists.Length)
			{
				var sectionLists = new SectionList[_sectionLists.Length * 2];
				Array.Copy(_sectionLists, sectionLists, _sectionLists.Length);
				_sectionLists = sectionLists;
			}

			_sectionLists[_numSectionLists++] = sectionList;
		}

		/// <summary>
		///   Represents a section of the quad list, with each quad using the same texture and rendering settings.
		/// </summary>
		private struct Section
		{
			/// <summary>
			///   The offset into the quad list. All quads from [Offset, Offset + _numQuads) use the same texture and rendering
			///   settings.
			/// </summary>
			public readonly int Offset;

			/// <summary>
			///   The index of the next section of the quad list using the same texture and rendering settings or -1 if this is the
			///   last
			///   section.
			/// </summary>
			public int Next;

			/// <summary>
			///   The number of quads in this section.
			/// </summary>
			public int NumQuads;

			/// <summary>
			///   Initializes the instance.
			/// </summary>
			/// <param name="offset">The index of the first quad of this section.</param>
			public Section(int offset)
			{
				Offset = offset;
				NumQuads = 0;
				Next = -1;
			}
		}

		/// <summary>
		///   Represents a list of sections using the same texture and rendering settings.
		/// </summary>
		private struct SectionList
		{
			public BlendState BlendState;
			public int FirstSection;
			public int Layer;
			public SamplerState SamplerState;
			public Rectangle ScissorArea;
			public Texture Texture;
			public bool UseScissorTest;
			public int LastSection;
			public int NumQuads;
			public Vector2 PositionOffset;
			public int PostitionOffsetIndex;

			/// <summary>
			///   Used to compare the layers of two section lists.
			/// </summary>
			public class LayerComparer : IComparer<SectionList>
			{
				/// <summary>
				///   The singleton comparer instance.
				/// </summary>
				public static readonly LayerComparer Instance = new LayerComparer();

				/// <summary>
				///   Compares two section list and returns a value indicating whether one belongs to a lower, the same, or greater layer
				///   than the other.
				/// </summary>
				/// <param name="x">The first section list to compare.</param>
				/// <param name="y">The second section list to compare.</param>
				public int Compare(SectionList x, SectionList y)
				{
					return x.Layer.CompareTo(y.Layer);
				}
			}
		}
	}
}