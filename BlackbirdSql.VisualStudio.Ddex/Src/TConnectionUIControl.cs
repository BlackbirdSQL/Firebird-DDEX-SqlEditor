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

using BlackbirdSql.Common;
using BlackbirdSql.Common.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using System.Data;

namespace BlackbirdSql.VisualStudio.Ddex
{

	public partial class TConnectionUIControl : DataConnectionUIControl
	{
		private readonly ErmBindingSource DataSources;

		private int _EventsDisabled = 0;

		private bool EventsDisabled
		{
			get { return _EventsDisabled > 0; }
		}


		#region · Constructors ·

		public TConnectionUIControl() : base()
		{
			// Diag.Trace();
			InitializeComponent();

			// Diag.Trace("Creating erd");
			DataSources = new()
			{
				DataSource = XmlParser.DataSources,
				DependentSource = XmlParser.Databases,
				PrimaryKey = "DataSourceLc",
				ForeignKey = "DataSourceLc"
			};
			// Diag.Trace("Erd created");

			try
			{
				cmbDataSource.DataSource = DataSources;
				cmbDataSource.ValueMember = "DataSourceLc";
				cmbDataSource.DisplayMember = "DataSourceName";
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}


			try
			{
				cmbDatabase.DataSource = DataSources.Dependent;
				cmbDatabase.ValueMember = "InitialCatalogLc";
				cmbDatabase.DisplayMember = "Name";
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

		}

		#endregion

		#region · Methods ·


		private void EnableEvents()
		{
			if (_EventsDisabled == 0)
				Diag.Dug(new InvalidOperationException("Events already enabled"));
			else
				_EventsDisabled--;
		}


		private void DisableEvents()
		{
			_EventsDisabled++;
		}


		public override void LoadProperties()
		{
			// Diag.Trace("Loading datasource text");
			DisableEvents();

			if (Site != null && Site.TryGetValue("Data Source", out object value))
				txtDataSource.Text = (string)value;
			else
				txtDataSource.Text = DslConnectionString.DefaultValueDataSource;

			if (txtDataSource.Text != "")
				cmbDataSource.SelectedValue = txtDataSource.Text.ToLower();
			else
				cmbDataSource.SelectedIndex = -1;


			if (Site != null && Site.TryGetValue("User ID", out value))
				txtUserName.Text = (string)value;
			else
				txtUserName.Text = DslConnectionString.DefaultValueUserId;

			if (Site != null && Site.TryGetValue("Initial Catalog", out value))
				txtDatabase.Text = (string)value;
			else
				txtDatabase.Text = DslConnectionString.DefaultValueCatalog;

			if (txtDatabase.Text != "")
				cmbDatabase.SelectedValue = txtDataSource.Text.ToLower();
			else
				cmbDatabase.SelectedIndex = -1;

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

			EnableEvents();

			// Diag.Trace("Positioning erd datasources");
			if (txtDataSource.Text.Length > 0)
				DataSources.Position = DataSources.Find(txtDataSource.Text.ToLower());
			else
				DataSources.Position = -1;

			// Diag.Trace("Adding datasources currentchanged");
			DataSources.CurrentChanged += DataSourcesCurrentChanged;
			// Diag.Trace("Adding databases currentchanged");
			DataSources.DependencyCurrentChanged += DatabasesCurrentChanged;
			// Diag.Trace("erd setup complete");


		}


		#endregion

		#region · Private Methods ·


		#endregion

		#region · Event Handlers ·




		/// <summary>
		/// Checks the database input text and move the binding source cursor if the data source is found
		/// </summary>
		private void DataSourceTextChanged(object sender, EventArgs e)
		{
			if (EventsDisabled)
				return;

			// Diag.Trace("Datasource text changed");

			Site["Data Source"] = txtDataSource.Text.Trim();

			if (!DataSources.IsReady)
				return;

			string datasource = txtDataSource.Text.Trim().ToLower();

			if (datasource == "")
				return;

			if (DataSources.Row != null)
			{
				if (datasource == (string)DataSources.CurrentValue)
					return;
			}


			DataSources.Position = DataSources.Find(datasource);

		}


		/// <summary>
		/// Raised when the DataSources binding source cursor position changes
		/// </summary>
		/// <remarks>
		/// This is probably the cleanest way of doing this. This event can be raised in one of two ways:
		///		1. The user selected a datasource from the dropdown.
		///		2. Yhe user type into the datasource textbox and a match was found in the binding source.
		///	If it's (1) did it, the input text will not match the binding source row info.
		///	If it's (2) did it the input text will already match the current row info
		/// </remarks>
		private void DataSourcesCurrentChanged(object sender, EventArgs e)
		{
			// Diag.Trace("DataSources CurrentChanged");

			if (DataSources.Row == null || (int)DataSources.Row["Orderer"] == 0)
				return;

			if ((int)DataSources.Row["Orderer"] == 1)
			{
				DisableEvents();

				Site["Data Source"] = txtDataSource.Text = "";
				Site["Port Number"] = txtPort.Text = "3050";
				Site["Server Type"] = cboServerType.SelectedIndex = 0;
				Site["Initial Catalog"] = txtDatabase.Text = "";
				cboDialect.SelectedIndex = 1;
				Site["Dialect"] = Convert.ToInt32(cboDialect.Text);
				Site["User ID"] = txtUserName.Text = "";
				Site["Password"] = txtPassword.Text = "";
				Site["Role Name"] = txtRole.Text = "";
				Site["Character Set"] = cboCharset.Text = "UTF8";

				EnableEvents();
				DataSources.Position = -1;

				return;
			}
			else
			{
				DisableEvents();

				if (txtDataSource.Text.ToLower() != (string)DataSources.Row["DataSourceLc"])
					Site["Data Source"] = txtDataSource.Text = (string)DataSources.Row["DataSource"];


				if ((int)DataSources.Row["PortNumber"] != 0 && txtPort.Text != DataSources.Row["PortNumber"].ToString())
					Site["Port Number"] = txtPort.Text = DataSources.Row["PortNumber"].ToString();

				EnableEvents();
			}

			// _SelectedIndexChanged = false;

		}



		/// <summary>
		/// Checks the database input text and updates form values
		/// </summary>
		private void DatabaseTextChanged(object sender, EventArgs e)
		{
			if (EventsDisabled)
				return;

			// Diag.Trace("Database text changed");
			Site["Initial Catalog"] = txtDatabase.Text.Trim();

			if (!DataSources.IsReady)
				return;

			string database = txtDatabase.Text.Trim().ToLower();

			if (database == "")
				return;

			if (DataSources.DependentRow != null)
			{
				if (database == (string)DataSources.DependentRow["InitialCatalogLc"])
					return;
			}

			DataSources.DependentPosition = DataSources.FindDependent("InitialCatalogLc", database);

			return;
		}

		private void DatabasesCurrentChanged(object sender, EventArgs e)
		{
			// Diag.Trace("Databases CurrentChanged");

			if (DataSources.DependentRow == null || (string)DataSources.DependentRow["InitialCatalogLc"] == "")
				return;

			DisableEvents();

			if (txtDatabase.Text.ToLower() != (string)DataSources.DependentRow["InitialCatalogLc"])
				Site["Initial Catalog"] = txtDatabase.Text = (string)DataSources.DependentRow["InitialCatalog"];

			int selectedIndex = cboCharset.SelectedIndex;

			cboCharset.SelectedValue = (string)DataSources.DependentRow["Charset"];

			if (cboCharset.SelectedIndex == -1)
				cboCharset.SelectedIndex = selectedIndex;
			else
				Site["Character Set"] = cboCharset.Text;

			if ((string)DataSources.DependentRow["UserName"] != "")
			{
				Site["User ID"] = txtUserName.Text = (string)DataSources.DependentRow["UserName"];
				Site["Password"] = txtPassword.Text = (string)DataSources.DependentRow["Password"];
				Site["Role Name"] = txtRole.Text = (string)DataSources.DependentRow["RoleName"];
			}

			EnableEvents();
		}



		private void SetProperty(object sender, EventArgs e)
		{
			if (EventsDisabled)
				return;

			if (Site != null)
			{
				if (sender.Equals(txtUserName))
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
						Site["Server Type"] = cboServerType.SelectedIndex;
				}
			}

		}



		private void CmbDataSource_SelectedIndexChanged(object sender, EventArgs e)
		{
			//_SelectedIndexChanged = true;
		}

		private void CmbDatabase_SelectedIndexChanged(object sender, EventArgs e)
		{
			// _SelectedIndexChanged = true;
		}

		private void CmdGetFile_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				cmbDatabase.Text = openFileDialog.FileName;
			}
		}


		#endregion
	}
}