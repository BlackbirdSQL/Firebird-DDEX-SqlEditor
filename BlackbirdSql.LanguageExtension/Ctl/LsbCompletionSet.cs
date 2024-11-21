// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.SqlCompletionSet
using System;
using System.Globalization;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.LanguageExtension.Properties;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.Ctl;


internal class LsbCompletionSet : CompletionSet, IVsCompletionSetBuilder
{

	public LsbCompletionSet(ImageList imageList, Source source)
		: base(imageList, source)
	{
		_Source = source;
		_TextView = null;
		_TextmarkerSpan.iStartLine = _TextmarkerSpan.iEndLine = -1;
		_TextmarkerSpan.iStartIndex = _TextmarkerSpan.iEndIndex = -1;
		InPreviewMode = true;
		InPreviewModeOutline = true;
	}





	private const char C_OpenSquareBracket = '[';
	private const char C_DoubleQuote = '"';

	private bool _CompleteWord;

	private readonly Source _Source;

	private IVsTextView _TextView;

	private LsbDeclarations _Declarations;
	private TextSpan _TextmarkerSpan;

	private int _LastBestMatch;

	private bool _ForceSelectInGetBestMatch;

	private bool _WasUnique;

	internal bool InPreviewMode { get; set; }

	internal bool InPreviewModeOutline { get; set; }


	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

	public override void Init(IVsTextView textView, Declarations declarations, bool completeWord)
	{
		_WasUnique = false;
		_CompleteWord = completeWord;
		if (IsDisplayed)
		{
			ResetTextMarker();
			ExplicitFilterDeclarationList();
			if (completeWord && !InPreviewMode || !InPreviewModeOutline)
			{
				base.Init(textView, declarations, completeWord);
			}
		}
		else
		{
			_LastBestMatch = 0;
			_TextView = textView;
			ResetTextMarker();
			_Declarations = (LsbDeclarations)declarations;
			InPreviewModeOutline = InPreviewMode;
			FilterDeclarationList(_Declarations);
			base.Init(textView, _Declarations, completeWord);
		}
		if (_WasUnique && completeWord)
		{
			Dismiss();
		}
	}

	public override int GetBestMatch(string textSoFar, int length, out int index, out uint flags)
	{
		flags = 0u;
		index = 0;
		bool uniqueMatch = false;

		LsbDeclarations declarations = Declarations as LsbDeclarations;

		if (declarations.GetCount() == 1 && _CompleteWord && (!InPreviewModeOutline || !InPreviewMode))
		{
			index = 0;
			flags = 1u;
			uniqueMatch = true;
			_WasUnique = true;
		}
		else if (!string.IsNullOrEmpty(textSoFar))
		{
			declarations.GetBestMatch(textSoFar, out index, out uniqueMatch);
			if (index < 0 || index >= declarations.GetCount())
			{
				index = _LastBestMatch;
				uniqueMatch = false;
			}
			else if (!InPreviewModeOutline || !InPreviewMode)
			{
				flags = 1u;
			}
		}
		if (_ForceSelectInGetBestMatch)
		{
			flags |= 1u;
		}
		if (uniqueMatch)
		{
			flags |= 2u;
		}
		_LastBestMatch = index;
		return 0;
	}

	internal void ResetTextMarker()
	{
		___(_TextView.GetCaretPos(out int piLine, out int piColumn));

		TokenInfo tokenInfo = _Source.GetTokenInfo(piLine, piColumn);

		if (tokenInfo.Type.Equals(TokenType.Unknown))
		{
			_TextmarkerSpan.iStartLine = piLine;
			_TextmarkerSpan.iStartIndex = piColumn;
		}
		else
		{
			bool flag = tokenInfo.Type.Equals(TokenType.Delimiter) || tokenInfo.Type.Equals(TokenType.WhiteSpace);
			_TextmarkerSpan.iStartLine = piLine;
			_TextmarkerSpan.iStartIndex = flag ? tokenInfo.EndIndex + 1 : tokenInfo.StartIndex;
		}

		IVsTextLines textLines = _Source.GetTextLines();
		textLines.GetLineCount(out _);
		textLines.GetLengthOfLine(piLine, out int piLength);
		_TextmarkerSpan.iEndLine = piLine;
		_TextmarkerSpan.iEndIndex = Math.Min(piColumn + 1, piLength - 1);
	}



