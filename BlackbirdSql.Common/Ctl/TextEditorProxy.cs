// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TextEditorProxy
using System;
using BlackbirdSql.Common.Interfaces;



namespace BlackbirdSql.Common.Ctl;

public class TextEditorProxy : MarshalByRefObject, ITextEditor, IDisposable
{
	private ITextEditor _underlyingEditor;

	private ITextEditorEvents _textEditorEvents;

	public ITextEditorEvents TextEditorEvents => _textEditorEvents;

	public TextEditorProxy(ITextEditor editor)
	{
		_underlyingEditor = editor;
	}

	public void Select(int offset, int length)
	{
		_underlyingEditor?.Select(offset, length);
	}

	public void Focus()
	{
		_underlyingEditor?.Focus();
	}

	public void SetTextEditorEvents(ITextEditorEvents events)
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
		ITextEditorEvents textEditorEvents = _textEditorEvents;
		_textEditorEvents = null;
		textEditorEvents?.SetTextEditor(null);
		_textEditorEvents = events;
		if (_textEditorEvents != null && marshalByRefObject != null)
		{
			_textEditorEvents.SetTextEditor(this);
			SyncState();
		}
	}

	public void SyncState()
	{
		_underlyingEditor?.SyncState();
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_textEditorEvents != null)
			{
				_textEditorEvents.SetTextEditor(null);
				_textEditorEvents = null;
			}
			_underlyingEditor = null;
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
