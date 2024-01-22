#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Diagnostics;
using System.Windows.Input;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl;
using System.Diagnostics.CodeAnalysis;

namespace BlackbirdSql.Common.Model;

public abstract class AbstractCommand : AbstractDispatcherConnection, IBOwnedCommand, ICommand
{
	public const string C_KeyIsExecuting = "IsExecuting";

	protected static new DescriberDictionary _Describers = null;


	private readonly Predicate<object> _CanExecute;

	private EventHandler _CanExecuteChangedHandler;


	public override DescriberDictionary Describers
	{
		get
		{
			if (_Describers == null)
				CreateAndPopulatePropertySet();

			return _Describers;
		}
	}

	public bool IsExecuting
	{
		get { return (bool)GetProperty(C_KeyIsExecuting); }
		protected set { SetProperty(C_KeyIsExecuting, value); }
	}



	public event EventHandler CanExecuteChanged
	{
		add
		{
			VerifyAccess();
			_CanExecuteChangedHandler = (EventHandler)Delegate.Combine(_CanExecuteChangedHandler, value);
			CommandManager.RequerySuggested += value;
		}
		remove
		{
			_CanExecuteChangedHandler = (EventHandler)Delegate.Remove(_CanExecuteChangedHandler, value);
			CommandManager.RequerySuggested -= value;
		}
	}

	protected AbstractCommand(Predicate<object> canExecute, AbstractCommand rhs)
	: base(rhs)
	{
		_CanExecute = canExecute;
	}

	protected AbstractCommand(Predicate<object> canExecute) : this(canExecute, null)
	{
	}

	[DebuggerStepThrough]
	public virtual bool CanExecute(object parameter)
	{
		if (IsExecuting)
			return false;

		if (_CanExecute == null)
			return true;

		try
		{
			return _CanExecute(parameter);
		}
		catch (Exception exception)
		{
			UiTracer.TraceSource.AssertTraceException2(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, exception, "An unhandled exception leaked from a UI entry point. You need to add a try/catch block.", 60, "CommandBase.cs", "CanExecute");
			return false;
		}
	}


	protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		if (_Describers == null)
		{
			_Describers = [];
			AbstractDispatcherConnection.CreateAndPopulatePropertySet(_Describers);

			_Describers.Add(C_KeyIsExecuting, typeof(bool), false);
		}

		describers?.AddRange(_Describers);
	}



	public abstract void Execute(object parameter);

	public void RaiseCanExecuteChanged()
	{
		_CanExecuteChangedHandler?.Invoke(this, EventArgs.Empty);
	}
}
