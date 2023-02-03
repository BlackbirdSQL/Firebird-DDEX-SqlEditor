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
		private readonly BindingSourceEx BsDataSources;
		private readonly BindingSourceEx BsDatabases;

		private bool _SelectedIndexChanged = false;



		#region · Constructors ·

		public TConnectionUIControl() : base()
		{
			Diag.Trace();
			InitializeComponent();

			BsDataSources = new BindingSourceEx
			{
				DataSource = XmlParser.DataSources
			};

			BsDatabases = new BindingSourceEx()
			{
				DataSource = XmlParser.Databases
			};

			try
			{
				cmbDataSource.DataSource = BsDataSources;
				cmbDataSource.ValueMember = "DataSourceLc";
				cmbDataSource.DisplayMember = "DataSourceName";
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			try
			{
				cmbDatabase.DataSource = BsDatabases;
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

		public override void LoadProperties()
		{
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



			if (txtDataSource.Text.Length > 0)
				BsDataSources.Position = BsDataSources.Find("DataSourceLc", txtDataSource.Text.ToLower());
			else
				BsDataSources.Position = -1;


			UpdateDatabasesFilter();


			BsDataSources.CurrentChanged += DataSourcesCurrentChanged;
			BsDatabases.CurrentChanged += DatabasesCurrentChanged;
			BsDatabases.ListChanged += DatabasesListChanged;


		}


		#endregion

		#region · Private Methods ·


		private void UpdateDatabasesFilter()
		{
			try
			{
				string filter;

				if (BsDataSources.Current != null)
				{
					DataRow row = ((DataRowView)BsDataSources.Current).Row;
					filter = "DataSourceLc = '" + (string)row["DataSourceLc"] + "' AND Name <> ''";
				}
				else
				{
					filter = "DataSourceLc = '' AND Name <> ''";
				}

				if (BsDatabases.Filter == filter)
					return;

				BsDatabases.Filter = filter;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return;
			}
		}

		#endregion

		#region · Event Handlers ·


		/// <summary>
		/// Raised when the database list changes of the data source has changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>
		/// Simply invalidates the current row on the dabases binding source so the CurrentChanged event handler ignores
		/// the change
		/// </remarks>
		private void DatabasesListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
		{
			BsDatabases.Position = -1;
		}


		/// <summary>
		/// Checks the database input text and move the binding source cursor if the data source is found
		/// </summary>
		private void DataSourceTextChanged(object sender, EventArgs e)
		{
			Site["Data Source"] = txtDataSource.Text.Trim();

			string datasource = txtDataSource.Text.Trim().ToLower();

			if (datasource == "")
				return;

			DataRow row;

			if (BsDataSources.Current != null)
			{
				row = ((DataRowView)BsDataSources.Current).Row;

				if (datasource == (string)row["DataSourceLc"])
					return;
			}


			BsDataSources.Position = BsDataSources.Find("DataSourceLc", datasource);

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
			if (BsDataSources.Current != null)
			{
				DataRow row = ((DataRowView)BsDataSources.Current).Row;

				if ((int)row["Orderer"] == 0)
				{
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

					BsDataSources.Position = -1;
					return;
				}
				else
				{
					if (txtDataSource.Text.ToLower() != (string)row["DataSourceLc"])
						Site["Data Source"] = txtDataSource.Text = (string)row["DataSource"];


					if ((int)row["PortNumber"] != 0 && txtPort.Text != row["PortNumber"].ToString())
						Site["Port Number"] = txtPort.Text = row["PortNumber"].ToString();
				}

			}

			_SelectedIndexChanged = false;

			UpdateDatabasesFilter();
		}



		/// <summary>
		/// Checks the database input text and updates form values
		/// </summary>
		private void DatabaseTextChanged(object sender, EventArgs e)
		{
			Site["Initial Catalog"] = txtDatabase.Text.Trim();

			string database = txtDatabase.Text.Trim().ToLower();

			if (database == "")
				return;

			DataRow row;

			if (BsDatabases.Current != null)
			{
				row = ((DataRowView)BsDatabases.Current).Row;

				if (database == (string)row["InitialCatalogLc"])
					return;
			}

			BsDatabases.Position = BsDatabases.Find("InitialCatalogLc", database);

			return;
		}

		private void DatabasesCurrentChanged(object sender, EventArgs e)
		{
			if (BsDatabases.Current == null)
				return;

			DataRow row = ((DataRowView)BsDatabases.Current).Row;

			if (txtDatabase.Text.ToLower() != (string)row["InitialCatalogLc"])
				Site["Initial Catalog"] = txtDatabase.Text = (string)row["InitialCatalog"];

			int selectedIndex = cboCharset.SelectedIndex;

			cboCharset.SelectedValue = (string)row["Charset"];

			if (cboCharset.SelectedIndex == -1)
				cboCharset.SelectedIndex = selectedIndex;
			else
				Site["Character Set"] = cboCharset.Text;

			if ((string)row["UserName"] != "")
			{
				Site["User ID"] = txtUserName.Text = (string)row["UserName"];
				Site["Password"] = txtPassword.Text = (string)row["Password"];
				Site["Role Name"] = txtRole.Text = (string)row["RoleName"];
			}
		}



		private void SetProperty(object sender, EventArgs e)
		{
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
			_SelectedIndexChanged = true;
		}

		private void CmbDatabase_SelectedIndexChanged(object sender, EventArgs e)
		{
			_SelectedIndexChanged = true;
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