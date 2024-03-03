#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Properties;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Ctl;


[ComVisible(false)]
public class Marker(IDictionary markers) : IVsTextMarkerClient
{
	private readonly IDictionary _markers = markers;


	public Marker(IDictionary markers, int mtype)
		: this(markers)
	{
		_markerType = mtype;
	}

	public Marker(IDictionary markers, int mtype, int doubleClickLine, SqlTextSpan textSpan)
		: this(markers, mtype)
	{
		_doubleClickLine = doubleClickLine;
		_TextSpan = textSpan;
	}

	public Marker(IDictionary markers, int mtype, string toolTip)
		: this(markers, mtype)
	{
		SetMarkerTooltip(toolTip);
	}

	public Marker(IDictionary markers, int mtype, string toolTip, int doubleClickLine, SqlTextSpan textSpan)
		: this(markers, mtype, toolTip)
	{
		_doubleClickLine = doubleClickLine;
		_TextSpan = textSpan;
	}





	private readonly int _Id = _nextMarkerId++;

	private int _markerType;

	private string _markerTooltip = "";

	private IVsTextStreamMarker _marker;

	private static int _nextMarkerId;

	private int _doubleClickLine;

	private SqlTextSpan _TextSpan;

	public IVsTextStreamMarker VsMarker
	{
		get
		{
			return _marker;
		}
		set
		{
			_marker = value;
		}
	}

	public int Id => _Id;

	public int MarkerType
	{
		get
		{
			return _markerType;
		}
		set
		{
			_markerType = value;
		}
	}

	public SqlTextSpan TextSpan
	{
		get
		{
			return _TextSpan;
		}
		set
		{
			_TextSpan = value;
		}
	}

	public int DoubleClickLine
	{
		get
		{
			return _doubleClickLine;
		}
		set
		{
			_doubleClickLine = value;
		}
	}


	public void SetMarkerTooltip(string newToolTip)
	{
		newToolTip ??= "";

		_markerTooltip = newToolTip;
	}

	void IVsTextMarkerClient.MarkerInvalidated()
	{
		try
		{
			_markers?.Remove(_Id);

			if (_marker != null)
			{
				_marker = null;
			}

			if (_TextSpan != null)
			{
				_TextSpan = null;
			}
		}
		catch (Exception e)
		{
			Diag.Dug(e);
		}
	}

	int IVsTextMarkerClient.GetTipText(IVsTextMarker marker, string[] text)
	{
		if (text != null)
		{
			text[0] = _markerTooltip;
		}

		return VSConstants.S_OK;
	}

	void IVsTextMarkerClient.OnBufferSave(string fileName)
	{
	}

	void IVsTextMarkerClient.OnBeforeBufferClose()
	{
	}

	int IVsTextMarkerClient.GetMarkerCommandInfo(IVsTextMarker marker, int item, string[] text, uint[] cmd)
	{
		if (text != null)
		{
			text[0] = "";
		}

		cmd[0] = 0u;
		if (item == (int)MarkerCommandValues.mcvBodyDoubleClickCommand && _TextSpan != null && _doubleClickLine >= 0)
		{
			cmd[0] = 1u;
		}

		return VSConstants.S_OK;
	}

	int IVsTextMarkerClient.ExecMarkerCommand(IVsTextMarker marker, int item)
	{
		if (item == 258)
		{
			OnDoubleClick();
			return 1;
		}

		COMException ex = new(ControlsResources.ThisCommandIsNotSupported, VSConstants.E_UNEXPECTED);
		Diag.Dug(ex);
		throw ex;
	}

	void IVsTextMarkerClient.OnAfterSpanReload()
	{
	}

	int IVsTextMarkerClient.OnAfterMarkerChange(IVsTextMarker marker)
	{
		return VSConstants.S_OK;
	}

	private void OnDoubleClick()
	{
		if (_TextSpan == null)
		{
			COMException ex = new(ControlsResources.ViewHaveToBeSetBeforeExecutingDoubleClickEvent, VSConstants.E_UNEXPECTED);
			Diag.Dug(ex);
			throw ex;
		}

		int num;
		if (_TextSpan.Offset > 0)
		{
			Native.ThrowOnFailure(_TextSpan.VsTextView.GetNearestPosition(_TextSpan.AnchorLine, _TextSpan.AnchorCol, out var piPos, out _), (string)null);
			piPos += _TextSpan.Offset;
			Native.ThrowOnFailure(_TextSpan.VsTextView.GetLineAndColumn(piPos, out var piLine, out _), (string)null);
			piLine += _doubleClickLine - 1;
			num = piLine;
		}
		else if (_TextSpan.LineWithinTextSpan >= 0)
		{
			int num2 = _TextSpan.LineWithinTextSpan;
			if (_doubleClickLine >= 0)
			{
				num2 = num2 + _doubleClickLine - 1;
			}

			num = _TextSpan.AnchorLine + num2;
		}
		else
		{
			num = _TextSpan.AnchorLine + _doubleClickLine - 1;
		}

		if (num > -1)
		{
			Native.ThrowOnFailure(_TextSpan.VsTextView.GetBuffer(out var ppBuffer), (string)null);
			if (ppBuffer != null)
			{
				Native.ThrowOnFailure(ppBuffer.GetLineCount(out var piLineCount), (string)null);
				if (num > piLineCount)
				{
					return;
				}

				Native.ThrowOnFailure(ppBuffer.GetLengthOfLine(num, out var piLength), (string)null);
				Native.ThrowOnFailure(_TextSpan.VsTextView.SetSelection(num, 0, num, piLength), (string)null);
			}
			else
			{
				Native.ThrowOnFailure(_TextSpan.VsTextView.SetCaretPos(num, 0), (string)null);
			}
		}

		IntPtr windowHandle = _TextSpan.VsTextView.GetWindowHandle();
		if (windowHandle != IntPtr.Zero)
		{
			Native.SetFocus(windowHandle);
		}
	}
}
