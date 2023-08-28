#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

namespace BlackbirdSql.Common.Ctl;


public abstract class AbstractTextBuffer
{
	private EventHandler _attributeChangedHandler;

	private EventHandler _textChangedHandler;

	private bool _lockEvents;

	public bool LockEvents
	{
		get
		{
			return _lockEvents;
		}
		set
		{
			_lockEvents = value;
		}
	}

	public virtual bool IsReadOnly => false;

	public virtual bool IsDirty => false;

	public abstract string Text { get; set; }

	public abstract int TextLength { get; }

	public event EventHandler AttributeChanged
	{
		add
		{
			_attributeChangedHandler = (EventHandler)Delegate.Combine(_attributeChangedHandler, value);
		}
		remove
		{
			_attributeChangedHandler = (EventHandler)Delegate.Remove(_attributeChangedHandler, value);
		}
	}

	public event EventHandler TextChanged
	{
		add
		{
			_textChangedHandler = (EventHandler)Delegate.Combine(_textChangedHandler, value);
		}
		remove
		{
			_textChangedHandler = (EventHandler)Delegate.Remove(_textChangedHandler, value);
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

	public abstract string GetText(int startPosition, int chars);

	protected void OnAttributeChanged(EventArgs e)
	{
		if (_attributeChangedHandler != null && !_lockEvents)
		{
			_attributeChangedHandler(this, e);
		}
	}

	protected void OnTextChanged(EventArgs e)
	{
		if (_textChangedHandler != null && !_lockEvents)
		{
			_textChangedHandler(this, e);
		}
	}

	public abstract void ReplaceText(int startPosition, int count, string text);

	public abstract void ShowCode();

	public abstract void ShowCode(int lineNum);
}
