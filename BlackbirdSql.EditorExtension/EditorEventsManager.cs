﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BlackbirdSql.Core;
using BlackbirdSql.EditorExtension.Events;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

using IOleUndoManager = Microsoft.VisualStudio.OLE.Interop.IOleUndoManager;



namespace BlackbirdSql.EditorExtension;


// =========================================================================================================
//										EditorEventsManager Class
//
/// <summary>
/// Manages Solution, RDT and Selection events for the editor extension.
/// </summary>
// =========================================================================================================
public sealed class EditorEventsManager : AbstractEventsManager
{

	// -----------------------------------------------------
	#region Constructors / Destructors - EditorEventsManager
	// -----------------------------------------------------


	/// <summary>
	/// Default .ctor fror singleton instance.
	/// </summary>
	private EditorEventsManager(IBsPackageController controller) : base(controller)
	{
	}


	/// <summary>
	/// Access to the singleton static at the instance local level.
	/// This allows the base class to access and update the localized
	/// static instance.
	/// </summary>
	protected override IBsEventsManager InternalInstance
	{
		get { return _Instance; }
		set { _Instance = value; }
	}


	/// <summary>
	/// Gets the instance of the Events Manager for this assembly.
	/// We do not auto-create to avoid instantiation confusion.
	/// Use CreateInstance() to instantiate.
	/// </summary>
	public static IBsEventsManager Instance => _Instance ??
		throw Diag.ExceptionInstance(typeof(EditorEventsManager));


	/// <summary>
	/// Creates the singleton instance of the Events Manager for this assembly.
	/// Instantiation must always occur here and not by the Instance accessor to avoid
	/// confusion.
	/// </summary>
	public static EditorEventsManager CreateInstance(IBsPackageController controller) =>
		new EditorEventsManager(controller);



	/// <summary>
	/// Implementation of <see cref="IDisposable"/>.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		Controller.OnAfterAttributeChangeExEvent -= OnAfterAttributeChangeEx;
		Controller.OnAfterDocumentWindowHideEvent -= OnAfterDocumentWindowHide;
		Controller.OnAfterSaveAsyncEvent -= OnAfterSaveAsync;
		Controller.OnBeforeDocumentWindowShowEvent -= OnBeforeDocumentWindowShow;
		Controller.OnBeforeLastDocumentUnlockEvent -= OnBeforeLastDocumentUnlock;
		Controller.OnElementValueChangedEvent -= OnElementValueChanged;
		// Controller.OnBeforeCloseProjectEvent -= OnBeforeCloseProject;
		Controller.OnQueryCloseProjectEvent -= OnQueryCloseProject;
		Controller.OnSelectionChangedEvent -= OnSelectionChanged;
		// Controller.OnNewQueryRequestedEvent -= OnNewQueryRequested;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Hooks onto the controller's RDT and Selection events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void Initialize()
	{
		Controller.OnAfterAttributeChangeExEvent += OnAfterAttributeChangeEx;
		Controller.OnAfterDocumentWindowHideEvent += OnAfterDocumentWindowHide;
		Controller.OnAfterSaveAsyncEvent += OnAfterSaveAsync;
		Controller.OnBeforeDocumentWindowShowEvent += OnBeforeDocumentWindowShow;
		Controller.OnBeforeLastDocumentUnlockEvent += OnBeforeLastDocumentUnlock;
		Controller.OnElementValueChangedEvent += OnElementValueChanged;
		// Controller.OnBeforeCloseProjectEvent += OnBeforeCloseProject;
		Controller.OnQueryCloseProjectEvent += OnQueryCloseProject;
		Controller.OnSelectionChangedEvent += OnSelectionChanged;
		// Controller.OnNewQueryRequestedEvent += OnNewQueryRequested;


		if (!ThreadHelper.CheckAccess())
		{
			// Fire and forget.

			Task.Run(InitializeUnsafeAsync).Forget();
			return;
		}

		InitializeUnsafeImpl();
	}

	private async Task<bool> InitializeUnsafeAsync()
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		InitializeUnsafeImpl();

