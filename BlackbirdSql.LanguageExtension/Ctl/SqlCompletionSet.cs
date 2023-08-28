#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Globalization;
using System.Windows.Forms;

using EnvDTE;
using EnvDTE80;

using Microsoft.SqlServer.Management.SqlParser.Parser;

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core;
using BlackbirdSql.LanguageExtension.Properties;

// using Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;



// namespace Microsoft.VisualStudio.Data.Tools.SqlLanguageServices
namespace BlackbirdSql.LanguageExtension
{
	internal class SqlCompletionSet : CompletionSet, IVsCompletionSetBuilder
	{
		private bool _completeWord;

		private readonly Microsoft.VisualStudio.Package.Source source;

		private IVsTextView textView;

		private Declarations declarations;

		private TextSpan textmarkerSpan;

		private int _lastBestMatch;

		private bool _forceSelectInGetBestMatch;

		private bool _wasUnique;

		internal bool InPreviewMode { get; set; }

		internal bool InPreviewModeOutline { get; set; }

		public SqlCompletionSet(ImageList imageList, Microsoft.VisualStudio.Package.Source source)
			: base(imageList, source)
		{
			this.source = source;
			textView = null;
			textmarkerSpan.iStartLine = (textmarkerSpan.iEndLine = -1);
			textmarkerSpan.iStartIndex = (textmarkerSpan.iEndIndex = -1);
			InPreviewMode = true;
			InPreviewModeOutline = true;
		}

		public override void Init(IVsTextView textView, Microsoft.VisualStudio.Package.Declarations declarations, bool completeWord)
		{
			_wasUnique = false;
			_completeWord = completeWord;
			if (base.IsDisplayed)
			{
				ResetTextMarker();
				ExplicitFilterDeclarationList();
				if ((completeWord && !InPreviewMode) || !InPreviewModeOutline)
				{
					base.Init(textView, declarations, completeWord);
				}
			}
			else
			{
				_lastBestMatch = 0;
				this.textView = textView;
				ResetTextMarker();
				this.declarations = (Declarations)declarations;
				InPreviewModeOutline = InPreviewMode;
				FilterDeclarationList(this.declarations);
				base.Init(textView, this.declarations, completeWord);
			}

			if (_wasUnique && completeWord)
			{
				Dismiss();
			}
		}

		public override int GetBestMatch(string textSoFar, int length, out int index, out uint flags)
		{
			flags = 0u;
			index = 0;
			bool uniqueMatch = false;
			Declarations declarations = base.Declarations as Declarations;
			if (declarations.GetCount() == 1 && _completeWord && (!InPreviewModeOutline || !InPreviewMode))
			{
				index = 0;
				flags = (uint)UpdateCompletionFlags.UCS_NAMESCHANGED;
				uniqueMatch = true;
				_wasUnique = true;
			}
			else if (!string.IsNullOrEmpty(textSoFar))
			{
				declarations.GetBestMatch(textSoFar, out index, out uniqueMatch);
				if (index < 0 || index >= declarations.GetCount())
				{
					index = _lastBestMatch;
					uniqueMatch = false;
				}
				else if (!InPreviewModeOutline || !InPreviewMode)
				{
					flags = (uint)UpdateCompletionFlags.UCS_NAMESCHANGED;
				}
			}

			if (_forceSelectInGetBestMatch)
			{
				flags |= (uint)UpdateCompletionFlags.UCS_NAMESCHANGED;
			}

			if (uniqueMatch)
			{
				flags |= (uint)UpdateCompletionFlags.UCS_EXTENTCHANGED;
			}

			_lastBestMatch = index;
			return 0;
		}

		internal void ResetTextMarker()
		{
			Native.ThrowOnFailure(textView.GetCaretPos(out var piLine, out var piColumn));
			TokenInfo tokenInfo = source.GetTokenInfo(piLine, piColumn);
			if (tokenInfo.Type.Equals(TokenType.Unknown))
			{
				textmarkerSpan.iStartLine = piLine;
				textmarkerSpan.iStartIndex = piColumn;
			}
			else
			{
				bool flag = tokenInfo.Type.Equals(TokenType.Delimiter) || tokenInfo.Type.Equals(TokenType.WhiteSpace);
				textmarkerSpan.iStartLine = piLine;
				textmarkerSpan.iStartIndex = (flag ? (tokenInfo.EndIndex + 1) : tokenInfo.StartIndex);
			}

			IVsTextLines textLines = source.GetTextLines();
			textLines.GetLineCount(out var _);
			textLines.GetLengthOfLine(piLine, out var piLength);
			textmarkerSpan.iEndLine = piLine;
			textmarkerSpan.iEndIndex = Math.Min(piColumn + 1, piLength - 1);
		}

		private bool FilterDeclarationList(Declarations decls)
		{
			if (decls == null || decls.GetCount() <= 0)
			{
				return false;
			}

			string textTypedSoFar = GetTextTypedSoFar();
			textTypedSoFar = EscapeSequence.UnescapeIdentifier(textTypedSoFar);
			bool flag = decls.Filter(textTypedSoFar, base.IsDisplayed);
			return InPreviewMode || flag;
		}

