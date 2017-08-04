namespace OrbsHavoc.Gameplay
{
	using System.Collections.Generic;
	using System.Numerics;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Represents a level within which a game session takes place.
	/// </summary>
	public class Level : DisposableObject
	{
		/// <summary>
		///   The size of a block within the
		/// </summary>
		public const float BlockSize = 128;

		/// <summary>
		///   The thickness of the walls.
		/// </summary>
		public const float WallThickness = 5.0f;

		/// <summary>
		///   Gets the level's position offset.
		/// </summary>
		public Vector2 PositionOffset { get; private set; }

		/// <summary>
		///   Gets the level's width in block units.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		///   Gets the level's height in block units.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		///   Gets the player start positions within the level.
		/// </summary>
		public List<(int x, int y)> PlayerStarts { get; } = new List<(int x, int y)>();

		/// <summary>
		///   Gets the level's blocks.
		/// </summary>
		public EntityType[] Blocks { get; private set; }

		/// <summary>
		///   Gets the block type at the given location.
		/// </summary>
		/// <param name="x">The zero-based index in x-direction.</param>
		/// <param name="y">The zero-based index in y-direction.</param>
		public EntityType this[int x, int y] => x < 0 || y < 0 || x >= Width || y >= Height ? EntityType.Wall : Blocks[x * Height + y];

		/// <summary>
		///   Loads the level from the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the level should be loaded from.</param>
		public void Load(ref BufferReader buffer)
		{
			Width = buffer.ReadInt16();
			Height = buffer.ReadInt16();

			PositionOffset = new Vector2(-MathUtils.Round(Width / 2.0f * BlockSize), -MathUtils.Round(Height / 2.0f * BlockSize));
			Blocks = new EntityType[Width * Height];

			for (var x = 0; x < Width; ++x)
			{
				for (var y = 0; y < Height; ++y)
				{
					Blocks[x * Height + y] = (EntityType)buffer.ReadByte();
					if (Blocks[x * Height + y] == EntityType.PlayerStart)
						PlayerStarts.Add((x, y));
				}
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			// Nothing to do here; however, the assets bundle generator expects all assets to be disposable
		}

		/// <summary>
		///   Gets the blocks covered by the circle at the given position with the given radius.
		/// </summary>
		/// <param name="circle">The circle the covered blocks should be returned for.</param>
		/// <returns>
		///   xFirst: Gets the zero-based index in x-direction of the first covered block.
		///   yFirst: Gets the zero-based index in y-direction of the first covered block.
		///   xLast: Gets the zero-based index in x-direction of the last covered block.
		///   yLast: Gets the zero-based index in y-direction of the last covered block.
		/// </returns>
		public (int xFirst, int yFirst, int xLast, int yLast) GetCoveredBlocks(Circle circle)
		{
			var (xFirst, yFirst) = GetBlock(circle.Position - new Vector2(circle.Radius, circle.Radius));
			var (xLast, yLast) = GetBlock(circle.Position + new Vector2(circle.Radius, circle.Radius));

			return (xFirst, yFirst, xLast, yLast);
		}

		/// <summary>
		///   Gets the block the given position lies within.
		/// </summary>
		/// <param name="position">The position the block should be returned for.</param>
		public (int x, int y) GetBlock(Vector2 position)
		{
			position -= PositionOffset;

			var x = (int)(position.X / BlockSize);
			var y = (int)(position.Y / BlockSize);

			return (x, y);
		}

		/// <summary>
		///   Gets the area occupied by the block with the given index.
		/// </summary>
		/// <param name="index">The index of the block whose area should be returned.</param>
		public Rectangle GetBlockArea((int x, int y) index)
		{
			return GetBlockArea(index.x, index.y);
		}

		/// <summary>
		///   Gets the area occupied by the block at the given location.
		/// </summary>
		/// <param name="x">The zero-based index of the block in x-direction.</param>
		/// <param name="y">The zero-based index of the block in y-direction.</param>
		public Rectangle GetBlockArea(int x, int y)
		{
			Assert.InRange(x, 0, Width - 1);
			Assert.InRange(y, 0, Height - 1);

			return new Rectangle(x * BlockSize + PositionOffset.X, y * BlockSize + PositionOffset.Y, BlockSize, BlockSize);
		}

		/// <summary>
		///   Casts a ray into the world and returns the result of the nearest collision.
		/// </summary>
		/// <param name="start">The start position of the ray.</param>
		/// <param name="normalizedDirection">The normalized direction of the ray.</param>
		/// <param name="length">The length of the ray.</param>
		public float RayCast(Vector2 start, Vector2 normalizedDirection, float length)
		{
			var position = start;
			var distanceToWall = 0.0f;

			const float bigStep = BlockSize / 2;
			const float smallStep = BlockSize / 32;

			while (distanceToWall < length)
			{
				var (x, y) = GetBlock(position);
				var blockType = this[x, y];

				if (blockType.IsWall())
				{
					var info = CheckWallCollision(new Circle(position, smallStep / 2));
					if (info.HasValue)
						return distanceToWall - info.Value.Offset.Length();

					// We've hit a wall block, so we have to do a more thorough check now
					distanceToWall += smallStep;
				}
				else
				{
					// We're not hitting a wall, advance in big steps
					distanceToWall += bigStep;
				}

				position = start + normalizedDirection * distanceToWall;
			}

			return length;
		}

		/// <summary>
		///   Checks whether the given collider collides with a wall.
		/// </summary>
		/// <param name="collider">The collider that should be checked for wall collisions.</param>
		public CollisionInfo? CheckWallCollision(Circle collider)
		{
			var (x, y) = GetBlock(collider.Position);

			var blockType = this[x, y];
			Vector2 position;
			Size size;

			switch (blockType)
			{
				case EntityType.TopWall:
					position = GetBlockArea(x, y).TopLeft;
					size = new Size(BlockSize, BlockSize / 2 + WallThickness / 2);
					return HandleWallCollision(collider, new Rectangle(position, size), blockType);
				case EntityType.LeftWall:
					position = GetBlockArea(x, y).TopLeft;
					size = new Size(BlockSize / 2 + WallThickness / 2, BlockSize);
					return HandleWallCollision(collider, new Rectangle(position, size), blockType);
				case EntityType.BottomWall:
					position = GetBlockArea(x, y).TopLeft + new Vector2(0, BlockSize / 2 - WallThickness / 2);
					size = new Size(BlockSize, BlockSize / 2 + WallThickness);
					return HandleWallCollision(collider, new Rectangle(position, size), blockType);
				case EntityType.RightWall:
					position = GetBlockArea(x, y).TopLeft + new Vector2(BlockSize / 2 - WallThickness / 2, 0);
					size = new Size(BlockSize / 2 + WallThickness, BlockSize);
					return HandleWallCollision(collider, new Rectangle(position, size), blockType);
				case EntityType.LeftTopWall:
					return HandleCollisionWithCurvedWall(collider, GetBlockArea(x, y).BottomRight);
				case EntityType.RightTopWall:
					return HandleCollisionWithCurvedWall(collider, GetBlockArea(x, y).BottomLeft);
				case EntityType.LeftBottomWall:
					return HandleCollisionWithCurvedWall(collider, GetBlockArea(x, y).TopRight);
				case EntityType.RightBottomWall:
					return HandleCollisionWithCurvedWall(collider, GetBlockArea(x, y).TopLeft);
				case EntityType.InverseLeftTopWall:
					return HandleCollisionWithInverseCurvedWall(collider, GetBlockArea(x, y).BottomRight);
				case EntityType.InverseRightTopWall:
					return HandleCollisionWithInverseCurvedWall(collider, GetBlockArea(x, y).BottomLeft);
				case EntityType.InverseLeftBottomWall:
					return HandleCollisionWithInverseCurvedWall(collider, GetBlockArea(x, y).TopRight);
				case EntityType.InverseRightBottomWall:
					return HandleCollisionWithInverseCurvedWall(collider, GetBlockArea(x, y).TopLeft);
				case EntityType.Wall:
					// That should be impossible, but, well...
					return new CollisionInfo(Vector2.Zero, Vector2.Zero, true);
			}

			return null;
		}

		/// <summary>
		///   Handles an entity collision with a horizontal or vertical wall.
		/// </summary>
		private static CollisionInfo? HandleWallCollision(Circle circle, Rectangle wall, EntityType wallType)
		{
			if (!wall.Intersects(circle))
				return null;

			switch (wallType)
			{
				case EntityType.LeftWall:
					return new CollisionInfo(new Vector2(wall.Right - circle.Position.X + circle.Radius, 0), Vector2.UnitX,
						circle.Position.X + circle.Radius < wall.Right);
				case EntityType.RightWall:
					return new CollisionInfo(new Vector2(wall.Left - circle.Position.X - circle.Radius, 0), -Vector2.UnitX,
						circle.Position.X - circle.Radius > wall.Left);
				case EntityType.TopWall:
					return new CollisionInfo(new Vector2(0, wall.Bottom - circle.Position.Y + circle.Radius), Vector2.UnitY,
						circle.Position.Y + circle.Radius < wall.Bottom);
				case EntityType.BottomWall:
					return new CollisionInfo(new Vector2(0, wall.Top - circle.Position.Y - circle.Radius), -Vector2.UnitY,
						circle.Position.Y - circle.Radius > wall.Top);
			}

			return null;
		}

		/// <summary>
		///   Handles an entity collision with a curved wall.
		/// </summary>
		private static CollisionInfo? HandleCollisionWithCurvedWall(Circle circle, Vector2 wallPosition)
		{
			var wallCircle = new Circle(wallPosition, BlockSize / 2 + WallThickness / 2);
			if (!circle.Intersects(wallCircle))
				return null;

			var normal = circle.Position - wallCircle.Position;
			var distance = normal.Length();
			normal /= distance;

			var radius = circle.Radius + wallCircle.Radius;
			var overlap = MathUtils.Abs(distance - radius);

			return new CollisionInfo(normal * overlap, normal, distance + circle.Radius < wallCircle.Radius);
		}

		/// <summary>
		///   Handles an entity collision with a curved wall.
		/// </summary>
		private static CollisionInfo? HandleCollisionWithInverseCurvedWall(Circle circle, Vector2 wallPosition)
		{
			var wallCircle = new Circle(wallPosition, BlockSize / 2 - WallThickness / 2);
			var normal = wallCircle.Position - circle.Position;
			var distance = normal.Length();
			var overlap = distance + circle.Radius - wallCircle.Radius + WallThickness;

			if (overlap < 0)
				return null;

			normal /= distance;
			return new CollisionInfo(normal * overlap, normal, distance > circle.Radius + wallCircle.Radius);
		}

		/// <summary>
		///   Provides information about a collision with a wall.
		/// </summary>
		public struct CollisionInfo
		{
			/// <summary>
			///   Gets the wall normal at the impact position.
			/// </summary>
			public Vector2 Normal { get; }

			/// <summary>
			///   Gets the offset that must be applied to the collider to resolve the collision.
			/// </summary>
			public Vector2 Offset { get; }

			/// <summary>
			///   Gets a value indicating whether the collider is fully submerged into the wall.
			/// </summary>
			public bool IsSubmerged { get; }

			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			public CollisionInfo(Vector2 offset, Vector2 normal, bool isSubmerged)
			{
				Offset = offset;
				Normal = normal;
				IsSubmerged = isSubmerged;
			}
		}
	}
}