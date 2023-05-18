using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using BlackbirdSql.Common.Commands;

namespace BlackbirdSql.Common.Provider;


[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]

internal class DocumentManager
{

	public enum WindowFrameShowAction
	{
		DoNotShow,
		Show,
		ShowNoActivate,
		Hide,
	}

	#region fields
	private readonly IVsDataExplorerNode _Node = null;
	#endregion

	#region properties
	protected IVsDataExplorerNode Node
	{
		get
		{
			return _Node;
		}
	}
	#endregion

	#region ctors
	public DocumentManager(IVsDataExplorerNode node)
	{
		_Node = node;
	}
	#endregion

	#region virtual methods

	/// <summary>
	/// Open a document using the standard editor. This method has no implementation since a document is abstract in this context
	/// </summary>
	/// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
	/// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
	/// <param name="windowFrame">A reference to the window frame that is mapped to the document</param>
	/// <param name="windowFrameAction">Determine the UI action on the document window</param>
	/// <returns>NotImplementedException</returns>
	/// <remarks>See FileDocumentManager class for an implementation of this method</remarks>
	public virtual int Open(ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame, WindowFrameShowAction windowFrameAction)
	{
		Guid editorGuid = VSConstants.GUID_ProjectDesignerEditor;
		return OpenWithSpecific(0, ref editorGuid, string.Empty, ref logicalView, docDataExisting, out windowFrame, windowFrameAction);
	}

	/// <summary>
	/// Open a document using a specific editor. This method has no implementation.
	/// </summary>
	/// <param name="editorFlags">Specifies actions to take when opening a specific editor. Possible editor flags are defined in the enumeration Microsoft.VisualStudio.Shell.Interop.__VSOSPEFLAGS</param>
	/// <param name="editorType">Unique identifier of the editor type</param>
	/// <param name="physicalView">Name of the physical view. If null, the environment calls MapLogicalView on the editor factory to determine the physical view that corresponds to the logical view. In this case, null does not specify the primary view, but rather indicates that you do not know which view corresponds to the logical view</param>
	/// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
	/// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
	/// <param name="frame">A reference to the window frame that is mapped to the document</param>
	/// <param name="windowFrameAction">Determine the UI action on the document window</param>
	/// <returns>NotImplementedException</returns>
	/// <remarks>See FileDocumentManager for an implementation of this method</remarks>
	public virtual int OpenWithSpecific(uint editorFlags, ref Guid editorType, string physicalView, ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame, WindowFrameShowAction windowFrameAction)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		frame = null;

		try
		{
			Debug.Assert(editorType == VSConstants.GUID_ProjectDesignerEditor, "Cannot open project designer with guid " + editorType.ToString());
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}



		if (_Node == null)
			return VSConstants.E_FAIL;

		if (logicalView == null)
			logicalView = new(DataToolsCommands.SqlEditorGuid);


		if (_Node.ExplorerConnection.Connection.GetService(typeof(IOleServiceProvider)) is IOleServiceProvider serviceProvider && _Node.ExplorerConnection.Connection.GetService(typeof(SVsUIShellOpenDocument)) is IVsUIShellOpenDocument uiShellOpenDocument)
		{
			string fullPath = GetFullPathForDocument();
			string caption = GetOwnerCaption();

			IVsUIHierarchy parentHierarchy = null;

			int parentHierarchyItemId = _Node.ItemId;

			try
			{ 
				ErrorHandler.ThrowOnFailure(uiShellOpenDocument.OpenSpecificEditor(editorFlags, fullPath, ref editorType, physicalView,
					ref logicalView, caption, parentHierarchy, (uint)parentHierarchyItemId, docDataExisting, serviceProvider, out frame));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			if (frame != null)
			{
				if (windowFrameAction == WindowFrameShowAction.Show)
				{
					try
					{ 
						ErrorHandler.ThrowOnFailure(frame.Show());
					}
					catch (Exception ex)
					{
						Diag.Dug(ex);
						throw;
					}
				}
			}
		}

		return VSConstants.S_OK;
	}

