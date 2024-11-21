// Microsoft.VisualStudio.Data.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Package.DataConnectionDialog
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools;
using BlackbirdSql.VisualStudio.Ddex.Enums;
using BlackbirdSql.VisualStudio.Ddex.Events;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.Data.ConnectionUI;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;


// =========================================================================================================
//										VxbConnectionDialogHandler Class
//
// =========================================================================================================
public class VxbConnectionDialogHandler : IBsConnectionDialogHandler
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbConnectionDialogHandler
	// ---------------------------------------------------------------------------------


	public VxbConnectionDialogHandler()
	{
		try
		{
			_ConnectionDlg = (VxbConnectionDialog)Activator.CreateInstance(typeof(VxbConnectionDialog),
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [], null);
		}
		catch (TargetInvocationException ex)
		{
			Diag.Ex(ex);
			throw ex.InnerException;
		}

		_ConnectionDlg.HeaderLabel = ControlsResources.TDataConnectionDlgHandler_HeaderLabel;
		_ConnectionDlg.ChooseDataSourceAcceptText = ControlsResources.TDataConnectionDlgHandler_ChooseSourceAcceptText;
		_ConnectionDlg.ContextHelpRequested += HandleContextHelpRequested;
		_ConnectionDlg.VerifySettings += HandleVerifySettings;
		_ConnectionDlg.LinkClicked += HandleLinkClicked;
	}


	~VxbConnectionDialogHandler()
	{
		Dispose(disposing: false);
	}


	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}


	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_ConnectionDlg.Dispose();
		}
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - VxbConnectionDialogHandler 
	// =========================================================================================================


	private bool _IsConnectMode;

	private bool _CreateNewConnection;

	private IVsDataConnection _Connection;

	private ICollection<Guid> _AvailableSources;

	private IDictionary<Guid, ICollection<Guid>> _AvailableProviders;

	// private static readonly IDictionary<Guid, Guid> _ProviderSelections;

	private readonly VxbConnectionDialog _ConnectionDlg;


	#endregion Fields




	// =========================================================================================================
	#region Property accessors - VxbConnectionDialogHandler
	// =========================================================================================================


	public string Title
	{
		get
		{
			return _ConnectionDlg.Title;
		}
		set
		{
			_ConnectionDlg.Title = value;
		}
	}

	public string HeaderLabel
	{
		get
		{
			return _ConnectionDlg.HeaderLabel;
		}
		set
		{
			_ConnectionDlg.HeaderLabel = value;
		}
	}

	public string ChooseSourceTitle
	{
		get
		{
			return _ConnectionDlg.ChooseDataSourceTitle;
		}
		set
		{
			_ConnectionDlg.ChooseDataSourceTitle = value;
		}
	}

	public string ChooseSourceHeaderLabel
	{
		get
		{
			return _ConnectionDlg.ChooseDataSourceHeaderLabel;
		}
		set
		{
			_ConnectionDlg.ChooseDataSourceHeaderLabel = value;
		}
	}

	public string ChooseSourceAcceptText
	{
		get
		{
			return _ConnectionDlg.ChooseDataSourceAcceptText;
		}
		set
		{
			_ConnectionDlg.ChooseDataSourceAcceptText = value;
		}
	}

	public string ChangeSourceTitle
	{
		get
		{
			return _ConnectionDlg.ChangeDataSourceTitle;
		}
		set
		{
			_ConnectionDlg.ChangeDataSourceTitle = value;
		}
	}

	public string ChangeSourceHeaderLabel
	{
		get
		{
			return _ConnectionDlg.ChangeDataSourceHeaderLabel;
		}
		set
		{
			_ConnectionDlg.ChangeDataSourceHeaderLabel = value;
		}
	}

	IUIService UiService => Package.GetGlobalService(typeof(IUIService)) as IUIService;


	public Guid UnspecifiedSource => new Guid("BF8BEF57-3EF1-4ec4-BEEB-6581498003A6");

	public ICollection<Guid> AvailableSources
	{
		get
		{
			_AvailableSources ??= new VxiDataSourceCollection(this);
			return _AvailableSources;
		}
	}

	private VxiDataSourceCollection AvailableDataSourceCollection => _AvailableSources as VxiDataSourceCollection;

	public Guid SelectedSource
	{
		get
		{
			switch (AvailableSources.Count)
			{
				case 0:
					return Guid.Empty;
				case 1:
					{
						IEnumerator<Guid> enumerator = AvailableSources.GetEnumerator();
						enumerator.MoveNext();
						return enumerator.Current;
					}
				default:
					if (_ConnectionDlg.SelectedDataSource != null && _ConnectionDlg.SelectedDataSource.NameClsid != Guid.Empty)
					{
						return _ConnectionDlg.SelectedDataSource.NameClsid;
					}
					if (_ConnectionDlg.SelectedDataSource == _ConnectionDlg.UnspecifiedDataSource)
					{
						return UnspecifiedSource;
					}
					return Guid.Empty;
			}
		}
		set
		{
			if (value != Guid.Empty)
			{
				if (!AvailableSources.Contains(value))
				{
					throw new ArgumentException(ControlsResources.TDataConnectionDlgHandler_UnknownSource);
				}
				_ConnectionDlg.SelectedDataSource = AvailableDataSourceCollection.GuidDataSourceMapping[value];
			}
			else
			{
				_ConnectionDlg.SelectedDataSource = null;
			}
		}
	}

	public ICollection<Guid> AvailableProviders
	{
		get
		{
			if (SelectedSource == Guid.Empty)
			{
				return null;
			}
			_AvailableProviders ??= new Dictionary<Guid, ICollection<Guid>>();
			if (!_AvailableProviders.ContainsKey(SelectedSource))
			{
				_AvailableProviders.Add(SelectedSource, new VxiDataProviderCollection(_ConnectionDlg.SelectedDataSource));
			}
			return _AvailableProviders[SelectedSource];
		}
	}

	public Guid SelectedProvider
	{
		get
		{
			if (SelectedSource == Guid.Empty)
			{
				return Guid.Empty;
			}
			switch (AvailableProviders.Count)
			{
				case 0:
					return Guid.Empty;
				case 1:
					{
						IEnumerator<Guid> enumerator = AvailableProviders.GetEnumerator();
						enumerator.MoveNext();
						return enumerator.Current;
					}
				default:
					if (_ConnectionDlg.SelectedDataProvider != null)
					{
						return (_ConnectionDlg.SelectedDataProvider as VxiUIDataProvider).Clsid;
					}
					return Guid.Empty;
			}
		}
		set
		{
			if (SelectedSource == Guid.Empty && value != Guid.Empty)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlgHandler_NoSourceSelected);
			}
			if (value != Guid.Empty)
			{
				if (!AvailableProviders.Contains(value))
				{
					throw new ArgumentException(ControlsResources.TDataConnectionDlgHandler_UnknownProvider);
				}
				_ConnectionDlg.SelectedDataProvider = (AvailableProviders as VxiDataProviderCollection).Mapping[value];
			}
			else
			{
				_ConnectionDlg.SelectedDataProvider = null;
			}
		}
	}

	public bool SaveSelection
	{
		get
		{
			return _ConnectionDlg.SaveSelection;
		}
		set
		{
			_ConnectionDlg.SaveSelection = value;
		}
	}

	public string DisplayConnectionString => _ConnectionDlg.DisplayConnectionString;

	public string SafeConnectionString
	{
		get
		{
			string result = DisplayConnectionString;
			if (_ConnectionDlg.SelectedDataProvider != null)
			{
				VxiDataConnectionUIProperties uIDataConnectionProperties = ((_ConnectionDlg.SelectedDataSource != _ConnectionDlg.UnspecifiedDataSource) ? (_ConnectionDlg.SelectedDataProvider.CreateConnectionProperties(_ConnectionDlg.SelectedDataSource) as VxiDataConnectionUIProperties) : (_ConnectionDlg.SelectedDataProvider.CreateConnectionProperties() as VxiDataConnectionUIProperties));
				if (uIDataConnectionProperties != null)
				{
					uIDataConnectionProperties.Parse(DataProtection.DecryptString(EncryptedConnectionString));
					result = uIDataConnectionProperties.InnerProperties.ToSafeString();
				}
			}
			return result;
		}
		set
		{
			_ConnectionDlg.ConnectionString = value;
		}
	}

	public string EncryptedConnectionString
	{
		get
		{
			return DataProtection.EncryptString(_ConnectionDlg.ConnectionString);
		}
		set
		{
			_ConnectionDlg.ConnectionString = DataProtection.DecryptString(value);
		}
	}

	public string AcceptButtonText
	{
		get
		{
			return _ConnectionDlg.AcceptButtonText;
		}
		set
		{
			_ConnectionDlg.AcceptButtonText = value;
		}
	}

	public bool CreateNewConnection
	{
		get
		{
			return _CreateNewConnection;
		}
		set
		{
			_CreateNewConnection = value;
		}
	}


	public bool UpdateServerExplorer => _ConnectionDlg.UpdateServerExplorer;


	/*
	private static IDictionary<Guid, Guid> ProviderSelections
	{
		get
		{
			_ProviderSelections ??= new Dictionary<Guid, Guid>
				{
					[new Guid(SystemData.C_DataSourceGuid)] = new Guid(SystemData.C_ProviderGuid)
				};

			return _ProviderSelections;
		}
	}
	*/


	public event EventHandler VerifyConfiguration;


	#endregion Property accessors




	// =========================================================================================================
	#region Methods - VxbConnectionDialogHandler
	// =========================================================================================================


	public void AddAllSources()
	{
		AddSources(null);
	}

	public void AddSources(Guid providerTechnology)
	{
		DataConnectionDialogFilterCallback callback = null;
		if (providerTechnology != Guid.Empty)
		{

			IVsDataProviderManager providerManager = ApcManager.GetService<IVsDataProviderManager>();
			callback = delegate (Guid source, Guid provider)
			{
				IVsDataProvider vsDataProvider = providerManager.Providers[provider];
				return vsDataProvider.Technology == providerTechnology;
			};
		}
		AddSources(callback);
	}

	public void AddSources(DataConnectionDialogFilterCallback callback)
	{
		AvailableSources.Clear();

		IVsDataSourceManager sourceManager = ApcManager.GetService<IVsDataSourceManager>();

		foreach (IVsDataSource value in sourceManager.Sources.Values)
		{
			if (value.Guid != VxbDataSource.FbDataSource.NameClsid)
				continue;

			AvailableDataSourceCollection.Add(value);
			Guid guid2 = (SelectedSource = value.Guid);
			Guid[] providers = value.GetProviders();
			foreach (Guid guid3 in providers)
			{
				if (callback == null || callback(guid2, guid3))
				{
					AvailableProviders.Add(guid3);
				}
			}
			if (AvailableProviders.Count > 0)
			{
				Guid defaultProvider = value.DefaultProvider;
				if (AvailableProviders.Contains(defaultProvider))
				{
					SelectedProvider = defaultProvider;
				}
			}
			else
			{
				AvailableSources.Remove(guid2);
			}
		}
		AvailableSources.Add(UnspecifiedSource);
		SelectedSource = UnspecifiedSource;
		/*
		foreach (IVsDataProvider value2 in DataPackage.ProviderManager.Providers.Values)
		{
			if ((value2 as IVsDataInternalProvider).IsSourceSupported(UnspecifiedSource) && (callback == null || callback(UnspecifiedSource, value2.Guid)))
			{
				(AvailableProviders as DataProviderCollection).Add(value2);
			}
		}
		*/
		if (AvailableProviders.Count == 0)
		{
			AvailableSources.Remove(UnspecifiedSource);
		}
		if (AvailableSources.Count != 1)
		{
			SelectedSource = Guid.Empty;
		}
	}


	public void LoadSourceSelection()
	{
		/*
		Guid guid = Guid.Empty;
		Guid guid2 = Guid.Empty;
		Microsoft.VisualStudio.Data.HostServices.Registry.Key key = Host.Environment.LocalRegistry.OpenKey(RegistryHive.CurrentUser, "Data Connection Dialog");
		if (key != null)
		{
			using (key)
			{
				try
				{
					guid = new Guid(key.GetValue("Selected Source") as string);
					guid2 = new Guid(key.GetValue("Selected Provider") as string);
				}
				catch
				{
				}
			}
		}
		if (guid != Guid.Empty || guid2 != Guid.Empty)
		{
			SaveSelection = false;
		}
		if (guid != Guid.Empty && AvailableSources.Contains(guid))
		{
			if (AvailableSources.Count > 1)
			{
				SelectedSource = guid;
			}
			if (guid2 != Guid.Empty && AvailableProviders.Contains(guid2))
			{
				if (AvailableProviders.Count > 1)
				{
					SelectedProvider = guid2;
				}
			}
			else if (AvailableProviders.Count > 1)
			{
				SelectedProvider = Guid.Empty;
			}
		}
		else if (AvailableSources.Count > 1)
		{
			SelectedSource = Guid.Empty;
		}
		*/
		SelectedSource = VxbDataSource.FbDataSource.NameClsid;
		SelectedProvider = VxbDataProvider.BlackbirdSqlDataProvider.NameClsid;
	}

	public void LoadProviderSelections()
	{
		/*
		Guid selectedProvider = SelectedProvider;
		Guid selectedSource = SelectedSource;
		foreach (KeyValuePair<Guid, Guid> providerSelection in ProviderSelections)
		{
			if (AvailableSources.Contains(providerSelection.Key))
			{
				SelectedSource = providerSelection.Key;
				if (AvailableProviders.Contains(providerSelection.Value))
				{
					SelectedProvider = providerSelection.Value;
				}
			}
		}
		SelectedSource = selectedSource;
		SelectedProvider = selectedProvider;
		*/
		SelectedSource = VxbDataSource.FbDataSource.NameClsid;
		SelectedProvider = VxbDataProvider.BlackbirdSqlDataProvider.NameClsid;
	}

	public void LoadExistingConfiguration(Guid clsidProvider, string connectionString, bool encryptedString)
	{
		if (clsidProvider == Guid.Empty)
		{
			ArgumentNullException ex = new(nameof(clsidProvider));
			Diag.Ex(ex);
			throw ex;
		}
		if (connectionString == null)
		{
			ArgumentNullException ex = new(nameof(connectionString));
			Diag.Ex(ex);
			throw ex;
		}
		if (clsidProvider.ToString().ToUpper() != VxbDataProvider.BlackbirdSqlDataProvider.Name)
		{
			ArgumentException ex = new($"Invalid provider with guid: {clsidProvider}.");
			Diag.Ex(ex);
			throw ex;
		}

		string text = connectionString;
		if (encryptedString)
		{
			text = DataProtection.DecryptString(connectionString);
		}

		IVsDataProvider value = new VxbVsDataProvider(clsidProvider);
		/*
		if (!DataPackage.ProviderManager.Providers.TryGetValue(clsidProvider, out value))
		{
			throw new ArgumentException(ControlsResources.DataConnectionDlgHandler_UnknownProvider);
		}
		*/

		Guid empty = value.DeriveSource(text);
		if (AvailableSources.Contains(empty))
		{
			SelectedSource = empty;
			if (AvailableProviders.Contains(clsidProvider))
			{
				SelectedProvider = clsidProvider;
				SafeConnectionString = text;
				return;
			}
		}
		if (AvailableSources.Contains(UnspecifiedSource))
		{
			SelectedSource = UnspecifiedSource;
			if (AvailableProviders.Contains(clsidProvider))
			{
				SelectedProvider = clsidProvider;
				SafeConnectionString = text;
				return;
			}
		}
		SelectedSource = Guid.Empty;
	}



	public bool ShowDialog() => ShowDialog(_ConnectionDlg) == DialogResult.OK;



	public virtual DialogResult ShowDialog(VxbConnectionDialog form)
	{
		if (ApcManager.ServiceProvider == null)
			return DialogResult.Cancel;

		Container container = new VxiConnectionDialogContainer(UiService, ApcManager.ServiceProvider);
		container.Add(form);

		RctManager.SessionConnectionSourceActive = true;

		try
		{
			return VxbConnectionDialog.Show(form, UiService.GetDialogOwnerWindow());
		}
		finally
		{
			container.Remove(form);
			RctManager.SessionConnectionSourceActive = false;
		}

		/*
		void onPreferencesChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			form.Font = (Font)UiService.Styles["DialogFont"];
		}
		SystemEvents.UserPreferenceChanged += onPreferencesChanged;

		try
		{
			return UiService.ShowDialog(form);
		}
		finally
		{
			SystemEvents.UserPreferenceChanged -= onPreferencesChanged;
		}
		*/
	}



	public IVsDataConnection ShowDialog(bool connect)
	{
		_Connection = null;
		if (connect)
		{
			_IsConnectMode = true;
		}
		try
		{
			if (ShowDialog())
			{
				if (_Connection == null)
				{
					if (!_CreateNewConnection)
					{
						IVsDataConnectionManager connectionManager = ApcManager.GetService<IVsDataConnectionManager>();
						_Connection = connectionManager.GetConnection(SelectedProvider, EncryptedConnectionString, encryptedString: true);
					}
					else
					{
						IVsDataConnectionFactory connectionFactory = ApcManager.GetService<IVsDataConnectionFactory>();
						_Connection = connectionFactory.CreateConnection(SelectedProvider, EncryptedConnectionString, encryptedString: true);
					}
				}
				if (connect)
				{
					EncryptedConnectionString = _Connection.EncryptedConnectionString;
				}
				return _Connection;
			}
			return null;
		}
		finally
		{
			_IsConnectMode = false;
		}
	}



	public void SaveProviderSelections()
	{
		/*
		Guid selectedProvider = SelectedProvider;
		Guid selectedSource = SelectedSource;
		using (IEnumerator<Guid> enumerator = AvailableSources.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Guid key = (SelectedSource = enumerator.Current);
				ProviderSelections[key] = SelectedProvider;
			}
		}
		SelectedSource = selectedSource;
		SelectedProvider = selectedProvider;
		Microsoft.VisualStudio.Data.HostServices.Registry.Key key2 = Host.Environment.LocalRegistry.CreateOrOpenKey(RegistryHive.CurrentUser, "Data Connection Dialog\\Provider Selections");
		if (key2 == null)
		{
			return;
		}
		using (key2)
		{
			foreach (KeyValuePair<Guid, Guid> providerSelection in ProviderSelections)
			{
				key2.SetValue(providerSelection.Key.ToString(), providerSelection.Value.ToString());
			}
		}
		*/
	}



	public void SaveSourceSelection()
	{
		/*
		Microsoft.VisualStudio.Data.HostServices.Registry.Key key = Host.Environment.LocalRegistry.CreateOrOpenKey(RegistryHive.CurrentUser, "Data Connection Dialog");
		if (key == null)
		{
			return;
		}
		using (key)
		{
			try
			{
				if (SelectedSource != Guid.Empty)
				{
					key.SetValue("Selected Source", SelectedSource.ToString("B", CultureInfo.InvariantCulture));
				}
				else
				{
					key.DeleteValue("Selected Source");
				}
				if (SelectedProvider != Guid.Empty)
				{
					key.SetValue("Selected Provider", SelectedProvider.ToString("B", CultureInfo.InvariantCulture));
				}
				else
				{
					key.DeleteValue("Selected Provider");
				}
			}
			catch
			{
			}
		}
		*/
	}


	#endregion Methods




	// =========================================================================================================
	#region Event Handling - VxbConnectionDialogHandler
	// =========================================================================================================


	protected virtual void OnVerifyConfiguration(EventArgs e)
	{
		VerifyConfiguration?.Invoke(this, e);
		if (!_IsConnectMode)
		{
			return;
		}
		/*
		using (Host.System.UserInterface.NewWaitCursor())
		{
			if (!_CreateNewConnection)
			{
				_Connection = DataPackage.ConnectionManager.GetConnection(SelectedProvider, EncryptedConnectionString, encryptedString: true, update: true);
			}
			else
			{
				_Connection = DataPackage.ConnectionFactory.CreateConnection(SelectedProvider, EncryptedConnectionString, encryptedString: true);
			}
			try
			{
				Guid source = Guid.Empty;
				if (SelectedSource != UnspecifiedSource)
				{
					source = SelectedSource;
				}
				DataPackage.ProviderManager.Providers[SelectedProvider].TryCreateObject<IVsDataConnectionUIConnector>(source)?.Connect(_Connection);
				if (_Connection.State != DataConnectionState.Open)
				{
					_Connection.Open();
				}
			}
			catch (DataConnectionOpenCanceledException ex)
			{
				throw new ExternalException(ex.Message, -2147217842);
			}
		}
		*/
	}



	private void HandleContextHelpRequested(object sender, ContextHelpEventArgs e)
	{
		string text = null;
		if ((e.Context & EnDataConnectionDlgContext.Source) > EnDataConnectionDlgContext.None)
		{
			text = "vs.dataconnectiondialog.source";
		}
		else if ((e.Context & EnDataConnectionDlgContext.Main) > EnDataConnectionDlgContext.None)
		{
			EnDataConnectionDlgContext context = e.Context;
			text = ((context != EnDataConnectionDlgContext.MainGenericConnectionUIControl) ? "vs.dataconnectiondialog" : "vs.dataconnectiondialog.connection.generic");
		}
		else if ((e.Context & EnDataConnectionDlgContext.Advanced) > EnDataConnectionDlgContext.None)
		{
			text = "vs.dataconnectiondialog.advanced";
		}
		else if ((e.Context & EnDataConnectionDlgContext.AddProperty) > EnDataConnectionDlgContext.None)
		{
			text = "vs.dataconnectiondialog.addproperty";
		}
		if (text != null)
		{
			// Host.Environment.ShowHelp(text);
			e.Handled = true;
		}
	}

	private void HandleVerifySettings(object sender, EventArgs e)
	{

		OnVerifyConfiguration(e);
	}

	private void HandleLinkClicked(object sender, LinkClickedEventArgs e)
	{
		if (!string.IsNullOrEmpty(e.LinkText))
		{
			try
			{
				Process.Start(e.LinkText);
			}
			catch (Exception)
			{
			}
		}
	}


	#endregion Event Handling




	// =========================================================================================================
	#region						Sub-Classes - VxbConnectionDialogHandler
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	//					   Internal Class VxiConnectionDialogContainer
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Orignal class: Microsoft.VisualStudio.Data.HostServices.Environment.ConnectionDialogContainer.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class VxiConnectionDialogContainer(IUIService uiService, IServiceProvider serviceProvider) : Container
	{
		private ProfessionalColorTable _ProfessionalColorTable;

		private AmbientProperties _AmbientProperties;

		private readonly IUIService _UiService = uiService;

		private readonly IServiceProvider _ServiceProvider = serviceProvider;

		protected override object GetService(Type serviceType)
		{
			if (serviceType == typeof(AmbientProperties))
			{
				_AmbientProperties ??= new AmbientProperties
				{
					Font = _UiService.Styles["DialogFont"] as Font
				};
				return _AmbientProperties;
			}
			if (serviceType == typeof(ProfessionalColorTable))
			{
				_ProfessionalColorTable ??= _UiService.Styles["VsColorTable"] as ProfessionalColorTable;
				return _ProfessionalColorTable;
			}
			object service = _ServiceProvider.GetService(serviceType);
			if (service != null)
			{
				return service;
			}
			return base.GetService(serviceType);
		}
	}




	// ---------------------------------------------------------------------------------
	//						Internal Class VxiDataSourceCollection
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original name: DataSourceCollection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class VxiDataSourceCollection(VxbConnectionDialogHandler dialog) : ICollection<Guid>, IEnumerable<Guid>, IEnumerable
	{
		private readonly IDictionary<Guid, VxbDataSource> _GuidDataSourceMapping = new Dictionary<Guid, VxbDataSource>();

		private readonly VxbConnectionDialogHandler _DlgHandler = dialog;

		public int Count => _DlgHandler._ConnectionDlg.DataSources.Count;

		public bool IsReadOnly => _DlgHandler._ConnectionDlg.DataSources.IsReadOnly;

		internal IDictionary<Guid, VxbDataSource> GuidDataSourceMapping => _GuidDataSourceMapping;

		public void Add(Guid clsidItem)
		{
			if (clsidItem == Guid.Empty)
			{
				throw new ArgumentNullException("item");
			}
			if (!_GuidDataSourceMapping.ContainsKey(clsidItem))
			{
				_GuidDataSourceMapping.Add(clsidItem, CreateSource(clsidItem));
				_DlgHandler._ConnectionDlg.DataSources.Add(_GuidDataSourceMapping[clsidItem]);
			}
		}

		public void Add(IVsDataSource vsDataSource)
		{
			Guid clsid = vsDataSource.Guid;
			if (!_GuidDataSourceMapping.ContainsKey(clsid))
			{
				_GuidDataSourceMapping.Add(clsid, CreateSource(clsid, vsDataSource));
				_DlgHandler._ConnectionDlg.DataSources.Add(_GuidDataSourceMapping[clsid]);
			}
		}


		public bool Contains(Guid clsidItem)
		{
			return _GuidDataSourceMapping.ContainsKey(clsidItem);
		}

		public bool Remove(Guid clsidItem)
		{
			if (_GuidDataSourceMapping.ContainsKey(clsidItem))
			{
				_DlgHandler._ConnectionDlg.DataSources.Remove(_GuidDataSourceMapping[clsidItem]);
				_GuidDataSourceMapping.Remove(clsidItem);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			_DlgHandler._ConnectionDlg.DataSources.Clear();
			_GuidDataSourceMapping.Clear();
		}

		public void CopyTo(Guid[] array, int arrayIndex)
		{
			_GuidDataSourceMapping.Keys.CopyTo(array, arrayIndex);
		}

		public IEnumerator<Guid> GetEnumerator()
		{
			return _GuidDataSourceMapping.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _GuidDataSourceMapping.Keys.GetEnumerator();
		}

		private VxbDataSource CreateSource(Guid clsidSource)
		{
			if (clsidSource == VxbDataSource.FbDataSource.NameClsid)
				return VxbDataSource.FbDataSource;

			if (clsidSource == _DlgHandler.UnspecifiedSource)
			{
				return _DlgHandler._ConnectionDlg.UnspecifiedDataSource;
			}

			IVsDataSourceManager sourceManager = ApcManager.GetService<IVsDataSourceManager>();

			if (!sourceManager.Sources.TryGetValue(clsidSource, out IVsDataSource value))
			{
				ArgumentException ex = new($"Unknown Source guid: {clsidSource}.");
				Diag.Ex(ex);
			}
			return CreateSource(clsidSource, value);
		}


		private static VxbDataSource CreateSource(Guid clsidSource, IVsDataSource vsDataSource)
		{
			return new VxbDataSource(clsidSource.ToString(null, CultureInfo.InvariantCulture), vsDataSource.DisplayName);
		}

	}




	// ---------------------------------------------------------------------------------
	//					    Internal Class VxiDataProviderCollection
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Orignal name: DataProviderCollection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class VxiDataProviderCollection(VxbDataSource source) : ICollection<Guid>, IEnumerable<Guid>, IEnumerable
	{
		private readonly IDictionary<Guid, BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools.VxbDataProvider> _GuidDataSourceMapping = new Dictionary<Guid, BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools.VxbDataProvider>();

		private readonly VxbDataSource _DataSource = source;

		public int Count => _DataSource.Providers.Count;

		public bool IsReadOnly => _DataSource.Providers.IsReadOnly;

		internal IDictionary<Guid, BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools.VxbDataProvider> Mapping => _GuidDataSourceMapping;

		public void Add(Guid item)
		{
			if (item == Guid.Empty)
			{
				throw new ArgumentNullException("item");
			}
			if (!_GuidDataSourceMapping.ContainsKey(item))
			{
				_GuidDataSourceMapping.Add(item, CreateProvider(item));
				_DataSource.Providers.Add(_GuidDataSourceMapping[item]);
			}
		}

		public void Add(IVsDataProvider item)
		{
			Guid guid = item.Guid;
			if (!_GuidDataSourceMapping.ContainsKey(guid))
			{
				_GuidDataSourceMapping.Add(guid, CreateProvider(item));
				_DataSource.Providers.Add(_GuidDataSourceMapping[guid]);
			}
		}

		public bool Contains(Guid item)
		{
			return _GuidDataSourceMapping.ContainsKey(item);
		}

		public bool Remove(Guid item)
		{
			if (_GuidDataSourceMapping.ContainsKey(item))
			{
				_DataSource.Providers.Remove(_GuidDataSourceMapping[item]);
				_GuidDataSourceMapping.Remove(item);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			_DataSource.Providers.Clear();
			_GuidDataSourceMapping.Clear();
		}

		public void CopyTo(Guid[] array, int arrayIndex)
		{
			_GuidDataSourceMapping.Keys.CopyTo(array, arrayIndex);
		}

		public IEnumerator<Guid> GetEnumerator()
		{
			return _GuidDataSourceMapping.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _GuidDataSourceMapping.Keys.GetEnumerator();
		}

		private static VxbDataProvider CreateProvider(Guid providerClsid)
		{
			// Always going to be BlackbirdSql.
			VxbVsDataProvider value = new(providerClsid);
			return new VxiUIDataProvider(value);
			/*
			IVsDataProvider value = null;
			if (!DataPackage.ProviderManager.Providers.TryGetValue(provider, out value))
			{
				throw new ArgumentException(ControlsResources.DataConnectionDlgHandler_UnknownProvider);
			}
			*/
		}

		private static VxbDataProvider CreateProvider(IVsDataProvider vsDataProvider)
		{
			return new VxiUIDataProvider(vsDataProvider);
		}
	}




	// ---------------------------------------------------------------------------------
	//						   Internal Class VxiUIDataProvider
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Orignal name: UIDataProvider.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class VxiUIDataProvider(IVsDataProvider vsDataProvider) : VxbDataProvider(vsDataProvider.Guid.ToString(null, CultureInfo.InvariantCulture), vsDataProvider.DisplayName, vsDataProvider.ShortDisplayName)
	{
		private readonly IVsDataProvider _VsDataProvider = vsDataProvider;

		public Guid Clsid => new Guid(Name);

		public override string GetDescription(VxbDataSource dataSource)
		{
			if (dataSource != null)
			{
				IVsDataSourceManager sourceManager = ApcManager.GetService<IVsDataSourceManager>();

				IVsDataSource vsDataSource = sourceManager.Sources[dataSource.NameClsid];

				string description = vsDataSource.GetDescription(_VsDataProvider.Guid);
				if (description != null)
					return description;
			}
			return _VsDataProvider.Description;
		}



		public override string GetDescriptionEx(VxbDataSource dataSource)
		{
			string text = $"Extended VxbDataSource Description for guid {dataSource.NameClsid}.";
			/*
			if (dataSource != null)
			{
				IVsDataSource vsDataSource = null; // DataPackage.SourceManager.Sources[new Guid(dataSource.Name)];
				if (vsDataSource is IVsDataInternalDataSourceDescriptionEx vsDataInternalDataSourceDescriptionEx)
				{
					text = vsDataInternalDataSourceDescriptionEx.GetDescriptionEx(_VsDataProvider.Guid);
					if (text != null)
					{
						return text;
					}
				}
			}
			IVsDataInternalDataProviderDescriptionEx vsDataInternalDataProviderDescriptionEx = _VsDataProvider as IVsDataInternalDataProviderDescriptionEx;
			if (_VsDataProvider != null)
			{
				text = vsDataInternalDataProviderDescriptionEx.DescriptionEx;
			}
			*/
			return text;
		}

		public override IDataConnectionUIControl CreateConnectionUIControl(VxbDataSource dataSource)
		{
			// Guid dataSourceGuid = ((dataSource != null) ? new Guid(dataSource.Guid) : Guid.Empty);
			IDataConnectionUIControl dataConnectionUIControl = null; //  _VsDataProvider.TryCreateObject<IDataConnectionUIControl>(dataSourceGuid);
			if (dataConnectionUIControl == null)
			{
				IVsDataConnectionUIControl vsDataConnectionUIControl = new VxbConnectionUIControl();
				// _VsDataProvider.TryCreateObject<IVsDataConnectionUIControl>(dataSourceGuid);

				if (vsDataConnectionUIControl != null)
					dataConnectionUIControl = new VxiDataConnectionUIControl(vsDataConnectionUIControl);
			}

			return dataConnectionUIControl;
		}


		public override IDataConnectionProperties CreateConnectionProperties(VxbDataSource dataSource)
		{
			Guid source = dataSource != null ? dataSource.NameClsid : Guid.Empty;
			IDataConnectionProperties dataConnectionProperties = _VsDataProvider.TryCreateObject<IDataConnectionProperties>(source);
			if (dataConnectionProperties == null)
			{
				IVsDataConnectionUIProperties vsDataConnectionUIProperties = _VsDataProvider.TryCreateObject<IVsDataConnectionUIProperties>(source);
				if (vsDataConnectionUIProperties != null)
				{
					if (vsDataConnectionUIProperties is IBsDataConnectionProperties uiConnectionProperties)
						uiConnectionProperties.ConnectionSource = EnConnectionSource.Session;

					dataConnectionProperties = new VxiDataConnectionUIProperties(source, vsDataConnectionUIProperties, _VsDataProvider);
				}
			}
			else
			{
				IVsDataConnectionUIProperties vsDataConnectionUIProperties = (IVsDataConnectionUIProperties)Reflect.GetFieldValue(dataConnectionProperties,
					"_connectionUIProperties");

				if (vsDataConnectionUIProperties is IBsDataConnectionProperties uiConnectionProperties)
					uiConnectionProperties.ConnectionSource = EnConnectionSource.Session;
			}

			return dataConnectionProperties;
		}

	}




	// ---------------------------------------------------------------------------------
	//						Internal Class VxiDataConnectionUIControl
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Orignal name: UIDataConnectionUIControl.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class VxiDataConnectionUIControl(IVsDataConnectionUIControl connectionUIControl)
		: IDataConnectionUIControl, IContainerControl
	{
		private readonly IVsDataConnectionUIControl _ConnectionUIControl = connectionUIControl;


		public Control ActiveControl
		{
			get { return _ConnectionUIControl.Control; }
			set { throw new NotImplementedException(); }
		}

		public void Initialize(IDataConnectionProperties connectionProperties)
		{
			IVsDataConnectionUIProperties vsDataConnectionUIProperties = connectionProperties as IVsDataConnectionUIProperties;
			if (vsDataConnectionUIProperties == null && connectionProperties is VxiDataConnectionUIProperties uIDataConnectionProperties)
			{
				vsDataConnectionUIProperties = uIDataConnectionProperties.InnerProperties;
			}

			_ConnectionUIControl.Site = vsDataConnectionUIProperties ?? throw new NotSupportedException();
		}

		public void LoadProperties()
		{
			_ConnectionUIControl.LoadProperties();
		}

		public bool ActivateControl(Control active)
		{
			throw new NotImplementedException();
		}
	}




	// ---------------------------------------------------------------------------------
	//					   Internal Class VxiDataConnectionUIProperties
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Orignal name: UIDataConnectionProperties.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class VxiDataConnectionUIProperties : IDataConnectionProperties, ICustomTypeDescriptor
	{
		private class DefaultConnectionUITester(Guid provider) : IVsDataConnectionUITester
		{
			private Guid _Provider = provider;

			public void Test(IVsDataConnectionUIProperties connectionUIProperties)
			{
				IVsDataConnectionFactory factory = Package.GetGlobalService(typeof(IVsDataConnectionFactory)) as IVsDataConnectionFactory
					?? throw Diag.ExceptionService(typeof(IVsDataConnectionFactory));
				IVsDataConnection vsDataConnection = factory.CreateConnection(_Provider, connectionUIProperties.ToString(), encryptedString: false);
				using (vsDataConnection)
				{
					IVsDataConnectionSupport vsDataConnectionSupport
						= vsDataConnection.GetService(typeof(IVsDataConnectionSupport)) as IVsDataConnectionSupport
						?? throw Diag.ExceptionService(typeof(IVsDataConnectionSupport));

					(vsDataConnectionSupport as IBsDataConnectionSupport).ConnectionSource = EnConnectionSource.Session;

					vsDataConnectionSupport.Open(false);
				}
			}
		}

		private Guid _DataSource;

		private readonly IVsDataConnectionUIProperties _DataConnectionUiProperties;

		private IVsDataConnectionUITester _DataConnectionUiTester;

		private readonly IVsDataProvider _Provider;

		public IVsDataConnectionUIProperties InnerProperties => _DataConnectionUiProperties;

		public bool IsExtensible => _DataConnectionUiProperties.IsExtensible;

		public object this[string propertyName]
		{
			get
			{
				if (_DataConnectionUiProperties.ContainsKey(propertyName))
				{
					object obj = _DataConnectionUiProperties[propertyName];
					if (obj == null)
					{
						return DBNull.Value;
					}
					return obj;
				}
				return null;
			}
			set
			{
				if (!Cmd.IsNullValue(value))
				{
					_DataConnectionUiProperties[propertyName] = value;
					return;
				}
				Reset(propertyName);
				if (value == null)
				{
					Remove(propertyName);
				}
			}
		}

		public bool IsComplete => _DataConnectionUiProperties.IsComplete;

		public event EventHandler PropertyChanged;

		public VxiDataConnectionUIProperties(Guid source, IVsDataConnectionUIProperties connectionUIProperties, IVsDataProvider provider)
		{
			_DataSource = source;
			_DataConnectionUiProperties = connectionUIProperties;
			_DataConnectionUiProperties.PropertyChanged += HandlePropertyChanged;
			_Provider = provider;
		}

		public void Reset()
		{
			_DataConnectionUiProperties.Reset();
		}

		public void Parse(string s)
		{
			_DataConnectionUiProperties.Parse(s);
		}

		public void Add(string propertyName)
		{
			try
			{
				_DataConnectionUiProperties.Add(propertyName, null);
			}
			catch (NotSupportedException ex)
			{
				throw new InvalidOperationException(ex.Message);
			}
		}

		public bool Contains(string propertyName)
		{
			return _DataConnectionUiProperties.ContainsKey(propertyName);
		}

		public void Remove(string propertyName)
		{
			try
			{
				_DataConnectionUiProperties.Remove(propertyName);
			}
			catch (NotSupportedException)
			{
			}
		}

		public void Reset(string propertyName)
		{
			_DataConnectionUiProperties.Reset(propertyName);
		}

		public void Test()
		{
			if (_DataConnectionUiTester == null)
			{
				_DataConnectionUiTester = _Provider.TryCreateObject<IVsDataConnectionUITester>(_DataSource);
				_DataConnectionUiTester ??= new DefaultConnectionUITester(_Provider.Guid);
			}
			_DataConnectionUiTester.Test(_DataConnectionUiProperties);
		}

		public override string ToString()
		{
			return ToFullString();
		}

		public string ToFullString()
		{
			return _DataConnectionUiProperties.ToString();
		}

		public string ToDisplayString()
		{
			return _DataConnectionUiProperties.ToDisplayString();
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return _DataConnectionUiProperties.GetClassName();
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return _DataConnectionUiProperties.GetComponentName();
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return _DataConnectionUiProperties.GetAttributes();
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return _DataConnectionUiProperties.GetEditor(editorBaseType);
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return _DataConnectionUiProperties.GetConverter();
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return _DataConnectionUiProperties.GetDefaultProperty();
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return _DataConnectionUiProperties.GetProperties();
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return _DataConnectionUiProperties.GetProperties(attributes);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return _DataConnectionUiProperties.GetDefaultEvent();
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return _DataConnectionUiProperties.GetEvents();
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return _DataConnectionUiProperties.GetEvents(attributes);
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return _DataConnectionUiProperties.GetPropertyOwner(pd);
		}

		private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}
	}


	#endregion Sub-Classes


}
