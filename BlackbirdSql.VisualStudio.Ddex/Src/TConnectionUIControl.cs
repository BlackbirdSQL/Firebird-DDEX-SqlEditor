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
using System.Windows.Forms;
using Microsoft.VisualStudio.Data.Framework;

using BlackbirdSql.VisualStudio.Ddex.Schema;
using BlackbirdSql.Common;

namespace BlackbirdSql.VisualStudio.Ddex
{

	public partial class TConnectionUIControl : DataConnectionUIControl
	{

		#region · Constructors ·

		public TConnectionUIControl() :base()
		{
			Diag.Trace();
			InitializeComponent();
		}

		#endregion

		#region · Methods ·

		public override void LoadProperties()
		{
			if (Site != null && Site.TryGetValue("Data Source", out object value))
				txtDataSource.Text = (string)value;
			else
				txtDataSource.Text = DslConnectionString.DefaultValueDataSource;

			if (Site != null && Site.TryGetValue("User ID", out value))
				txtUserName.Text = (string)value;
			else
				txtUserName.Text = DslConnectionString.DefaultValueUserId;

			if (Site != null && Site.TryGetValue("Initial Catalog", out value))
				txtDatabase.Text = (string)value;
			else
				txtDatabase.Text = DslConnectionString.DefaultValueCatalog;

			if (Site != null && Site.TryGetValue("Password", out value))
				txtPassword.Text = (string)value;
			else
				txtPassword.Text = DslConnectionString.DefaultValuePassword;


			if (Site != null && Site.TryGetValue("Role Name", out value))
				txtRole.Text = (string)value;
			else
				txtRole.Text = DslConnectionString.DefaultValueRoleName;

			if (Site != null && Site.TryGetValue("Character Set", out value))
				cboCharset.Text = (string)value;
			else
				cboCharset.Text = DslConnectionString.DefaultValueCharacterSet;

			if (Site != null && Site.TryGetValue("Port Number", out value))
				txtPort.Text = (string)value;
			else
				txtPort.Text = DslConnectionString.DefaultValuePortNumber.ToString();

			if (Site == null || !Site.TryGetValue("Dialect", out value))
				value = DslConnectionString.DefaultValueDialect;
			if (Convert.ToInt32(value) == 1)
				cboDialect.SelectedIndex = 0;
			else
				cboDialect.SelectedIndex = 1;

			if (Site == null || !Site.TryGetValue("Server Type", out value))
				value = DslConnectionString.DefaultValueServerType;
			// Strange bug here. The default on the enum is being returned as the literal. Cannot trace it
			if (Convert.ToString(value) == "Default" || Convert.ToInt32(value) == 0)
				cboServerType.SelectedIndex = 0;
			else
				cboServerType.SelectedIndex = 1;

		}

		#endregion

		#region · Private Methods ·


		#endregion

		#region · Event Handlers ·

		private void SetProperty(object sender, EventArgs e)
		{
			if (Site != null)
			{
				if (sender.Equals(txtDataSource))
					Site["Data Source"] = txtDataSource.Text;
				else if (sender.Equals(txtDatabase))
					Site["Initial Catalog"] = txtDatabase.Text;
				else if (sender.Equals(txtUserName))
					Site["User ID"] = txtUserName.Text;
				else if (sender.Equals(txtPassword))
					Site["Password"] = txtPassword.Text;
				else if (sender.Equals(txtRole))
					Site["Role Name"] = txtRole.Text;
				else if (sender.Equals(cboCharset))
					Site["Character Set"] = cboCharset.Text;
				else if (sender.Equals(txtPort))
				{
					if (!String.IsNullOrEmpty(txtPort.Text))
						Site["Port Number"] = Convert.ToInt32(txtPort.Text);
				}
				else if (sender.Equals(cboDialect))
				{
					if (!String.IsNullOrEmpty(cboDialect.Text))
						Site["Dialect"] = Convert.ToInt32(cboDialect.Text);
				}
				else if (sender.Equals(cboServerType))
				{
					if (cboServerType.SelectedIndex != -1)
						Site["Server Type"] = Convert.ToInt32(cboServerType.SelectedIndex);
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