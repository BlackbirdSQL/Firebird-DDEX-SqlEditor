// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.Declarations
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BlackbirdSql.LanguageExtension.Interfaces;
using BlackbirdSql.LanguageExtension.Properties;
using BlackbirdSql.LanguageExtension.Services;
using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.LanguageExtension.Ctl;

public class LsbDeclarations : Microsoft.VisualStudio.Package.Declarations
{
	private List<Declaration> _FilteredDeclarations;

	private readonly LsbSource _Source;

	private readonly IList<Declaration> _UnderlyingDeclarations;

	private static readonly char[] _SLikelyDelimiters = [' ', '\t', '.', '\r'];

	public LsbDeclarations()
		: this([], null)
	{
	}

	public LsbDeclarations(IList<Declaration> declarations, LsbSource source)
	{
		// TraceUtils.Trace(GetType(), "Declarations() ctor", "starting...(ThreadName = " + Thread.CurrentThread.Name + ")");

		_Source = source;

		List<Declaration> list = new List<Declaration>(declarations);

		static int comparison(Declaration x, Declaration y)
		{
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null && y != null)
			{
				return -1;
			}
			return (y == null) ? 1 : string.Compare(x.Title, y.Title, StringComparison.CurrentCulture);
		}

		list.Sort(comparison);
		_UnderlyingDeclarations = list;
		_FilteredDeclarations = new List<Declaration>(list);

