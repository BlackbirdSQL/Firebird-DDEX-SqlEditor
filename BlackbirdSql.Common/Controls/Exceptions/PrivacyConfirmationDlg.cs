// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.PrivacyConfirmation

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Properties;


namespace BlackbirdSql.Common.Controls.Exceptions;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class PrivacyConfirmationDlg : Form
{
	private IContainer components;

	private DataTable m_table = new DataTable("table");

	private Label para1;

	private CheckBox checkBox1;

	private Button btnYes;

	private Button btnNo;

	private string m_url;

	// private RegistryKey m_key;

	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	private DataGridView dataGridView1;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.PrivacyConfirmation", typeof(PrivacyConfirmationDlg).Assembly);
			}

			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static AnchorStyles btnNo_Anchor => (AnchorStyles)ResourceManager.GetObject("btnNo.Anchor", resourceCulture);

	public static ImeMode btnNo_ImeMode => (ImeMode)ResourceManager.GetObject("btnNo.ImeMode", resourceCulture);

	public static Point btnNo_Location => (Point)ResourceManager.GetObject("btnNo.Location", resourceCulture);

	public static Size btnNo_Size => (Size)ResourceManager.GetObject("btnNo.Size", resourceCulture);

	public static int btnNo_TabIndex => (int)ResourceManager.GetObject("btnNo.TabIndex", resourceCulture);

	public static string btnNo_Text => ResourceManager.GetString("btnNo.Text", resourceCulture);

	public static AnchorStyles btnYes_Anchor => (AnchorStyles)ResourceManager.GetObject("btnYes.Anchor", resourceCulture);

	public static ImeMode btnYes_ImeMode => (ImeMode)ResourceManager.GetObject("btnYes.ImeMode", resourceCulture);

	public static Point btnYes_Location => (Point)ResourceManager.GetObject("btnYes.Location", resourceCulture);

	public static Size btnYes_Size => (Size)ResourceManager.GetObject("btnYes.Size", resourceCulture);

	public static int btnYes_TabIndex => (int)ResourceManager.GetObject("btnYes.TabIndex", resourceCulture);

	public static string btnYes_Text => ResourceManager.GetString("btnYes.Text", resourceCulture);

	public static AnchorStyles checkBox1_Anchor => (AnchorStyles)ResourceManager.GetObject("checkBox1.Anchor", resourceCulture);

	public static ContentAlignment checkBox1_CheckAlign => (ContentAlignment)ResourceManager.GetObject("checkBox1.CheckAlign", resourceCulture);

	public static ImeMode checkBox1_ImeMode => (ImeMode)ResourceManager.GetObject("checkBox1.ImeMode", resourceCulture);

	public static Point checkBox1_Location => (Point)ResourceManager.GetObject("checkBox1.Location", resourceCulture);

	public static Size checkBox1_Size => (Size)ResourceManager.GetObject("checkBox1.Size", resourceCulture);

	public static int checkBox1_TabIndex => (int)ResourceManager.GetObject("checkBox1.TabIndex", resourceCulture);

	public static string checkBox1_Text => ResourceManager.GetString("checkBox1.Text", resourceCulture);

	public static ContentAlignment checkBox1_TextAlign => (ContentAlignment)ResourceManager.GetObject("checkBox1.TextAlign", resourceCulture);

	public static AnchorStyles grid_Anchor => (AnchorStyles)ResourceManager.GetObject("grid.Anchor", resourceCulture);

	public static Font grid_Font => (Font)ResourceManager.GetObject("grid.Font", resourceCulture);

	public static Point grid_Location => (Point)ResourceManager.GetObject("grid.Location", resourceCulture);

	public static Size grid_Size => (Size)ResourceManager.GetObject("grid.Size", resourceCulture);

	public static int grid_TabIndex => (int)ResourceManager.GetObject("grid.TabIndex", resourceCulture);

	public static AnchorStyles para1_Anchor => (AnchorStyles)ResourceManager.GetObject("para1.Anchor", resourceCulture);

	public static ImeMode para1_ImeMode => (ImeMode)ResourceManager.GetObject("para1.ImeMode", resourceCulture);

	public static Point para1_Location => (Point)ResourceManager.GetObject("para1.Location", resourceCulture);

	public static Size para1_Size => (Size)ResourceManager.GetObject("para1.Size", resourceCulture);

	public static int para1_TabIndex => (int)ResourceManager.GetObject("para1.TabIndex", resourceCulture);

	public static string para1_Text => ResourceManager.GetString("para1.Text", resourceCulture);

	public PrivacyConfirmationDlg(string title, string url)
	{
		InitializeComponent();
		Text = title;
		m_url = url;
		m_table.Locale = CultureInfo.CurrentCulture;
	}

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
		components = new Container();
		para1 = new Label();
		checkBox1 = new CheckBox();
		btnYes = new Button();
		btnNo = new Button();
		dataGridView1 = new DataGridView();
		((ISupportInitialize)dataGridView1).BeginInit();
		SuspendLayout();
		// 
		// para1
		// 
		para1.Anchor = AnchorStyles.Top | AnchorStyles.Left
		| AnchorStyles.Right;
		para1.ImeMode = ImeMode.NoControl;
		para1.Location = new Point(8, 6);
		para1.Name = "para1";
		para1.Size = new Size(655, 22);
		para1.TabIndex = 0;
		para1.Text = "&To get help for this message, you must send information across the Internet. Do " +
