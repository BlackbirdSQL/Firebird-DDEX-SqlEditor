// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Common.RdtManager

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Interfaces;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql;



// =========================================================================================================
//											RdtManager Class
//
/// <summary>
/// Provides application-wide static member access to the RunningDocumentTable. Consumers should always
/// access the Rdt through this class.
/// </summary>
// =========================================================================================================
public sealed class RdtManager : AbstractRdtManager
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
	#region Property Accessors - RdtManager
	// =====================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Operates as a push/pop stack. Whenever inflight documents are added by the shell
	/// their monikers are assigned here against our internal fbsql++:// url document
	/// moniker for an SE node.
	/// This allows the Editor to do a single pass pop to associate an SE ConnectionNode
	/// against a new AuxilliaryDocData object.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string InflightMonikerStack
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
		set
		{
			_InflightMonikers ??= [];

			if (_InflightMonikerCursor == -1)
				_InflightMonikerCursor = 0;

			_InflightMonikerSeed++;

			_InflightMonikers[_InflightMonikerSeed] = value;

		}
	}


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
	public static Dictionary<string, object> InflightMonikerCsbTable => _InflightMonikerCsbTable ??= [];


	public static IEnumerable<RunningDocumentInfo> Enumerator => Instance.Rdt;

	public static object LockGlobal => _LockGlobal;

	public static bool ServiceAvailable => Instance.RdtSvc != null;


	#endregion Property Accessors





	// =====================================================================================================
	#region Methods - RdtManager
	// =====================================================================================================


	public static int AdviseRunningDocTableEvents(IVsRunningDocTableEvents pSink, out uint pdwCookie) =>
		Instance.RdtSvc.AdviseRunningDocTableEvents(pSink, out pdwCookie);



	public static CloseResult CloseDocument(__FRAMECLOSE options, uint docCookie) =>
		Instance.Rdt.CloseDocument(options, docCookie);



	public static int FindAndLockDocument(uint dwRDTLockType, string pszMkDocument,
			out IVsHierarchy ppHier, out uint pitemid, out IntPtr ppunkDocData, out uint pdwCookie) =>
		Instance.FindAndLockDocumentImpl(dwRDTLockType, pszMkDocument, out ppHier, out pitemid,
			out ppunkDocData, out pdwCookie);



	public static RunningDocumentInfo GetDocumentInfo(uint docCookie) =>
		Instance.Rdt.GetDocumentInfo(docCookie);


	public static int GetDocumentInfo(uint docCookie, out uint pgrfRDTFlags, out uint pdwReadLocks,
			out uint pdwEditLocks, out string pbstrMkDocument, out IVsHierarchy ppHier,
			out uint pitemid, out IntPtr ppunkDocData) =>
		Instance.RdtSvc.GetDocumentInfo(docCookie, out pgrfRDTFlags, out pdwReadLocks,
			out pdwEditLocks, out pbstrMkDocument, out ppHier, out pitemid, out ppunkDocData);



	public static string GetDocumentMoniker(uint cookie) =>
		Instance.RdtSvc4.GetDocumentMoniker(cookie);




	public static uint GetRdtCookie(string mkDocument) =>
		Instance.GetRdtCookieImpl(mkDocument);


	public static void HandsOffDocument(uint cookie, string moniker) =>
		Instance.HandsOffDocumentImpl(cookie, moniker);


	public static void HandsOnDocument(uint cookie, string moniker) =>
		Instance.HandsOnDocumentImpl(cookie, moniker);


	public static bool IsDirty(uint cookie) =>
		Instance.IsDirtyImpl(cookie);


	public static bool IsFileInRdt(string mkDocument) =>
		Instance.IsFileInRdtImpl(mkDocument);

	public static bool IsInflightMonikerRegistered(string mkDocument)
	{
		if (_InflightMonikerCsbTable == null)
			return false;

		return _InflightMonikerCsbTable.ContainsKey(mkDocument);
	}

	public static bool IsInflightMonikerFilenameRegistered(string filename)
	{
		if (_InflightMonikerCsbTable == null)
			return false;

		foreach (KeyValuePair<string, object> pair in _InflightMonikerCsbTable)
		{
			if (Path.GetFileName(pair.Key) == filename)
				return true;
		}

		return false;
	}


	public static int QueryCloseRunningDocument(string pszMkDocument, out int pfFoundAndClosed) =>
		Instance.QueryCloseRunningDocumentImpl(pszMkDocument, out pfFoundAndClosed);



	public static int LockDocument(uint grfRDTLockType, uint dwCookie) =>
		Instance.RdtSvc.LockDocument(grfRDTLockType, dwCookie);



	public static int NotifyDocumentChanged(uint dwCookie, uint grfDocChanged)
		=> Instance.RdtSvc.NotifyDocumentChanged(dwCookie, grfDocChanged);


	public static int RegisterAndLockDocument(uint grfRDTLockType, string pszMkDocument,
			IVsHierarchy pHier, uint itemid, IntPtr punkDocData, out uint pdwCookie) =>
		Instance.RdtSvc.RegisterAndLockDocument(grfRDTLockType, pszMkDocument,
			pHier, itemid, punkDocData, out pdwCookie);



	public static int RegisterDocumentLockHolder(uint grfRDLH, uint dwCookie, IVsDocumentLockHolder
			pLockHolder, out uint pdwLHCookie) =>
		Instance.RdtSvc.RegisterDocumentLockHolder(grfRDLH, dwCookie, pLockHolder, out pdwLHCookie);


	public static int RenameDocument(string pszMkDocumentOld, string pszMkDocumentNew, IntPtr pHier, uint itemidNew) =>
		Instance.RdtSvc.RenameDocument(pszMkDocumentOld, pszMkDocumentNew, pHier, itemidNew);


	public static int SaveDocuments(uint grfSaveOpts, IVsHierarchy pHier, uint itemid, uint docCookie) =>
		Instance.RdtSvc.SaveDocuments(grfSaveOpts, pHier, itemid, docCookie);




	public static bool ShouldKeepDocDataAliveOnClose(uint docCookie) =>
		Instance.ShouldKeepDocDataAliveOnCloseImpl(docCookie);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Deprecated because we let the shell create the document so we don't do any of
	/// the prep, but leaving this method in, in case we ever need it again.
	/// Provide either a moniker or codeWindow.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void SuppressChangeTracking(string mkDocument, IVsCodeWindow codeWindow, bool suppress)
	{
		// Tracer.Trace(typeof(AbstractDesignerServices), "SuppressChangeTracking()", "mkDocument: {0}.", mkDocument);

		if (codeWindow == null && !TryGetCodeWindow(mkDocument, out codeWindow))
			return;

		IVsTextView ppView = ((IBsEditorWindowPane)codeWindow).GetCodeEditorTextView();

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


	public static object GetDocDataFromCookie(uint cookie) =>
		Instance.GetDocDataFromCookieImpl(cookie);


	public static void ShowFrame(uint cookie)
	{
		string mkDocument = Instance.RdtSvc4.GetDocumentMoniker(cookie);

		if (string.IsNullOrEmpty(mkDocument))
			return;

		Instance.ShowFrameImpl(mkDocument);
	}


	public static bool TryGetCodeWindow(string mkDocument, out IVsCodeWindow codeWindow)
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


	public static IVsWindowFrame GetWindowFrame(string mkDocument) =>
		Instance.GetWindowFrameImpl(mkDocument);

	public static IVsWindowFrame GetWindowFrame(uint dwCookie) =>
		Instance.GetWindowFrameImpl(Instance.RdtSvc4.GetDocumentMoniker(dwCookie));



	public static int UnadviseRunningDocTableEvents(uint dwCookie) =>
		Instance.RdtSvc.UnadviseRunningDocTableEvents(dwCookie);



	public static int UnregisterDocumentLockHolder(uint dwLHCookie) =>
		Instance.RdtSvc.UnregisterDocumentLockHolder(dwLHCookie);


	#endregion Methods


}
