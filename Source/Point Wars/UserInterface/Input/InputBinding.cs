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
	using Utilities;

	/// <summary>
	///   Represents an input binding that invokes a method on the associated UI element's data context whenever it is triggered.
	/// </summary>
	public abstract class InputBinding
	{
		/// <summary>
		///   The callback that is invoked when the binding is triggered.
		/// </summary>
		private Action _callback;

		/// <summary>
		///   The target method on the associated UI element that is invoked whenever the binding is triggered.
		/// </summary>
		private Action _targetMethod;

		/// <summary>
		///   The trigger mode of the input binding.
		/// </summary>
		private TriggerMode _triggerMode = TriggerMode.Activated;

		/// <summary>
		///   Gets or sets the callback that is invoked when the binding is triggered.
		/// </summary>
		public Action Callback
		{
			get { return _callback; }
			set
			{
				Assert.ArgumentNotNull(value, nameof(value));
				_callback = value;
			}
		}

		/// <summary>
		///   Gets or sets the trigger mode of the input binding.
		/// </summary>
		public TriggerMode TriggerMode
		{
			get { return _triggerMode; }
			set
			{
				Assert.ArgumentInRange(value, nameof(value));
				_triggerMode = value;
			}
		}

		/// <summary>
		///   Handles the given event, invoking the target method if the input binding is triggered.
		/// </summary>
		/// <param name="args">The arguments of the event that should be handled.</param>
		internal void HandleEvent(RoutedEventArgs args)
		{
			Assert.ArgumentNotNull(args, nameof(args));

			if (_targetMethod == null || !IsTriggered(args))
				return;

			_targetMethod();
			args.Handled = true;
		}

		/// <summary>
		///   Checks whether the given event triggers the input binding.
		/// </summary>
		/// <param name="args">The arguments of the event that should be checked.</param>
		protected abstract bool IsTriggered(RoutedEventArgs args);
	}
}