	/// <summary>
	/// Close an open document window
	/// </summary>
	/// <param name="closeFlag">Decides how to close the document</param>
	/// <returns>S_OK if successful, otherwise an error is returned</returns>
	public virtual int Close(__FRAMECLOSE closeFlag)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (_Node == null)
		{
			return VSConstants.E_FAIL;
		}
		GetDocInfo(out _, out _, out bool isOpenedByUs, out _, out _);

		if (isOpenedByUs)
		{
			IVsUIShellOpenDocument shell = _Node.ExplorerConnection.Connection.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

			try { Assumes.Present(shell); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			Guid logicalView = Guid.Empty;
			uint grfIDO = 0;
			uint[] itemIdOpen = new uint[1];

			IVsUIHierarchy pHierCaller = null;
			IVsWindowFrame windowFrame;

			try
			{
				_ = ErrorHandler.ThrowOnFailure(shell.IsDocumentOpen(pHierCaller, (uint)Node.ItemId, _Node.FullName,
					ref logicalView, grfIDO, out _, itemIdOpen, out windowFrame, out _));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			if (windowFrame != null)
			{
				return windowFrame.CloseFrame((uint)closeFlag);
			}
		}

		return VSConstants.S_OK;
	}

	/// <summary>
	/// Silently saves an open document
	/// </summary>
	/// <param name="saveIfDirty">Save the open document only if it is dirty</param>
	/// <remarks>The call to SaveDocData may return Microsoft.VisualStudio.Shell.Interop.PFF_RESULTS.STG_S_DATALOSS to indicate some characters could not be represented in the current codepage</remarks>
	public virtual void Save(bool saveIfDirty)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		GetDocInfo(out _, out bool isDirty, out _, out _, out IVsPersistDocData persistDocData);
		if (isDirty && saveIfDirty && persistDocData != null)
		{
			try
			{ 
				_ = ErrorHandler.ThrowOnFailure(persistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_SilentSave, out _, out _));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}
	}

	#endregion

	#region helper methods
	/// <summary>
	/// Get document properties from RDT
	/// </summary>
	internal void GetDocInfo(
		out bool isOpen,     // true if the doc is opened
		out bool isDirty,    // true if the doc is dirty
		out bool isOpenedByUs, // true if opened by our project
		out uint docCookie, // VSDOCCOOKIE if open
		out IVsPersistDocData persistDocData)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		isOpen = isDirty = isOpenedByUs = false;
		docCookie = (uint)ShellConstants.VSDOCCOOKIE_NIL;
		persistDocData = null;

		if (_Node == null || _Node.ExplorerConnection == null
			|| (_Node.ExplorerConnection.Connection.State & DataConnectionState.Open) == 0)
		{
			return;
		}

		IServiceProvider serviceProvider = Node.ExplorerConnection.Connection.GetService(typeof(IServiceProvider)) as IServiceProvider;
		VsShellUtilities.GetRDTDocumentInfo(serviceProvider, _Node.FullName, out IVsHierarchy hierarchy, out _, out persistDocData, out docCookie);

		if (hierarchy == null || docCookie == (uint)ShellConstants.VSDOCCOOKIE_NIL)
		{
			return;
		}

		isOpen = true;
		// check if the doc is opened by another project
		isOpenedByUs = true;

		if (persistDocData != null)
		{
			int isDocDataDirty;
			try
			{ 
				ErrorHandler.ThrowOnFailure(persistDocData.IsDocDataDirty(out isDocDataDirty));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			isDirty = isDocDataDirty != 0;
		}
	}

	protected string GetOwnerCaption()
	{
		Debug.Assert(_Node != null, "No node has been initialized for the document manager");

		object pvar = _Node.FullName;
		// ErrorHandler.ThrowOnFailure(_Node.FullNameGetProperty(_Node.ID, (int)__VSHPROPID.VSHPROPID_Caption, out pvar));

		return pvar as string;
	}

