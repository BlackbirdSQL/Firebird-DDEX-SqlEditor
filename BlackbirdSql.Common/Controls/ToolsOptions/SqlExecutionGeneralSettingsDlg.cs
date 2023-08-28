#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Globalization;
using System.Windows.Forms;

using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Diagnostics;



namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public class SqlExecutionGeneralSettingsDlg : ToolsOptionsBaseControl
	{
		private static readonly string helpKeyword = "BlackbirdSql.Common.Controls.ToolsOptions.SqlExecutionGeneralSettingsDlg";

		private Label _numRowsExplainLabel;

		private Label _textSizeExplainLabel;

		private Label _execTimeoutExplainLabel;

		private Label _batchSepExplainLabel;

		private Label _rowCountLabel;

		private NumericUpDown _rowCountNumericUpDown;

		private Label _textsizeLabel;

		private NumericUpDown _textsizeNumericUpDown;

		private Label _execTimeoutLabel;

		private Label _bytesLabel;

		private Label _secondsLabel;

		private NumericUpDown _execTimeoutNumericUpDown;

		private WrappingCheckBox _enableOLESQLCheckBox;

		private TableLayoutPanel tableLayoutPanelBottom;

		private Button _ResetButton;

		private TableLayoutPanel tableLayoutPanel1;

		public SqlExecutionGeneralSettingsDlg()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		public void SetSqlCmdControlMode(bool showCheckBox)
		{
			((Control)(object)_enableOLESQLCheckBox).Visible = showCheckBox;
		}

		private void InitializeComponent()
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Expected O, but got Unknown
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlExecutionGeneralSettingsDlg));
			_numRowsExplainLabel = new Label();
			_textSizeExplainLabel = new Label();
			_execTimeoutExplainLabel = new Label();
			_batchSepExplainLabel = new Label();
			_ResetButton = new Button();
			tableLayoutPanel1 = new TableLayoutPanel();
			_rowCountLabel = new Label();
			_rowCountNumericUpDown = new NumericUpDown();
			_textsizeLabel = new Label();
			_textsizeNumericUpDown = new NumericUpDown();
			_bytesLabel = new Label();
			_execTimeoutLabel = new Label();
			_execTimeoutNumericUpDown = new NumericUpDown();
			_secondsLabel = new Label();
			_enableOLESQLCheckBox = new WrappingCheckBox();
			tableLayoutPanelBottom = new TableLayoutPanel();
			tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)_rowCountNumericUpDown).BeginInit();
			((System.ComponentModel.ISupportInitialize)_textsizeNumericUpDown).BeginInit();
			((System.ComponentModel.ISupportInitialize)_execTimeoutNumericUpDown).BeginInit();
			tableLayoutPanelBottom.SuspendLayout();
			SuspendLayout();
			resources.ApplyResources(_numRowsExplainLabel, "_numRowsExplainLabel");
			tableLayoutPanel1.SetColumnSpan(_numRowsExplainLabel, 4);
			_numRowsExplainLabel.Name = "_numRowsExplainLabel";
			resources.ApplyResources(_textSizeExplainLabel, "_textSizeExplainLabel");
			tableLayoutPanel1.SetColumnSpan(_textSizeExplainLabel, 4);
			_textSizeExplainLabel.Name = "_textSizeExplainLabel";
			resources.ApplyResources(_execTimeoutExplainLabel, "_execTimeoutExplainLabel");
			tableLayoutPanel1.SetColumnSpan(_execTimeoutExplainLabel, 4);
			_execTimeoutExplainLabel.Name = "_execTimeoutExplainLabel";
			resources.ApplyResources(_batchSepExplainLabel, "_batchSepExplainLabel");
			tableLayoutPanel1.SetColumnSpan(_batchSepExplainLabel, 4);
			_batchSepExplainLabel.Name = "_batchSepExplainLabel";
			resources.ApplyResources(_ResetButton, "_ResetButton");
			tableLayoutPanelBottom.SetColumnSpan(_ResetButton, 4);
			_ResetButton.MinimumSize = new System.Drawing.Size(75, 23);
			_ResetButton.Name = "_ResetButton";
			_ResetButton.Click += new EventHandler(ResetButton_Click);
			resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
			tableLayoutPanel1.Controls.Add(_numRowsExplainLabel, 0, 0);
			tableLayoutPanel1.Controls.Add(_rowCountLabel, 0, 1);
			tableLayoutPanel1.Controls.Add(_rowCountNumericUpDown, 1, 1);
			tableLayoutPanel1.Controls.Add(_textSizeExplainLabel, 0, 2);
			tableLayoutPanel1.Controls.Add(_textsizeLabel, 0, 3);
			tableLayoutPanel1.Controls.Add(_textsizeNumericUpDown, 1, 3);
			tableLayoutPanel1.Controls.Add(_bytesLabel, 2, 3);
			tableLayoutPanel1.Controls.Add(_execTimeoutExplainLabel, 0, 4);
			tableLayoutPanel1.Controls.Add(_execTimeoutLabel, 0, 5);
			tableLayoutPanel1.Controls.Add(_execTimeoutNumericUpDown, 1, 5);
			tableLayoutPanel1.Controls.Add(_secondsLabel, 2, 5);
			tableLayoutPanel1.Controls.Add(_batchSepExplainLabel, 0, 6);
			tableLayoutPanel1.Controls.Add((Control)(object)_enableOLESQLCheckBox, 0, 8);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			resources.ApplyResources(_rowCountLabel, "_rowCountLabel");
			_rowCountLabel.Name = "_rowCountLabel";
			resources.ApplyResources(_rowCountNumericUpDown, "_rowCountNumericUpDown");
			_rowCountNumericUpDown.Maximum = new decimal(new int[4] { 999999999, 0, 0, 0 });
			_rowCountNumericUpDown.MinimumSize = new System.Drawing.Size(100, 0);
			_rowCountNumericUpDown.Name = "_rowCountNumericUpDown";
			resources.ApplyResources(_textsizeLabel, "_textsizeLabel");
			_textsizeLabel.Name = "_textsizeLabel";
			resources.ApplyResources(_textsizeNumericUpDown, "_textsizeNumericUpDown");
			_textsizeNumericUpDown.Maximum = new decimal(new int[4] { int.MaxValue, 0, 0, 0 });
			_textsizeNumericUpDown.MinimumSize = new System.Drawing.Size(100, 0);
			_textsizeNumericUpDown.Name = "_textsizeNumericUpDown";
			resources.ApplyResources(_bytesLabel, "_bytesLabel");
			tableLayoutPanel1.SetColumnSpan(_bytesLabel, 2);
			_bytesLabel.Name = "_bytesLabel";
			resources.ApplyResources(_execTimeoutLabel, "_execTimeoutLabel");
			_execTimeoutLabel.Name = "_execTimeoutLabel";
			resources.ApplyResources(_execTimeoutNumericUpDown, "_execTimeoutNumericUpDown");
			_execTimeoutNumericUpDown.Maximum = new decimal(new int[4] { 32000, 0, 0, 0 });
			_execTimeoutNumericUpDown.MinimumSize = new System.Drawing.Size(100, 0);
			_execTimeoutNumericUpDown.Name = "_execTimeoutNumericUpDown";
			resources.ApplyResources(_secondsLabel, "_secondsLabel");
			tableLayoutPanel1.SetColumnSpan(_secondsLabel, 2);
			_secondsLabel.Name = "_secondsLabel";
			resources.ApplyResources(_enableOLESQLCheckBox, "_enableOLESQLCheckBox");
			tableLayoutPanel1.SetColumnSpan((Control)(object)_enableOLESQLCheckBox, 4);
			((Control)(object)_enableOLESQLCheckBox).Name = "_enableOLESQLCheckBox";
			resources.ApplyResources(tableLayoutPanelBottom, "tableLayoutPanelBottom");
			tableLayoutPanelBottom.Controls.Add(_ResetButton, 0, 0);
			tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
			resources.ApplyResources(this, "$this");
			Controls.Add(tableLayoutPanelBottom);
			Controls.Add(tableLayoutPanel1);
			Name = "SqlExecutionGeneralSettingsDlg";
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)_rowCountNumericUpDown).EndInit();
			((System.ComponentModel.ISupportInitialize)_textsizeNumericUpDown).EndInit();
			((System.ComponentModel.ISupportInitialize)_execTimeoutNumericUpDown).EndInit();
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
			Tracer.Trace(GetType(), "SqlExecutionGeneralSettingsDlg.ValidateValuesInControls", "", null);
			if (!ValidateNumeric(_rowCountNumericUpDown, string.Format(CultureInfo.CurrentCulture, SharedResx.ErrInvalidSetRowcount, _rowCountNumericUpDown.Minimum, _rowCountNumericUpDown.Maximum)))
			{
				return false;
			}

			if (!ValidateNumeric(_textsizeNumericUpDown, string.Format(CultureInfo.CurrentCulture, SharedResx.ErrInvalidSetTextsize, _textsizeNumericUpDown.Minimum, _textsizeNumericUpDown.Maximum)))
			{
				return false;
			}

			if (!ValidateNumeric(_execTimeoutNumericUpDown, string.Format(CultureInfo.CurrentCulture, SharedResx.ErrInvalidExecutionTimeout, _execTimeoutNumericUpDown.Minimum, _execTimeoutNumericUpDown.Maximum)))
			{
				return false;
			}

			if (TrackChanges && !SaveOrCompareCurrentValueOfControls(save: false))
			{
				Cmd.ShowMessageBoxEx(null, SharedResx.WarnSqlGeneralQueryChanges, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				TrackChanges = false;
			}

			return true;
		}

		protected override void ApplySettingsToUI(IUserSettings options)
		{
			if (options != null)
			{
				_rowCountNumericUpDown.Text = options.Execution.SetRowCount.ToString(CultureInfo.CurrentCulture);
				_textsizeNumericUpDown.Text = options.Execution.SetTextSize.ToString(CultureInfo.CurrentCulture);
				_execTimeoutNumericUpDown.Text = options.Execution.ExecutionTimeout.ToString(CultureInfo.CurrentCulture);
				((CheckBox)(object)_enableOLESQLCheckBox).Checked = options.Execution.OLESQLScriptingByDefault;
				if (TrackChanges)
				{
					SaveOrCompareCurrentValueOfControls(save: true);
				}
			}
		}

		protected override void SaveSettingsFromUI(IUserSettings options)
		{
			options.Execution.SetRowCount = (int)_rowCountNumericUpDown.Value;
			options.Execution.SetTextSize = (int)_textsizeNumericUpDown.Value;
			options.Execution.ExecutionTimeout = (int)_execTimeoutNumericUpDown.Value;
			options.Execution.OLESQLScriptingByDefault = ((CheckBox)(object)_enableOLESQLCheckBox).Checked;
		}

		private void ResetButton_Click(object sender, EventArgs e)
		{
			ResetSettings();
		}
	}
}
