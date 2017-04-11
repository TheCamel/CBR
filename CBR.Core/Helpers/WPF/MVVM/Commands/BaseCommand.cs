using System;
using System.Windows.Input;

namespace CBR.Core.Helpers
{
	internal class BaseCommand : ICommand
	{
		private readonly Action _command;
		private readonly Func<bool> _canExecute;

		public BaseCommand(Action command, Func<bool> canExecute = null)
		{
			if (command == null)
				throw new ArgumentNullException("command");
			_canExecute = canExecute;
			_command = command;
		}

		public void Execute(object parameter)
		{
			_command();
		}

		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
				return true;
			return _canExecute();
		}

		public event EventHandler CanExecuteChanged;
	}
}