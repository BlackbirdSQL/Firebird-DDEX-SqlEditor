// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.ShowPlanPanel
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;

// using Microsoft.AnalysisServices.Graphing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace BlackbirdSql.Common.Controls.ResultsPane;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "UIThread compliance is performed by the class.")]

public class ExecutionPlanPanel : AbstractResultsPanel, IOleCommandTarget
{
	private ExecutionPlanControl _ExecutionPlanCtl;

	private MenuCommandsService _MenuService = [];

	public ExecutionPlanControl ExecutionPlanCtl => _ExecutionPlanCtl;

	public ExecutionPlanPanel(string defaultResultsDirectory)
		: base(defaultResultsDirectory)
	{
		AutoScroll = true;
	}

	public override void Initialize(object rawServiceProvider)
	{
		base.Initialize(rawServiceProvider);
		_ExecutionPlanCtl = new()
		{
			Name = "ExecutionPlanControl",
			Dock = DockStyle.Fill
		};
		((BlackbirdSql.Common.Controls.Interfaces.IBObjectWithSite)_ExecutionPlanCtl).SetSite((System.IServiceProvider)_ServiceProvider);
		base.Controls.Add(_ExecutionPlanCtl);
	}

	public override void Clear()
	{
	}

	/*
	public void AddGraphs(IGraph[] graphs, object dataSource)
	{
		try
		{
			_ExecutionPlanCtl.AddGraphs(graphs, dataSource);
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
			throw;
		}
	}
	*/

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_ExecutionPlanCtl != null)
			{
				_ExecutionPlanCtl.Dispose();
				_ExecutionPlanCtl = null;
			}
			if (_MenuService != null)
			{
				_MenuService.Dispose();
				_MenuService = null;
			}
		}
		base.Dispose(disposing);
	}

	int IOleCommandTarget.QueryStatus(ref Guid guidGroup, uint cmdID, OLECMD[] oleCmd, IntPtr oleText)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		return ((IOleCommandTarget)ExecutionPlanCtl).QueryStatus(ref guidGroup, cmdID, oleCmd, oleText);
	}

	int IOleCommandTarget.Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr vIn, IntPtr vOut)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		return ((IOleCommandTarget)ExecutionPlanCtl).Exec(ref guidGroup, nCmdId, nCmdExcept, vIn, vOut);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == Native.WM_CONTEXTMENU)
		{
			if (CommonUtils.GetCoordinatesForPopupMenuFromWM_Context(ref m, out var xPos, out var yPos, this))
			{
				CommonUtils.ShowContextMenuEvent(264, xPos, yPos, this);
			}
		}
		else
		{
			base.WndProc(ref m);
		}
	}
}
