#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Ctl.Diagnostics;



namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public class SqlExecutionAnsiSettingsDlg : ToolsOptionsBaseControl
	{
		private static readonly string helpKeyword = "BlackbirdSql.Common.Controls.ToolsOptions.SqlExecutionAnsiSettingsDlg";

		private CheckBox _setDefaultsCheck;

		private CheckBox _setNullsCheck;

		private CheckBox _setNullDefaultCheck;

		private CheckBox _setPaddingCheck;

		private CheckBox _setWarningCheck;

		private CheckBox _setCursorCloseCheck;

		private CheckBox _setImplicitCheck;

		private CheckBox _setQuotedCheck;

		private Label _captionLabel;

		private TableLayoutPanel _tableLayoutPanel;

		private TableLayoutPanel tableLayoutPanelBottom;

		private Button _resetButton;

		public SqlExecutionAnsiSettingsDlg()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlExecutionAnsiSettingsDlg));
			_setWarningCheck = new CheckBox();
			_setCursorCloseCheck = new CheckBox();
			_setQuotedCheck = new CheckBox();
			_setDefaultsCheck = new CheckBox();
			_setImplicitCheck = new CheckBox();
			_setNullsCheck = new CheckBox();
			_setNullDefaultCheck = new CheckBox();
			_setPaddingCheck = new CheckBox();
			_captionLabel = new Label();
			_tableLayoutPanel = new TableLayoutPanel();
			tableLayoutPanelBottom = new TableLayoutPanel();
			_resetButton = new Button();
			_tableLayoutPanel.SuspendLayout();
			tableLayoutPanelBottom.SuspendLayout();
			SuspendLayout();
			resources.ApplyResources(_setWarningCheck, "_setWarningCheck");
			_setWarningCheck.Name = "_setWarningCheck";
			_setWarningCheck.Click += new EventHandler(UpdateDefaultsCheckBox);
			resources.ApplyResources(_setCursorCloseCheck, "_setCursorCloseCheck");
			_setCursorCloseCheck.Name = "_setCursorCloseCheck";
			_setCursorCloseCheck.Click += new EventHandler(UpdateDefaultsCheckBox);
			resources.ApplyResources(_setQuotedCheck, "_setQuotedCheck");
			_setQuotedCheck.Name = "_setQuotedCheck";
			_setQuotedCheck.Click += new EventHandler(UpdateDefaultsCheckBox);
			resources.ApplyResources(_setDefaultsCheck, "_setDefaultsCheck");
			_setDefaultsCheck.Name = "_setDefaultsCheck";
			_setDefaultsCheck.Click += new EventHandler(SetDefaultsCheck_Click);
			resources.ApplyResources(_setImplicitCheck, "_setImplicitCheck");
			_setImplicitCheck.Name = "_setImplicitCheck";
			_setImplicitCheck.Click += new EventHandler(UpdateDefaultsCheckBox);
			resources.ApplyResources(_setNullsCheck, "_setNullsCheck");
			_setNullsCheck.Name = "_setNullsCheck";
			_setNullsCheck.Click += new EventHandler(UpdateDefaultsCheckBox);
			resources.ApplyResources(_setNullDefaultCheck, "_setNullDefaultCheck");
			_setNullDefaultCheck.Name = "_setNullDefaultCheck";
			_setNullDefaultCheck.Click += new EventHandler(UpdateDefaultsCheckBox);
			resources.ApplyResources(_setPaddingCheck, "_setPaddingCheck");
			_setPaddingCheck.Name = "_setPaddingCheck";
			_setPaddingCheck.Click += new EventHandler(UpdateDefaultsCheckBox);
			resources.ApplyResources(_captionLabel, "_captionLabel");
			_tableLayoutPanel.SetColumnSpan(_captionLabel, 3);
			_captionLabel.Name = "_captionLabel";
			resources.ApplyResources(_tableLayoutPanel, "_tableLayoutPanel");
			_tableLayoutPanel.Controls.Add(_captionLabel, 0, 0);
			_tableLayoutPanel.Controls.Add(_setDefaultsCheck, 0, 1);
			_tableLayoutPanel.Controls.Add(_setQuotedCheck, 0, 2);
			_tableLayoutPanel.Controls.Add(_setPaddingCheck, 2, 2);
			_tableLayoutPanel.Controls.Add(_setNullDefaultCheck, 0, 3);
			_tableLayoutPanel.Controls.Add(_setWarningCheck, 2, 3);
			_tableLayoutPanel.Controls.Add(_setImplicitCheck, 0, 4);
			_tableLayoutPanel.Controls.Add(_setNullsCheck, 2, 4);
			_tableLayoutPanel.Controls.Add(_setCursorCloseCheck, 0, 5);
			_tableLayoutPanel.Name = "_tableLayoutPanel";
			resources.ApplyResources(tableLayoutPanelBottom, "tableLayoutPanelBottom");
			tableLayoutPanelBottom.Controls.Add(_resetButton, 0, 0);
			tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
			resources.ApplyResources(_resetButton, "_resetButton");
			_resetButton.MinimumSize = new System.Drawing.Size(75, 23);
			_resetButton.Name = "_resetButton";
			_resetButton.Click += new EventHandler(ResetButton_Click);
			resources.ApplyResources(this, "$this");
			Controls.Add(tableLayoutPanelBottom);
			Controls.Add(_tableLayoutPanel);
			Name = "SqlExecutionAnsiSettingsDlg";
			_tableLayoutPanel.ResumeLayout(false);
			_tableLayoutPanel.PerformLayout();
			tableLayoutPanelBottom.ResumeLayout(false);
			tableLayoutPanelBottom.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		public override string GetHelpKeyword()
		{
			return helpKeyword;
		}

		public override bool ValidateValuesInControls()
		{
			Tracer.Trace(GetType(), "SqlExecutionAnsiSettingsDlg.ValidateValuesInControls", "", null);
			if (TrackChanges && !SaveOrCompareCurrentValueOfControls(save: false))
			{
				Cmd.ShowMessageBoxEx(null, ControlsResources.WarnSqlAnsiQueryChanges, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				TrackChanges = false;
			}

			return true;
		}

		protected override void ApplySettingsToUI(IBUserSettings options)
		{
			if (options != null)
			{
				_setNullsCheck.Checked = options.Execution.SetAnsiNulls;
				_setNullDefaultCheck.Checked = options.Execution.SetAnsiNullDefault;
				_setPaddingCheck.Checked = options.Execution.SetAnsiPadding;
				_setWarningCheck.Checked = options.Execution.SetAnsiWarnings;
				_setCursorCloseCheck.Checked = options.Execution.SetCursorCloseOnCommit;
				_setImplicitCheck.Checked = options.Execution.SetImplicitTransaction;
				_setQuotedCheck.Checked = options.Execution.SetQuotedIdentifier;
				UpdateDefaultsCheckBox(this, new EventArgs());
				if (TrackChanges)
				{
					SaveOrCompareCurrentValueOfControls(save: true);
				}
			}
		}

		protected override void SaveSettingsFromUI(IBUserSettings options)
		{
			options.Execution.SetAnsiNulls = _setNullsCheck.Checked;
			options.Execution.SetAnsiNullDefault = _setNullDefaultCheck.Checked;
			options.Execution.SetAnsiPadding = _setPaddingCheck.Checked;
			options.Execution.SetAnsiWarnings = _setWarningCheck.Checked;
			options.Execution.SetCursorCloseOnCommit = _setCursorCloseCheck.Checked;
			options.Execution.SetImplicitTransaction = _setImplicitCheck.Checked;
			options.Execution.SetQuotedIdentifier = _setQuotedCheck.Checked;
		}

		private void UpdateDefaultsCheckBox(object sender, EventArgs a)
		{
			Tracer.Trace(GetType(), "SqlExecutionAnsiSettingsDlg.UpdateDefaultsCheckBox", "", null);
			int num = 0;

			foreach (Control control in Controls)
			{
				if (control is CheckBox checkBox)
				{
					if (checkBox != _setDefaultsCheck && checkBox.Checked)
					{
						num++;
					}
				}
			}

			if (num == 7)
			{
				_setDefaultsCheck.CheckState = CheckState.Checked;
			}
			else if (num > 0)
			{
				_setDefaultsCheck.CheckState = CheckState.Indeterminate;
			}
			else
			{
				_setDefaultsCheck.CheckState = CheckState.Unchecked;
			}
		}

		private void SetDefaultsCheck_Click(object sender, EventArgs e)
		{
			foreach (Control control in Controls)
			{
				if (control is CheckBox checkBox)
				{
					if (checkBox != _setDefaultsCheck)
					{
						checkBox.Checked = _setDefaultsCheck.CheckState == CheckState.Checked;
					}
				}
			}
		}

		private void ResetButton_Click(object sender, EventArgs e)
		{
			ResetSettings();
		}
	}
}
