// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Common.RdtManager

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using static Microsoft.VisualStudio.Threading.SingleThreadedSynchronizationContext;



namespace BlackbirdSql;



// =========================================================================================================
//											RdtManager Class
//
/// <summary>
/// Provides application-wide static member access to the RunningDocumentTable. Consumers should always
/// access the Rdt through this class.
/// </summary>
// =========================================================================================================
internal sealed class RdtManager : AbstractRdtManager
{

	// ----------------------------------------------------
	#region Constructors / Destructors - AbstractRdtManager
	// ----------------------------------------------------


	/// <summary>
	/// Default .ctor of the singleton instance.
	/// </summary>
	private RdtManager() : base()
	{
	}


	/// <summary>
	/// Gets or creates the singelton instance of the RdtManager for this session.
	/// </summary>
	private static RdtManager Instance => (RdtManager)(_Instance ??= new RdtManager());


	/// <summary>
	/// IDisposable implementation
	/// </summary>
	public override void Dispose()
	{
		base.Dispose();
	}


	#endregion Constructors / Destructors





	// =====================================================================================================
	#region Fields - AbstractRdtManager
	// =====================================================================================================


	private static int _InflightMonikerCursor = -1;
	private static int _InflightMonikerSeed = -1;

	private static Dictionary<int, string> _InflightMonikers = null;
	private static Dictionary<string, IBsCsb> _InflightMonikerCsbTable = null;

	#endregion Fields





	// =====================================================================================================
	#region Property Accessors - RdtManager
	// =====================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Contains the registered fbsql++:// monikers of active editor documents in the
	/// Rdt that were spawned from SE nodes.
	/// The value is the Csb csb of the SE ConnectionNode the moniker was spawned
	/// from.
	/// Once an AuxilliaryDocData has used what it needs from the csb it should set it
	/// to null but leave the entry intact.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static Dictionary<string, IBsCsb> InflightMonikerCsbTable => _InflightMonikerCsbTable ??= [];


	internal static IEnumerable<RunningDocumentInfo> Enumerator => Instance.Rdt;

	internal static object LockGlobal => _LockGlobal;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Operates as a push/pop stack. Whenever inflight documents are added by the shell
	/// their monikers are assigned here against our internal fbsql++:// url document
	/// moniker for an SE node.
	/// This allows the Editor to do a single pass pop to associate an SE ConnectionNode
	/// against a new AuxilliaryDocData object.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static string PopInflightMonikerStack
	{
		get
		{
			if (_InflightMonikerCursor == -1)
				return null;

			string moniker = _InflightMonikers[_InflightMonikerCursor];

			_InflightMonikers.Remove(_InflightMonikerCursor);

			_InflightMonikerCursor++;

			if (_InflightMonikerCursor > _InflightMonikerSeed)
			{
				_InflightMonikerCursor = _InflightMonikerSeed = -1;
				_InflightMonikers = null;
			}

			return moniker;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Operates as a push/pop stack. Whenever inflight documents are added by the shell
	/// their monikers are assigned here against our internal fbsql++:// url document
	/// moniker for an SE node.
	/// This allows the Editor to do a single pass pop to associate an SE ConnectionNode
	/// against a new AuxilliaryDocData object.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static string PushInflightMonikerStack
	{
		set
		{
			_InflightMonikers ??= [];

			if (_InflightMonikerCursor == -1)
				_InflightMonikerCursor = 0;

			_InflightMonikerSeed++;

			_InflightMonikers[_InflightMonikerSeed] = value;

		}
	}


	internal static bool ServiceAvailable => Instance.RdtSvc != null;


	#endregion Property Accessors





	// =====================================================================================================
	#region Methods - RdtManager
	// =====================================================================================================


	internal static int AdviseRunningDocTableEvents(IVsRunningDocTableEvents pSink, out uint pdwCookie) =>
		Instance.RdtSvc.AdviseRunningDocTableEvents(pSink, out pdwCookie);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Launch ensure UI thread]: Invalidates the document window's toolbar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	[SuppressMessage("Usage", "VSTHRD102:Implement internal logic asynchronously")]
	internal static void InvalidateToolbarAsyeu(uint dwCookie)
	{
		if (dwCookie == 0)
			return;

		if (!ThreadHelper.CheckAccess())
		{
			// Fire and wait.

			bool result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				InvalidateToolbarImplAsyeu(dwCookie);

				return true;
			});

			return;
		}