		// TraceUtils.Trace(GetType(), "Declarations() ctor", "...ending(ThreadName = " + Thread.CurrentThread.Name + ")");
	}

	public bool Filter(string textSoFar, bool isDisplayed)
	{
		if (textSoFar == null)
			return false;

		bool result = false;

		if (textSoFar == "")
		{
			if (isDisplayed)
			{
				result = _FilteredDeclarations.Count != _UnderlyingDeclarations.Count;

				if (!result)
					return result;
			}
			else
			{
				result = true;
			}

			_FilteredDeclarations.Clear();
			_FilteredDeclarations.AddRange(_UnderlyingDeclarations);

			return result;
		}

		List<Declaration> list = [];

		foreach (Declaration underlyingDeclaration in _UnderlyingDeclarations)
		{
			if (underlyingDeclaration.Title.ToUpperInvariant().Contains(textSoFar.ToUpperInvariant()))
			{
				list.Add(underlyingDeclaration);
			}
		}

		if (!isDisplayed || (list.Count > 0 && (list.Count != _UnderlyingDeclarations.Count
			|| _FilteredDeclarations.Count != _UnderlyingDeclarations.Count)))
		{
			if (list.Count > 0)
			{
				_FilteredDeclarations.Clear();
				_FilteredDeclarations = list;
			}
			result = true;
		}
		return result;
	}

	public override int GetCount()
	{
		return _FilteredDeclarations.Count;
	}

	public override string GetDescription(int index)
	{
		Declaration declaration = _FilteredDeclarations[index];

		IBsMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();
		if (metadataProviderProvider != null)
		{
			object localfunc() => declaration.Description;

			IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(localfunc);
			if (asyncResult.AsyncWaitHandle.WaitOne(LsbLanguageService.C_UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
			{
				return asyncResult.AsyncState as string;
			}
		}

		return "";
	}

	public override string GetDisplayText(int index)
	{
		Declaration declaration = _FilteredDeclarations[index];

		IBsMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();

		if (metadataProviderProvider != null)
		{
			object localfunc() => declaration.Title;
			IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(localfunc);

			if (asyncResult.AsyncWaitHandle.WaitOne(LsbLanguageService.C_UIThreadWaitMilliseconds)
				&& asyncResult.IsCompleted)
			{
				return asyncResult.AsyncState as string;
			}
		}

		return "";
	}


	public override int GetGlyph(int index)
	{
		Declaration declaration = _FilteredDeclarations[index];
		DeclarationType declarationType = DeclarationType.Table;

		IBsMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();

		if (metadataProviderProvider != null)
		{
			object localfunc() => declaration.Type;
			IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(localfunc);

			if (asyncResult.AsyncWaitHandle.WaitOne(LsbLanguageService.C_UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
			{
				declarationType = (DeclarationType)asyncResult.AsyncState;
			}
		}

		switch (declarationType)
		{
			case DeclarationType.AsymmetricKey:
				return 0;
			case DeclarationType.BuiltInFunction:
			case DeclarationType.ScalarValuedFunction:
			case DeclarationType.UserDefinedAggregate:
				return 6;
			case DeclarationType.Certificate:
				return 1;
			case DeclarationType.Column:
			case DeclarationType.DatePart:
				return 2;
			case DeclarationType.Credential:
				return 3;
			case DeclarationType.Database:
				return 4;
			case DeclarationType.Login:
				return 5;
			case DeclarationType.User:
				return 11;
			case DeclarationType.Schema:
				return 7;
			case DeclarationType.CursorParameter:
			case DeclarationType.CursorVariable:
			case DeclarationType.ScalarParameter:
			case DeclarationType.ScalarVariable:
			case DeclarationType.TableParameter:
			case DeclarationType.TableVariable:
				return 12;
			case DeclarationType.Table:
			case DeclarationType.VirtualTable:
				return 9;
			case DeclarationType.View:
				return 13;
			case DeclarationType.TableValuedFunction:
				return 10;
			case DeclarationType.ExtendedStoredProcedure:
			case DeclarationType.StoredProcedure:
				return 8;
			default:
				return -1;
		}
	}

	public override void GetBestMatch(string textSoFar, out int index, out bool uniqueMatch)
	{
		index = -1;
		uniqueMatch = false;
		base.LastBestMatch = "";
		if (textSoFar != null)
		{
			textSoFar = EscapeSequence.UnescapeIdentifier(textSoFar);
		}
		if (textSoFar != null)
		{
			Tuple<int, bool, Func<string, string, bool>>[] array =
			[
				new Tuple<int, bool, Func<string, string, bool>>(-1, item2: false, (string a, string b) => a.Equals(b, StringComparison.Ordinal)),
				new Tuple<int, bool, Func<string, string, bool>>(-1, item2: false, (string a, string b) => a.Equals(b, StringComparison.OrdinalIgnoreCase)),
				new Tuple<int, bool, Func<string, string, bool>>(-1, item2: false, (string a, string b) => string.Compare(a, 0, b, 0, a.Length, StringComparison.Ordinal) == 0),
				new Tuple<int, bool, Func<string, string, bool>>(-1, item2: false, (string a, string b) => string.Compare(a, 0, b, 0, a.Length, StringComparison.OrdinalIgnoreCase) == 0)
			];

			int count = GetCount();

			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < count; j++)
				{
					string name = GetName(j);
					if (!string.IsNullOrEmpty(name) && array[i].Item3(textSoFar, name))
					{
						if (array[i].Item1 != -1)
						{
							array[i] = new Tuple<int, bool, Func<string, string, bool>>(array[i].Item1, item2: false, array[i].Item3);
							break;
						}
						array[i] = new Tuple<int, bool, Func<string, string, bool>>(j, item2: true, array[i].Item3);
					}
				}
				if (array[i].Item1 > -1)
				{
					break;
				}
			}
			for (int k = 0; k < 4; k++)
			{
				if (array[k].Item1 > -1)
				{
					index = array[k].Item1;
					uniqueMatch = array[k].Item2;
					break;
				}
			}
			return;
		}
		throw new COMException("", 1);
	}

	public override bool IsMatch(string textSoFar, int index)
	{
		textSoFar = EscapeSequence.UnescapeIdentifier(textSoFar);
		return base.IsMatch(textSoFar, index);
	}

	public string GetIntendedBestMatch(string typedSoFar)
	{
		base.GetBestMatch(typedSoFar, out var index, out _);

		if (string.IsNullOrEmpty(typedSoFar))
			return "";

		if (EscapeSequence.IdentifyEscapeSequence(typedSoFar) == null || index >= 0)
			return typedSoFar;

		string text = typedSoFar;
		int num = typedSoFar.IndexOfAny(_SLikelyDelimiters);

		if (num != -1)
		{
			text = typedSoFar[..num];
			base.GetBestMatch(text, out _, out _);
			num = typedSoFar.IndexOfAny(_SLikelyDelimiters);
			bool flag = true;

			while (num != -1)
			{
				string text2 = typedSoFar[..num];
				base.GetBestMatch(text2, out var index2, out _);

				if (index2 == -1)
					break;

				text = text2;

				if (flag)
				{
					num++;
					if (num > typedSoFar.Length)
					{
						num = -1;
					}
				}
				else
				{
					num = typedSoFar.IndexOfAny(_SLikelyDelimiters, num + 1);
				}
				flag = !flag;
			}
		}
		return text;
	}

	public override string GetName(int index)
	{
		if (index < 0)
		{
			return null;
		}
		return _FilteredDeclarations[index].Title;
	}

	public override string OnCommit(IVsTextView textView, string textSoFar, char commitCharacter, int index, ref TextSpan initialExtent)
	{
		string text;

		if (commitCharacter == '\'' && textSoFar.Length == 1 && (textSoFar[0] == 'N' || textSoFar[0] == 'n'))
		{
			text = textSoFar;
		}
		else
		{
			string text2 = GetName(index);
			if (text2 == null)
			{
				return textSoFar;
			}
			EscapeSequence escapeSequence = EscapeSequence.IdentifyEscapeSequence(textSoFar);
			if (escapeSequence == null && EscapeSequence.RequiresEscaping(text2))
			{
				escapeSequence = EscapeSequence.BracketedEscapeSequence;
			}
			if (escapeSequence != null)
			{
				string text3 = escapeSequence.Escape(text2);
				text2 = ((text3[^1] != commitCharacter) ? text3 : text3[..^1]);
			}
			text = text2;
			if (LanguageExtensionPackage.Instance.LanguageSvc is LsbLanguageService languageSvc
				&& languageSvc.GetSource(textView) is LsbSource source)
			{
				IWpfTextView wpfTextView = LanguageExtensionPackage.Instance.EditorAdaptersFactorySvc.GetWpfTextView(textView);
				if (wpfTextView != null)
				{
					ITextBuffer textBuffer = wpfTextView.TextBuffer;
					if (LanguageExtensionPackage.Instance.TextUndoHistoryRegistrySvc.TryGetHistory(textBuffer, out var history))
					{
						source.CurrentCommitUndoTransaction = history.CreateTransaction(Resources.IntellisenseCompletionUndoText);
					}
				}
			}
		}
		return text;
	}

	public override bool IsCommitChar(string textSoFar, int selected, char commitCharacter)
	{
		bool result = false;
		if (IsCommitCharCandidate(commitCharacter))
		{
			EscapeSequence escapeSequence = EscapeSequence.IdentifyEscapeSequence(textSoFar);
			if (escapeSequence != null)
			{
				if ((escapeSequence == EscapeSequence.BracketedEscapeSequence && commitCharacter == ']') || (escapeSequence == EscapeSequence.DoubleQuotedEscapeSequence && commitCharacter == '"') || (escapeSequence == EscapeSequence.SingleQuotedEscapeSequence && commitCharacter == '\''))
				{
					return true;
				}
				if (GetIntendedBestMatch(textSoFar + commitCharacter).Length != textSoFar.Length + 1)
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}
		}
		return result;
	}

	private bool IsCommitCharCandidate(char commitCharacter)
	{
		switch (commitCharacter)
		{
		case '\t':
		case '\n':
		case '\r':
		case ' ':
		case '"':
		case '%':
		case '&':
		case '\'':
		case '(':
		case ')':
		case '*':
		case '+':
		case ',':
		case '-':
		case '.':
		case '/':
		case ':':
		case ';':
		case '<':
		case '=':
		case '>':
		case ']':
		case '^':
		case '|':
		case '~':
			return true;
		default:
			return false;
		}
	}
}
