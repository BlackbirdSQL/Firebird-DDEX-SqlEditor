// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.WindowWithFocusRestorer
using System;



namespace BlackbirdSql.Common.Ctl;

internal class WindowWithFocusRestorer : IDisposable
{
	private readonly IntPtr _hwndFocus;

	private bool _isRestored;

	public WindowWithFocusRestorer()
	{
		_hwndFocus = Native.GetFocus();
	}

	~WindowWithFocusRestorer()
	{
		RestoreFocus();
	}

	public void Dispose()
	{
		RestoreFocus();
		GC.SuppressFinalize(this);
	}

	private void RestoreFocus()
	{
		if (!_isRestored)
		{
			_isRestored = true;
			if (_hwndFocus != IntPtr.Zero && Native.IsWindow(_hwndFocus))
			{
				Native.SetFocus(_hwndFocus);
			}
			GC.KeepAlive(this);
		}
	}
}