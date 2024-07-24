// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Common.WaitCursorHelper

using System;
using System.Windows.Forms;



namespace BlackbirdSql.Shared.Ctl;


public sealed class WaitCursorHelper
{
	private class WaitCursor : IDisposable
	{
		private Cursor _CurrentCursor;

		public WaitCursor()
		{
			_CurrentCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
		}

		~WaitCursor()
		{
			DisposeInternal();
		}

		public void Dispose()
		{
			DisposeInternal();
			GC.SuppressFinalize(this);
		}

		private void DisposeInternal()
		{
			if (!(_CurrentCursor != null))
			{
				return;
			}

			lock (this)
			{
				if (_CurrentCursor != null)
				{
					Cursor.Current = _CurrentCursor;
					_CurrentCursor = null;
				}
			}
		}
	}

	public static IDisposable NewWaitCursor()
	{
		return new WaitCursor();
	}
}
