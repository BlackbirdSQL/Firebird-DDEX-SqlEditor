// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Common.AbstractRdtManager

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Properties;

using EnvDTE;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Core.Ctl;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification="Using Diag.ThrowIfNotOnUIThread()")]
public abstract class AbstractRdtManager : IDisposable
{
	protected static volatile AbstractRdtManager _Instance = null;

	// A static class lock
	protected static readonly object _LockGlobal = new object();

	private IVsInvisibleEditorManager _InvisibleEditorManager = null;


	private _DTE _Dte = null;

	private IVsUIShell _UiShell = null;

	private readonly Dictionary<uint, int> _KeepAliveDocCookies = [];

	private readonly object _KeepAliveLockLocal = new object();

	private RunningDocumentTable _Rdt = null;
	private IVsRunningDocumentTable _RdtSvc = null;



	/// <summary>
	/// Gets or creates the instance of the RdtManager for this session.
	/// </summary>



	protected RunningDocumentTable Rdt => _Rdt
		??= new RunningDocumentTable((IServiceProvider)Controller.DdexPackage);


	protected IVsRunningDocumentTable RdtSvc => _RdtSvc
		??= Package.GetGlobalService(typeof(IVsRunningDocumentTable)) as IVsRunningDocumentTable;


	protected IVsRunningDocumentTable2 RdtSvc2 => RdtSvc as IVsRunningDocumentTable2;


	protected IVsRunningDocumentTable3 RdtSvc3 => RdtSvc as IVsRunningDocumentTable3;

	protected IVsRunningDocumentTable4 RdtSvc4 => RdtSvc as IVsRunningDocumentTable4;

	protected IVsInvisibleEditorManager InvisibleEditorManager => _InvisibleEditorManager ??=
		Package.GetGlobalService(typeof(SVsInvisibleEditorManager)) as IVsInvisibleEditorManager;

	protected _DTE Dte => _Dte ??= Package.GetGlobalService(typeof(_DTE)) as _DTE;

	protected IVsUIShell UiShell => _UiShell ??= Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;



	protected AbstractRdtManager()
	{
		if (!Application.MessageLoop)
		{
			InvalidOperationException ex = new("Must create Events Manager on the UI Thread");
			Diag.Dug(ex);
			throw ex;
		}
		Diag.ThrowIfNotOnUIThread();

	}


	protected void AddKeepAlive(uint docCookie)
	{
		lock (_KeepAliveLockLocal)
		{
			if (_KeepAliveDocCookies.TryGetValue(docCookie, out var value))
			{
				_KeepAliveDocCookies[docCookie] = value + 1;
				return;
			}

			Diag.ThrowIfNotOnUIThread();

			_KeepAliveDocCookies.Add(docCookie, 1);
			RdtSvc.LockDocument((uint)_VSRDTFLAGS.RDT_EditLock, docCookie);
		}
	}

	protected static int Exf(int hr, string context = null) => Native.ThrowOnFailure(hr, context);

	protected void RemoveKeepAlive(uint docCookie)
	{
		lock (_KeepAliveLockLocal)
		{
			if (_KeepAliveDocCookies.TryGetValue(docCookie, out var value))
			{
				if (value == 1)
				{
					Diag.ThrowIfNotOnUIThread();

					_KeepAliveDocCookies.Remove(docCookie);
					RdtSvc.UnlockDocument((uint)_VSRDTFLAGS.RDT_EditLock, docCookie);
				}
				else
				{
					_KeepAliveDocCookies[docCookie] = value - 1;
				}
			}
		}
	}

