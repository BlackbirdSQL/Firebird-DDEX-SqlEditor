#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Providers;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Operations;
using BlackbirdSql.Core.Model;
using BlackbirdSql.LanguageExtension.Interfaces;

namespace BlackbirdSql.LanguageExtension;


// Register services
[ProvideService(typeof(IBLanguageService), IsAsyncQueryable = true, ServiceName = ServiceData.LanguageServiceName)]
// [ProvideService(typeof(IBColorService), IsAsyncQueryable = true, ServiceName = ServiceData.ColorServiceName)]

[ProvideLanguageExtension(typeof(IBLanguageService), MonikerAgent.C_SqlExtension)]
[ProvideLanguageService(typeof(IBLanguageService), "FB-SQL", 0, CodeSense = true, EnableCommenting = true, MatchBraces = true, ShowSmartIndent = true, ShowCompletion = true, ShowMatchingBrace = true, AutoOutlining = true, EnableAsyncCompletion = true, EnableLineNumbers = true, DefaultToInsertSpaces = true, EnableFormatSelection = true, RequestStockColors = false, MatchBracesAtCaret = true, CodeSenseDelay = 0)]
[ProvideLanguageCodeExpansion(typeof(IBLanguageService), "FB-SQL Language", 1131, "FB-SQL", "$PackageFolder$\\Snippets\\FbSqlSnippetsIndex.xml", SearchPaths = "$PackageFolder$\\Snippets\\FB-SQL\\")]
// [CustomSnippetCodeExpansion("CSharp", "USQL CSharp", "$PackageFolder$\\Snippets\\CSharp\\FB-SQL\\")]
[ProvideBraceCompletion("FB-SQL")]



public abstract class LanguageExtensionAsyncPackage : AbstractAsyncPackage, IBLanguageExtensionAsyncPackage
{
	#region Variables - AbstractLanguageServicePackage



	private uint _ComponentID;
	private LanguageService _LanguageService = null;
	private IVsEditorAdaptersFactoryService _EditorAdaptersFactoryService = null;
	private ITextUndoHistoryRegistry _TextUndoHistoryRegistry = null;

	private SqlLanguagePreferences _Preferences;



	#endregion Variables





	// =========================================================================================================
	#region Property accessors - LanguageExtensionAsyncPackage
	// =========================================================================================================


	public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService
	{
		get
		{
			if (_EditorAdaptersFactoryService == null)
			{
				IComponentModel componentModel = ((AsyncPackage)this).GetService<SComponentModel, IComponentModel>();
				_EditorAdaptersFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
			}
			return _EditorAdaptersFactoryService;
		}
	}


	public ITextUndoHistoryRegistry TextUndoHistoryRegistry
	{
		get
		{
			if (_TextUndoHistoryRegistry == null)
			{
				IComponentModel componentModel = ((AsyncPackage)this).GetService<SComponentModel, IComponentModel>();
				_TextUndoHistoryRegistry = componentModel.GetService<ITextUndoHistoryRegistry>();
			}
			return _TextUndoHistoryRegistry;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Accessor to this package's <see cref="IBLanguageService"/> else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual LanguageService LanguageService => _LanguageService ??= new DslLanguageService();


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - LanguageExtensionAsyncPackage
	// =========================================================================================================


	#endregion Constructors / Destructors





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Asynchronous initialization of the package. The class must register services it
	/// requires using the ServicesCreatorCallback method.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		await base.InitializeAsync(cancellationToken, progress);

		Services.AddService(typeof(IBLanguageService), ServicesCreatorCallbackAsync, promote: true);
		Services.AddService(typeof(IBColorService), ServicesCreatorCallbackAsync, promote: true);

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Final asynchronous initialization tasks for the package that must occur after
	/// all descendents and ancestors have completed their InitializeAsync() tasks.
	/// It is the final descendent package class's responsibility to initiate the call
	/// to FinalizeAsync.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task FinalizeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		await base.FinalizeAsync(cancellationToken, progress);


		_ = TextUndoHistoryRegistry;
		_ = EditorAdaptersFactoryService;

		// IsInitialized = true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service instance of the specified type if this class has access to the
	/// final class type of the service being added.
	/// The class requiring and adding the service may not necessarily be the class that
	/// creates an instance of the service.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override async Task<object> CreateServiceInstanceAsync(Type serviceType, CancellationToken token)
	{
		if (serviceType == null)
		{
			ArgumentNullException ex = new("serviceType");
			Diag.Dug(ex);
			throw ex;
		}

		if (serviceType == typeof(IBLanguageService))
		{
			try
			{
				return LanguageService ?? throw new TypeAccessException(serviceType.FullName);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

		}
		else if (serviceType == typeof(IBColorService))
		{
			try
			{
				if (await GetServiceAsync(typeof(SVsUIShell)) is not IVsUIShell5 uiShell)
				{
					TypeAccessException ex = new(typeof(IVsUIShell5).FullName);
					Diag.Dug(ex);
					throw ex;
				}

				return ColorService.ColorService.GetInstance(uiShell) ?? throw new TypeAccessException(serviceType.FullName);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
		}
		else if (serviceType.IsInstanceOfType(this))
		{
			return this;
		}

		return null;
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
		if (serviceType == typeof(IBLanguageService))
		{
			DslLanguageService service = (DslLanguageService) await CreateServiceInstanceAsync(serviceType, token);

			try
			{
				service.SetSite(this);

				if (_ComponentID == 0 && await GetServiceAsync(typeof(SOleComponentManager)) is IOleComponentManager oleComponentManager)
				{
					OLECRINFO[] array = new OLECRINFO[1];
					array[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
					array[0].grfcrf = (uint)(_OLECRF.olecrfNeedIdleTime | _OLECRF.olecrfNeedPeriodicIdleTime);
					array[0].grfcadvf = (uint)(_OLECADVF.olecadvfModal | _OLECADVF.olecadvfRedrawOff | _OLECADVF.olecadvfWarningsOff);
					array[0].uIdleTimeInterval = 1000u;
					oleComponentManager.FRegisterComponent(this, array, out _ComponentID);
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			return service;
		}
		else if (serviceType == typeof(IBColorService))
		{
			ColorService.ColorService service = (ColorService.ColorService) await CreateServiceInstanceAsync(serviceType, token);

			return service;
		}


		return null;
	}



	protected override void Dispose(bool disposing)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		try
		{
			if (_ComponentID != 0)
			{
				(GetService(typeof(SOleComponentManager)) as IOleComponentManager)?.FRevokeComponent(_ComponentID);
				_ComponentID = 0u;
			}

			PreferencesDispose();
		}
		finally
		{
			base.Dispose(disposing);
		}
	}



	public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
	{
		return 1;
	}

	public int FDoIdle(uint grfidlef)
	{
		// Instance.LanguageService?.OnIdle((grfidlef & 1) != 0);
		((DslLanguageService)LanguageService)?.OnIdle((grfidlef & 1) != 0);
		return VSConstants.S_OK;
	}

	public int FPreTranslateMessage(MSG[] pMsg)
	{
		return VSConstants.S_OK;
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

	public void Terminate()
	{
	}

	public void PreferencesDispose()
	{
		if (_Preferences != null)
		{
			_Preferences.Dispose();
			_Preferences = null;
		}
	}

	public SqlLanguagePreferences GetLanguagePreferences()
	{
		if (_Preferences == null)
		{
			_Preferences = new SqlLanguagePreferences(this, new Guid(LibraryData.DslLanguageServiceGuid), "SQL Server Tools");
			_Preferences.Init();
		}

		return _Preferences;
	}

}
