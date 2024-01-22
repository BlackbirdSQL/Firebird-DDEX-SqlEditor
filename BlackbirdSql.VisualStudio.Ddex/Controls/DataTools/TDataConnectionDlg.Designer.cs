
using System.Windows.Forms;

namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools
{
	partial class TDataConnectionDlg
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TDataConnectionDlg));
			this.dataSourceLabel = new System.Windows.Forms.Label();
			this.containerControl = new System.Windows.Forms.ContainerControl();
			this.advancedButton = new System.Windows.Forms.Button();
			this.separatorPanel = new System.Windows.Forms.Panel();
			this.testConnectionButton = new System.Windows.Forms.Button();
			this.buttonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.acceptButton = new BlackbirdSql.VisualStudio.Ddex.Controls.DataTools.ExceptionSafeButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.dataProviderToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.chkUpdateServerExplorer = new System.Windows.Forms.CheckBox();
			this.changeDataSourceButton = new System.Windows.Forms.Button();
			this.dataSourceTextBox = new System.Windows.Forms.TextBox();
			this.dataSourceTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.buttonsTableLayoutPanel.SuspendLayout();
			this.dataSourceTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// dataSourceLabel
			// 
			resources.ApplyResources(this.dataSourceLabel, "dataSourceLabel");
			this.dataSourceLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.dataSourceLabel.Name = "dataSourceLabel";
			// 
			// containerControl
			// 
			resources.ApplyResources(this.containerControl, "containerControl");
			this.containerControl.Name = "containerControl";
			this.containerControl.SizeChanged += new System.EventHandler(this.SetConnectionUIControlDockStyle);
			// 
			// advancedButton
			// 
			resources.ApplyResources(this.advancedButton, "advancedButton");
			this.advancedButton.Name = "advancedButton";
			this.advancedButton.Click += new System.EventHandler(this.ShowAdvanced);
			// 
			// separatorPanel
			// 
			resources.ApplyResources(this.separatorPanel, "separatorPanel");
			this.separatorPanel.Name = "separatorPanel";
			this.separatorPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintSeparator);
			// 
			// testConnectionButton
			// 
			resources.ApplyResources(this.testConnectionButton, "testConnectionButton");
			this.testConnectionButton.Name = "testConnectionButton";
			this.testConnectionButton.Click += new System.EventHandler(this.TestConnection);
			// 
			// buttonsTableLayoutPanel
			// 
			resources.ApplyResources(this.buttonsTableLayoutPanel, "buttonsTableLayoutPanel");
			this.buttonsTableLayoutPanel.Controls.Add(this.acceptButton, 0, 0);
			this.buttonsTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
			this.buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
			// 
			// acceptButton
			// 
			resources.ApplyResources(this.acceptButton, "acceptButton");
			this.acceptButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.acceptButton.Name = "acceptButton";
			// 
			// cancelButton
			// 
			resources.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Name = "cancelButton";
			// 
			// chkUpdateServerExplorer
			// 
			resources.ApplyResources(this.chkUpdateServerExplorer, "chkUpdateServerExplorer");
			this.chkUpdateServerExplorer.Checked = true;
			this.chkUpdateServerExplorer.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkUpdateServerExplorer.ForeColor = System.Drawing.SystemColors.ControlText;
			this.chkUpdateServerExplorer.Name = "chkUpdateServerExplorer";
			this.dataProviderToolTip.SetToolTip(this.chkUpdateServerExplorer, resources.GetString("chkUpdateServerExplorer.ToolTip"));
			this.chkUpdateServerExplorer.UseVisualStyleBackColor = true;
			// 
			// changeDataSourceButton
			// 
			resources.ApplyResources(this.changeDataSourceButton, "changeDataSourceButton");
			this.changeDataSourceButton.Name = "changeDataSourceButton";
			this.changeDataSourceButton.Click += new System.EventHandler(this.ChangeDataSource);
			// 
			// dataSourceTextBox
			// 
			resources.ApplyResources(this.dataSourceTextBox, "dataSourceTextBox");
			this.dataSourceTextBox.Name = "dataSourceTextBox";
			this.dataSourceTextBox.ReadOnly = true;
			// 
			// dataSourceTableLayoutPanel
			// 
			resources.ApplyResources(this.dataSourceTableLayoutPanel, "dataSourceTableLayoutPanel");
			this.dataSourceTableLayoutPanel.Controls.Add(this.dataSourceTextBox, 0, 0);
			this.dataSourceTableLayoutPanel.Controls.Add(this.changeDataSourceButton, 1, 0);
			this.dataSourceTableLayoutPanel.Name = "dataSourceTableLayoutPanel";
			// 
			// TDataConnectionDlg
			// 
			this.AcceptButton = this.acceptButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.Controls.Add(this.chkUpdateServerExplorer);
			this.Controls.Add(this.buttonsTableLayoutPanel);
			this.Controls.Add(this.testConnectionButton);
			this.Controls.Add(this.separatorPanel);
			this.Controls.Add(this.advancedButton);
			this.Controls.Add(this.containerControl);
			this.Controls.Add(this.dataSourceTableLayoutPanel);
			this.Controls.Add(this.dataSourceLabel);
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TDataConnectionDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.buttonsTableLayoutPanel.ResumeLayout(false);
			this.buttonsTableLayoutPanel.PerformLayout();
			this.dataSourceTableLayoutPanel.ResumeLayout(false);
			this.dataSourceTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Label dataSourceLabel;
		private ToolTip dataProviderToolTip;
		private ContainerControl containerControl;
		private Button advancedButton;
		private Panel separatorPanel;
		private Button testConnectionButton;
		private TableLayoutPanel buttonsTableLayoutPanel;
		private ExceptionSafeButton acceptButton;
		private Button cancelButton;
		private Button changeDataSourceButton;
		private TextBox dataSourceTextBox;
		private TableLayoutPanel dataSourceTableLayoutPanel;
		private CheckBox chkUpdateServerExplorer;
	}
}
