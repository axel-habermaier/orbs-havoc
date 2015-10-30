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
	///   Represents an input binding that is triggered by a key event, identifying the key by its scan code.
	/// </summary>
	public sealed class ScanCodeBinding : InputBinding
	{
		/// <summary>
		///   The key modifiers that must be pressed for the binding to trigger.
		/// </summary>
		private readonly KeyModifiers _modifiers;

		/// <summary>
		///   The scan code of the key that activates the binding.
		/// </summary>
		private readonly ScanCode _scanCode;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="callback">The callback that should be invoked when the binding is triggered.</param>
		/// <param name="scanCode">The scan code of the key that should activate the binding.</param>
		/// <param name="modifiers">The key modifiers that must be pressed for the binding to trigger.</param>
		public ScanCodeBinding(Action callback, ScanCode scanCode, KeyModifiers modifiers = KeyModifiers.None)
			: base(callback)
		{
			Assert.ArgumentInRange(scanCode, nameof(scanCode));

			_scanCode = scanCode;
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
					return keyEventArgs.Kind == InputEventKind.Down && keyEventArgs.ScanCode == _scanCode &&
						   keyEventArgs.Modifiers == _modifiers && keyEventArgs.KeyState.WentDown;
				case TriggerMode.Repeatedly:
					return keyEventArgs.Kind == InputEventKind.Down && keyEventArgs.ScanCode == _scanCode &&
						   keyEventArgs.Modifiers == _modifiers && keyEventArgs.KeyState.IsRepeated;
				case TriggerMode.OnDeactivation:
					return keyEventArgs.Kind == InputEventKind.Up && keyEventArgs.ScanCode == _scanCode && keyEventArgs.Modifiers == _modifiers;
				default:
					Log.Die("Unknown trigger mode.");
					return false;
			}
		}
	}
}