	private bool FilterDeclarationList(LsbDeclarations decls)
	{
		if (decls == null || decls.GetCount() <= 0)
		{
			return false;
		}
		string textTypedSoFar = GetTextTypedSoFar();
		textTypedSoFar = EscapeSequence.UnescapeIdentifier(textTypedSoFar);


		bool flag = (bool)decls.Filter(textTypedSoFar, IsDisplayed);

		return InPreviewMode || flag;
	}

	public void ExplicitFilterDeclarationList()
	{
		if (FilterDeclarationList((LsbDeclarations)Declarations))
			UpdateCompletionStatus(forceSelect: false);
	}

	internal void UpdateCompletionStatus(bool forceSelect)
	{
		bool forceSelectInGetBestMatch = _ForceSelectInGetBestMatch;
		try
		{
			_ForceSelectInGetBestMatch = forceSelect;
			___(_TextView.UpdateCompletionStatus(this, 1u));
		}
		finally
		{
			_ForceSelectInGetBestMatch = forceSelectInGetBestMatch;
		}
	}

	public string GetTextTypedSoFar()
	{
		if (_TextView == null)
		{
			return "";
		}

		___(_TextView.GetCaretPos(out int piLine, out int piColumn));

		TokenInfo tokenInfo = _Source.GetTokenInfo(piLine, piColumn);
		bool flag = tokenInfo.Type.Equals(TokenType.Delimiter) || tokenInfo.Type.Equals(TokenType.Unknown);

		_TextmarkerSpan.iEndLine = piLine;
		_TextmarkerSpan.iEndIndex = flag ? tokenInfo.StartIndex : tokenInfo.EndIndex + 1;
		_TextmarkerSpan.iEndIndex = Math.Max(_TextmarkerSpan.iStartIndex, _TextmarkerSpan.iEndIndex);

		int startLine = _TextmarkerSpan.iStartLine;
		int endLine = _TextmarkerSpan.iEndLine;
		int startCol = _TextmarkerSpan.iStartIndex;
		int endCol = _TextmarkerSpan.iEndIndex;

		if (!ValidateTextMarkerSpan())
		{
			startLine = piLine;
			endLine = piLine;
			startCol = tokenInfo.StartIndex;
			endCol = flag ? tokenInfo.StartIndex : tokenInfo.EndIndex + 1;
		}

		string empty = _Source.GetText(startLine, startCol, endLine, endCol);

		if (StartsWithEscapeSequence(empty))
		{

			string intendedBestMatch = _Declarations.GetIntendedBestMatch(empty);
			_TextmarkerSpan.iEndIndex = _TextmarkerSpan.iStartIndex + intendedBestMatch.Length;
			empty = intendedBestMatch;
		}

		return empty;
	}



