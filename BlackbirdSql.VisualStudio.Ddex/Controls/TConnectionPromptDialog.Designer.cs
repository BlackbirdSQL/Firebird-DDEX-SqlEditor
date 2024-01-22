
using System.Windows.Forms;

namespace BlackbirdSql.VisualStudio.Ddex.Controls
{
    partial class TConnectionPromptDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TConnectionPromptDialog));
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
			resources.ApplyResources(this.headerLabel, "headerLabel");
			this.headerLabel.Name = "headerLabel";
			// 
			// headerPanel
			// 
			resources.ApplyResources(this.headerPanel, "headerPanel");
			this.headerPanel.Controls.Add(this.headerLabel);
			this.headerPanel.Name = "headerPanel";
			// 
			// mainTableLayoutPanel
			// 
			resources.ApplyResources(this.mainTableLayoutPanel, "mainTableLayoutPanel");
			this.mainTableLayoutPanel.Controls.Add(this.serverLabel, 0, 0);
			this.mainTableLayoutPanel.Controls.Add(this.serverTextBox, 1, 0);
			this.mainTableLayoutPanel.Controls.Add(this.databaseLabel, 0, 1);
			this.mainTableLayoutPanel.Controls.Add(this.databaseTextBox, 1, 1);
			this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
			// 
			// serverLabel
			// 
			resources.ApplyResources(this.serverLabel, "serverLabel");
			this.serverLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.serverLabel.Name = "serverLabel";
			// 
			// serverTextBox
			// 
			resources.ApplyResources(this.serverTextBox, "serverTextBox");
			this.serverTextBox.Name = "serverTextBox";
			this.serverTextBox.Leave += new System.EventHandler(this.TrimControlText);
			// 
			// databaseLabel
			// 
			resources.ApplyResources(this.databaseLabel, "databaseLabel");
			this.databaseLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.databaseLabel.Name = "databaseLabel";
			// 
			// databaseTextBox
			// 
			resources.ApplyResources(this.databaseTextBox, "databaseTextBox");
			this.databaseTextBox.Name = "databaseTextBox";
			// 
			// loginTableLayoutPanel
			// 
			resources.ApplyResources(this.loginTableLayoutPanel, "loginTableLayoutPanel");
			this.loginTableLayoutPanel.Controls.Add(this.userNameLabel, 0, 0);
			this.loginTableLayoutPanel.Controls.Add(this.userNameTextBox, 1, 0);
			this.loginTableLayoutPanel.Controls.Add(this.passwordLabel, 0, 1);
			this.loginTableLayoutPanel.Controls.Add(this.passwordTextBox, 1, 1);
			this.loginTableLayoutPanel.Controls.Add(this.savePasswordCheckBox, 1, 2);
			this.loginTableLayoutPanel.Name = "loginTableLayoutPanel";
			// 
			// userNameLabel
			// 
			resources.ApplyResources(this.userNameLabel, "userNameLabel");
			this.userNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.userNameLabel.Name = "userNameLabel";
			// 
			// userNameTextBox
			// 
			resources.ApplyResources(this.userNameTextBox, "userNameTextBox");
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.TextChanged += new System.EventHandler(this.SetOkButtonStatus);
			this.userNameTextBox.Leave += new System.EventHandler(this.TrimControlText);
			// 
			// passwordLabel
			// 
			resources.ApplyResources(this.passwordLabel, "passwordLabel");
			this.passwordLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.passwordLabel.Name = "passwordLabel";
			// 
			// passwordTextBox
			// 
			resources.ApplyResources(this.passwordTextBox, "passwordTextBox");
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.UseSystemPasswordChar = true;
			this.passwordTextBox.TextChanged += new System.EventHandler(this.SetOkButtonStatus);
			this.passwordTextBox.Leave += new System.EventHandler(this.ResetControlText);
			// 
			// savePasswordCheckBox
			// 
			resources.ApplyResources(this.savePasswordCheckBox, "savePasswordCheckBox");
			this.savePasswordCheckBox.Name = "savePasswordCheckBox";
			// 
			// buttonsTableLayoutPanel
			// 
			resources.ApplyResources(this.buttonsTableLayoutPanel, "buttonsTableLayoutPanel");
			this.buttonsTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
			this.buttonsTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
			this.buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
			// 
			// okButton
			// 
			resources.ApplyResources(this.okButton, "okButton");
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Name = "okButton";
			// 
			// cancelButton
			// 
			resources.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Name = "cancelButton";
			// 
			// overarchingTableLayoutPanel
			// 
			resources.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
			this.overarchingTableLayoutPanel.Controls.Add(this.headerPanel, 0, 0);
			this.overarchingTableLayoutPanel.Controls.Add(this.mainTableLayoutPanel, 0, 1);
			this.overarchingTableLayoutPanel.Controls.Add(this.authenticationTableLayoutPanel, 0, 2);
			this.overarchingTableLayoutPanel.Controls.Add(this.loginTableLayoutPanel, 0, 3);
			this.overarchingTableLayoutPanel.Controls.Add(this.buttonsTableLayoutPanel, 0, 4);
			this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
			// 
			// authenticationTableLayoutPanel
			// 
			resources.ApplyResources(this.authenticationTableLayoutPanel, "authenticationTableLayoutPanel");
			this.authenticationTableLayoutPanel.Controls.Add(this.authenticationTypeLabel, 0, 0);
			this.authenticationTableLayoutPanel.Name = "authenticationTableLayoutPanel";
			// 
			// authenticationTypeLabel
			// 
			resources.ApplyResources(this.authenticationTypeLabel, "authenticationTypeLabel");
			this.authenticationTypeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.authenticationTypeLabel.Name = "authenticationTypeLabel";
			// 
			// TConnectionPromptDialog
			// 
			this.AcceptButton = this.okButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.Controls.Add(this.overarchingTableLayoutPanel);
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TConnectionPromptDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
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