"you want to send the following information?";
		// 
		// checkBox1
		// 
		checkBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
		| AnchorStyles.Right;
		checkBox1.CheckAlign = ContentAlignment.TopLeft;
		checkBox1.ImeMode = ImeMode.NoControl;
		checkBox1.Location = new Point(11, 267);
		checkBox1.Name = "checkBox1";
		checkBox1.Size = new Size(655, 24);
		checkBox1.TabIndex = 4;
		checkBox1.Text = "&Do not ask me this again (always send the information)";
		checkBox1.TextAlign = ContentAlignment.TopLeft;
		// 
		// btnYes
		// 
		btnYes.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		btnYes.DialogResult = DialogResult.Yes;
		btnYes.ImeMode = ImeMode.NoControl;
		btnYes.Location = new Point(505, 288);
		btnYes.Name = "btnYes";
		btnYes.Size = new Size(75, 23);
		btnYes.TabIndex = 1;
		btnYes.Text = "&Yes";
		btnYes.Click += new EventHandler(btnYes_Click);
		// 
		// btnNo
		// 
		btnNo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		btnNo.DialogResult = DialogResult.No;
		btnNo.ImeMode = ImeMode.NoControl;
		btnNo.Location = new Point(586, 288);
		btnNo.Name = "btnNo";
		btnNo.Size = new Size(75, 23);
		btnNo.TabIndex = 2;
		btnNo.Text = "&No";
		btnNo.Click += new EventHandler(btnNo_Click);
		// 
		// dataGridView1
		// 
		dataGridView1.AllowUserToAddRows = false;
		dataGridView1.AllowUserToDeleteRows = false;
		dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
		| AnchorStyles.Left
		| AnchorStyles.Right;
		dataGridView1.BackgroundColor = SystemColors.Window;
		dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		dataGridView1.Location = new Point(12, 31);
		dataGridView1.Name = "dataGridView1";
		dataGridView1.ReadOnly = true;
		dataGridView1.RowHeadersVisible = false;
		dataGridView1.Size = new Size(649, 230);
		dataGridView1.TabIndex = 5;
		// 
		// PrivacyConfirmation
		// 
		AcceptButton = btnYes;
		AccessibleRole = AccessibleRole.Dialog;
		AutoScaleDimensions = new SizeF(6F, 13F);
		AutoScaleMode = AutoScaleMode.Font;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		CancelButton = btnNo;
		ClientSize = new Size(673, 329);
		ControlBox = false;
		Controls.Add(dataGridView1);
		Controls.Add(btnNo);
		Controls.Add(btnYes);
		Controls.Add(checkBox1);
		Controls.Add(para1);
		Font = new Font("Tahoma", 8.25F);
		MaximizeBox = false;
		MinimizeBox = false;
		MinimumSize = new Size(238, 152);
		Name = "PrivacyConfirmation";
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		Text = "Get Help from the Internet";
		Load += new EventHandler(PrivacyConfirmation_Load);
		Leave += new EventHandler(PrivacyConfirmation_Closed);
		((ISupportInitialize)dataGridView1).EndInit();
		ResumeLayout(false);

	}

	public new DialogResult ShowDialog(IWin32Window owner)
	{
		bool flag = true;
		if (m_url.IndexOf('?') < 0)
		{
			return DialogResult.Yes;
		}

		/*
		try
		{
			m_key = Registry.CurrentUser.CreateSubKey($"Software\\Microsoft\\Microsoft SQL Server\\{AssemblyVersionInfo.SqlVersionLocationString}\\Tools\\Client\\ExceptionMessageBox");
			flag = (int)m_key.GetValue("ShowPrivacyDialog", 1) == 1;
		}
		catch (Exception)
		{
		}
		*/
		if (flag)
		{
			return base.ShowDialog(owner);
		}

		return DialogResult.Yes;
	}

	private void ParseUrl()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		object[] array = new object[2];
		num = m_url.IndexOf('?', 0);
		while (num > 0)
		{
			num2 = m_url.IndexOf('=', num);
			num3 = m_url.IndexOf('&', num + 1);
			string stringToUnescape;
			string stringToUnescape2;
			if (num3 < 0)
			{
				if (num2 < 0)
				{
					stringToUnescape = m_url.Substring(num + 1);
					stringToUnescape2 = string.Empty;
				}
				else
				{
					stringToUnescape = m_url.Substring(num + 1, num2 - num - 1);
					stringToUnescape2 = m_url.Substring(num2 + 1);
				}
			}
			else if (num2 < 0 || num2 > num3)
			{
				stringToUnescape = m_url.Substring(num + 1, num3 - num - 1);
				stringToUnescape2 = string.Empty;
			}
			else
			{
				stringToUnescape = m_url.Substring(num + 1, num2 - num - 1);
				stringToUnescape2 = m_url.Substring(num2 + 1, num3 - num2 - 1);
			}

			num = num3;
			array[0] = Uri.UnescapeDataString(stringToUnescape);
			if (string.Compare((string)array[0], "ProdName", ignoreCase: false, CultureInfo.CurrentCulture) == 0)
			{
				array[0] = ExceptionsResources.ProductName;
			}
			else if (string.Compare((string)array[0], "ProdVer", ignoreCase: false, CultureInfo.CurrentCulture) == 0)
			{
				array[0] = ExceptionsResources.ProductVersion;
			}
			else if (string.Compare((string)array[0], "EvtSrc", ignoreCase: false, CultureInfo.CurrentCulture) == 0)
			{
				array[0] = ExceptionsResources.MessageSource;
			}
			else if (string.Compare((string)array[0], "EvtID", ignoreCase: false, CultureInfo.CurrentCulture) == 0)
			{
				array[0] = ExceptionsResources.MessageID;
			}

			array[1] = Uri.UnescapeDataString(stringToUnescape2);
			m_table.Rows.Add(array);
		}
	}

	private void PrivacyConfirmation_Load(object sender, EventArgs e)
	{
		m_table.Columns.Add(new DataColumn(ExceptionsResources.PrivacyItemName, typeof(string)));
		m_table.Columns.Add(new DataColumn(ExceptionsResources.PrivacyItemValue, typeof(string)));
		dataGridView1.DataSource = m_table;
		dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
		dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
		ParseUrl();
	}

	private void btnNo_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btnYes_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void PrivacyConfirmation_Closed(object sender, EventArgs e)
	{
		/*
		try
		{
			if (m_key != null && checkBox1.Checked)
			{
				m_key.SetValue("ShowPrivacyDialog", 0);
			}
		}
		catch (Exception)
		{
		}
		*/
	}

	public PrivacyConfirmationDlg()
	{
	}
}
