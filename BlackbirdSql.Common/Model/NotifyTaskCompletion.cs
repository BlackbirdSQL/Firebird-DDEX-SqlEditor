// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.Commands.NotifyTaskCompletion<TResult>

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Common.Model;

public sealed class NotifyTaskCompletion<TResult> : AbstractDispatcherConnection
{
	public Task TaskCompletion { get; private set; }

	public Task<TResult> Task { get; private set; }

	public TResult Result
	{
		get
		{
			if (Task.Status != TaskStatus.RanToCompletion)
			{
				return default;
			}
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
			return Task.Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
		}
	}

	public TaskStatus Status => Task.Status;

	public bool IsCompleted => Task.IsCompleted;

	public bool IsNotCompleted => !Task.IsCompleted;

	public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

	public bool IsCanceled => Task.IsCanceled;

	public bool IsFaulted => Task.IsFaulted;

	public AggregateException Exception => Task.Exception;

	public Exception InnerException
	{
		get
		{
			if (Exception != null)
			{
				return Exception.InnerException;
			}
			return null;
		}
	}

	public string ErrorMessage
	{
		get
		{
			if (InnerException != null)
			{
				return InnerException.Message;
			}
			return null;
		}
	}



	public NotifyTaskCompletion(Task<TResult> task, NotifyTaskCompletion<TResult> rhs = null, bool generateNewId = true)
		: base(rhs, generateNewId)
	{
		Task = task;
		if (!task.IsCompleted)
		{
			if (CheckAccess())
			{
				RaisePropertyChanged("IsCompleted");
				RaisePropertyChanged("IsNotCompleted");
			}
			TaskCompletion = WatchTaskAsync(task);
		}
	}



	public override IBPropertyAgent Copy()
	{
		return new NotifyTaskCompletion<TResult>(Task, this, true);
	}



	private async Task WatchTaskAsync(Task task)
	{
		try
		{
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
			await task;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
		}
		catch (Exception ex)
		{
			UiTracer.TraceSource.DebugTraceException(TraceEventType.Error, 12, ex, ex.Message);
		}
		if (CheckAccess())
		{
			RaisePropertyChanged("Status");
			RaisePropertyChanged("IsCompleted");
			RaisePropertyChanged("IsNotCompleted");
			if (task.IsCanceled)
			{
				RaisePropertyChanged("IsCanceled");
			}
			else if (task.IsFaulted)
			{
				RaisePropertyChanged("IsFaulted");
				RaisePropertyChanged("Exception");
				RaisePropertyChanged("InnerException");
				RaisePropertyChanged("ErrorMessage");
			}
			else
			{
				RaisePropertyChanged("IsSuccessfullyCompleted");
				RaisePropertyChanged("Result");
			}
		}
	}
}