	protected static void CloseWindowFrame(ref IVsWindowFrame windowFrame)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (windowFrame != null)
		{
			try
			{
				ErrorHandler.ThrowOnFailure(windowFrame.CloseFrame(0));
			}
			finally
			{
				windowFrame = null;
			}
		}
	}

	protected string GetFullPathForDocument()
	{
		Debug.Assert(_Node != null, "No node has been initialized for the document manager");

		// Get the URL representing the item
		string fullPath = _Node.FullName;
		Debug.Assert(!string.IsNullOrEmpty(fullPath), "Could not retrive the fullpath for the node" + _Node.ItemId.ToString(CultureInfo.CurrentCulture));
		return fullPath;
	}

	#endregion

	#region static methods
	/// <summary>
	/// Updates the caption for all windows associated to the document.
	/// </summary>
	/// <param name="site">The service provider.</param>
	/// <param name="caption">The new caption.</param>
	/// <param name="docData">The IUnknown interface to a document data object associated with a registered document.</param>
	public static void UpdateCaption(IServiceProvider site, string caption, IntPtr docData)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (site == null)
		{
			throw new ArgumentNullException("site");
		}

		if (string.IsNullOrEmpty(caption))
		{
			throw new ArgumentNullException("ParameterCannotBeNullOrEmpty", "caption");
		}

		IVsUIShell uiShell = site.GetService(typeof(SVsUIShell)) as IVsUIShell;

		try { Assumes.Present(uiShell); }
		catch (Exception ex) { Diag.Dug(ex); throw; }

		// We need to tell the windows to update their captions. 
		ErrorHandler.ThrowOnFailure(uiShell.GetDocumentWindowEnum(out IEnumWindowFrames windowFramesEnum));
		IVsWindowFrame[] windowFrames = new IVsWindowFrame[1];
		while (windowFramesEnum.Next(1, windowFrames, out uint fetched) == VSConstants.S_OK && fetched == 1)
		{
			IVsWindowFrame windowFrame = windowFrames[0];
			ErrorHandler.ThrowOnFailure(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out object data));
			IntPtr ptr = Marshal.GetIUnknownForObject(data);
			try
			{
				if (ptr == docData)
				{
					ErrorHandler.ThrowOnFailure(windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption, caption));
				}
			}
			finally
			{
				if (ptr != IntPtr.Zero)
				{
					Marshal.Release(ptr);
				}
			}
		}
	}

	/// <summary>
	/// Rename document in the running document table from oldName to newName.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	/// <param name="oldName">Full path to the old name of the document.</param>		
	/// <param name="newName">Full path to the new name of the document.</param>		
	/// <param name="newItemId">The new item id of the document</param>		
	public static void RenameDocument(IServiceProvider site, string oldName, string newName, uint newItemId)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (site == null)
		{
			throw new ArgumentNullException("site");
		}

		if (string.IsNullOrEmpty(oldName))
		{
			throw new ArgumentNullException("ParameterCannotBeNullOrEmpty", "oldName");
		}

		if (string.IsNullOrEmpty(newName))
		{
			throw new ArgumentNullException("ParameterCannotBeNullOrEmpty", "newName");
		}

		if (newItemId == VSConstants.VSITEMID_NIL)
		{
			throw new ArgumentNullException("newItemId");
		}

		if (site.GetService(typeof(SVsRunningDocumentTable)) is not IVsRunningDocumentTable pRDT
			|| site.GetService(typeof(SVsUIShellOpenDocument)) is not IVsUIShellOpenDocument) return;

		_ = ErrorHandler.ThrowOnFailure(pRDT.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, oldName, out IVsHierarchy pIVsHierarchy, out _, out IntPtr docData, out _));

		if (docData != IntPtr.Zero)
		{
			try
			{
				IntPtr pUnk = Marshal.GetIUnknownForObject(pIVsHierarchy);
				Guid iid = typeof(IVsHierarchy).GUID;
				Marshal.QueryInterface(pUnk, ref iid, out IntPtr pHier);
				try
				{
					ErrorHandler.ThrowOnFailure(pRDT.RenameDocument(oldName, newName, pHier, newItemId));
				}
				finally
				{
					if (pHier != IntPtr.Zero)
						Marshal.Release(pHier);
					if (pUnk != IntPtr.Zero)
						Marshal.Release(pUnk);
				}
			}
			finally
			{
				Marshal.Release(docData);
			}
		}
	}
	#endregion
}
