#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Globalization;
using System.Windows.Forms;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public class SqlExecutionAdvancedSettingsDlg : ToolsOptionsBaseControl
	{
		private const int C_NormalPrioComboIndex = 0;

		private const int C_LowPrioComboIndex = 1;

		// private static readonly string[] s_deadlockPrioComboBoxItems = new string[2] { "Normal", "Low" };

		// private static readonly string[] s_tranIsolComboBoxItems = new string[4] { "READ COMMITTED", "READ UNCOMMITTED", "REPEATABLE READ", "SERIALIZABLE" };

		private static readonly string helpKeyword = "BlackbirdSql.Common.Controls.SqlExecutionAdvancedSettingsOption";

		private NumericUpDown _queryGovernorUpDown;

		private NumericUpDown _lockTimeoutUpDown;

		private Label _secondsLabel;

		private Label _captionLabel;

		private CheckBox _noCountCheckBox;

		private CheckBox _parseOnlyCheckBox;

		private CheckBox _noExecCheckBox;

		private CheckBox _concatNullCheckBox;

		private CheckBox _arithCheckBox;

		private WrappingCheckBox _spTextCheckBox;

		private WrappingCheckBox _statTimeCheckBox;

		private WrappingCheckBox _statIoCheckBox;

		private Label _tranIsollabel;

		private ComboBox _tranIsolComboBox;

		private Label _lockTimeoutLabel;

		private Label _queryGovLabel;

		private Label _deadlockPrioLabel;

		private CheckBox _noProvInErrorCheckBox;

		private CheckBox _discAfterQueryCheckBox;

		private TableLayoutPanel _tableLayoutPanel;

		private TableLayoutPanel tableLayoutPanel1;

		private Button _resetButton;

		private ComboBox _deadlockPrioComboBox;

		public SqlExecutionAdvancedSettingsDlg()
		{
			InitializeComponent();
			((CheckBox)(object)_spTextCheckBox).CheckedChanged += OnExecutionPlanTextCheckedChanged;
			_noExecCheckBox.CheckedChanged += OnNoExecCheckedChanged;
			_parseOnlyCheckBox.CheckedChanged += OnParseonlyCheckedChanged;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this._resetButton = new System.Windows.Forms.Button();
			this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this._captionLabel = new System.Windows.Forms.Label();
			this._noCountCheckBox = new System.Windows.Forms.CheckBox();
			this._arithCheckBox = new System.Windows.Forms.CheckBox();
			this._noExecCheckBox = new System.Windows.Forms.CheckBox();
			this._spTextCheckBox = new BlackbirdSql.Common.Controls.WrappingCheckBox();
			this._parseOnlyCheckBox = new System.Windows.Forms.CheckBox();
			this._statTimeCheckBox = new BlackbirdSql.Common.Controls.WrappingCheckBox();
			this._concatNullCheckBox = new System.Windows.Forms.CheckBox();
			this._statIoCheckBox = new BlackbirdSql.Common.Controls.WrappingCheckBox();
			this._tranIsollabel = new System.Windows.Forms.Label();
			this._tranIsolComboBox = new System.Windows.Forms.ComboBox();
			this._deadlockPrioLabel = new System.Windows.Forms.Label();
			this._deadlockPrioComboBox = new System.Windows.Forms.ComboBox();
			this._lockTimeoutLabel = new System.Windows.Forms.Label();
			this._lockTimeoutUpDown = new System.Windows.Forms.NumericUpDown();
			this._secondsLabel = new System.Windows.Forms.Label();
			this._queryGovLabel = new System.Windows.Forms.Label();
			this._queryGovernorUpDown = new System.Windows.Forms.NumericUpDown();
			this._noProvInErrorCheckBox = new System.Windows.Forms.CheckBox();
			this._discAfterQueryCheckBox = new System.Windows.Forms.CheckBox();
			this.tableLayoutPanel1.SuspendLayout();
			this._tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._lockTimeoutUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._queryGovernorUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this._resetButton, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 306);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(783, 24);
			this.tableLayoutPanel1.TabIndex = 22;
			// 
			// _resetButton
			// 
			this._resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._resetButton.AutoSize = true;
			this._resetButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.SetColumnSpan(this._resetButton, 4);
			this._resetButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._resetButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._resetButton.Location = new System.Drawing.Point(677, 0);
			this._resetButton.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this._resetButton.MinimumSize = new System.Drawing.Size(75, 23);
			this._resetButton.Name = "_resetButton";
			this._resetButton.Size = new System.Drawing.Size(104, 24);
			this._resetButton.TabIndex = 1;
			this._resetButton.Text = "&Reset to Default";
			this._resetButton.Click += new System.EventHandler(this.ResetButton_Click);
			// 
			// _tableLayoutPanel
			// 
			this._tableLayoutPanel.AutoSize = true;
			this._tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._tableLayoutPanel.ColumnCount = 4;
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutPanel.Controls.Add(this._captionLabel, 0, 0);
			this._tableLayoutPanel.Controls.Add(this._noCountCheckBox, 0, 1);
			this._tableLayoutPanel.Controls.Add(this._arithCheckBox, 1, 1);
			this._tableLayoutPanel.Controls.Add(this._noExecCheckBox, 0, 2);
			this._tableLayoutPanel.Controls.Add(this._spTextCheckBox, 1, 2);
			this._tableLayoutPanel.Controls.Add(this._parseOnlyCheckBox, 0, 3);
			this._tableLayoutPanel.Controls.Add(this._statTimeCheckBox, 1, 3);
			this._tableLayoutPanel.Controls.Add(this._concatNullCheckBox, 0, 4);
			this._tableLayoutPanel.Controls.Add(this._statIoCheckBox, 1, 4);
			this._tableLayoutPanel.Controls.Add(this._tranIsollabel, 0, 5);
			this._tableLayoutPanel.Controls.Add(this._tranIsolComboBox, 1, 5);
			this._tableLayoutPanel.Controls.Add(this._deadlockPrioLabel, 0, 6);
			this._tableLayoutPanel.Controls.Add(this._deadlockPrioComboBox, 1, 6);
			this._tableLayoutPanel.Controls.Add(this._lockTimeoutLabel, 0, 7);
			this._tableLayoutPanel.Controls.Add(this._lockTimeoutUpDown, 1, 7);
			this._tableLayoutPanel.Controls.Add(this._secondsLabel, 2, 7);
			this._tableLayoutPanel.Controls.Add(this._queryGovLabel, 0, 8);
			this._tableLayoutPanel.Controls.Add(this._queryGovernorUpDown, 1, 8);
			this._tableLayoutPanel.Controls.Add(this._noProvInErrorCheckBox, 0, 9);
			this._tableLayoutPanel.Controls.Add(this._discAfterQueryCheckBox, 0, 10);
			this._tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this._tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this._tableLayoutPanel.Name = "_tableLayoutPanel";
			this._tableLayoutPanel.RowCount = 13;
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.Size = new System.Drawing.Size(783, 280);
			this._tableLayoutPanel.TabIndex = 21;
			// 
			// _captionLabel
			// 
			this._captionLabel.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._captionLabel, 3);
			this._captionLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._captionLabel.Location = new System.Drawing.Point(0, 8);
			this._captionLabel.Margin = new System.Windows.Forms.Padding(0, 8, 2, 2);
			this._captionLabel.Name = "_captionLabel";
			this._captionLabel.Size = new System.Drawing.Size(221, 15);
			this._captionLabel.TabIndex = 1;
			this._captionLabel.Text = "Specify the advanced execution settings.";
			// 
			// _noCountCheckBox
			// 
			this._noCountCheckBox.AutoSize = true;
			this._noCountCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._noCountCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._noCountCheckBox.Location = new System.Drawing.Point(0, 35);
			this._noCountCheckBox.Margin = new System.Windows.Forms.Padding(0, 10, 2, 2);
			this._noCountCheckBox.Name = "_noCountCheckBox";
			this._noCountCheckBox.Size = new System.Drawing.Size(111, 20);
			this._noCountCheckBox.TabIndex = 2;
			this._noCountCheckBox.Text = "SET &NOCOUNT";
			// 
			// _arithCheckBox
			// 
			this._arithCheckBox.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._arithCheckBox, 3);
			this._arithCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._arithCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._arithCheckBox.Location = new System.Drawing.Point(213, 35);
			this._arithCheckBox.Margin = new System.Windows.Forms.Padding(2, 10, 0, 2);
			this._arithCheckBox.Name = "_arithCheckBox";
			this._arithCheckBox.Size = new System.Drawing.Size(122, 20);
			this._arithCheckBox.TabIndex = 3;
			this._arithCheckBox.Text = "SET ARITHA&BORT";
			// 
			// _noExecCheckBox
			// 
			this._noExecCheckBox.AutoSize = true;
			this._noExecCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._noExecCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._noExecCheckBox.Location = new System.Drawing.Point(0, 59);
			this._noExecCheckBox.Margin = new System.Windows.Forms.Padding(0, 2, 2, 2);
			this._noExecCheckBox.Name = "_noExecCheckBox";
			this._noExecCheckBox.Size = new System.Drawing.Size(98, 20);
			this._noExecCheckBox.TabIndex = 4;
			this._noExecCheckBox.Text = "SET N&OEXEC";
			// 
			// _spTextCheckBox
			// 
			this._spTextCheckBox.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._spTextCheckBox, 3);
			this._spTextCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._spTextCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._spTextCheckBox.Location = new System.Drawing.Point(213, 59);
			this._spTextCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this._spTextCheckBox.Name = "_spTextCheckBox";
			this._spTextCheckBox.Size = new System.Drawing.Size(148, 20);
			this._spTextCheckBox.TabIndex = 5;
			this._spTextCheckBox.Text = "SET &SHOWPLAN_TEXT";
			// 
			// _parseOnlyCheckBox
			// 
			this._parseOnlyCheckBox.AutoSize = true;
			this._parseOnlyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._parseOnlyCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._parseOnlyCheckBox.Location = new System.Drawing.Point(0, 83);
			this._parseOnlyCheckBox.Margin = new System.Windows.Forms.Padding(0, 2, 2, 2);
			this._parseOnlyCheckBox.Name = "_parseOnlyCheckBox";
			this._parseOnlyCheckBox.Size = new System.Drawing.Size(116, 20);
			this._parseOnlyCheckBox.TabIndex = 6;
			this._parseOnlyCheckBox.Text = "SET &PARSEONLY";
			// 
			// _statTimeCheckBox
			// 
			this._statTimeCheckBox.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._statTimeCheckBox, 3);
			this._statTimeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._statTimeCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._statTimeCheckBox.Location = new System.Drawing.Point(213, 83);
			this._statTimeCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this._statTimeCheckBox.Name = "_statTimeCheckBox";
			this._statTimeCheckBox.Size = new System.Drawing.Size(138, 20);
			this._statTimeCheckBox.TabIndex = 7;
			this._statTimeCheckBox.Text = "S&ET STATISTICS TIME";
			// 
			// _concatNullCheckBox
			// 
			this._concatNullCheckBox.AutoSize = true;
			this._concatNullCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._concatNullCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._concatNullCheckBox.Location = new System.Drawing.Point(0, 107);
			this._concatNullCheckBox.Margin = new System.Windows.Forms.Padding(0, 2, 2, 2);
			this._concatNullCheckBox.Name = "_concatNullCheckBox";
			this._concatNullCheckBox.Size = new System.Drawing.Size(209, 20);
			this._concatNullCheckBox.TabIndex = 8;
			this._concatNullCheckBox.Text = "SET &CONCAT_NULL_YIELDS_NULL";
			// 
			// _statIoCheckBox
			// 
			this._statIoCheckBox.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._statIoCheckBox, 3);
			this._statIoCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._statIoCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._statIoCheckBox.Location = new System.Drawing.Point(213, 107);
			this._statIoCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this._statIoCheckBox.Name = "_statIoCheckBox";
			this._statIoCheckBox.Size = new System.Drawing.Size(124, 20);
			this._statIoCheckBox.TabIndex = 9;
			this._statIoCheckBox.Text = "SET ST&ATISTICS IO";
			// 
			// _tranIsollabel
			// 
			this._tranIsollabel.AutoSize = true;
			this._tranIsollabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._tranIsollabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._tranIsollabel.Location = new System.Drawing.Point(0, 136);
			this._tranIsollabel.Margin = new System.Windows.Forms.Padding(0, 7, 2, 2);
			this._tranIsollabel.Name = "_tranIsollabel";
			this._tranIsollabel.Size = new System.Drawing.Size(205, 15);
			this._tranIsollabel.TabIndex = 10;
			this._tranIsollabel.Text = "SET &TRANSACTION ISOLATION LEVEL:";
			this._tranIsollabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _tranIsolComboBox
			// 
			this._tranIsolComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanel.SetColumnSpan(this._tranIsolComboBox, 2);
			this._tranIsolComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._tranIsolComboBox.FormattingEnabled = true;
			this._tranIsolComboBox.ItemHeight = 15;
			this._tranIsolComboBox.Items.AddRange(new object[] {
            "READ COMMITTED",
            "READ UNCOMMITTED",
            "REPEATABLE READ",
            "SERIALIZABLE"});
			this._tranIsolComboBox.Location = new System.Drawing.Point(213, 131);
			this._tranIsolComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this._tranIsolComboBox.MinimumSize = new System.Drawing.Size(150, 0);
			this._tranIsolComboBox.Name = "_tranIsolComboBox";
			this._tranIsolComboBox.Size = new System.Drawing.Size(162, 23);
			this._tranIsolComboBox.TabIndex = 11;
			// 
			// _deadlockPrioLabel
			// 
			this._deadlockPrioLabel.AutoSize = true;
			this._deadlockPrioLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._deadlockPrioLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._deadlockPrioLabel.Location = new System.Drawing.Point(0, 161);
			this._deadlockPrioLabel.Margin = new System.Windows.Forms.Padding(0, 7, 2, 2);
			this._deadlockPrioLabel.Name = "_deadlockPrioLabel";
			this._deadlockPrioLabel.Size = new System.Drawing.Size(145, 15);
			this._deadlockPrioLabel.TabIndex = 12;
			this._deadlockPrioLabel.Text = "SET &DEADLOCK_PRIORITY:";
			this._deadlockPrioLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _deadlockPrioComboBox
			// 
			this._deadlockPrioComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanel.SetColumnSpan(this._deadlockPrioComboBox, 2);
			this._deadlockPrioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._deadlockPrioComboBox.FormattingEnabled = true;
			this._deadlockPrioComboBox.ItemHeight = 15;
			this._deadlockPrioComboBox.Items.AddRange(new object[] {
            "Normal",
            "Low"});
			this._deadlockPrioComboBox.Location = new System.Drawing.Point(213, 156);
			this._deadlockPrioComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this._deadlockPrioComboBox.MinimumSize = new System.Drawing.Size(144, 0);
			this._deadlockPrioComboBox.Name = "_deadlockPrioComboBox";
			this._deadlockPrioComboBox.Size = new System.Drawing.Size(162, 23);
			this._deadlockPrioComboBox.TabIndex = 13;
			// 
			// _lockTimeoutLabel
			// 
			this._lockTimeoutLabel.AutoSize = true;
			this._lockTimeoutLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._lockTimeoutLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._lockTimeoutLabel.Location = new System.Drawing.Point(0, 186);
			this._lockTimeoutLabel.Margin = new System.Windows.Forms.Padding(0, 7, 2, 2);
			this._lockTimeoutLabel.Name = "_lockTimeoutLabel";
			this._lockTimeoutLabel.Size = new System.Drawing.Size(113, 15);
			this._lockTimeoutLabel.TabIndex = 14;
			this._lockTimeoutLabel.Text = "SET &LOCK TIMEOUT:";
			this._lockTimeoutLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _lockTimeoutUpDown
			// 
			this._lockTimeoutUpDown.AutoSize = true;
			this._lockTimeoutUpDown.Location = new System.Drawing.Point(213, 181);
			this._lockTimeoutUpDown.Margin = new System.Windows.Forms.Padding(2);
			this._lockTimeoutUpDown.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
			this._lockTimeoutUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this._lockTimeoutUpDown.Name = "_lockTimeoutUpDown";
			this._lockTimeoutUpDown.Size = new System.Drawing.Size(83, 23);
			this._lockTimeoutUpDown.TabIndex = 15;
			// 
			// _secondsLabel
			// 
			this._secondsLabel.AutoSize = true;
			this._secondsLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._secondsLabel.Location = new System.Drawing.Point(300, 186);
			this._secondsLabel.Margin = new System.Windows.Forms.Padding(2, 7, 2, 2);
			this._secondsLabel.Name = "_secondsLabel";
			this._secondsLabel.Size = new System.Drawing.Size(73, 15);
			this._secondsLabel.TabIndex = 16;
			this._secondsLabel.Text = "milliseconds";
			this._secondsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _queryGovLabel
			// 
			this._queryGovLabel.AutoSize = true;
			this._queryGovLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._queryGovLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._queryGovLabel.Location = new System.Drawing.Point(0, 213);
			this._queryGovLabel.Margin = new System.Windows.Forms.Padding(0, 7, 2, 2);
			this._queryGovLabel.Name = "_queryGovLabel";
			this._queryGovLabel.Size = new System.Drawing.Size(203, 15);
			this._queryGovLabel.TabIndex = 17;
			this._queryGovLabel.Text = "SET &QUERY_GOVERNOR_COST_LIMIT:";
			this._queryGovLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _queryGovernorUpDown
			// 
			this._queryGovernorUpDown.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._queryGovernorUpDown, 2);
			this._queryGovernorUpDown.Location = new System.Drawing.Point(213, 208);
			this._queryGovernorUpDown.Margin = new System.Windows.Forms.Padding(2);
			this._queryGovernorUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this._queryGovernorUpDown.Name = "_queryGovernorUpDown";
			this._queryGovernorUpDown.Size = new System.Drawing.Size(59, 23);
			this._queryGovernorUpDown.TabIndex = 18;
			// 
			// _noProvInErrorCheckBox
			// 
			this._noProvInErrorCheckBox.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._noProvInErrorCheckBox, 3);
			this._noProvInErrorCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._noProvInErrorCheckBox.Location = new System.Drawing.Point(0, 235);
			this._noProvInErrorCheckBox.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this._noProvInErrorCheckBox.Name = "_noProvInErrorCheckBox";
			this._noProvInErrorCheckBox.Size = new System.Drawing.Size(213, 19);
			this._noProvInErrorCheckBox.TabIndex = 19;
			this._noProvInErrorCheckBox.Text = "Suppress provider &message headers";
			// 
			// _discAfterQueryCheckBox
			// 
			this._discAfterQueryCheckBox.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._discAfterQueryCheckBox, 3);
			this._discAfterQueryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._discAfterQueryCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._discAfterQueryCheckBox.Location = new System.Drawing.Point(0, 258);
			this._discAfterQueryCheckBox.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this._discAfterQueryCheckBox.Name = "_discAfterQueryCheckBox";
			this._discAfterQueryCheckBox.Size = new System.Drawing.Size(220, 20);
			this._discAfterQueryCheckBox.TabIndex = 20;
			this._discAfterQueryCheckBox.Text = "D&isconnect after the query executes";
			// 
			// SqlExecutionAdvancedSettingsDlg
			// 
			this.AutoScroll = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this._tableLayoutPanel);
			this.Name = "SqlExecutionAdvancedSettingsDlg";
			this.Size = new System.Drawing.Size(783, 330);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this._tableLayoutPanel.ResumeLayout(false);
			this._tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._lockTimeoutUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._queryGovernorUpDown)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		public override string GetHelpKeyword()
		{
			return helpKeyword;
		}

		public override bool ValidateValuesInControls()
		{
			Tracer.Trace(GetType(), "", null);
			if (!ValidateNumeric(_lockTimeoutUpDown, string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrInvalidSetLockTimeout, _lockTimeoutUpDown.Minimum, _lockTimeoutUpDown.Maximum)))
			{
				return false;
			}

			if (!ValidateNumeric(_queryGovernorUpDown, string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrInvalidSetQueryGov, _queryGovernorUpDown.Minimum, _queryGovernorUpDown.Maximum)))
			{
				return false;
			}

			if (TrackChanges && !SaveOrCompareCurrentValueOfControls(save: false))
			{
				Cmd.ShowMessageBoxEx(null, ControlsResources.WarnSqlAdvancedQueryChanges, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				TrackChanges = false;
			}

			return true;
		}

		protected override void ApplySettingsToUI(IBUserSettings options)
		{
			if (options != null)
			{
				_noExecCheckBox.Checked = options.Execution.SetNoExec;
				_noCountCheckBox.Checked = options.Execution.SetNoCount;
				_parseOnlyCheckBox.Checked = options.Execution.SetParseOnly;
				_concatNullCheckBox.Checked = options.Execution.SetConcatenationNull;
				_arithCheckBox.Checked = options.Execution.SetArithAbort;
				((CheckBox)(object)_spTextCheckBox).Checked = options.Execution.SetShowplanText;
				((CheckBox)(object)_statTimeCheckBox).Checked = options.Execution.SetStatisticsTime;
				((CheckBox)(object)_statIoCheckBox).Checked = options.Execution.SetStatisticsIO;
				_noProvInErrorCheckBox.Checked = options.Execution.SuppressProviderMessageHeaders;
				if (options.Execution.SetDeadlockPriorityLow)
				{
					_deadlockPrioComboBox.SelectedIndex = C_LowPrioComboIndex;
				}
				else
				{
					_deadlockPrioComboBox.SelectedIndex = C_NormalPrioComboIndex;
				}

				_queryGovernorUpDown.Text = options.Execution.SetQueryGovernorCost.ToString(CultureInfo.InvariantCulture);
				_lockTimeoutUpDown.Text = options.Execution.SetLockTimeout.ToString(CultureInfo.InvariantCulture);
				_tranIsolComboBox.Text = options.Execution.SetTransactionIsolationLevel;
				_discAfterQueryCheckBox.Checked = options.Execution.DisconnectAfterQueryExecutes;
				if (TrackChanges)
				{
					SaveOrCompareCurrentValueOfControls(save: true);
				}
			}
		}

		protected override void SaveSettingsFromUI(IBUserSettings options)
		{
			options.Execution.SetNoExec = _noExecCheckBox.Checked;
			options.Execution.SetNoCount = _noCountCheckBox.Checked;
			options.Execution.SetParseOnly = _parseOnlyCheckBox.Checked;
			options.Execution.SetConcatenationNull = _concatNullCheckBox.Checked;
			options.Execution.SetArithAbort = _arithCheckBox.Checked;
			options.Execution.SetShowplanText = ((CheckBox)(object)_spTextCheckBox).Checked;
			options.Execution.SetStatisticsTime = ((CheckBox)(object)_statTimeCheckBox).Checked;
			options.Execution.SetStatisticsIO = ((CheckBox)(object)_statIoCheckBox).Checked;
			options.Execution.SuppressProviderMessageHeaders = _noProvInErrorCheckBox.Checked;
			options.Execution.DisconnectAfterQueryExecutes = _discAfterQueryCheckBox.Checked;
			options.Execution.SetDeadlockPriorityLow = _deadlockPrioComboBox.SelectedIndex == 1;
			options.Execution.SetQueryGovernorCost = (int)_queryGovernorUpDown.Value;
			options.Execution.SetLockTimeout = (int)_lockTimeoutUpDown.Value;
			options.Execution.SetTransactionIsolationLevel = _tranIsolComboBox.Text;
		}

		private void ResetButton_Click(object sender, EventArgs e)
		{
			Tracer.Trace(GetType(), "SqlExecutionAdvancedSettingsDlg.m_resetButton_Click", "", null);
			ResetSettings();
		}

		private void ProcessGlobalCheckBoxCommon(CheckBox changed, CheckBox current)
		{
			current.Enabled = !changed.Checked;
			if (!current.Enabled)
			{
				current.Checked = false;
			}
		}

		private void OnExecutionPlanTextCheckedChanged(object sender, EventArgs e)
		{
			foreach (Control control in Controls)
			{
				if (control is CheckBox && control != _spTextCheckBox && control != _arithCheckBox && control != _concatNullCheckBox && control != _noProvInErrorCheckBox && control != _discAfterQueryCheckBox)
				{
					ProcessGlobalCheckBoxCommon((CheckBox)(object)_spTextCheckBox, control as CheckBox);
				}
			}
		}

		private void OnNoExecCheckedChanged(object sender, EventArgs e)
		{
			OnParseonlyOrNoExecCheckedChanged(_noExecCheckBox);
		}

		private void OnParseonlyCheckedChanged(object sender, EventArgs e)
		{
			OnParseonlyOrNoExecCheckedChanged(_parseOnlyCheckBox);
		}

		private void OnParseonlyOrNoExecCheckedChanged(CheckBox changedCheckBox)
		{
			foreach (Control control in Controls)
			{
				if (control is CheckBox && control != changedCheckBox && control != _concatNullCheckBox && control != _noProvInErrorCheckBox && control != _discAfterQueryCheckBox)
				{
					ProcessGlobalCheckBoxCommon(changedCheckBox, control as CheckBox);
				}
			}
		}
	}
}
