namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	using System;
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
		private readonly Func<SceneNode, bool> _collisionFilter;
		private uint _lastUpdateSequenceNumber;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public LightingBolt()
		{
			Type = EntityType.LightingBolt;
			_collisionFilter = FilterCollision;
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
			var normalizedDirection = MathUtils.FromAngle(Parent.Orientation);
			var distanceToNearestWall = GameSession.Level.RayCast(WorldPosition, normalizedDirection, Weapons.LightingGun.Range);
			var entityCollision = GameSession.Physics.RayCast(WorldPosition, normalizedDirection, Weapons.LightingGun.Range, _collisionFilter);

			if (distanceToNearestWall <= entityCollision.Length || entityCollision.Entity?.Type != EntityType.Orb)
				Length = distanceToNearestWall;
			else if (entityCollision.Entity?.Type == EntityType.Orb)
			{
				Length = entityCollision.Length;
				HandleCollision(entityCollision.Entity);
				entityCollision.Entity.HandleCollision(this);
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

		/// <summary>
		///   Filters out scene nodes the lighting bolt should not collide with.
		/// </summary>
		private bool FilterCollision(SceneNode sceneNode)
		{
			if (sceneNode == Parent)
				return false;

			var entity = sceneNode as Entity;
			return entity?.Type == EntityType.Orb;
		}
	}
}