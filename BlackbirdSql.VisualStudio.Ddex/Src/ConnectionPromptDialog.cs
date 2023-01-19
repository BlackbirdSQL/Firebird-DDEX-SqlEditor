/*
 *  Visual Studio DDEX Provider for FirebirdClient
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;



public partial class ConnectionPromptDialog : DataConnectionPromptDialog
{

	private enum AuthenticationType
	{
		Windows,
		SQLServer,
		ActiveDirectoryPassword,
		ActiveDirectoryIntegrated
	}


	#region · Constructors ·

	public ConnectionPromptDialog()
	{
		InitializeComponent();

		int width = savePasswordCheckBox.PreferredSize.Width;
		if (savePasswordCheckBox.Width < width)
		{
			base.Width += width - savePasswordCheckBox.Width;
		}

		int preferredHeight = headerLabel.PreferredHeight;
		if (headerLabel.Height > preferredHeight)
		{
			base.Height += headerLabel.Height - preferredHeight;
		}

		MinimumSize = new Size(base.Width, base.Height);
	}

	#endregion

	#region · Methods ·

	public new string ShowDialog(IVsDataConnectionSupport dataConnectionSupport)
	{
		ConnectionSupport connectionSupport = (ConnectionSupport)dataConnectionSupport;


		authenticationTableLayoutPanel.SuspendLayout();
		authenticationTypeComboBox.Items.Clear();
		authenticationTypeComboBox.Items.Add(string.Format(CultureInfo.CurrentCulture, "Windows"));
		authenticationTypeComboBox.Items.Add(string.Format(CultureInfo.CurrentCulture, "SQLServer"));
		if (connectionSupport.ProviderObject is SqlConnection)
		{
			authenticationTypeComboBox.Items.Add(string.Format(CultureInfo.CurrentCulture, "ActiveDirectoryPassword"));
			authenticationTypeComboBox.Items.Add(string.Format(CultureInfo.CurrentCulture, "ActiveDirectoryIntegrated"));
		}

		authenticationTableLayoutPanel.ResumeLayout();
		return base.ShowDialog((IVsDataConnectionSupport)connectionSupport);
	}

	protected override void LoadProperties()
	{
		base.ConnectionUIProperties.Parse(base.ConnectionSupport.ConnectionString);
		serverTextBox.Text = base.ConnectionUIProperties["Data Source"] as string;
		databaseTextBox.Text = base.ConnectionUIProperties["Initial Catalog"] as string;
		if ((bool)base.ConnectionUIProperties["Enlist"])
		{
			authenticationTypeComboBox.SelectedIndex = 0;
		}
		else if (string.Equals(base.ConnectionUIProperties["Role Name"]?.ToString(), "ActiveDirectoryIntegrated", StringComparison.InvariantCultureIgnoreCase))
		{
			authenticationTypeComboBox.SelectedIndex = 3;
		}
		else
		{
			userNameTextBox.Text = base.ConnectionUIProperties["User ID"] as string;
			passwordTextBox.Text = base.ConnectionUIProperties["Password"] as string;
			savePasswordCheckBox.Checked = (bool)base.ConnectionUIProperties["No Garbage Collect"];
			if (string.Equals(base.ConnectionUIProperties["Role Name"]?.ToString(), "ActiveDirectoryPassword", StringComparison.InvariantCultureIgnoreCase))
			{
				authenticationTypeComboBox.SelectedIndex = 2;
			}
			else
			{
				authenticationTypeComboBox.SelectedIndex = 1;
			}
		}

		if (base.ConnectionUIProperties.IsComplete)
		{
			serverTextBox.ReadOnly = true;
			databaseTextBox.ReadOnly = true;
			authenticationTypeComboBox.Enabled = false;
			userNameTextBox.ReadOnly = true;
		}
	}

	protected override void SaveProperties()
	{
		if (serverTextBox.Enabled)
		{
			base.ConnectionUIProperties["Data Source"] = serverTextBox.Text.Trim();
		}

		if (databaseTextBox.Enabled)
		{
			base.ConnectionUIProperties["Initial Catalog"] = databaseTextBox.Text.Trim();
		}

		if (authenticationTypeComboBox.Enabled)
		{
			switch (authenticationTypeComboBox.SelectedIndex)
			{
				case 0:
					base.ConnectionUIProperties["Enlist"] = true;
					break;
				case 3:
					base.ConnectionUIProperties["Enlist"] = false;
					base.ConnectionUIProperties["Role Name"] = "Active Directory Integrated";
					break;
				case 2:
					base.ConnectionUIProperties["Enlist"] = false;
					base.ConnectionUIProperties["Role Name"] = "Active Directory Password";
					break;
				case 1:
					base.ConnectionUIProperties["Enlist"] = false;
					break;
			}
		}

		if (userNameTextBox.Enabled)
		{
			base.ConnectionUIProperties["User ID"] = userNameTextBox.Text.Trim();
		}

		base.ConnectionUIProperties["Password"] = passwordTextBox.Text;
		base.ConnectionUIProperties["No Garbage Collect"] = savePasswordCheckBox.Checked;
		base.ConnectionSupport.ConnectionString = base.ConnectionUIProperties.ToString();
	}

	#endregion

	#region · Event Handlers ·

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		int num = buttonsTableLayoutPanel.Top - buttonsTableLayoutPanel.Margin.Top - loginTableLayoutPanel.Margin.Bottom - loginTableLayoutPanel.Bottom;
		MinimumSize = new Size(MinimumSize.Width, MinimumSize.Height - num);
		base.Height += -num;
	}

	protected override void OnShown(EventArgs e)
	{
		if (base.ConnectionUIProperties.IsComplete)
		{
			passwordTextBox.Focus();
		}

		base.OnShown(e);
	}

	protected override void OnHelpRequested(HelpEventArgs hevent)
	{
		// Host.ShowHelp("vs.sqlconnectionpromptdialog");
		// hevent.Handled = true;
		base.OnHelpRequested(hevent);
	}

	private void SetSecurityOption(object sender, EventArgs e)
	{
		loginTableLayoutPanel.Enabled = authenticationTypeComboBox.SelectedIndex == 1 || authenticationTypeComboBox.SelectedIndex == 2;
		SetOkButtonStatus(sender, e);
	}

	private void TrimControlText(object sender, EventArgs e)
	{
		Control control = sender as Control;
		control.Text = control.Text.Trim();
	}

	private void ResetControlText(object sender, EventArgs e)
	{
		Control control = sender as Control;
		control.Text = control.Text;
	}

	private void SetOkButtonStatus(object sender, EventArgs e)
	{
		okButton.Enabled = authenticationTypeComboBox.SelectedIndex == 0 || authenticationTypeComboBox.SelectedIndex == 3 || userNameTextBox.Text.Length > 0;
	}


	/*
	 * protected override void WndProc(ref Message m)
	{
		if (Host.TranslateContextHelpMessage((Form)this, ref m))
		{
			DefWndProc(ref m);
		}
		else
		{
			base.WndProc(ref m);
		}
	}
	*/
	#endregion
}