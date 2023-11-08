// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.ResultWindowPane
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.ResultsPane;
using BlackbirdSql.Core;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.Common.Controls;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

public class ResultWindowPane : WindowPane, IOleCommandTarget
{
	private readonly Panel _WindowPanel = new Panel();

	private IOleCommandTarget _CmdTarget;

	public override IWin32Window Window => _WindowPanel;

	public IOleCommandTarget CommandTarget
	{
		get
		{
			return _CmdTarget;
		}
		set
		{
			_CmdTarget = value;
		}
	}

	private AbstractResultsPanel TargetResultsPanel { get; set; }

	public event EventHandler<ResultControlEventArgs> PanelAddedEvent;

	public event EventHandler<ResultControlEventArgs> PanelRemovedEvent;

	public ResultWindowPane()
	{
		_WindowPanel.Name = "ResultsWindowPaneWindowPanel";
		_WindowPanel.Dock = DockStyle.Fill;
		_WindowPanel.Location = new Point(0, 0);
	}

	public int QueryStatus(ref Guid guidGroup, uint cmdId, OLECMD[] oleCmd, IntPtr oleText)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (_CmdTarget != null)
			return _CmdTarget.QueryStatus(ref guidGroup, cmdId, oleCmd, oleText);

		return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
	}

	public int Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr variantIn, IntPtr variantOut)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (_CmdTarget != null)
			return _CmdTarget.Exec(ref guidGroup, nCmdId, nCmdExcept, variantIn, variantOut);

		return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
	}

	public void Clear()
	{
		if (_WindowPanel != null)
		{
			_WindowPanel.Controls.Clear();
			RaisePanelRemovedEvent();
			CommandTarget = null;
			TargetResultsPanel = null;
		}
	}

	public void Add(AbstractResultsPanel resultsTabPanel)
	{
		if (_WindowPanel != null)
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			_WindowPanel.Controls.Add(resultsTabPanel);
			if (resultsTabPanel is IOleCommandTarget)
			{
				CommandTarget = resultsTabPanel as IOleCommandTarget;
			}

			TargetResultsPanel = resultsTabPanel;
			RaisePanelAddedEvent();
		}
	}

	public bool Contains(AbstractResultsPanel resultsTabPanel)
	{
		if (_WindowPanel != null)
		{
			return _WindowPanel.Controls.Contains(resultsTabPanel);
		}

		return false;
	}

	public void Remove(AbstractResultsPanel resultsTabPanel)
	{
		if (_WindowPanel != null)
		{
			_WindowPanel.Controls.Remove(resultsTabPanel);
			CommandTarget = null;
			RaisePanelRemovedEvent();
			TargetResultsPanel = null;
		}
	}

	public void SetFocus()
	{
		if (_CmdTarget != null)
		{
			(_CmdTarget as AbstractResultsPanel).ActivateControl();
		}
	}

	private void RaisePanelAddedEvent()
	{
		PanelAddedEvent?.Invoke(this, new ResultControlEventArgs(TargetResultsPanel));
	}

	private void RaisePanelRemovedEvent()
	{
		PanelRemovedEvent?.Invoke(this, new ResultControlEventArgs(TargetResultsPanel));
	}
}
