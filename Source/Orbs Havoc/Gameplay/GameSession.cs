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

namespace OrbsHavoc.Gameplay
{
	using System;
	using Assets;
	using Behaviors;
	using Client;
	using Network.Messages;
	using Platform.Memory;
	using SceneNodes;
	using SceneNodes.Entities;
	using Server;
	using Utilities;

	/// <summary>
	///   Represents a client- or server-side game session.
	/// </summary>
	internal class GameSession : DisposableObject
	{
		/// <summary>
		///   Initializes a new client-side instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate game objects.</param>
		public GameSession(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			Allocator = allocator;
			SceneGraph = new SceneGraph(allocator);

			SceneGraph.NodeAdded += OnNodeAdded;
			SceneGraph.NodeRemoved += OnNodeRemoved;
		}

		/// <summary>
		///   Gets a value indicating whether the local player is dead.
		/// </summary>
		public bool IsLocalPlayerDead => Players.LocalPlayer?.Orb == null;

		/// <summary>
		///   Gets a value indicating whether the game session is run in server mode.
		/// </summary>
		public bool ServerMode { get; private set; }

		/// <summary>
		///   Gets the allocator that is used to allocate game objects.
		/// </summary>
		public PoolAllocator Allocator { get; }

		/// <summary>
		///   Gets the renderer that renders the game session's level.
		/// </summary>
		public LevelRenderer LevelRenderer { get; private set; }

		/// <summary>
		///   Gets the renderer that renders the game session's entities.
		/// </summary>
		public EntityRenderer EntityRenderer { get; private set; }

		/// <summary>
		///   Gets the particle effect templates.
		/// </summary>
		public ParticleEffects Effects { get; private set; }

		/// <summary>
		///   Gets the level that is used by the game session.
		/// </summary>
		public Level Level { get; private set; }

		/// <summary>
		///   Gets the game session's collision handler in server mode.
		/// </summary>
		public PhysicsSimulation Physics { get; private set; }

		/// <summary>
		///   Gets the scene graph of the game session.
		/// </summary>
		public SceneGraph SceneGraph { get; }

		/// <summary>
		///   Gets the collection of players that are participating in the game session.
		/// </summary>
		public PlayerCollection Players { get; private set; }

		/// <summary>
		///   Gets or sets a handler for messages that should be broadcast to all connected clients.
		/// </summary>
		public Action<Message> Broadcast { get; set; }

		/// <summary>
		///   Gets or sets the scene node that represents the mouse effect.
		/// </summary>
		public SceneNode MouseEffect { get; set; }

		/// <summary>
		///   Changes level that is used by the game session.
		/// </summary>
		/// <param name="level">The level that should be used by the game session.</param>
		public void ChangeLevel(Level level)
		{
			Assert.ArgumentNotNull(level, nameof(level));

			Level = level;

			if (ServerMode)
				CreateSpawners();
			else
				LevelRenderer = new LevelRenderer(level);
		}

		/// <summary>
		///   Allocates an instance of the given type using the game session's allocator.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be allocated.</typeparam>
		public T Allocate<T>()
			where T : class, new()
		{
			return Allocator.Allocate<T>();
		}

		/// <summary>
		///   Raised when an entity has been added to the game session.
		/// </summary>
		public event Action<Entity> EntityAdded;

		/// <summary>
		///   Raised when an entity has been removed from the game session.
		/// </summary>
		public event Action<Entity> EntityRemoved;

		/// <summary>
		///   If an entity has been added, raises the entity added event.
		/// </summary>
		/// <param name="sceneNode">The scene node that has been added.</param>
		private void OnNodeAdded(SceneNode sceneNode)
		{
			var entity = sceneNode as Entity;
			if (entity != null)
				EntityAdded?.Invoke(entity);
		}

		/// <summary>
		///   If an entity has been removed, raises the entity removed event.
		/// </summary>
		/// <param name="sceneNode">The scene node that has been removed.</param>
		private void OnNodeRemoved(SceneNode sceneNode)
		{
			var entity = sceneNode as Entity;
			if (entity != null)
				EntityRemoved?.Invoke(entity);
		}

		/// <summary>
		///   Initializes a client-side game session.
		/// </summary>
		public void InitializeClient()
		{
			ServerMode = false;
			Players = new PlayerCollection(Allocator, serverMode: false);
			Effects = new ParticleEffects(this);
			EntityRenderer = new EntityRenderer();
		}

		/// <summary>
		///   Initializes a host game session.
		/// </summary>
		/// <param name="serverLogic">The server logic that handles the communication between the server and the clients.</param>
		public void InitializeServer(ServerLogic serverLogic)
		{
			Assert.ArgumentNotNull(serverLogic, nameof(serverLogic));

			ServerMode = true;
			Players = new PlayerCollection(Allocator, serverMode: true);
			Physics = new PhysicsSimulation(this);

			ChangeLevel(AssetBundle.TestLevel);
		}

		/// <summary>
		///   Updates the state of a client game session.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public void UpdateClient(float elapsedSeconds)
		{
			SceneGraph.Update();

			foreach (var entity in SceneGraph.EnumeratePreOrder<Entity>())
				entity.ClientUpdate(elapsedSeconds);

			SceneGraph.ExecuteBehaviors(elapsedSeconds);

			foreach (var particleNode in SceneGraph.EnumeratePostOrder<ParticleEffectNode>())
					particleNode.Update(elapsedSeconds);

			SceneGraph.Update();
		}

		/// <summary>
		///   Updates the state of a server game session.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public void UpdateServer(float elapsedSeconds)
		{
			SceneGraph.Update();
			Physics.Update(elapsedSeconds);

			foreach (var entity in SceneGraph.EnumeratePreOrder<Entity>())
				entity.ServerUpdate(elapsedSeconds);

			SceneGraph.ExecuteBehaviors(elapsedSeconds);
			SceneGraph.Update();

			Players.UpdatePlayerRanks();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			SceneGraph.SafeDispose();
			Players.SafeDispose();
			Effects.SafeDispose();
		}

		/// <summary>
		///   Creates the spawners within the level.
		/// </summary>
		private void CreateSpawners()
		{
			for (var x = 0; x < Level.Width; ++x)
			{
				for (var y = 0; y < Level.Height; ++y)
				{
					var type = Level[x, y];
					if (type.IsCollectible())
						SceneGraph.Root.AddBehavior(SpawnBehavior.Create(this, Level.GetBlockArea(x, y).Center, type));
				}
			}
		}
	}
}