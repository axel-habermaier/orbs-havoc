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

namespace PointWars.UserInterface.Input
{
	using System;
	using Platform.Input;
	using Platform.Logging;
	using Utilities;

	/// <summary>
	///   Represents an input binding that is triggered by a key event.
	/// </summary>
	public sealed class KeyBinding : InputBinding
	{
		private readonly Key _key;
		private readonly KeyModifiers _modifiers;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="callback">The callback that should be invoked when the binding is triggered.</param>
		/// <param name="key">The key that should activate the binding.</param>
		/// <param name="modifiers">The key modifiers that must be pressed for the binding to trigger.</param>
		/// <param name="triggerMode">Determines when the input binding should be triggered.</param>
		public KeyBinding(Action callback, Key key, KeyModifiers modifiers = KeyModifiers.None, TriggerMode triggerMode = TriggerMode.OnActivation)
			: base(callback, triggerMode)
		{
			Assert.ArgumentInRange(key, nameof(key));

			_key = key;
			_modifiers = modifiers;
		}

		/// <summary>
		///   Checks whether the given event triggers the input binding.
		/// </summary>
		/// <param name="args">The arguments of the event that should be checked.</param>
		protected override bool IsTriggered(InputEventArgs args)
		{
			var keyEventArgs = args as KeyEventArgs;
			if (keyEventArgs == null)
				return false;

			switch (TriggerMode)
			{
				case TriggerMode.OnActivation:
					return keyEventArgs.Key == _key && keyEventArgs.Modifiers == _modifiers && keyEventArgs.KeyState.WentDown;
				case TriggerMode.Repeatedly:
					return keyEventArgs.Key == _key && keyEventArgs.Modifiers == _modifiers &&
						   !keyEventArgs.KeyState.WentDown && keyEventArgs.KeyState.IsRepeated;
				case TriggerMode.OnDeactivation:
					return keyEventArgs.KeyState.WentUp && keyEventArgs.Key == _key && keyEventArgs.Modifiers == _modifiers;
				default:
					Log.Die("Unknown trigger mode.");
					return false;
			}
		}
	}
}