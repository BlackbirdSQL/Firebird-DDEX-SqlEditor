// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Common.RdtManager

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.Core.Controls.Interfaces;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Core.Ctl;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification="Using Diag.ThrowIfNotOnUIThread()")]
public sealed class RdtManager : AbstractRdtManager
{


	private RdtManager() : base()
	{
	}

	/// <summary>
	/// Gets or creates the instance of the RdtManager for this session.
	/// </summary>
	private static RdtManager Instance => (RdtManager)(_Instance ??= new RdtManager());


	public override void Dispose()
	{
		base.Dispose();
	}




	public static IEnumerable<RunningDocumentInfo> Enumerator => Instance.Rdt;

	public static object LockGlobal => _LockGlobal;

	public static bool ServiceAvailable => Instance.RdtSvc != null;




	public static int AdviseRunningDocTableEvents(IVsRunningDocTableEvents pSink, out uint pdwCookie) =>
		Instance.RdtSvc.AdviseRunningDocTableEvents(pSink, out pdwCookie);



	public static CloseResult CloseDocument(__FRAMECLOSE options, uint docCookie) =>
		Instance.Rdt.CloseDocument(options, docCookie);



	public static int FindAndLockDocument(uint dwRDTLockType, string pszMkDocument,
			out IVsHierarchy ppHier, out uint pitemid, out IntPtr ppunkDocData, out uint pdwCookie) =>
		Instance.FindAndLockDocumentImpl(dwRDTLockType, pszMkDocument, out ppHier, out pitemid,
			out ppunkDocData, out pdwCookie);


	public static bool GetChangeTrackingStatus(string mkDocument)
	{
		// Tracer.Trace(typeof(AbstractDesignerServices), "SuppressChangeTracking()", "mkDocument: {0}.", mkDocument);

		if (!TryGetCodeWindow(mkDocument, out IVsCodeWindow codeWindow) || codeWindow == null)
		{
			return false;
		}


		IVsTextView ppView = ((IBEditorWindowPane)codeWindow).GetCodeEditorTextView();

		// Tracer.Trace(typeof(AbstractDesignerServices), "SuppressChangeTracking()", "CodeWindow primary view found for mkDocument: {0}.", mkDocument);

		if (Controller.GetService<SComponentModel>() is not IComponentModel componentModel)
			return false;

		IVsEditorAdaptersFactoryService service = componentModel.GetService<IVsEditorAdaptersFactoryService>();
		if (service == null)
			return false;

		IWpfTextViewHost wpfTextViewHost = service.GetWpfTextViewHost(ppView);
		if (wpfTextViewHost == null)
			return false;

		return wpfTextViewHost.TextView.Options.GetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId);

		// Tracer.Trace(typeof(AbstractDesignerServices), "SuppressChangeTracking()", "TRACKING {0}", suppress ? "OFF" : "ON");

	}


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





	public static bool IsFileInRdt(string mkDocument) =>
		Instance.IsFileInRdtImpl(mkDocument);



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




	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.OnlineDocumentHelpers:SuppressChangeTracking
	public static void SuppressChangeTracking(string mkDocument, bool suppress)
	{
		// Tracer.Trace(typeof(AbstractDesignerServices), "SuppressChangeTracking()", "mkDocument: {0}.", mkDocument);

		if (!RdtManager.TryGetCodeWindow(mkDocument, out IVsCodeWindow codeWindow) || codeWindow == null)
		{
			return;
		}

		// Tracer.Trace(typeof(AbstractDesignerServices), "SuppressChangeTracking()", "CodeWindow '{0}' found for mkDocument: {1}.", codeWindow.GetType().FullName, mkDocument);

		/*
		if (codeWindow.GetPrimaryView(out IVsTextView ppView) != 0 || ppView == null)
		{
			return;
		}
		*/

		IVsTextView ppView = ((IBEditorWindowPane)codeWindow).GetCodeEditorTextView();

		// Tracer.Trace(typeof(AbstractDesignerServices), "SuppressChangeTracking()", "CodeWindow primary view found for mkDocument: {0}.", mkDocument);

		if (Controller.GetService<SComponentModel>() is not IComponentModel componentModel)
			return;

		IVsEditorAdaptersFactoryService service = componentModel.GetService<IVsEditorAdaptersFactoryService>();
		if (service == null)
			return;

		IWpfTextViewHost wpfTextViewHost = service.GetWpfTextViewHost(ppView);
		if (wpfTextViewHost == null)
			return;

		if (suppress)
		{
			wpfTextViewHost.TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, value: false);
		}
		else
		{
			wpfTextViewHost.TextView.Options.ClearOptionValue(DefaultTextViewHostOptions.ChangeTrackingId);
		}

		// Tracer.Trace(typeof(AbstractDesignerServices), "SuppressChangeTracking()", "TRACKING {0}", suppress ? "OFF" : "ON");

	}


	public static bool TryGetDocDataFromCookie(uint cookie, out object docData) =>
	Instance.TryGetDocDataFromCookieImpl(cookie, out docData);



	public static bool TryGetCodeWindow(string mkDocument, out IVsCodeWindow codeWindow)
	{
		codeWindow = null;

		if (!string.IsNullOrEmpty(mkDocument))
		{
			IVsWindowFrame windowFrame = Instance.GetWindowFrameImpl(mkDocument);
			if (windowFrame != null)
			{
				Diag.ThrowIfNotOnUIThread();

				Exf(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar));
				if (pvar != null)
				{
					codeWindow = pvar as IVsCodeWindow;
					return codeWindow != null;
				}
			}
		}

		return false;
	}


	public static IVsWindowFrame GetWindowFrame(string mkDocument) =>
		Instance.GetWindowFrameImpl(mkDocument);



	public static int UnadviseRunningDocTableEvents(uint dwCookie) =>
		Instance.RdtSvc.UnadviseRunningDocTableEvents(dwCookie);



	public static int UnlockDocument(uint grfRDTLockType, uint dwCookie) =>
		Instance.RdtSvc.UnlockDocument(grfRDTLockType, dwCookie);



	public static int UnregisterDocumentLockHolder(uint dwLHCookie) =>
		Instance.RdtSvc.UnregisterDocumentLockHolder(dwLHCookie);


	public static void UpdateDirtyState(uint cookie) =>
		Instance.RdtSvc3.UpdateDirtyState(cookie);

}
