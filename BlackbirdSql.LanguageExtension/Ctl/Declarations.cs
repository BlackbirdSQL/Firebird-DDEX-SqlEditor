#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.LanguageExtension.Properties;

using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;




namespace BlackbirdSql.LanguageExtension;


internal class Declarations : Microsoft.VisualStudio.Package.Declarations
{
	private List<Declaration> filteredDeclarations;

	// private readonly SourceAgent _Source;

	private readonly IList<Declaration> underlyingDeclarations;

	private static readonly char[] likelyDelimiters = new char[4] { ' ', '\t', '.', '\r' };

	public Declarations()
		: this(new List<Declaration>(), null)
	{
	}

	public Declarations(IList<Declaration> declarations, SourceAgent source)
	{
		Tracer.Trace(GetType(), "Declarations() ctor", "starting...(ThreadName = " + Thread.CurrentThread.Name + ")");
		// _Source = source;
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
		underlyingDeclarations = list;
		filteredDeclarations = new List<Declaration>(list);
		Tracer.Trace(GetType(), "Declarations() ctor", "...ending(ThreadName = " + Thread.CurrentThread.Name + ")");
	}

	public bool Filter(string textSoFar, bool isDisplayed)
	{
		if (textSoFar == null)
		{
			return false;
		}

		if (textSoFar == string.Empty)
		{
			bool num = !isDisplayed || filteredDeclarations.Count != underlyingDeclarations.Count;
			if (num)
			{
				filteredDeclarations.Clear();
				filteredDeclarations.AddRange(underlyingDeclarations);
			}

			return num;
		}

		List<Declaration> list = new List<Declaration>();
		bool result = false;
		foreach (Declaration underlyingDeclaration in underlyingDeclarations)
		{
			if (underlyingDeclaration.Title.ToUpperInvariant().Contains(textSoFar.ToUpperInvariant()))
			{
				list.Add(underlyingDeclaration);
			}
		}

		if (!isDisplayed || (list.Count > 0 && (list.Count != underlyingDeclarations.Count || filteredDeclarations.Count != underlyingDeclarations.Count)))
		{
			if (list.Count > 0)
			{
				filteredDeclarations.Clear();
				filteredDeclarations = list;
			}

			result = true;
		}

		return result;
	}

	public override int GetCount()
	{
		return filteredDeclarations.Count;
	}

	public override string GetDescription(int index)
	{
		/*
		Declaration d = filteredDeclarations[index];
		IMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();
		if (metadataProviderProvider != null)
		{
			object f() => d.Description;

			IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(f);
			if (asyncResult.AsyncWaitHandle.WaitOne(AbstractLanguageService.UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
			{
				return asyncResult.AsyncState as string;
			}
		}
		*/

		return string.Empty;
	}

	public override string GetDisplayText(int index)
	{
		/*
		Declaration d = filteredDeclarations[index];
		IMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();
		if (metadataProviderProvider != null)
		{
			object f() => d.Title;

			IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(f);
			if (asyncResult.AsyncWaitHandle.WaitOne(AbstractLanguageService.UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
			{
				return asyncResult.AsyncState as string;
			}
		}
		*/

		return string.Empty;
	}

	public override int GetGlyph(int index)
	{
		return -1;
		/*
		Declaration d = filteredDeclarations[index];
		DeclarationType declarationType = DeclarationType.Table;
		IMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();
		if (metadataProviderProvider != null)
		{
			object f() => d.Type;

			IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(f);
			if (asyncResult.AsyncWaitHandle.WaitOne(AbstractLanguageService.UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
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
		*/
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
			Tuple<int, bool, Func<string, string, bool>>[] array = new Tuple<int, bool, Func<string, string, bool>>[4]
			{
				new Tuple<int, bool, Func<string, string, bool>>(-1, item2: false, (string a, string b) => a.Equals(b, StringComparison.Ordinal)),
				new Tuple<int, bool, Func<string, string, bool>>(-1, item2: false, (string a, string b) => a.Equals(b, StringComparison.OrdinalIgnoreCase)),
				new Tuple<int, bool, Func<string, string, bool>>(-1, item2: false, (string a, string b) => string.Compare(a, 0, b, 0, a.Length, StringComparison.Ordinal) == 0),
				new Tuple<int, bool, Func<string, string, bool>>(-1, item2: false, (string a, string b) => string.Compare(a, 0, b, 0, a.Length, StringComparison.OrdinalIgnoreCase) == 0)
			};
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

		COMException ex = new("", 1);
		Diag.Dug(ex);
		throw ex;
	}

	public override bool IsMatch(string textSoFar, int index)
	{
		textSoFar = EscapeSequence.UnescapeIdentifier(textSoFar);
		return base.IsMatch(textSoFar, index);
	}

	internal string GetIntendedBestMatch(string typedSoFar)
	{
#pragma warning disable IDE0059 // Unnecessary assignment of a value
		base.GetBestMatch(typedSoFar, out var index, out var uniqueMatch);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
		if (string.IsNullOrEmpty(typedSoFar))
		{
			return "";
		}

		if (EscapeSequence.IdentifyEscapeSequence(typedSoFar) == null || index >= 0)
		{
			return typedSoFar;
		}

		string text = typedSoFar;
		int num = typedSoFar.IndexOfAny(likelyDelimiters);
		if (num != -1)
		{
			text = typedSoFar[..num];
#pragma warning disable IDE0059 // Unnecessary assignment of a value
			base.GetBestMatch(text, out index, out uniqueMatch);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
			num = typedSoFar.IndexOfAny(likelyDelimiters);
			bool flag = true;
			while (num != -1)
			{
				string text2 = typedSoFar[..num];
#pragma warning disable IDE0059 // Unnecessary assignment of a value
				base.GetBestMatch(text2, out var index2, out var uniqueMatch2);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
				if (index2 == -1)
				{
					break;
				}

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
					num = typedSoFar.IndexOfAny(likelyDelimiters, num + 1);
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

		return filteredDeclarations[index].Title;
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

			IBPackageController controller = Package.GetGlobalService(typeof(IBPackageController)) as IBPackageController;

			LanguageExtensionAsyncPackage languagePackage = (LanguageExtensionAsyncPackage)controller.DdexPackage;
			LanguageService languageService = languagePackage.LanguageService;

			if (languageService.GetSource(textView) is SourceAgent source)
			{
				IWpfTextView wpfTextView = languagePackage.EditorAdaptersFactoryService.GetWpfTextView(textView);
				if (wpfTextView != null)
				{
					ITextBuffer textBuffer = wpfTextView.TextBuffer;
					if (languagePackage.TextUndoHistoryRegistry.TryGetHistory(textBuffer, out var history))
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
