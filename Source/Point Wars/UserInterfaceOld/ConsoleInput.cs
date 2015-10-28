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

namespace PointWars.UserInterfaceOld
{
	using Platform.Input;
	using Platform.Memory;

	/// <summary>
	///   Manages the input functions for the console.
	/// </summary>
	internal class ConsoleInput : DisposableObject
	{
		/// <summary>
		///   The logical input device that provides the user input.
		/// </summary>
		private readonly LogicalInputDevice _device;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="device">The logical input device that provides the user input.</param>
		public ConsoleInput(LogicalInputDevice device)
		{
			_device = device;

			// We don't care which of the two control buttons has been pressed
			var controlPressed = Key.LeftControl.IsPressed() | Key.RightControl.IsPressed();
			var controlReleased = Key.LeftControl.IsReleased() | Key.RightControl.IsReleased();

			Toggle = new LogicalInput(new ScanCodeKeyTrigger(KeyTriggerType.WentDown, ScanCode.Grave), InputLayer.All);
			Submit = new LogicalInput(Key.Enter.WentDown() | Key.NumpadEnter.WentDown(), InputLayer.Console);
			Clear = new LogicalInput(controlPressed + Key.L.IsPressed(), InputLayer.Console);
			ClearPrompt = new LogicalInput(Key.Escape.WentDown(), InputLayer.Console);
			ShowOlderHistory = new LogicalInput(Key.Up.IsRepeated(), InputLayer.Console);
			ShowNewerHistory = new LogicalInput(Key.Down.IsRepeated(), InputLayer.Console);
			ScrollUp = new LogicalInput(Key.PageUp.IsRepeated() & controlReleased, InputLayer.Console);
			ScrollDown = new LogicalInput(Key.PageDown.IsRepeated() & controlReleased, InputLayer.Console);
			ScrollToTop = new LogicalInput(controlPressed + Key.PageUp.IsPressed(), InputLayer.Console);
			ScrollToBottom = new LogicalInput(controlPressed + Key.PageDown.IsPressed(), InputLayer.Console);
			AutoCompleteNext = new LogicalInput(Key.Tab.WentDown() & Key.LeftShift.IsReleased(), InputLayer.Console);
			AutoCompletePrevious = new LogicalInput(Key.Tab.IsPressed() + Key.LeftShift.IsPressed(), InputLayer.Console);

			device.Add(Toggle);
			device.Add(Submit);
			device.Add(Clear);
			device.Add(ClearPrompt);
			device.Add(ShowOlderHistory);
			device.Add(ShowNewerHistory);
			device.Add(ScrollUp);
			device.Add(ScrollDown);
			device.Add(ScrollToTop);
			device.Add(ScrollToBottom);
			device.Add(AutoCompleteNext);
			device.Add(AutoCompletePrevious);
		}

		/// <summary>
		///   Gets the logical input for the console's toggle action.
		/// </summary>
		public LogicalInput Toggle { get; }

		/// <summary>
		///   Gets the logical input for the console's clear action.
		/// </summary>
		public LogicalInput Clear { get; }

		/// <summary>
		///   Gets the logical input for the console's clear prompt action.
		/// </summary>
		public LogicalInput ClearPrompt { get; }

		/// <summary>
		///   Gets the logical input for the console's scroll down action.
		/// </summary>
		public LogicalInput ScrollDown { get; }

		/// <summary>
		///   Gets the logical input for the console's scroll to bottom action.
		/// </summary>
		public LogicalInput ScrollToBottom { get; }

		/// <summary>
		///   Gets the logical input for the console's scroll to top action.
		/// </summary>
		public LogicalInput ScrollToTop { get; }

		/// <summary>
		///   Gets the logical input for the console's scroll up action.
		/// </summary>
		public LogicalInput ScrollUp { get; }

		/// <summary>
		///   Gets the logical input for the console's show new history action.
		/// </summary>
		public LogicalInput ShowNewerHistory { get; }

		/// <summary>
		///   Gets the logical input for the console's show older history action.
		/// </summary>
		public LogicalInput ShowOlderHistory { get; }

		/// <summary>
		///   Gets the logical input for the console's submit action.
		/// </summary>
		public LogicalInput Submit { get; }

		/// <summary>
		///   Gets the logical input for the console's auto-completion action in forward direction.
		/// </summary>
		public LogicalInput AutoCompleteNext { get; }

		/// <summary>
		///   Gets the logical input for the console's auto-completion action in backwards direction.
		/// </summary>
		public LogicalInput AutoCompletePrevious { get; }

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_device.Remove(Toggle);
			_device.Remove(Submit);
			_device.Remove(Clear);
			_device.Remove(ClearPrompt);
			_device.Remove(ShowOlderHistory);
			_device.Remove(ShowNewerHistory);
			_device.Remove(ScrollUp);
			_device.Remove(ScrollDown);
			_device.Remove(ScrollToTop);
			_device.Remove(ScrollToBottom);
			_device.Remove(AutoCompleteNext);
			_device.Remove(AutoCompletePrevious);
		}
	}
}