#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;


using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Controls.ResultsPane;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

public sealed class ShellTextBuffer : AbstractTextBuffer, IVsTextStreamEvents, IVsTextBufferDataEvents, IVsChangeClusterEvents, IVsTextBufferEvents
{
	private EventHandler _LoadedHandler;

	private Microsoft.VisualStudio.Shell.ServiceProvider serviceProvider;

	private IVsTextStream vsTextStream;

	private IVsTextBuffer vsTextBuffer;

	private ConnectionPointCookie textEventCookie;

	private ConnectionPointCookie bufferEventCookie;

	private ConnectionPointCookie clusterEventCookie;

	private ConnectionPointCookie textBufferEventCookie;

	private bool changingText;

	// private bool loaded;

	private bool withEncoding;

	private bool undoEnabled = true;

	private bool initContent;

	private readonly Hashtable markers = new Hashtable();

	public static int markerTypeError = 4;

	private static readonly int markerTypeTemplateParam = 4;

	public bool DetectLangSid
	{
		get
		{
			IVsUserData obj = vsTextStream as IVsUserData;
			Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
			Native.ThrowOnFailure(obj.GetData(ref riidKey, out var pvtData), (string)null);
			return (bool)pvtData;
		}
		set
		{
			IVsUserData obj = vsTextStream as IVsUserData;
			Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
			Native.ThrowOnFailure(obj.SetData(ref riidKey, value), (string)null);
		}
	}

	public IVsTextStream TextStream => vsTextStream;

	public IVsTextBuffer VsTextBuffer => vsTextBuffer;

	public string FileName
	{
		get
		{
			string result = null;
			if (vsTextStream is IVsUserData data)
			{
				Guid riidKey = typeof(IVsUserData).GUID;
				Native.ThrowOnFailure(data.GetData(ref riidKey, out object pvtData), (string)null);
				result = pvtData as string;
			}

			return result;
		}
	}

	public bool InitContent
	{
		get
		{
			return initContent;
		}
		set
		{
			initContent = value;
		}
	}

	public override bool IsReadOnly
	{
		get
		{
			bool result = false;
			Native.ThrowOnFailure(vsTextBuffer.GetStateFlags(out uint pdwReadOnlyFlags), (string)null);

			if ((pdwReadOnlyFlags & (uint)BUFFERSTATEFLAGS.BSF_FILESYS_READONLY) != 0)
				result = true;

			return result;
		}
	}

	public override bool IsDirty
	{
		get
		{
			bool result = false;
			Native.ThrowOnFailure(vsTextBuffer.GetStateFlags(out uint pdwReadOnlyFlags), (string)null);
			if ((pdwReadOnlyFlags & (uint)BUFFERSTATEFLAGS.BSF_MODIFIED) != 0)
			{
				result = true;
			}

			return result;
		}
	}

	public override string Text
	{
		get
		{
			return GetText(0, TextLength);
		}
		set
		{
			ReplaceText(0, TextLength, value);
		}
	}

	public override int TextLength
	{
		get
		{
			Native.ThrowOnFailure(vsTextStream.GetSize(out var piLength), (string)null);
			return piLength;
		}
	}

	public bool WithEncoding
	{
		get
		{
			return withEncoding;
		}
		set
		{
			withEncoding = value;
		}
	}

	public bool UndoEnabled
	{
		get
		{
			return undoEnabled;
		}
		set
		{
			if (value == undoEnabled)
			{
				return;
			}

			if (vsTextStream != null)
			{

				Native.ThrowOnFailure(vsTextStream.GetUndoManager(out IOleUndoManager ppUndoManager), (string)null);
				if (ppUndoManager != null)
				{
					if (!ThreadHelper.CheckAccess())
					{
						COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
						Diag.Dug(exc);
						throw exc;
					}

					ppUndoManager.Enable(value ? 1 : 0);
					undoEnabled = value;
				}
			}
		}
	}

	public event NewLangSvcEventHandler NewLangSvcEvent;

	public event EventHandler LoadedEvent
	{
		add
		{
			_LoadedHandler = (EventHandler)Delegate.Combine(_LoadedHandler, value);
		}
		remove
		{
			_LoadedHandler = (EventHandler)Delegate.Remove(_LoadedHandler, value);
		}
	}

