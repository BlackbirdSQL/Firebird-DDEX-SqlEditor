// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.EditorExtension.Ctl.Events;

using EnvDTE;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IOleUndoManager = Microsoft.VisualStudio.OLE.Interop.IOleUndoManager;
using Native = BlackbirdSql.Core.Native;


namespace BlackbirdSql.EditorExtension;

// =========================================================================================================
//										EditorEventsManager Class
//
/// <summary>
/// Manages Solution, RDT and Selection events for the editor extension.
/// </summary>
// =========================================================================================================
public sealed class EditorEventsManager : AbstractEditorEventsManager
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - EditorEventsManager
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// .ctor
	/// </summary>
	private EditorEventsManager(IBPackageController controller) : base(controller)
	{
	}


	/// <summary>
	/// Access to the static at the instance local level. This allows the base class to access and update
	/// the localized static instance.
	/// </summary>
	protected override IBEventsManager InternalInstance
	{
		get { return _Instance; }
		set { _Instance = value; }
	}


	/// <summary>
	/// Gets the instance of the Events Manager for this assembly.
	/// We do not auto-create to avoid instantiation confusion.
	/// Use CreateInstance() to instantiate.
	/// </summary>
	public static IBEventsManager Instance => _Instance ??
		throw Diag.ExceptionInstance(typeof(EditorEventsManager));


	/// <summary>
	/// Creates the singleton instance of the Events Manager for this assembly.
	/// Instantiation must always occur here and not by the Instance accessor to avoid
	/// confusion.
	/// </summary>
	public static EditorEventsManager CreateInstance(IBPackageController controller) =>
		new EditorEventsManager(controller);



	public override void Dispose()
	{
		Controller.OnAfterDocumentWindowHideEvent -= OnAfterDocumentWindowHide;
		Controller.OnAfterSaveEvent -= OnAfterSave;
		Controller.OnBeforeDocumentWindowShowEvent -= OnBeforeDocumentWindowShow;
		Controller.OnBeforeLastDocumentUnlockEvent -= OnBeforeLastDocumentUnlock;
		Controller.OnCmdUIContextChangedEvent -= OnCmdUIContextChanged;
		Controller.OnElementValueChangedEvent -= OnElementValueChanged;
		Controller.OnQueryCloseProjectEvent -= OnQueryCloseProject;
		Controller.OnSelectionChangedEvent -= OnSelectionChanged;
		Controller.OnNewQueryRequestedEvent -= OnNewQueryRequested;
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Constants and Fields - EditorEventsManager
	// =========================================================================================================


	public const int C_EID_UndoManager = 0;
	public const int C_EID_WindowFrame = 1;
	public const int C_EID_DocumentFrame = 2;
	public const int C_EID_StartupProject = 3;
	public const int C_EID_PropertyBrowserSID = 4;
	public const int C_EID_UserContext = 5;
	public const int C_EID_ResultList = 6;
	public const int C_EID_LastWindowFrame = 7;


	private static IBEventsManager _Instance;

	private uint _PublishingPreviewCommitOffCookie;
	// private uint _PreviewCommitOffCookie;
	private uint _NotBuildingCookie;
	private uint _SolutionOpeningCookie;

	private uint _SolutionOrProjectUpgradingCookie;

	private uint _ServerExplorerCookie;


	#endregion Constants and Fields





	// =========================================================================================================
	#region Property Accessors - EditorEventsManager
	// =========================================================================================================


	EditorExtensionAsyncPackage EditorPackage => (EditorExtensionAsyncPackage)DdexPackage;




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
				&& !GetUiContextValue(_SolutionOrProjectUpgradingCookie) && !GetUiContextValue(_PublishingPreviewCommitOffCookie))
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

				ErrorHandler.ThrowOnFailure(CurrentDocumentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar));
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

				ErrorHandler.ThrowOnFailure(CurrentDocumentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out pvar));
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

				ErrorHandler.ThrowOnFailure(CurrentWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out pvar));
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
	private int CleanupTemporarySqlItems(IVsUIHierarchy miscHierarchy)
	{
		Diag.ThrowIfNotOnUIThread();

		bool deleted = false;
		var itemid = VSConstants.VSITEMID_ROOT;
		object objProj;

		// Get the hierarchy root node.
		try
		{
			miscHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out objProj);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (objProj == null || objProj is not Project project)
		{
			ArgumentException ex = new($"Could not get hierarchy root for {miscHierarchy}");
			Diag.Dug(ex);
			throw ex;
		}


		if (project.ProjectItems == null || project.ProjectItems.Count == 0)
		{
			return VSConstants.S_OK;
		}

		foreach (ProjectItem projectItem in project.ProjectItems)
		{
			// Diag.Trace("Validating project item: " + projectItem.Name + ":  Kind: " + Kind(projectItem.Kind) + " FileCount: "
			//	+ projectItem.FileCount);
			deleted |= RemoveTemporarySqlItem(projectItem);
		}


		return VSConstants.S_OK;
	}


	private bool GetUiContextValue(uint cookie)
	{
		Diag.ThrowIfNotOnUIThread();

		SelectionMonitor.IsCmdUIContextActive(cookie, out var pfActive);

		if (pfActive != 1)
			return false;

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Removes a temporary editor item from the Misc project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool RemoveTemporarySqlItem(ProjectItem projectItem)
	{
		Diag.ThrowIfNotOnUIThread();

		if (projectItem.FileCount == 0 || Kind(projectItem.Kind) != "MiscItem")
			return false;

		bool deleted = false;
		// FileNames is 1 based indexing - How/Why??? - A VB team did this!
		string path = projectItem.FileNames[1];


		if (path.EndsWith(MonikerAgent.C_SqlExtension, StringComparison.InvariantCultureIgnoreCase))
		{
			try
			{
				projectItem.Remove();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				try
				{
					projectItem.Delete();
					deleted = true;
				}
				catch (Exception exd)
				{
					Diag.Dug(exd);
				}
			}
		}

		return deleted;
	}



	private void SetUiContextValue(uint cookie, bool value)
	{
		Diag.ThrowIfNotOnUIThread();

		SelectionMonitor.SetCmdUIContext(cookie, value ? 1 : 0);
	}

	public bool IsCommandContextActive(Guid commandContext)
	{
		int pfActive = 0;
		if (SelectionMonitor != null)
		{
			Diag.ThrowIfNotOnUIThread();

			ErrorHandler.ThrowOnFailure(SelectionMonitor.GetCmdUIContextCookie(ref commandContext, out var pdwCmdUICookie));
			ErrorHandler.ThrowOnFailure(SelectionMonitor.IsCmdUIContextActive(pdwCmdUICookie, out pfActive));
		}

		return pfActive == 1;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Hooks onto the controller's RDT and Selection events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void Initialize()
	{
		Controller.OnAfterDocumentWindowHideEvent += OnAfterDocumentWindowHide;
		Controller.OnAfterSaveEvent += OnAfterSave;
		Controller.OnBeforeDocumentWindowShowEvent += OnBeforeDocumentWindowShow;
		Controller.OnBeforeLastDocumentUnlockEvent += OnBeforeLastDocumentUnlock;
		Controller.OnCmdUIContextChangedEvent += OnCmdUIContextChanged;
		Controller.OnElementValueChangedEvent += OnElementValueChanged;
		Controller.OnQueryCloseProjectEvent += OnQueryCloseProject;
		Controller.OnSelectionChangedEvent += OnSelectionChanged;
		Controller.OnNewQueryRequestedEvent += OnNewQueryRequested;

		if (!ThreadHelper.CheckAccess())
		{
			bool result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				InitializeUnsafe();
				return true;
			});

			return;
		}

		InitializeUnsafe();

	}


	private void InitializeUnsafe()
	{
		Diag.ThrowIfNotOnUIThread();

		Controller.EnsureMonitorSelection();

		Native.ThrowOnFailure(SelectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out var pvarValue));
		CurrentDocumentFrame = pvarValue as IVsWindowFrame;

		Native.ThrowOnFailure(SelectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_WindowFrame, out pvarValue));
		CurrentWindowFrame = pvarValue as IVsWindowFrame;

		Native.ThrowOnFailure(SelectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_UndoManager, out pvarValue));
		CurrentUndoManager = pvarValue as IOleUndoManager;

		Guid rguidCmdUI = Core.VS.UICONTEXT_PublishingPreviewCommitOff;
		Native.ThrowOnFailure(SelectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out _PublishingPreviewCommitOffCookie));

		// rguidCmdUI = new(ServiceData.PreviewCommitOffGuid);
		// Native.ThrowOnFailure(MonitorSelection.GetCmdUIContextCookie(ref rguidCmdUI, out _PreviewCommitOffCookie));


		rguidCmdUI = VSConstants.StandardToolWindows.ServerExplorer;
		Native.ThrowOnFailure(SelectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out _ServerExplorerCookie));

		rguidCmdUI = VSConstants.UICONTEXT.NotBuildingAndNotDebugging_guid;
		Native.ThrowOnFailure(SelectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out _NotBuildingCookie));

		rguidCmdUI = VSConstants.UICONTEXT.SolutionOpening_guid;
		Native.ThrowOnFailure(SelectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out _SolutionOpeningCookie));

		rguidCmdUI = VSConstants.UICONTEXT.SolutionOrProjectUpgrading_guid;
		Native.ThrowOnFailure(SelectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out _SolutionOrProjectUpgradingCookie));

	}



	public bool HasAnyAuxiliaryDocData()
	{
		lock (Controller.LockGlobal)
		{
			return EditorPackage.DocDataEditors.Count > 0;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Cleans up any SE sql editor documents that may have been left dangling.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool ShouldStopClose(IVsHierarchy hierarchy)
	{
		if (!HasAnyAuxiliaryDocData())
		{
			return false;
		}


		foreach (RunningDocumentInfo item in new RunningDocumentTable(EditorPackage))
		{
			if (item.Hierarchy == hierarchy && !string.IsNullOrWhiteSpace(item.Moniker)
				&& item.Moniker.EndsWith(MonikerAgent.C_SqlExtension, StringComparison.OrdinalIgnoreCase)
				&& item.DocData != null)
			{
				AuxiliaryDocData docData = EditorPackage.GetAuxiliaryDocData(item.DocData);

				if (docData != null && ShouldStopClose(docData, GetType()))
				{
					return true;
				}
			}
		}


		return false;
	}


	#endregion Methods





	// =========================================================================================================
	#region IVs Events Implementation and Event handling - EditorEventsManager
	// =========================================================================================================


	public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
	{
		Diag.ThrowIfNotOnUIThread();

		lock (Controller.LockGlobal)
		{

			if (new RunningDocumentTable(EditorPackage).GetDocumentInfo(docCookie).IsDocumentInitialized
				&& Native.Succeeded(pFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar)))
			{
				if (pvar is SqlEditorTabbedEditorPane sqlEditorTabbedEditorPane
					&& sqlEditorTabbedEditorPane == EditorPackage.LastFocusedSqlEditor)
				{
					EditorPackage.LastFocusedSqlEditor = null;
				}
			}

			return VSConstants.S_OK;

		}

	}



	public int OnAfterSave(uint docCookie)
	{
		RunningDocumentInfo documentInfo = new RunningDocumentTable(EditorPackage).GetDocumentInfo(docCookie);

		if (documentInfo.IsDocumentInitialized && documentInfo.DocData != null)
		{
			AuxiliaryDocData auxDocData = EditorPackage.GetAuxiliaryDocData(documentInfo.DocData);
			if (auxDocData != null)
			{
				auxDocData.IsQueryWindow = false;
			}
		}

		return VSConstants.S_OK;
	}



	public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
	{
		Diag.ThrowIfNotOnUIThread();

		if (new RunningDocumentTable(EditorPackage).GetDocumentInfo(docCookie).IsDocumentInitialized
			&& Native.Succeeded(pFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar)))
		{
			if (pvar is SqlEditorTabbedEditorPane sqlEditorTabbedEditorPane)
			{
				EditorPackage.LastFocusedSqlEditor = sqlEditorTabbedEditorPane;
			}
		}

		return VSConstants.S_OK;
	}



	public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
	{
		RunningDocumentTable runningDocumentTable = new RunningDocumentTable(EditorPackage);

		if (dwReadLocksRemaining == 0 && dwEditLocksRemaining == 0)
		{
			RunningDocumentInfo documentInfo = runningDocumentTable.GetDocumentInfo(docCookie);

			if (documentInfo.IsDocumentInitialized && EditorPackage.ContainsEditorStatus(documentInfo.DocData))
			{
				EditorPackage.RemoveEditorStatus(documentInfo.DocData);
			}
		}
		else if (dwEditLocksRemaining == 1 && RdtManager.Instance.ShouldKeepDocDataAliveOnClose(docCookie))
		{
			RunningDocumentInfo documentInfo2 = runningDocumentTable.GetDocumentInfo(docCookie);

			if (documentInfo2.IsDocumentInitialized && EditorPackage.ContainsEditorStatus(documentInfo2.DocData))
			{
				AuxiliaryDocData auxDocData = EditorPackage.GetAuxiliaryDocData(documentInfo2.DocData);

				if (auxDocData != null)
					auxDocData.IntellisenseEnabled = null;
			}
		}

		return VSConstants.S_OK;
	}



	public int OnCmdUIContextChanged(uint cookie, int fActive)
	{
		return VSConstants.S_OK;
	}



	public int OnElementValueChanged(uint elementid, object oldValue, object newValue)
	{
		Diag.ThrowIfNotOnUIThread();

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
						ErrorHandler.ThrowOnFailure(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar));
					}

					object pvar2 = null;
					if (CurrentDocumentFrame != null)
					{
						ErrorHandler.ThrowOnFailure(CurrentDocumentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar2));
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
		Diag.ThrowIfNotOnUIThread();

		if (!Native.Succeeded(hierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out var pguid))
			|| !(pguid == VSConstants.GUID_ItemType_VirtualFolder))
		{
			return VSConstants.S_OK;
		}

		if (ShouldStopClose(hierarchy))
			cancel = 1;
		else
			CleanupTemporarySqlItems((IVsUIHierarchy)hierarchy);

		return VSConstants.S_OK;

	}




	public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMisOld, ISelectionContainer pScOld,
		IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMisNew, ISelectionContainer pScNew)
	{
		CurrentSelectionContainer = pScNew;
		MonitorSelectionChangedEvent?.Invoke(this, new MonitorSelectionEventArgs(pScOld, pScNew));

		return VSConstants.S_OK;
	}

	/// <summary>
	/// This roadblocks because key services in DataTools.Interop are protected.
	/// </summary>
	public int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType)
	{
		// This roadbloacks atm because of protection modfiers. TBC
		// new QueryDesignerDocument(site).Show(nodeSystemType);
		// host.QueryDesignerProviderTelemetry(qualityMetricProvider);
		return VSConstants.S_OK;
	}

	#endregion IVs Events Implementation and Event handling

}