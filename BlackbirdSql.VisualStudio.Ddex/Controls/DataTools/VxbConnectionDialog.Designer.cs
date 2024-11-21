// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.DataConnectionDialog



namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools
{
	// =========================================================================================================
	//
	//										VxbConnectionDialog Class
	//
	/// <summary>
	/// Replication of Microsoft.Data.ConnectionUI.DataConnectionDialog.
	/// </summary>
	// =========================================================================================================
	partial class VxbConnectionDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VxbConnectionDialog));
			this.dataSourceTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.dataSourceTextBox = new System.Windows.Forms.TextBox();
			this.changeDataSourceButton = new System.Windows.Forms.Button();
			this.buttonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.acceptButton = new BlackbirdSql.Core.Controls.Widgets.ExceptionSafeButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.dataSourceLabel = new System.Windows.Forms.Label();
			this.containerControl = new System.Windows.Forms.ContainerControl();
			this.advancedButton = new System.Windows.Forms.Button();
			this.separatorPanel = new System.Windows.Forms.Panel();
			this.testConnectionButton = new System.Windows.Forms.Button();
			this.chkUpdateServerExplorer = new System.Windows.Forms.CheckBox();
			this.dataProviderToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.dataSourceTableLayoutPanel.SuspendLayout();
			this.buttonsTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// dataSourceTableLayoutPanel
			// 
			resources.ApplyResources(this.dataSourceTableLayoutPanel, "dataSourceTableLayoutPanel");
			this.dataSourceTableLayoutPanel.Controls.Add(this.dataSourceTextBox, 0, 0);
			this.dataSourceTableLayoutPanel.Controls.Add(this.changeDataSourceButton, 1, 0);
			this.dataSourceTableLayoutPanel.Name = "dataSourceTableLayoutPanel";
			// 
			// dataSourceTextBox
			// 
			resources.ApplyResources(this.dataSourceTextBox, "dataSourceTextBox");
			this.dataSourceTextBox.Name = "dataSourceTextBox";
			this.dataSourceTextBox.ReadOnly = true;
			this.dataSourceTextBox.TabStop = false;
			// 
			// changeDataSourceButton
			// 
			resources.ApplyResources(this.changeDataSourceButton, "changeDataSourceButton");
			this.changeDataSourceButton.Name = "changeDataSourceButton";
			this.changeDataSourceButton.Click += new System.EventHandler(this.OnChangeDataSource);
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
			this.containerControl.SizeChanged += new System.EventHandler(this.OnSetConnectionUIControlDockStyle);
			// 
			// advancedButton
			// 
			resources.ApplyResources(this.advancedButton, "advancedButton");
			this.advancedButton.Name = "advancedButton";
			this.advancedButton.Click += new System.EventHandler(this.OnShowAdvanced);
			// 
			// separatorPanel
			// 
			resources.ApplyResources(this.separatorPanel, "separatorPanel");
			this.separatorPanel.Name = "separatorPanel";
			this.separatorPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintSeparator);
			// 
			// testConnectionButton
			// 
			resources.ApplyResources(this.testConnectionButton, "testConnectionButton");
			this.testConnectionButton.Name = "testConnectionButton";
			this.testConnectionButton.Click += new System.EventHandler(this.OnTestConnection);
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
			this.chkUpdateServerExplorer.CheckedChanged += new System.EventHandler(this.ChkUpdateServerExplorer_CheckedChanged);
			// 
			// VxbConnectionDialog
			// 
			this.AcceptButton = this.acceptButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.Controls.Add(this.chkUpdateServerExplorer);
			this.Controls.Add(this.testConnectionButton);
			this.Controls.Add(this.separatorPanel);
			this.Controls.Add(this.advancedButton);
			this.Controls.Add(this.containerControl);
			this.Controls.Add(this.dataSourceLabel);
			this.Controls.Add(this.dataSourceTableLayoutPanel);
			this.Controls.Add(this.buttonsTableLayoutPanel);
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "VxbConnectionDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.dataSourceTableLayoutPanel.ResumeLayout(false);
			this.dataSourceTableLayoutPanel.PerformLayout();
			this.buttonsTableLayoutPanel.ResumeLayout(false);
			this.buttonsTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel dataSourceTableLayoutPanel;
		private System.Windows.Forms.TableLayoutPanel buttonsTableLayoutPanel;
		private System.Windows.Forms.Label dataSourceLabel;
		private System.Windows.Forms.ContainerControl containerControl;
		private System.Windows.Forms.Button advancedButton;
		private System.Windows.Forms.Panel separatorPanel;
		private System.Windows.Forms.Button testConnectionButton;
		private BlackbirdSql.Core.Controls.Widgets.ExceptionSafeButton acceptButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button changeDataSourceButton;
		private System.Windows.Forms.TextBox dataSourceTextBox;
		private System.Windows.Forms.CheckBox chkUpdateServerExplorer;
		private System.Windows.Forms.ToolTip dataProviderToolTip;
	}
}