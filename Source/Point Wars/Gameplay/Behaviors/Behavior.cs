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

namespace PointWars.Gameplay.Behaviors
{
	using Platform.Memory;
	using SceneNodes;

	/// <summary>
	///   Represents a behavior that can be attached to a scene node.
	/// </summary>
	public abstract class Behavior : PooledObject
	{
		/// <summary>
		///   Gets or sets the next behavior in an intrusive list.
		/// </summary>
		public Behavior Next { get; set; }

		/// <summary>
		///   Gets or sets the previous behavior in an intrusive list.
		/// </summary>
		public Behavior Previous { get; set; }

		/// <summary>
		///   Gets the scene node the behavior is attached to.
		/// </summary>
		public abstract SceneNode GetSceneNode();

		/// <summary>
		///   Attaches the behavior to the given scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node the behavior should be attached to.</param>
		public abstract void Attach(SceneNode sceneNode);

		/// <summary>
		///   Detaches the behavior from the scene node it is attached to.
		/// </summary>
		public abstract void Detach();

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public abstract void Execute(float elapsedSeconds);
	}
}