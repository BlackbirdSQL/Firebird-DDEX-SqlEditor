// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlTextSpan

using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Ctl;


public class SqlTextSpan : IBsTextSpan
{
	private readonly int _anchorLine;

	private readonly int _endLine;

	private readonly int _anchorCol;

	private readonly int _endCol;

	private readonly int _offset;

	private string _text;

	private readonly IVsTextView _vsTextView;

	private readonly int _lineWithinTextSpan = -1;

	public int AnchorLine => _anchorLine;

	public int AnchorCol => _anchorCol;

	public int EndLine => _endLine;

	public int EndCol => _endCol;

	public int Offset => _offset;

	public int LineWithinTextSpan => _lineWithinTextSpan;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	public object TextView => _vsTextView;

	public IVsTextView VsTextView => _vsTextView;

	public SqlTextSpan(int anchorLine, int anchorCol, int endLine, int endCol, string text, IVsTextView vsTextView)
	{
		if (anchorLine < endLine)
		{
			_anchorLine = anchorLine;
			_anchorCol = anchorCol;
			_endLine = endLine;
			_endCol = endCol;
		}
		else
		{
			_anchorLine = endLine;
			_anchorCol = endCol;
			_endLine = anchorLine;
			_endCol = anchorCol;
		}

		_offset = 0;
		_text = text;
		_vsTextView = vsTextView;
	}

	public SqlTextSpan(int anchorLine, int anchorCol, int endLine, int endCol, int offset, string text, IVsTextView vsTextView)
		: this(anchorLine, anchorCol, endLine, endCol, text, vsTextView)
	{
		_offset = offset;
	}

	public SqlTextSpan(int anchorLine, int anchorCol, int endLine, int endCol, int offset, int lineWithinTextSpan, string text, IVsTextView vsTextView)
		: this(anchorLine, anchorCol, endLine, endCol, offset, text, vsTextView)
	{
		_lineWithinTextSpan = lineWithinTextSpan;
	}
}
