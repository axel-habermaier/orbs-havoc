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

namespace PointWars.Gameplay.Entities
{
	using System.Numerics;
	using Network;
	using Network.Messages;
	using Scene;

	/// <summary>
	///   A base class for server-side and client-side entity scene nodes.
	/// </summary>
	internal abstract class Entity : SceneNode
	{
		/// <summary>
		///   Gets the network type of the entity.
		/// </summary>
		public EntityType NetworkType { get; protected set; }

		/// <summary>
		///   Gets or sets the network identity of the entity.
		/// </summary>
		public NetworkIdentity NetworkIdentity { get; set; }

		/// <summary>
		///   Gets the type of the update messages that are sent for the entity.
		/// </summary>
		public MessageType UpdateMessageType { get; protected set; }

		/// <summary>
		///   Gets or sets the game session the entity belongs to.
		/// </summary>
		public GameSession GameSession { get; set; }

		/// <summary>
		///   Gets or sets the velocity of the entity in 2D space.
		/// </summary>
		public Vector2 Velocity { get; set; }

		/// <summary>
		///   Gets or sets the player the entity belongs to.
		/// </summary>
		public Player Player { get; protected set; }

		/// <summary>
		///   Invoked when the scene node is attached to a parent scene node.
		/// </summary>
		/// <remarks>
		///   The method is intentionally hidden from deriving types; deriving entities should use the OnAdded method instead.
		/// </remarks>
		protected sealed override void OnAttached()
		{
		}

		/// <summary>
		///   Invoked when the scene node is detached from its scene graph.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		/// <remarks>
		///   The method is intentionally hidden from deriving types; deriving entities should use the OnRemoved method instead.
		/// </remarks>
		protected sealed override void OnDetached()
		{
			Velocity = Vector2.Zero;
			Player = null;
			GameSession = null;
		}

		/// <summary>
		///   Updates the state of the entity.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public virtual void Update(float elapsedSeconds)
		{
		}

		/// <summary>
		///   Invoked when the entity is added to a game session.
		/// </summary>
		public virtual void OnAdded()
		{
		}

		/// <summary>
		///   Invoked when the entity is removed from a game session.
		/// </summary>
		/// <remarks>This method is not called when the game session is disposed.</remarks>
		public virtual void OnRemoved()
		{
		}
	}
}