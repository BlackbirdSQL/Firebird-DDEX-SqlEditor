// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.FileEncodingDialog

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Controls.Dialogs;


internal partial class FileEncodingDialog : Form, IVsSaveOptionsDlg
{

	public FileEncodingDialog()
	{
		InitializeComponent();
		if (!ControlUtils.IsGb18030Supported)
		{
			m_encodingCombo.Items.RemoveAt(C_Gb18030ItemIndex);
		}
	}



	internal Encoding Encoding => m_encodingIndex switch
	{
		0 => Encoding.UTF8,
		1 => Encoding.Unicode,
		2 => Encoding.ASCII,
		3 => Encoding.GetEncoding(54936),
		_ => Encoding.UTF8,
	};


	[ComVisible(false)]
	private class Win32WindowWrapper : IWin32Window
	{
		private readonly IntPtr handle;

		public IntPtr Handle => handle;

		public Win32WindowWrapper(IntPtr handle)
		{
			this.handle = handle;
		}
	}


	private const int C_DefaultEncodingComboIndex = 0;

	private const int C_Gb18030ItemIndex = 2;

	private int m_encodingIndex;




	int IVsSaveOptionsDlg.ShowSaveOptionsDlg(uint dwReserved, IntPtr hwndDlgParent, IntPtr fileName)
	{
		ResetUI();
		if (DialogResult.Cancel == ShowDialog(new Win32WindowWrapper(hwndDlgParent)))
		{
			return VSConstants.OLE_E_PROMPTSAVECANCELLED;
		}
		return 0;
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
