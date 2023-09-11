#region Assembly Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Common.Properties;

using EnvDTE;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;





namespace BlackbirdSql.Common.Ctl;


public sealed class RdtManager : IDisposable
{
	private static volatile RdtManager _Instance;

	private static readonly object _LockObject = new object();

	private readonly IVsInvisibleEditorManager _InvisibleEditorManager;

	private readonly IVsRunningDocumentTable _RunningDocumentTable;

	private readonly _DTE _Dte;

	private readonly IVsUIShell _UiShell;

	private readonly Dictionary<uint, int> _DocDataToKeepAliveOnClose = new Dictionary<uint, int>();

	private readonly object _KeepAliveLockObject = new object();

	private readonly RunningDocumentTable _ShellRunningDocumentTable;

	public static RdtManager Instance
	{
		get
		{
			if (_Instance == null)
			{
				lock (_LockObject)
				{
					if (_Instance == null)
					{
						InitializeInstance();
					}
				}
			}

			return _Instance;
		}
	}

	public static void InitializeInstance()
	{
		if (_Instance != null)
		{
			return;
		}

		lock (_LockObject)
		{
			_Instance ??= new RdtManager();
		}
	}

	private RdtManager()
	{
		if (!Application.MessageLoop)
		{
			InvalidOperationException ex = new("Must create EventsManager on the UI Thread");
			Diag.Dug(ex);
			throw ex;
		}

		_ShellRunningDocumentTable = new RunningDocumentTable();
		_InvisibleEditorManager = Package.GetGlobalService(typeof(SVsInvisibleEditorManager)) as IVsInvisibleEditorManager;
		_RunningDocumentTable = Package.GetGlobalService(typeof(IVsRunningDocumentTable)) as IVsRunningDocumentTable;
		_Dte = Package.GetGlobalService(typeof(_DTE)) as _DTE;
		_UiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
		if (_UiShell == null)
		{
			InvalidOperationException ex = new("Could not get _UiShell!");
			Diag.Dug(ex);
			throw ex;
		}
	}

	public void AddKeepDocDataAliveOnCloseReference(uint docCookie)
	{
		lock (_KeepAliveLockObject)
		{
			if (_DocDataToKeepAliveOnClose.TryGetValue(docCookie, out var value))
			{
				_DocDataToKeepAliveOnClose[docCookie] = value + 1;
				return;
			}

			_DocDataToKeepAliveOnClose.Add(docCookie, 1);
			GetRunningDocumentTable().LockDocument((uint)_VSRDTFLAGS.RDT_EditLock, docCookie);
		}
	}

	public void RemoveKeepDocDataAliveOnCloseReference(uint docCookie)
	{
		lock (_KeepAliveLockObject)
		{
			if (_DocDataToKeepAliveOnClose.TryGetValue(docCookie, out var value))
			{
				if (value == 1)
				{
					_DocDataToKeepAliveOnClose.Remove(docCookie);
					GetRunningDocumentTable().UnlockDocument((uint)_VSRDTFLAGS.RDT_EditLock, docCookie);
				}
				else
				{
					_DocDataToKeepAliveOnClose[docCookie] = value - 1;
				}
			}
		}
	}

	public bool ShouldKeepDocDataAliveOnClose(uint docCookie)
	{
		lock (_KeepAliveLockObject)
		{
			return _DocDataToKeepAliveOnClose.ContainsKey(docCookie);
		}
	}

	private static int FindAndLockDocument(uint rdtLockType, string fullPathFileName, out IVsHierarchy ppHier, out uint pitemid, out IntPtr ppunkDocData, out uint pdwCookie)
	{
		int num = 1;
		IVsRunningDocumentTable runningDocumentTable = Instance.GetRunningDocumentTable();
		ppHier = null;
		pitemid = 0u;
		ppunkDocData = IntPtr.Zero;
		pdwCookie = 0u;
		if (runningDocumentTable != null)
		{
			num = runningDocumentTable.FindAndLockDocument(rdtLockType, fullPathFileName, out ppHier, out pitemid, out ppunkDocData, out pdwCookie);
			if (Cmd.Failed(num))
			{
				try
				{
					if (Path.IsPathRooted(fullPathFileName))
					{
						return num;
					}

					fullPathFileName = Path.GetFullPath(fullPathFileName);
					num = runningDocumentTable.FindAndLockDocument(rdtLockType, fullPathFileName, out ppHier, out pitemid, out ppunkDocData, out pdwCookie);
					return num;
				}
				catch (ArgumentException)
				{
					return num;
				}
				catch (IOException)
				{
					return num;
				}
			}
		}

		return num;
	}

