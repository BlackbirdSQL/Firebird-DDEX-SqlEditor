
namespace BlackbirdSql.VisualStudio.Ddex.Controls
{
    partial class TConnectionUIControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
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
			this.cmdTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdTest.Location = new System.Drawing.Point(-162, 180);
			this.cmdTest.Name = "cmdTest";
			this.cmdTest.Size = new System.Drawing.Size(75, 23);
			this.cmdTest.TabIndex = 6;
			this.cmdTest.Text = "&Test";
			// 
			// lblDataSource
			// 
			this.lblDataSource.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDataSource.Location = new System.Drawing.Point(0, 0);
			this.lblDataSource.Margin = new System.Windows.Forms.Padding(0);
			this.lblDataSource.Name = "lblDataSource";
			this.lblDataSource.Size = new System.Drawing.Size(122, 16);
			this.lblDataSource.TabIndex = 27;
			this.lblDataSource.Text = "Server hostname";
			// 
			// cboDialect
			// 
			this.cboDialect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDialect.FormattingEnabled = true;
			this.cboDialect.Items.AddRange(new object[] {
            "1",
            "3"});
			this.cboDialect.Location = new System.Drawing.Point(56, 53);
			this.cboDialect.Name = "cboDialect";
			this.cboDialect.Size = new System.Drawing.Size(78, 21);
			this.cboDialect.TabIndex = 9;
			this.cboDialect.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// grbSettings
			// 
			this.grbSettings.Controls.Add(this.cboCharset);
			this.grbSettings.Controls.Add(this.lblCharset);
			this.grbSettings.Controls.Add(this.cboDialect);
			this.grbSettings.Controls.Add(this.lblDialect);
			this.grbSettings.Location = new System.Drawing.Point(230, 120);
			this.grbSettings.Name = "grbSettings";
			this.grbSettings.Size = new System.Drawing.Size(180, 109);
			this.grbSettings.TabIndex = 25;
			this.grbSettings.TabStop = false;
			this.grbSettings.Text = "Database settings";
			// 
			// cboCharset
			// 
			this.cboCharset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboCharset.FormattingEnabled = true;
			this.cboCharset.Items.AddRange(new object[] {
            "NONE",
            "ASCII",
            "Big5",
            "DOS437",
            "DOS850",
            "DOS860",
            "DOS861",
            "DOS863",
            "DOS865",
            "EUCJ_0208",
            "GB_2312",
            "ISO8859_1",
            "ISO8859_2",
            "KSC_5601",
            "ISO2022-JP",
            "SJIS_0208",
            "UNICODE_FSS",
            "UTF8",
            "WIN1250",
            "WIN1251",
            "WIN1252",
            "WIN1253",
            "WIN1254",
            "WIN1255",
            "WIN1257",
            "KOI8R",
            "KOI8U"});
			this.cboCharset.Location = new System.Drawing.Point(56, 24);
			this.cboCharset.Name = "cboCharset";
			this.cboCharset.Size = new System.Drawing.Size(108, 21);
			this.cboCharset.TabIndex = 7;
			this.cboCharset.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblCharset
			// 
			this.lblCharset.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCharset.Location = new System.Drawing.Point(8, 26);
			this.lblCharset.Name = "lblCharset";
			this.lblCharset.Size = new System.Drawing.Size(48, 16);
			this.lblCharset.TabIndex = 19;
			this.lblCharset.Text = "Charset";
			// 
			// lblDialect
			// 
			this.lblDialect.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDialect.Location = new System.Drawing.Point(8, 55);
			this.lblDialect.Name = "lblDialect";
			this.lblDialect.Size = new System.Drawing.Size(48, 16);
			this.lblDialect.TabIndex = 17;
			this.lblDialect.Text = "Dialect";
			// 
			// cboServerType
			// 
			this.cboServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboServerType.FormattingEnabled = true;
			this.cboServerType.Items.AddRange(new object[] {
            "Standalone Server",
            "Embedded Server"});
			this.cboServerType.Location = new System.Drawing.Point(265, 17);
			this.cboServerType.Name = "cboServerType";
			this.cboServerType.Size = new System.Drawing.Size(145, 21);
			this.cboServerType.TabIndex = 32;
			this.cboServerType.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblServerType
			// 
			this.lblServerType.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblServerType.Location = new System.Drawing.Point(265, 1);
			this.lblServerType.Name = "lblServerType";
			this.lblServerType.Size = new System.Drawing.Size(64, 16);
			this.lblServerType.TabIndex = 13;
			this.lblServerType.Text = "Server type";
			// 
			// grbLogin
			// 
			this.grbLogin.Controls.Add(this.lblRole);
			this.grbLogin.Controls.Add(this.txtRole);
			this.grbLogin.Controls.Add(this.lblPassword);
			this.grbLogin.Controls.Add(this.txtPassword);
			this.grbLogin.Controls.Add(this.lblUser);
			this.grbLogin.Controls.Add(this.txtUserName);
			this.grbLogin.Location = new System.Drawing.Point(0, 120);
			this.grbLogin.Name = "grbLogin";
			this.grbLogin.Size = new System.Drawing.Size(220, 109);
			this.grbLogin.TabIndex = 24;
			this.grbLogin.TabStop = false;
			this.grbLogin.Text = "Login";
			// 
			// lblRole
			// 
			this.lblRole.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblRole.Location = new System.Drawing.Point(8, 86);
			this.lblRole.Name = "lblRole";
			this.lblRole.Size = new System.Drawing.Size(48, 14);
			this.lblRole.TabIndex = 4;
			this.lblRole.Text = "Role";
			// 
			// txtRole
			// 
			this.txtRole.Location = new System.Drawing.Point(64, 83);
			this.txtRole.Name = "txtRole";
			this.txtRole.Size = new System.Drawing.Size(140, 20);
			this.txtRole.TabIndex = 5;
			this.txtRole.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblPassword
			// 
			this.lblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblPassword.Location = new System.Drawing.Point(8, 55);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(48, 16);
			this.lblPassword.TabIndex = 2;
			this.lblPassword.Text = "Password";
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(64, 53);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(140, 20);
			this.txtPassword.TabIndex = 3;
			this.txtPassword.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblUser
			// 
			this.lblUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblUser.Location = new System.Drawing.Point(8, 27);
			this.lblUser.Name = "lblUser";
			this.lblUser.Size = new System.Drawing.Size(48, 14);
			this.lblUser.TabIndex = 0;
			this.lblUser.Text = "User";
			// 
			// txtUserName
			// 
			this.txtUserName.Location = new System.Drawing.Point(64, 24);
			this.txtUserName.Name = "txtUserName";
			this.txtUserName.Size = new System.Drawing.Size(140, 20);
			this.txtUserName.TabIndex = 1;
			this.txtUserName.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// cmdGetFile
			// 
			this.cmdGetFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdGetFile.Location = new System.Drawing.Point(386, 57);
			this.cmdGetFile.Name = "cmdGetFile";
			this.cmdGetFile.Size = new System.Drawing.Size(24, 23);
			this.cmdGetFile.TabIndex = 23;
			this.cmdGetFile.Text = "...";
			this.cmdGetFile.Click += new System.EventHandler(this.OnCmdGetFileClick);
			// 
			// cmbDatabase
			// 
			this.cmbDatabase.Location = new System.Drawing.Point(0, 58);
			this.cmbDatabase.Name = "cmbDatabase";
			this.cmbDatabase.Size = new System.Drawing.Size(384, 21);
			this.cmbDatabase.TabIndex = 22;
			// 
			// lblDatabase
			// 
			this.lblDatabase.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDatabase.Location = new System.Drawing.Point(0, 41);
			this.lblDatabase.Name = "lblDatabase";
			this.lblDatabase.Size = new System.Drawing.Size(48, 16);
			this.lblDatabase.TabIndex = 21;
			this.lblDatabase.Text = "Database";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(180, 17);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(65, 20);
			this.txtPort.TabIndex = 31;
			this.txtPort.Text = "3050";
			this.txtPort.TextChanged += new System.EventHandler(this.OnSetProperty);
			// 
			// lblPort
			// 
			this.lblPort.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblPort.Location = new System.Drawing.Point(180, 0);
			this.lblPort.Name = "lblPort";
			this.lblPort.Size = new System.Drawing.Size(69, 16);
			this.lblPort.TabIndex = 15;
			this.lblPort.Text = "Port number";
			// 
			// cmbDataSource
			// 
			this.cmbDataSource.Location = new System.Drawing.Point(0, 17);
			this.cmbDataSource.Name = "cmbDataSource";
			this.cmbDataSource.Size = new System.Drawing.Size(160, 21);
			this.cmbDataSource.TabIndex = 30;
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "FDB";
			this.openFileDialog.Filter = "Firebird Databases|*.fdb|Interbase Databases|*.gdb|All files|*.*";
			// 
			// txtDataSource
			// 
			this.txtDataSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtDataSource.Location = new System.Drawing.Point(4, 21);
			this.txtDataSource.Name = "txtDataSource";
			this.txtDataSource.Size = new System.Drawing.Size(134, 13);
			this.txtDataSource.TabIndex = 29;
			this.txtDataSource.TextChanged += new System.EventHandler(this.OnDataSourceTextChanged);
			// 
			// txtDatabase
			// 
			this.txtDatabase.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtDatabase.Location = new System.Drawing.Point(4, 62);
			this.txtDatabase.Name = "txtDatabase";
			this.txtDatabase.Size = new System.Drawing.Size(358, 13);
			this.txtDatabase.TabIndex = 33;
			this.txtDatabase.TextChanged += new System.EventHandler(this.OnDatabaseTextChanged);
			// 
			// lblDatabaseBlank
			// 
			this.lblDatabaseBlank.BackColor = System.Drawing.Color.White;
			this.lblDatabaseBlank.Location = new System.Drawing.Point(2, 60);
			this.lblDatabaseBlank.Name = "lblDatabaseBlank";
			this.lblDatabaseBlank.Size = new System.Drawing.Size(360, 17);
			this.lblDatabaseBlank.TabIndex = 34;
			// 
			// lblDataSourceBlank
			// 
			this.lblDataSourceBlank.BackColor = System.Drawing.Color.White;
			this.lblDataSourceBlank.Location = new System.Drawing.Point(2, 19);
			this.lblDataSourceBlank.Name = "lblDataSourceBlank";
			this.lblDataSourceBlank.Size = new System.Drawing.Size(136, 17);
			this.lblDataSourceBlank.TabIndex = 35;
			// 
			// lblDatasetKeyDescription
			// 
			this.lblDatasetKeyDescription.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lblDatasetKeyDescription.Location = new System.Drawing.Point(0, 82);
			this.lblDatasetKeyDescription.Name = "lblDatasetKeyDescription";
			this.lblDatasetKeyDescription.Size = new System.Drawing.Size(410, 30);
			this.lblDatasetKeyDescription.TabIndex = 36;
			this.lblDatasetKeyDescription.Text = "To assign a custom name to this connection specify a proposed Custom DatasetKey\r\n" +
    "under Advanced Properties.";
			// 
			// TConnectionUIControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
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
			this.Margin = new System.Windows.Forms.Padding(0);
			this.MinimumSize = new System.Drawing.Size(410, 198);
			this.Name = "TConnectionUIControl";
			this.Size = new System.Drawing.Size(410, 229);
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
