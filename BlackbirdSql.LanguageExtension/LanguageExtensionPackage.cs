// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.Package
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core;
using BlackbirdSql.LanguageExtension.Ctl.ComponentModel;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using BlackbirdSql.LanguageExtension.Services;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;



namespace BlackbirdSql.LanguageExtension;


// =========================================================================================================
//										LanguageExtensionPackage Class 
//
/// <summary>
/// BlackbirdSql Language Extension <see cref="AsyncPackage"/> class implementation
/// </summary>
// =========================================================================================================



// ---------------------------------------------------------------------------------------------------------
#region							LanguageExtensionPackage Class Attributes
// ---------------------------------------------------------------------------------------------------------


[ProvideService(typeof(LsbLanguageService), IsAsyncQueryable = true, ServiceName = PackageData.C_LanguageServiceName)]
[ProvideLanguageService(typeof(LsbLanguageService), PackageData.C_LanguageLongName, 330, CodeSense = true, EnableCommenting = true, MatchBraces = true, ShowCompletion = true, ShowMatchingBrace = true, AutoOutlining = true, EnableAsyncCompletion = true, MaxErrorMessages = 200, CodeSenseDelay = 500)]

[VsProvideEditorAutomationPage(typeof(SettingsProvider.AdvancedPreferencesPage), SettingsProvider.CategoryName, "Advanced", 300, 330)]
[ProvideLanguageEditorOptionPage(typeof(SettingsProvider.AdvancedPreferencesPage), SettingsProvider.CategoryName, "Advanced", null, "#331")]
[ProvideProfile(typeof(SettingsProvider.AdvancedPreferencesPage), SettingsProvider.CategoryName, "Editor", 300, 330, false, AlternateParent = "AutomationProperties\\TextEditor")]

[ProvideLanguageExtension(typeof(LsbLanguageService), PackageData.C_Extension)]

[ProvideLanguageCodeExpansion(typeof(LsbLanguageService), "SQL_FIREBIRD", 303, "SQL_FIREBIRD", "$PackageFolder$\\Snippets\\SnippetsIndex.xml", SearchPaths = "$PackageFolder$\\Snippets\\Function;$PackageFolder$\\Snippets\\Index;$PackageFolder$\\Snippets\\Role;$PackageFolder$\\Snippets\\Surround;$PackageFolder$\\Snippets\\Stored Procedure;$PackageFolder$\\Snippets\\Table;$PackageFolder$\\Snippets\\Trigger;$PackageFolder$\\Snippets\\User;$PackageFolder$\\Snippets\\View;%MyDocs%\\Code Snippets\\SQL_FIREBIRD\\My Code Snippets", ForceCreateDirs = "%MyDocs%\\Code Snippets\\SQL_FIREBIRD\\My Code Snippets")]


#endregion Class Attributes



// =========================================================================================================
#region							LanguageExtensionPackage Class Declaration
// =========================================================================================================
public abstract class LanguageExtensionPackage : AbstractCorePackage, IBsLanguagePackage, IOleComponent
{

	// ----------------------------------------------------------
	#region Constructors / Destructors - LanguageExtensionPackage
	// ----------------------------------------------------------


	protected LanguageExtensionPackage() : base()
	{
		// SyncServiceContainer.AddService(typeof(LsbLanguageService), ServicesCreatorCallback, promote: true);
	}


	/// <summary>
	/// LanguageExtensionPackage static .ctor
	/// </summary>
	static LanguageExtensionPackage()
	{
		RegisterAssemblies();
	}


	/// <summary>
	/// Gets the singleton Package instance
	/// </summary>
	public static new LanguageExtensionPackage Instance
	{
		get
		{
			// if (_Instance == null)
			//	DemandLoadPackage(Sys.LibraryData.AsyncPackageGuid, out _);
			return (LanguageExtensionPackage)_Instance;
		}
	}



