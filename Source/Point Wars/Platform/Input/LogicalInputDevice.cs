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

namespace PointWars.Platform.Input
{
	using System;
	using System.Collections.Generic;
	using Logging;
	using Memory;
	using Utilities;

	/// <summary>
	///   Manages logical inputs that are triggered by input triggers. A logical input device has support for 32 unique and
	///   prioritized input layers that determine which inputs can be triggered. Higher-numbered layers take priority over
	///   lower-numbered ones, effectively disabling all input to lower layers. Layers are activated and deactivated using the
	///   ActivateLayer and DeactivateLayer methods, where the number of calls to DeactivateLayer for a given layer n must
	///   match the number of calls to ActivateLayer for n before n is considered inactive again. This way, the order of
	///   activation and deactivation is not important. The actual layer activation and deactivation is deferred one frame.
	/// </summary>
	public class LogicalInputDevice : DisposableObject
	{
		/// <summary>
		///   The logical inputs that are currently registered on the device.
		/// </summary>
		private readonly List<LogicalInput> _inputs = new List<LogicalInput>(256);

		/// <summary>
		///   Stores the activation states of the currently active input layers.
		/// </summary>
		private readonly ActivationState[] _layerStates = new ActivationState[Enum.GetValues(typeof(InputLayer)).Length];

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="window">The window that should be associated with this logical device.</param>
		public LogicalInputDevice(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			Keyboard = new Keyboard(window);
			Mouse = new Mouse(window);
		}

		/// <summary>
		///   Gets the current input layer used by the input device.
		/// </summary>
		public InputLayer InputLayer { get; private set; }

		/// <summary>
		///   Gets the keyboard that is associated with this logical device.
		/// </summary>
		public Keyboard Keyboard { get; }

		/// <summary>
		///   Gets the mouse that is associated with this logical device.
		/// </summary>
		public Mouse Mouse { get; }

		/// <summary>
		///   Registers a logical input on the logical input device. Subsequently, the logical input's IsTriggered
		///   property can be used to determine whether the logical input is currently triggered.
		/// </summary>
		/// <param name="input">The logical input that should be registered on the device.</param>
		public void Add(LogicalInput input)
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Assert.ArgumentSatisfies(!input.IsRegistered, nameof(input), "The input is already registered on a device.");

			_inputs.Add(input);
			input.SetLogicalDevice(this);

			Log.Debug("A logical input with trigger '{0}' has been registered.", input.Trigger);
		}

		/// <summary>
		///   Removes the logical input from the logical input device.
		/// </summary>
		/// <param name="input">The logical input that should be removed.</param>
		public void Remove(LogicalInput input)
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Assert.ArgumentSatisfies(input.IsRegistered, nameof(input), "The input trigger is not registered.");

			if (_inputs.Remove(input))
				input.SetLogicalDevice(null);
		}

		/// <summary>
		///   Activates the given input layer on the device, disabling all lower input layers starting with the next frame.
		/// </summary>
		/// <param name="layer">The layer that should be activated.</param>
		public void ActivateLayer(InputLayer layer)
		{
			Assert.ArgumentInRange(layer, nameof(layer));

			if (layer == InputLayer.None)
				return;

			for (var i = 0; i < 32; ++i)
			{
				if ((int)layer == 1 << i)
				{
					_layerStates[i].Activate();
					return;
				}
			}

			Assert.NotReached("Unknown input layer.");
		}

		/// <summary>
		///   Deactivates the given input layer on the device, only enabling the next lower active input layer if the number of
		///   deactivation requests for the given layer matches the number of activation requests. The actual deactivation is
		///   deferred until the next frame.
		/// </summary>
		/// <param name="layer">The layer that should be activated.</param>
		public void DeactivateLayer(InputLayer layer)
		{
			Assert.ArgumentInRange(layer, nameof(layer));

			if (layer == InputLayer.None)
				return;

			for (var i = 1; i < 32; ++i)
			{
				if ((int)layer != 1 << i)
					continue;

				_layerStates[i].Deactivate();
				return;
			}

			Assert.NotReached("Unknown input layer.");
		}

		/// <summary>
		///   Updates the device state.
		/// </summary>
		internal void Update()
		{
			// Perform all deferred layer activations and deactivations
			for (var i = 0; i < _layerStates.Length; ++i)
				_layerStates[i].Update();

			// Determine the currently active input layout with the highest priority
			for (var i = _layerStates.Length; i > 0; --i)
			{
				if (_layerStates[i - 1].Count > 0)
				{
					var layer = (InputLayer)(1 << (i - 1));
					if (InputLayer != layer)
						Log.Debug("Input layer has been switched to '{0}'.", layer);

					InputLayer = layer;
					break;
				}
			}

			// Update all inputs
			foreach (var input in _inputs)
				input.Update(this);

			Keyboard.Update();
			Mouse.Update();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Keyboard.SafeDispose();
			Mouse.SafeDispose();
		}
	}
}