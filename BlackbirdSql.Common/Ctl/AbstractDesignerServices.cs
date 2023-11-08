// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;


namespace BlackbirdSql.Common.Ctl;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Callers using this class must ensure compliance")]

public abstract class AbstractDesignerServices
{


	protected static Guid _DslEditorFactoryClsid = Guid.Empty;
	protected static Dictionary<DatabaseLocation, Dictionary<NodeElementDescriptor, string>> _InflightOpens = null;
	// A static class lock
	protected static object _LockClass;


	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
	protected static event EventHandler<BeforeOpenDocumentEventArgs> S_BeforeOpenDocumentEvent;


	public static event EventHandler<BeforeOpenDocumentEventArgs> SBeforeOpenDocumentEvent
	{
		add {S_BeforeOpenDocumentEvent += value; }
		remove { S_BeforeOpenDocumentEvent -= value; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
	protected static EventHandler<BeforeOpenDocumentEventArgs> S_BeforeOpenDocumentHandler => S_BeforeOpenDocumentEvent;


	public static Guid DslEditorFactoryClsid
	{
		get
		{
			if (_DslEditorFactoryClsid == Guid.Empty)
				_DslEditorFactoryClsid = new(SystemData.DslEditorFactoryGuid);

			return _DslEditorFactoryClsid;
		}
	}

	protected static Dictionary<DatabaseLocation, Dictionary<NodeElementDescriptor, string>> InflightOpens
		=> _InflightOpens ??= new(DatabaseLocation.CreateComparer());

	protected static IServiceProvider OleProvider => Controller.OleServiceProvider;


	public AbstractDesignerServices()
	{
		_LockClass ??= new object();
	}

	protected static void AddInflightOpen(DatabaseLocation dbl, NodeElementDescriptor descriptor, string moniker)
	{
		lock (_LockClass)
		{
			if (!InflightOpens.TryGetValue(dbl, out Dictionary<NodeElementDescriptor, string> value))
			{
				value = new Dictionary<NodeElementDescriptor, string>();
				InflightOpens[dbl] = value;
			}
			value[descriptor] = moniker;
		}
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.VsTextBufferHelper
	public static IVsTextLines CreateTextLines(string moniker, DbConnectionStringBuilder csb)
	{
		SqlTracer.AssertTraceEvent(!RdtManager.Instance.IsFileInRdt(moniker), TraceEventType.Warning, EnSqlTraceId.TableDesigner, "The document: " + moniker + " is already in RDT.");

		IVsTextLines vsTextLines = null;
		ILocalRegistry localRegistry = Controller.GetService<SLocalRegistry, ILocalRegistry>();

		if (localRegistry != null)
		{
			vsTextLines = CreateTextLinesInstance<IVsTextLines>(OleProvider, localRegistry);

			IVsUserData obj = (IVsUserData)vsTextLines;
			Guid clsid = new(LibraryData.SqlEditorConnectionStringGuid);
			obj.SetData(clsid, (object)csb);

			Guid clsidUserData = typeof(IVsUserData).GUID;
			ErrorHandler.ThrowOnFailure(obj.SetData(ref clsidUserData, moniker));
			SetTextBufferLanguageService(vsTextLines);
		}

		return vsTextLines;
	}



	// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Design.Common.VsTextBufferFactory:CreateInstance
	public static T CreateTextLinesInstance<T>(IServiceProvider serviceProvider, ILocalRegistry localRegistry) where T : IVsTextBuffer
	{
		T val = CreateTextLinesObject<T>(localRegistry);

		if (val is IObjectWithSite)
			((IObjectWithSite)(object)val).SetSite(serviceProvider);

		return val;
	}


	// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Design.Common.VsTextBufferFactory:CreateObject
	protected static T CreateTextLinesObject<T>(ILocalRegistry localRegistry)
	{
		object obj = null;
		Guid riid = typeof(T).GUID;

		Native.ThrowOnFailure(localRegistry.CreateInstance(typeof(VsTextBufferClass).GUID, null, ref riid, 1u, out IntPtr ppvObj));

		try
		{
			obj = Marshal.GetObjectForIUnknown(ppvObj);
		}
		finally
		{
			Marshal.Release(ppvObj);
		}
		return (T)obj;
	}



	// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.Data.Tools.Schema.Common.Threading.EventMarshaler:Invoke
	protected static void ExecuteDocumentLoadedCallback(Delegate method, params object[] args)
	{
		if (method == null)
			return;

		try
		{
			method.DynamicInvoke(args);
		}
		catch (MemberAccessException ex)
		{
			Diag.Dug(ex);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): Could not access target member {0}", ex);
			throw;
		}
		catch (TargetException ex2)
		{
			Diag.Dug(ex2);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): Attempted to invoke on null target or the member does not exist {0}", ex2);
			throw;
		}
		catch (TargetInvocationException ex3)
		{
			Diag.Dug(ex3);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): The target threw an exception.  {0}", ex3);
			if (ex3.InnerException != null)
			{
				Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): The target threw an exception.  Inner exception {0}", ex3.InnerException);
				Exception innerException = ex3.InnerException;
				innerException.Data.Add("TargetSiteCallstack", innerException.StackTrace);
				innerException.Data.Add("OriginalException", ex3);
				ExceptionDispatchInfo.Capture(innerException).Throw();
			}
			throw;
		}
		catch (Exception ex4)
		{
			Diag.Dug(ex4);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "Exception in EventMarshaler.Invoke(...): " + ex4.ToString());
			throw;
		}
	}



	protected static string LookupObjectMoniker(DatabaseLocation dbl, EnModelObjectType elementType, IList<string> identifierList)
	{
		string value = null;

		lock (_LockClass)
		{
			if (InflightOpens.TryGetValue(dbl, out Dictionary<NodeElementDescriptor, string> value2))
			{
				NodeElementDescriptor key = new(elementType, identifierList);
				value2.TryGetValue(key, out value);
			}
		}

		return value;
	}




	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.OnlineDocumentHelpers
	public static void OpenMiscDocument(string mkDocument, DbConnectionStringBuilder csb, bool createIfNotOpen, bool isReadonly,
		Guid editorFactory, out uint docCookie, out IVsWindowFrame frame, out bool editorAlreadyOpened,
		out bool documentAlreadyLoaded, string physicalViewName = null)
	{
		OpenMiscDocumentWithDocData(mkDocument, csb, null, createIfNotOpen, editorFactory, out docCookie,
			out frame, out editorAlreadyOpened, out documentAlreadyLoaded,
			delegate
			{
				IVsTextLines vsTextLines = CreateTextLines(mkDocument, csb);
				uint lockType = (uint)_VSRDTFLAGS.RDT_ReadLock;

				uint cookie = RegisterAndLockDocumentInRDT(mkDocument, vsTextLines, lockType);
				vsTextLines.InitializeContent(string.Empty, 0);

				if (isReadonly)
					SetDocumentReadOnly(cookie, readOnly: true);

				RegisterForDirtyChangeNotification(cookie);

				return cookie;
			},
			physicalViewName);
	}


	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.OnlineDocumentHelpers
	public static int OpenMiscDocumentWithSpecific(string mkDocument, string physicalViewName, __VSSPECIFICEDITORFLAGS editorFlags,
		ref Guid guidEditorType, ref Guid guidLogicalView, out IVsWindowFrame frame, out IServiceProvider sp,
		out uint itemid, out IVsUIHierarchy hier)
	{
		itemid = 0u;
		hier = null;
		frame = null;
		sp = null;

		IVsUIShellOpenDocument vsUIShellOpenDocument = Controller.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();

		if (vsUIShellOpenDocument == null)
		{
			NotSupportedException ex = new("IVsUIShellOpenDocument");
			Diag.Dug(ex);
			SqlTracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "Could not get uiOpenDocument");
			return VSConstants.E_FAIL;
		}

		int hr = vsUIShellOpenDocument.OpenDocumentViaProjectWithSpecific(mkDocument, (uint)editorFlags, ref guidEditorType,
			physicalViewName, ref guidLogicalView, out sp, out hier, out itemid, out frame);

		if (hr < 0 && (editorFlags & __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseView) > 0)
		{
			// This issue does not seem to be occuring anymore.
			Diag.Dug(true, "OpenDocumentViaProjectWithSpecific with UseView failed. RETRYING with UseEditor");
			editorFlags = __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen | __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor;
			hr = vsUIShellOpenDocument.OpenDocumentViaProjectWithSpecific(mkDocument, (uint)editorFlags, ref guidEditorType,
			physicalViewName, ref guidLogicalView, out sp, out hier, out itemid, out frame);
		}

		if (hr < 0)
		{
			Diag.Dug(new Exception($"OpenDocumentViaProjectWithSpecific() failed with hr = '{hr} ({guidEditorType} : {editorFlags})"));
		}

		return hr;
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.OnlineDocumentHelpers
	public static void OpenMiscDocumentReadOnly(string mkDocument, DbConnectionStringBuilder csb, bool createIfNotOpen, Guid editorFactory,
		out uint docCookie, out IVsWindowFrame frame, out bool editorAlreadyOpened,
		out bool documentAlreadyLoaded, string physicalViewName = null)
	{
		OpenMiscDocument(mkDocument, csb, createIfNotOpen, true, editorFactory, out docCookie, out frame, out editorAlreadyOpened, out documentAlreadyLoaded, physicalViewName);
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.OnlineDocumentHelpers
	protected static void OpenMiscDocumentWithDocData(string mkDocument, DbConnectionStringBuilder csb, string captionPostfix, bool createIfNotOpen,
		Guid editorFactory, out uint docCookie, out IVsWindowFrame frame, out bool editorAlreadyOpened,
		out bool documentAlreadyLoaded, Func<uint> docDataCreateFunc, string physicalViewName = null)
	{
		docCookie = RdtManager.Instance.GetRdtCookie(mkDocument);
		editorAlreadyOpened = false;
		documentAlreadyLoaded = docCookie != 0;
		frame = null;

		IVsUIShellOpenDocument vsUIShellOpenDocument = Controller.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument >();

		if (vsUIShellOpenDocument == null)
		{
			NotSupportedException ex = new("IVsUIShellOpenDocument");
			Diag.Dug(ex);
			SqlTracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "Could not get uiOpenDocument");
			return;
		}

		Guid guidLogicalView = VSConstants.LOGVIEWID_Primary;
		Guid rguidEditorType = editorFactory;
		uint[] array = new uint[1];
		IVsUIHierarchy pHierCaller = null;

		int num = vsUIShellOpenDocument.IsSpecificDocumentViewOpen(pHierCaller, uint.MaxValue,
			mkDocument, ref rguidEditorType, physicalViewName, (uint)__VSIDOFLAGS.IDO_ActivateIfOpen, out IVsUIHierarchy ppHierOpen, out array[0],
			out IVsWindowFrame ppWindowFrame, out int pfOpen);

		_ = ppHierOpen; // Suppress

		editorAlreadyOpened = ErrorHandler.Succeeded(num) && pfOpen != 0;

		if (editorAlreadyOpened && ppWindowFrame != null)
		{
			ppWindowFrame.Show();
			frame = ppWindowFrame;
		}
		else
		{
			if (editorAlreadyOpened || !createIfNotOpen)
			{
				return;
			}
			if (!documentAlreadyLoaded && docDataCreateFunc != null)
			{
				docCookie = docDataCreateFunc();
			}
			try
			{
				__VSSPECIFICEDITORFLAGS flags = __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen;

				 flags |= rguidEditorType != DslEditorFactoryClsid
					? __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor
					: __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseView;

				num = OpenMiscDocumentWithSpecific(mkDocument, physicalViewName, flags, ref rguidEditorType,
					ref guidLogicalView, out ppWindowFrame, out IServiceProvider sp, out var itemid, out var hier);
				if (ErrorHandler.Succeeded(num))
				{
					if (hier != null)
					{
						string text = Path.GetFileNameWithoutExtension(mkDocument);
						if (!string.IsNullOrWhiteSpace(captionPostfix))
						{
							text = text + " " + captionPostfix;
						}
						hier.SetProperty(itemid, (int)__VSHPROPID.VSHPROPID_IsNewUnsavedItem, true);
						hier.SetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Caption, text);
						hier.SetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ItemDocCookie, docCookie);
					}
					if (ppWindowFrame != null)
					{
						ppWindowFrame.Show();
						frame = ppWindowFrame;
					}
				}
				else
				{
					SqlTracer.AssertTraceEvent(condition: false, TraceEventType.Critical, EnSqlTraceId.CoreServices, "Failed to open editor hr = " + num);
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
			finally
			{
				if (!documentAlreadyLoaded)
				{
					UnlockDocumentInRDT(docCookie, (uint)_VSRDTFLAGS.RDT_ReadLock);
				}
			}
		}
	}



	protected static void PopulateEditorWithObject(bool isNewObject, string mkDocument, uint docCookie,
		string script, HashSet<NodeElementDescriptor> originalObjects = null, bool updateBufferText = true)
	{
		try
		{
			if (RdtManager.Instance.GetRdtCookie(mkDocument) == docCookie && docCookie != 0)
			{
				if (updateBufferText)
				{
					SetDocumentReadOnly(docCookie, readOnly: false);
					SetTextIntoTextBuffer(mkDocument, script);
					SuppressChangeTracking(mkDocument, suppress: false);
				}
				// proj.MonitorBuffer(isNewObject, docCookie, script, originalObjects);
			}
			else
			{
				KeyNotFoundException ex = new($"Document cookie ({docCookie}) not found for document '{mkDocument}'.");
				Diag.Dug(ex);
			}
		}
		catch (ObjectDisposedException ex)
		{
			Diag.Dug(ex);
			WriteModelDisposedToDocument(mkDocument);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}



	protected static void RaiseBeforeOpenDocument(string mkDocument, DatabaseLocation dbl, IList<string> identifierList, EnModelObjectType elementType, EventHandler<BeforeOpenDocumentEventArgs> handlers)
	{
		if (handlers == null)
			return;

		BeforeOpenDocumentEventArgs e = new BeforeOpenDocumentEventArgs(mkDocument, dbl, identifierList, elementType);

		handlers(null, e);
	}


	public static uint RegisterAndLockDocumentInRDT(string moniker, object docData,
		uint lockType, IVsHierarchy hier = null, uint itemid = uint.MaxValue)
	{
		uint pdwCookie = 0u;
		IVsRunningDocumentTable vsRunningDocumentTable = Controller.DocTable;
		if (vsRunningDocumentTable != null)
		{
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				intPtr = Marshal.GetIUnknownForObject(docData);
				vsRunningDocumentTable.RegisterAndLockDocument(lockType, moniker, hier, itemid, intPtr, out pdwCookie);
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.Release(intPtr);
				}
			}
		}
		return pdwCookie;
	}



	public static void RegisterForDirtyChangeNotification(uint docCookie)
	{
		if (!RdtManager.Instance.TryGetDocDataFromCookie(docCookie, out var docData))
		{
			return;
		}

		IComponentModel componentModel = Controller.GetService<SComponentModel, IComponentModel>();
		if (componentModel == null)
			return;

		IVsEditorAdaptersFactoryService service = componentModel.GetService<IVsEditorAdaptersFactoryService>();
		if (service == null)
			return;

		ITextBuffer documentBuffer = service.GetDocumentBuffer((IVsTextBuffer)docData);
		ITextDocumentFactoryService service2 = componentModel.GetService<ITextDocumentFactoryService>();
		if (service2 != null)
		{
			service2.TryGetTextDocument(documentBuffer, out var textDocument);
			textDocument.DirtyStateChanged += delegate
			{
				(Controller.Instance.DocTable as IVsRunningDocumentTable3)?.UpdateDirtyState(docCookie);
			};
		}
	}



	protected static void RemoveInflightOpen(DatabaseLocation dbl, NodeElementDescriptor desc)
	{
		/*
		if (desc == null)
		{
			throw ExceptionFactory.CreateArgumentNullException("desc");
		}
		*/
		lock (_LockClass)
		{
			if (_InflightOpens.TryGetValue(dbl, out var value))
			{
				value.Remove(desc);
			}
		}
	}


	public static void SetDocumentReadOnly(uint docCookie, bool readOnly)
	{
		if (RdtManager.Instance.TryGetDocDataFromCookie(docCookie, out var docData))
		{
			((IVsPersistDocData2)docData).SetDocDataReadOnly(readOnly ? 1 : 0);
			IVsRunningDocumentTable vsRunningDocumentTable = Controller.DocTable;
			if (vsRunningDocumentTable != null)
			{
				uint grfDocChanged = (readOnly ? 262144u : 524288u);
				vsRunningDocumentTable.NotifyDocumentChanged(docCookie, grfDocChanged);
			}
		}
	}



	public static void SetTextBufferLanguageService(IVsTextLines textBuffer)
	{
		// SqlExceptionUtils.ValidateNullParameter(textBuffer, "textBuffer");
		Guid guidLangService = new(SystemData.MandatedSqlLanguageServiceGuid);
		ErrorHandler.ThrowOnFailure(textBuffer.SetLanguageServiceID(ref guidLangService));
		IVsUserData obj = (IVsUserData)textBuffer;
		Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
		ErrorHandler.ThrowOnFailure(obj.SetData(ref riidKey, false));
	}

	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.VsTextBufferHelper
	public static void SetTextIntoTextBuffer(string mkDocument, string text)
	{
		uint rdtCookie = RdtManager.Instance.GetRdtCookie(mkDocument);
		if (rdtCookie != 0)
		{
			SetTextIntoTextBuffer(rdtCookie, text);
		}
		else
		{
			ArgumentException ex = new("NOT FOUND cookie to set text from: " + mkDocument);
			Diag.Dug(ex);
			throw ex;
		}

	}

	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.VsTextBufferHelper
	public static void SetTextIntoTextBuffer(uint docCookie, string text)
	{
		if (RdtManager.Instance.TryGetDocDataFromCookie(docCookie, out object docData) && docData is IVsTextLines buffer)
		{
			SetTextIntoTextBuffer(buffer, text);
		}
		else
		{
			ArgumentException ex = new("NOT FOUND docdata to set text from cookie: " + docCookie);
			Diag.Dug(ex);
			throw ex;
		}
	}

	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.VsTextBufferHelper
	public static void SetTextIntoTextBuffer(IVsTextLines buffer, string text)
	{
		buffer.GetLastLineIndex(out var piLine, out var piIndex);
		int iNewLen = ((!string.IsNullOrEmpty(text)) ? text.Length : 0);
		IntPtr intPtr = Marshal.StringToCoTaskMemAuto(text);

		try
		{
			IVsPersistDocData2 vsPersistDocData = (IVsPersistDocData2)buffer;
			vsPersistDocData.IsDocDataReadOnly(out int pfReadOnly);

			if (pfReadOnly != 0)
				vsPersistDocData.SetDocDataReadOnly(0);

			buffer.ReplaceLines(0, 0, piLine, piIndex, intPtr, iNewLen, null);
			buffer.GetUndoManager(out IOleUndoManager ppUndoManager);
			SqlTracer.AssertTraceEvent(ppUndoManager != null, TraceEventType.Warning, EnSqlTraceId.CoreServices, "Undo manager was null");

			if (ppUndoManager != null)
			{
				ppUndoManager.DiscardFrom(null);
				vsPersistDocData.SetDocDataDirty(0);
			}
			if (pfReadOnly != 0)
			{
				vsPersistDocData.SetDocDataReadOnly(1);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(intPtr);
			}
		}
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.OnlineDocumentHelpers:SuppressChangeTracking
	public static void SuppressChangeTracking(string mkDocument, bool suppress)
	{
		if (!RdtManager.Instance.TryGetCodeWindow(mkDocument, out IVsCodeWindow codeWindow) || codeWindow.GetPrimaryView(out IVsTextView ppView) != 0 || ppView == null)
		{
			return;
		}
		if (((System.IServiceProvider)Controller.DdexPackage).GetService(typeof(SComponentModel)) is not IComponentModel componentModel)
		{
			return;
		}
		IVsEditorAdaptersFactoryService service = componentModel.GetService<IVsEditorAdaptersFactoryService>();
		if (service == null)
			return;

		IWpfTextViewHost wpfTextViewHost = service.GetWpfTextViewHost(ppView);
		if (wpfTextViewHost != null)
		{
			if (suppress)
			{
				wpfTextViewHost.TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, value: false);
			}
			else
			{
				wpfTextViewHost.TextView.Options.ClearOptionValue(DefaultTextViewHostOptions.ChangeTrackingId);
			}
		}
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.OnlineDocumentHelpers
	public static void UnlockDocumentInRDT(uint docCookie, uint lockType)
	{
		Controller.Instance.DocTable?.UnlockDocument(lockType, docCookie);
	}



	protected static void WriteModelDisposedToDocument(string mkDocument)
	{
		SqlTracer.TraceEvent(TraceEventType.Information, EnSqlTraceId.CoreServices, "Node disposed of before object definition could be retrieved");
		string text = string.Format(CultureInfo.CurrentCulture, "/*\n\r{0}\n\r*/", ControlsResources.PowerBuffer_ModelDisposed);
		SetTextIntoTextBuffer(mkDocument, text);
	}
}
