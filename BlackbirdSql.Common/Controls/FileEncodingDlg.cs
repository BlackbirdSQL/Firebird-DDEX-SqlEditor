// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.FileEncodingDialog
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Controls;

public class FileEncodingDlg : Form, IVsSaveOptionsDlg
{
	private const int C_DefaultEncodingComboIndex = 0;

	private int m_encodingIndex;

	private const int C_Gb18030ItemIndex = 2;

	private Label m_encodingLabel;

	private ComboBox m_encodingCombo;

	private Button m_OKButton;

	private Button m_cancelButton;

#pragma warning disable CS0649 // Field 'FileEncodingDlg.components' is never assigned to, and will always have its default value null
	private readonly Container components;
#pragma warning restore CS0649 // Field 'FileEncodingDlg.components' is never assigned to, and will always have its default value null

	public Encoding Encoding => m_encodingIndex switch
	{
		0 => Encoding.UTF8,
		1 => Encoding.Unicode,
		2 => Encoding.ASCII,
		3 => Encoding.GetEncoding(54936),
		_ => Encoding.UTF8,
	};

	public FileEncodingDlg()
	{
		InitializeComponent();
		if (!ControlUtils.IsGb18030Supported)
		{
			m_encodingCombo.Items.RemoveAt(C_Gb18030ItemIndex);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	int IVsSaveOptionsDlg.ShowSaveOptionsDlg(uint dwReserved, IntPtr hwndDlgParent, IntPtr fileName)
	{
		ResetUI();
		if (DialogResult.Cancel == ShowDialog(new Win32WindowWrapper(hwndDlgParent)))
		{
			return VSConstants.OLE_E_PROMPTSAVECANCELLED;
		}
		return 0;
	}

	private void InitializeComponent()
	{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileEncodingDlg));
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
			// FileEncodingDlg
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
			this.Name = "FileEncodingDlg";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.ResumeLayout(false);

	}

	private void ResetUI()
	{
		m_encodingCombo.SelectedIndex = C_DefaultEncodingComboIndex;
	}

	private void CancelButton_Click(object sender, EventArgs e)
	{
		m_encodingIndex = C_DefaultEncodingComboIndex;
		DialogResult = DialogResult.Cancel;
	}

	private void OKButton_Click(object sender, EventArgs e)
	{
		m_encodingIndex = m_encodingCombo.SelectedIndex;
		DialogResult = DialogResult.OK;
	}
}
