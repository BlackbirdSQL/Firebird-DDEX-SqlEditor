// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Controls;


// =========================================================================================================
//										TConnectionUIControl Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionUIControl"/> interface
/// </summary>
// =========================================================================================================
public partial class TConnectionUIControl : DataConnectionUIControl
{


	// -----------------------------------------------------------------------------------------------------
	#region Constructors / Destructors - TConnectionUIControl
	// -----------------------------------------------------------------------------------------------------


	public TConnectionUIControl() : base()
	{
		// Tracer.Trace(GetType(), "TConnectionUIControl.TConnectionUIControl");

		try
		{
			InitializeComponent();

			// Diag.Trace("Creating erd");
			_DataSources = new()
			{
				DataSource = ConnectionLocator.DataSources,
				DependentSource = ConnectionLocator.Databases,
				PrimaryKey = "DataSourceLc",
				ForeignKey = "DataSourceLc"
			};

			cmbDataSource.DataSource = _DataSources;
			cmbDataSource.ValueMember = "DataSourceLc";
			cmbDataSource.DisplayMember = "DataSource";

			cmbDatabase.DataSource = _DataSources.Dependent;
			cmbDatabase.ValueMember = "DatabaseLc";
			cmbDatabase.DisplayMember = "DatasetId";
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Variables - TConnectionUIControl
	// =========================================================================================================


	private readonly ErmBindingSource _DataSources;

	private int _EventsCardinal = 0;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - TConnectionUIControl
	// =========================================================================================================


	private bool InvalidDependent => _DataSources.DependentRow == null
		|| _DataSources.DependentRow["DatabaseLc"] == DBNull.Value
		|| (string)_DataSources.DependentRow["DatabaseLc"] == "";

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if when execution has entered an event handler that may cause recursion
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool EventsDisabled
	{
		get { return _EventsCardinal > 0; }
	}


	#endregion Property accessors




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
		// Tracer.Trace(GetType(), "LoadProperties()");

		try
		{
			// Diag.Trace("Loading datasource text");
			DisableEvents();


			if (Site != null && Site.TryGetValue("DataSource", out object value))
				txtDataSource.Text = (string)value;
			else
				txtDataSource.Text = CoreConstants.C_DefaultDataSource;

			if (txtDataSource.Text != "")
				cmbDataSource.SelectedValue = txtDataSource.Text.ToLower();
			else
				cmbDataSource.SelectedIndex = -1;


			if (Site != null && Site.TryGetValue("User ID", out value))
				txtUserName.Text = (string)value;
			else
				txtUserName.Text = CoreConstants.C_DefaultUserID;

			if (Site != null && Site.TryGetValue("Database", out value))
				txtDatabase.Text = (string)value;
			else
				txtDatabase.Text = CoreConstants.C_DefaultDatabase;


			if (Site != null && Site.TryGetValue("Password", out value))
				txtPassword.Text = (string)value;
			else
				txtPassword.Text = CoreConstants.C_DefaultPassword;


			if (Site != null && Site.TryGetValue("Role", out value))
				txtRole.Text = (string)value;
			else
				txtRole.Text = ModelConstants.C_DefaultRole;

			if (Site != null && Site.TryGetValue("Character Set", out value))
				cboCharset.SetSelectedValueX(value);
			else
				cboCharset.SetSelectedValueX(ModelConstants.C_DefaultCharset);

			if (Site != null && Site.TryGetValue("Port", out value))
				txtPort.Text = (string)value;
			else
				txtPort.Text = CoreConstants.C_DefaultPort.ToString();

			if (Site != null && Site.TryGetValue("Dialect", out value))
				cboDialect.SetSelectedValueX(value);
			else
				cboDialect.SetSelectedValueX(ModelConstants.C_DefaultDialect);

			if (Site != null && Site.TryGetValue("ServerType", out value))
				cboServerType.SelectedIndex = Convert.ToInt32((string)value);
			else
				cboServerType.SelectedIndex = (int)CoreConstants.C_DefaultServerType;
			// Diag.Trace("Default ServerType: " + (int)Constants.DefaultValueServerType);
			// Strange bug here. The default on the enum is being returned as the literal. Cannot trace it.
			// Fixed.


			EnableEvents();

			// Diag.Trace("Positioning erd datasources");
			if (txtDataSource.Text.Length > 0)
				_DataSources.Position = _DataSources.Find(txtDataSource.Text.ToLower());
			else
				_DataSources.Position = -1;

			_DataSources.CurrentChanged += OnDataSourcesCurrentChanged;
			_DataSources.DependencyCurrentChanged += OnDatabasesCurrentChanged;
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
		_EventsCardinal++;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="EventsDisabled"/> counter that was previously incremented by
	/// <see cref="DisableEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EnableEvents()
	{
		if (_EventsCardinal == 0)
			Diag.Dug(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
		else
			_EventsCardinal--;
	}



	private void ValidateDatasetKey()
	{
		if (InvalidDependent)
		{
			DisableEvents();

			try
			{
				Site.Remove("Dataset");
				Site.Remove("DatasetId");
				Site.Remove("DatasetKey");
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
			finally
			{
				EnableEvents();
			}
		}
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
	private void OnCmdGetFileClick(object sender, EventArgs e)
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
	private void OnDatabaseTextChanged(object sender, EventArgs e)
	{
		try
		{
			if (EventsDisabled)
				return;

			// Diag.Trace("Database text changed");
			if (Site != null)
			{
				// if (txtDatabase.Text.Trim() == "")
				//	Site.Remove("Database");
				// else
					Site["Database"] = txtDatabase.Text.Trim();
			}

			if (!_DataSources.IsReady)
				return;

			string database = txtDatabase.Text.Trim().ToLower();

			if (database == "")
				return;

			if (_DataSources.DependentRow != null)
			{
				if (database == (string)_DataSources.DependentRow["DatabaseLc"])
					return;
			}

			_DataSources.DependentPosition = _DataSources.FindDependent("DatabaseLc", database);
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
	private void OnDatabasesCurrentChanged(object sender, EventArgs e)
	{
		// Diag.Trace("Databases CurrentChanged");
		if (InvalidDependent)
		{
			ValidateDatasetKey();
			return;
		}

		DisableEvents();

		try
		{
			if (txtDatabase.Text.ToLower() != (string)_DataSources.DependentRow["DatabaseLc"])
			{
				txtDatabase.Text = ((string)_DataSources.DependentRow["Database"]).Trim();
				if (Site != null)
				{
					// if (txtDatabase.Text == "")
					//	Site.Remove("Database");
					// else
						Site["Database"] = txtDatabase.Text;
				}
			}

			int selectedIndex = cboCharset.SelectedIndex;

			cboCharset.SelectedValue = (string)_DataSources.DependentRow["Charset"];

			if (cboCharset.SelectedIndex == -1)
			{
				cboCharset.SelectedIndex = selectedIndex;
			}
			else if (Site != null)
			{
				if (cboCharset.Text.Trim() == "" || cboCharset.Text.Trim().ToUpper() == ModelConstants.C_DefaultCharset)
					Site.Remove("Character Set");
				else
					Site["Character Set"] = cboCharset.Text;
			}

			if ((string)_DataSources.DependentRow["UserID"] != "")
			{
				txtUserName.Text = (string)_DataSources.DependentRow["UserID"];
				txtPassword.Text = (string)_DataSources.DependentRow["Password"];
				txtRole.Text = (string)_DataSources.DependentRow["Role"];

				if (Site != null)
				{
					// if (txtUserName.Text == "")
					//	Site.Remove("User ID");
					// else
						Site["User ID"] = txtUserName.Text;
					// if (txtPassword.Text == "")
					// 	Site.Remove("Password");
					// else
						Site["Password"] = txtPassword.Text;
					if (txtRole.Text == "")
						Site.Remove("Role");
					 else
						Site["Role"] = txtRole.Text;
				}
			}

			if (Site != null)
			{
				foreach (Describer describer in CsbAgent.Describers.Advanced)
				{
					if (!describer.DefaultEqualsOrEmpty(_DataSources.DependentRow[describer.Name]))
						Site[describer.Key] = _DataSources.DependentRow[describer.Name];
					else
						Site.Remove(describer.Key);
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnableEvents();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks the database input text and moves the binding source cursor if the data source is found
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnDataSourceTextChanged(object sender, EventArgs e)
	{
		try
		{
			if (EventsDisabled)
				return;

			// Diag.Trace("DataSource text changed");

			if (Site != null)
			{
				// if (txtDataSource.Text.Trim() == "")
				//	Site.Remove("DataSource");
				// else
					Site["DataSource"] = txtDataSource.Text.Trim();
			}

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
	///		2. The user typed into the datasource textbox and a match was found in <see cref="ErmBindingSource"/>.
	///	If it's (1) did it, the input text will not match the binding source row info.
	///	If it's (2) did it the input text will already match the current row info
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private void OnDataSourcesCurrentChanged(object sender, EventArgs e)
	{
		// Diag.Trace("_DataSources CurrentChanged");
		try
		{
			if (_DataSources.Row == null || (int)_DataSources.Row["Orderer"] == 0)
				return;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return;
		}

		DisableEvents();

		try
		{


			if ((int)_DataSources.Row["Orderer"] == 1)
			{
				txtDataSource.Text = CoreConstants.C_DefaultDataSource;
				txtPort.Text = CoreConstants.C_DefaultPort.ToString();
				cboServerType.SetSelectedIndexX((int)CoreConstants.C_DefaultServerType);
				txtDatabase.Text = CoreConstants.C_DefaultDatabase;
				cboDialect.SetSelectedValueX(ModelConstants.C_DefaultDialect);
				txtUserName.Text = CoreConstants.C_DefaultUserID;
				txtPassword.Text = CoreConstants.C_DefaultPassword;
				txtRole.Text = ModelConstants.C_DefaultRole;
				cboCharset.SetSelectedValueX(ModelConstants.C_DefaultCharset);

				if (Site != null)
				{
					foreach (Describer describer in CsbAgent.Describers.Values)
					{
						if (describer.IsAdvanced || describer.IsConnectionParameter)
							Site.Remove(describer.Key);
						else if (describer.IsConnectionParameter)
							Site[describer.Key] = describer.DefaultValue;
					}
				}

				_DataSources.Position = -1;
			}
			else
			{
				if (txtDataSource.Text.ToLower() != (string)_DataSources.Row["DataSourceLc"])
				{
					txtDataSource.Text = (string)_DataSources.Row["DataSource"];
					if (Site != null)
					{
						// if (txtDataSource.Text.Trim() == "")
						//	Site.Remove("DataSource");
						// else
							Site["DataSource"] = txtDataSource.Text.Trim();
					}
				}


				if ((int)_DataSources.Row["Port"] != 0 && txtPort.Text != _DataSources.Row["Port"].ToString())
				{
					txtPort.Text = _DataSources.Row["Port"].ToString();
					if (Site != null)
					{
						if (Convert.ToInt32(txtPort.Text) == CoreConstants.C_DefaultPort)
							Site.Remove("Port");
						else
							Site["Port"] = txtPort.Text;
					}
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnableEvents();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for all other input controls
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	private void OnSetProperty(object sender, EventArgs e)
	{
		try
		{
			if (EventsDisabled)
				return;

			if (Site != null)
			{
				if (sender.Equals(txtUserName))
				{
					// if (txtUserName.Text.Trim() == "")
					//	Site.Remove("User ID");
					// else
						Site["User ID"] = txtUserName.Text.Trim();
				}
				else if (sender.Equals(txtPassword))
				{
					// if (txtPassword.Text.Trim() == "")
					//	Site.Remove("Password");
					// else
						Site["Password"] = txtPassword.Text.Trim();
				}
				else if (sender.Equals(txtRole))
				{
					if (txtRole.Text.Trim() == "")
						Site.Remove("Role");
					 else
						Site["Role"] = txtRole.Text.Trim();
				}
				else if (sender.Equals(cboCharset))
				{
					if (cboCharset.Text.Trim() == "" || cboCharset.Text.Trim().ToUpper() == ModelConstants.C_DefaultCharset)
						Site.Remove("Character Set");
					else
						Site["Character Set"] = cboCharset.Text.Trim();
				}
				else if (sender.Equals(txtPort))
				{
					if (String.IsNullOrWhiteSpace(txtPort.Text) || Convert.ToInt32(txtPort.Text.Trim()) == CoreConstants.C_DefaultPort)
						Site.Remove("Port");
					else
						Site["Port"] = Convert.ToInt32(txtPort.Text);
				}
				else if (sender.Equals(cboDialect))
				{
					if (String.IsNullOrWhiteSpace(cboDialect.Text) || Convert.ToInt32(cboDialect.Text.Trim()) == ModelConstants.C_DefaultDialect)
						Site.Remove("Dialect");
					else
						Site["Dialect"] = Convert.ToInt32(cboDialect.Text);
				}
				else if (sender.Equals(cboServerType))
				{
					if (cboServerType.SelectedIndex == -1 || cboServerType.SelectedIndex == (int)CoreConstants.C_DefaultServerType)
						Site.Remove("ServerType");
					else
						Site["ServerType"] = cboServerType.SelectedIndex;
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