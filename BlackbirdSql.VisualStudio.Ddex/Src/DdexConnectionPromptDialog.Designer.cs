/*
 *  Visual Studio DDEX Provider for FirebirdClient
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

using System.Windows.Forms;

namespace BlackbirdSql.VisualStudio.Ddex
{
    partial class DdexConnectionPromptDialog
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

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.headerLabel = new System.Windows.Forms.Label();
			this.headerPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.serverLabel = new System.Windows.Forms.Label();
			this.serverTextBox = new System.Windows.Forms.TextBox();
			this.databaseLabel = new System.Windows.Forms.Label();
			this.databaseTextBox = new System.Windows.Forms.TextBox();
			this.loginTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.userNameLabel = new System.Windows.Forms.Label();
			this.userNameTextBox = new System.Windows.Forms.TextBox();
			this.passwordLabel = new System.Windows.Forms.Label();
			this.passwordTextBox = new System.Windows.Forms.TextBox();
			this.savePasswordCheckBox = new System.Windows.Forms.CheckBox();
			this.buttonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.overarchingTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.authenticationTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.authenticationTypeLabel = new System.Windows.Forms.Label();
			this.headerPanel.SuspendLayout();
			this.mainTableLayoutPanel.SuspendLayout();
			this.loginTableLayoutPanel.SuspendLayout();
			this.buttonsTableLayoutPanel.SuspendLayout();
			this.overarchingTableLayoutPanel.SuspendLayout();
			this.authenticationTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// headerLabel
			// 
			this.headerLabel.AutoSize = true;
			this.headerLabel.Location = new System.Drawing.Point(3, 3);
			this.headerLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = new System.Drawing.Size(206, 13);
			this.headerLabel.TabIndex = 0;
			this.headerLabel.Text = "This connection requires more information.";
			// 
			// headerPanel
			// 
			this.headerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.headerPanel.AutoSize = true;
			this.headerPanel.Controls.Add(this.headerLabel);
			this.headerPanel.Location = new System.Drawing.Point(0, 0);
			this.headerPanel.Margin = new System.Windows.Forms.Padding(0);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.Size = new System.Drawing.Size(284, 22);
			this.headerPanel.TabIndex = 0;
			// 
			// mainTableLayoutPanel
			// 
			this.mainTableLayoutPanel.AutoSize = true;
			this.mainTableLayoutPanel.ColumnCount = 2;
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainTableLayoutPanel.Controls.Add(this.serverLabel, 0, 0);
			this.mainTableLayoutPanel.Controls.Add(this.serverTextBox, 1, 0);
			this.mainTableLayoutPanel.Controls.Add(this.databaseLabel, 0, 1);
			this.mainTableLayoutPanel.Controls.Add(this.databaseTextBox, 1, 1);
			this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTableLayoutPanel.Location = new System.Drawing.Point(3, 25);
			this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
			this.mainTableLayoutPanel.RowCount = 2;
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.Size = new System.Drawing.Size(278, 52);
			this.mainTableLayoutPanel.TabIndex = 1;
			// 
			// serverLabel
			// 
			this.serverLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.serverLabel.AutoSize = true;
			this.serverLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.serverLabel.Location = new System.Drawing.Point(0, 6);
			this.serverLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.serverLabel.Name = "serverLabel";
			this.serverLabel.Size = new System.Drawing.Size(41, 13);
			this.serverLabel.TabIndex = 0;
			this.serverLabel.Text = "S&erver:";
			// 
			// serverTextBox
			// 
			this.serverTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.serverTextBox.Location = new System.Drawing.Point(62, 3);
			this.serverTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.serverTextBox.Name = "serverTextBox";
			this.serverTextBox.Size = new System.Drawing.Size(216, 20);
			this.serverTextBox.TabIndex = 1;
			this.serverTextBox.Leave += new System.EventHandler(this.TrimControlText);
			// 
			// databaseLabel
			// 
			this.databaseLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.databaseLabel.AutoSize = true;
			this.databaseLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.databaseLabel.Location = new System.Drawing.Point(0, 32);
			this.databaseLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.databaseLabel.Name = "databaseLabel";
			this.databaseLabel.Size = new System.Drawing.Size(56, 13);
			this.databaseLabel.TabIndex = 2;
			this.databaseLabel.Text = "&Database:";
			// 
			// databaseTextBox
			// 
			this.databaseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.databaseTextBox.Location = new System.Drawing.Point(62, 29);
			this.databaseTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.databaseTextBox.Name = "databaseTextBox";
			this.databaseTextBox.Size = new System.Drawing.Size(216, 20);
			this.databaseTextBox.TabIndex = 3;
			// 
			// loginTableLayoutPanel
			// 
			this.loginTableLayoutPanel.AutoSize = true;
			this.loginTableLayoutPanel.ColumnCount = 2;
			this.loginTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.loginTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.loginTableLayoutPanel.Controls.Add(this.userNameLabel, 0, 0);
			this.loginTableLayoutPanel.Controls.Add(this.userNameTextBox, 1, 0);
			this.loginTableLayoutPanel.Controls.Add(this.passwordLabel, 0, 1);
			this.loginTableLayoutPanel.Controls.Add(this.passwordTextBox, 1, 1);
			this.loginTableLayoutPanel.Controls.Add(this.savePasswordCheckBox, 1, 2);
			this.loginTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.loginTableLayoutPanel.Location = new System.Drawing.Point(24, 120);
			this.loginTableLayoutPanel.Margin = new System.Windows.Forms.Padding(24, 0, 3, 3);
			this.loginTableLayoutPanel.Name = "loginTableLayoutPanel";
			this.loginTableLayoutPanel.RowCount = 3;
			this.loginTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.loginTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.loginTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.loginTableLayoutPanel.Size = new System.Drawing.Size(257, 70);
			this.loginTableLayoutPanel.TabIndex = 4;
			// 
			// userNameLabel
			// 
			this.userNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.userNameLabel.AutoSize = true;
			this.userNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.userNameLabel.Location = new System.Drawing.Point(0, 6);
			this.userNameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.userNameLabel.Name = "userNameLabel";
			this.userNameLabel.Size = new System.Drawing.Size(61, 13);
			this.userNameLabel.TabIndex = 0;
			this.userNameLabel.Text = "&User name:";
			// 
			// userNameTextBox
			// 
			this.userNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.userNameTextBox.Location = new System.Drawing.Point(67, 3);
			this.userNameTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.Size = new System.Drawing.Size(190, 20);
			this.userNameTextBox.TabIndex = 1;
			this.userNameTextBox.TextChanged += new System.EventHandler(this.SetOkButtonStatus);
			this.userNameTextBox.Leave += new System.EventHandler(this.TrimControlText);
			// 
			// passwordLabel
			// 
			this.passwordLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.passwordLabel.AutoSize = true;
			this.passwordLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.passwordLabel.Location = new System.Drawing.Point(0, 32);
			this.passwordLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.passwordLabel.Name = "passwordLabel";
			this.passwordLabel.Size = new System.Drawing.Size(56, 13);
			this.passwordLabel.TabIndex = 2;
			this.passwordLabel.Text = "&Password:";
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.passwordTextBox.Location = new System.Drawing.Point(67, 29);
			this.passwordTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.PasswordChar = '●';
			this.passwordTextBox.Size = new System.Drawing.Size(190, 20);
			this.passwordTextBox.TabIndex = 3;
			this.passwordTextBox.UseSystemPasswordChar = true;
			this.passwordTextBox.Leave += new System.EventHandler(this.ResetControlText);
			// 
			// savePasswordCheckBox
			// 
			this.savePasswordCheckBox.AutoSize = true;
			this.savePasswordCheckBox.Enabled = false;
			this.savePasswordCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.savePasswordCheckBox.Location = new System.Drawing.Point(67, 52);
			this.savePasswordCheckBox.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.savePasswordCheckBox.Name = "savePasswordCheckBox";
			this.savePasswordCheckBox.Size = new System.Drawing.Size(121, 18);
			this.savePasswordCheckBox.TabIndex = 4;
			this.savePasswordCheckBox.Text = "&Save my password";
			// 
			// buttonsTableLayoutPanel
			// 
			this.buttonsTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonsTableLayoutPanel.AutoSize = true;
			this.buttonsTableLayoutPanel.ColumnCount = 2;
			this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.buttonsTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
			this.buttonsTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
			this.buttonsTableLayoutPanel.Location = new System.Drawing.Point(125, 217);
			this.buttonsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
			this.buttonsTableLayoutPanel.RowCount = 1;
			this.buttonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.buttonsTableLayoutPanel.Size = new System.Drawing.Size(156, 23);
			this.buttonsTableLayoutPanel.TabIndex = 5;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.AutoSize = true;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(0, 0);
			this.okButton.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.okButton.MinimumSize = new System.Drawing.Size(75, 23);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.AutoSize = true;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(81, 0);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			// 
			// overarchingTableLayoutPanel
			// 
			this.overarchingTableLayoutPanel.AutoSize = true;
			this.overarchingTableLayoutPanel.ColumnCount = 1;
			this.overarchingTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.overarchingTableLayoutPanel.Controls.Add(this.headerPanel, 0, 0);
			this.overarchingTableLayoutPanel.Controls.Add(this.mainTableLayoutPanel, 0, 1);
			this.overarchingTableLayoutPanel.Controls.Add(this.authenticationTableLayoutPanel, 0, 2);
			this.overarchingTableLayoutPanel.Controls.Add(this.loginTableLayoutPanel, 0, 3);
			this.overarchingTableLayoutPanel.Controls.Add(this.buttonsTableLayoutPanel, 0, 4);
			this.overarchingTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.overarchingTableLayoutPanel.Location = new System.Drawing.Point(9, 9);
			this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
			this.overarchingTableLayoutPanel.RowCount = 5;
			this.overarchingTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.overarchingTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.overarchingTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.overarchingTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.overarchingTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.overarchingTableLayoutPanel.Size = new System.Drawing.Size(284, 243);
			this.overarchingTableLayoutPanel.TabIndex = 0;
			// 
			// authenticationTableLayoutPanel
			// 
			this.authenticationTableLayoutPanel.ColumnCount = 2;
			this.authenticationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.authenticationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.authenticationTableLayoutPanel.Controls.Add(this.authenticationTypeLabel, 0, 0);
			this.authenticationTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.authenticationTableLayoutPanel.Location = new System.Drawing.Point(3, 83);
			this.authenticationTableLayoutPanel.Name = "authenticationTableLayoutPanel";
			this.authenticationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.authenticationTableLayoutPanel.Size = new System.Drawing.Size(278, 34);
			this.authenticationTableLayoutPanel.TabIndex = 2;
			// 
			// authenticationTypeLabel
			// 
			this.authenticationTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.authenticationTypeLabel.AutoSize = true;
			this.authenticationTypeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.authenticationTypeLabel.Location = new System.Drawing.Point(0, 10);
			this.authenticationTypeLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.authenticationTypeLabel.Name = "authenticationTypeLabel";
			this.authenticationTypeLabel.Size = new System.Drawing.Size(75, 13);
			this.authenticationTypeLabel.TabIndex = 0;
			this.authenticationTypeLabel.Text = "&Authentication";
			// 
			// DdexConnectionPromptDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(302, 261);
			this.Controls.Add(this.overarchingTableLayoutPanel);
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(318, 283);
			this.Name = "DdexConnectionPromptDialog";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Connect to Firebird Server";
			this.headerPanel.ResumeLayout(false);
			this.headerPanel.PerformLayout();
			this.mainTableLayoutPanel.ResumeLayout(false);
			this.mainTableLayoutPanel.PerformLayout();
			this.loginTableLayoutPanel.ResumeLayout(false);
			this.loginTableLayoutPanel.PerformLayout();
			this.buttonsTableLayoutPanel.ResumeLayout(false);
			this.buttonsTableLayoutPanel.PerformLayout();
			this.overarchingTableLayoutPanel.ResumeLayout(false);
			this.overarchingTableLayoutPanel.PerformLayout();
			this.authenticationTableLayoutPanel.ResumeLayout(false);
			this.authenticationTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Label headerLabel;
		private FlowLayoutPanel headerPanel;
		private TableLayoutPanel mainTableLayoutPanel;
		private Label serverLabel;
		private TextBox serverTextBox;
		private Label databaseLabel;
		private TextBox databaseTextBox;
		private TableLayoutPanel loginTableLayoutPanel;
		private Label userNameLabel;
		private TextBox userNameTextBox;
		private Label passwordLabel;
		private TextBox passwordTextBox;
		private CheckBox savePasswordCheckBox;
		private TableLayoutPanel buttonsTableLayoutPanel;
		private Button okButton;
		private Button cancelButton;
		private TableLayoutPanel overarchingTableLayoutPanel;
		private TableLayoutPanel authenticationTableLayoutPanel;
		private Label authenticationTypeLabel;
	}
}
