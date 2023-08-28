// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.FileEncodingDialog
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Properties;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Common.Controls;

public class FileEncodingDialog : Form, IVsSaveOptionsDlg
{
	private const int C_DefaultEncodingComboIndex = 0;

	private int m_encodingIndex;

	private const int C_Gb18030ItemIndex = 2;

	private Label m_encodingLabel;

	private ComboBox m_encodingCombo;

	private Button m_OKButton;

	private Button m_cancelButton;

#pragma warning disable CS0649 // Field 'FileEncodingDialog.components' is never assigned to, and will always have its default value null
	private readonly Container components;
#pragma warning restore CS0649 // Field 'FileEncodingDialog.components' is never assigned to, and will always have its default value null

	public Encoding Encoding => m_encodingIndex switch
	{
		0 => Encoding.UTF8,
		1 => Encoding.Unicode,
		2 => Encoding.ASCII,
		3 => Encoding.GetEncoding(54936),
		_ => Encoding.UTF8,
	};

	public FileEncodingDialog()
	{
		InitializeComponent();
		Icon = ControlsResources.FileEncodingDialog;
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
			return -2147221492;
		}
		return 0;
	}

	private void InitializeComponent()
	{
		ResourceManager resources = new ResourceManager(typeof(FileEncodingDialog));
		m_encodingCombo = new ComboBox();
		m_OKButton = new Button();
		m_cancelButton = new Button();
		m_encodingLabel = new Label();
		SuspendLayout();
		m_encodingCombo.AccessibleDescription = resources.GetString("m_encodingCombo.AccessibleDescription");
		m_encodingCombo.AccessibleName = resources.GetString("m_encodingCombo.AccessibleName");
		m_encodingCombo.Anchor = (AnchorStyles)resources.GetObject("m_encodingCombo.Anchor");
		m_encodingCombo.BackgroundImage = (Image)resources.GetObject("m_encodingCombo.BackgroundImage");
		m_encodingCombo.Dock = (DockStyle)resources.GetObject("m_encodingCombo.Dock");
		m_encodingCombo.DropDownStyle = ComboBoxStyle.DropDownList;
		m_encodingCombo.DropDownWidth = 136;
		m_encodingCombo.Enabled = (bool)resources.GetObject("m_encodingCombo.Enabled");
		m_encodingCombo.Font = (Font)resources.GetObject("m_encodingCombo.Font");
		m_encodingCombo.ImeMode = (ImeMode)resources.GetObject("m_encodingCombo.ImeMode");
		m_encodingCombo.IntegralHeight = (bool)resources.GetObject("m_encodingCombo.IntegralHeight");
		m_encodingCombo.ItemHeight = (int)resources.GetObject("m_encodingCombo.ItemHeight");
		m_encodingCombo.Items.AddRange(new object[4]
		{
			resources.GetString("m_encodingCombo.Items"),
			resources.GetString("m_encodingCombo.Items1"),
			resources.GetString("m_encodingCombo.Items2"),
			resources.GetString("m_encodingCombo.Items3")
		});
		m_encodingCombo.Location = (Point)resources.GetObject("m_encodingCombo.Location");
		m_encodingCombo.MaxDropDownItems = (int)resources.GetObject("m_encodingCombo.MaxDropDownItems");
		m_encodingCombo.MaxLength = (int)resources.GetObject("m_encodingCombo.MaxLength");
		m_encodingCombo.Name = "m_encodingCombo";
		m_encodingCombo.RightToLeft = (RightToLeft)resources.GetObject("m_encodingCombo.RightToLeft");
		m_encodingCombo.Size = (Size)resources.GetObject("m_encodingCombo.Size");
		m_encodingCombo.TabIndex = (int)resources.GetObject("m_encodingCombo.TabIndex");
		m_encodingCombo.Text = resources.GetString("m_encodingCombo.Text");
		m_encodingCombo.Visible = (bool)resources.GetObject("m_encodingCombo.Visible");
		m_OKButton.AccessibleDescription = resources.GetString("m_OKButton.AccessibleDescription");
		m_OKButton.AccessibleName = resources.GetString("m_OKButton.AccessibleName");
		m_OKButton.Anchor = (AnchorStyles)resources.GetObject("m_OKButton.Anchor");
		m_OKButton.BackgroundImage = (Image)resources.GetObject("m_OKButton.BackgroundImage");
		m_OKButton.Dock = (DockStyle)resources.GetObject("m_OKButton.Dock");
		m_OKButton.Enabled = (bool)resources.GetObject("m_OKButton.Enabled");
		m_OKButton.FlatStyle = (FlatStyle)resources.GetObject("m_OKButton.FlatStyle");
		m_OKButton.Font = (Font)resources.GetObject("m_OKButton.Font");
		m_OKButton.Image = (Image)resources.GetObject("m_OKButton.Image");
		m_OKButton.ImageAlign = (ContentAlignment)resources.GetObject("m_OKButton.ImageAlign");
		m_OKButton.ImageIndex = (int)resources.GetObject("m_OKButton.ImageIndex");
		m_OKButton.ImeMode = (ImeMode)resources.GetObject("m_OKButton.ImeMode");
		m_OKButton.Location = (Point)resources.GetObject("m_OKButton.Location");
		m_OKButton.Name = "m_OKButton";
		m_OKButton.RightToLeft = (RightToLeft)resources.GetObject("m_OKButton.RightToLeft");
		m_OKButton.Size = (Size)resources.GetObject("m_OKButton.Size");
		m_OKButton.TabIndex = (int)resources.GetObject("m_OKButton.TabIndex");
		m_OKButton.Text = resources.GetString("m_OKButton.Text");
		m_OKButton.TextAlign = (ContentAlignment)resources.GetObject("m_OKButton.TextAlign");
		m_OKButton.Visible = (bool)resources.GetObject("m_OKButton.Visible");
		m_OKButton.Click += new EventHandler(OKButton_Click);
		m_cancelButton.AccessibleDescription = resources.GetString("m_cancelButton.AccessibleDescription");
		m_cancelButton.AccessibleName = resources.GetString("m_cancelButton.AccessibleName");
		m_cancelButton.Anchor = (AnchorStyles)resources.GetObject("m_cancelButton.Anchor");
		m_cancelButton.BackgroundImage = (Image)resources.GetObject("m_cancelButton.BackgroundImage");
		m_cancelButton.DialogResult = DialogResult.Cancel;
		m_cancelButton.Dock = (DockStyle)resources.GetObject("m_cancelButton.Dock");
		m_cancelButton.Enabled = (bool)resources.GetObject("m_cancelButton.Enabled");
		m_cancelButton.FlatStyle = (FlatStyle)resources.GetObject("m_cancelButton.FlatStyle");
		m_cancelButton.Font = (Font)resources.GetObject("m_cancelButton.Font");
		m_cancelButton.Image = (Image)resources.GetObject("m_cancelButton.Image");
		m_cancelButton.ImageAlign = (ContentAlignment)resources.GetObject("m_cancelButton.ImageAlign");
		m_cancelButton.ImageIndex = (int)resources.GetObject("m_cancelButton.ImageIndex");
		m_cancelButton.ImeMode = (ImeMode)resources.GetObject("m_cancelButton.ImeMode");
		m_cancelButton.Location = (Point)resources.GetObject("m_cancelButton.Location");
		m_cancelButton.Name = "m_cancelButton";
		m_cancelButton.RightToLeft = (RightToLeft)resources.GetObject("m_cancelButton.RightToLeft");
		m_cancelButton.Size = (Size)resources.GetObject("m_cancelButton.Size");
		m_cancelButton.TabIndex = (int)resources.GetObject("m_cancelButton.TabIndex");
		m_cancelButton.Text = resources.GetString("m_cancelButton.Text");
		m_cancelButton.TextAlign = (ContentAlignment)resources.GetObject("m_cancelButton.TextAlign");
		m_cancelButton.Visible = (bool)resources.GetObject("m_cancelButton.Visible");
		m_cancelButton.Click += new EventHandler(CancelButton_Click);
		m_encodingLabel.AccessibleDescription = resources.GetString("m_encodingLabel.AccessibleDescription");
		m_encodingLabel.AccessibleName = resources.GetString("m_encodingLabel.AccessibleName");
		m_encodingLabel.Anchor = (AnchorStyles)resources.GetObject("m_encodingLabel.Anchor");
		m_encodingLabel.AutoSize = (bool)resources.GetObject("m_encodingLabel.AutoSize");
		m_encodingLabel.Dock = (DockStyle)resources.GetObject("m_encodingLabel.Dock");
		m_encodingLabel.Enabled = (bool)resources.GetObject("m_encodingLabel.Enabled");
		m_encodingLabel.Font = (Font)resources.GetObject("m_encodingLabel.Font");
		m_encodingLabel.Image = (Image)resources.GetObject("m_encodingLabel.Image");
		m_encodingLabel.ImageAlign = (ContentAlignment)resources.GetObject("m_encodingLabel.ImageAlign");
		m_encodingLabel.ImageIndex = (int)resources.GetObject("m_encodingLabel.ImageIndex");
		m_encodingLabel.ImeMode = (ImeMode)resources.GetObject("m_encodingLabel.ImeMode");
		m_encodingLabel.Location = (Point)resources.GetObject("m_encodingLabel.Location");
		m_encodingLabel.Name = "m_encodingLabel";
		m_encodingLabel.RightToLeft = (RightToLeft)resources.GetObject("m_encodingLabel.RightToLeft");
		m_encodingLabel.Size = (Size)resources.GetObject("m_encodingLabel.Size");
		m_encodingLabel.TabIndex = (int)resources.GetObject("m_encodingLabel.TabIndex");
		m_encodingLabel.Text = resources.GetString("m_encodingLabel.Text");
		m_encodingLabel.TextAlign = (ContentAlignment)resources.GetObject("m_encodingLabel.TextAlign");
		m_encodingLabel.Visible = (bool)resources.GetObject("m_encodingLabel.Visible");
		AcceptButton = m_OKButton;
		AccessibleDescription = resources.GetString("$this.AccessibleDescription");
		AccessibleName = resources.GetString("$this.AccessibleName");
		AutoScaleBaseSize = (Size)resources.GetObject("$this.AutoScaleBaseSize");
		AutoScroll = (bool)resources.GetObject("$this.AutoScroll");
		AutoScrollMargin = (Size)resources.GetObject("$this.AutoScrollMargin");
		AutoScrollMinSize = (Size)resources.GetObject("$this.AutoScrollMinSize");
		BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
		CancelButton = m_cancelButton;
		ClientSize = (Size)resources.GetObject("$this.ClientSize");
		Controls.Add(m_cancelButton);
		Controls.Add(m_OKButton);
		Controls.Add(m_encodingCombo);
		Controls.Add(m_encodingLabel);
		Enabled = (bool)resources.GetObject("$this.Enabled");
		Font = (Font)resources.GetObject("$this.Font");
		FormBorderStyle = FormBorderStyle.FixedDialog;
		Icon = (Icon)resources.GetObject("$this.Icon");
		ImeMode = (ImeMode)resources.GetObject("$this.ImeMode");
		Location = (Point)resources.GetObject("$this.Location");
		MaximizeBox = false;
		MaximumSize = (Size)resources.GetObject("$this.MaximumSize");
		MinimizeBox = false;
		MinimumSize = (Size)resources.GetObject("$this.MinimumSize");
		Name = "FileEncodingDialog";
		RightToLeft = (RightToLeft)resources.GetObject("$this.RightToLeft");
		ShowInTaskbar = false;
		SizeGripStyle = SizeGripStyle.Hide;
		StartPosition = (FormStartPosition)resources.GetObject("$this.StartPosition");
		Text = resources.GetString("$this.Text");
		ResumeLayout(false);
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