	/// <summary>
	/// LanguageExtensionPackage package disposal
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (_ComponentID != 0)
			{
				if (GetService(typeof(SOleComponentManager)) is IOleComponentManager oleComponentManager)
				{
					oleComponentManager.FRevokeComponent(_ComponentID);
				}
				_ComponentID = 0u;
			}
			UserDispose();
		}
		finally
		{
			base.Dispose(disposing);
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants & Fields - LanguageExtensionPackage
	// =========================================================================================================


	private uint _ComponentID;
	private LsbLanguageService _LsbLanguageSvc;
	private LsbLanguagePreferences _UserPreferences = null;


	#endregion Constants & Fields





	// =========================================================================================================
	#region Property accessors - LanguageExtensionPackage
	// =========================================================================================================


	public ITextUndoHistoryRegistry TextUndoHistoryRegistrySvc { get; private set; }

	public IVsEditorAdaptersFactoryService EditorAdaptersFactorySvc { get; private set; }





	public IBsLanguageService LanguageSvc => LsbLanguageSvc;


	public LsbLanguageService LsbLanguageSvc
	{
		get
		{
			if (_LsbLanguageSvc == null)
			{
				if (PersistentSettings.EditorLanguageService != EnLanguageService.FbSql)
					return null;

				ThreadHelper.Generic.Invoke(delegate
				{
					GetService(typeof(LsbLanguageService));
				});
			}
			return _LsbLanguageSvc;
		}
	}


	public bool UserPreferencesExist
	{
		get
		{
			using RegistryKey registryKey = UserRegistryRoot;

			string settingsKey = PackageData.C_RegistrySettingsKey;
			RegistryKey registryKey2 = registryKey.OpenSubKey(settingsKey, writable: false);

			return registryKey2 != null;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Package Methods Implementations - LanguageExtensionPackage
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Final asynchronous initialization tasks for the package that must occur after
	/// all descendents and ancestors have completed their InitializeAsync() tasks.
	/// It is the final descendent package class's responsibility to initiate the call
	/// to FinalizeAsync.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task FinalizeAsync(CancellationToken cancelToken, IProgress<ServiceProgressData> progress)
	{
		Diag.ThrowIfNotOnUIThread();

		if (cancelToken.Cancelled() || ApcManager.IdeShutdownState)
			return;


		ProgressAsync(progress, "Finalizing Language Services...").Forget();

		await base.FinalizeAsync(cancelToken, progress);



		ProgressAsync(progress, "Finalizing Language Services. Proffering services...").Forget();

		// TODO: Used by Declarations.

		IComponentModel componentModel = await GetServiceAsync<SComponentModel, IComponentModel>();

		TextUndoHistoryRegistrySvc = componentModel.GetService<ITextUndoHistoryRegistry>();
		EditorAdaptersFactorySvc = componentModel.GetService<IVsEditorAdaptersFactoryService>();

		// _ = SqlSchemaModel.ModelSchema;

		ProgressAsync(progress, "Finalizing Language Services. Proffering Language services... Done.").Forget();



		ProgressAsync(progress, "Finalizing Language Services... Done.").Forget();

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service instance of the specified type if this class has access to the
	/// final class type of the service being added.
	/// The class requiring and adding the service may not necessarily be the class that
	/// creates an instance of the service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<TInterface> GetLocalServiceInstanceAsync<TService, TInterface>(CancellationToken token)
		 where TInterface : class
	{
		Type serviceType = typeof(TService);

		if (serviceType == typeof(LanguageService) || serviceType == typeof(LsbLanguageService))
		{
			if (_LsbLanguageSvc != null)
				return _LsbLanguageSvc as TInterface;

			if (PersistentSettings.EditorLanguageService != EnLanguageService.FbSql)
				return null;

			_LsbLanguageSvc = new LsbLanguageService(this);
			_LsbLanguageSvc.SetSite(this);


			if (_ComponentID == 0 && await GetServiceAsync(typeof(SOleComponentManager)) is IOleComponentManager oleComponentManager)
			{
				OLECRINFO[] array = new OLECRINFO[1];
				array[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
				array[0].grfcrf = 3u;
				array[0].grfcadvf = 7u;
				array[0].uIdleTimeInterval = 1000u;
				oleComponentManager.FRegisterComponent(this, array, out _ComponentID);
			}

			return _LsbLanguageSvc as TInterface;
		}


		return await base.GetLocalServiceInstanceAsync<TService, TInterface>(token);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override async Task InitializeAsync(CancellationToken cancelToken, IProgress<ServiceProgressData> progress)
	{
		ProgressAsync(progress, "Initializing Language services...").Forget();


		ProgressAsync(progress, "Language service registering...").Forget();

		AddService(typeof(LsbLanguageService), ServicesCreatorCallbackAsync, promote: true);

		ProgressAsync(progress, "Language registering service... Done.").Forget();

		await base.InitializeAsync(cancelToken, progress);

		ProgressAsync(progress, "Initializing Language services... Done.").Forget();

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initializes and configures a service of the specified type that is used by this
	/// Package.
	/// Configuration is performed by the class requiring the service.
	/// The actual instance creation of the service is the responsibility of the class
	/// Package that has access to the final descendent class of the Service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<object> ServicesCreatorCallbackAsync(IAsyncServiceContainer container, CancellationToken token, Type serviceType)
	{


		if (serviceType == typeof(LanguageService) || serviceType == typeof(LsbLanguageService))
			return await GetLocalServiceInstanceAsync<LanguageService, LsbLanguageService>(token);


		return await base.ServicesCreatorCallbackAsync(container, token, serviceType);
	}


	#endregion Package Methods Implementations





	// =========================================================================================================
	#region Methods - LanguageExtensionPackage
	// =========================================================================================================


	public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
	{
		return 1;
	}

	public int FDoIdle(uint grfidlef)
	{
		Instance.LsbLanguageSvc?.OnIdle((grfidlef & 1) != 0);
		return 0;
	}

	public int FPreTranslateMessage(MSG[] pMsg)
	{
		return 0;
	}

	public int FQueryTerminate(int fPromptUser)
	{
		return 1;
	}

	public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
	{
		return 1;
	}



	public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
	{
		return IntPtr.Zero;
	}


	public LsbLanguagePreferences GetUserPreferences()
	{
		if (_UserPreferences == null)
		{
			_UserPreferences = new LsbLanguagePreferences(this, typeof(LsbLanguageService).GUID, PackageData.C_LanguageLongName);
			_UserPreferences.Init();
		}
		return _UserPreferences;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds this assembly to CurrentDomain.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void RegisterAssemblies()
	{
		AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
		{
			if (args.Name == typeof(LanguageExtensionPackage).Assembly.FullName)
				return typeof(LanguageExtensionPackage).Assembly;

			return null;
		};
	}



	public override void SaveUserPreferences()
	{
		using RegistryKey registryKey = base.UserRegistryRoot;

		string settingsKey = PackageData.C_RegistrySettingsKey;
		object languagePreferences = GetUserPreferences();
		RegistryKey registryKey2 = registryKey.OpenSubKey(settingsKey, writable: true);

		registryKey2 ??= registryKey.CreateSubKey(settingsKey);

		IList<string> savedProperties = [];

		using (registryKey2)
		{
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(languagePreferences, []))
			{
				TypeConverter converter = property.Converter;
				if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
				{
					savedProperties.Add(property.Name);
					registryKey2.SetValue(property.Name, converter.ConvertToInvariantString(property.GetValue(languagePreferences)));
				}
			}
		}

	}



	public void Terminate()
	{
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates and saves LanguagePreferences from the settings model if exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void UpdateUserPreferences()
	{
		if (_UserPreferences != null /* && _UserPreferences.IsDirty */)
		{
			_UserPreferences.Update();
			_UserPreferences.Apply();
		}
	}



	private void UserDispose()
	{
		if (_UserPreferences != null)
		{
			_UserPreferences.Dispose();
			_UserPreferences = null;
		}
	}


	#endregion Methods and Implementations




	// =========================================================================================================
	#region Event handlers - LanguageExtensionPackage
	// =========================================================================================================


	public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
	{
	}

	public void OnAppActivate(int fActive, uint dwOtherThreadID)
	{
	}

	public void OnEnterState(uint uStateID, int fEnter)
	{
	}

	public void OnLoseActivation()
	{
	}


	#endregion Event handlers


}

#endregion Class Declaration