		InvalidateToolbarImplAsyeu(dwCookie);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Launch ensure UI thread]: Invalidates the document window's toolbar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void InvalidateToolbarImplAsyeu(uint dwCookie)
	{
		IVsWindowFrame frame = GetWindowFrame(dwCookie);

		if (frame == null)
			return;

		if (!__(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, out object pToolbar)))
			return;

		if (pToolbar is not IVsToolWindowToolbarHost toolbarHost)
			return;

		toolbarHost.ForceUpdateUI();

		Application.DoEvents();
	}



	internal static CloseResult CloseDocument(__FRAMECLOSE options, uint docCookie) =>
		Instance.Rdt.CloseDocument(options, docCookie);



	internal static int FindAndLockDocument(uint dwRDTLockType, string pszMkDocument,
			out IVsHierarchy ppHier, out uint pitemid, out IntPtr ppunkDocData, out uint pdwCookie) =>
		Instance.FindAndLockDocumentImpl(dwRDTLockType, pszMkDocument, out ppHier, out pitemid,
			out ppunkDocData, out pdwCookie);



	internal static RunningDocumentInfo GetDocumentInfo(uint docCookie) =>
		Instance.Rdt.GetDocumentInfo(docCookie);


	internal static int GetDocumentInfo(uint docCookie, out uint pgrfRDTFlags, out uint pdwReadLocks,
			out uint pdwEditLocks, out string pbstrMkDocument, out IVsHierarchy ppHier,
			out uint pitemid, out IntPtr ppunkDocData) =>
		Instance.RdtSvc.GetDocumentInfo(docCookie, out pgrfRDTFlags, out pdwReadLocks,
			out pdwEditLocks, out pbstrMkDocument, out ppHier, out pitemid, out ppunkDocData);



	internal static string GetDocumentMoniker(uint cookie) =>
		Instance.RdtSvc4.GetDocumentMoniker(cookie);




	internal static uint GetRdtCookie(string mkDocument) =>
		Instance.GetRdtCookieImpl(mkDocument);



	internal static IVsWindowFrame GetWindowFrameForDocData(object docData, System.IServiceProvider serviceProvider)
	{
		Diag.ThrowIfNotOnUIThread();

		foreach (IVsWindowFrame frame in GetWindowFramesForDocData(docData, serviceProvider))
			return frame;

		return null;
	}



	internal static IEnumerable<IVsWindowFrame> GetWindowFramesForDocData(object docData, System.IServiceProvider serviceProvider)
	{
		Diag.ThrowIfNotOnUIThread();

		if (docData == null)
		{
			ArgumentNullException ex = new(nameof(docData));
			Diag.Ex(ex);
			throw ex;
		}

		___((serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell).GetDocumentWindowEnum(out IEnumWindowFrames windowFramesEnum));
		IVsWindowFrame[] windowFrames = new IVsWindowFrame[1];

		while (windowFramesEnum.Next(1u, windowFrames, out uint pceltFetched) == 0 && pceltFetched == 1)
		{
			IVsWindowFrame vsWindowFrame = windowFrames[0];
			___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out object pvar));
			if (pvar == docData)
			{
				yield return vsWindowFrame;
			}
		}
	}


	internal static void HandsOffDocument(uint cookie, string moniker) =>
		Instance.HandsOffDocumentImpl(cookie, moniker);


	internal static void HandsOnDocument(uint cookie, string moniker) =>
		Instance.HandsOnDocumentImpl(cookie, moniker);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invalidates the document window's toolbar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static async Task InvalidateToolbarAsync(uint dwCookie)
	{
		if (dwCookie == 0)
			return;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		InvalidateToolbarImplAsyeu(dwCookie);
	}



	internal static bool IsDirty(uint cookie) =>
		Instance.IsDirtyImpl(cookie);


	internal static bool IsFileInRdt(string mkDocument) =>
		Instance.IsFileInRdtImpl(mkDocument);

	internal static bool IsInflightMonikerRegistered(string mkDocument)
	{
		if (_InflightMonikerCsbTable == null)
			return false;

		return _InflightMonikerCsbTable.ContainsKey(mkDocument);
	}

	internal static bool IsInflightMonikerFilenameRegistered(string filename)
	{
		if (_InflightMonikerCsbTable == null)
			return false;

		foreach (KeyValuePair<string, IBsCsb> pair in _InflightMonikerCsbTable)
		{
			try
			{
				if (Cmd.GetFileName(pair.Key) == filename)
					return true;
			}
			catch (Exception ex)
			{
				Diag.Ex(ex, $"Inflight moniker: {pair.Key}.");
				throw;
			}
		}

		return false;
	}


	internal static int QueryCloseRunningDocument(string pszMkDocument, out int pfFoundAndClosed) =>
		Instance.QueryCloseRunningDocumentImpl(pszMkDocument, out pfFoundAndClosed);



	internal static int LockDocument(uint grfRDTLockType, uint dwCookie) =>
		Instance.RdtSvc.LockDocument(grfRDTLockType, dwCookie);



	internal static int NotifyDocumentChanged(uint dwCookie, uint grfDocChanged)
		=> Instance.RdtSvc.NotifyDocumentChanged(dwCookie, grfDocChanged);



	internal static int RegisterAndLockDocument(uint grfRDTLockType, string pszMkDocument,
			IVsHierarchy pHier, uint itemid, IntPtr punkDocData, out uint pdwCookie) =>
		Instance.RdtSvc.RegisterAndLockDocument(grfRDTLockType, pszMkDocument,
			pHier, itemid, punkDocData, out pdwCookie);



	internal static int RegisterDocumentLockHolder(uint grfRDLH, uint dwCookie, IVsDocumentLockHolder
			pLockHolder, out uint pdwLHCookie) =>
		Instance.RdtSvc.RegisterDocumentLockHolder(grfRDLH, dwCookie, pLockHolder, out pdwLHCookie);


	internal static int RenameDocument(string pszMkDocumentOld, string pszMkDocumentNew, IntPtr pHier, uint itemidNew) =>
		Instance.RdtSvc.RenameDocument(pszMkDocumentOld, pszMkDocumentNew, pHier, itemidNew);


	internal static int SaveDocuments(uint grfSaveOpts, IVsHierarchy pHier, uint itemid, uint docCookie) =>
		Instance.RdtSvc.SaveDocuments(grfSaveOpts, pHier, itemid, docCookie);




	internal static bool ShouldKeepDocDataAliveOnClose(uint docCookie) =>
		Instance.ShouldKeepDocDataAliveOnCloseImpl(docCookie);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Switches to the document's window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static async Task ShowWindowFrameAsync(uint cookie)
	{
		if (cookie == 0)
			return;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		ShowFrame(cookie);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// [Launch ensure UI thread]: Switches to the document's window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	[SuppressMessage("Usage", "VSTHRD102:Implement internal logic asynchronously")]
	internal static void ShowWindowFrameAsyeu(uint cookie)
	{
		if (cookie == 0)
			return;

		if (!ThreadHelper.CheckAccess())
		{
			// Fire and wait.

			bool result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				ShowFrame(cookie);

				return true;
			});

		}
		else
		{
			ShowFrame(cookie);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Deprecated because we let the shell create the document so we don't do any of
	/// the prep, but leaving this method in, in case we ever need it again.
	/// Provide either a moniker or codeWindow.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static void SuppressChangeTracking(string mkDocument, IVsCodeWindow codeWindow, bool suppress)
	{
		// Evs.Trace(typeof(AbstractDesignerServices), nameof(SuppressChangeTracking), "mkDocument: {0}.", mkDocument);

		if (codeWindow == null && !TryGetCodeWindow(mkDocument, out codeWindow))
			return;

		IVsTextView ppView = ((IBsEditorPane)codeWindow).GetCodeEditorTextView();

		if (ApcManager.GetService<SComponentModel>() is not IComponentModel componentModelSvc)
			return;

		if (componentModelSvc.GetService<IVsEditorAdaptersFactoryService>() is not IVsEditorAdaptersFactoryService factorySvc)
			return;

		if (factorySvc.GetWpfTextViewHost(ppView) is not IWpfTextViewHost wpfTextViewHost)
			return;

		if (suppress)
		{
			wpfTextViewHost.TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, value: false);
		}
		else
		{
			wpfTextViewHost.TextView.Options.ClearOptionValue(DefaultTextViewHostOptions.ChangeTrackingId);
		}

	}


	internal static object GetDocDataFromCookie(uint cookie) =>
		Instance.GetDocDataFromCookieImpl(cookie);


	internal static void ShowFrame(uint cookie)
	{
		string mkDocument = Instance.RdtSvc4.GetDocumentMoniker(cookie);

		if (string.IsNullOrEmpty(mkDocument))
			return;

		Instance.ShowFrameImpl(mkDocument);
	}


	internal static bool TryGetCodeWindow(string mkDocument, out IVsCodeWindow codeWindow)
	{
		codeWindow = null;

		if (string.IsNullOrEmpty(mkDocument))
			return false;

		IVsWindowFrame windowFrame = Instance.GetWindowFrameImpl(mkDocument);

		if (windowFrame == null)
			return false;

		Diag.ThrowIfNotOnUIThread();

		___(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar));

		if (pvar == null)
			return false;

		codeWindow = pvar as IVsCodeWindow;

		return codeWindow != null;
	}


	private static IVsWindowFrame GetWindowFrame(uint dwCookie) =>
		Instance.GetWindowFrameImpl(Instance.RdtSvc4.GetDocumentMoniker(dwCookie));



	internal static async Task<IVsWindowFrame> GetWindowFrameAsync(uint cookie)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		try
		{
			return GetWindowFrame(cookie);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			return null;
		}
	}



	internal static int UnadviseRunningDocTableEvents(uint dwCookie) =>
		Instance.RdtSvc.UnadviseRunningDocTableEvents(dwCookie);



	internal static int UnregisterDocumentLockHolder(uint dwLHCookie) =>
		Instance.RdtSvc.UnregisterDocumentLockHolder(dwLHCookie);


	internal static bool WindowFrameIsOnScreen(string mkDocument)
	{
		IVsWindowFrame frame = Instance.GetWindowFrameImpl(mkDocument);

		if (frame == null)
			return false;

		if (!__(frame.IsOnScreen(out int pfOnScreen)))
			return false;

		return pfOnScreen.AsBool();
	}


	#endregion Methods

}
