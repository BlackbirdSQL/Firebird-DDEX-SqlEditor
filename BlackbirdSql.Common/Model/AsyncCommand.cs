// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.Commands.AsyncCommand<TResult>

using System;
using System.Threading.Tasks;
using System.Windows.Input;

using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Interfaces;



namespace BlackbirdSql.Common.Model;


public class AsyncCommand<TResult> : AbstractCommand, IAsyncCommand, IOwnedCommand, ICommand
{
	private readonly Func<object, Task<TResult>> _Command;

	public NotifyTaskCompletion<TResult> Execution { get; private set; }

	public AsyncCommand(Func<object, Task<TResult>> command)
		: this(command, null)
	{
	}

	public AsyncCommand(Func<object, Task<TResult>> command, Predicate<object> canExecute)
		: base(canExecute)
	{
		_Command = command;
	}



	public override IBPropertyAgent Copy()
	{
		return new AsyncCommand<TResult>(_Command);
	}

	public override async void Execute(object parameter)
	{
		await ExecuteAsync(parameter);
	}

	public async Task ExecuteAsync(object parameter)
	{
		IsExecuting = true;
		Execution = new NotifyTaskCompletion<TResult>(_Command(parameter));
		RaiseCanExecuteChanged();
		if (Execution.TaskCompletion != null)
		{
			await Execution.TaskCompletion;
		}
		if (CheckAccess())
		{
			IsExecuting = false;
			RaiseCanExecuteChanged();
		}
	}
}
