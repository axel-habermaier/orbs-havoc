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

namespace PointWars.Gameplay
{
	using System;
	using Entities;
	using Platform.Memory;
	using Scene;
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
		///   Gets the allocator that is used to allocate game objects.
		/// </summary>
		public PoolAllocator Allocator { get; }

		/// <summary>
		///   Gets the scene graph of the game session.
		/// </summary>
		public SceneGraph SceneGraph { get; }

		/// <summary>
		///   Gets the collection of players that are participating in the game session.
		/// </summary>
		public PlayerCollection Players { get; private set; }

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
			Players = new PlayerCollection(Allocator, serverMode: false);
		}

		/// <summary>
		///   Initializes a server-side game session.
		/// </summary>
		/// <param name="serverLogic">The server logic that handles the communication between the server and the clients.</param>
		public void InitializeServer(ServerLogic serverLogic)
		{
			Assert.ArgumentNotNull(serverLogic, nameof(serverLogic));

			Players = new PlayerCollection(Allocator, serverMode: true);
		}

		/// <summary>
		///   Updates the state of the game session.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public void Update(float elapsedSeconds)
		{
			foreach (var entity in SceneGraph.EnumeratePreOrder<Entity>())
				entity.Update(elapsedSeconds);

			SceneGraph.ExecuteBehaviors(elapsedSeconds);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			SceneGraph.SafeDispose();
			Players.SafeDispose();
		}
	}
}