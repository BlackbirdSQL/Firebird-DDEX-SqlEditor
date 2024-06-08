// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.AdvancedInformation

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;



namespace BlackbirdSql.Sys.Controls;


partial class PrivacyConfirmationDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrivacyConfirmationDialog));
			this.para1 = new System.Windows.Forms.Label();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.btnYes = new System.Windows.Forms.Button();
			this.btnNo = new System.Windows.Forms.Button();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// para1
			// 
			resources.ApplyResources(this.para1, "para1");
			this.para1.Name = "para1";
			// 
			// checkBox1
			// 
			resources.ApplyResources(this.checkBox1, "checkBox1");
			this.checkBox1.Name = "checkBox1";
			// 
			// btnYes
			// 
			resources.ApplyResources(this.btnYes, "btnYes");
			this.btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.btnYes.Name = "btnYes";
			this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
			// 
			// btnNo
			// 
			resources.ApplyResources(this.btnNo, "btnNo");
			this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
			this.btnNo.Name = "btnNo";
			this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			resources.ApplyResources(this.dataGridView1, "dataGridView1");
			this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.RowHeadersVisible = false;
			// 
			// PrivacyConfirmationDialog
			// 
			this.AcceptButton = this.btnYes;
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnNo;
			this.ControlBox = false;
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.btnNo);
			this.Controls.Add(this.btnYes);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.para1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PrivacyConfirmationDialog";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PrivacyConfirmationDialog_FormClosed);
			this.Load += new System.EventHandler(this.PrivacyConfirmationDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

	}

	#endregion


	private Label para1;
	private CheckBox checkBox1;
	private Button btnYes;
	private Button btnNo;
	private DataGridView dataGridView1;


}
