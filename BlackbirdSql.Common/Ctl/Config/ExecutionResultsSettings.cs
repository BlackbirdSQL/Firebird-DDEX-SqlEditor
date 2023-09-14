// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.ExecutionResultsSettings

using System;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Ctl.Enums;

namespace BlackbirdSql.Common.Ctl.Config;

public sealed class ExecutionResultsSettings : IBQueryExecutionResultsSettings, ICloneable
{
	public static class Defaults
	{
		public static readonly char ColumnDelimiterForText = '\0';

		public static readonly bool DiscardResultsForGrid = false;

		public static readonly bool DiscardResultsForText = false;

		public static readonly bool DisplayResultInSeparateTabForGrid = false;

		public static readonly bool DisplayResultInSeparateTabForText = false;

		public static readonly bool IncludeColumnHeadersWhileSavingGridResults = false;

		public static readonly int MaxCharsPerColumnForGrid = 65535;

		public static readonly int MaxCharsPerColumnForText = 256;

		public static readonly int MaxCharsPerColumnForXml = 2097152;

		public static readonly bool OutputQueryForGrid = false;

		public static readonly bool OutputQueryForText = false;

		public static readonly bool PrintColumnHeadersForText = true;

		public static readonly bool ProvideFeedbackWithSounds = false;

		public static readonly bool QuoteStringsContainingCommas = false;

		public static readonly bool RightAlignNumericsForText = false;

		public static readonly bool ScrollResultsAsReceivedForText = true;

		public static readonly bool ShowAllGridsInTheSameTab = true;

		public static readonly bool ShowGridLinesInMap = true;

		public static readonly bool ShowMessagesInNewTabForText = false;

		public static readonly EnSqlExecutionMode SqlExecutionMode = EnSqlExecutionMode.ResultsToGrid;

		public static readonly bool SwitchToResultsTabAfterQueryExecutesForGrid = true;

		public static readonly bool SwitchToResultsTabAfterQueryExecutesForText = true;

		public static string ResultsDirectory => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
	}

	private char? _columnDelimiterForText;

	private bool? _discardResultsForGrid;

	private bool? _discardResultsForText;

	private bool? _displayResultInSeparateTabForGrid;

	private bool? _displayResultInSeparateTabForText;

	private bool? _includeColumnHeadersWhileSavingGridResults;

	private int? _maxCharsPerColumnForGrid;

	private int? _maxCharsPerColumnForText;

	private int? _maxCharsPerColumnForXml;

	private bool? _outputQueryForGrid;

	private bool? _outputQueryForText;

	private bool? _printColumnHeadersForText;

	private bool? _provideFeedbackWithSounds;

	private bool? _quoteStringsContainingCommas;

	private string _resultsDirectory;

	private bool? _rightAlignNumericsForText;

	private bool? _scrollResultsAsReceivedForText;

	private bool? _showAllGridsInTheSameTab;

	private bool? _showGridLinesInMap;

	private bool? _showMessagesInNewTabForText;

	private EnSqlExecutionMode? _sqlExecutionMode;

	private bool? _switchToResultsTabAfterQueryExecutesForGrid;

	private bool? _switchToResultsTabAfterQueryExecutesForText;

	public char ColumnDelimiterForText
	{
		get
		{
			if (!_columnDelimiterForText.HasValue)
			{
				return Defaults.ColumnDelimiterForText;
			}

			return _columnDelimiterForText.Value;
		}
		set
		{
			_columnDelimiterForText = value;
		}
	}

	public bool DiscardResultsForGrid
	{
		get
		{
			if (!_discardResultsForGrid.HasValue)
			{
				return Defaults.DiscardResultsForGrid;
			}

			return _discardResultsForGrid.Value;
		}
		set
		{
			_discardResultsForGrid = value;
		}
	}

	public bool DiscardResultsForText
	{
		get
		{
			if (!_discardResultsForText.HasValue)
			{
				return Defaults.DiscardResultsForText;
			}

			return _discardResultsForText.Value;
		}
		set
		{
			_discardResultsForText = value;
		}
	}

	public bool DisplayResultInSeparateTabForGrid
	{
		get
		{
			if (!_displayResultInSeparateTabForGrid.HasValue)
			{
				return Defaults.DisplayResultInSeparateTabForGrid;
			}

			return _displayResultInSeparateTabForGrid.Value;
		}
		set
		{
			_displayResultInSeparateTabForGrid = value;
		}
	}

