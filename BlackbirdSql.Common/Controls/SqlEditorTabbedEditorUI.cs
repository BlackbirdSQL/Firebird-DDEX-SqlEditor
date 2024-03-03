#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Controls.QueryExecution;
using BlackbirdSql.Common.Properties;


namespace BlackbirdSql.Common.Controls
{
	public class SqlEditorTabbedEditorUI(AbstractTabbedEditorPane tabbedEditorPane, Guid toolbarGuid, uint toolbarId)
		: TabbedEditorUI(tabbedEditorPane, toolbarGuid, toolbarId)
	{
		private StatusStrip _StatusBar;

		public QEStatusBarManager StatusBarManager { get; private set; }

		public StatusStrip StatusBar => _StatusBar;

		protected override void AddCustomControlsToPanel(Panel panel)
		{
			StatusStrip statusStrip = CreateStatusBar();
			if (statusStrip != null)
			{
				panel.SuspendLayout();
				this.SuspendLayout();
				statusStrip.Dock = DockStyle.Bottom;
				statusStrip.TabStop = false;
				panel.Controls.Add(statusStrip);
				this.ResumeLayout(performLayout: false);
				panel.ResumeLayout(performLayout: false);
				statusStrip.Hide();
				this.PerformLayout();
			}
		}

		private StatusStrip CreateStatusBar()
		{
			StatusBarManager = new QEStatusBarManager();
			_StatusBar = new StatusStrip
			{
				Dock = DockStyle.Top,
				SizingGrip = false,
				ShowItemToolTips = true,
				AccessibleName = ControlsResources.StatusBarAccessibleName
			};
			_StatusBar.Height += 4;
			AbstractTabbedEditorPane editor = TabbedEditorPane;
			StatusBarManager.Initialize(_StatusBar, rowCountValid: true, (IBSqlEditorWindowPane)editor);
			return _StatusBar;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && StatusBarManager != null)
			{
				StatusBarManager.Dispose();
			}
		}
	}
}
