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
	/// <summary>
	///   Identifies the type of power up.
	/// </summary>
	public enum PowerUpType : byte
	{
		/// <summary>
		///   Indicates that a player is not affected by any power up.
		/// </summary>
		None,

		/// <summary>
		///   The armor power up increases the player's resistance to damage.
		/// </summary>
		Armor,

		/// <summary>
		///   The regeneration power up increases the player's health continuously, exceeding the usual maximum.
		/// </summary>
		Regeneration,

		/// <summary>
		///   The quad damage power up quadruples the player's damage.
		/// </summary>
		QuadDamage,

		/// <summary>
		///   The speed power up increases both the player's movement speed as well as the firing rate.
		/// </summary>
		Speed,

		/// <summary>
		///   The invisibility power up makes the player almost invisible.
		/// </summary>
		Invisibility
	}
}