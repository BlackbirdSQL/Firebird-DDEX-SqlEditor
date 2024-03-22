// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.FileEncodingDialog



namespace BlackbirdSql.Common.Controls.Dialogs;


partial class FileEncodingDialog
{

	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}



	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileEncodingDialog));
		this.m_encodingCombo = new System.Windows.Forms.ComboBox();
		this.m_OKButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_encodingLabel = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// m_encodingCombo
		// 
		this.m_encodingCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_encodingCombo.DropDownWidth = 136;
		resources.ApplyResources(this.m_encodingCombo, "m_encodingCombo");
		this.m_encodingCombo.Items.AddRange(new object[] {
			resources.GetString("m_encodingCombo.Items"),
			resources.GetString("m_encodingCombo.Items1"),
			resources.GetString("m_encodingCombo.Items2"),
			resources.GetString("m_encodingCombo.Items3")});
		this.m_encodingCombo.Name = "m_encodingCombo";
		// 
		// m_OKButton
		// 
		resources.ApplyResources(this.m_OKButton, "m_OKButton");
		this.m_OKButton.Name = "m_OKButton";
		this.m_OKButton.Click += new System.EventHandler(this.OKButton_Click);
		// 
		// m_cancelButton
		// 
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
		// 
		// m_encodingLabel
		// 
		resources.ApplyResources(this.m_encodingLabel, "m_encodingLabel");
		this.m_encodingLabel.Name = "m_encodingLabel";
		// 
		// FileEncodingDialog
		// 
		this.AcceptButton = this.m_OKButton;
		resources.ApplyResources(this, "$this");
		this.CancelButton = this.m_cancelButton;
		this.Controls.Add(this.m_cancelButton);
		this.Controls.Add(this.m_OKButton);
		this.Controls.Add(this.m_encodingCombo);
		this.Controls.Add(this.m_encodingLabel);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = "FileEncodingDialog";
		this.ShowInTaskbar = false;
		this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		this.ResumeLayout(false);

	}


	private System.Windows.Forms.Label m_encodingLabel;
	private System.Windows.Forms.ComboBox m_encodingCombo;
	private System.Windows.Forms.Button m_OKButton;
	private System.Windows.Forms.Button m_cancelButton;

}
