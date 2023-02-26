//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using System.Linq.Expressions;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TConnectionUIControl Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionUIControl"/> interface
/// </summary>
// =========================================================================================================
public partial class TConnectionUIControl : DataConnectionUIControl
{
	#region Variables - TConnectionUIControl


	private readonly ErmBindingSource _DataSources;

	private int _EventsDisabled = 0;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - TConnectionUIControl
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if when execution has enetered an event handler that may cause recursion
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool EventsDisabled
	{
		get { return _EventsDisabled > 0; }
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - TConnectionUIControl
	// =========================================================================================================


	public TConnectionUIControl() : base()
	{
		try
		{ 
		// Diag.Trace();
		InitializeComponent();

		// Diag.Trace("Creating erd");
		_DataSources = new()
		{
			DataSource = XmlParser.DataSources,
			DependentSource = XmlParser.Databases,
			PrimaryKey = "DataSourceLc",
			ForeignKey = "DataSourceLc"
		};

		cmbDataSource.DataSource = _DataSources;
		cmbDataSource.ValueMember = "DataSourceLc";
		cmbDataSource.DisplayMember = "DataSourceName";

		cmbDatabase.DataSource = _DataSources.Dependent;
		cmbDatabase.ValueMember = "InitialCatalogLc";
		cmbDatabase.DisplayMember = "Name";
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TConnectionUIControl
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads a previously saved connection string's properties into the form
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void LoadProperties()
	{
		try
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


			if (Site != null && Site.TryGetValue("Password", out value))
				txtPassword.Text = (string)value;
			else
				txtPassword.Text = DslConnectionString.DefaultValuePassword;


			if (Site != null && Site.TryGetValue("Role Name", out value))
				txtRole.Text = (string)value;
			else
				txtRole.Text = DslConnectionString.DefaultValueRoleName;

			if (Site != null && Site.TryGetValue("Character Set", out value))
				cboCharset.SetSelectedValueX(value);
			else
				cboCharset.SetSelectedValueX(DslConnectionString.DefaultValueCharacterSet);

			if (Site != null && Site.TryGetValue("Port Number", out value))
				txtPort.Text = (string)value;
			else
				txtPort.Text = DslConnectionString.DefaultValuePortNumber.ToString();

			if (Site != null && Site.TryGetValue("Dialect", out value))
				cboDialect.SetSelectedValueX(value);
			else
				cboDialect.SetSelectedValueX(DslConnectionString.DefaultValueDialect);

			if (Site != null && Site.TryGetValue("Server Type", out value))
				cboServerType.SelectedIndex = Convert.ToInt32((string)value);
			else
				cboServerType.SelectedIndex = (int)DslConnectionString.DefaultValueServerType;
			// Diag.Trace("Default ServerType: " + (int)DslConnectionString.DefaultValueServerType);
			// Strange bug here. The default on the enum is being returned as the literal. Cannot trace it


			EnableEvents();

			// Diag.Trace("Positioning erd datasources");
			if (txtDataSource.Text.Length > 0)
				_DataSources.Position = _DataSources.Find(txtDataSource.Text.ToLower());
			else
				_DataSources.Position = -1;

			_DataSources.CurrentChanged += DataSourcesCurrentChanged;
			_DataSources.DependencyCurrentChanged += DatabasesCurrentChanged;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - TConnectionUIControl
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="EventsDisabled"/> counter when execution enters an event handler
	/// to prevent recursion
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void DisableEvents()
	{
		_EventsDisabled++;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="EventsDisabled"/> counter that was previously incremented by
	/// <see cref="DisableEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EnableEvents()
	{
		if (_EventsDisabled == 0)
			Diag.Dug(new InvalidOperationException("Events already enabled"));
		else
			_EventsDisabled--;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - TConnectionUIControl
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Open FileDialog button click event handler
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	private void CmdGetFile_Click(object sender, EventArgs e)
	{
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			cmbDatabase.Text = openFileDialog.FileName;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks the database input text and updates form values
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void DatabaseTextChanged(object sender, EventArgs e)
	{
		try
		{
			if (EventsDisabled)
				return;

			// Diag.Trace("Database text changed");
			Site["Initial Catalog"] = txtDatabase.Text.Trim();

			if (!_DataSources.IsReady)
				return;

			string database = txtDatabase.Text.Trim().ToLower();

			if (database == "")
				return;

			if (_DataSources.DependentRow != null)
			{
				if (database == (string)_DataSources.DependentRow["InitialCatalogLc"])
					return;
			}

			_DataSources.DependentPosition = _DataSources.FindDependent("InitialCatalogLc", database);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Handles changes the underlying <see cref="ErmBindingSource.Dependent"/>'s <see cref="ErmBindingSource.DependentRow"/>
	/// linked to <see cref="cmbDatabase"/>.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	private void DatabasesCurrentChanged(object sender, EventArgs e)
	{
		// Diag.Trace("Databases CurrentChanged");

		try
		{
			if (_DataSources.DependentRow == null || (string)_DataSources.DependentRow["InitialCatalogLc"] == "")
				return;

			DisableEvents();

			if (txtDatabase.Text.ToLower() != (string)_DataSources.DependentRow["InitialCatalogLc"])
				Site["Initial Catalog"] = txtDatabase.Text = (string)_DataSources.DependentRow["InitialCatalog"];

			int selectedIndex = cboCharset.SelectedIndex;

			cboCharset.SelectedValue = (string)_DataSources.DependentRow["Charset"];

			if (cboCharset.SelectedIndex == -1)
				cboCharset.SelectedIndex = selectedIndex;
			else
				Site["Character Set"] = cboCharset.Text;

			if ((string)_DataSources.DependentRow["UserName"] != "")
			{
				Site["User ID"] = txtUserName.Text = (string)_DataSources.DependentRow["UserName"];
				Site["Password"] = txtPassword.Text = (string)_DataSources.DependentRow["Password"];
				Site["Role Name"] = txtRole.Text = (string)_DataSources.DependentRow["RoleName"];
			}

			EnableEvents();

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks the database input text and moves the binding source cursor if the data source is found
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void DataSourceTextChanged(object sender, EventArgs e)
	{
		try
		{
			if (EventsDisabled)
				return;

			// Diag.Trace("Datasource text changed");

			Site["Data Source"] = txtDataSource.Text.Trim();

			if (!_DataSources.IsReady)
				return;

			string datasource = txtDataSource.Text.Trim().ToLower();

			if (datasource == "")
				return;

			if (_DataSources.Row != null)
			{
				if (datasource == (string)_DataSources.CurrentValue)
					return;
			}


			_DataSources.Position = _DataSources.Find(datasource);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}


	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Raised when the <see cref="_DataSources"/> master <see cref="BindingSource"/> cursor position changes
	/// </summary>
	/// <remarks>
	/// This is probably the cleanest way of doing this. This event can be raised in one of two ways:
	///		1. The user selected a datasource from the dropdown.
	///		2. The user type into the datasource textbox and a match was found in <see cref="ErmBindingSource"/>.
	///	If it's (1) did it, the input text will not match the binding source row info.
	///	If it's (2) did it the input text will already match the current row info
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private void DataSourcesCurrentChanged(object sender, EventArgs e)
	{
		// Diag.Trace("_DataSources CurrentChanged");

		try
		{
			if (_DataSources.Row == null || (int)_DataSources.Row["Orderer"] == 0)
				return;

			DisableEvents();

			if ((int)_DataSources.Row["Orderer"] == 1)
			{
				Site["Data Source"] = txtDataSource.Text = "";
				Site["Port Number"] = txtPort.Text = DslConnectionString.DefaultValuePortNumber.ToString();
				Site["Server Type"] = cboServerType.SetSelectedIndexX((int)DslConnectionString.DefaultValueServerType).ToString();
				Site["Initial Catalog"] = txtDatabase.Text = "";
				Site["Dialect"] = (string)cboDialect.SetSelectedValueX(DslConnectionString.DefaultValueDialect);
				Site["User ID"] = txtUserName.Text = "";
				Site["Password"] = txtPassword.Text = "";
				Site["Role Name"] = txtRole.Text = "";
				cboCharset.SelectedValue = DslConnectionString.DefaultValueCharacterSet;
				Site["Character Set"] = DslConnectionString.DefaultValueCharacterSet;
				Site["Character Set"] = (string)cboCharset.SetSelectedValueX(DslConnectionString.DefaultValueCharacterSet);

				_DataSources.Position = -1;
			}
			else
			{
				if (txtDataSource.Text.ToLower() != (string)_DataSources.Row["DataSourceLc"])
					Site["Data Source"] = txtDataSource.Text = (string)_DataSources.Row["DataSource"];


				if ((int)_DataSources.Row["PortNumber"] != 0 && txtPort.Text != _DataSources.Row["PortNumber"].ToString())
					Site["Port Number"] = txtPort.Text = _DataSources.Row["PortNumber"].ToString();
			}

			EnableEvents();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for all other input controls
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	private void SetProperty(object sender, EventArgs e)
	{
		try
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
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}


	#endregion Event handlers

}