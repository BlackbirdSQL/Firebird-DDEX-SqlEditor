#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Diagnostics;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;



namespace BlackbirdSql.Common.Model;


public class RelayCommand : AbstractCommand
{
	private readonly Action<object> m_execute;

	public RelayCommand(Action execute)
		: this(delegate
		{
			execute();
		}, null)
	{
	}

	public RelayCommand(Action<object> execute)
		: this(execute, null)
	{
	}

	public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		: base(canExecute)
	{
		m_execute = execute ?? throw new ArgumentNullException("execute");
	}

	public override void Execute(object parameter)
	{
		try
		{
			UiTracer.TraceSource.AssertTraceEvent(CanExecute(parameter), TraceEventType.Error, EnUiTraceId.UiInfra, "Cannot execute the commands");
			IsExecuting = true;
			m_execute(parameter);
		}
		finally
		{
			IsExecuting = false;
		}
	}
}
