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

namespace PointWars.Gameplay.SceneNodes.Entities
{
	using System;
	using System.Numerics;
	using Network;
	using Network.Messages;
	using Platform.Logging;

	/// <summary>
	///   A base class for server-side and client-side entity scene nodes.
	/// </summary>
	internal abstract class Entity : SceneNode
	{
		/// <summary>
		///   The sequence number of the last remote transform update of the entity.
		/// </summary>
		private uint _lastTransformUpdateSequenceNumber;

		/// <summary>
		///   Gets the type of the entity.
		/// </summary>
		public EntityType Type { get; protected set; }

		/// <summary>
		///   Gets or sets the network identity of the entity.
		/// </summary>
		public NetworkIdentity NetworkIdentity { get; set; }

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
			_lastTransformUpdateSequenceNumber = 0;
		}

		/// <summary>
		///   Updates the state of the server-side entity.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public virtual void ServerUpdate(float elapsedSeconds)
		{
		}

		/// <summary>
		///   Updates the state of the client-side entity.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public virtual void ClientUpdate(float elapsedSeconds)
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

		/// <summary>
		///   Handles the collision with the given entity.
		/// </summary>
		/// <param name="entity">The entity this entity collided with.</param>
		public virtual void HandleCollision(Entity entity)
		{
		}

		/// <summary>
		///   Broadcasts update messages for the entity.
		/// </summary>
		/// <param name="broadcast">The callback that should be used to broadcast the message.</param>
		public virtual void BroadcastUpdates(Action<Message> broadcast)
		{
			broadcast(UpdateTransformMessage.Create(GameSession.Allocator, NetworkIdentity, Position, Orientation));
		}

		/// <summary>
		///   Updates the entity's transformation based on the data in the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		public void UpdateTransform(UpdateTransformMessage message, uint sequenceNumber)
		{
			if (!AcceptUpdate(ref _lastTransformUpdateSequenceNumber, sequenceNumber))
				return;

			// TODO: Interpolation
			ChangeLocalTransformation(message.Position, message.Orientation);
		}

		/// <summary>
		///   Checks whether the entity accepts an update with the given sequence number. All following entity updates are only
		///   accepted when their sequence number exceeds the given one.
		/// </summary>
		/// <param name="lastSequenceNumber">The last accepted sequence number.</param>
		/// <param name="sequenceNumber">The sequence number that should be checked.</param>
		protected static bool AcceptUpdate(ref uint lastSequenceNumber, uint sequenceNumber)
		{
			if (lastSequenceNumber >= sequenceNumber)
			{
				Log.Debug("Entity rejected outdated update.");
				return false;
			}

			lastSequenceNumber = sequenceNumber;
			return true;
		}

		/// <summary>
		///   Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"{GetType().Name} {NetworkIdentity}";
		}
	}
}