		public void ExplicitFilterDeclarationList()
		{
			if (FilterDeclarationList((Declarations)base.Declarations))
			{
				UpdateCompletionStatus(forceSelect: false);
			}
		}

		internal void UpdateCompletionStatus(bool forceSelect)
		{
			bool forceSelectInGetBestMatch = _forceSelectInGetBestMatch;
			try
			{
				_forceSelectInGetBestMatch = forceSelect;
				Native.ThrowOnFailure(textView.UpdateCompletionStatus(this, (uint)UpdateCompletionFlags.UCS_NAMESCHANGED));
			}
			finally
			{
				_forceSelectInGetBestMatch = forceSelectInGetBestMatch;
			}
		}

		public string GetTextTypedSoFar()
		{
			if (textView == null)
			{
				return string.Empty;
			}

			Native.ThrowOnFailure(textView.GetCaretPos(out var piLine, out var piColumn));
			TokenInfo tokenInfo = source.GetTokenInfo(piLine, piColumn);
			bool flag = tokenInfo.Type.Equals(TokenType.Delimiter) || tokenInfo.Type.Equals(TokenType.Unknown);
			textmarkerSpan.iEndLine = piLine;
			textmarkerSpan.iEndIndex = (flag ? tokenInfo.StartIndex : (tokenInfo.EndIndex + 1));
			textmarkerSpan.iEndIndex = Math.Max(textmarkerSpan.iStartIndex, textmarkerSpan.iEndIndex);
			int startLine = textmarkerSpan.iStartLine;
			int endLine = textmarkerSpan.iEndLine;
			int startCol = textmarkerSpan.iStartIndex;
			int endCol = textmarkerSpan.iEndIndex;
			if (!ValidateTextMarkerSpan())
			{
				startLine = piLine;
				endLine = piLine;
				startCol = tokenInfo.StartIndex;
				endCol = (flag ? tokenInfo.StartIndex : (tokenInfo.EndIndex + 1));
			}

			string empty = source.GetText(startLine, startCol, endLine, endCol);
			if (StartsWithEscapeSequence(empty))
			{
				string intendedBestMatch = declarations.GetIntendedBestMatch(empty);
				textmarkerSpan.iEndIndex = textmarkerSpan.iStartIndex + intendedBestMatch.Length;
				empty = intendedBestMatch;
			}

			return empty;
		}

		private bool ValidateTextMarkerSpan()
		{
			bool result = true;
			if (textmarkerSpan.iStartLine > textmarkerSpan.iEndLine)
			{
				result = false;
			}

			IVsTextLines textLines = source.GetTextLines();
			textLines.GetLineCount(out var piLineCount);
			if (textmarkerSpan.iStartLine >= 0 && textmarkerSpan.iEndLine < piLineCount)
			{
				textLines.GetLengthOfLine(textmarkerSpan.iStartLine, out var piLength);
				textLines.GetLengthOfLine(textmarkerSpan.iEndLine, out var piLength2);
				if (textmarkerSpan.iStartIndex < 0 || textmarkerSpan.iStartIndex > piLength)
				{
					result = false;
				}

				if (textmarkerSpan.iEndIndex < 0 || textmarkerSpan.iEndIndex > piLength2)
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
			if (SourceAgent.IsExplicitFilteringRequired(textTypedSoFar) || StartsWithEscapeSequence(textTypedSoFar))
			{
				line = textmarkerSpan.iStartLine;
				startIdx = textmarkerSpan.iStartIndex;
				endIdx = textmarkerSpan.iEndIndex;
				return 0;
			}

			return base.GetInitialExtent(out line, out startIdx, out endIdx);
		}

		public override int OnCommitComplete()
		{
			SourceAgent source = this.source as SourceAgent;
			if (source.CurrentCommitUndoTransaction != null)
			{
				source.CurrentCommitUndoTransaction.Complete();
				source.CurrentCommitUndoTransaction = null;
			}

			return base.OnCommitComplete();
		}

		public override void Dismiss()
		{
			/*
			if (Ns.LanguageServiceTestEvents.Instance.EnableTestEvents)
			{
				Ns.LanguageServiceTestEvents.Instance.RaiseSqlCompletionSetDismissedEvent();
			}
			*/

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
			pbstrDescription = string.Empty;
			if (iIndex == 0)
			{
				string firstBindingForCommand = GetFirstBindingForCommand("Edit.ToggleCompletionMode");
				if (!string.IsNullOrEmpty(firstBindingForCommand))
				{
					pbstrDescription = string.Format(CultureInfo.CurrentCulture, Resources.PreviewModeCompletionDescription, firstBindingForCommand);
				}
			}

			return 0;
		}

		public int GetBuilderDisplayText(int iIndex, out string pbstrText, int[] piGlyph = null)
		{
			pbstrText = string.Empty;
			if (base.IsDisplayed)
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
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

			IBPackageController controller = Package.GetGlobalService(typeof(IBPackageController)) as IBPackageController;

			EnvDTE.Commands commands = (controller.Dte as DTE2).Commands;
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
				return string.Empty;
			}

			object[] array = (object[])command.Bindings;
			if (array.Length < 1)
			{
				return string.Empty;
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
}
