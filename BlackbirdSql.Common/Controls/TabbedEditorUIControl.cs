﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorTabbedEditorUI

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Controls.Widgets;
using BlackbirdSql.Common.Properties;



namespace BlackbirdSql.Common.Controls;

[SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Readability")]


public class TabbedEditorUIControl : AbstractTabbedEditorUIControl
{

	public TabbedEditorUIControl(AbstractTabbedEditorWindowPane tabbedEditorPane, Guid toolbarGuid, uint toolbarId)
		: base(tabbedEditorPane, toolbarGuid, toolbarId)
	{

	}



	private StatusStrip _StatusBar;
	private QEStatusBarManager _StatusBarManager;



	public StatusStrip StatusBar => _StatusBar;


	public QEStatusBarManager StatusBarManager => _StatusBarManager;


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
		_StatusBarManager = new QEStatusBarManager();

		_StatusBar = new StatusStrip
		{
			Dock = DockStyle.Top,
			SizingGrip = false,
			ShowItemToolTips = true,
			AccessibleName = ControlsResources.StatusBarAccessibleName
		};

		_StatusBar.Height += 4;

		_StatusBarManager.Initialize(_StatusBar, rowCountValid: true, (IBSqlEditorWindowPane)TabbedEditorPane);

		return _StatusBar;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (disposing && _StatusBarManager != null)
		{
			_StatusBarManager.Dispose();
		}
	}
}
