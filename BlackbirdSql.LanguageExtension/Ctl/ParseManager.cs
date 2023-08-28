#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;

using Babel;

using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.SqlParser;
using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using Microsoft.SqlServer.Management.SqlParser.Parser;
// using Microsoft.VisualStudio.Data.Tools.SqlEditor;
// using Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Interfaces;

// using Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;


// namespace Microsoft.VisualStudio.Data.Tools.SqlLanguageServices
namespace BlackbirdSql.LanguageExtension
{
	internal class ParseManager
	{
		private string _InputSql;

		private ParseOptions _ParseOptions;

		private IBinder _LastBinderUsed;

		private string _Database;

		private readonly SourceAgent _Source;

		private ParseResult _ParseResult;

		private IEnumerable<Region> _HiddenRegions;

		public ParseResult ParseResult => _ParseResult;

		public IEnumerable<Error> Errors
		{
			get
			{
				if (_ParseResult == null)
				{
					yield break;
				}

				foreach (Error error in _ParseResult.Errors)
				{
					yield return error;
				}
			}
		}

		public IEnumerable<Region> HiddenRegions => _HiddenRegions;

		public ParseManager(SourceAgent source)
		{
			Reset();
			_Source = source;
		}

		public bool ExecuteParseRequest(string text, ParseOptions _ParseOptions, IBinder binder, string _Database)
		{
			bool flag = !_ParseOptions.Equals(this._ParseOptions) || !IsInputSqlSameAs(text);
			bool flag2 = binder != null && _Database != null && (flag || !binder.Equals(_LastBinderUsed) || !_Database.Equals(this._Database));
			if (flag || flag2)
			{
				ParseResult currentResult = null;
				try
				{
					ParseResult prevResult = _ParseResult;
					_ParseResult = null;
					currentResult = Parser.IncrementalParse(text, prevResult, _ParseOptions);
					_InputSql = text;
					this._ParseOptions = _ParseOptions;
					_HiddenRegions = FilterHiddenRegions(Resolver.FindRegionObjects(currentResult));
				}
				catch (SqlParserInternalParserError)
				{
					Reset();
					flag2 = false;
				}

				if (!flag2)
				{
					_ParseResult = currentResult;
				}
				else
				{
					List<ParseResult> parseResults = new(1)
					{
						currentResult
					};

					object f()
					{
						bool flag3 = false;
						try
						{
							binder.Bind(parseResults, _Database, BindMode.Batch);
							flag3 = true;
							_LastBinderUsed = binder;
							this._Database = _Database;
							_ParseResult = currentResult;
						}
						catch (ConnectionException e)
						{
							Diag.Dug(e, "ParseManager::ExecuteParseRequest() bindFunction() - Hit ConnectionException while binding.");
							throw e;
						}
						catch (SqlParserInternalBinderError e2)
						{
							Diag.Dug(e2, "ParseManager::ExecuteParseRequest() bindFunction() - Hit SqlParserInternalBinderError while binding.");
							throw e2;
						}

						return flag3;
					}

					IMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();
					if (metadataProviderProvider != null)
					{
						IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueBindAction(f);
						if (!asyncResult.AsyncWaitHandle.WaitOne(AbstractLanguageService.BinderWaitMilliseconds) || !asyncResult.IsCompleted || asyncResult.AsyncState == null || !(bool)asyncResult.AsyncState)
						{
							_LastBinderUsed = null;
						}
					}
				}
			}

			return flag || flag2;
		}

		private IEnumerable<Region> FilterHiddenRegions(IEnumerable<Region> inputRegions)
		{
			List<Region> list = new List<Region>(inputRegions);
			SortRegionsByStartLine(list);
			int num = 0;
			int i = 1;
			int num2 = 0;
			while (i < list.Count)
			{
				if (list[num].StartLocation.LineNumber == list[i].StartLocation.LineNumber)
				{
					int columnNumber = list[num].StartLocation.ColumnNumber;
					int lineNumber = list[num].EndLocation.LineNumber;
					int columnNumber2 = list[num].EndLocation.ColumnNumber;
					for (; i < list.Count && list[num].StartLocation.LineNumber == list[i].StartLocation.LineNumber; i++)
					{
						if (list[i].StartLocation.ColumnNumber > columnNumber)
						{
							columnNumber = list[i].StartLocation.ColumnNumber;
						}

						if (list[i].EndLocation.LineNumber < lineNumber)
						{
							lineNumber = list[i].EndLocation.LineNumber;
							columnNumber2 = list[i].EndLocation.ColumnNumber;
						}
						else if (list[i].EndLocation.LineNumber == lineNumber && list[i].EndLocation.ColumnNumber > columnNumber2)
						{
							columnNumber2 = list[i].EndLocation.ColumnNumber;
						}
					}

					Location startLocation = new Location(list[num].StartLocation.LineNumber, columnNumber);
					Location endLocation = new Location(lineNumber, columnNumber2);
					list[num2] = new Region(startLocation, endLocation);
				}
				else
				{
					list[num2] = list[num];
				}

				num = i;
				i++;
				num2++;
			}

			if (num < list.Count)
			{
				list[num2] = list[num];
				num2++;
			}

			list.RemoveRange(num2, list.Count - num2);
			return list;
		}

		private static void SortRegionsByStartLine(List<Region> regions)
		{
			regions.Sort((Region x, Region y) => ((Comparison<int>)delegate (int a, int b)
			{
				if (a < b)
				{
					return -1;
				}

				return (a > b) ? 1 : 0;
			})(x.StartLocation.LineNumber, y.StartLocation.LineNumber));
		}

		public void Reset()
		{
			_InputSql = null;
			_ParseOptions = null;
			_LastBinderUsed = null;
			_Database = null;
			_ParseResult = null;
			_HiddenRegions = null;
		}

		private bool IsInputSqlSameAs(string newSql)
		{
			bool result = false;
			if (_InputSql != null)
			{
				result = _InputSql.Length == newSql.Length && _InputSql.Equals(newSql);
			}

			return result;
		}
	}
}
