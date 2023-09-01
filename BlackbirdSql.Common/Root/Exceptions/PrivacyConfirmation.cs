#region Assembly Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

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
using Microsoft.Win32;




namespace BlackbirdSql.Common.Exceptions
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	public class PrivacyConfirmation : Form
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
					resourceMan = new ResourceManager("Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.PrivacyConfirmation", typeof(PrivacyConfirmation).Assembly);
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

		public PrivacyConfirmation(string title, string url)
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
			this.para1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.para1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.para1.Location = new System.Drawing.Point(8, 6);
			this.para1.Name = "para1";
			this.para1.Size = new System.Drawing.Size(655, 22);
			this.para1.TabIndex = 0;
			this.para1.Text = "&To get help for this message, you must send information across the Internet. Do " +
    "you want to send the following information?";
			// 
			// checkBox1
			// 
			this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBox1.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.checkBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.checkBox1.Location = new System.Drawing.Point(11, 267);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(655, 24);
			this.checkBox1.TabIndex = 4;
			this.checkBox1.Text = "&Do not ask me this again (always send the information)";
			this.checkBox1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// btnYes
			// 
			this.btnYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.btnYes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnYes.Location = new System.Drawing.Point(505, 288);
			this.btnYes.Name = "btnYes";
			this.btnYes.Size = new System.Drawing.Size(75, 23);
			this.btnYes.TabIndex = 1;
			this.btnYes.Text = "&Yes";
			this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
			// 
			// btnNo
			// 
			this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
			this.btnNo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnNo.Location = new System.Drawing.Point(586, 288);
			this.btnNo.Name = "btnNo";
			this.btnNo.Size = new System.Drawing.Size(75, 23);
			this.btnNo.TabIndex = 2;
			this.btnNo.Text = "&No";
			this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Location = new System.Drawing.Point(12, 31);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.RowHeadersVisible = false;
			this.dataGridView1.Size = new System.Drawing.Size(649, 230);
			this.dataGridView1.TabIndex = 5;
			// 
			// PrivacyConfirmation
			// 
			this.AcceptButton = this.btnYes;
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.btnNo;
			this.ClientSize = new System.Drawing.Size(673, 329);
			this.ControlBox = false;
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.btnNo);
			this.Controls.Add(this.btnYes);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.para1);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(238, 152);
			this.Name = "PrivacyConfirmation";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Get Help from the Internet";
			this.Load += new System.EventHandler(this.PrivacyConfirmation_Load);
			this.Leave += new System.EventHandler(this.PrivacyConfirmation_Closed);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

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

		public PrivacyConfirmation()
		{
		}
	}
}
