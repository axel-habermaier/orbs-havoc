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

namespace PointWars.Rendering.Particles
{
	using System;
	using System.Numerics;
	using System.Runtime.InteropServices;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents a collection of a particle emitter's live particles.
	/// </summary>
	public sealed unsafe class ParticleCollection : DisposableObject
	{
		/// <summary>
		///   The pointer to the memory allocated for the particle collection.
		/// </summary>
		private readonly void* _memory;

		/// <summary>
		///   The size of the allocated memory in bytes.
		/// </summary>
		private readonly int _size;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="emitter">The emitter the particles belong to.</param>
		/// <param name="capacity">The maximum number of particles that the collection should be able to contain.</param>
		internal ParticleCollection(Emitter emitter, int capacity)
		{
			Assert.ArgumentNotNull(emitter, nameof(emitter));

			Emitter = emitter;
			Capacity = capacity;

			var particleSize = sizeof(Vector2) + // positions
							   sizeof(Vector2) + // velocities
							   sizeof(float) + // initial speed
							   sizeof(Color) + // colors
							   sizeof(float) + // remaining lifetimes
							   sizeof(float) + // initial lifetimes
							   sizeof(float) + // age
							   sizeof(float) + // orientation
							   sizeof(float); // scales

			_size = particleSize * Capacity;
			_memory = Marshal.AllocHGlobal(_size).ToPointer();

			GC.AddMemoryPressure(_size);

			var pointer = (byte*)_memory;
			Positions = (Vector2*)pointer;
			pointer += sizeof(Vector2) * Capacity;

			Velocities = (Vector2*)pointer;
			pointer += sizeof(Vector2) * Capacity;

			InitialSpeeds = (float*)pointer;
			pointer += sizeof(float) * Capacity;

			Colors = (Color*)pointer;
			pointer += sizeof(Color) * Capacity;

			Lifetimes = (float*)pointer;
			pointer += sizeof(float) * Capacity;

			InitialLifetimes = (float*)pointer;
			pointer += sizeof(float) * Capacity;

			Ages = (float*)pointer;
			pointer += sizeof(float) * Capacity;

			Orientations = (float*)pointer;
			pointer += sizeof(float) * Capacity;

			Scales = (float*)pointer;
		}

		/// <summary>
		///   Gets the emitter the particles belong to.
		/// </summary>
		public Emitter Emitter { get; }

		/// <summary>
		///   Gets the maximum number of particles that can be stored in the collection.
		/// </summary>
		internal int Capacity { get; }

		/// <summary>
		///   Stores the age of each particle as a floating-point value in the range [0,1], starting at 1 and decreasing to 0 at the
		///   end of the particle's life.
		/// </summary>
		public float* Ages { get; }

		/// <summary>
		///   Stores the scale of each particle as a floating-point value.
		/// </summary>
		public float* Scales { get; }

		/// <summary>
		///   Stores the orientation of each particle as a floating-point value.
		/// </summary>
		public float* Orientations { get; }

		/// <summary>
		///   Stores the remaining life time time of each particle as a floating-point value in seconds.
		/// </summary>
		public float* Lifetimes { get; }

		/// <summary>
		///   Stores the initial life time time of each particle as a floating-point value in seconds.
		/// </summary>
		public float* InitialLifetimes { get; }

		/// <summary>
		///   Stores the color of each particle.
		/// </summary>
		public Color* Colors { get; }

		/// <summary>
		///   Stores the position of each particle.
		/// </summary>
		public Vector2* Positions { get; }

		/// <summary>
		///   Stores the velocity of each particle.
		/// </summary>
		public Vector2* Velocities { get; }

		/// <summary>
		///   Stores the initial speed of each particle.
		/// </summary>
		public float* InitialSpeeds { get; }

		/// <summary>
		///   Copies the particle at the source index to the target index.
		/// </summary>
		/// <param name="source">The index of the source particle.</param>
		/// <param name="target">The index of the target particle.</param>
		public void Copy(int source, int target)
		{
			Assert.InRange(source, 0, Capacity - 1);
			Assert.InRange(target, 0, Capacity - 1);

			if (source == target)
				return;

			Positions[target] = Positions[source];
			Velocities[target] = Velocities[source];
			InitialSpeeds[target] = InitialSpeeds[source];
			Colors[target] = Colors[source];
			Lifetimes[target] = Lifetimes[source];
			InitialLifetimes[target] = InitialLifetimes[source];
			Ages[target] = Ages[source];
			Orientations[target] = Orientations[source];
			Scales[target] = Scales[source];
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Marshal.FreeHGlobal(new IntPtr(_memory));
			GC.RemoveMemoryPressure(_size);
		}
	}
}