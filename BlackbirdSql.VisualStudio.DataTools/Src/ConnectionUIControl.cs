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
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.Data;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.DataTools
{
	public partial class ConnectionUIControl : DataConnectionUIControl
	{

		#region · Constructors ·

		public ConnectionUIControl()
		{
			Diag.Trace();
			InitializeComponent();
		}

		#endregion

		#region · Methods ·

		public override void LoadProperties()
		{
			Diag.Trace();

			object value;

			try
			{
				if (ConnectionProperties.Contains("Data Source"))
					txtDataSource.Text = (string)ConnectionProperties["Data Source"];
				else
					ConnectionProperties["Data Source"] = txtDataSource.Text = ConnectionString.DefaultValueDataSource;

				if (ConnectionProperties.Contains("User Id"))
					txtUserName.Text = (string)ConnectionProperties["User ID"];
				else
					ConnectionProperties["User ID"] = txtUserName.Text = ConnectionString.DefaultValueUserId;

				if (ConnectionProperties.Contains("Initial Catalog"))
					txtDatabase.Text = (string)ConnectionProperties["Initial Catalog"];
				else
					ConnectionProperties["Initial Catalog"] = txtDatabase.Text = ConnectionString.DefaultValueCatalog;

				if (ConnectionProperties.Contains("Password"))
					txtPassword.Text = (string)ConnectionProperties["Password"];
				else
					ConnectionProperties["Password"] = txtPassword.Text = ConnectionString.DefaultValuePassword;

				if (ConnectionProperties.Contains("Role Name"))
					txtRole.Text = (string)ConnectionProperties["Role Name"];
				else
					ConnectionProperties["Role Name"] = txtRole.Text = ConnectionString.DefaultValueRoleName;

				if (ConnectionProperties.Contains("Character Set"))
					cboCharset.Text = (string)ConnectionProperties["Character Set"];
				else
					ConnectionProperties["Character Set"] = cboCharset.Text = ConnectionString.DefaultValueCharacterSet;


				if (ConnectionProperties.Contains("Port Number"))
					txtPort.Text = (string)ConnectionProperties["Port Number"];
				else
					ConnectionProperties["Port Number"]  = txtPort.Text = ConnectionString.DefaultValuePortNumber.ToString();


				if (ConnectionProperties.Contains("Dialect"))
					value = ConnectionProperties["Dialect"];
				else
					ConnectionProperties["Dialect"] = value = ConnectionString.DefaultValueDialect;
				if (Convert.ToInt32(value) == 1)
					cboDialect.SelectedIndex = 0;
				else
					cboDialect.SelectedIndex = 1;

				if (ConnectionProperties.Contains("Server Type"))
					value = ConnectionProperties["Server Type"];
				else
					ConnectionProperties["Server Type"] = value = ConnectionString.DefaultValueServerType;

				// Strange bug here. The default on the enum is being returned as the literal. Cannot trace it
				if (Convert.ToString(value) == "Default" || Convert.ToInt32(value) == 0)
					cboServerType.SelectedIndex = 0;
				else
					cboServerType.SelectedIndex = 1;

			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
}

#endregion

#region · Private Methods ·

private void SetProperty(string propertyName, object value)
{
	this.ConnectionProperties[propertyName] = value;
}

#endregion

#region · Event Handlers ·

private void SetProperty(object sender, EventArgs e)
{
	if (sender.Equals(txtDataSource))
	{
		SetProperty("Data Source", txtDataSource.Text);
	}
	else if (sender.Equals(txtDatabase))
	{
		SetProperty("Initial Catalog", txtDatabase.Text);
	}
	else if (sender.Equals(txtUserName))
	{
		SetProperty("User ID", txtUserName.Text);
	}
	else if (sender.Equals(txtPassword))
	{
		SetProperty("Password", txtPassword.Text);
	}
	else if (sender.Equals(txtRole))
	{
		SetProperty("Role", txtRole.Text);
	}
	else if (sender.Equals(txtPort))
	{
		if (!String.IsNullOrEmpty(txtPort.Text))
		{
			SetProperty("Port Number", Convert.ToInt32(txtPort.Text));
		}
	}
	else if (sender.Equals(cboCharset))
	{
		SetProperty("Character Set", cboCharset.Text);
	}
	else if (sender.Equals(cboDialect))
	{
		if (!String.IsNullOrEmpty(cboDialect.Text))
		{
			SetProperty("Dialect", Convert.ToInt32(cboDialect.Text));
		}
	}
	else if (sender.Equals(cboServerType))
	{
		if (cboServerType.SelectedIndex != -1)
		{
			SetProperty("Server Type", Convert.ToInt32(cboServerType.SelectedIndex));
		}
	}
}

private void CmdGetFile_Click(object sender, EventArgs e)
{
	if (openFileDialog.ShowDialog() == DialogResult.OK)
	{
		txtDatabase.Text = openFileDialog.FileName;
	}
}

		#endregion
	}
}