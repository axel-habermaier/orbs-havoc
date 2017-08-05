namespace OrbsHavoc.UserInterface.Input
{
	using System;
	using Platform.Input;
	using Scripting;
	using Utilities;

	public sealed class ConfigurableBinding : InputBinding
	{
		private readonly Cvar<InputTrigger> _cvar;

		public ConfigurableBinding(Action callback, Cvar<InputTrigger> cvar)
			: base(callback)
		{
			Assert.ArgumentNotNull(cvar, nameof(cvar));
			_cvar = cvar;
		}

		protected override bool IsTriggered(InputEventArgs args)
		{
			return _cvar.Value.IsTriggered(args.Keyboard, args.Mouse, GetTriggerType());
		}

		private TriggerType GetTriggerType()
		{
			switch (TriggerMode)
			{
				case TriggerMode.OnActivation:
					return TriggerType.WentDown;
				case TriggerMode.Repeatedly:
					return TriggerType.Repeated;
				case TriggerMode.OnDeactivation:
					return TriggerType.WentUp;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}