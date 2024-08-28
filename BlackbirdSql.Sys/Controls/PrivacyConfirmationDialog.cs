// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.PrivacyConfirmation

using System;
using System.CodeDom.Compiler;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using BlackbirdSql.Sys.Properties;



namespace BlackbirdSql.Sys.Controls;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]


public partial class PrivacyConfirmationDialog : Form
{
	public PrivacyConfirmationDialog()
	{
	}

	public PrivacyConfirmationDialog(string title, string url)
	{
		InitializeComponent();
		Text = title;
		m_url = url;
		m_table.Locale = CultureInfo.CurrentCulture;
	}



	private readonly DataTable m_table = new DataTable("table");

	private readonly string m_url;


	public new DialogResult ShowDialog(IWin32Window owner)
	{
		bool flag = true;
		if (m_url.IndexOf('?') < 0)
		{
			return DialogResult.Yes;
		}

		if (flag)
		{
			return base.ShowDialog(owner);
		}

		return DialogResult.Yes;
	}



	private void ParseUrl()
	{
		object[] array = new object[2];
		int num = m_url.IndexOf('?', 0);

		while (num > 0)
		{
			int num2 = m_url.IndexOf('=', num);
			int num3 = m_url.IndexOf('&', num + 1);
			string stringToUnescape;
			string stringToUnescape2;
			if (num3 < 0)
			{
				if (num2 < 0)
				{
					stringToUnescape = m_url[(num + 1)..];
					stringToUnescape2 = "";
				}
				else
				{
					stringToUnescape = m_url.Substring(num + 1, num2 - num - 1);
					stringToUnescape2 = m_url[(num2 + 1)..];
				}
			}
			else if (num2 < 0 || num2 > num3)
			{
				stringToUnescape = m_url.Substring(num + 1, num3 - num - 1);
				stringToUnescape2 = "";
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
				array[0] = ControlsResources.PrivacyConfirmationDialog_ProductName;
			}
			else if (string.Compare((string)array[0], "ProdVer", ignoreCase: false, CultureInfo.CurrentCulture) == 0)
			{
				array[0] = ControlsResources.PrivacyConfirmationDialog_ProductVersion;
			}
			else if (string.Compare((string)array[0], "EvtSrc", ignoreCase: false, CultureInfo.CurrentCulture) == 0)
			{
				array[0] = ControlsResources.PrivacyConfirmationDialog_MessageSource;
			}
			else if (string.Compare((string)array[0], "EvtID", ignoreCase: false, CultureInfo.CurrentCulture) == 0)
			{
				array[0] = ControlsResources.PrivacyConfirmationDialog_MessageId;
			}

			array[1] = Uri.UnescapeDataString(stringToUnescape2);
			m_table.Rows.Add(array);
		}
	}



	private void PrivacyConfirmationDialog_Load(object sender, EventArgs e)
	{
		m_table.Columns.Add(new DataColumn(ControlsResources.PrivacyConfirmationDialog_PrivacyItemName, typeof(string)));
		m_table.Columns.Add(new DataColumn(ControlsResources.PrivacyConfirmationDialog_PrivacyItemValue, typeof(string)));
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


	private void PrivacyConfirmationDialog_FormClosed(object sender, FormClosedEventArgs e)
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
}