		return true;
	}

	private bool InitializeUnsafeImpl()
	{
		Diag.ThrowIfNotOnUIThread();


		___(SelectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out var pvarValue));
		CurrentDocumentFrame = pvarValue as IVsWindowFrame;

		___(SelectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_WindowFrame, out pvarValue));
		CurrentWindowFrame = pvarValue as IVsWindowFrame;

		___(SelectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_UndoManager, out pvarValue));
		CurrentUndoManager = pvarValue as IOleUndoManager;

		Guid rclsidCommitOffCmdUI = VS.UICONTEXT_PublishingPreviewCommitOff;
		___(SelectionMonitor.GetCmdUIContextCookie(ref rclsidCommitOffCmdUI, out _PublishingPreviewCommitOffCookie));

		// rguidCmdUI = new(ServiceData.PreviewCommitOffGuid);
		// ___(MonitorSelection.GetCmdUIContextCookie(ref rguidCmdUI, out _PreviewCommitOffCookie));


		Guid rclsidServerExplorerCmdUI = VSConstants.StandardToolWindows.ServerExplorer;
		___(SelectionMonitor.GetCmdUIContextCookie(ref rclsidServerExplorerCmdUI, out _ServerExplorerCookie));

		Guid rclsidNotBuildingDebuggingCmdUI = VSConstants.UICONTEXT.NotBuildingAndNotDebugging_guid;
		___(SelectionMonitor.GetCmdUIContextCookie(ref rclsidNotBuildingDebuggingCmdUI, out _NotBuildingCookie));

		Guid rclsidSolutionOpeningCmdUI = VSConstants.UICONTEXT.SolutionOpening_guid;
		___(SelectionMonitor.GetCmdUIContextCookie(ref rclsidSolutionOpeningCmdUI, out _SolutionOpeningCookie));

		Guid rclsidUpgradingCmdUI = VSConstants.UICONTEXT.SolutionOrProjectUpgrading_guid;
		___(SelectionMonitor.GetCmdUIContextCookie(ref rclsidUpgradingCmdUI, out _SolutionOrProjectUpgradingCookie));

		return true;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - EditorEventsManager
	// =========================================================================================================


	public const int C_EID_UndoManager = 0;
	public const int C_EID_WindowFrame = 1;
	public const int C_EID_DocumentFrame = 2;
	public const int C_EID_StartupProject = 3;
	public const int C_EID_PropertyBrowserSID = 4;
	public const int C_EID_UserContext = 5;
	public const int C_EID_ResultList = 6;
	public const int C_EID_LastWindowFrame = 7;


	#endregion Constants





	// =========================================================================================================
	#region Fields - EditorEventsManager
	// =========================================================================================================


	private static IBsEventsManager _Instance;

	private uint _PublishingPreviewCommitOffCookie;
	// private uint _PreviewCommitOffCookie;
	private uint _NotBuildingCookie;
	private uint _SolutionOpeningCookie;
	private uint _SolutionOrProjectUpgradingCookie;
	private uint _ServerExplorerCookie;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - EditorEventsManager
	// =========================================================================================================


	EditorExtensionPackage EditorPackage => (EditorExtensionPackage)PackageInstance;


	public bool IsServerExplorerActive
	{
		get { return GetUiContextValue(_ServerExplorerCookie); }
		set { SetUiContextValue(_ServerExplorerCookie, value); }
	}


	public bool IsVisualStudioBusy
	{
		get
		{
			if (GetUiContextValue(_NotBuildingCookie) && !GetUiContextValue(_SolutionOpeningCookie)
				&& !GetUiContextValue(_SolutionOrProjectUpgradingCookie)
				&& !GetUiContextValue(_PublishingPreviewCommitOffCookie))
			{
				return false;
			}

			return true;
		}
	}


	public bool IsPublishing
	{
		get { return GetUiContextValue(_PublishingPreviewCommitOffCookie); }
		set { SetUiContextValue(_PublishingPreviewCommitOffCookie, value); }
	}


	public bool IsPreviewCommitOff
	{
		get { return IsPublishing; }
		set { IsPublishing = value; }
	}


	public object CurrentDocument
	{
		get
		{
			object pvar = null;

			if (CurrentDocumentFrame != null)
			{
				Diag.ThrowIfNotOnUIThread();

				___(CurrentDocumentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar));
			}

			return pvar;
		}
	}


	public object CurrentDocumentView
	{
		get
		{
			object pvar = null;

			if (CurrentDocumentFrame != null)
			{
				Diag.ThrowIfNotOnUIThread();

				___(CurrentDocumentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out pvar));
			}

			return pvar;
		}
	}


	public object CurrentWindow
	{
		get
		{
			object pvar = null;

			if (CurrentWindowFrame != null)
			{
				Diag.ThrowIfNotOnUIThread();

				___(CurrentWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out pvar));
			}

			return pvar;
		}
	}



	public ISelectionContainer CurrentSelectionContainer { get; private set; }
	public IOleUndoManager CurrentUndoManager { get; private set; }
	public IVsWindowFrame CurrentDocumentFrame { get; private set; }
	public IVsWindowFrame CurrentWindowFrame { get; private set; }



	public event EventHandler<MonitorSelectionEventArgs> MonitorWindowChangedEvent;
	public event EventHandler<MonitorSelectionEventArgs> MonitorDocumentChangedEvent;
	public event EventHandler<MonitorSelectionEventArgs> MonitorDocumentWindowChangedEvent;
	public event EventHandler<MonitorSelectionEventArgs> MonitorUndoManagerChangedEvent;
	public event EventHandler<MonitorSelectionEventArgs> MonitorSelectionChangedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - EditorEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Cleans up any SE sql editor documents that may have been left dangling.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private int CleanupTemporarySqlItems(IVsHierarchy projectHierarchy)
	{
		IDictionary<ProjectItem, (uint, string)> tempSqlItems = UnsafeCmd.GetOpenMiscProjectItems(projectHierarchy, [NativeDb.Extension], Path.GetTempPath());

		// Tracer.Trace(GetType(), "CleanupTemporarySqlItems()", "Count: {0}.", tempSqlItems.Count);

		foreach (KeyValuePair<ProjectItem, (uint, string)> pair in tempSqlItems)
		{
			// Tracer.Trace(GetType(), "CleanupTemporarySqlItems()", "Deleting project item: {0}.", projectItem.Name);

			// RdtManager.HandsOffDocument(pair.Value.Item1, pair.Value.Item2);

			try
			{
				pair.Key.Delete();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
			finally
			{
				// RdtManager.HandsOnDocument(pair.Value.Item1, pair.Value.Item2);
			}
		}


		return VSConstants.S_OK;
	}



	private bool GetUiContextValue(uint cookie)
	{
		Diag.ThrowIfNotOnUIThread();

		SelectionMonitor.IsCmdUIContextActive(cookie, out int pfActive);

		if (pfActive != 1)
			return false;

		return true;
	}



	public bool IsCommandContextActive(Guid commandContext)
	{
		int pfActive = 0;
		if (SelectionMonitor != null)
		{
			Diag.ThrowIfNotOnUIThread();

			___(SelectionMonitor.GetCmdUIContextCookie(ref commandContext, out uint pdwCmdUICookie));
			___(SelectionMonitor.IsCmdUIContextActive(pdwCmdUICookie, out pfActive));
		}

		return pfActive == 1;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Cleans up any SE sql editor documents that may have been left dangling.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool QueryAbortClose(IVsHierarchy hierarchy)
	{

		foreach (RunningDocumentInfo item in RdtManager.Enumerator)
		{
			if (item.Hierarchy == hierarchy && !string.IsNullOrWhiteSpace(item.Moniker)
				&& Path.GetExtension(item.Moniker).Equals(NativeDb.Extension, StringComparison.OrdinalIgnoreCase)
				&& item.DocData != null)
			{
				AuxilliaryDocData auxDocData = EditorPackage.GetAuxilliaryDocData(item.DocData);

				if (auxDocData != null && Shared.Cmd.ShouldStopCloseDialog(auxDocData, GetType()))
				{
					return true;
				}
			}
		}


		return false;
	}



	private async Task<bool> ResetDocumentStatusAsync(AuxilliaryDocData auxDocData, bool resetIntellisense)
	{
		try
		{
			// Tracer.Trace(GetType(), "ResetDocumentStatusAsync()", "ENTER!!!");

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			// HACK: to correct intellisense target. 
			if (resetIntellisense)
				auxDocData.IntellisenseEnabled = true;

			// Tracer.Trace(GetType(), "ResetDocumentStatusAsync()", "Hack for title update");
			// HACK: to kickstart dirty state title update.
			uint saveOpts = (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_ForceSave;
			RdtManager.SaveDocuments(saveOpts, null, uint.MaxValue, auxDocData.DocCookie);

			// RegisterForDirtyChangeNotification(auxDocData.DocCookie);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			Controller.EventRdtExit();

			// Tracer.Trace(GetType(), "ResetDocumentStatusAsync()", "FINALLY: Intellisense and RdtEvents enabled.");
		}

		return true;

	}



	private void SetUiContextValue(uint cookie, bool value)
	{
		Diag.ThrowIfNotOnUIThread();

		SelectionMonitor.SetCmdUIContext(cookie, value ? 1 : 0);
	}


	#endregion Methods





	// =========================================================================================================
	#region Event and Event handling - EditorEventsManager
	// =========================================================================================================


	public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld,
		uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew,
		string pszMkDocumentNew)
	{
		if (ApcManager.IdeShutdownState || !Controller.EventRdtEnter(false))
			return VSConstants.S_OK;

		AuxilliaryDocData auxDocData = null;
		object docData = RdtManager.GetDocDataFromCookie(docCookie);

		if (docData != null)
		{
			auxDocData = EditorPackage.GetAuxilliaryDocData(docData);

			// If no auxdocdata, it's not ours, exit.
			if (auxDocData == null)
				return VSConstants.S_OK;

			// Set the auxdocdata cookie if it was never set. 
			if (auxDocData.DocCookie != docCookie)
				auxDocData.DocCookie = docCookie;
		}

		// If it's not a name change, exit.
		if ((grfAttribs & (uint)__VSRDTATTRIB.RDTA_MkDocument) == 0)
			return VSConstants.S_OK;

		// The following code corrects the Intellisense target and kickstarts dirty state title update after a save (moniker rename).
		// If we don't kickstart, dirty state title updates take +- 120 seconds to resume. This is a strange anomaly.

		// Break the link between auxdocdata and the explorer moniker, if it exists, because this is now a disk file.
		if (auxDocData.InflightMoniker != null)
		{
			RdtManager.InflightMonikerCsbTable.Remove(auxDocData.InflightMoniker);
			auxDocData.InflightMoniker = null;
		}


		// If no intellisense, no need to fix targeting.
		bool resetIntellisense = auxDocData.IntellisenseEnabled.AsBool();

		// Tracer.Trace(GetType(), "OnAfterAttributeChangeEx()", "AuxilliaryDocData exists. \nOld mk: {0}\nNew mk: {1}",
		//	pszMkDocumentOld, pszMkDocumentNew);

		// Disable rdt events to avoid any recursion.
		if (!Controller.EventRdtEnter())
			return VSConstants.S_OK;

		if (resetIntellisense)
			auxDocData.IntellisenseEnabled = false;

		// Fire and forget.
		Task<bool> payloadAsync() =>
			ResetDocumentStatusAsync(auxDocData, resetIntellisense);

		_ = Task.Factory.StartNew(payloadAsync, default, TaskCreationOptions.PreferFairness, TaskScheduler.Default);

		// Tracer.Trace(GetType(), "OnAfterAttributeChangeEx()", "DONE!!! Intellisense and RdtEvents disabled");

		return VSConstants.S_OK;
	}



	public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
	{
		// Tracer.Trace(GetType(), "OnAfterDocumentWindowHide()");

		if (!ApcManager.IdeShutdownState)
			return VSConstants.S_OK;

		RunningDocumentInfo docInfo;

		lock (RdtManager.LockGlobal)
			docInfo = RdtManager.GetDocumentInfo(docCookie);

		if (docInfo.IsDocumentInitialized
			&& __(pFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar)))
		{
			if (pvar is TabbedEditorWindowPane sqlEditorTabbedEditorPane
				&& sqlEditorTabbedEditorPane == EditorPackage.LastFocusedSqlEditor)
			{
				EditorPackage.LastFocusedSqlEditor = null;
			}
		}

		return VSConstants.S_OK;

	}



	public IVsTask OnAfterSaveAsync(uint cookie, uint flags)
	{
		// Tracer.Trace(GetType(), "OnAfterSaveAsync()", "DocCookie: {0}.", cookie);

		RunningDocumentInfo documentInfo = RdtManager.GetDocumentInfo(cookie);

		if (!documentInfo.IsDocumentInitialized || documentInfo.DocData == null)
			return null;

		documentInfo.Sync();

		AuxilliaryDocData auxDocData = EditorPackage.GetAuxilliaryDocData(documentInfo.DocData);

		if (auxDocData == null)
			return null;

		if (auxDocData.DocCookie != cookie)
			auxDocData.DocCookie = cookie;

		if (!auxDocData.IsVirtualWindow)
			return null;

		auxDocData.IsVirtualWindow = false;


		return null;
	}



	public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
	{
		Diag.ThrowIfNotOnUIThread();

		RunningDocumentInfo documentInfo = RdtManager.GetDocumentInfo(docCookie);

		if (!documentInfo.IsDocumentInitialized)
			return VSConstants.S_OK;


		if (!__(pFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar)))
			return VSConstants.S_OK;

		if (pvar is TabbedEditorWindowPane sqlEditorTabbedEditorPane)
			EditorPackage.LastFocusedSqlEditor = sqlEditorTabbedEditorPane;

		return VSConstants.S_OK;
	}



	public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
	{
		Controller.EventRdtEnter(true, true);

		try
		{
			if (dwReadLocksRemaining == 0 && dwEditLocksRemaining == 0)
			{
				RunningDocumentInfo documentInfo = RdtManager.GetDocumentInfo(docCookie);

				if (documentInfo.IsDocumentInitialized && EditorPackage.AuxilliaryDocDataExists(documentInfo.DocData))
				{
					EditorPackage.RemoveAuxilliaryDocData(documentInfo.DocData);
				}
			}
			else if (dwEditLocksRemaining == 1 && RdtManager.ShouldKeepDocDataAliveOnClose(docCookie))
			{
				RunningDocumentInfo documentInfo2 = RdtManager.GetDocumentInfo(docCookie);

				if (documentInfo2.IsDocumentInitialized && EditorPackage.AuxilliaryDocDataExists(documentInfo2.DocData))
				{
					AuxilliaryDocData auxDocData = EditorPackage.GetAuxilliaryDocData(documentInfo2.DocData);

					if (auxDocData != null)
						auxDocData.IntellisenseEnabled = null;
				}
			}
		}
		finally
		{
			Controller.EventRdtExit();
		}


		return VSConstants.S_OK;
	}



	/*
	public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
	{
		// This event is fired after the misc project's items are no longer available. ?????????

		// Tracer.Trace(GetType(), "OnBeforeCloseProject()");

		if (fRemoved.AsBool() || !pHierarchy.IsMiscellaneous())
			return VSConstants.S_OK;

		CleanupTemporarySqlItems(pHierarchy as IVsUIHierarchy);

		return VSConstants.S_OK;
	}
	*/


	public int OnElementValueChanged(uint elementid, object oldValue, object newValue)
	{
		switch ((VSConstants.VSSELELEMID)elementid)
		{
			case VSConstants.VSSELELEMID.SEID_WindowFrame:
				CurrentWindowFrame = newValue as IVsWindowFrame;
				MonitorWindowChangedEvent?.Invoke(this, new MonitorSelectionEventArgs(oldValue, newValue));

				break;
			case VSConstants.VSSELELEMID.SEID_DocumentFrame:
				CurrentDocumentFrame = newValue as IVsWindowFrame;
				MonitorDocumentWindowChangedEvent?.Invoke(this, new MonitorSelectionEventArgs(oldValue, newValue));

				if (MonitorDocumentChangedEvent != null)
				{
					object pvar = null;
					if (oldValue is IVsWindowFrame vsWindowFrame)
					{
						___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar));
					}

					object pvar2 = null;
					if (CurrentDocumentFrame != null)
					{
						___(CurrentDocumentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar2));
					}

					if (pvar != pvar2)
					{
						MonitorDocumentChangedEvent(this, new MonitorSelectionEventArgs(oldValue, newValue));
					}
				}

				break;
			case VSConstants.VSSELELEMID.SEID_UndoManager:
				CurrentUndoManager = newValue as IOleUndoManager;
				MonitorUndoManagerChangedEvent?.Invoke(this, new MonitorSelectionEventArgs(oldValue, newValue));

				break;
		}

		return VSConstants.S_OK;
	}



	public int OnQueryCloseProject(IVsHierarchy hierarchy, int removing, ref int cancel)
	{
		// Tracer.Trace(GetType(), "OnQueryCloseProject()");

		bool hasAuxDocData;

		lock (EditorPackage.LockLocal)
			hasAuxDocData = EditorPackage.AuxilliaryDocDataTable.Count > 0;

		if (hasAuxDocData && QueryAbortClose(hierarchy))
		{
			cancel = 1;
			return VSConstants.S_OK;
		}


		if (!hierarchy.IsMiscellaneous())
			return VSConstants.S_OK;

		Controller.EventRdtEnter(true, true);

		try
		{
			CleanupTemporarySqlItems(hierarchy);
		}
		finally
		{
			Controller.EventRdtExit();
		}


		return VSConstants.S_OK;
	}




	public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMisOld, ISelectionContainer pScOld,
		IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMisNew, ISelectionContainer pScNew)
	{
		CurrentSelectionContainer = pScNew;
		MonitorSelectionChangedEvent?.Invoke(this, new MonitorSelectionEventArgs(pScOld, pScNew));

		return VSConstants.S_OK;
	}



	/*
	/// <summary>
	/// This roadblocks because key services in DataTools.Interop are protected.
	/// </summary>
	private int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType)
	{
		// This roadbloacks atm because of protection modifiers. TBC
		// new QueryDesignerDocument(site).Show(nodeSystemType);
		// host.QueryDesignerProviderTelemetry(qualityMetricProvider);
		return VSConstants.S_OK;
	}
	*/


	#endregion Events and Event handling


}