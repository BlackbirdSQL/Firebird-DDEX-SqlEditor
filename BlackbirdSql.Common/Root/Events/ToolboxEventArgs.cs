// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ToolboxEventArgs
using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Common.Events;

public class ToolboxEventArgs : EventArgs
{
	private readonly IDataObject _data;

	private bool _handled;

	private int _hresult;

	public IDataObject Data => _data;

	public bool Handled
	{
		get
		{
			return _handled;
		}
		set
		{
			_handled = value;
		}
	}

	public int HResult
	{
		get
		{
			return _hresult;
		}
		set
		{
			_hresult = value;
		}
	}

	public ToolboxEventArgs(IDataObject pDO)
	{
		_data = pDO;
		_handled = false;
		_hresult = VSConstants.E_NOTIMPL;
	}
}