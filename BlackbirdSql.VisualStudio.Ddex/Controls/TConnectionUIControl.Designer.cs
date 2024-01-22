
namespace BlackbirdSql.VisualStudio.Ddex.Controls
{
	partial class TConnectionUIControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TConnectionUIControl));
			this.cmdTest = new System.Windows.Forms.Button();
			this.lblDataSource = new System.Windows.Forms.Label();
			this.cboDialect = new System.Windows.Forms.ComboBox();
			this.grbSettings = new System.Windows.Forms.GroupBox();
			this.cboCharset = new System.Windows.Forms.ComboBox();
			this.lblCharset = new System.Windows.Forms.Label();
			this.lblDialect = new System.Windows.Forms.Label();
			this.cboServerType = new System.Windows.Forms.ComboBox();
			this.lblServerType = new System.Windows.Forms.Label();
			this.grbLogin = new System.Windows.Forms.GroupBox();
			this.lblRole = new System.Windows.Forms.Label();
			this.txtRole = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblUser = new System.Windows.Forms.Label();
			this.txtUserName = new System.Windows.Forms.TextBox();
			this.cmdGetFile = new System.Windows.Forms.Button();
			this.cmbDatabase = new System.Windows.Forms.ComboBox();
			this.lblDatabase = new System.Windows.Forms.Label();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.lblPort = new System.Windows.Forms.Label();
			this.cmbDataSource = new System.Windows.Forms.ComboBox();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.txtDataSource = new System.Windows.Forms.TextBox();
			this.txtDatabase = new System.Windows.Forms.TextBox();
			this.lblDatabaseBlank = new System.Windows.Forms.Label();
			this.lblDataSourceBlank = new System.Windows.Forms.Label();
			this.lblDatasetKeyDescription = new System.Windows.Forms.Label();
			this.grbSettings.SuspendLayout();
			this.grbLogin.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmdTest
			// 
			resources.ApplyResources(this.cmdTest, "cmdTest");
			this.cmdTest.Name = "cmdTest";
			// 
			// lblDataSource
			// 
			this.lblDataSource.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblDataSource, "lblDataSource");
			this.lblDataSource.Name = "lblDataSource";
			// 
			// cboDialect
			// 
			this.cboDialect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDialect.FormattingEnabled = true;
			this.cboDialect.Items.AddRange(new object[] {
            resources.GetString("cboDialect.Items"),
            resources.GetString("cboDialect.Items1")});
			resources.ApplyResources(this.cboDialect, "cboDialect");
			this.cboDialect.Name = "cboDialect";
			this.cboDialect.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// grbSettings
			// 
			this.grbSettings.Controls.Add(this.cboCharset);
			this.grbSettings.Controls.Add(this.lblCharset);
			this.grbSettings.Controls.Add(this.cboDialect);
			this.grbSettings.Controls.Add(this.lblDialect);
			resources.ApplyResources(this.grbSettings, "grbSettings");
			this.grbSettings.Name = "grbSettings";
			this.grbSettings.TabStop = false;
			// 
			// cboCharset
			// 
			this.cboCharset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCharset.FormattingEnabled = true;
			this.cboCharset.Items.AddRange(new object[] {
            resources.GetString("cboCharset.Items"),
            resources.GetString("cboCharset.Items1"),
            resources.GetString("cboCharset.Items2"),
            resources.GetString("cboCharset.Items3"),
            resources.GetString("cboCharset.Items4"),
            resources.GetString("cboCharset.Items5"),
            resources.GetString("cboCharset.Items6"),
            resources.GetString("cboCharset.Items7"),
            resources.GetString("cboCharset.Items8"),
            resources.GetString("cboCharset.Items9"),
            resources.GetString("cboCharset.Items10"),
            resources.GetString("cboCharset.Items11"),
            resources.GetString("cboCharset.Items12"),
            resources.GetString("cboCharset.Items13"),
            resources.GetString("cboCharset.Items14"),
            resources.GetString("cboCharset.Items15"),
            resources.GetString("cboCharset.Items16"),
            resources.GetString("cboCharset.Items17"),
            resources.GetString("cboCharset.Items18"),
            resources.GetString("cboCharset.Items19"),
            resources.GetString("cboCharset.Items20"),
            resources.GetString("cboCharset.Items21"),
            resources.GetString("cboCharset.Items22"),
            resources.GetString("cboCharset.Items23"),
            resources.GetString("cboCharset.Items24"),
            resources.GetString("cboCharset.Items25"),
            resources.GetString("cboCharset.Items26")});
			resources.ApplyResources(this.cboCharset, "cboCharset");
			this.cboCharset.Name = "cboCharset";
			this.cboCharset.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblCharset
			// 
			this.lblCharset.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblCharset, "lblCharset");
			this.lblCharset.Name = "lblCharset";
			// 
			// lblDialect
			// 
			this.lblDialect.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblDialect, "lblDialect");
			this.lblDialect.Name = "lblDialect";
			// 
			// cboServerType
			// 
			this.cboServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboServerType.FormattingEnabled = true;
			this.cboServerType.Items.AddRange(new object[] {
            resources.GetString("cboServerType.Items"),
            resources.GetString("cboServerType.Items1")});
			resources.ApplyResources(this.cboServerType, "cboServerType");
			this.cboServerType.Name = "cboServerType";
			this.cboServerType.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblServerType
			// 
			this.lblServerType.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblServerType, "lblServerType");
			this.lblServerType.Name = "lblServerType";
			// 
			// grbLogin
			// 
			this.grbLogin.Controls.Add(this.lblRole);
			this.grbLogin.Controls.Add(this.txtRole);
			this.grbLogin.Controls.Add(this.lblPassword);
			this.grbLogin.Controls.Add(this.txtPassword);
			this.grbLogin.Controls.Add(this.lblUser);
			this.grbLogin.Controls.Add(this.txtUserName);
			resources.ApplyResources(this.grbLogin, "grbLogin");
			this.grbLogin.Name = "grbLogin";
			this.grbLogin.TabStop = false;
			// 
			// lblRole
			// 
			this.lblRole.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblRole, "lblRole");
			this.lblRole.Name = "lblRole";
			// 
			// txtRole
			// 
			resources.ApplyResources(this.txtRole, "txtRole");
			this.txtRole.Name = "txtRole";
			this.txtRole.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblPassword
			// 
			this.lblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblPassword, "lblPassword");
			this.lblPassword.Name = "lblPassword";
			// 
			// txtPassword
			// 
			resources.ApplyResources(this.txtPassword, "txtPassword");
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblUser
			// 
			this.lblUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblUser, "lblUser");
			this.lblUser.Name = "lblUser";
			// 
			// txtUserName
			// 
			resources.ApplyResources(this.txtUserName, "txtUserName");
			this.txtUserName.Name = "txtUserName";
			this.txtUserName.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// cmdGetFile
			// 
			resources.ApplyResources(this.cmdGetFile, "cmdGetFile");
			this.cmdGetFile.Name = "cmdGetFile";
			this.cmdGetFile.Click += new System.EventHandler(this.OnCmdGetFileClick);
			// 
			// cmbDatabase
			// 
			resources.ApplyResources(this.cmbDatabase, "cmbDatabase");
			this.cmbDatabase.Name = "cmbDatabase";
			// 
			// lblDatabase
			// 
			this.lblDatabase.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblDatabase, "lblDatabase");
			this.lblDatabase.Name = "lblDatabase";
			// 
			// txtPort
			// 
			resources.ApplyResources(this.txtPort, "txtPort");
			this.txtPort.Name = "txtPort";
			this.txtPort.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblPort
			// 
			this.lblPort.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblPort, "lblPort");
			this.lblPort.Name = "lblPort";
			// 
			// cmbDataSource
			// 
			resources.ApplyResources(this.cmbDataSource, "cmbDataSource");
			this.cmbDataSource.Name = "cmbDataSource";
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "FDB";
			resources.ApplyResources(this.openFileDialog, "openFileDialog");
			// 
			// txtDataSource
			// 
			this.txtDataSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(this.txtDataSource, "txtDataSource");
			this.txtDataSource.Name = "txtDataSource";
			this.txtDataSource.TextChanged += new System.EventHandler(this.OnDataSourceTextChanged);
			// 
			// txtDatabase
			// 
			this.txtDatabase.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(this.txtDatabase, "txtDatabase");
			this.txtDatabase.Name = "txtDatabase";
			this.txtDatabase.TextChanged += new System.EventHandler(this.OnDatabaseTextChanged);
			// 
			// lblDatabaseBlank
			// 
			this.lblDatabaseBlank.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.lblDatabaseBlank, "lblDatabaseBlank");
			this.lblDatabaseBlank.Name = "lblDatabaseBlank";
			// 
			// lblDataSourceBlank
			// 
			this.lblDataSourceBlank.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.lblDataSourceBlank, "lblDataSourceBlank");
			this.lblDataSourceBlank.Name = "lblDataSourceBlank";
			// 
			// lblDatasetKeyDescription
			// 
			resources.ApplyResources(this.lblDatasetKeyDescription, "lblDatasetKeyDescription");
			this.lblDatasetKeyDescription.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lblDatasetKeyDescription.Name = "lblDatasetKeyDescription";
			// 
			// TConnectionUIControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblDatasetKeyDescription);
			this.Controls.Add(this.txtDatabase);
			this.Controls.Add(this.txtDataSource);
			this.Controls.Add(this.lblDataSourceBlank);
			this.Controls.Add(this.lblDatabaseBlank);
			this.Controls.Add(this.lblServerType);
			this.Controls.Add(this.cboServerType);
			this.Controls.Add(this.lblDataSource);
			this.Controls.Add(this.grbSettings);
			this.Controls.Add(this.grbLogin);
			this.Controls.Add(this.cmdGetFile);
			this.Controls.Add(this.cmbDatabase);
			this.Controls.Add(this.lblDatabase);
			this.Controls.Add(this.txtPort);
			this.Controls.Add(this.lblPort);
			this.Controls.Add(this.cmbDataSource);
			this.Controls.Add(this.cmdTest);
			this.Name = "TConnectionUIControl";
			this.grbSettings.ResumeLayout(false);
			this.grbLogin.ResumeLayout(false);
			this.grbLogin.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cmdTest;
		private System.Windows.Forms.Label lblDataSource;
		private System.Windows.Forms.ComboBox cboDialect;
		private System.Windows.Forms.GroupBox grbSettings;
		private System.Windows.Forms.ComboBox cboServerType;
		private System.Windows.Forms.Label lblServerType;
		private System.Windows.Forms.GroupBox grbLogin;
		private System.Windows.Forms.Label lblRole;
		private System.Windows.Forms.TextBox txtRole;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblUser;
		private System.Windows.Forms.TextBox txtUserName;
		private System.Windows.Forms.Button cmdGetFile;
		private System.Windows.Forms.ComboBox cmbDatabase;
		private System.Windows.Forms.Label lblDatabase;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.Label lblPort;
		private System.Windows.Forms.Label lblDialect;
		private System.Windows.Forms.ComboBox cmbDataSource;
		private System.Windows.Forms.Label lblCharset;
		private System.Windows.Forms.ComboBox cboCharset;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.TextBox txtDataSource;
		private System.Windows.Forms.TextBox txtDatabase;
		private System.Windows.Forms.Label lblDatabaseBlank;
		private System.Windows.Forms.Label lblDataSourceBlank;
		private System.Windows.Forms.Label lblDatasetKeyDescription;
	}
}
