// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.ShowPlanPanel

using System;
using System.Windows.Forms;
using BlackbirdSql.Shared.Ctl.Commands;
// using Microsoft.AnalysisServices.Graphing;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Controls.Results;


internal class ExecutionPlanPanel : AbstractResultsPanel, IOleCommandTarget
{
	private ExecutionPlanControl _ExecutionPlanCtl;

	private MenuCommandsService _MenuService = [];

	internal ExecutionPlanControl ExecutionPlanCtl => _ExecutionPlanCtl;

	public ExecutionPlanPanel(string defaultResultsDirectory)
		: base(defaultResultsDirectory)
	{
		AutoScroll = true;
	}

	internal override void Initialize(object rawServiceProvider)
	{
		base.Initialize(rawServiceProvider);
		_ExecutionPlanCtl = new()
		{
			Name = "ExecutionPlanControl",
			Dock = DockStyle.Fill
		};
		((Interfaces.IBsObjectWithSite)_ExecutionPlanCtl).SetSite(_ServiceProvider);
		Controls.Add(_ExecutionPlanCtl);
	}

	internal override void Clear()
	{
	}

	/*
	internal void AddGraphs(IGraph[] graphs, object dataSource)
	{
		try
		{
			_ExecutionPlanCtl.AddGraphs(graphs, dataSource);
		}
		catch (Exception e)
		{
			Diag.ThrowException(e);
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

	int IOleCommandTarget.QueryStatus(ref Guid guidGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		Diag.ThrowIfNotOnUIThread();

		return ((IOleCommandTarget)ExecutionPlanCtl).QueryStatus(ref guidGroup, cCmds, prgCmds, pCmdText);
	}

	int IOleCommandTarget.Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr vIn, IntPtr vOut)
	{
		Diag.ThrowIfNotOnUIThread();

		return ((IOleCommandTarget)ExecutionPlanCtl).Exec(ref guidGroup, nCmdId, nCmdExcept, vIn, vOut);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == Native.WM_CONTEXTMENU)
		{
			if (VS.GetCoordinatesForPopupMenuFromWM_Context(ref m, out int xPos, out int yPos, this))
				VS.ShowContextMenuEvent(264, xPos, yPos, this);
		}
		else
		{
			base.WndProc(ref m);
		}
	}
}
