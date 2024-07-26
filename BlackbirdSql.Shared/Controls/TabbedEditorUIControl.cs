// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorTabbedEditorUI

using System;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Widgets;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Controls;


public class TabbedEditorUIControl : AbstractTabbedEditorUIControl
{

	public TabbedEditorUIControl(AbstractTabbedEditorWindowPane tabbedEditorPane, Guid toolbarGuid, uint toolbarId)
		: base(tabbedEditorPane, toolbarGuid, toolbarId)
	{

	}



	private StatusStrip _StatusBar;
	private StatusBarManager _StatusBarMgr;



	public StatusStrip StatusBar => _StatusBar;


	protected override void AddCustomControlsToPanel(Panel panel)
	{
		StatusStrip statusStrip = CreateStatusBar();
		if (statusStrip != null)
		{
			panel.SuspendLayout();
			this.SuspendLayout();

			try
			{
				statusStrip.Dock = DockStyle.Bottom;
				statusStrip.TabStop = false;
				panel.Controls.Add(statusStrip);
			}
			finally
			{
				this.ResumeLayout(performLayout: false);
				panel.ResumeLayout(performLayout: false);
			}

			statusStrip.Hide();
			this.PerformLayout();
		}
	}

	private StatusStrip CreateStatusBar()
	{
		_StatusBarMgr = new StatusBarManager();

		_StatusBar = new StatusStrip
		{
			Dock = DockStyle.Top,
			SizingGrip = false,
			ShowItemToolTips = true,
			AccessibleName = ControlsResources.StatusBar_AccessibleName
		};

		_StatusBar.Height += 4;

		_StatusBarMgr.Initialize(_StatusBar, rowCountValid: true, (IBsTabbedEditorWindowPane)TabbedEditorPane);

		return _StatusBar;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (disposing && _StatusBarMgr != null)
		{
			_StatusBarMgr.Dispose();
		}
	}
}
