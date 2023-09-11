#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Globalization;
using System.Windows.Forms;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public class SqlResultToTextSettingsDlg : ToolsOptionsBaseControl
	{
		private static readonly string helpKeyword = "BlackbirdSql.Common.Controls.ToolsOptions.SqlResultToTextSettingsDlg";

		private TextBox _delimiterEdit;

		private Label _delimiterLabel;

		private ComboBox _outputFormatCombo;

		private Label _outputFormatLabel;

		private CheckBox _rightAlignCheck;

		private CheckBox _printColHeadersCheck;

		private static readonly char[] s_delimChars = new char[4] { '\0', ',', '\t', ' ' };

#pragma warning disable CS0649 // Field 'SqlResultToTextSettingsDlg.m_columnAlignedItemIndex' is never assigned to, and will always have its default value 0
		private readonly int m_columnAlignedItemIndex;
#pragma warning restore CS0649 // Field 'SqlResultToTextSettingsDlg.m_columnAlignedItemIndex' is never assigned to, and will always have its default value 0

		private readonly int m_customDelimitedItemIndex = 4;

		private CheckBox _includeQueryCheckBox;

		private CheckBox _discardResultsCheckBox;

		private Label _maxNumOfCharsLabel;

		private NumericUpDown _maxNumOfCharsUpDown;

		private CheckBox _resInSepTabCheckBox;

		private CheckBox _scrollResutsCheckBox;

		private CheckBox _switchToResultsTabCheckBox;

		private TableLayoutPanel _tableLayoutPanel;

		private TableLayoutPanel tableLayoutPanel1;

		private Button _resetButton;

		public SqlResultToTextSettingsDlg()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlResultToTextSettingsDlg));
			tableLayoutPanel1 = new TableLayoutPanel();
			_resetButton = new Button();
			_tableLayoutPanel = new TableLayoutPanel();
			_outputFormatLabel = new Label();
			_outputFormatCombo = new ComboBox();
			_delimiterEdit = new TextBox();
			_delimiterLabel = new Label();
			_printColHeadersCheck = new CheckBox();
			_includeQueryCheckBox = new CheckBox();
			_scrollResutsCheckBox = new CheckBox();
			_rightAlignCheck = new CheckBox();
			_discardResultsCheckBox = new CheckBox();
			_resInSepTabCheckBox = new CheckBox();
			_switchToResultsTabCheckBox = new CheckBox();
			_maxNumOfCharsLabel = new Label();
			_maxNumOfCharsUpDown = new NumericUpDown();
			tableLayoutPanel1.SuspendLayout();
			_tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)_maxNumOfCharsUpDown).BeginInit();
			SuspendLayout();
			resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
			tableLayoutPanel1.Controls.Add(_resetButton, 0, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			resources.ApplyResources(_resetButton, "_resetButton");
			tableLayoutPanel1.SetColumnSpan(_resetButton, 4);
			_resetButton.MinimumSize = new System.Drawing.Size(75, 23);
			_resetButton.Name = "_resetButton";
			_resetButton.Click += new EventHandler(ResetButton_Click);
			resources.ApplyResources(_tableLayoutPanel, "_tableLayoutPanel");
			_tableLayoutPanel.Controls.Add(_outputFormatLabel, 0, 0);
			_tableLayoutPanel.Controls.Add(_outputFormatCombo, 3, 0);
			_tableLayoutPanel.Controls.Add(_delimiterEdit, 3, 1);
			_tableLayoutPanel.Controls.Add(_delimiterLabel, 0, 1);
			_tableLayoutPanel.Controls.Add(_printColHeadersCheck, 0, 2);
			_tableLayoutPanel.Controls.Add(_includeQueryCheckBox, 0, 3);
			_tableLayoutPanel.Controls.Add(_scrollResutsCheckBox, 0, 4);
			_tableLayoutPanel.Controls.Add(_rightAlignCheck, 0, 5);
			_tableLayoutPanel.Controls.Add(_discardResultsCheckBox, 0, 6);
			_tableLayoutPanel.Controls.Add(_resInSepTabCheckBox, 0, 7);
			_tableLayoutPanel.Controls.Add(_switchToResultsTabCheckBox, 0, 8);
			_tableLayoutPanel.Controls.Add(_maxNumOfCharsLabel, 0, 9);
			_tableLayoutPanel.Controls.Add(_maxNumOfCharsUpDown, 3, 9);
			_tableLayoutPanel.Name = "_tableLayoutPanel";
			resources.ApplyResources(_outputFormatLabel, "_outputFormatLabel");
			_tableLayoutPanel.SetColumnSpan(_outputFormatLabel, 3);
			_outputFormatLabel.Name = "_outputFormatLabel";
			resources.ApplyResources(_outputFormatCombo, "_outputFormatCombo");
			_outputFormatCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			_outputFormatCombo.DropDownWidth = 235;
			_outputFormatCombo.FormattingEnabled = true;
			_outputFormatCombo.Items.AddRange(new object[5]
			{
				resources.GetString("_outputFormatCombo.Items"),
				resources.GetString("_outputFormatCombo.Items1"),
				resources.GetString("_outputFormatCombo.Items2"),
				resources.GetString("_outputFormatCombo.Items3"),
				resources.GetString("_outputFormatCombo.Items4")
			});
			_outputFormatCombo.Name = "_outputFormatCombo";
			_outputFormatCombo.SelectedIndexChanged += new EventHandler(OutputFormatCombo_SelectedIndexChanged);
			resources.ApplyResources(_delimiterEdit, "_delimiterEdit");
			_delimiterEdit.Name = "_delimiterEdit";
			resources.ApplyResources(_delimiterLabel, "_delimiterLabel");
			_tableLayoutPanel.SetColumnSpan(_delimiterLabel, 3);
			_delimiterLabel.Name = "_delimiterLabel";
			resources.ApplyResources(_printColHeadersCheck, "_printColHeadersCheck");
			_tableLayoutPanel.SetColumnSpan(_printColHeadersCheck, 4);
			_printColHeadersCheck.Name = "_printColHeadersCheck";
			resources.ApplyResources(_includeQueryCheckBox, "_includeQueryCheckBox");
			_tableLayoutPanel.SetColumnSpan(_includeQueryCheckBox, 4);
			_includeQueryCheckBox.Name = "_includeQueryCheckBox";
			resources.ApplyResources(_scrollResutsCheckBox, "_scrollResutsCheckBox");
			_tableLayoutPanel.SetColumnSpan(_scrollResutsCheckBox, 4);
			_scrollResutsCheckBox.Name = "_scrollResutsCheckBox";
			resources.ApplyResources(_rightAlignCheck, "_rightAlignCheck");
			_tableLayoutPanel.SetColumnSpan(_rightAlignCheck, 4);
			_rightAlignCheck.Name = "_rightAlignCheck";
			resources.ApplyResources(_discardResultsCheckBox, "_discardResultsCheckBox");
			_tableLayoutPanel.SetColumnSpan(_discardResultsCheckBox, 4);
			_discardResultsCheckBox.Name = "_discardResultsCheckBox";
			_discardResultsCheckBox.CheckedChanged += new EventHandler(DiscardResultsCheckBox_CheckedChanged);
			resources.ApplyResources(_resInSepTabCheckBox, "_resInSepTabCheckBox");
			_tableLayoutPanel.SetColumnSpan(_resInSepTabCheckBox, 4);
			_resInSepTabCheckBox.Name = "_resInSepTabCheckBox";
			_resInSepTabCheckBox.CheckedChanged += new EventHandler(ResInSepTabCheckBox_CheckedChanged);
			resources.ApplyResources(_switchToResultsTabCheckBox, "_switchToResultsTabCheckBox");
			_tableLayoutPanel.SetColumnSpan(_switchToResultsTabCheckBox, 4);
			_switchToResultsTabCheckBox.Name = "_switchToResultsTabCheckBox";
			resources.ApplyResources(_maxNumOfCharsLabel, "_maxNumOfCharsLabel");
			_tableLayoutPanel.SetColumnSpan(_maxNumOfCharsLabel, 3);
			_maxNumOfCharsLabel.Name = "_maxNumOfCharsLabel";
			resources.ApplyResources(_maxNumOfCharsUpDown, "_maxNumOfCharsUpDown");
			_maxNumOfCharsUpDown.Maximum = new decimal(new int[4] { 8192, 0, 0, 0 });
			_maxNumOfCharsUpDown.Minimum = new decimal(new int[4] { 30, 0, 0, 0 });
			_maxNumOfCharsUpDown.MinimumSize = new System.Drawing.Size(74, 0);
			_maxNumOfCharsUpDown.Name = "_maxNumOfCharsUpDown";
			_maxNumOfCharsUpDown.Value = new decimal(new int[4] { 30, 0, 0, 0 });
			resources.ApplyResources(this, "$this");
			Controls.Add(tableLayoutPanel1);
			Controls.Add(_tableLayoutPanel);
			Name = "SqlResultToTextSettingsDlg";
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			_tableLayoutPanel.ResumeLayout(false);
			_tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)_maxNumOfCharsUpDown).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		public override string GetHelpKeyword()
		{
			return helpKeyword;
		}

		public override bool ValidateValuesInControls()
		{
			Tracer.Trace(GetType(), "ResultsToTextSettings.ValidateValuesInControls", "", null);
			if (_outputFormatCombo.SelectedIndex == m_customDelimitedItemIndex && _delimiterEdit.Text.Length == 0)
			{
				Cmd.ShowMessageBoxEx(null, ControlsResources.ColumnDelimiterCannotBeEmpty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}

			if (!ValidateNumeric(_maxNumOfCharsUpDown, string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrInvalidMaxCharsPerColumn, _maxNumOfCharsUpDown.Minimum, _maxNumOfCharsUpDown.Maximum)))
			{
				return false;
			}

			if (TrackChanges && !SaveOrCompareCurrentValueOfControls(save: false))
			{
				Cmd.ShowMessageBoxEx(null, ControlsResources.WarnSqlResultsToTextChanges, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				TrackChanges = false;
			}

			return true;
		}

		protected override void ApplySettingsToUI(IUserSettings options)
		{
			if (options == null)
			{
				return;
			}

			_outputFormatCombo.SelectedIndex = m_customDelimitedItemIndex;
			_delimiterEdit.Text = options.ExecutionResults.ColumnDelimiterForText.ToString();
			for (int i = 0; i < s_delimChars.Length; i++)
			{
				if (s_delimChars[i] == options.ExecutionResults.ColumnDelimiterForText)
				{
					_outputFormatCombo.SelectedIndex = i;
					_delimiterEdit.Text = "";
					_delimiterEdit.Enabled = false;
					break;
				}
			}

			_maxNumOfCharsUpDown.Text = options.ExecutionResults.MaxCharsPerColumnForText.ToString(CultureInfo.InvariantCulture);
			_printColHeadersCheck.Checked = options.ExecutionResults.PrintColumnHeadersForText;
			_includeQueryCheckBox.Checked = options.ExecutionResults.OutputQueryForText;
			_discardResultsCheckBox.Checked = options.ExecutionResults.DiscardResultsForText;
			_scrollResutsCheckBox.Checked = options.ExecutionResults.ScrollResultsAsReceivedForText;
			_resInSepTabCheckBox.Checked = options.ExecutionResults.DisplayResultInSeparateTabForText;
			_switchToResultsTabCheckBox.Checked = options.ExecutionResults.SwitchToResultsTabAfterQueryExecutesForText;
			_switchToResultsTabCheckBox.Enabled = _resInSepTabCheckBox.Checked;
			if (_switchToResultsTabCheckBox.Enabled)
			{
				_switchToResultsTabCheckBox.Checked = options.ExecutionResults.SwitchToResultsTabAfterQueryExecutesForText;
			}

			DiscardResultsCheckBox_CheckedChanged(this, EventArgs.Empty);
			ProcessRightAlighCheckBox();
			if (_rightAlignCheck.Enabled)
			{
				_rightAlignCheck.Checked = options.ExecutionResults.RightAlignNumericsForText;
			}

			if (TrackChanges)
			{
				SaveOrCompareCurrentValueOfControls(save: true);
			}
		}

		protected override void SaveSettingsFromUI(IUserSettings options)
		{
			if (_outputFormatCombo.SelectedIndex == m_customDelimitedItemIndex)
			{
				if (_delimiterEdit.Text.Length != 0)
				{
					options.ExecutionResults.ColumnDelimiterForText = _delimiterEdit.Text[0];
				}
				else
				{
					options.ExecutionResults.ColumnDelimiterForText = s_delimChars[0];
				}
			}
			else
			{
				options.ExecutionResults.ColumnDelimiterForText = s_delimChars[_outputFormatCombo.SelectedIndex];
			}

			options.ExecutionResults.MaxCharsPerColumnForText = (int)_maxNumOfCharsUpDown.Value;
			options.ExecutionResults.PrintColumnHeadersForText = _printColHeadersCheck.Checked;
			options.ExecutionResults.RightAlignNumericsForText = _rightAlignCheck.Checked;
			options.ExecutionResults.OutputQueryForText = _includeQueryCheckBox.Checked;
			options.ExecutionResults.DiscardResultsForText = _discardResultsCheckBox.Checked;
			options.ExecutionResults.DisplayResultInSeparateTabForText = _resInSepTabCheckBox.Checked;
			options.ExecutionResults.ScrollResultsAsReceivedForText = _scrollResutsCheckBox.Checked;
			if (_switchToResultsTabCheckBox.Enabled)
			{
				options.ExecutionResults.SwitchToResultsTabAfterQueryExecutesForText = _switchToResultsTabCheckBox.Checked;
			}
		}

		private void ResetButton_Click(object sender, EventArgs e)
		{
			ResetSettings();
		}

		private void ProcessRightAlighCheckBox()
		{
			_rightAlignCheck.Enabled = _outputFormatCombo.SelectedIndex == m_columnAlignedItemIndex && !_discardResultsCheckBox.Checked;
			if (!_rightAlignCheck.Enabled)
			{
				_rightAlignCheck.Checked = false;
			}
		}

		private void OutputFormatCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			Tracer.Trace(GetType(), "ResultsToTextSettings.OutputFormatCombo_SelectedIndexChanged", "", null);
			_delimiterEdit.Enabled = _outputFormatCombo.SelectedIndex == m_customDelimitedItemIndex;
			ProcessRightAlighCheckBox();
		}

		private void ResInSepTabCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Tracer.Trace(GetType(), "ResultsToTextSettings.ResInSepTabCheckBox_CheckedChanged", "", null);
			_switchToResultsTabCheckBox.Enabled = _resInSepTabCheckBox.Checked;
			if (!_switchToResultsTabCheckBox.Enabled)
			{
				_switchToResultsTabCheckBox.Checked = false;
			}
		}

		private void DiscardResultsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Tracer.Trace(GetType(), "ResultsToTextSettings.DiscardResultsCheckBox_CheckedChanged", "", null);
			foreach (Control control in Controls)
			{
				if (control is CheckBox checkBox && checkBox != _discardResultsCheckBox && checkBox != _switchToResultsTabCheckBox && checkBox != _rightAlignCheck)
				{
					checkBox.Enabled = !_discardResultsCheckBox.Checked;
					if (!checkBox.Enabled)
					{
						checkBox.Checked = false;
					}
				}
			}

			_switchToResultsTabCheckBox.Enabled = !_discardResultsCheckBox.Checked && _resInSepTabCheckBox.Checked;
			if (!_switchToResultsTabCheckBox.Enabled)
			{
				_switchToResultsTabCheckBox.Checked = false;
			}

			ProcessRightAlighCheckBox();
		}
	}
}
