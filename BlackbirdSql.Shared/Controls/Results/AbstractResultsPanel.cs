// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.DisplaySqlResultsBasePanel

using System;
using System.Windows.Forms;
using BlackbirdSql.Shared.Ctl.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql.Shared.Controls.Results;


public abstract class AbstractResultsPanel : Panel
{
	public const string C_TName = "DispSQLResults";

	protected object _ObjServiceProvider;

	protected ServiceProvider _ServiceProvider;

	protected IVsUIShell _vsUIShell;

	private MenuCommandsService _MenuService = null;

	private string _defaultResultsDirectory;

	protected MenuCommandsService MenuService => _MenuService ??= [];

	public string DefaultResultsDirectory
	{
		get
		{
			return _defaultResultsDirectory;
		}
		set
		{
			_defaultResultsDirectory = value;
		}
	}

	protected AbstractResultsPanel(string defaultResultsDirectory)
	{
		// Evs.Trace(GetType(), "DisplaySqlResultsBaseTabPage.DisplaySqlResultsBaseTabPage", "", null);
		_defaultResultsDirectory = defaultResultsDirectory;
	}

	protected override void Dispose(bool disposing)
	{
		if (_ServiceProvider != null)
		{
			_ServiceProvider = null;
		}

		if (_ObjServiceProvider != null)
		{
			_ObjServiceProvider = null;
		}

		if (disposing && _MenuService != null)
		{
			_MenuService.Dispose();
			_MenuService = null;
		}

		base.Dispose(disposing);
	}

	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

	protected new object GetService(Type serviceType)
	{
		return _ServiceProvider.GetService(serviceType);
	}

	public virtual void Initialize(object oleServiceProvider)
	{
		// Evs.Trace(GetType(), "DisplaySqlResultsBaseTabPage.Initialize", "", null);

		Diag.ThrowIfNotOnUIThread();

		_ObjServiceProvider = oleServiceProvider;
		_ServiceProvider = new ServiceProvider((IOleServiceProvider)oleServiceProvider);
		_vsUIShell = GetService(typeof(IVsUIShell)) as IVsUIShell;
	}

	public abstract void Clear();

	public virtual void ActivateControl()
	{
		if (Controls.Count > 0)
		{
			Controls[0].Focus();
		}
	}
}
