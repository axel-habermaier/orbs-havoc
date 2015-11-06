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

namespace PointWars.Gameplay.Client
{
	using System.Numerics;
	using Platform;
	using Rendering;
	using SceneNodes;
	using Utilities;

	/// <summary>
	///   Represents the camera that is used to draw the game.
	/// </summary>
	internal class Camera
	{
		private readonly GameSession _gameSession;
		private readonly Window _window;
		private Vector2 _position;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="gameSession">The game session that should be drawn.</param>
		/// <param name="window">The window the game session is drawn to.</param>
		public Camera(GameSession gameSession, Window window)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(window, nameof(window));

			_gameSession = gameSession;
			_window = window;
		}

		/// <summary>
		///   Draws the game session.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the view.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			var avatar = _gameSession.Players?.LocalPlayer?.Avatar;
			if (avatar != null)
				_position = -avatar.WorldPosition;

			spriteBatch.PositionOffset = _position + new Vector2(_window.Size.Width / 2, _window.Size.Height / 2);

			foreach (var spriteNode in _gameSession.SceneGraph.EnumeratePostOrder<SpriteNode>())
				spriteNode.Draw(spriteBatch);

			_gameSession.LevelRenderer.Draw(spriteBatch);
			spriteBatch.PositionOffset = Vector2.Zero;
		}
	}
}