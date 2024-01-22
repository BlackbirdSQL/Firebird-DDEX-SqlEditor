// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.Commands.AsyncCommand<TResult>

using System;
using System.Threading.Tasks;
using System.Windows.Input;

using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Interfaces;
using EnvDTE;


namespace BlackbirdSql.Common.Model;

public class AsyncCommand<TResult>(Func<object, Task<TResult>> command, Predicate<object> canExecute)
	: AbstractCommand(canExecute), IBAsyncCommand, IBOwnedCommand, ICommand
{
	public AsyncCommand(Func<object, Task<TResult>> command)
		: this(command, null)
	{
	}



	private readonly Func<object, Task<TResult>> _Command = command;

	public NotifyTaskCompletion<TResult> Execution { get; private set; }


	public override IBPropertyAgent Copy()
	{
		return new AsyncCommand<TResult>(_Command);
	}

	public override void Execute(object parameter)
	{
		_ = Task.Run(() => ExecuteAsync(parameter));
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
		if (Dispatcher.CheckAccess())
		{
			IsExecuting = false;
			RaiseCanExecuteChanged();
		}
	}
}