	public bool DisplayResultInSeparateTabForText
	{
		get
		{
			if (!_displayResultInSeparateTabForText.HasValue)
			{
				return Defaults.DisplayResultInSeparateTabForText;
			}

			return _displayResultInSeparateTabForText.Value;
		}
		set
		{
			_displayResultInSeparateTabForText = value;
		}
	}

	public bool IncludeColumnHeadersWhileSavingGridResults
	{
		get
		{
			if (!_includeColumnHeadersWhileSavingGridResults.HasValue)
			{
				return Defaults.IncludeColumnHeadersWhileSavingGridResults;
			}

			return _includeColumnHeadersWhileSavingGridResults.Value;
		}
		set
		{
			_includeColumnHeadersWhileSavingGridResults = value;
		}
	}

	public int MaxCharsPerColumnForGrid
	{
		get
		{
			if (!_maxCharsPerColumnForGrid.HasValue)
			{
				return Defaults.MaxCharsPerColumnForGrid;
			}

			return _maxCharsPerColumnForGrid.Value;
		}
		set
		{
			_maxCharsPerColumnForGrid = value;
		}
	}

	public int MaxCharsPerColumnForText
	{
		get
		{
			if (!_maxCharsPerColumnForText.HasValue)
			{
				return Defaults.MaxCharsPerColumnForText;
			}

			return _maxCharsPerColumnForText.Value;
		}
		set
		{
			_maxCharsPerColumnForText = value;
		}
	}

	public int MaxCharsPerColumnForXml
	{
		get
		{
			if (!_maxCharsPerColumnForXml.HasValue)
			{
				return Defaults.MaxCharsPerColumnForXml;
			}

			return _maxCharsPerColumnForXml.Value;
		}
		set
		{
			_maxCharsPerColumnForXml = value;
		}
	}

	public bool OutputQueryForGrid
	{
		get
		{
			if (!_outputQueryForGrid.HasValue)
			{
				return Defaults.OutputQueryForGrid;
			}

			return _outputQueryForGrid.Value;
		}
		set
		{
			_outputQueryForGrid = value;
		}
	}

	public bool OutputQueryForText
	{
		get
		{
			if (!_outputQueryForText.HasValue)
			{
				return Defaults.OutputQueryForText;
			}

			return _outputQueryForText.Value;
		}
		set
		{
			_outputQueryForText = value;
		}
	}

	public bool PrintColumnHeadersForText
	{
		get
		{
			if (!_printColumnHeadersForText.HasValue)
			{
				return Defaults.PrintColumnHeadersForText;
			}

			return _printColumnHeadersForText.Value;
		}
		set
		{
			_printColumnHeadersForText = value;
		}
	}

	public bool ProvideFeedbackWithSounds
	{
		get
		{
			if (!_provideFeedbackWithSounds.HasValue)
			{
				return Defaults.ProvideFeedbackWithSounds;
			}

			return _provideFeedbackWithSounds.Value;
		}
		set
		{
			_provideFeedbackWithSounds = value;
		}
	}

	public bool QuoteStringsContainingCommas
	{
		get
		{
			if (!_quoteStringsContainingCommas.HasValue)
			{
				return Defaults.QuoteStringsContainingCommas;
			}

			return _quoteStringsContainingCommas.Value;
		}
		set
		{
			_quoteStringsContainingCommas = value;
		}
	}

