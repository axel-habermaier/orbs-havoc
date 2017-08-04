namespace OrbsHavoc.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Parsing;
	using Platform.Input;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Manages all registered instruction bindings.
	/// </summary>
	internal class BindingCollection : DisposableObject
	{
		/// <summary>
		///   The registered instruction bindings.
		/// </summary>
		private readonly List<Binding> _bindings = new List<Binding>();

		/// <summary>
		///   The logical input device that is used to determine whether the logical inputs are triggered.
		/// </summary>
		private readonly LogicalInputDevice _device;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="device">The logical input device that is used to determine whether the logical inputs are triggered.</param>
		public BindingCollection(LogicalInputDevice device)
		{
			Assert.ArgumentNotNull(device, nameof(device));

			_device = device;

			Commands.OnBind += OnBind;
			Commands.OnUnbind += OnUnbind;
			Commands.OnUnbindAll += OnUnbindAll;
			Commands.OnListBindings += OnListBindings;
		}

		/// <summary>
		///   Executes all instructions for which the binding's trigger has been triggered.
		/// </summary>
		public void Update()
		{
			foreach (var binding in _bindings)
				binding.ExecuteIfTriggered();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnBind -= OnBind;
			Commands.OnUnbind -= OnUnbind;
			Commands.OnUnbindAll -= OnUnbindAll;
			Commands.OnListBindings -= OnListBindings;
		}

		/// <summary>
		///   Lists all active bindings.
		/// </summary>
		private void OnListBindings()
		{
			var builder = new StringBuilder();
			var bindingGroups = (from binding in _bindings
								 group binding by binding.Command
								 into bindingGroup
								 orderby bindingGroup.Key
								 select new { Command = bindingGroup.Key, Bindings = bindingGroup.ToArray() }).ToArray();

			builder.Append("\n");
			foreach (var group in bindingGroups)
			{
				builder.AppendFormat("'\\lightgrey{0}\\default' on ", group.Command);
				builder.Append(String.Join(", ",
					group.Bindings.Select(binding => $"\\lightgrey{TypeRegistry.ToString(binding.Trigger)}\\default")));
				builder.Append("\n");
			}

			if (bindingGroups.Length == 0)
			{
				Log.Warn("There are no registered bindings.");
				return;
			}

			Log.Info(builder.ToString());
		}

		/// <summary>
		///   Removes all bindings.
		/// </summary>
		private void OnUnbindAll()
		{
			_bindings.Clear();
			Log.Info("All command bindings have been removed.");
		}

		/// <summary>
		///   Invoked when the unbind command is used.
		/// </summary>
		/// <param name="trigger">The trigger that should be unbound.</param>
		private void OnUnbind(InputTrigger trigger)
		{
			var removed = 0;
			for (var i = 0; i < _bindings.Count; ++i)
			{
				if (_bindings[i].Trigger != trigger)
					continue;

				++removed;
				_bindings.RemoveAt(i);
				--i;
			}

			if (removed == 1)
				Log.Info($"The command binding for '{TypeRegistry.ToString(trigger)}' has been removed.");
			else if (removed != 0)
				Log.Info($"{removed} command bindings for '{TypeRegistry.ToString(trigger)}' have been removed.");
			else
			{
				Log.Error($"No binding could be found with trigger '{TypeRegistry.ToString(trigger)}'. " +
						  "Use the 'list_bindings' command to view all active bindings.");
			}
		}

		/// <summary>
		///   Invoked when the bind command is used.
		/// </summary>
		/// <param name="trigger">The trigger that should be bound.</param>
		/// <param name="command">The instruction that should be bound.</param>
		private void OnBind(InputTrigger trigger, string command)
		{
			var input = new LogicalInput(trigger, TriggerType.WentDown);
			_device.Add(input);

			try
			{
				_bindings.Add(new Binding(trigger, input, command, Parser.ParseInstruction(new InputStream(command))));
			}
			catch (ParseException e)
			{
				_bindings.Add(new Binding(trigger, input, command, e.Message));
			}
		}
	}
}