﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.DisplaySqlResultsBasePanel

using System;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl;

using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Shell.Interop;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Controls.ResultsPane;

public abstract class AbstractResultsPanel : Panel
{
	public const string C_TName = "DispSQLResults";

	protected object _ObjServiceProvider;

	protected ServiceProvider _serviceProvider;

	protected IVsUIShell _vsUIShell;

	private MenuCommandsService _MenuService = null;

	private string _defaultResultsDirectory;

	protected MenuCommandsService MenuService => _MenuService ??= new MenuCommandsService();

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
		Tracer.Trace(GetType(), "DisplaySqlResultsBaseTabPage.DisplaySqlResultsBaseTabPage", "", null);
		_defaultResultsDirectory = defaultResultsDirectory;
	}

	protected override void Dispose(bool disposing)
	{
		if (_serviceProvider != null)
		{
			_serviceProvider = null;
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

	protected new object GetService(Type serviceType)
	{
		return _serviceProvider.GetService(serviceType);
	}

	public virtual void Initialize(object oleServiceProvider)
	{
		Tracer.Trace(GetType(), "DisplaySqlResultsBaseTabPage.Initialize", "", null);
		_ObjServiceProvider = oleServiceProvider;
		_serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)oleServiceProvider);
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
