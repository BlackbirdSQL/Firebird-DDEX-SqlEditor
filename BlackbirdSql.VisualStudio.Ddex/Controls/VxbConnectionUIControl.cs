// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Config;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.VisualStudio.Ddex.Controls;


// =========================================================================================================
//										VxbConnectionUIControl Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionUIControl"/> interface
/// </summary>
// =========================================================================================================
public partial class VxbConnectionUIControl : DataConnectionUIControl
{


	// -----------------------------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbConnectionUIControl
	// -----------------------------------------------------------------------------------------------------


	public VxbConnectionUIControl() : base()
	{
		Diag.ThrowIfNotOnUIThread();

		RctManager.EventConnectionDialogEnter(false, true);


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
			else if (ConnectionSource == EnConnectionSource.EntityDataModel)
			{
				lblDatasetKeyDescription.Text = ControlsResources.TConnectionUIControl_DatasetKeyDescription_EntityDataModel;
			}
			else
			{
				lblDatasetKeyDescription.Text = ControlsResources.TConnectionUIControl_DatasetKeyDescription;
			}

			// Evs.Trace("Creating erd");

			cmbDataSource.DataSource = DataSources;
			cmbDataSource.ValueMember = "DataSourceLc";
			cmbDataSource.DisplayMember = "DataSource";

			cmbDatabase.DataSource = DataSources.Dependent;
			cmbDatabase.ValueMember = "DatabaseLc";
			cmbDatabase.DisplayMember = CoreConstants.C_KeyExAdornedDisplayName;

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
		RctManager.EventConnectionDialogExit();

		if (disposing && (components != null))
		{
			DataSources.CurrentChanged -= OnDataSourcesCursorChanged;
			DataSources.DependencyCurrentChanged -= OnDatabasesCursorChanged;

			RemoveEventHandlers();

			components.Dispose();
		}

		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - VxbConnectionUIControl
	// =========================================================================================================

	private readonly object _LockLocal = new();

	private ErmBindingSource _DataSources;
	private string _DatabasePathName = null;
	private int _EventInputCardinal = 0;
	private int _EventCursorCardinal = 0;
	private int _EventPropertyCardinal = 0;
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


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - VxbConnectionUIControl
	// =========================================================================================================


	private EnConnectionSource ConnectionSource
	{
		get
		{
			if (_ConnectionSource == EnConnectionSource.Undefined)
			{
				_ConnectionSource = SessionDlg != null
					? EnConnectionSource.Session
					: RctManager.ConnectionSource;
			}

			return _ConnectionSource;
		}
	}



	private ErmBindingSource DataSources
	{
		get
		{
			if (_DataSources == null)
			{
				_DataSources = new()
				{
					DataSource = RctManager.Servers,
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


	IBsDataConnectionDlg SessionDlg => Parent != null ? Parent.Parent as IBsDataConnectionDlg : null;

	#endregion Property accessors




	// =========================================================================================================
	#region Method Implementations - VxbConnectionUIControl
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads a previously saved connection string's properties into the form
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void LoadProperties()
	{

		// Evs.Trace(GetType(), nameof(LoadProperties));

		AddEventHandlers();

		EventInputEnter(false, true);

		try
		{
			// Fill out main screen input fields.

			if (Site != null && Site.TryGetValue("DataSource", out object value))
				txtDataSource.Text = (string)value;
			else
				txtDataSource.Text = SysConstants.C_DefaultDataSource;

			if (txtDataSource.Text != "")
				cmbDataSource.SelectedValue = txtDataSource.Text.ToLower();
			else
				cmbDataSource.SelectedIndex = -1;


			if (Site != null && Site.TryGetValue("User ID", out value))
				txtUserName.Text = (string)value;
			else
				txtUserName.Text = SysConstants.C_DefaultUserID;

			if (Site != null && Site.TryGetValue("Database", out value))
				txtDatabase.Text = Cmd.CleanPath((string)value);
			else
				txtDatabase.Text = SysConstants.C_DefaultDatabase;


			if (Site != null && Site.TryGetValue("Password", out value))
				txtPassword.Text = (string)value;
			else
				txtPassword.Text = SysConstants.C_DefaultPassword;


			if (Site != null && Site.TryGetValue("Role", out value))
				txtRole.Text = (string)value;
			else
				txtRole.Text = SysConstants.C_DefaultRole;

			if (Site != null && Site.TryGetValue("Character Set", out value))
				cboCharset.SetSelectedValueX(value);
			else
				cboCharset.SetSelectedValueX(SysConstants.C_DefaultCharset);

			if (Site != null && Site.TryGetValue("Port", out value))
				txtPort.Text = (string)value;
			else
				txtPort.Text = SysConstants.C_DefaultPort.ToString();

			if (Site != null && Site.TryGetValue("Dialect", out value))
				cboDialect.SetSelectedValueX(value);
			else
				cboDialect.SetSelectedValueX(SysConstants.C_DefaultDialect);

			if (Site != null && Site.TryGetValue("ServerType", out value))
				cboServerType.SelectedIndex = Convert.ToInt32((string)value);
			else
				cboServerType.SelectedIndex = (int)SysConstants.C_DefaultServerType;


			// Move the cursor to it's correct position.

			if (!SetCursorPositionFromSite(false))
			{
				if (!_SiteChanged)
					InvalidateSiteProperties(false);
				return;
			}


			object @object;

			// Update the database name label.

			@object = DataSources.DependentRow[CoreConstants.C_KeyExAdornedQualifiedName];

			lblCurrentDisplayName.Text = !Cmd.IsNullValue(@object)
				? (string)@object : ControlsResources.TConnectionUIControl_NewDatabaseConnection;


			// All done if it's a site change.
			if (_SiteChanged)
				return;

			// From here everything is loaded from Advanced so all we need to do is
			// update the keys and database name label which will come from our
			// dependent's cursor position.

			EventPropertyEnter(false, true);


			try
			{
				// We're leaving proposed keys from Advanced intact.

				if (Site.ContainsKey(SysConstants.C_KeyExConnectionName)
					&& string.IsNullOrWhiteSpace((string)Site[SysConstants.C_KeyExConnectionName]))
				{
					Site.Remove(SysConstants.C_KeyExConnectionName);
				}

				if (Site.ContainsKey(SysConstants.C_KeyExDatasetId)
					&& string.IsNullOrWhiteSpace((string)Site[SysConstants.C_KeyExDatasetId]))
				{
					Site.Remove(SysConstants.C_KeyExDatasetId);
				}


				@object = DataSources.DependentRow[CoreConstants.C_KeyExDatasetKey];
				if (!Cmd.IsNullValue(@object) && (string)@object != "")
					Site[CoreConstants.C_KeyExDatasetKey] = (string)@object;
				else
					Site.Remove(CoreConstants.C_KeyExDatasetKey);

				@object = DataSources.DependentRow[CoreConstants.C_KeyExConnectionKey];
				if (!Cmd.IsNullValue(@object) && (string)@object != "")
					Site[CoreConstants.C_KeyExConnectionKey] = (string)@object;
				else
					Site.Remove(CoreConstants.C_KeyExConnectionKey);

				@object = DataSources.DependentRow[CoreConstants.C_KeyExDataset];
				if (!Cmd.IsNullValue(@object) && (string)@object != "")
					Site[CoreConstants.C_KeyExDataset] = (string)@object;
				else
					Site.Remove(CoreConstants.C_KeyExDataset);

				@object = DataSources.DependentRow[CoreConstants.C_KeyExConnectionSource];
				if (!Cmd.IsNullValue(@object) && (EnConnectionSource)(int)@object > EnConnectionSource.Unknown)
					Site[CoreConstants.C_KeyExConnectionSource] = (int)@object;
				else
					Site.Remove(CoreConstants.C_KeyExConnectionSource);

				Site.ValidateKeys();

			}
			finally
			{
				EventPropertyExit();
			}


		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			_SiteChanged = false;
			EventInputExit();
		}

	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - VxbConnectionUIControl
	// =========================================================================================================


	private void AddEventHandlers()
	{
		if (_EventsLoaded || Parent == null || Parent.Parent is not Form form)
			return;

		// Evs.Trace(GetType(), nameof(AddEventHandlers), "Container: {0}, Parent: {1}, ParentForm: {2}, Enabled: {3}, Visible: {4}.",
		//	Container, Parent, ParentForm, Enabled, Visible);

		_EventsLoaded = true;
		_ParentParentForm = form;

		_OnVerifySettingsDelegate = Reflect.AddEventHandler(this, nameof(OnVerifySettings), form, "VerifySettings");

		Button btnAccept = (Button)Reflect.GetFieldValue(form, "acceptButton");

		_OnAcceptDelegate = Reflect.AddEventHandler(this, nameof(OnAccept), btnAccept, "Click");

		if (ConnectionSource == EnConnectionSource.Session)
			SessionDlg.UpdateServerExplorerChangedEvent += OnUpdateServerExplorerChanged;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventCursorCardinal"/> counter when execution
	/// enters a Cursor event handler to prevent recursion.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool EventCursorEnter(bool test = false, bool force = false)
	{
		lock (_LockLocal)
		{
			if (_EventCursorCardinal != 0 && !force)
				return false;

			if (!test)
				_EventCursorCardinal++;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventCursorCardinal"/> counter that was previously
	/// incremented by <see cref="EventCursorEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EventCursorExit()
	{
		lock (_LockLocal)
		{
			if (_EventCursorCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit Cursor event when not in a Cursor event. _EventCursorCardinal: {_EventCursorCardinal}");
				Diag.Dug(ex);
				throw ex;
			}
			_EventCursorCardinal--;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventInputCardinal"/> counter when execution
	/// enters an Input event handler to prevent recursion.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool EventInputEnter(bool test = false, bool force = false)
	{
		lock (_LockLocal)
		{
			if (_EventInputCardinal != 0 && !force)
				return false;

			if (!test)
				_EventInputCardinal++;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventInputCardinal"/> counter that was previously
	/// incremented by <see cref="EventInputEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EventInputExit()
	{
		lock (_LockLocal)
		{
			if (_EventInputCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit Cursor event when not in a Cursor event. _EventCursorCardinal: {_EventInputCardinal}");
				Diag.Dug(ex);
				throw ex;
			}
			_EventInputCardinal--;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventPropertyCardinal"/> counter when execution
	/// enters a Property event handler to prevent recursion.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool EventPropertyEnter(bool test = false, bool force = false)
	{
		lock (_LockLocal)
		{
			if (_EventPropertyCardinal != 0 && !force)
				return false;

			if (!test)
				_EventPropertyCardinal++;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventPropertyCardinal"/> counter that was previously
	/// incremented by <see cref="EventPropertyEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EventPropertyExit()
	{
		lock (_LockLocal)
		{
			if (_EventPropertyCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit Cursor event when not in a Cursor event. _EventCursorCardinal: {_EventPropertyCardinal}");
				Diag.Dug(ex);
				throw ex;
			}
			_EventPropertyCardinal--;
		}
	}



	private void RemoveEventHandlers()
	{
		if (!_EventsLoaded)
			return;

		// Evs.Trace(GetType(), nameof(RemoveEventHandlers), "Container: {0}, Parent: {1}, ParentForm: {2}, Enabled: {3}, Visible: {4}.",
		//	Container, Parent, ParentForm, Enabled, Visible);

		Reflect.RemoveEventHandler(_ParentParentForm, "VerifySettings", _OnVerifySettingsDelegate);

		Button btnAccept = (Button)Reflect.GetFieldValue(_ParentParentForm, "acceptButton");

		Reflect.RemoveEventHandler(btnAccept, "Click", _OnAcceptDelegate);

		if (ConnectionSource == EnConnectionSource.Session)
			SessionDlg.UpdateServerExplorerChangedEvent -= OnUpdateServerExplorerChanged;

		_EventsLoaded = false;
		_ParentParentForm = null;
		_OnVerifySettingsDelegate = null;
		_OnAcceptDelegate = null;
	}



	/// <summary>
	/// Restores all Site properties that may have been updated by RctManager.ValidateSiteProperties()
	/// </summary>
	private void RestoreSiteProperties(string restoreConnectionString)
	{
		EventInputEnter(false, true);
		EventPropertyEnter(false, true);

		try
		{
			((IBsDataConnectionProperties)Site).Csa.ConnectionString = restoreConnectionString;
		}
		finally
		{
			EventPropertyExit();
			EventInputExit();
		}
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

		// Evs.Trace(GetType(), nameof(SetCursorPositionFromSite), "enableCursorEvents: {0}.", enableCursorEvents);

		if (!enableCursorEvents)
			EventCursorEnter(false, true);

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

			string dataSource = Site.ContainsKey(SysConstants.C_KeyDataSource)
				? (string)Site[SysConstants.C_KeyDataSource] : "";


			// Try to move the Datasource combo table to it's correct position.

			if (dataSource.Length > 0)
				position = DataSources.Find(dataSource);

			if (position == -1)
				position = 0;

			DataSources.Position = position;

			if (position == 0)
				return false;

			// Now the database.

			string connectionUrl = Csb.CreateConnectionUrl(Site.ToString());

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
				EventCursorExit();
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

		EventPropertyEnter(false, true);

		// Evs.Trace(GetType(), nameof(InvalidateSiteProperties));

		try
		{
			Site.Remove(CoreConstants.C_KeyExDatasetKey);
			Site.Remove(CoreConstants.C_KeyExConnectionKey);
			Site.Remove(CoreConstants.C_KeyExDataset);

			if (removeProposed)
			{
				Site.Remove(SysConstants.C_KeyExConnectionName);
				Site.Remove(SysConstants.C_KeyExDatasetId);
			}

			Site[CoreConstants.C_KeyExConnectionSource] = ConnectionSource;

			lblCurrentDisplayName.Text = ControlsResources.TConnectionUIControl_NewDatabaseConnection;

			EnConnectionSource connectionSource = ConnectionSource == EnConnectionSource.EntityDataModel
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
			EventPropertyExit();
		}

	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - VxbConnectionUIControl
	// =========================================================================================================


	/// <summary>
	/// On this event we simply validate the form settings. If there's an issue we either
	/// resolve it auto or request approval to continue or abort the save.
	/// No updating will take place. Only site properties may be updated.
	/// </summary>
	private void OnAccept(object sender, EventArgs e)
	{
		// Evs.Trace(GetType(), nameof(OnAccept), "ConnectionSource: {0}.", ConnectionSource);

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

		// Evs.Trace(GetType(), nameof(OnAccept));
		IDbConnection connection = null;


		EventCursorEnter(false, true);
		EventInputEnter(false, true);
		EventPropertyEnter(false, true);

		try
		{
			// If SE or Session connections are equal to the original, exit.

			if ((ConnectionSource == EnConnectionSource.Session
				|| ConnectionSource == EnConnectionSource.ServerExplorer)
				&& _OriginalConnectionString != null)
			{

				string originalConnectionString = _OriginalConnectionString;
				Csb csa2 = new(Site.ToString(), false);

				if (Csb.AreEquivalent(originalConnectionString, Site.ToString(), Csb.DescriberKeys))
				{
					// They are equivalent. Validate here.

					if (ConnectionSource == EnConnectionSource.Session
						&& PersistentSettings.ValidateSessionConnectionOnFormAccept)
					{
						connection = NativeDb.CreateDbConnection(site.ToString());

						connection.Open();
					}

					if (ConnectionSource == EnConnectionSource.ServerExplorer)
					{
						(Parent.Parent as Form).DialogResult = DialogResult.Cancel;
						return;
					}

					// All ok so it's safe to restore the glyphs and let connection through as
					// unchanged.
					_HandleVerification = false;

					RestoreSiteProperties(originalConnectionString);

					return;
				}

			}

			string restoreConnectionString = Site.ToString();
			// Evs.Trace(GetType(), nameof(OnAccept), "restoreConnectionString: {0}.", restoreConnectionString);

			try
			{
				bool serverExplorerInsertMode = _InsertMode && ConnectionSource == EnConnectionSource.ServerExplorer;
				bool createServerExplorerConnection = ConnectionSource == EnConnectionSource.Session && SessionDlg.UpdateServerExplorer;


				(bool success, bool addInternally, bool modifyInternally)
					= RctManager.ValidateSiteProperties(site, ConnectionSource, serverExplorerInsertMode, createServerExplorerConnection, _OriginalConnectionString);

				if (!success)
				{
					(Parent.Parent as Form).DialogResult = DialogResult.None;

					RestoreSiteProperties(restoreConnectionString);

					return;
				}

				// Evs.Trace(GetType(), nameof(OnAccept), "ValidateSiteProperties(): success: {0}, addInternally: {1}, modifyInternally: {2}\nrestoreConnectionString: {3}.",
				//	success, addInternally, modifyInternally, restoreConnectionString);


				// This only applies if the ConnectionSource is SqlEditor.

				addInternally &= (createServerExplorerConnection || ConnectionSource != EnConnectionSource.Session);


				// This validation test will be duplicated if ConnectionSource != EnConnectionSource.Session but this is the only
				// opportunity to control a failed connect.
				if (ConnectionSource != EnConnectionSource.Session || addInternally || modifyInternally
					|| PersistentSettings.ValidateSessionConnectionOnFormAccept)
				{
					connection ??= NativeDb.CreateDbConnection(site.ToString());

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
			if (connection != null)
			{
				if (connection.State == ConnectionState.Open)
					connection.Close();
				connection.Dispose();
			}

			EventPropertyExit();
			EventInputExit();
			EventCursorExit();
		}

		// The connection may not be open and when opened will fire the Rct OnExplorerConnectionNodeChanged()
		// event. Disable it.

		if (ConnectionSource == EnConnectionSource.ServerExplorer)
			RctManager.ExternalEventSinkEnter(false, true);

		// Evs.Trace(GetType(), nameof(OnAccept), "Completed. Site.ToString(): {0}.", Site.ToString());
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
		if ((!string.IsNullOrEmpty(_DatabasePathName)
			&& _DatabasePathName != txtDatabase.Text.Trim().ToLowerInvariant())
			|| (string.IsNullOrEmpty(_DatabasePathName)
			&& !string.IsNullOrEmpty(txtDatabase.Text)))
		{
			_DatabasePathName = txtDatabase.Text.Trim().ToLowerInvariant();
			openFileDialog.InitialDirectory = Cmd.GetDirectoryName(txtDatabase.Text.Trim());

			string filename = Cmd.GetFileName(txtDatabase.Text.Trim());

			if (!string.IsNullOrWhiteSpace(filename))
				openFileDialog.FileName = filename;
		}

		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			txtDatabase.Text = Cmd.CleanPath(openFileDialog.FileName);
			_DatabasePathName = txtDatabase.Text.Trim().ToLowerInvariant();
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
		// Evs.Trace(GetType(), nameof(OnDatabasesCursorChanged));

		if (!EventCursorEnter())
			return;

		if (InvalidDependent)
		{
			EventCursorExit();
			InvalidateSiteProperties(true);
			return;
		}

		EventInputEnter(false, true);
		EventPropertyEnter(false, true);

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
				if (cboCharset.Text.Trim() == "" || cboCharset.Text.Trim().ToUpper() == SysConstants.C_DefaultCharset)
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

			object @object = DataSources.DependentRow[CoreConstants.C_KeyExAdornedQualifiedName];

			lblCurrentDisplayName.Text = !Cmd.IsNullValue(@object)
				? (string)@object : ControlsResources.TConnectionUIControl_NewDatabaseConnection;


			if (Site != null)
			{
				foreach (Describer describer in Csb.AdvancedKeys)
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
			}

			// If it's an Edm connection dialog the _OriginalConnectionString should be updated
			// so that the user can be warned if they then change the properties and cause a target
			// change. For ServerExplorer and Session connection dialogs _OriginalConnectionString
			// will already be set.
			if (ConnectionSource == EnConnectionSource.EntityDataModel)
			{
				@object = DataSources.DependentRow[CoreConstants.C_KeyExConnectionString];
				_OriginalConnectionString = !Cmd.IsNullValue(@object)
					? (string)@object : null;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EventPropertyExit();
			EventInputExit();
			EventCursorExit();
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
		// Evs.Trace(GetType(), nameof(OnDataSourcesCursorChanged));

		if (!EventCursorEnter())
			return;

		if (DataSources.Row == null || (int)DataSources.Row["Orderer"] == 0)
		{
			EventCursorExit();
			return;
		}

		EventInputEnter(false, true);
		EventPropertyEnter(false, true);

		try
		{
			if ((int)DataSources.Row["Orderer"] == 1)
			{
				txtDataSource.Text = SysConstants.C_DefaultDataSource;
				txtPort.Text = SysConstants.C_DefaultPort.ToString();
				cboServerType.SetSelectedIndexX((int)SysConstants.C_DefaultServerType);
				txtDatabase.Text = SysConstants.C_DefaultDatabase;
				cboDialect.SetSelectedValueX(SysConstants.C_DefaultDialect);
				txtUserName.Text = SysConstants.C_DefaultUserID;
				txtPassword.Text = SysConstants.C_DefaultPassword;
				txtRole.Text = SysConstants.C_DefaultRole;
				cboCharset.SetSelectedValueX(SysConstants.C_DefaultCharset);

				if (Site != null)
				{
					foreach (Describer describer in Csb.Describers.DescriberKeys)
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
						if (Convert.ToInt32(txtPort.Text) == SysConstants.C_DefaultPort)
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
			EventPropertyExit();
			EventInputExit();
			EventCursorExit();
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
		if (Site == null || !EventInputEnter())
			return;

		// Evs.Trace(GetType(), nameof(OnInputChanged), "Sender: {0}.", ((Control)sender)?.Name);

		// Disable property property events because we're going to invoke
		// OnPropertyChanged afterwards.

		EventPropertyEnter(false, true);

		try
		{
			string propertyName;

			if (sender.Equals(txtDataSource))
			{
				propertyName = SysConstants.C_KeyDataSource;
				Site[propertyName] = txtDataSource.Text.Trim();
			}
			else if (sender.Equals(txtDatabase))
			{
				propertyName = SysConstants.C_KeyDatabase;
				Site[propertyName] = Cmd.CleanPath(txtDatabase.Text);
			}
			else if (sender.Equals(txtUserName))
			{
				propertyName = SysConstants.C_KeyUserID;
				Site[propertyName] = txtUserName.Text.Trim();
			}
			else if (sender.Equals(txtPassword))
			{
				propertyName = SysConstants.C_KeyPassword;
				Site[propertyName] = txtPassword.Text.Trim();
			}
			else if (sender.Equals(txtRole))
			{
				propertyName = SysConstants.C_KeyRole;
				if (txtRole.Text.Trim() == "")
					Site.Remove(propertyName);
				else
					Site[propertyName] = txtRole.Text.Trim();
			}
			else if (sender.Equals(cboCharset))
			{
				propertyName = SysConstants.C_KeyCharset;
				if (cboCharset.Text.Trim() == "" || cboCharset.Text.Trim().ToUpper() == SysConstants.C_DefaultCharset)
					Site.Remove(propertyName);
				else
					Site[propertyName] = cboCharset.Text.Trim();
			}
			else if (sender.Equals(txtPort))
			{
				propertyName = SysConstants.C_KeyPort;
				if (string.IsNullOrWhiteSpace(txtPort.Text) || Convert.ToInt32(txtPort.Text.Trim()) == SysConstants.C_DefaultPort)
					Site.Remove(propertyName);
				else
					Site[propertyName] = Convert.ToInt32(txtPort.Text);
			}
			else if (sender.Equals(cboDialect))
			{
				propertyName = SysConstants.C_KeyDialect;
				if (string.IsNullOrWhiteSpace(cboDialect.Text) || Convert.ToInt32(cboDialect.Text.Trim()) == SysConstants.C_DefaultDialect)
					Site.Remove(propertyName);
				else
					Site[propertyName] = Convert.ToInt32(cboDialect.Text);
			}
			else if (sender.Equals(cboServerType))
			{
				propertyName = SysConstants.C_KeyServerType;
				if (cboServerType.SelectedIndex == -1 || cboServerType.SelectedIndex == (int)SysConstants.C_DefaultServerType)
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
			EventPropertyExit();
			EventInputExit();
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
		// Evs.Trace(GetType(), nameof(OnSiteChanged), "Site.ToString(): {0}, Site.Count: {1}", Site != null ? Site.ToString() : "Null",
		//	Site != null ? Site.Count : -1);

		_OriginalConnectionString = null;
		_InsertMode = true;
		_SiteChanged = true;

		if (Site != null)
		{
			(Site as IBsDataConnectionProperties).ConnectionSource = ConnectionSource;

			_OriginalConnectionString = Site.ToString();

			if (string.IsNullOrWhiteSpace(_OriginalConnectionString))
				_OriginalConnectionString = null;

			_InsertMode = _OriginalConnectionString == null;

			EnConnectionSource storedConnectionSource = Site.ContainsKey(CoreConstants.C_KeyExConnectionSource)
				? (EnConnectionSource)Site[CoreConstants.C_KeyExConnectionSource] : EnConnectionSource.Undefined;

			EventPropertyEnter(false, true);

			try
			{
				if (ConnectionSource == EnConnectionSource.Application)
				{
					foreach (Describer describer in Csb.AdvancedKeys)
					{
						if (!describer.IsConnectionParameter && describer.Key != CoreConstants.C_KeyExConnectionSource)
							Site.Remove(describer.Key);
					}

					if (storedConnectionSource <= EnConnectionSource.Unknown)
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


					if (!Site.ContainsKey(CoreConstants.C_KeyExConnectionSource)
						|| (EnConnectionSource)Site[CoreConstants.C_KeyExConnectionSource] <= EnConnectionSource.Unknown)
					{
						EnConnectionSource connectionSource = ConnectionSource == EnConnectionSource.EntityDataModel
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
				EventPropertyExit();
			}

		}

		base.OnSiteChanged(e);
	}


	/// <summary>
	/// Apply any changes to the Rct and SE.
	/// </summary>
	private void OnVerifySettings(object sender, EventArgs e)
	{
		// Evs.Trace(GetType(), nameof(OnVerifySettings), "ConnectionSource: {0}.", ConnectionSource);

		// Validate the new parse request connection string and then apply.

		// Reenable the Rct OnExplorerConnectionNodeChanged() event.

		if (ConnectionSource == EnConnectionSource.ServerExplorer)
			RctManager.ExternalEventSinkExit();

		if (RctManager.ShutdownState)
			return;


		if (ConnectionSource == EnConnectionSource.Application || !_HandleVerification
			|| (ConnectionSource == EnConnectionSource.EntityDataModel && !_HandleNewInternally
			&& !_HandleModifyInternally))
		{
			// Evs.Trace(GetType(), nameof(OnVerifySettings), "Not handling internally.");
			return;
		}

		// Evs.Trace(GetType(), nameof(OnVerifySettings), "Before Verify - ConnectionSource: {0}, _HandleNewInternally: {1}, _HandleModifyInternally: {2}.", ConnectionSource, _HandleNewInternally, _HandleModifyInternally);


		EnConnectionSource registrationSource =
			(ConnectionSource == EnConnectionSource.EntityDataModel
			|| (ConnectionSource == EnConnectionSource.Session && _HandleNewInternally))
			? EnConnectionSource.ServerExplorer : ConnectionSource;



		// Special case. If an existing connection is updated we lock it's parser against
		// disposal in IVsDataViewSupport.
		if (ConnectionSource == EnConnectionSource.ServerExplorer
			&& !_HandleNewInternally && !_HandleModifyInternally && !_InsertMode)
		{
			// Lock the parser if properties that have been changed don't affect the parser.
			NativeDb.LockLoadedParser(_OriginalConnectionString, Site.ToString());
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
		}

		// Evs.Trace(GetType(), nameof(OnVerifySettings), "After Verify - ConnectionSource: {0}, Site.ToString(): {1}.", ConnectionSource, Site.ToString());

	}



	private void OnUpdateServerExplorerChanged(object sender, EventArgs e)
	{
		if (Site == null || !InvalidDependent)
			return;

		EventPropertyEnter(false, true);

		try
		{
			if (SessionDlg.UpdateServerExplorer)
				Site[CoreConstants.C_KeyExConnectionSource] = EnConnectionSource.ServerExplorer;
			else
				Site[CoreConstants.C_KeyExConnectionSource] = ConnectionSource;
		}
		finally
		{
			EventPropertyExit();
		}

	}


	#endregion Event handlers
}