	protected int FindAndLockDocumentImpl(uint rdtLockType, string mkDocument, out IVsHierarchy ppHier, out uint pitemid, out IntPtr ppunkDocData, out uint pdwCookie)
	{
		int hresult = VSConstants.S_FALSE;
		ppHier = null;
		pitemid = 0u;
		ppunkDocData = IntPtr.Zero;
		pdwCookie = 0u;

		if (RdtSvc == null)
			return hresult;

		Diag.ThrowIfNotOnUIThread();

		return RdtSvc.FindAndLockDocument(rdtLockType, mkDocument, out ppHier, out pitemid, out ppunkDocData, out pdwCookie);

		// We're not going to try for !Path.IsPathRooted(mkDocument) because it already will be.
	}



	protected IVsTextLines GetTextLines(string mkDocument)
	{
		IVsTextLines textLines = null;
		if (RdtSvc != null)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				Exf(FindAndLockDocumentImpl((uint)_VSRDTFLAGS.RDT_NoLock, mkDocument, out var _, out var _, out ppunkDocData, out var pdwCookie));

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
					TryGetTextLinesAndInvisibleEditor(mkDocument, out spEditor, out textLines);
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



	protected bool TryGetTextLinesAndInvisibleEditor(string mkDocument, out IVsInvisibleEditor spEditor, out IVsTextLines textLines)
	{
		return TryGetTextLinesAndInvisibleEditor(mkDocument, null, out spEditor, out textLines);
	}

	protected bool TryGetTextLinesAndInvisibleEditor(string mkDocument, IVsProject project, out IVsInvisibleEditor spEditor, out IVsTextLines textLines)
	{
		spEditor = null;
		textLines = null;
		IntPtr ppDocData = IntPtr.Zero;
		bool result = false;
		Guid riid = typeof(IVsTextLines).GUID;
		try
		{
			Diag.ThrowIfNotOnUIThread();

			Exf(_InvisibleEditorManager.RegisterInvisibleEditor(mkDocument, project, (uint)_EDITORREGFLAGS.RIEF_ENABLECACHING, null, out spEditor));
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
			Diag.Dug(ex);

			if (Cmd.IsCriticalException(ex))
				throw;

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

	protected string ReadFromFile(string mkDocument)
	{
		IVsInvisibleEditor spEditor = null;
		try
		{
			IVsTextLines textLines;
			if (IsFileInRdtImpl(mkDocument))
			{
				textLines = GetTextLines(mkDocument);
			}
			else if (!TryGetTextLinesAndInvisibleEditor(mkDocument, out spEditor, out textLines))
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

	protected static string GetAllTextFromTextLines(IVsTextLines textLines)
	{
		string pbstrBuf = null;
		if (textLines != null && textLines.GetLastLineIndex(out var piLine, out var piIndex) == 0 && textLines.GetLineText(0, 0, piLine, piIndex, out pbstrBuf) != 0)
		{
			pbstrBuf = null;
		}

		return pbstrBuf;
	}

	protected void ResetMkDocument(string oldMkDoc, string newMkDoc, IVsUIHierarchy hierarchy, uint newNodeId)
	{
		Cmd.CheckForNullReference((object)hierarchy, "hierarchy");
		Cmd.CheckForEmptyString(oldMkDoc, "oldMkDoc");
		Cmd.CheckForEmptyString(newMkDoc, "newMkDoc");
		IntPtr intPtrUnknown = Marshal.GetIUnknownForObject(hierarchy);
		IntPtr ppv;
		try
		{
			Guid iid = typeof(IVsHierarchy).GUID;
			Marshal.QueryInterface(intPtrUnknown, ref iid, out ppv);
		}
		finally
		{
			if (intPtrUnknown != IntPtr.Zero)
			{
				Marshal.Release(intPtrUnknown);
			}
		}

		/*
		if (Cmd.Failed(runningDocumentTable.GetRunningDocumentsEnum(out var ppenum)))
		{
			ppenum = null;
		}
		*/
		Diag.ThrowIfNotOnUIThread();

		Exf(RdtSvc.RenameDocument(oldMkDoc, newMkDoc, ppv, newNodeId));
	}



	protected static void GetMkDocument(IVsWindowFrame frame, out string mkDoc)
	{
		mkDoc = null;
		if (frame == null)
		{
			InvalidOperationException ex = new("frame argument cannot be null.");
			Diag.Dug(ex);
			throw ex;
		}

		Diag.ThrowIfNotOnUIThread();

		Exf(frame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out var pvar));
		if (pvar != null)
		{
			mkDoc = pvar as string;
		}
	}

	protected void SaveDirtyFile(string fullFilePath)
	{
		if (!string.IsNullOrEmpty(fullFilePath))
		{
			IList<string> dirtyFiles = new List<string> { fullFilePath };
			SaveDirtyFiles(dirtyFiles);
		}
	}

	protected void SaveDirtyFiles(IList<string> dirtyFiles)
	{
		Cmd.CheckForNullReference((object)dirtyFiles, "dirtyFiles");

		Diag.ThrowIfNotOnUIThread();

		int count = dirtyFiles.Count;
		for (int i = 0; i < count; i++)
		{
			string mkDocument = dirtyFiles[i];
			if (GetDocData(mkDocument) is IVsPersistDocData vsPersistDocData)
			{
				uint rdtCookie = GetRdtCookieImpl(mkDocument);
				Exf(RdtSvc.NotifyOnBeforeSave(rdtCookie));
				if (vsPersistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_Save, out var _, out var pfSaveCanceled) != 0 || pfSaveCanceled != 0)
				{
					InvalidOperationException ex = new(string.Format(CultureInfo.CurrentCulture, Resources.Exception_FailedToSaveFile, mkDocument));
					Diag.Dug(ex);
					throw ex;
				}

				Exf(RdtSvc.NotifyOnAfterSave(rdtCookie));
			}
		}
	}

	protected void SaveDirtyFiles(Predicate<string> shouldSave)
	{
		Cmd.CheckForNullReference((object)shouldSave, "shouldSave");
		SaveDirtyFiles(GetDirtyFiles(shouldSave));
	}

	protected List<string> GetDirtyFiles(Predicate<string> shouldHandle)
	{
		Cmd.CheckForNullReference((object)shouldHandle, "shouldHandle");
		List<string> dirtyFiles = [];

		if (RdtSvc == null)
			return dirtyFiles;

		// Fire and wait.

		ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				Exf(UiShell.GetDocumentWindowEnum(out var ppenum));
				IVsWindowFrame[] array = new IVsWindowFrame[1];
				while (ppenum.Next(1u, array, out uint pceltFetched) == 0 && pceltFetched == 1)
				{
					Exf(array[0].GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var pvar));
					if (pvar is IPersistFileFormat persistFileFormat)
					{
						int num = persistFileFormat.IsDirty(out int pfIsDirty);
						if (num != VSConstants.E_NOTIMPL)
						{
							Exf(num);
							if (pfIsDirty == 1)
							{
								Exf(persistFileFormat.GetCurFile(out var ppszFilename, out var _));

								if (!string.IsNullOrEmpty(ppszFilename) && shouldHandle(ppszFilename))
								{
									dirtyFiles.Add(ppszFilename);
								}
							}
						}
					}
				}
			});

		return dirtyFiles;
	}



