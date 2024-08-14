// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.ShellTextBuffer

using System;
using System.Collections;
using System.Runtime.InteropServices;
using BlackbirdSql.Shared.Events;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Ctl.IO;


public sealed class ShellTextBuffer : AbstractTextBuffer, IVsTextStreamEvents, IVsTextBufferDataEvents, IVsChangeClusterEvents, IVsTextBufferEvents
{
	private EventHandler _LoadedHandler;

	private ServiceProvider serviceProvider;

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

	private readonly Hashtable markers = [];

	public static int markerTypeError = 4;

	private static readonly int markerTypeTemplateParam = 4;

	public bool DetectLangSid
	{
		get
		{
			IVsUserData obj = vsTextStream as IVsUserData;
			Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;

			___(obj.GetData(ref riidKey, out var pvtData));

			return (bool)pvtData;
		}
		set
		{
			IVsUserData obj = vsTextStream as IVsUserData;
			Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;

			___(obj.SetData(ref riidKey, value));
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
				___(data.GetData(ref riidKey, out object pvtData));
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
			___(vsTextBuffer.GetStateFlags(out uint pdwReadOnlyFlags));

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

			___(vsTextBuffer.GetStateFlags(out uint pdwReadOnlyFlags));

			if ((pdwReadOnlyFlags & (uint)BUFFERSTATEFLAGS.BSF_MODIFIED) != 0)
				result = true;

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
			___(vsTextStream.GetSize(out var piLength));
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

				___(vsTextStream.GetUndoManager(out IOleUndoManager ppUndoManager));
				if (ppUndoManager != null)
				{
					Diag.ThrowIfNotOnUIThread();

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
		Diag.ThrowIfNotOnUIThread();

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
			Diag.ThrowException(ex);
		}

		if (serviceProvider is not Microsoft.VisualStudio.OLE.Interop.IServiceProvider)
		{
			Exception ex2 = new ArgumentException("", "serviceProvider");
			Diag.ThrowException(ex2);
		}

		Diag.ThrowIfNotOnUIThread();

		ServiceProvider serviceProvider2 = new ServiceProvider(serviceProvider as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
		ILocalRegistry obj = (ILocalRegistry)serviceProvider2.GetService(typeof(ILocalRegistry));
		if (obj == null)
		{
			ServiceUnavailableException ex3 = new(typeof(ILocalRegistry));
			Diag.ThrowException(ex3);
		}

		Guid riid = typeof(IVsTextStream).GUID;
		___(obj.CreateInstance(VS.CLSID_VsTextBuffer, null,
			ref riid, (uint)CLSCTX.CLSCTX_INPROC_SERVER, out IntPtr ppvObj));
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
			___(obj2.SetData(ref riidKey, num));
		}

		IVsTextStream vsTextStream2 = vsTextStream;
		if (InitContent)
		{
			vsTextStream2.InitializeContent("", 0);
		}

		Dispose();
		Initialize(vsTextStream, serviceProvider2);
	}

	private void Initialize(IVsTextStream textStream, ServiceProvider sp)
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
		// Tracer.Trace(GetType(), "Dispose()", "");

		SinkTextBufferEvents(sink: false);
		if (vsTextStream != null)
		{
			vsTextStream = null;
		}

		if (vsTextBuffer != null)
		{
			Diag.ThrowIfNotOnUIThread();

			(vsTextBuffer as IVsPersistDocData)?.Close();
			vsTextBuffer = null;
		}

		serviceProvider = null;
		ClearAllMarkers();

		base.Dispose();
	}

	public override void Dirty()
	{
		___(vsTextBuffer.GetStateFlags(out uint pdwReadOnlyFlags));
		___(vsTextBuffer.SetStateFlags(pdwReadOnlyFlags | (uint)BUFFERSTATEFLAGS.BSF_MODIFIED));
	}

