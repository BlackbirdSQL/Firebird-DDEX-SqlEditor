// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions2.SqlEditorGeneralSettingsUserControl
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Config;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.EditorExtension.Config;


public class SqlEditorGeneralSettingsUserControl : UserControl
{
	private WrappingCheckBox _promptToSaveWhenClosingQueryWindowCheckBox;

	private TableLayoutPanel _tableLayoutPanelBottom;

	private Button _resetButton;

	private TableLayoutPanel _tableLayoutPanel1;

	public SqlEditorGeneralSettingsUserControl()
	{
		InitializeComponent();
		InitializeUIFromOptions(UserSettings.Instance.Current);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlackbirdSql.EditorExtension.Config.SqlEditorGeneralSettingsUserControl));
		this._resetButton = new System.Windows.Forms.Button();
		this._tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this._promptToSaveWhenClosingQueryWindowCheckBox = new BlackbirdSql.Common.Controls.WrappingCheckBox();
		this._tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
		this._tableLayoutPanel1.SuspendLayout();
		this._tableLayoutPanelBottom.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this._resetButton, "_resetButton");
		this._tableLayoutPanelBottom.SetColumnSpan(this._resetButton, 4);
		this._resetButton.MinimumSize = new System.Drawing.Size(75, 23);
		this._resetButton.Name = "_resetButton";
		this._resetButton.Click += new System.EventHandler(ResetButton_Click);
		resources.ApplyResources(this._tableLayoutPanel1, "_tableLayoutPanel1");
		this._tableLayoutPanel1.Controls.Add(this._promptToSaveWhenClosingQueryWindowCheckBox, 0, 3);
		this._tableLayoutPanel1.SetColumnSpan(this._promptToSaveWhenClosingQueryWindowCheckBox, 4);
		this._tableLayoutPanel1.Name = "_tableLayoutPanel1";
		resources.ApplyResources(this._promptToSaveWhenClosingQueryWindowCheckBox, "_promptToSaveWhenClosingQueryWindowCheckBox");
		this._promptToSaveWhenClosingQueryWindowCheckBox.Name = "_promptToSaveWhenClosingQueryWindowCheckBox";
		resources.ApplyResources(this._tableLayoutPanelBottom, "_tableLayoutPanelBottom");
		this._tableLayoutPanelBottom.Controls.Add(this._resetButton, 0, 0);
		this._tableLayoutPanelBottom.Name = "_tableLayoutPanelBottom";
		resources.ApplyResources(this, "$this");
		base.Controls.Add(this._tableLayoutPanel1);
		base.Controls.Add(this._tableLayoutPanelBottom);
		base.Name = "SqlEditorGeneralSettingsDlg";
		this._tableLayoutPanel1.ResumeLayout(false);
		this._tableLayoutPanel1.PerformLayout();
		this._tableLayoutPanelBottom.ResumeLayout(false);
		this._tableLayoutPanelBottom.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public void InitializeUIFromOptions(IUserSettings options)
	{
		_promptToSaveWhenClosingQueryWindowCheckBox.Checked = options.General.PromptForSaveWhenClosingQueryWindows;
	}

	public void ApplyUIToOptions(IUserSettings options)
	{
		options.General.PromptForSaveWhenClosingQueryWindows = _promptToSaveWhenClosingQueryWindowCheckBox.Checked;
	}

	public void ResetSettings()
	{
		InitializeUIFromOptions(UserSettings.Instance.Default);
	}

	private void ResetButton_Click(object sender, EventArgs e)
	{
		ResetSettings();
	}
}
