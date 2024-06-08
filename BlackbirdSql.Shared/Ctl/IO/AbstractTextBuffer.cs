// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.TextBuffer

using System;
using Microsoft.VisualStudio;



namespace BlackbirdSql.Shared.Ctl.IO;


public abstract class AbstractTextBuffer
{
	private EventHandler _AttributeChangedHandler;

	private EventHandler _TextChangedHandler;

	private bool _LockEvents;

	public bool LockEvents
	{
		get
		{
			return _LockEvents;
		}
		set
		{
			_LockEvents = value;
		}
	}

	public virtual bool IsReadOnly => false;

	public virtual bool IsDirty => false;

	public abstract string Text { get; set; }

	public abstract int TextLength { get; }

	public event EventHandler AttributeChangedEvent
	{
		add
		{
			_AttributeChangedHandler = (EventHandler)Delegate.Combine(_AttributeChangedHandler, value);
		}
		remove
		{
			_AttributeChangedHandler = (EventHandler)Delegate.Remove(_AttributeChangedHandler, value);
		}
	}

	public event EventHandler TextChangedEvent
	{
		add
		{
			_TextChangedHandler = (EventHandler)Delegate.Combine(_TextChangedHandler, value);
		}
		remove
		{
			_TextChangedHandler = (EventHandler)Delegate.Remove(_TextChangedHandler, value);
		}
	}

	public virtual void Checkout()
	{
	}

	public virtual void Dirty()
	{
	}

	public virtual void Dispose()
	{
	}



	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	public abstract string GetText(int startPosition, int chars);

	protected void OnAttributeChanged(EventArgs e)
	{
		if (_AttributeChangedHandler != null && !_LockEvents)
		{
			_AttributeChangedHandler(this, e);
		}
	}

	protected void OnTextChanged(EventArgs e)
	{
		if (_TextChangedHandler != null && !_LockEvents)
		{
			_TextChangedHandler(this, e);
		}
	}

	public abstract void ReplaceText(int startPosition, int count, string text);

	public abstract void ShowCode();

	public abstract void ShowCode(int lineNum);
}