	private IVsWindowFrame GetWindowFrame(Guid logview)
	{
		Diag.ThrowIfNotOnUIThread();

		IVsUIShellOpenDocument obj = (IVsUIShellOpenDocument)serviceProvider.GetService(typeof(IVsUIShellOpenDocument));
		if (obj == null)
		{
			ServiceUnavailableException ex = new(typeof(IVsUIShellOpenDocument));
			Diag.ThrowException(ex);
		}

		___(obj.OpenDocumentViaProject(FileName, ref logview, out _, out _, out _, out IVsWindowFrame ppWindowFrame));

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
			___(vsTextStream.GetStream(startPosition, chars, intPtr));
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

	public int CreateStreamMarker(int markerType, int position, int length, int doubleClickLine, TextSpanEx textSpan)
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
				___(marker.VsMarker.Invalidate());
			}
		}
	}

	public void ClearAllMarkers()
	{
		ClearMarkers(1);
		ClearMarkers(2);
		markers.Clear();
	}

	private int CreateStreamMarkerInternal(int markerType, int position, int length, int doubleClickLine, TextSpanEx textSpan)
	{
		IVsTextStreamMarker[] array = new IVsTextStreamMarker[1];
		Marker marker = new Marker(markers, markerType);
		switch (markerType)
		{
			case 1:
				___(vsTextStream.CreateStreamMarker(markerTypeError, position, length, marker, array));
				break;
			case 2:
				___(vsTextStream.CreateStreamMarker(markerTypeTemplateParam, position, length, marker, array));
				break;
			case 3:
				___(vsTextStream.CreateStreamMarker(1, position, length, marker, array));
				break;
			default:
				Exception ex = new ArgumentException("Unknown marker type: " + markerType);
				Diag.ThrowException(ex);
				break;
		}

		marker.VsMarker = array[0];
		marker.DoubleClickLine = doubleClickLine;
		marker.TextSpan = textSpan;
		markers.Add(marker.Id, marker);
		return marker.Id;
	}

	public override void ReplaceText(int startPosition, int count, string text)
	{
		___(vsTextStream.GetStateFlags(out uint pdwReadOnlyFlags));
		try
		{
			___(vsTextStream.SetStateFlags(pdwReadOnlyFlags & 0xFFFFFFFEu));
			int length = text.Length;
			int textLength = TextLength;
			___(vsTextStream.CanReplaceStream(startPosition, count, length));
			count = Math.Min(count, textLength);
			changingText = true;

			try
			{
				IntPtr intPtr = Marshal.StringToCoTaskMemUni(text);
				try
				{
					if (!__(vsTextStream.ReplaceStream(startPosition, count, intPtr, text.Length)))
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
				Diag.Dug(ex);
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
			___(vsTextStream.SetStateFlags(pdwReadOnlyFlags));
		}
	}

	public override void ShowCode()
	{
		Diag.ThrowIfNotOnUIThread();

		___(GetWindowFrame(VSConstants.LOGVIEWID_Code).Show());
	}

	public override void ShowCode(int lineNum)
	{
		Diag.ThrowIfNotOnUIThread();

		try
		{
			IVsWindowFrame windowFrame = GetWindowFrame(VSConstants.LOGVIEWID_Code);
			___(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar));
			IVsCodeWindow vsCodeWindow = (IVsCodeWindow)pvar;
			if (vsCodeWindow != null)
			{
				___(vsCodeWindow.GetBuffer(out var ppBuffer));
				IVsTextManager vsTextManager = (IVsTextManager)serviceProvider.GetService(VS.CLSID_TextManager);
				if (lineNum > 0)
				{
					lineNum--;
				}

				if (vsTextManager != null)
				{
					Guid guidDocViewType = VSConstants.LOGVIEWID_Code;
					___(vsTextManager.NavigateToLineAndColumn((VsTextBuffer)ppBuffer, ref guidDocViewType, lineNum, 0, lineNum, 0));
				}
			}
		}
		catch (Exception e)
		{
			Diag.Dug(e);
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
				___(vsTextStream.GetUndoManager(out IOleUndoManager ppUndoManager));
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