	private bool ValidateTextMarkerSpan()
	{
		bool result = true;
		if (_TextmarkerSpan.iStartLine > _TextmarkerSpan.iEndLine)
		{
			result = false;
		}
		IVsTextLines textLines = _Source.GetTextLines();
		textLines.GetLineCount(out int piLineCount);
		if (_TextmarkerSpan.iStartLine >= 0 && _TextmarkerSpan.iEndLine < piLineCount)
		{
			textLines.GetLengthOfLine(_TextmarkerSpan.iStartLine, out int piLength);
			textLines.GetLengthOfLine(_TextmarkerSpan.iEndLine, out int piLength2);
			if (_TextmarkerSpan.iStartIndex < 0 || _TextmarkerSpan.iStartIndex > piLength)
			{
				result = false;
			}
			if (_TextmarkerSpan.iEndIndex < 0 || _TextmarkerSpan.iEndIndex > piLength2)
			{
				result = false;
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	private bool StartsWithEscapeSequence(string str)
	{
		if (!str.StartsWith("[", StringComparison.OrdinalIgnoreCase) && !str.StartsWith("\"", StringComparison.OrdinalIgnoreCase))
		{
			return str.StartsWith("'", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public override int GetInitialExtent(out int line, out int startIdx, out int endIdx)
	{
		string textTypedSoFar = GetTextTypedSoFar();

		if (IsExplicitFilteringRequired(textTypedSoFar) || StartsWithEscapeSequence(textTypedSoFar))
		{
			line = _TextmarkerSpan.iStartLine;
			startIdx = _TextmarkerSpan.iStartIndex;
			endIdx = _TextmarkerSpan.iEndIndex;
			return 0;
		}
		return base.GetInitialExtent(out line, out startIdx, out endIdx);
	}

	private static bool IsExplicitFilteringRequired(string textTypedSofar)
	{
		if (string.IsNullOrEmpty(textTypedSofar) || textTypedSofar[0] == C_OpenSquareBracket
			|| textTypedSofar[0] == C_DoubleQuote || char.IsLetter(textTypedSofar[0]))
		{
			return false;
		}
		return true;
	}

	public override int OnCommitComplete()
	{
		// TODO: link to connection TTS.

		LsbSource source = _Source as LsbSource;

		if (source.CurrentCommitUndoTransaction != null)
		{
			source.CurrentCommitUndoTransaction.Complete();
			source.CurrentCommitUndoTransaction = null;
		}

		return base.OnCommitComplete();
	}

	public override void Dismiss()
	{
		if (LsbLanguageServiceTestEvents.Instance.EnableTestEvents)
		{
			LsbLanguageServiceTestEvents.Instance.RaiseSqlCompletionSetDismissedEvent();
		}

		base.Dismiss();
	}

	public int GetBuilderCount(ref int piCount)
	{
		piCount = 0;
		if (InPreviewMode)
		{
			piCount = 1;
		}
		return 0;
	}

	public int GetBuilderDescriptionText(int iIndex, out string pbstrDescription)
	{
		pbstrDescription = "";

		if (iIndex == 0)
		{
			string firstBindingForCommand = GetFirstBindingForCommand("Edit.ToggleCompletionMode");
			if (!string.IsNullOrEmpty(firstBindingForCommand))
				pbstrDescription = Resources.PreviewModeCompletionDescription.Fmt(firstBindingForCommand);
		}

		return 0;
	}

	public int GetBuilderDisplayText(int iIndex, out string pbstrText, int[] piGlyph = null)
	{
		pbstrText = "";
		if (IsDisplayed)
		{
			pbstrText = GetTextTypedSoFar();
		}
		return 0;
	}

	public int GetBuilderImageList(out IntPtr phImages)
	{
		phImages = IntPtr.Zero;
		return 0;
	}

	public int GetBuilderItemColor(int iIndex, out uint dwFGColor, out uint dwBGColor)
	{
		dwFGColor = 0u;
		dwBGColor = 0u;
		return 0;
	}

	public int OnBuilderCommit(int iIndex)
	{
		Dismiss();
		return 0;
	}

	private string GetFirstBindingForCommand(string commandName)
	{
		Commands commands = (ApcManager.Dte as DTE2).Commands;
		Command command;
		try
		{
			command = commands.Item(commandName);
		}
		catch (ArgumentException)
		{
			command = null;
		}
		if (command == null)
		{
			return "";
		}
		object[] array = (object[])command.Bindings;
		if (array.Length < 1)
		{
			return "";
		}
		string text = (string)array[0];
		string text2 = "::";
		int num = text.IndexOf(text2, StringComparison.OrdinalIgnoreCase);
		if (num >= 0)
		{
			int startIndex = num + text2.Length;
			return text[startIndex..];
		}
		return text;
	}
}