	public IVsTextLines GetTextLines(string fullPathFileName)
	{
		IVsTextLines textLines = null;
		if (Instance.GetRunningDocumentTable() != null)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				Native.ThrowOnFailure(FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, fullPathFileName, out var _, out var _, out ppunkDocData, out var pdwCookie));
				if (pdwCookie != 0)
				{
					if (ppunkDocData != IntPtr.Zero)
					{
						try
						{
							textLines = Marshal.GetObjectForIUnknown(ppunkDocData) as IVsTextLines;
							return textLines;
						}
						catch (ArgumentException)
						{
							return textLines;
						}
					}

					return textLines;
				}

				IVsInvisibleEditor spEditor = null;
				try
				{
					TryGetTextLinesAndInvisibleEditor(fullPathFileName, out spEditor, out textLines);
					return textLines;
				}
				finally
				{
					if (spEditor != null)
					{
						Marshal.ReleaseComObject(spEditor);
					}
				}
			}
			finally
			{
				if (ppunkDocData != IntPtr.Zero)
				{
					Marshal.Release(ppunkDocData);
				}
			}
		}

		return textLines;
	}

	public bool TryGetCodeWindow(string documentFullPath, out IVsCodeWindow codeWindow)
	{
		codeWindow = null;
		if (!string.IsNullOrEmpty(documentFullPath))
		{
			IVsWindowFrame windowFrame = Instance.GetWindowFrame(documentFullPath);
			if (windowFrame != null)
			{
				Native.ThrowOnFailure(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar));
				if (pvar != null)
				{
					codeWindow = pvar as IVsCodeWindow;
					return codeWindow != null;
				}
			}
		}

		return false;
	}

	public bool TryGetTextLinesAndInvisibleEditor(string fullPathFileName, out IVsInvisibleEditor spEditor, out IVsTextLines textLines)
	{
		return TryGetTextLinesAndInvisibleEditor(fullPathFileName, null, out spEditor, out textLines);
	}

	public bool TryGetTextLinesAndInvisibleEditor(string fullPathFileName, IVsProject project, out IVsInvisibleEditor spEditor, out IVsTextLines textLines)
	{
		spEditor = null;
		textLines = null;
		IntPtr ppDocData = IntPtr.Zero;
		bool result = false;
		Guid riid = typeof(IVsTextLines).GUID;
		try
		{
			Native.ThrowOnFailure(_InvisibleEditorManager.RegisterInvisibleEditor(fullPathFileName, project, (uint)_EDITORREGFLAGS.RIEF_ENABLECACHING, null, out spEditor));
			if (spEditor != null)
			{
				if (spEditor.GetDocData(0, ref riid, out ppDocData) == 0)
				{
					if (ppDocData != IntPtr.Zero)
					{
						textLines = Marshal.GetTypedObjectForIUnknown(ppDocData, typeof(IVsTextLines)) as IVsTextLines;
						result = true;
						return result;
					}

					return result;
				}

				return result;
			}

			return result;
		}
		catch (Exception ex)
		{
			SqlTracer.TraceException((TraceEventType)356, (EnSqlTraceId)3, ex);
			if (Cmd.IsCriticalException(ex))
			{
				throw;
			}

			return result;
		}
		finally
		{
			if (ppDocData != IntPtr.Zero)
			{
				Marshal.Release(ppDocData);
			}
		}
	}

	public string ReadFromFile(string fullPathFileName)
	{
		IVsInvisibleEditor spEditor = null;
		try
		{
			IVsTextLines textLines;
			if (Instance.IsFileInRdt(fullPathFileName))
			{
				textLines = GetTextLines(fullPathFileName);
			}
			else if (!TryGetTextLinesAndInvisibleEditor(fullPathFileName, out spEditor, out textLines))
			{
				textLines = null;
			}

			return GetAllTextFromTextLines(textLines);
		}
		finally
		{
			if (spEditor != null)
			{
				Marshal.ReleaseComObject(spEditor);
			}
		}
	}

	public string GetAllTextFromTextLines(IVsTextLines textLines)
	{
		string pbstrBuf = null;
		if (textLines != null && textLines.GetLastLineIndex(out var piLine, out var piIndex) == 0 && textLines.GetLineText(0, 0, piLine, piIndex, out pbstrBuf) != 0)
		{
			pbstrBuf = null;
		}

		return pbstrBuf;
	}

	public void ResetMkDocument(string oldMkDoc, string newMkDoc, IVsUIHierarchy hierarchy, uint newNodeId)
	{
		Cmd.CheckForNullReference((object)hierarchy, "hierarchy");
		Cmd.CheckForEmptyString(oldMkDoc, "oldMkDoc");
		Cmd.CheckForEmptyString(newMkDoc, "newMkDoc");
		IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(hierarchy);
		IntPtr ppv;
		try
		{
			Guid iid = typeof(IVsHierarchy).GUID;
			Marshal.QueryInterface(iUnknownForObject, ref iid, out ppv);
		}
		finally
		{
			if (iUnknownForObject != IntPtr.Zero)
			{
				Marshal.Release(iUnknownForObject);
			}
		}

		IVsRunningDocumentTable runningDocumentTable = Instance.GetRunningDocumentTable();
		/*
		if (Cmd.Failed(runningDocumentTable.GetRunningDocumentsEnum(out var ppenum)))
		{
			ppenum = null;
		}
		*/
		Native.ThrowOnFailure(runningDocumentTable.RenameDocument(oldMkDoc, newMkDoc, ppv, newNodeId));
	}

	public void GetMkDocument(IVsWindowFrame frame, out string mkDoc)
	{
		mkDoc = null;
		if (frame == null)
		{
			InvalidOperationException ex = new("frame argument cannot be null.");
			Diag.Dug(ex);
			throw ex;
		}

		Native.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out var pvar));
		if (pvar != null)
		{
			mkDoc = pvar as string;
		}
	}

	public void SaveDirtyFile(string fullFilePath)
	{
		if (!string.IsNullOrEmpty(fullFilePath))
		{
			IList<string> dirtyFiles = new List<string> { fullFilePath };
			SaveDirtyFiles(dirtyFiles);
		}
	}

	public void SaveDirtyFiles(IList<string> dirtyFiles)
	{
		Cmd.CheckForNullReference((object)dirtyFiles, "dirtyFiles");
		IVsRunningDocumentTable runningDocumentTable = _RunningDocumentTable;
		int count = dirtyFiles.Count;
		for (int i = 0; i < count; i++)
		{
			string text = dirtyFiles[i];
			if (Instance.GetDocData(text) is IVsPersistDocData vsPersistDocData)
			{
				uint rdtCookie = Instance.GetRdtCookie(text);
				Native.ThrowOnFailure(runningDocumentTable.NotifyOnBeforeSave(rdtCookie));
				if (vsPersistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_Save, out var _, out var pfSaveCanceled) != 0 || pfSaveCanceled != 0)
				{
					InvalidOperationException ex = new(string.Format(CultureInfo.CurrentCulture, ControlsResources.Exception_FailedToSaveFile, text));
					Diag.Dug(ex);
					throw ex;
				}

				Native.ThrowOnFailure(runningDocumentTable.NotifyOnAfterSave(rdtCookie));
			}
		}
	}

	public void SaveDirtyFiles(Predicate<string> shouldSave)
	{
		Cmd.CheckForNullReference((object)shouldSave, "shouldSave");
		SaveDirtyFiles(GetDirtyFiles(shouldSave));
	}

	public List<string> GetDirtyFiles(Predicate<string> shouldHandle)
	{
		Cmd.CheckForNullReference((object)shouldHandle, "shouldHandle");
		List<string> dirtyFiles = new List<string>();
		if (Instance.GetRunningDocumentTable() != null)
		{
			IVsUIShell uiShell = _UiShell;
			ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				Native.ThrowOnFailure(uiShell.GetDocumentWindowEnum(out var ppenum));
				IVsWindowFrame[] array = new IVsWindowFrame[1];
				while (ppenum.Next(1u, array, out uint pceltFetched) == 0 && pceltFetched == 1)
				{
					Native.ThrowOnFailure(array[0].GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var pvar));
					if (pvar is IPersistFileFormat persistFileFormat)
					{
						int num = persistFileFormat.IsDirty(out int pfIsDirty);
						if (num != VSConstants.E_NOTIMPL)
						{
							Native.ThrowOnFailure(num);
							if (pfIsDirty == 1)
							{
								Native.ThrowOnFailure(persistFileFormat.GetCurFile(out var ppszFilename, out var _));
								if (!string.IsNullOrEmpty(ppszFilename) && shouldHandle(ppszFilename))
								{
									dirtyFiles.Add(ppszFilename);
								}
							}
						}
					}
				}
			});
		}

		return dirtyFiles;
	}

	public IVsRunningDocumentTable GetRunningDocumentTable()
	{
		return _RunningDocumentTable;
	}

	public IVsRunningDocumentTable2 GetRunningDocumentTable2()
	{
		return _RunningDocumentTable as IVsRunningDocumentTable2;
	}

	public string GetActiveDocument()
	{
		_DTE dte = _Dte;
		if (dte != null)
		{
			Document activeDocument = dte.ActiveDocument;
			if (activeDocument != null)
			{
				return activeDocument.FullName;
			}
		}

		return string.Empty;
	}

	public void SetFocusToActiveDocument()
	{
		_Dte?.ActiveDocument?.Activate();
	}

	public bool TryGetHierarchyFromName(string fileName, out IVsHierarchy hierarchy, out uint itemId, out uint docCookie)
	{
		hierarchy = null;
		itemId = 0u;
		docCookie = 0u;
		if (Instance.GetRunningDocumentTable() != null)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				if (Native.Succeeded(FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, fileName, out hierarchy, out itemId, out ppunkDocData, out docCookie)))
				{
					return true;
				}
			}
			finally
			{
				if (ppunkDocData != IntPtr.Zero)
				{
					Marshal.Release(ppunkDocData);
				}
			}
		}

		return false;
	}

	public IVsHierarchy GetHierarchyFromDocCookie(uint docCookie)
	{
		IntPtr ppunkDocData = IntPtr.Zero;
		try
		{
			Native.ThrowOnFailure(_RunningDocumentTable.GetDocumentInfo(docCookie, out var _, out var _, out var _, out var _, out var ppHier, out var _, out ppunkDocData));
			return ppHier;
		}
		finally
		{
			if (ppunkDocData != IntPtr.Zero)
			{
				Marshal.Release(ppunkDocData);
			}
		}
	}

	public bool TryGetDocDataFromCookie(uint cookie, out object docData)
	{
		docData = null;
		IVsRunningDocumentTable runningDocumentTable = Instance.GetRunningDocumentTable();
		if (runningDocumentTable != null && Native.Succeeded(runningDocumentTable.GetDocumentInfo(cookie, out var _, out var _, out var _, out var _, out var _, out var _, out var ppunkDocData)))
		{
			docData = Marshal.GetObjectForIUnknown(ppunkDocData);
			Marshal.Release(ppunkDocData);
			return true;
		}

		return false;
	}

	public bool TrySetDocDataDirty(string fileName, bool dirty)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(fileName))
		{
			IVsRunningDocumentTable runningDocumentTable = Instance.GetRunningDocumentTable();
			if (runningDocumentTable != null)
			{
				IntPtr ppunkDocData = IntPtr.Zero;
				try
				{
					if (!Native.Succeeded(FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, fileName, out var ppHier, out var pitemid, out ppunkDocData, out var pdwCookie)))
					{
						SqlTracer.TraceEvent(TraceEventType.Warning, (EnSqlTraceId)0, "Failed to find document " + fileName);
						return false;
					}

					if (!Native.Succeeded(runningDocumentTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, pdwCookie)))
					{
						SqlTracer.TraceEvent(TraceEventType.Warning, (EnSqlTraceId)0, "Failed to unlock document " + fileName);
						return false;
					}

					if (!Native.Succeeded(runningDocumentTable.GetDocumentInfo(pdwCookie, out var _, out var _, out var _, out var _, out ppHier, out pitemid, out var ppunkDocData2)))
					{
						SqlTracer.TraceEvent(TraceEventType.Warning, (EnSqlTraceId)0, "Failed to get document info " + fileName);
						return false;
					}

					if (ppunkDocData2 != IntPtr.Zero)
					{
						try
						{
							if (Marshal.GetObjectForIUnknown(ppunkDocData2) is IVsPersistDocData2 vsPersistDocData)
							{
								if (Native.Succeeded(vsPersistDocData.SetDocDataDirty(dirty ? 1 : 0)))
								{
									return true;
								}

								SqlTracer.TraceEvent(TraceEventType.Warning, (EnSqlTraceId)0, "Failed to set docData dirty " + fileName);
								return false;
							}

							return result;
						}
						finally
						{
							Marshal.Release(ppunkDocData2);
						}
					}

					return result;
				}
				finally
				{
					if (ppunkDocData != IntPtr.Zero)
					{
						Marshal.Release(ppunkDocData);
					}
				}
			}
		}

		return result;
	}

	public bool IsDirty(string docFullPath)
	{
		if (Instance.GetDocData(docFullPath) is IVsPersistDocData vsPersistDocData)
		{
			Native.ThrowOnFailure(vsPersistDocData.IsDocDataDirty(out var pfDirty));
			return pfDirty != 0;
		}

		return false;
	}

	public bool IsDirty(uint docCookie)
	{
		if (TryGetDocDataFromCookie(docCookie, out var docData))
		{
			return IsDirty(docData);
		}

		return false;
	}

	public bool IsDirty(object docData)
	{
		if (docData is IVsPersistDocData vsPersistDocData)
		{
			Native.ThrowOnFailure(vsPersistDocData.IsDocDataDirty(out var pfDirty));
			return pfDirty != 0;
		}

		return false;
	}

	public int NotifyDocChanged(string fileName)
	{
		int result = 0;
		if (string.IsNullOrEmpty(fileName))
		{
			ArgumentNullException ex = new("fileName");
			Diag.Dug(ex);
			throw ex;
		}

		IVsRunningDocumentTable runningDocumentTable = Instance.GetRunningDocumentTable();
		if (runningDocumentTable != null)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				result = FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, fileName, out var _, out var _, out ppunkDocData, out var pdwCookie);
				if (result == 1)
				{
					result = VSConstants.E_FAIL;
				}

				if (Native.Succeeded(result))
				{
					try
					{
						return runningDocumentTable.NotifyDocumentChanged(pdwCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);
					}
					finally
					{
						Native.ThrowOnFailure(runningDocumentTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, pdwCookie));
					}
				}

				return result;
			}
			finally
			{
				if (ppunkDocData != IntPtr.Zero)
				{
					Marshal.Release(ppunkDocData);
				}
			}
		}

		return result;
	}

	public int SetDocumentSaveNotSupported(string fileName, bool isNotSupported)
	{
		int result = VSConstants.E_FAIL;
		if (!string.IsNullOrEmpty(fileName))
		{
			IVsRunningDocumentTable runningDocumentTable = Instance.GetRunningDocumentTable();
			if (runningDocumentTable != null)
			{
				IntPtr ppunkDocData = IntPtr.Zero;
				try
				{
					result = FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, fileName, out var _, out var _, out ppunkDocData, out var pdwCookie);
					if (Native.Succeeded(result))
					{
						try
						{
							return runningDocumentTable.ModifyDocumentFlags(pdwCookie, (uint)_VSRDTFLAGS.RDT_CantSave, isNotSupported ? 1 : 0);
						}
						finally
						{
							Native.ThrowOnFailure(runningDocumentTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, pdwCookie));
						}
					}

					return result;
				}
				finally
				{
					if (ppunkDocData != IntPtr.Zero)
					{
						Marshal.Release(ppunkDocData);
					}
				}
			}
		}

		return result;
	}

	public bool CloseOpenDocument(string fileName)
	{
		Instance.CloseFrame(fileName, out var _);
		return Instance.GetRdtCookie(fileName) == 0;
	}

	public void CloseFrame(string fullFileName, out int foundAndClosed)
	{
		foundAndClosed = 0;
		if (string.IsNullOrEmpty(fullFileName))
		{
			return;
		}

		IVsRunningDocumentTable2 runningDocumentTable = GetRunningDocumentTable2();
		if (runningDocumentTable != null)
		{
			int num = runningDocumentTable.QueryCloseRunningDocument(fullFileName, out foundAndClosed);
			if (num != VSConstants.OLE_E_PROMPTSAVECANCELLED)
			{
				Native.WrapComCall(num, Array.Empty<int>());
			}
		}
	}

	public bool ShowFrame(string fullFileName)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(fullFileName))
		{
			IVsWindowFrame windowFrame = Instance.GetWindowFrame(fullFileName);
			if (windowFrame != null)
			{
				Native.ThrowOnFailure(windowFrame.Show());
				result = true;
			}
		}

		return result;
	}

	public object GetWindowDocView(string fullFileName)
	{
		if (string.IsNullOrEmpty(fullFileName))
		{
			return null;
		}

		return GetWindowDocView(GetWindowFrame(fullFileName));
	}

	public object GetWindowDocView(IVsWindowFrame frame)
	{
		if (frame == null)
		{
			return null;
		}

		int property = frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar);
		if (property != 0)
		{
			pvar = null;
			SqlTracer.TraceEvent(TraceEventType.Warning, (EnSqlTraceId)3, "Could not retrieve docView from frame " + property);
		}

		return pvar;
	}

	public IVsWindowFrame GetWindowFrame(string fullFileName)
	{
		if (string.IsNullOrEmpty(fullFileName))
		{
			return null;
		}

		IVsWindowFrame result = null;
		if (Instance.GetRunningDocumentTable() != null)
		{
			Native.ThrowOnFailure(_UiShell.GetDocumentWindowEnum(out var ppenum));
			IVsWindowFrame[] array = new IVsWindowFrame[1];
			while (ppenum.Next(1u, array, out uint pceltFetched) == 0 && pceltFetched == 1)
			{
				IVsWindowFrame vsWindowFrame = array[0];
				Native.ThrowOnFailure(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var pvar));
				if (pvar is IPersistFileFormat persistFileFormat)
				{
					Native.ThrowOnFailure(persistFileFormat.GetCurFile(out var ppszFilename, out var _));
					if (!string.IsNullOrEmpty(ppszFilename) && (string.Compare(ppszFilename, fullFileName, ignoreCase: true, CultureInfo.CurrentCulture) == 0 || Cmd.IsSamePath(ppszFilename, fullFileName)))
					{
						result = vsWindowFrame;
						break;
					}
				}
			}
		}

		return result;
	}

	public uint GetRdtCookie(string fullPathFileName)
	{
		uint pdwCookie = 0u;
		if (Instance.GetRunningDocumentTable() != null)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				try
				{
					if (Path.IsPathRooted(fullPathFileName))
					{
						fullPathFileName = Path.GetFullPath(fullPathFileName);
					}
				}
				catch (ArgumentException)
				{
				}

				FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, fullPathFileName, out var _, out var _, out ppunkDocData, out pdwCookie);
				return pdwCookie;
			}
			finally
			{
				if (ppunkDocData != IntPtr.Zero)
				{
					Marshal.Release(ppunkDocData);
				}
			}
		}

		return pdwCookie;
	}

	public object GetDocData(string fullPathFileName)
	{
		IntPtr ppunkDocData = IntPtr.Zero;
		object result = null;
		if (Instance.GetRunningDocumentTable() != null)
		{
			try
			{
				try
				{
					if (Path.IsPathRooted(fullPathFileName))
					{
						fullPathFileName = Path.GetFullPath(fullPathFileName);
					}
				}
				catch (ArgumentException)
				{
				}

				FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, fullPathFileName, out var _, out var _, out ppunkDocData, out var _);
			}
			finally
			{
				if (ppunkDocData != IntPtr.Zero)
				{
					result = Marshal.GetObjectForIUnknown(ppunkDocData);
					Marshal.Release(ppunkDocData);
				}
			}
		}

		return result;
	}

	public bool IsFileInRdt(string fullPathFileName)
	{
		if (!string.IsNullOrEmpty(fullPathFileName))
		{
			return Instance.GetRdtCookie(fullPathFileName) != 0;
		}

		return false;
	}

	public bool TryGetFileNameFromCookie(uint docCookie, out string fileName)
	{
		fileName = string.Empty;
		IVsRunningDocumentTable runningDocumentTable = Instance.GetRunningDocumentTable();
		if (runningDocumentTable != null)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				if (runningDocumentTable.GetDocumentInfo(docCookie, out var _, out var _, out var _, out var pbstrMkDocument, out var _, out var _, out ppunkDocData) == 0)
				{
					fileName = pbstrMkDocument;
				}
			}
			finally
			{
				if (ppunkDocData != IntPtr.Zero)
				{
					Marshal.Release(ppunkDocData);
				}
			}
		}

		return !string.IsNullOrEmpty(fileName);
	}

	public bool WriteToFile(string fullPathFileName, string content)
	{
		return WriteToFile(fullPathFileName, content, saveFile: false);
	}

	public bool WriteToFile(string fullPathFileName, string content, bool saveFile)
	{
		return WriteToFile(fullPathFileName, content, saveFile, createIfNotExist: false);
	}

	public bool WriteToFile(string fullPathFileName, string content, bool saveFile, bool createIfNotExist)
	{
		bool result = true;
		IVsInvisibleEditor spEditor = null;
		IVsTextLines textLines = null;
		bool flag = false;
		try
		{
			if (Instance.IsFileInRdt(fullPathFileName))
			{
				textLines = GetTextLines(fullPathFileName);
			}
			else
			{
				if (createIfNotExist && !File.Exists(fullPathFileName))
				{
					flag = true;
					FileStream fileStream = null;
					StreamWriter streamWriter = null;
					try
					{
						fileStream = File.Create(fullPathFileName);
						streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
						streamWriter.Write(content);
					}
					catch (IOException ex1)
					{
						throw new InvalidOperationException(ex1.Message);
					}
					catch (ArgumentException ex2)
					{
						throw new InvalidOperationException(ex2.Message);
					}
					finally
					{
						streamWriter?.Close();
						fileStream?.Close();
					}
				}

				if (!flag && !TryGetTextLinesAndInvisibleEditor(fullPathFileName, out spEditor, out textLines))
				{
					textLines = null;
				}
			}

			if (!flag)
			{
				if (textLines != null)
				{
					if (textLines.GetLastLineIndex(out var piLine, out var piIndex) == 0)
					{
						IntPtr intPtr = IntPtr.Zero;
						try
						{
							intPtr = Marshal.StringToHGlobalAuto(content);
							if (textLines.ReloadLines(0, 0, piLine, piIndex, intPtr, content.Length, new TextSpan[1]) != 0)
							{
								result = false;
							}
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}

						if (saveFile)
						{
							List<string> dirtyFiles = new List<string> { fullPathFileName };
							Instance.SaveDirtyFiles(dirtyFiles);
							return result;
						}

						return result;
					}

					return false;
				}

				return false;
			}

			return result;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			if (spEditor != null)
			{
				Marshal.ReleaseComObject(spEditor);
			}
		}
	}

	public bool IsDocumentInitialized(uint docCookie)
	{
		return _ShellRunningDocumentTable.GetDocumentInfo(docCookie).IsDocumentInitialized;
	}

	public void Dispose()
	{
		lock (_KeepAliveLockObject)
		{
			SqlTracer.AssertTraceEvent(_DocDataToKeepAliveOnClose.Keys.Count == 0, TraceEventType.Error, (EnSqlTraceId)3, "EventsManager is still trying to keep doc data alive on dispose, this could be a symptom of memory leak from invisible doc data.");
		}
	}
}
