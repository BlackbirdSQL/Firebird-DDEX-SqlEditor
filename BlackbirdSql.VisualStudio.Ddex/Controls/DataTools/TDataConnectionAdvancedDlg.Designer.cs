// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.AdvancedInformation

using System.ComponentModel;
using System.Windows.Forms;



namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;


partial class TDataConnectionAdvancedDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TDataConnectionAdvancedDlg));
		this.propertyGrid = new BlackbirdSql.VisualStudio.Ddex.Controls.DataTools.TDataConnectionAdvancedDlg.TiSpecializedPropertyGrid();
		this.textBox = new System.Windows.Forms.TextBox();
		this.buttonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.buttonsTableLayoutPanel.SuspendLayout();
		this.SuspendLayout();
		// 
		// propertyGrid
		// 
		resources.ApplyResources(this.propertyGrid, "propertyGrid");
		this.propertyGrid.CommandsActiveLinkColor = System.Drawing.SystemColors.ActiveCaption;
		this.propertyGrid.CommandsDisabledLinkColor = System.Drawing.SystemColors.ControlDark;
		this.propertyGrid.CommandsLinkColor = System.Drawing.SystemColors.ActiveCaption;
		this.propertyGrid.Name = "propertyGrid";
		this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.SetTextBox);
		// 
		// textBox
		// 
		resources.ApplyResources(this.textBox, "textBox");
		this.textBox.Name = "textBox";
		this.textBox.ReadOnly = true;
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
		this.cancelButton.Click += new System.EventHandler(this.RevertProperties);
		// 
		// TDataConnectionAdvancedDlg
		// 
		this.AcceptButton = this.okButton;
		resources.ApplyResources(this, "$this");
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.CancelButton = this.cancelButton;
		this.Controls.Add(this.buttonsTableLayoutPanel);
		this.Controls.Add(this.textBox);
		this.Controls.Add(this.propertyGrid);
		this.HelpButton = true;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = "TDataConnectionAdvancedDlg";
		this.ShowIcon = false;
		this.ShowInTaskbar = false;
		this.buttonsTableLayoutPanel.ResumeLayout(false);
		this.buttonsTableLayoutPanel.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	private TextBox textBox;
	private TableLayoutPanel buttonsTableLayoutPanel;
	private Button okButton;
	private Button cancelButton;

}
