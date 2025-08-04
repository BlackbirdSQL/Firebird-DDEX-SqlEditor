// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.ParseManager

using System;
using System.Collections.Generic;
using System.Linq;
using Babel;
using BlackbirdSql.LanguageExtension.Interfaces;
using BlackbirdSql.LanguageExtension.Services;
using Microsoft.SqlServer.Management.SqlParser;
using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using Microsoft.SqlServer.Management.SqlParser.Parser;



namespace BlackbirdSql.LanguageExtension.Ctl;


// =========================================================================================================
//
//										LsbParseManager Class
//
/// <summary>
/// Language service Parser Manager.
/// </summary>
// =========================================================================================================
internal class LsbParseManager
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbParseManager
	// ---------------------------------------------------------------------------------


	public LsbParseManager(LsbSource source)
	{
		Reset();

		_Source = source;
		_ = _Source; // Warning suppression.
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbParseManager
	// =========================================================================================================


	private string _DatabaseName;
	private IEnumerable<Region> _HiddenRegions;
	private string _InputSql;
	private IBinder _LastBinderUsed;
	private ParseOptions _ParseOptions;
	private ParseResult _ParseResult;
	private readonly LsbSource _Source;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbParseManager
	// =========================================================================================================


	internal ParseResult ParseResult => _ParseResult;


	internal IEnumerable<Error> Errors
	{
		get
		{
			if (_ParseResult == null)
				yield break;

			foreach (Error error in _ParseResult.Errors.Cast<Error>())
				yield return error;
		}
	}


	internal IEnumerable<Region> HiddenRegions => _HiddenRegions;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbParseManager
	// =========================================================================================================


	internal bool ExecuteParseRequest(string text, ParseOptions parseOptions, IBinder binder, string databaseName)
	{
		bool flag = !parseOptions.Equals(_ParseOptions) || !IsInputSqlSameAs(text);
		bool flag2 = binder != null && databaseName != null && (flag || !binder.Equals(_LastBinderUsed)
			|| !databaseName.Equals(_DatabaseName));

		if (flag || flag2)
		{
			ParseResult currentResult = null;
			try
			{
				ParseResult prevResult = _ParseResult;
				_ParseResult = null;

				currentResult = Parser.IncrementalParse(text, prevResult, parseOptions);

				_InputSql = text;
				_ParseOptions = parseOptions;

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
				List<ParseResult> parseResults =
				[
					currentResult
				];

				object localfunc()
				{
					bool flag3 = false;
					try
					{
						binder.Bind(parseResults, databaseName, BindMode.Batch);
						flag3 = true;
						_LastBinderUsed = binder;
						_DatabaseName = databaseName;
						_ParseResult = currentResult;
					}
					catch (Exception ex)
					{
						string exceptionName = ex.GetType().Name;

						if (exceptionName.EndsWith("ConnectionException"))
							Diag.Ex(ex, "bindFunction() - Hit ConnectionException while binding.");
						else if (exceptionName.EndsWith("SqlParserInternalBinderError"))
							Diag.Ex(ex, "bindFunction() - Hit SqlParserInternalBinderError while binding.");
						else
							Diag.ThrowException(ex);

					}

					return flag3;
				}
				IBsMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();

				if (metadataProviderProvider != null)
				{
					IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueBindAction(localfunc);

					if (!asyncResult.AsyncWaitHandle.WaitOne(LsbLanguageService.C_BinderWaitMilliseconds) ||
						!asyncResult.IsCompleted || asyncResult.AsyncState == null || !(bool)asyncResult.AsyncState)
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
				Region value = new Region(startLocation, endLocation);
				list[num2] = value;
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
		regions.Sort((Region x, Region y) => ((Comparison<int>)delegate(int a, int b)
		{
			if (a < b)
			{
				return -1;
			}
			return (a > b) ? 1 : 0;
		})(x.StartLocation.LineNumber, y.StartLocation.LineNumber));
	}



	internal void Reset()
	{
		_InputSql = null;
		_ParseOptions = null;
		_LastBinderUsed = null;
		_DatabaseName = null;
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


	#endregion Methods

}