	public ShellTextBuffer(IVsTextStream textStream, object serviceProvider)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		Initialize(textStream, new ServiceProvider(serviceProvider as Microsoft.VisualStudio.OLE.Interop.IServiceProvider));
	}

	public ShellTextBuffer()
	{
	}

	public void SetSite(object serviceProvider)
	{
		if (serviceProvider == null)
		{
			Exception ex = new ArgumentNullException("serviceProvider");
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		if (serviceProvider is not Microsoft.VisualStudio.OLE.Interop.IServiceProvider)
		{
			Exception ex2 = new ArgumentException("", "serviceProvider");
			Tracer.LogExThrow(GetType(), ex2);
			throw ex2;
		}

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		ServiceProvider serviceProvider2 = new ServiceProvider(serviceProvider as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
		ILocalRegistry obj = (ILocalRegistry)serviceProvider2.GetService(typeof(ILocalRegistry));
		if (obj == null)
		{
			ServiceUnavailableException ex3 = new(typeof(ILocalRegistry));
			Tracer.LogExThrow(GetType(), ex3);
			throw ex3;
		}

		Guid riid = typeof(IVsTextStream).GUID;
		Native.ThrowOnFailure(obj.CreateInstance(DefGuidList.CLSID_VsTextBuffer, null,
			ref riid, (uint)CLSCTX.CLSCTX_INPROC_SERVER, out IntPtr ppvObj), (string)null);
		IVsTextStream vsTextStream = (IVsTextStream)Marshal.GetObjectForIUnknown(ppvObj);
		Marshal.Release(ppvObj);

		if (vsTextStream is IObjectWithSite site)
		{
			site.SetSite(serviceProvider);
		}

		if (withEncoding)
		{
			uint num = 1u;
			IVsUserData obj2 = vsTextStream as IVsUserData;
			Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingPromptOnLoad_guid;
			Native.ThrowOnFailure(obj2.SetData(ref riidKey, num), (string)null);
		}

		IVsTextStream vsTextStream2 = vsTextStream;
		if (InitContent)
		{
			vsTextStream2.InitializeContent("", 0);
		}

		Dispose();
		Initialize(vsTextStream, serviceProvider2);
	}

	private void Initialize(IVsTextStream textStream, Microsoft.VisualStudio.Shell.ServiceProvider sp)
	{
		vsTextStream = textStream;
		vsTextBuffer = textStream;
		serviceProvider = sp;
		// string fileName = FileName;
		// loaded = fileName != null && fileName.Length > 0;
		SinkTextBufferEvents(sink: true);
	}

	public override void Dispose()
	{
		// Tracer.Trace(GetType(), "ShellTextBuffer.Dispose", "", null);
		SinkTextBufferEvents(sink: false);
		if (vsTextStream != null)
		{
			vsTextStream = null;
		}

		if (vsTextBuffer != null)
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			(vsTextBuffer as IVsPersistDocData)?.Close();
			vsTextBuffer = null;
		}

		serviceProvider = null;
		ClearAllMarkers();

		base.Dispose();
	}

	public override void Dirty()
	{
		Native.ThrowOnFailure(vsTextBuffer.GetStateFlags(out uint pdwReadOnlyFlags), (string)null);
		Native.ThrowOnFailure(vsTextBuffer.SetStateFlags(pdwReadOnlyFlags | (uint)BUFFERSTATEFLAGS.BSF_MODIFIED), (string)null);
	}

	private IVsWindowFrame GetWindowFrame(Guid logview)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		IVsUIShellOpenDocument obj = (IVsUIShellOpenDocument)serviceProvider.GetService(typeof(IVsUIShellOpenDocument));
		if (obj == null)
		{
			ServiceUnavailableException ex = new (typeof(IVsUIShellOpenDocument));
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		Native.ThrowOnFailure(obj.OpenDocumentViaProject(FileName, ref logview, out _, out _, out _, out IVsWindowFrame ppWindowFrame), (string)null);

		return ppWindowFrame;
	}

	public override string GetText(int startPosition, int chars)
	{
		if (chars > 1073741822)
		{
			ArgumentOutOfRangeException ex = new("chars");
			Diag.Dug(ex);
			throw ex;
		}

		IntPtr intPtr = Marshal.AllocCoTaskMem((chars + 1) * 2);
		try
		{
			Native.ThrowOnFailure(vsTextStream.GetStream(startPosition, chars, intPtr), (string)null);
			return Marshal.PtrToStringUni(intPtr);
		}
		finally
		{
			Marshal.FreeCoTaskMem(intPtr);
		}
	}

	void IVsChangeClusterEvents.OnChangeClusterOpening(uint dwFlags)
	{
	}

	void IVsChangeClusterEvents.OnChangeClusterClosing(uint dwFlags)
	{
		if (((dwFlags & (uint)ChangeClusterFlags.CCE_UNDO) != 0 || (dwFlags & (uint)ChangeClusterFlags.CCE_REDO) != 0) && !changingText)
		{
			OnTextChanged(EventArgs.Empty);
		}
	}

	void IVsTextBufferDataEvents.OnFileChanged(uint grfChange, uint dwFileAttrs)
	{
		if (grfChange != 1)
		{
			if (!changingText)
			{
				OnTextChanged(EventArgs.Empty);
			}
		}
		else
		{
			OnAttributeChanged(EventArgs.Empty);
		}
	}

	int IVsTextBufferDataEvents.OnLoadCompleted(int fReload)
	{
		if (fReload != 0)
		{
			if (!changingText)
			{
				OnTextChanged(EventArgs.Empty);
			}
		}
		else
		{
			// loaded = true;
			_LoadedHandler?.Invoke(this, EventArgs.Empty);
		}

		return VSConstants.S_OK;
	}

	void IVsTextStreamEvents.OnChangeStreamAttributes(int iPos, int iLength)
	{
	}

	void IVsTextStreamEvents.OnChangeStreamText(int iPos, int iOldLen, int iNewLen, int fLast)
	{
		if (!changingText)
		{
			OnTextChanged(EventArgs.Empty);
		}
	}

	void IVsTextBufferEvents.OnNewLanguageService(ref Guid sidLangServiceID)
	{
		NewLangSvcEvent?.Invoke(this, new(sidLangServiceID));
	}

	public void Clear()
	{
		bool flag = UndoEnabled;
		UndoEnabled = false;
		ClearAllMarkers();
		ReplaceText(0, TextLength, "");
		UndoEnabled = flag;
	}

	public int CreateStreamMarker(int markerType, int position, int length, int doubleClickLine, SqlTextSpan textSpan)
	{
		return CreateStreamMarkerInternal(markerType, position, length, doubleClickLine, textSpan);
	}

	public void ClearMarkers(int markerType)
	{
		object[] array = new object[markers.Values.Count];
		markers.Values.CopyTo(array, 0);
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Marker marker = (Marker)array2[i];
			if (marker.MarkerType == markerType)
			{
				Native.ThrowOnFailure(marker.VsMarker.Invalidate(), (string)null);
			}
		}
	}

	public void ClearAllMarkers()
	{
		ClearMarkers(1);
		ClearMarkers(2);
		markers.Clear();
	}

	private int CreateStreamMarkerInternal(int markerType, int position, int length, int doubleClickLine, SqlTextSpan textSpan)
	{
		IVsTextStreamMarker[] array = new IVsTextStreamMarker[1];
		Marker marker = new Marker(markers, markerType);
		switch (markerType)
		{
			case 1:
				Native.ThrowOnFailure(vsTextStream.CreateStreamMarker(markerTypeError, position, length, marker, array), (string)null);
				break;
			case 2:
				Native.ThrowOnFailure(vsTextStream.CreateStreamMarker(markerTypeTemplateParam, position, length, marker, array), (string)null);
				break;
			case 3:
				Native.ThrowOnFailure(vsTextStream.CreateStreamMarker(1, position, length, marker, array), (string)null);
				break;
			default:
				{
					Exception ex = new ArgumentException("Unknown marker type: " + markerType);
					Tracer.LogExThrow(GetType(), ex);
					throw ex;
				}
		}

		marker.VsMarker = array[0];
		marker.DoubleClickLine = doubleClickLine;
		marker.TextSpan = textSpan;
		markers.Add(marker.Id, marker);
		return marker.Id;
	}

	public override void ReplaceText(int startPosition, int count, string text)
	{
		Native.ThrowOnFailure(vsTextStream.GetStateFlags(out uint pdwReadOnlyFlags), (string)null);
		try
		{
			Native.ThrowOnFailure(vsTextStream.SetStateFlags(pdwReadOnlyFlags & 0xFFFFFFFEu), (string)null);
			int length = text.Length;
			int textLength = TextLength;
			Native.ThrowOnFailure(vsTextStream.CanReplaceStream(startPosition, count, length), (string)null);
			count = Math.Min(count, textLength);
			changingText = true;
			try
			{
				IntPtr intPtr = Marshal.StringToCoTaskMemUni(text);
				try
				{
					if (Native.Failed(vsTextStream.ReplaceStream(startPosition, count, intPtr, text.Length)))
					{
						Exception ex = new("Couldn't replace text");
						throw ex;
					}
				}
				finally
				{
					Marshal.FreeCoTaskMem(intPtr);
				}
			}
			catch (Exception ex)
			{
				Tracer.LogExCatch(GetType(), ex);
				Exception exo = new("Couldn't replace text", ex);
				throw exo;
			}
			finally
			{
				changingText = false;
			}
		}
		finally
		{
			Native.ThrowOnFailure(vsTextStream.SetStateFlags(pdwReadOnlyFlags), (string)null);
		}
	}

	public override void ShowCode()
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		Native.ThrowOnFailure(GetWindowFrame(VSConstants.LOGVIEWID_Code).Show(), (string)null);
	}

	public override void ShowCode(int lineNum)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		try
		{
			IVsWindowFrame windowFrame = GetWindowFrame(VSConstants.LOGVIEWID_Code);
			Native.ThrowOnFailure(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar), (string)null);
			IVsCodeWindow vsCodeWindow = (IVsCodeWindow)pvar;
			if (vsCodeWindow != null)
			{
				Native.ThrowOnFailure(vsCodeWindow.GetBuffer(out var ppBuffer), (string)null);
				IVsTextManager vsTextManager = (IVsTextManager)serviceProvider.GetService(VS.CLSID_TextManager);
				if (lineNum > 0)
				{
					lineNum--;
				}

				if (vsTextManager != null)
				{
					Guid guidDocViewType = VSConstants.LOGVIEWID_Code;
					Native.ThrowOnFailure(vsTextManager.NavigateToLineAndColumn((VsTextBuffer)ppBuffer, ref guidDocViewType, lineNum, 0, lineNum, 0), (string)null);
				}
			}
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
		}
	}

	private void SinkTextBufferEvents(bool sink)
	{
		if (sink && vsTextStream != null)
		{
			bufferEventCookie ??= new ConnectionPointCookie(vsTextStream, this, typeof(IVsTextBufferDataEvents));

			textEventCookie ??= new ConnectionPointCookie(vsTextStream, this, typeof(IVsTextStreamEvents));

			if (clusterEventCookie == null)
			{
				Native.ThrowOnFailure(vsTextStream.GetUndoManager(out IOleUndoManager ppUndoManager), (string)null);
				clusterEventCookie = new ConnectionPointCookie(ppUndoManager, this, typeof(IVsChangeClusterEvents));
			}

			textBufferEventCookie ??= new ConnectionPointCookie(vsTextStream, this, typeof(IVsTextBufferEvents));

			return;
		}

		if (textEventCookie != null)
		{
			textEventCookie.Dispose(disposing: true);
			textEventCookie = null;
		}

		if (bufferEventCookie != null)
		{
			bufferEventCookie.Dispose(disposing: true);
			bufferEventCookie = null;
		}

		if (clusterEventCookie != null)
		{
			clusterEventCookie.Dispose(disposing: true);
			clusterEventCookie = null;
		}

		if (textBufferEventCookie != null)
		{
			textBufferEventCookie.Dispose(disposing: true);
			textBufferEventCookie = null;
		}
	}
}
