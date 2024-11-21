// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.AddPropertyDlg

using System.Windows.Forms;



namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;


partial class VxbAddPropertyDialog
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VxbAddPropertyDialog));
		this.propertyLabel = new System.Windows.Forms.Label();
		this.propertyTextBox = new System.Windows.Forms.TextBox();
		this.buttonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.buttonsTableLayoutPanel.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this.propertyLabel, "propertyLabel");
		this.propertyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.propertyLabel.Name = "propertyLabel";
		resources.ApplyResources(this.propertyTextBox, "propertyTextBox");
		this.propertyTextBox.Name = "propertyTextBox";
		this.propertyTextBox.TextChanged += new System.EventHandler(SetOkButtonStatus);
		resources.ApplyResources(this.buttonsTableLayoutPanel, "buttonsTableLayoutPanel");
		this.buttonsTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
		this.buttonsTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
		this.buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
		resources.ApplyResources(this.okButton, "okButton");
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.MinimumSize = new System.Drawing.Size(75, 23);
		this.okButton.Name = "okButton";
		resources.ApplyResources(this.cancelButton, "cancelButton");
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
		this.cancelButton.Name = "cancelButton";
		base.AcceptButton = this.okButton;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.Controls.Add(this.buttonsTableLayoutPanel);
		base.Controls.Add(this.propertyTextBox);
		base.Controls.Add(this.propertyLabel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.HelpButton = true;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "AddPropertyDlg";
		base.ShowInTaskbar = false;
		this.buttonsTableLayoutPanel.ResumeLayout(false);
		this.buttonsTableLayoutPanel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	#endregion

	private Label propertyLabel;
	private TextBox propertyTextBox;
	private TableLayoutPanel buttonsTableLayoutPanel;
	private Button okButton;
	private Button cancelButton;


}
