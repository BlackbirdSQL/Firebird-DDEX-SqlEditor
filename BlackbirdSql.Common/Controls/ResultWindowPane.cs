#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.ResultsPane;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor
namespace BlackbirdSql.Common.Controls;


public class ResultWindowPane : WindowPane, IOleCommandTarget
{
	private readonly Panel _windowPanel = new Panel();

	private IOleCommandTarget _CmdTarget;

	public override IWin32Window Window => _windowPanel;

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

	public event EventHandler<ResultControlEventArgs> PanelAdded;

	public event EventHandler<ResultControlEventArgs> PanelRemoved;

	public ResultWindowPane()
	{
		_windowPanel.Name = "ResultsWindowPaneWindowPanel";
		_windowPanel.Dock = DockStyle.Fill;
		_windowPanel.Location = new Point(0, 0);
	}

	public int QueryStatus(ref Guid guidGroup, uint cmdId, OLECMD[] oleCmd, IntPtr oleText)
	{
		if (_CmdTarget != null)
			return _CmdTarget.QueryStatus(ref guidGroup, cmdId, oleCmd, oleText);

		return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
	}

	public int Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr variantIn, IntPtr variantOut)
	{
		if (_CmdTarget != null)
		{
			return _CmdTarget.Exec(ref guidGroup, nCmdId, nCmdExcept, variantIn, variantOut);
		}

		return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
	}

	public void Clear()
	{
		if (_windowPanel != null)
		{
			_windowPanel.Controls.Clear();
			RaisePanelRemovedEvent();
			CommandTarget = null;
			TargetResultsPanel = null;
		}
	}

	public void Add(AbstractResultsPanel resultsTabPanel)
	{
		if (_windowPanel != null)
		{
			_windowPanel.Controls.Add(resultsTabPanel);
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
		if (_windowPanel != null)
		{
			return _windowPanel.Controls.Contains(resultsTabPanel);
		}

		return false;
	}

	public void Remove(AbstractResultsPanel resultsTabPanel)
	{
		if (_windowPanel != null)
		{
			_windowPanel.Controls.Remove(resultsTabPanel);
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
		PanelAdded?.Invoke(this, new ResultControlEventArgs(TargetResultsPanel));
	}

	private void RaisePanelRemovedEvent()
	{
		PanelRemoved?.Invoke(this, new ResultControlEventArgs(TargetResultsPanel));
	}
}
