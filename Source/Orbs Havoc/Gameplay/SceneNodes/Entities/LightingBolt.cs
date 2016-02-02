// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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

namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	using Client;
	using Network;
	using Network.Messages;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a lighting bolt fired by a lighting gun.
	/// </summary>
	internal class LightingBolt : Entity, IRenderable
	{
		private uint _lastUpdateSequenceNumber;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public LightingBolt()
		{
			Type = EntityType.LightingBolt;
		}

		/// <summary>
		///   Gets or sets the lighting bolt's length.
		/// </summary>
		public float Length { get; set; }

		/// <summary>
		///   Draws the renderable using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used for drawing.</param>
		void IRenderable.Draw(SpriteBatch spriteBatch)
		{
			var start = WorldPosition;
			var end = WorldPosition + MathUtils.FromAngle(Parent.Orientation) * Length;

			spriteBatch.RenderState.Layer = 250;
			spriteBatch.DrawLine(start, end, Player.ColorRange.LowerBound, 1);
		}

		/// <summary>
		///   Handles the collision with the given entity.
		/// </summary>
		/// <param name="entity">The entity this entity collided with.</param>
		public override void HandleCollision(Entity entity)
		{
			if (entity.Type != EntityType.Orb || Player == entity.Player)
				return;

			var orb = (Orb)entity;
			var damageMultiplier = Player.Orb != null && Player.Orb.PowerUp == EntityType.QuadDamage ? PowerUps.QuadDamage.DamageMultiplier : 1;
			orb.ApplyDamage(Player, Weapons.LightingGun.Damage * damageMultiplier * 1.0f / NetworkProtocol.ServerUpdateFrequency);
		}

		/// <summary>
		///   Handles the collision with a level wall.
		/// </summary>
		public override void HandleWallCollision()
		{
		}

		/// <summary>
		///   Updates the state of the client-side entity.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override void ClientUpdate(float elapsedSeconds)
		{
		}

		/// <summary>
		///   Updates the state of the server-side entity.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override void ServerUpdate(float elapsedSeconds)
		{
			var result = GameSession.PhysicsSimulation.RayCast(WorldPosition, Parent.Orientation, Weapons.LightingGun.Range, Parent);

			if (result.Entity == null || result.Entity.Type == EntityType.Orb)
				Length = result.Length;

			if (result.Entity != null)
			{
				HandleCollision(result.Entity);
				result.Entity.HandleCollision(this);
			}
		}

		/// <summary>
		///   Invoked when the entity is added to a game session.
		/// </summary>
		public override void OnAdded()
		{
			if (GameSession.ServerMode)
				return;

			GameSession.EntityRenderer.Add(this);
		}

		/// <summary>
		///   Invoked when the entity is removed from a game session.
		/// </summary>
		/// <remarks>This method is not called when the game session is disposed.</remarks>
		public override void OnRemoved()
		{
			if (GameSession.ServerMode)
				return;

			GameSession.EntityRenderer.Remove(this);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		public void OnUpdate(UpdateLightingBoltMessage message, uint sequenceNumber)
		{
			if (!AcceptUpdate(ref _lastUpdateSequenceNumber, sequenceNumber))
				return;

			Length = message.Length;
		}

		/// <summary>
		///   Broadcasts update messages for the entity.
		/// </summary>
		public override void BroadcastUpdates()
		{
			GameSession.Broadcast(UpdateLightingBoltMessage.Create(GameSession.Allocator, NetworkIdentity, Length));
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the entity belongs to.</param>
		/// <param name="player">The player the bullet belongs to.</param>
		public static LightingBolt Create(GameSession gameSession, Player player)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(player, nameof(player));

			var bolt = gameSession.Allocate<LightingBolt>();
			bolt.GameSession = gameSession;
			bolt.Player = player;
			bolt._lastUpdateSequenceNumber = 0;

			return bolt;
		}
	}
}