	protected string GetActiveDocument()
	{
		_DTE dte = _Dte;
		if (dte != null)
		{
			Diag.ThrowIfNotOnUIThread();

			Document activeDocument = dte.ActiveDocument;
			if (activeDocument != null)
			{
				return activeDocument.FullName;
			}
		}

		return string.Empty;
	}

	protected void SetFocusToActiveDocument()
	{
		Diag.ThrowIfNotOnUIThread();

		_Dte?.ActiveDocument?.Activate();
	}

	protected bool TryGetHierarchyFromName(string fileName, out IVsHierarchy hierarchy, out uint itemId, out uint docCookie)
	{
		hierarchy = null;
		itemId = 0u;
		docCookie = 0u;
		if (RdtSvc != null)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				if (Native.Succeeded(FindAndLockDocumentImpl((uint)_VSRDTFLAGS.RDT_NoLock, fileName, out hierarchy, out itemId, out ppunkDocData, out docCookie)))
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

	protected IVsHierarchy GetHierarchyFromDocCookie(uint docCookie)
	{
		Diag.ThrowIfNotOnUIThread();

		IntPtr ppunkDocData = IntPtr.Zero;
		try
		{
			Exf(RdtSvc.GetDocumentInfo(docCookie, out var _, out var _, out var _, out var _, out var ppHier, out var _, out ppunkDocData));
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


	protected bool TryGetDocDataFromCookieImpl(uint cookie, out object docData)
	{
		Diag.ThrowIfNotOnUIThread();

		docData = null;

		if (RdtSvc != null && Native.Succeeded(RdtSvc.GetDocumentInfo(cookie, out var _, out var _, out var _, out var _, out var _, out var _, out var ppunkDocData)))
		{
			docData = Marshal.GetObjectForIUnknown(ppunkDocData);
			Marshal.Release(ppunkDocData);
			return true;
		}

		return false;
	}



	protected bool TrySetDocDataDirty(string fileName, bool dirty)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(fileName))
		{
			if (RdtSvc != null)
			{
				IntPtr ppunkDocData = IntPtr.Zero;
				try
				{
					if (!Native.Succeeded(FindAndLockDocumentImpl((uint)_VSRDTFLAGS.RDT_ReadLock, fileName, out var ppHier, out var pitemid, out ppunkDocData, out var pdwCookie)))
					{
						Tracer.Warning(GetType(), "TrySetDocDataDirty()", "Failed to find document {0}.", fileName);
						return false;
					}

					Diag.ThrowIfNotOnUIThread();

					if (!Native.Succeeded(RdtSvc.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, pdwCookie)))
					{
						Tracer.Warning(GetType(), "TrySetDocDataDirty()", "Failed to unlock document {0}.", fileName);
						return false;
					}

					if (!Native.Succeeded(RdtSvc.GetDocumentInfo(pdwCookie, out var _, out var _, out var _, out var _, out ppHier, out pitemid, out var ppunkDocData2)))
					{
						Tracer.Warning(GetType(), "TrySetDocDataDirty()", "Failed to get document info {0}.", fileName);
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

								Tracer.Warning(GetType(), "TrySetDocDataDirty()", "Failed to set docData dirty {0}.", fileName);
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

	protected bool IsDirty(string docFullPath)
	{
		Diag.ThrowIfNotOnUIThread();

		if (GetDocData(docFullPath) is IVsPersistDocData vsPersistDocData)
		{
			Exf(vsPersistDocData.IsDocDataDirty(out var pfDirty));
			return pfDirty != 0;
		}

		return false;
	}

	protected bool IsDirty(uint docCookie)
	{
		if (TryGetDocDataFromCookieImpl(docCookie, out var docData))
		{
			return IsDirty(docData);
		}

		return false;
	}

	protected bool IsDirty(object docData)
	{
		Diag.ThrowIfNotOnUIThread();

		if (docData is IVsPersistDocData vsPersistDocData)
		{
			Exf(vsPersistDocData.IsDocDataDirty(out var pfDirty));
			return pfDirty != 0;
		}

		return false;
	}

	protected int NotifyDocChanged(string fileName)
	{
		int result = 0;
		if (string.IsNullOrEmpty(fileName))
		{
			ArgumentNullException ex = new("fileName");
			Diag.Dug(ex);
			throw ex;
		}

		if (RdtSvc != null)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				result = FindAndLockDocumentImpl((uint)_VSRDTFLAGS.RDT_ReadLock, fileName, out var _, out var _, out ppunkDocData, out var pdwCookie);

				if (result == VSConstants.S_FALSE)
					result = VSConstants.E_FAIL;


				if (Native.Succeeded(result))
				{
					Diag.ThrowIfNotOnUIThread();

					try
					{
						return RdtSvc.NotifyDocumentChanged(pdwCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);
					}
					finally
					{
						Exf(RdtSvc.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, pdwCookie));
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

	protected int SetDocumentSaveNotSupported(string fileName, bool isNotSupported)
	{
		int result = VSConstants.E_FAIL;
		if (!string.IsNullOrEmpty(fileName))
		{
			if (RdtSvc != null)
			{
				IntPtr ppunkDocData = IntPtr.Zero;
				try
				{
					result = FindAndLockDocumentImpl((uint)_VSRDTFLAGS.RDT_ReadLock, fileName, out var _, out var _, out ppunkDocData, out var pdwCookie);
					if (Native.Succeeded(result))
					{
						Diag.ThrowIfNotOnUIThread();

						try
						{
							return RdtSvc.ModifyDocumentFlags(pdwCookie, (uint)_VSRDTFLAGS.RDT_CantSave, isNotSupported ? 1 : 0);
						}
						finally
						{
							Exf(RdtSvc.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, pdwCookie));
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

	protected bool CloseOpenDocument(string fileName)
	{
		CloseFrame(fileName, out var _);
		return GetRdtCookieImpl(fileName) == 0;
	}

	protected void CloseFrame(string mkDocument, out int foundAndClosed)
	{
		foundAndClosed = 0;
		if (string.IsNullOrEmpty(mkDocument))
		{
			return;
		}

		if (RdtSvc2 != null)
		{
			Diag.ThrowIfNotOnUIThread();

			int num = RdtSvc2.QueryCloseRunningDocument(mkDocument, out foundAndClosed);
			if (num != VSConstants.OLE_E_PROMPTSAVECANCELLED)
			{
				Native.WrapComCall(num, []);
			}
		}
	}


	protected bool ShouldKeepDocDataAliveOnCloseImpl(uint docCookie)
	{
		lock (_KeepAliveLockLocal)
			return _KeepAliveDocCookies.ContainsKey(docCookie);
	}



	protected bool ShowFrame(string mkDocument)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(mkDocument))
		{
			IVsWindowFrame windowFrame = GetWindowFrameImpl(mkDocument);
			if (windowFrame != null)
			{
				Diag.ThrowIfNotOnUIThread();

				result = true;
			}
		}

		return result;
	}

	protected object GetWindowDocView(string mkDocument)
	{
		if (string.IsNullOrEmpty(mkDocument))
		{
			return null;
		}

		return GetWindowDocView(GetWindowFrameImpl(mkDocument));
	}

	protected object GetWindowDocView(IVsWindowFrame frame)
	{
		if (frame == null)
		{
			return null;
		}

		Diag.ThrowIfNotOnUIThread();

		int property = frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar);
		if (property != 0)
		{
			pvar = null;
			Tracer.Warning(GetType(), "GetWindowDocView()", "Could not retrieve docView from frame {0}.", property);
		}

		return pvar;
	}

	protected IVsWindowFrame GetWindowFrameImpl(string mkDocument)
	{
		if (string.IsNullOrEmpty(mkDocument))
			Diag.ThrowException(new ArgumentNullException(nameof(mkDocument)));

		Diag.ThrowIfNotOnUIThread();

		IVsWindowFrame result = null;

		Exf(UiShell.GetDocumentWindowEnum(out IEnumWindowFrames ppenum));
		IVsWindowFrame[] array = new IVsWindowFrame[1];

		while (ppenum.Next(1u, array, out uint pceltFetched) == 0 && pceltFetched == 1)
		{
			IVsWindowFrame vsWindowFrame = array[0];
			Exf(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out object pvar));

			if (pvar is IPersistFileFormat persistFileFormat)
			{
				Exf(persistFileFormat.GetCurFile(out string ppszFilename, out _));

				if (!string.IsNullOrEmpty(ppszFilename) && (string.Compare(ppszFilename, mkDocument,
					ignoreCase: true, CultureInfo.CurrentCulture) == 0 || Cmd.IsSamePath(ppszFilename, mkDocument)))
				{
					result = vsWindowFrame;
					break;
				}
			}
		}

		if (result == null)
			Tracer.Warning(GetType(), "GetWindowFrame()", "FAILED to find window frame for mkDocument: {0}", mkDocument);

		return result;
	}


	protected uint GetRdtCookieImpl(string mkDocument)
	{
		uint pdwCookie = 0u;

		if (RdtSvc == null)
			return pdwCookie;

			IntPtr ppunkDocData = IntPtr.Zero;
		try
		{
			FindAndLockDocumentImpl((uint)_VSRDTFLAGS.RDT_NoLock, mkDocument, out var _, out var _, out ppunkDocData, out pdwCookie);
			// We're not going to try for !Path.IsPathRooted(mkDocument) because it already will be.
		}
		finally
		{
			if (ppunkDocData != IntPtr.Zero)
			{
				Marshal.Release(ppunkDocData);
			}
		}

		return pdwCookie;
	}


	protected object GetDocData(string mkDocument)
	{
		IntPtr ppunkDocData = IntPtr.Zero;
		object result = null;

		if (RdtSvc == null)
			return null;

		try
		{
			FindAndLockDocumentImpl((uint)_VSRDTFLAGS.RDT_NoLock, mkDocument, out var _, out var _, out ppunkDocData, out var _);
		}
		finally
		{
			if (ppunkDocData != IntPtr.Zero)
			{
				result = Marshal.GetObjectForIUnknown(ppunkDocData);
				Marshal.Release(ppunkDocData);
			}
		}

		return result;
	}


	protected bool IsFileInRdtImpl(string mkDocument)
	{
		if (!string.IsNullOrEmpty(mkDocument))
		{
			return GetRdtCookieImpl(mkDocument) != 0;
		}

		return false;
	}



	protected bool TryGetFileNameFromCookie(uint docCookie, out string fileName)
	{
		fileName = string.Empty;
		if (RdtSvc != null)
		{
			Diag.ThrowIfNotOnUIThread();

			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				if (RdtSvc.GetDocumentInfo(docCookie, out var _, out var _, out var _, out var pbstrMkDocument, out var _, out var _, out ppunkDocData) == 0)
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

	protected bool WriteToFile(string mkDocument, string content)
	{
		return WriteToFile(mkDocument, content, saveFile: false);
	}

	protected bool WriteToFile(string mkDocument, string content, bool saveFile)
	{
		return WriteToFile(mkDocument, content, saveFile, createIfNotExist: false);
	}

	protected bool WriteToFile(string mkDocument, string content, bool saveFile, bool createIfNotExist)
	{
		bool result = true;
		IVsInvisibleEditor spEditor = null;
		IVsTextLines textLines = null;
		bool flag = false;
		try
		{
			if (IsFileInRdtImpl(mkDocument))
			{
				textLines = GetTextLines(mkDocument);
			}
			else
			{
				if (createIfNotExist && !File.Exists(mkDocument))
				{
					flag = true;
					FileStream fileStream = null;
					StreamWriter streamWriter = null;
					try
					{
						fileStream = File.Create(mkDocument);
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

				if (!flag && !TryGetTextLinesAndInvisibleEditor(mkDocument, out spEditor, out textLines))
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
							List<string> dirtyFiles = [mkDocument];
							SaveDirtyFiles(dirtyFiles);
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

	protected bool IsDocumentInitialized(uint docCookie)
	{
		return Rdt.GetDocumentInfo(docCookie).IsDocumentInitialized;
	}



	public virtual void Dispose()
	{
		lock (_KeepAliveLockLocal)
		{
			if (_KeepAliveDocCookies.Keys.Count != 0)
			{
				Diag.Dug(new ApplicationException("Events Manager is still trying to keep doc data alive on dispose, this could be a symptom of memory leak from invisible doc data."));
			}
		}
	}
}
