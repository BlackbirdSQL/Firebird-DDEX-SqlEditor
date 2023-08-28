#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Globalization;
using System.Windows.Forms;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Properties;




namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public class SqlResultsToGridSettingsDlg : ToolsOptionsBaseControl
	{
		private static readonly string helpKeyword = "BlackbirdSql.Common.Controls.ToolsOptions.SqlResultsToGridSettingsDlg";

		private Label _maxCharsLabel;

		private NumericUpDown _maxCharsUpDown;

		private CheckBox _outputQueryCheck;

		private CheckBox _discardResCheck;

		private CheckBox _multGridsInSepTabsCheck;

		private Label _captionLabel;

		private WrappingCheckBox _includeColumnHeadersCheckBox;

		private Separator _maxCharCaption;

		private Label _maxCharsLabelXml;

		private ComboBox _maxCharsXML;

		private WrappingCheckBox _quoteStringsContainingCommasCheckbox;

		private TableLayoutPanel _tableLayoutPanel;

		private TableLayoutPanel tableLayoutPanelBottom;

		private Button _resetButton;

		private Panel _dummyPanelforRenderingComboBoxAbove;

		private CheckBox _switchToResultsTabCheckBox;

		public SqlResultsToGridSettingsDlg()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Expected O, but got Unknown
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlResultsToGridSettingsDlg));
			tableLayoutPanelBottom = new TableLayoutPanel();
			_resetButton = new Button();
			_tableLayoutPanel = new TableLayoutPanel();
			_captionLabel = new Label();
			_outputQueryCheck = new CheckBox();
			_includeColumnHeadersCheckBox = new WrappingCheckBox();
			_quoteStringsContainingCommasCheckbox = new WrappingCheckBox();
			_discardResCheck = new CheckBox();
			_multGridsInSepTabsCheck = new CheckBox();
			_switchToResultsTabCheckBox = new CheckBox();
			_maxCharCaption = new Separator();
			_maxCharsUpDown = new NumericUpDown();
			_maxCharsLabel = new Label();
			_maxCharsXML = new ComboBox();
			_maxCharsLabelXml = new Label();
			_dummyPanelforRenderingComboBoxAbove = new Panel();
			tableLayoutPanelBottom.SuspendLayout();
			_tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)_maxCharsUpDown).BeginInit();
			SuspendLayout();
			resources.ApplyResources(tableLayoutPanelBottom, "tableLayoutPanelBottom");
			tableLayoutPanelBottom.Controls.Add(_resetButton, 0, 0);
			tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
			resources.ApplyResources(_resetButton, "_resetButton");
			tableLayoutPanelBottom.SetColumnSpan(_resetButton, 2);
			_resetButton.Name = "_resetButton";
			_resetButton.Click += new EventHandler(ResetButton_Click);
			resources.ApplyResources(_tableLayoutPanel, "_tableLayoutPanel");
			_tableLayoutPanel.Controls.Add(_captionLabel, 0, 0);
			_tableLayoutPanel.Controls.Add(_outputQueryCheck, 0, 1);
			_tableLayoutPanel.Controls.Add((Control)(object)_includeColumnHeadersCheckBox, 0, 2);
			_tableLayoutPanel.Controls.Add((Control)(object)_quoteStringsContainingCommasCheckbox, 0, 3);
			_tableLayoutPanel.Controls.Add(_discardResCheck, 0, 4);
			_tableLayoutPanel.Controls.Add(_multGridsInSepTabsCheck, 0, 5);
			_tableLayoutPanel.Controls.Add(_switchToResultsTabCheckBox, 0, 6);
			_tableLayoutPanel.Controls.Add((Control)(object)_maxCharCaption, 0, 7);
			_tableLayoutPanel.Controls.Add(_maxCharsUpDown, 1, 8);
			_tableLayoutPanel.Controls.Add(_maxCharsLabel, 0, 8);
			_tableLayoutPanel.Controls.Add(_maxCharsXML, 1, 9);
			_tableLayoutPanel.Controls.Add(_maxCharsLabelXml, 0, 9);
			_tableLayoutPanel.Controls.Add(_dummyPanelforRenderingComboBoxAbove, 1, 10);
			_tableLayoutPanel.Name = "_tableLayoutPanel";
			resources.ApplyResources(_captionLabel, "_captionLabel");
			_tableLayoutPanel.SetColumnSpan(_captionLabel, 2);
			_captionLabel.Name = "_captionLabel";
			resources.ApplyResources(_outputQueryCheck, "_outputQueryCheck");
			_tableLayoutPanel.SetColumnSpan(_outputQueryCheck, 2);
			_outputQueryCheck.Name = "_outputQueryCheck";
			resources.ApplyResources(_includeColumnHeadersCheckBox, "_includeColumnHeadersCheckBox");
			_tableLayoutPanel.SetColumnSpan((Control)(object)_includeColumnHeadersCheckBox, 2);
			((Control)(object)_includeColumnHeadersCheckBox).Name = "_includeColumnHeadersCheckBox";
			resources.ApplyResources(_quoteStringsContainingCommasCheckbox, "_quoteStringsContainingCommasCheckbox");
			_tableLayoutPanel.SetColumnSpan((Control)(object)_quoteStringsContainingCommasCheckbox, 2);
			((Control)(object)_quoteStringsContainingCommasCheckbox).Name = "_quoteStringsContainingCommasCheckbox";
			resources.ApplyResources(_discardResCheck, "_discardResCheck");
			_tableLayoutPanel.SetColumnSpan(_discardResCheck, 2);
			_discardResCheck.Name = "_discardResCheck";
			_discardResCheck.CheckedChanged += new EventHandler(DiscardResCheck_CheckedChanged);
			resources.ApplyResources(_multGridsInSepTabsCheck, "_multGridsInSepTabsCheck");
			_tableLayoutPanel.SetColumnSpan(_multGridsInSepTabsCheck, 2);
			_multGridsInSepTabsCheck.Name = "_multGridsInSepTabsCheck";
			_multGridsInSepTabsCheck.CheckedChanged += new EventHandler(MultGridsInSepTabsCheck_CheckedChanged);
			resources.ApplyResources(_switchToResultsTabCheckBox, "_switchToResultsTabCheckBox");
			_tableLayoutPanel.SetColumnSpan(_switchToResultsTabCheckBox, 2);
			_switchToResultsTabCheckBox.Name = "_switchToResultsTabCheckBox";
			resources.ApplyResources(_maxCharCaption, "_maxCharCaption");
			_tableLayoutPanel.SetColumnSpan((Control)(object)_maxCharCaption, 2);
			((Control)(object)_maxCharCaption).Name = "_maxCharCaption";
			resources.ApplyResources(_maxCharsUpDown, "_maxCharsUpDown");
			_maxCharsUpDown.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
			_maxCharsUpDown.Minimum = new decimal(new int[4] { 30, 0, 0, 0 });
			_maxCharsUpDown.Name = "_maxCharsUpDown";
			_maxCharsUpDown.Value = new decimal(new int[4] { 30, 0, 0, 0 });
			resources.ApplyResources(_maxCharsLabel, "_maxCharsLabel");
			_maxCharsLabel.Name = "_maxCharsLabel";
			resources.ApplyResources(_maxCharsXML, "_maxCharsXML");
			_maxCharsXML.DropDownStyle = ComboBoxStyle.DropDownList;
			_maxCharsXML.FormattingEnabled = true;
			_maxCharsXML.Items.AddRange(new object[4]
			{
				resources.GetString("_maxCharsXML.Items"),
				resources.GetString("_maxCharsXML.Items1"),
				resources.GetString("_maxCharsXML.Items2"),
				resources.GetString("_maxCharsXML.Items3")
			});
			_maxCharsXML.Name = "_maxCharsXML";
			resources.ApplyResources(_maxCharsLabelXml, "_maxCharsLabelXml");
			_maxCharsLabelXml.Name = "_maxCharsLabelXml";
			resources.ApplyResources(_dummyPanelforRenderingComboBoxAbove, "_dummyPanelforRenderingComboBoxAbove");
			_dummyPanelforRenderingComboBoxAbove.Name = "_dummyPanelforRenderingComboBoxAbove";
			resources.ApplyResources(this, "$this");
			Controls.Add(tableLayoutPanelBottom);
			Controls.Add(_tableLayoutPanel);
			Name = "SqlResultsToGridSettingsDlg";
			tableLayoutPanelBottom.ResumeLayout(false);
			tableLayoutPanelBottom.PerformLayout();
			_tableLayoutPanel.ResumeLayout(false);
			_tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)_maxCharsUpDown).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		public override string GetHelpKeyword()
		{
			return helpKeyword;
		}

		public override bool ValidateValuesInControls()
		{
			if (!ValidateNumeric(_maxCharsUpDown, string.Format(CultureInfo.CurrentCulture, SharedResx.ErrInvalidMaxCharsPerCell, _maxCharsUpDown.Minimum, _maxCharsUpDown.Maximum)))
			{
				return false;
			}

			if (TrackChanges && !SaveOrCompareCurrentValueOfControls(save: false))
			{
				Cmd.ShowMessageBoxEx(null, SharedResx.WarnSqlResultsToGridChanges, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				TrackChanges = false;
			}

			return true;
		}

		protected override void ApplySettingsToUI(IUserSettings options)
		{
			if (options != null)
			{
				switch (options.ExecutionResults.MaxCharsPerColumnForXml)
				{
					case 1048576:
						_maxCharsXML.SelectedIndex = 0;
						break;
					case 2097152:
						_maxCharsXML.SelectedIndex = 1;
						break;
					case 5242880:
						_maxCharsXML.SelectedIndex = 2;
						break;
					case int.MaxValue:
						_maxCharsXML.SelectedIndex = 3;
						break;
					default:
						_maxCharsXML.SelectedIndex = _maxCharsXML.Items.Add(string.Format(CultureInfo.InvariantCulture, "{0} MB", options.ExecutionResults.MaxCharsPerColumnForXml / 1048576.0));
						break;
				}

				_maxCharsUpDown.Text = options.ExecutionResults.MaxCharsPerColumnForGrid.ToString(CultureInfo.InvariantCulture);
				_outputQueryCheck.Checked = options.ExecutionResults.OutputQueryForGrid;
				_discardResCheck.Checked = options.ExecutionResults.DiscardResultsForGrid;
				((CheckBox)(object)_includeColumnHeadersCheckBox).Checked = options.ExecutionResults.IncludeColumnHeadersWhileSavingGridResults;
				_multGridsInSepTabsCheck.Checked = options.ExecutionResults.DisplayResultInSeparateTabForGrid;
				((CheckBox)(object)_quoteStringsContainingCommasCheckbox).Checked = options.ExecutionResults.QuoteStringsContainingCommas;
				_switchToResultsTabCheckBox.Enabled = _multGridsInSepTabsCheck.Checked;
				if (_switchToResultsTabCheckBox.Enabled)
				{
					_switchToResultsTabCheckBox.Checked = options.ExecutionResults.SwitchToResultsTabAfterQueryExecutesForGrid;
				}

				DiscardResCheck_CheckedChanged(this, EventArgs.Empty);
				if (TrackChanges)
				{
					SaveOrCompareCurrentValueOfControls(save: true);
				}
			}
		}

		protected override void SaveSettingsFromUI(IUserSettings options)
		{
			switch (_maxCharsXML.SelectedIndex)
			{
				case 0:
					options.ExecutionResults.MaxCharsPerColumnForXml = 1048576;
					break;
				case 1:
					options.ExecutionResults.MaxCharsPerColumnForXml = 2097152;
					break;
				case 2:
					options.ExecutionResults.MaxCharsPerColumnForXml = 5242880;
					break;
				case 3:
					options.ExecutionResults.MaxCharsPerColumnForXml = int.MaxValue;
					break;
			}

			options.ExecutionResults.MaxCharsPerColumnForGrid = (int)_maxCharsUpDown.Value;
			options.ExecutionResults.OutputQueryForGrid = _outputQueryCheck.Checked;
			options.ExecutionResults.DisplayResultInSeparateTabForGrid = _multGridsInSepTabsCheck.Checked;
			options.ExecutionResults.DiscardResultsForGrid = _discardResCheck.Checked;
			options.ExecutionResults.IncludeColumnHeadersWhileSavingGridResults = ((CheckBox)(object)_includeColumnHeadersCheckBox).Checked;
			options.ExecutionResults.QuoteStringsContainingCommas = ((CheckBox)(object)_quoteStringsContainingCommasCheckbox).Checked;
			if (_switchToResultsTabCheckBox.Enabled)
			{
				options.ExecutionResults.SwitchToResultsTabAfterQueryExecutesForGrid = _switchToResultsTabCheckBox.Checked;
			}
		}

		private void ResetButton_Click(object sender, EventArgs e)
		{
			ResetSettings();
		}

		private void DiscardResCheck_CheckedChanged(object sender, EventArgs e)
		{
			Tracer.Trace(GetType(), "ResultsSettings.DiscardResCheck_CheckedChanged", "", null);
			foreach (Control control in Controls)
			{
				if (control is CheckBox cb && control != _discardResCheck && control != _switchToResultsTabCheckBox)
				{
					control.Enabled = !_discardResCheck.Checked;
					if (!control.Enabled)
					{
						cb.Checked = false;
					}
				}
			}

			_switchToResultsTabCheckBox.Enabled = !_discardResCheck.Checked && _multGridsInSepTabsCheck.Checked;
			if (!_switchToResultsTabCheckBox.Enabled)
			{
				_switchToResultsTabCheckBox.Checked = false;
			}
		}

		private void MultGridsInSepTabsCheck_CheckedChanged(object sender, EventArgs e)
		{
			Tracer.Trace(GetType(), "ResultsSettings.MultGridsInSepTabsCheck_CheckedChanged", "", null);
			_switchToResultsTabCheckBox.Enabled = _multGridsInSepTabsCheck.Checked;
			if (!_switchToResultsTabCheckBox.Enabled)
			{
				_switchToResultsTabCheckBox.Checked = false;
			}
		}
	}
}
