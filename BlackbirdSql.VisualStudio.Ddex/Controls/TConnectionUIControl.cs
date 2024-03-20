// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Reflection;
using System.Web.UI.Design.WebControls;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Config;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;


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


	public TConnectionUIControl() : this(EnConnectionSource.Undefined)
	{
	}


	public TConnectionUIControl(EnConnectionSource connectionSource) : base()
	{
		Diag.ThrowIfNotOnUIThread();

		_ConnectionSource = connectionSource;

		try
		{
			if (RctManager.ShutdownState || !RctManager.EnsureLoaded())
			{
				ApplicationException ex;

				if (RctManager.ShutdownState)
					ex = new("RunningConnectionTable is in a shutdown state. Aborting.");
				else
					ex = new("RunningConnectionTable is not loaded.");
				Diag.Dug(ex);
				throw ex;
			}

			InitializeComponent();

			if (ConnectionSource == EnConnectionSource.Application)
			{
				lblDatasetKeyDescription.Text = ControlsResources.TConnectionUIControl_DatasetKeyDescription_Application;
			}
			else if (ConnectionSource == EnConnectionSource.HierarchyMarshaler)
			{
				lblDatasetKeyDescription.Text = ControlsResources.TConnectionUIControl_DatasetKeyDescription_EntityDataModel;
			}
			else
			{
				lblDatasetKeyDescription.Text = ControlsResources.TConnectionUIControl_DatasetKeyDescription;
			}

			// Tracer.Trace("Creating erd");

			cmbDataSource.DataSource = DataSources;
			cmbDataSource.ValueMember = "DataSourceLc";
			cmbDataSource.DisplayMember = "DataSource";

			cmbDatabase.DataSource = DataSources.Dependent;
			cmbDatabase.ValueMember = "DatabaseLc";
			cmbDatabase.DisplayMember = "DisplayName";

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}


	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			DataSources.CurrentChanged -= OnDataSourcesCursorChanged;
			DataSources.DependencyCurrentChanged -= OnDatabasesCursorChanged;

			// if (Site != null)
			//	Site.PropertiesChanged -= OnPropertyChanged;

			RemoveEventHandlers();

			components.Dispose();
		}
		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - TConnectionUIControl
	// =========================================================================================================


	private ErmBindingSource _DataSources;
	private int _InputEventsCardinal = 0;
	private int _CursorEventsCardinal = 0;
	private int _PropertyEventsCardinal = 0;
	private EnConnectionSource _ConnectionSource = EnConnectionSource.Undefined;
	private bool _EventsLoaded = false;
	private Form _ParentParentForm = null;
	private bool _SiteChanged = true;

	private Delegate _OnVerifySettingsDelegate = null;
	private Delegate _OnAcceptDelegate = null;

	private bool _InsertMode = false;
	private string _OriginalConnectionString = null;

	private bool _HandleNewInternally = false;
	private bool _HandleModifyInternally = false;
	private bool _HandleVerification = true;


	private string _StrippedConnectionString = null;
	private string _UnstrippedConnectionString = null;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - TConnectionUIControl
	// =========================================================================================================


	private EnConnectionSource ConnectionSource
	{
		get
		{
			if (_ConnectionSource == EnConnectionSource.Undefined)
			{
				_ConnectionSource = SessionDlg != null
					? EnConnectionSource.Session
					: UnsafeCmd.GetConnectionSource();
			}

			return _ConnectionSource;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if when execution has entered a DataTable cursor event handler
	/// that may cause recursion on Position changes.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool CursorEventsDisabled
	{
		get { return _CursorEventsCardinal > 0; }
	}


	private ErmBindingSource DataSources
	{
		get
		{
			// Tracer.Trace("Creating erd");

			if (_DataSources == null)
			{
				_DataSources = new()
				{
					DataSource = RctManager.DataSources,
					DependentSource = RctManager.Databases,
					PrimaryKey = "DataSourceLc",
					ForeignKey = "DataSourceLc"
				};

				_DataSources.CurrentChanged += OnDataSourcesCursorChanged;
				_DataSources.DependencyCurrentChanged += OnDatabasesCursorChanged;
			}

			return _DataSources;

		}
	}


	private bool InvalidDependent => DataSources.DependentRow == null
		|| DataSources.DependentRow["DatabaseLc"] == DBNull.Value
		|| (string)DataSources.DependentRow["DatabaseLc"] == "";



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if when execution has entered an input control event handler that
	/// may cause recursion on text changes.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool InputEventsDisabled
	{
		get { return _InputEventsCardinal > 0; }
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if when execution has entered a Site properties event handler
	/// that may cause recursion on property changes.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool PropertyEventsDisabled
	{
		get { return _PropertyEventsCardinal > 0; }
	}


	IBDataConnectionDlg SessionDlg => Parent != null ? Parent.Parent as IBDataConnectionDlg : null;

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

		AddEventHandlers();

		DisableInputEvents();

		try
		{
			// Tracer.Trace("Loading datasource text");

			// Fill out main screen input fields.

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


			// Move the cursor to it's correct position.

			if (!SetCursorPositionFromSite(false))
			{
				if (!_SiteChanged)
					InvalidateSiteProperties(false);
				return;
			}


			object @object;

			// Update the database name label.

			@object = DataSources.DependentRow[CoreConstants.C_KeyExDisplayName];

			lblCurrentDisplayName.Text = @object != DBNull.Value && @object != null
				? (string)@object : ControlsResources.TConnectionUIControl_NewDatabaseConnection;


			// All done if it's a site change.
			if (_SiteChanged)
				return;

			// From here everything is loaded from Advanced so all we need to do is
			// update the keys and database name label which will come from our
			// dependent's cursor position.

			DisablePropertyEvents();


			try
			{
				// We're leaving proposed keys from Advanced intact.

				if (Site.ContainsKey(CoreConstants.C_KeyExConnectionName)
					&& string.IsNullOrWhiteSpace((string)Site[CoreConstants.C_KeyExConnectionName]))
				{
					Site.Remove(CoreConstants.C_KeyExConnectionName);
				}

				if (Site.ContainsKey(CoreConstants.C_KeyExDatasetId)
					&& string.IsNullOrWhiteSpace((string)Site[CoreConstants.C_KeyExDatasetId]))
				{
					Site.Remove(CoreConstants.C_KeyExDatasetId);
				}


				@object = DataSources.DependentRow[CoreConstants.C_KeyExDatasetKey];
				if (@object != DBNull.Value && @object != null && (string)@object != string.Empty)
					Site[CoreConstants.C_KeyExDatasetKey] = (string)@object;
				else
					Site.Remove(CoreConstants.C_KeyExDatasetKey);

				@object = DataSources.DependentRow[CoreConstants.C_KeyExConnectionKey];
				if (@object != DBNull.Value && @object != null && (string)@object != string.Empty)
					Site[CoreConstants.C_KeyExConnectionKey] = (string)@object;
				else
					Site.Remove(CoreConstants.C_KeyExConnectionKey);

				@object = DataSources.DependentRow[CoreConstants.C_KeyExDataset];
				if (@object != DBNull.Value && @object != null && (string)@object != string.Empty)
					Site[CoreConstants.C_KeyExDataset] = (string)@object;
				else
					Site.Remove(CoreConstants.C_KeyExDataset);

				@object = DataSources.DependentRow[CoreConstants.C_KeyExConnectionSource];
				if (@object != DBNull.Value && @object != null && (EnConnectionSource)(int)@object > EnConnectionSource.None)
					Site[CoreConstants.C_KeyExConnectionSource] = (int)@object;
				else
					Site.Remove(CoreConstants.C_KeyExConnectionSource);

				Site.ValidateKeys();

			}
			finally
			{
				EnablePropertyEvents();
			}


		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			_SiteChanged = false;
			EnableInputEvents();
		}

	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - TConnectionUIControl
	// =========================================================================================================


	private void AddEventHandlers()
	{
		if (_EventsLoaded || Parent == null || Parent.Parent is not Form form)
			return;

		// Tracer.Trace(GetType(), "AddEventHandlers()", "Container: {0}, Parent: {1}, ParentForm: {2}, Enabled: {3}, Visible: {4}.",
		//	Container, Parent, ParentForm, Enabled, Visible);

		_EventsLoaded = true;
		_ParentParentForm = form;

		// form.FormClosing += OnFormClosing;
		// form.FormClosed += OnFormClosed;

		_OnVerifySettingsDelegate = Reflect.AddEventHandler(this, nameof(OnVerifySettings),
			BindingFlags.Instance | BindingFlags.NonPublic, form, "VerifySettings",
			BindingFlags.Instance | BindingFlags.Public);

		Button btnAccept = (Button)Reflect.GetFieldValue(form, "acceptButton",
			BindingFlags.Instance | BindingFlags.NonPublic);

		_OnAcceptDelegate = Reflect.AddEventHandler(this, nameof(OnAccept),
			BindingFlags.Instance | BindingFlags.NonPublic, btnAccept, "Click",
			BindingFlags.Instance | BindingFlags.Public);

		if (ConnectionSource == EnConnectionSource.Session)
			SessionDlg.UpdateServerExplorerChangedEvent += OnUpdateServerExplorerChanged;

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="CursorEventsDisabled"/> counter when execution enters
	/// an event handler to prevent recursion.
	/// Call DisableCursorEvents() whenever a method directly moves the cursor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void DisableCursorEvents()
	{
		_CursorEventsCardinal++;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="InputEventsDisabled"/> counter when execution enters an
	/// event handler to prevent recursion.
	/// Call DisableInputEvents() whenever a method directly changes an input
	/// control's text.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void DisableInputEvents()
	{
		_InputEventsCardinal++;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="PropertyEventsDisabled"/> counter when execution
	/// enters an event handler to prevent recursion.
	/// Call DisablePropertyEvents() whenever a method directly changes a Site property.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void DisablePropertyEvents()
	{
		_PropertyEventsCardinal++;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="CursorEventsDisabled"/> counter that was previously
	/// incremented by <see cref="DisableCursorEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EnableCursorEvents()
	{
		if (_CursorEventsCardinal == 0)
			Diag.Dug(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
		else
			_CursorEventsCardinal--;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="InputEventsDisabled"/> counter that was previously
	/// incremented by <see cref="DisableInputEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EnableInputEvents()
	{
		if (_InputEventsCardinal == 0)
			Diag.Dug(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
		else
			_InputEventsCardinal--;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="PropertyEventsDisabled"/> counter that was previously
	/// incremented by <see cref="DisablePropertyEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EnablePropertyEvents()
	{
		if (_PropertyEventsCardinal == 0)
			Diag.Dug(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
		else
			_PropertyEventsCardinal--;
	}



	private void RemoveEventHandlers()
	{
		if (!_EventsLoaded)
			return;

		// Tracer.Trace(GetType(), "RemoveEventHandlers()", "Container: {0}, Parent: {1}, ParentForm: {2}, Enabled: {3}, Visible: {4}.",
		//	Container, Parent, ParentForm, Enabled, Visible);

		Reflect.RemoveEventHandler(_ParentParentForm, "VerifySettings", _OnVerifySettingsDelegate,
			BindingFlags.Instance | BindingFlags.Public);

		Button btnAccept = (Button)Reflect.GetFieldValue(_ParentParentForm, "acceptButton",
			BindingFlags.Instance | BindingFlags.NonPublic);

		Reflect.RemoveEventHandler(btnAccept, "Click", _OnAcceptDelegate,
			BindingFlags.Instance | BindingFlags.Public);

		if (ConnectionSource == EnConnectionSource.Session)
			SessionDlg.UpdateServerExplorerChangedEvent -= OnUpdateServerExplorerChanged;

		_EventsLoaded = false;
		_ParentParentForm = null;
		_OnVerifySettingsDelegate = null;
		_OnAcceptDelegate = null;
	}




	private void RemoveGlyph(bool siteInitialization)
	{
		DisablePropertyEvents();

		try
		{
			EnConnectionSource source = (EnConnectionSource)Site[CoreConstants.C_KeyExConnectionSource];

			if (source == EnConnectionSource.HierarchyMarshaler || source == EnConnectionSource.Application
				|| source == EnConnectionSource.ExternalUtility)
			{
				int pos;
				string datasetId = (string)Site[CoreConstants.C_KeyExDatasetId];

				if ((pos = datasetId.IndexOf(RctManager.EdmDatasetGlyph)) != -1)
					datasetId = datasetId.Remove(pos, 1);
				else if ((pos = datasetId.IndexOf(RctManager.ProjectDatasetGlyph)) != -1)
					datasetId = datasetId.Remove(pos, 1);
				else if ((pos = datasetId.IndexOf(RctManager.UtilityDatasetGlyph)) != -1)
					datasetId = datasetId.Remove(pos, 1);

				if (pos != -1)
				{
					if (siteInitialization && ConnectionSource == EnConnectionSource.Session)
						_UnstrippedConnectionString = Site.ToString();

					Site[CoreConstants.C_KeyExDatasetId] = datasetId;
					Site[CoreConstants.C_KeyExDatasetKey] =
						SystemData.DatasetKeyFmt.FmtRes((string)Site[CoreConstants.C_KeyDataSource], datasetId);

					if (siteInitialization && ConnectionSource == EnConnectionSource.Session)
						_StrippedConnectionString = Site.ToString();
				}
			}
		}
		finally
		{
			EnablePropertyEvents();
		}
	}


	/// <summary>
	/// Restores all Site properties that may have been updated by RctManager.ValidateSiteProperties()
	/// </summary>
	private void RestoreSiteProperties(string restoreConnectionString)
	{
		DisableInputEvents();
		DisablePropertyEvents();


		((IBDataConnectionProperties)Site).Csa.ConnectionString = restoreConnectionString;

		EnablePropertyEvents();
		EnableInputEvents();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets the DataSources cursor positions using current Site values.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool SetCursorPositionFromSite(bool enableCursorEvents)
	{
		if (!DataSources.IsReady)
		{
			ApplicationException ex = new("ErmBindingSource is not configured");
			Diag.Dug(ex);
			throw ex;
		}

		if (!enableCursorEvents)
			DisableCursorEvents();

		try
		{
			if (Site == null)
			{
				DataSources.Position = 0;
				return false;
			}

			int position = 0;
			int dbPosition = 0;

			// First see if the DataSources cursor has changed.

			string dataSource = Site.ContainsKey(CoreConstants.C_KeyDataSource)
				? (string)Site[CoreConstants.C_KeyDataSource] : "";


			// Try to move the Datasource combo table to it's correct position.

			if (dataSource.Length > 0)
				position = DataSources.Find(dataSource);

			if (position == -1)
				position = 0;

			DataSources.Position = position;

			if (position == 0)
				return false;

			// Now the database.

			string connectionUrl = CsbAgent.CreateConnectionUrl(Site.ToString());

			// Try to move the Dependent combo table to it's correct position.

			if (connectionUrl != null)
				dbPosition = DataSources.FindDependent(CoreConstants.C_KeyExConnectionUrl, connectionUrl);

			if (dbPosition == -1)
				dbPosition = 0;


			if (DataSources.DependentPosition != dbPosition)
				DataSources.DependentPosition = dbPosition;

			return !InvalidDependent;
		}
		finally
		{
			if (!enableCursorEvents)
				EnableCursorEvents();
		}

	}



	public void ShowError(IUIService uiService, string title, Exception ex)
	{
		if (uiService != null)
		{
			uiService.ShowError(ex);
		}
		else
		{
			Diag.ExceptionService(typeof(IUIService));
			// RTLAwareMessageBox.Show(title, ex.Message, MessageBoxIcon.Exclamation);
		}
	}

	public void ShowError(string title, Exception ex)
	{
		IUIService uiService = Package.GetGlobalService(typeof(IUIService)) as IUIService;
		if (ex is AggregateException ex2)
		{
			ex2.Flatten().Handle(delegate (Exception e)
			{
				ShowError(uiService, title, e);
				return true;
			});
		}
		else
		{
			ShowError(uiService, title, ex);
		}
	}


	/// <summary>
	/// Invalidates key readonly properties if no underlying registered connection exists.
	/// </summary>
	private void InvalidateSiteProperties(bool removeProposed)
	{
		if (!InvalidDependent)
			return;

		DisablePropertyEvents();

		// Tracer.Trace(GetType(), "InvalidateSiteProperties()");

		try
		{
			Site.Remove(CoreConstants.C_KeyExDatasetKey);
			Site.Remove(CoreConstants.C_KeyExConnectionKey);
			Site.Remove(CoreConstants.C_KeyExDataset);

			if (removeProposed)
			{
				Site.Remove(CoreConstants.C_KeyExConnectionName);
				Site.Remove(CoreConstants.C_KeyExDatasetId);
			}

			Site[CoreConstants.C_KeyExConnectionSource] = ConnectionSource;

			lblCurrentDisplayName.Text = ControlsResources.TConnectionUIControl_NewDatabaseConnection;

			EnConnectionSource connectionSource = ConnectionSource == EnConnectionSource.HierarchyMarshaler
				? EnConnectionSource.ServerExplorer : ConnectionSource;

			if (connectionSource == EnConnectionSource.Session
				&& (SessionDlg == null || SessionDlg.UpdateServerExplorer))
			{
				connectionSource = EnConnectionSource.ServerExplorer;
			}

			Site[CoreConstants.C_KeyExConnectionSource] = connectionSource;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnablePropertyEvents();
		}

	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - TConnectionUIControl
	// =========================================================================================================


	/// <summary>
	/// On this event we simply validate the form settings. If there's an issue we either
	/// resolve it auto or request approval to continue or abort the save.
	/// No updating will take place. Only site properties may be updated.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void OnAccept(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnAccept()", "ConnectionSource: {0}.", ConnectionSource);

		_HandleNewInternally = false;
		_HandleModifyInternally = false;
		_HandleVerification = true;

		if (ConnectionSource == EnConnectionSource.Application)
			return;


		if (RctManager.ShutdownState)
			return;

		if (Site is not IVsDataConnectionProperties site)
		{
			InvalidCastException ex = new($"Cannot cast IVsDataConnectionUIProperties Site to IVsDataConnectionProperties.");
			throw ex;
		}

		// Tracer.Trace(GetType(), "OnAccept()");

		DisableCursorEvents();
		DisableInputEvents();
		DisablePropertyEvents();

		try
		{
			// Special case. If ConnectionSource is Session and Properties have not changed
			// since stripping any glyph on the initial load, just reset to pre-stripping
			// state and return.
			// We don't want the glyph stripped on accept when nothing has changed.

			if (_StrippedConnectionString != null)
			{
				CsbAgent csa1 = new(_StrippedConnectionString, false);
				CsbAgent csa2 = new(Site.ToString(), false);

				if (CsbAgent.AreEquivalent(csa1, csa2, CsbAgent.DescriberKeys, true))
				{
					// They are equivalent. Validate here.

					if (PersistentSettings.ValidateConnectionOnFormAccept)
					{
						FbConnection connection = new(site.ToString());

						connection.Open();
					}

					// All ok so it's safe to restore the glyphs and let connection through as
					// unchanged.
					_HandleVerification = false;

					if (_UnstrippedConnectionString != null) 
						RestoreSiteProperties(_UnstrippedConnectionString);

					return;
				}

			}

			string restoreConnectionString = Site.ToString();

			// Tracer.Trace(GetType(), "OnAccept()", "restoreConnectionString: {0}.", restoreConnectionString);

			try
			{
				bool serverExplorerInsertMode = _InsertMode && ConnectionSource == EnConnectionSource.ServerExplorer;

				(bool success, bool addInternally, bool modifyInternally)
					= RctManager.ValidateSiteProperties(site, ConnectionSource, serverExplorerInsertMode, _OriginalConnectionString);

				if (!success)
				{
					(Parent.Parent as Form).DialogResult = DialogResult.None;

					RestoreSiteProperties(restoreConnectionString);

					return;
				}


				// This only applies if the ConnectionSource is SqlEditor.

				if (ConnectionSource == EnConnectionSource.Session)
					addInternally &= SessionDlg.UpdateServerExplorer;


				// This validation test will be duplicated if ConnectionSource != EnConnectionSource.Session but this is the only
				// opportunity to control a failed connect.
				if (ConnectionSource != EnConnectionSource.Session || addInternally || modifyInternally
					|| PersistentSettings.ValidateConnectionOnFormAccept)
				{
					FbConnection connection = new(site.ToString());

					connection.Open();
				}

				// If a new unique SE connection is going to be created in a Session set the connection source.
				if (ConnectionSource == EnConnectionSource.Session && addInternally)
					site[CoreConstants.C_KeyExConnectionKey] = site[CoreConstants.C_KeyExDatasetKey];

				_HandleNewInternally = addInternally;
				_HandleModifyInternally = modifyInternally;


			}
			catch
			{
				RestoreSiteProperties(restoreConnectionString);
				throw;
			}
		}
		finally
		{
			EnablePropertyEvents();
			EnableInputEvents();
			EnableCursorEvents();
		}

		// Tracer.Trace(GetType(), "OnAccept()", "Exiting Site.ToString(): {0}.", Site.ToString());
	}



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
	/// Handles changes the underlying <see cref="ErmBindingSource.Dependent"/>'s <see cref="ErmBindingSource.DependentRow"/>
	/// linked to <see cref="cmbDatabase"/>.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	private void OnDatabasesCursorChanged(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnDatabasesCursorChanged()");

		if (CursorEventsDisabled)
			return;

		if (InvalidDependent)
		{
			InvalidateSiteProperties(true);
			return;
		}

		DisableCursorEvents();
		DisableInputEvents();
		DisablePropertyEvents();

		try
		{
			if (txtDatabase.Text.ToLower() != (string)DataSources.DependentRow["DatabaseLc"])
			{
				txtDatabase.Text = ((string)DataSources.DependentRow["Database"]).Trim();
				if (Site != null)
				{
					// if (txtDatabase.Text == "")
					//	Site.Remove("Database");
					// else
					Site["Database"] = txtDatabase.Text;
				}
			}

			int selectedIndex = cboCharset.SelectedIndex;

			cboCharset.SelectedValue = (string)DataSources.DependentRow["Charset"];

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

			if ((string)DataSources.DependentRow["UserID"] != "")
			{
				txtUserName.Text = (string)DataSources.DependentRow["UserID"];
				txtPassword.Text = (string)DataSources.DependentRow["Password"];
				txtRole.Text = (string)DataSources.DependentRow["Role"];

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

			object @object = DataSources.DependentRow[CoreConstants.C_KeyExDisplayName];

			lblCurrentDisplayName.Text = @object != DBNull.Value && @object != null
				? (string)@object : ControlsResources.TConnectionUIControl_NewDatabaseConnection;

			if (Site != null)
			{
				foreach (Describer describer in CsbAgent.AdvancedKeys)
				{
					if (!describer.DefaultEqualsOrEmpty(DataSources.DependentRow[describer.Name]))
					{
						Site[describer.Key] = describer.DataType == typeof(int)
							? Convert.ToInt32(DataSources.DependentRow[describer.Name])
							: DataSources.DependentRow[describer.Name];
					}
					else
					{
						Site.Remove(describer.Key);
					}
				}

				Site.ValidateKeys();

				// Take the glyph out of Application, HierarchyMarshaler and ExternalUtility source
				// dataset ids.
				if ((ConnectionSource != EnConnectionSource.Application)
					&& Site.ContainsKey(CoreConstants.C_KeyExDatasetId)
					&& Site.ContainsKey(CoreConstants.C_KeyExConnectionSource))
				{
					RemoveGlyph(false);
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnablePropertyEvents();
			EnableInputEvents();
			EnableCursorEvents();
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
	private void OnDataSourcesCursorChanged(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnDataSourcesCursorChanged()");

		if (CursorEventsDisabled)
			return;

		if (DataSources.Row == null || (int)DataSources.Row["Orderer"] == 0)
			return;

		DisableCursorEvents();
		DisableInputEvents();
		DisablePropertyEvents();

		try
		{
			if ((int)DataSources.Row["Orderer"] == 1)
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
					foreach (Describer describer in CsbAgent.Describers.DescriberKeys)
					{
						if (describer.IsAdvanced || describer.IsConnectionParameter)
							Site.Remove(describer.Key);
					}
				}

				Site[CoreConstants.C_KeyExConnectionSource] = ConnectionSource;

				DataSources.Position = 0;
			}
			else
			{
				if (txtDataSource.Text.ToLower() != (string)DataSources.Row["DataSourceLc"])
				{
					txtDataSource.Text = (string)DataSources.Row["DataSource"];
					if (Site != null)
					{
						// if (txtDataSource.Text.Trim() == "")
						//	Site.Remove("DataSource");
						// else
						Site["DataSource"] = txtDataSource.Text.Trim();
					}
				}


				if ((int)DataSources.Row["Port"] != 0 && txtPort.Text != DataSources.Row["Port"].ToString())
				{
					txtPort.Text = DataSources.Row["Port"].ToString();
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
			EnablePropertyEvents();
			EnableInputEvents();
			EnableCursorEvents();
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for main screen input controls
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	// ---------------------------------------------------------------------------------
	private void OnInputChanged(object sender, EventArgs e)
	{
		if (Site == null || InputEventsDisabled)
			return;

		// Tracer.Trace(GetType(), "OnInputChanged()", "Container: {0}.", Container);

		// Disable property property events because we're going to invoke
		// OnPropertyChanged afterwards.

		DisableInputEvents();
		DisablePropertyEvents();

		try
		{
			string propertyName;

			if (sender.Equals(txtDataSource))
			{
				propertyName = CoreConstants.C_KeyDataSource;
				Site[propertyName] = txtDataSource.Text.Trim();
			}
			else if (sender.Equals(txtDatabase))
			{
				propertyName = CoreConstants.C_KeyDatabase;
				Site[propertyName] = txtDatabase.Text.Trim();
			}
			else if (sender.Equals(txtUserName))
			{
				propertyName = CoreConstants.C_KeyUserID;
				Site[propertyName] = txtUserName.Text.Trim();
			}
			else if (sender.Equals(txtPassword))
			{
				propertyName = CoreConstants.C_KeyPassword;
				Site[propertyName] = txtPassword.Text.Trim();
			}
			else if (sender.Equals(txtRole))
			{
				propertyName = ModelConstants.C_KeyRole;
				if (txtRole.Text.Trim() == "")
					Site.Remove(propertyName);
				else
					Site[propertyName] = txtRole.Text.Trim();
			}
			else if (sender.Equals(cboCharset))
			{
				propertyName = ModelConstants.C_KeyCharset;
				if (cboCharset.Text.Trim() == "" || cboCharset.Text.Trim().ToUpper() == ModelConstants.C_DefaultCharset)
					Site.Remove(propertyName);
				else
					Site[propertyName] = cboCharset.Text.Trim();
			}
			else if (sender.Equals(txtPort))
			{
				propertyName = CoreConstants.C_KeyPort;
				if (String.IsNullOrWhiteSpace(txtPort.Text) || Convert.ToInt32(txtPort.Text.Trim()) == CoreConstants.C_DefaultPort)
					Site.Remove(propertyName);
				else
					Site[propertyName] = Convert.ToInt32(txtPort.Text);
			}
			else if (sender.Equals(cboDialect))
			{
				propertyName = ModelConstants.C_KeyDialect;
				if (String.IsNullOrWhiteSpace(cboDialect.Text) || Convert.ToInt32(cboDialect.Text.Trim()) == ModelConstants.C_DefaultDialect)
					Site.Remove(propertyName);
				else
					Site[propertyName] = Convert.ToInt32(cboDialect.Text);
			}
			else if (sender.Equals(cboServerType))
			{
				propertyName = CoreConstants.C_KeyServerType;
				if (cboServerType.SelectedIndex == -1 || cboServerType.SelectedIndex == (int)CoreConstants.C_DefaultServerType)
					Site.Remove(propertyName);
				else
					Site[propertyName] = cboServerType.SelectedIndex;
			}
			else
			{
				return;
			}


			if (!SetCursorPositionFromSite(true))
				InvalidateSiteProperties(false);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnablePropertyEvents();
			EnableInputEvents();
		}

	}


	protected override void OnParentChanged(EventArgs e)
	{
		if (Parent != null)
			AddEventHandlers();
		else
			RemoveEventHandlers();

		base.OnParentChanged(e);
	}


	protected override void OnSiteChanged(EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnSiteChanged()", "Site.ToString(): {0}, Site.Count: {1}", Site != null ? Site.ToString() : "Null",
		//	Site != null ? Site.Count : -1);

		_OriginalConnectionString = null;
		_StrippedConnectionString = null;
		_UnstrippedConnectionString = null;
		_InsertMode = true;
		_SiteChanged = true;

		if (Site != null)
		{
			_OriginalConnectionString = Site.ToString();

			if (string.IsNullOrWhiteSpace(_OriginalConnectionString))
				_OriginalConnectionString = null;

			_InsertMode = _OriginalConnectionString == null;

			EnConnectionSource storedConnectionSource = Site.ContainsKey(CoreConstants.C_KeyExConnectionSource)
				? (EnConnectionSource)Site[CoreConstants.C_KeyExConnectionSource] : EnConnectionSource.Undefined;

			DisablePropertyEvents();

			try
			{

				if (ConnectionSource == EnConnectionSource.Application)
				{
					foreach (Describer describer in CsbAgent.AdvancedKeys)
					{
						if (!describer.IsConnectionParameter && describer.Key != CoreConstants.C_KeyExConnectionSource)
							Site.Remove(describer.Key);
					}

					if (storedConnectionSource <= EnConnectionSource.None)
						Site[CoreConstants.C_KeyExConnectionSource] = ConnectionSource;
				}
				else
				{
					if (ConnectionSource == EnConnectionSource.ServerExplorer
						|| storedConnectionSource == EnConnectionSource.ServerExplorer)
					{
						string connectionKey = Site.FindConnectionKey();

						if (connectionKey != null)
							Site[CoreConstants.C_KeyExConnectionKey] = connectionKey;
						else
							Site.Remove(CoreConstants.C_KeyExConnectionKey);
					}

					if (Site.ContainsKey(CoreConstants.C_KeyExDatasetId)
						&& (storedConnectionSource == EnConnectionSource.Application
						|| storedConnectionSource == EnConnectionSource.HierarchyMarshaler
						|| storedConnectionSource == EnConnectionSource.ExternalUtility))
					{
						RemoveGlyph(true);
					}

					if (ConnectionSource == EnConnectionSource.Session && _StrippedConnectionString == null)
						_StrippedConnectionString = Site.ToString();


					if (!Site.ContainsKey(CoreConstants.C_KeyExConnectionSource)
						|| (EnConnectionSource)Site[CoreConstants.C_KeyExConnectionSource] <= EnConnectionSource.None)
					{
						EnConnectionSource connectionSource = ConnectionSource == EnConnectionSource.HierarchyMarshaler
							? EnConnectionSource.ServerExplorer : ConnectionSource;

						if (connectionSource == EnConnectionSource.Session
							&& (SessionDlg == null || SessionDlg.UpdateServerExplorer))
						{
							connectionSource = EnConnectionSource.ServerExplorer;
						}

						Site[CoreConstants.C_KeyExConnectionSource] = connectionSource;
					}

				}


				Site.ValidateKeys();

				// Site.OnPropertiesChanged += OnPropertyChanged;

			}
			finally
			{
				EnablePropertyEvents();
			}

		}

		base.OnSiteChanged(e);
	}


	/// <summary>
	/// Apply any changes to the Rct and SE.
	/// </summary>
	private void OnVerifySettings(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnVerifySettings()", "Container: {0}, Parent: {1}, ParentForm: {2}, Enabled: {3}, Visible: {4}.",
		//	Container, Parent, ParentForm, Enabled, Visible);

		// Validate the new parse request connection string and then apply.


		if (RctManager.ShutdownState)
			return;

		if (ConnectionSource == EnConnectionSource.Application || !_HandleVerification
			|| (ConnectionSource == EnConnectionSource.HierarchyMarshaler && !_HandleNewInternally
			&& !_HandleModifyInternally))
		{
			// Tracer.Trace(GetType(), "OnVerifySettings()", "Not handling internally.");
			return;
		}

		// Tracer.Trace(GetType(), "OnVerifySettings()", "Before Verify - ConnectionSource: {0}, _HandleNewInternally: {1}, _HandleModifyInternally: {2}.", ConnectionSource, _HandleNewInternally, _HandleModifyInternally);


		EnConnectionSource registrationSource =
			(ConnectionSource == EnConnectionSource.HierarchyMarshaler
			|| (ConnectionSource == EnConnectionSource.Session && _HandleNewInternally))
			? EnConnectionSource.ServerExplorer : ConnectionSource;



		// Special case. If an existing connection is updated we lock it's parser against
		// disposal in IVsDataViewSupport.
		if (ConnectionSource == EnConnectionSource.ServerExplorer
			&& !_HandleNewInternally && !_HandleModifyInternally && !_InsertMode)
		{
			// Lock the parser if properties that have been changed don't affect the parser.
			LinkageParser.LockLoadedParser(Site.ToString());
		}

		try
		{
			// Try to update an existing instance.
			RctManager.UpdateOrRegisterConnection(Site.ToString(), registrationSource, _HandleNewInternally, _HandleModifyInternally);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		if (ConnectionSource == EnConnectionSource.ServerExplorer)
		{
			if (_HandleNewInternally || _HandleModifyInternally)
			{
				(Parent.Parent as Form).DialogResult = DialogResult.Cancel;
			}
			else if (_InsertMode)
			{
				RctManager.StoreUnadvisedConnection(Site.ToString());
			}
		}

		// Tracer.Trace(GetType(), "OnVerifySettings()", "After Verify - ConnectionSource: {0}, Site.ToString(): {1}.", ConnectionSource, Site.ToString());

	}



	private void OnUpdateServerExplorerChanged(object sender, EventArgs e)
	{
		if (Site == null || !InvalidDependent)
			return;

		DisablePropertyEvents();

		try
		{
			if (SessionDlg.UpdateServerExplorer)
				Site[CoreConstants.C_KeyExConnectionSource] = EnConnectionSource.ServerExplorer;
			else
				Site[CoreConstants.C_KeyExConnectionSource] = ConnectionSource;
		}
		finally
		{
			EnablePropertyEvents();
		}

	}


	#endregion Event handlers
}