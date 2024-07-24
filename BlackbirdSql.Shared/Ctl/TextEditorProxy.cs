// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TextEditorProxy

using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Ctl;


public class TextEditorProxy : MarshalByRefObject, IBsTextEditor, IDisposable
{

	public TextEditorProxy(IBsTextEditor editor)
	{
		_UnderlyingEditor = editor;
	}



	private IBsTextEditor _UnderlyingEditor;

	private IBsTextEditorEvents _TextEditorEvents;

	public IBsTextEditorEvents TextEditorEvents => _TextEditorEvents;


	public void Select(int offset, int length)
	{
		_UnderlyingEditor?.Select(offset, length);
	}

	public void Focus()
	{
		_UnderlyingEditor?.Focus();
	}

	public void SetTextEditorEvents(IBsTextEditorEvents events)
	{
		MarshalByRefObject marshalByRefObject = null;
		if (events != null)
		{
			marshalByRefObject = events as MarshalByRefObject;
			if (marshalByRefObject == null)
			{
				throw new NotSupportedException();
			}
		}
		IBsTextEditorEvents textEditorEvents = _TextEditorEvents;
		_TextEditorEvents = null;
		textEditorEvents?.SetTextEditor(null);
		_TextEditorEvents = events;
		if (_TextEditorEvents != null && marshalByRefObject != null)
		{
			_TextEditorEvents.SetTextEditor(this);
			SyncState();
		}
	}

	public void SyncState()
	{
		_UnderlyingEditor?.SyncState();
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_TextEditorEvents != null)
			{
				_TextEditorEvents.SetTextEditor(null);
				_TextEditorEvents = null;
			}
			_UnderlyingEditor = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~TextEditorProxy()
	{
		Dispose(disposing: false);
	}
}