	public string ResultsDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(_resultsDirectory))
			{
				return Defaults.ResultsDirectory;
			}

			return _resultsDirectory;
		}
		set
		{
			_resultsDirectory = value;
		}
	}

	public bool RightAlignNumericsForText
	{
		get
		{
			if (!_rightAlignNumericsForText.HasValue)
			{
				return Defaults.RightAlignNumericsForText;
			}

			return _rightAlignNumericsForText.Value;
		}
		set
		{
			_rightAlignNumericsForText = value;
		}
	}

	public bool ScrollResultsAsReceivedForText
	{
		get
		{
			if (!_scrollResultsAsReceivedForText.HasValue)
			{
				return Defaults.ScrollResultsAsReceivedForText;
			}

			return _scrollResultsAsReceivedForText.Value;
		}
		set
		{
			_scrollResultsAsReceivedForText = value;
		}
	}

	public bool ShowAllGridsInTheSameTab
	{
		get
		{
			if (!_showAllGridsInTheSameTab.HasValue)
			{
				return Defaults.ShowAllGridsInTheSameTab;
			}

			return _showAllGridsInTheSameTab.Value;
		}
		set
		{
			_showAllGridsInTheSameTab = value;
		}
	}

	public bool ShowGridLinesInMap
	{
		get
		{
			if (!_showGridLinesInMap.HasValue)
			{
				return Defaults.ShowGridLinesInMap;
			}

			return _showGridLinesInMap.Value;
		}
		set
		{
			_showGridLinesInMap = value;
		}
	}

	public bool ShowMessagesInNewTabForText
	{
		get
		{
			if (!_showMessagesInNewTabForText.HasValue)
			{
				return Defaults.ShowMessagesInNewTabForText;
			}

			return _showMessagesInNewTabForText.Value;
		}
		set
		{
			_showMessagesInNewTabForText = value;
		}
	}

	public EnSqlExecutionMode SqlExecutionMode
	{
		get
		{
			if (!_sqlExecutionMode.HasValue)
			{
				return Defaults.SqlExecutionMode;
			}

			return _sqlExecutionMode.Value;
		}
		set
		{
			_sqlExecutionMode = value;
		}
	}

	public bool SwitchToResultsTabAfterQueryExecutesForGrid
	{
		get
		{
			if (!_switchToResultsTabAfterQueryExecutesForGrid.HasValue)
			{
				return Defaults.SwitchToResultsTabAfterQueryExecutesForGrid;
			}

			return _switchToResultsTabAfterQueryExecutesForGrid.Value;
		}
		set
		{
			_switchToResultsTabAfterQueryExecutesForGrid = value;
		}
	}

	public bool SwitchToResultsTabAfterQueryExecutesForText
	{
		get
		{
			if (!_switchToResultsTabAfterQueryExecutesForText.HasValue)
			{
				return Defaults.SwitchToResultsTabAfterQueryExecutesForText;
			}

			return _switchToResultsTabAfterQueryExecutesForText.Value;
		}
		set
		{
			_switchToResultsTabAfterQueryExecutesForText = value;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public void ResetToDefault()
	{
		_columnDelimiterForText = Defaults.ColumnDelimiterForText;
		_discardResultsForGrid = Defaults.DiscardResultsForGrid;
		_discardResultsForText = Defaults.DiscardResultsForText;
		_displayResultInSeparateTabForGrid = Defaults.DisplayResultInSeparateTabForGrid;
		_displayResultInSeparateTabForText = Defaults.DisplayResultInSeparateTabForText;
		_includeColumnHeadersWhileSavingGridResults = Defaults.IncludeColumnHeadersWhileSavingGridResults;
		_maxCharsPerColumnForGrid = Defaults.MaxCharsPerColumnForGrid;
		_maxCharsPerColumnForText = Defaults.MaxCharsPerColumnForText;
		_maxCharsPerColumnForXml = Defaults.MaxCharsPerColumnForXml;
		_outputQueryForGrid = Defaults.OutputQueryForGrid;
		_outputQueryForText = Defaults.OutputQueryForText;
		_printColumnHeadersForText = Defaults.PrintColumnHeadersForText;
		_provideFeedbackWithSounds = Defaults.ProvideFeedbackWithSounds;
		_quoteStringsContainingCommas = Defaults.QuoteStringsContainingCommas;
		_resultsDirectory = Defaults.ResultsDirectory;
		_rightAlignNumericsForText = Defaults.RightAlignNumericsForText;
		_scrollResultsAsReceivedForText = Defaults.ScrollResultsAsReceivedForText;
		_showAllGridsInTheSameTab = Defaults.ShowAllGridsInTheSameTab;
		_showGridLinesInMap = Defaults.ShowGridLinesInMap;
		_showMessagesInNewTabForText = Defaults.ShowMessagesInNewTabForText;
		_sqlExecutionMode = Defaults.SqlExecutionMode;
		_switchToResultsTabAfterQueryExecutesForGrid = Defaults.SwitchToResultsTabAfterQueryExecutesForGrid;
		_switchToResultsTabAfterQueryExecutesForText = Defaults.SwitchToResultsTabAfterQueryExecutesForText;
	}
}
