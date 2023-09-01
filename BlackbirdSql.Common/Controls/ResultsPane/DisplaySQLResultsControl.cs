// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.DisplaySQLResultsControl

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using BlackbirdSql.Common.Config.Interfaces;
using BlackbirdSql.Common.Events;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Controls.ResultsPane;


public class DisplaySQLResultsControl : ISqlQueryExecutionHandler, IQueryExecutionHandler, IDisposable
{
	private enum ResultMessageType
	{
		Normal,
		Warning,
		Error
	}

	private delegate void FlushXMLWriterOrEnsureXMLResultsTabDelegate(ShellBufferWriter writer);

	// private readonly object _thisObject = new object();

	private AbstractQESQLBatchConsumer _BatchConsumer;

	private ResultsToTextOrFileBatchConsumer _resultRedirBatchConsumer;

	private StatisticsControl _clientStatisticsControl;

	protected ServiceProvider _sp;

	protected object _rawSP;

	private ResultsWriter _resultsWriter;

	private ResultsWriter _messagesWriter;

	private ResultsWriter _errorsWriter;

	protected bool _bHasMessages;

	protected bool _bHasTextResults;

	private bool _hadErrorsDuringExecution;

	protected int _totalNumberOfGrids;

	private AuxiliaryDocData _auxDocData;

	protected string _defResultsDirectory = "";

	private const int C_MaxGridResultSets = 10000000;

	// private const int C_MaxExecutionPlanControls = 100;

	// private bool _bExecutionPlanMaxCountExceeded;

	private readonly ResultWindowPane _resultsGridPane;

	private readonly ResultWindowPane _messagePane;

	private readonly ResultWindowPane _textResultsPane;

	private readonly ResultWindowPane _statisticsPane;

	private readonly ResultWindowPane _executionPlanPane;

	// private readonly ResultWindowPane _spatialResultsPane;

	private GridResultsPanel _gridResultsPage;

	private VSTextEditorPanel _textResultsPage;

	private VSTextEditorPanel _textMessagesPage;

	private StatisticsPanel _statisticsPage;

	// private ExecutionPlanPanel _executionPlanPage;

	private Font _fontGridResults = Control.DefaultFont;

	private Color _bkGridColor = SystemColors.Window;

	private Color _foreGridColor = SystemColors.WindowText;

	private Color _selectedCellColor = SystemColors.Highlight;

	private Color _inactiveCellColor = SystemColors.InactiveCaption;

	private Color _nullValueCellColor = SystemColors.InactiveCaption;

	private Color _headerRowColor = SystemColors.InactiveCaption;

	private bool _clearStatisticsControl;

	private QueryExecutor _QueryExecutor;

	public AuxiliaryDocData AuxiliaryDocData
	{
		get
		{
			if (_auxDocData == null && SqlEditorPane != null)
			{
				_auxDocData = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(SqlEditorPane.DocData);
			}

			return _auxDocData;
		}
	}

	public EnSqlExecutionMode SqlExecutionMode
	{
		get
		{
			return AuxiliaryDocData.SqlExecutionMode;
		}
		set
		{
			AuxiliaryDocData.SqlExecutionMode = value;
		}
	}

	public IQueryExecutionResultsSettings ResultsSettings
	{
		get
		{
			return AuxiliaryDocData.ResultsSettings;
		}
		set
		{
		}
	}

	public string DefaultResultsDirectory
	{
		get
		{
			return _defResultsDirectory;
		}
		set
		{
			if (value == null)
			{
				Exception ex = new ArgumentNullException("value");
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}

			Tracer.Trace(GetType(), "DisplaySQLResultsControl.DefaultResultsDirectory", "value = {0}", value);
			_defResultsDirectory = value;
			if (_defResultsDirectory != value)
			{
				if (_textResultsPage != null)
				{
					_textResultsPage.DefaultResultsDirectory = value;
				}

				if (_textMessagesPage != null)
				{
					_textMessagesPage.DefaultResultsDirectory = value;
				}

				if (_gridResultsPage != null)
				{
					_gridResultsPage.DefaultResultsDirectory = value;
				}

				if (_statisticsPage != null)
				{
					_statisticsPage.DefaultResultsDirectory = value;
				}

				/*
				if (_executionPlanPage != null)
				{
					_executionPlanPage.DefaultResultsDirectory = value;
				}
				*/
			}
		}
	}

	public QESQLExecutionOptions SqlExecutionOptions => AuxiliaryDocData.QueryExecutor.ExecutionOptions;

	public ResultWindowPane ExecutionPlanWindowPane => _executionPlanPane;

	public IQESQLBatchConsumer BatchConsumer => _BatchConsumer;

	public bool CanAddMoreGrids => _totalNumberOfGrids < C_MaxGridResultSets;

	private bool CouldNotShowSomeGridResults => _totalNumberOfGrids > C_MaxGridResultSets;

	public ISqlEditorWindowPane SqlEditorPane { get; set; }

	private bool ShouldDiscardResults
	{
		get
		{
			if (AuxiliaryDocData.ResultsSettings.SqlExecutionMode != EnSqlExecutionMode.ResultsToText && AuxiliaryDocData.ResultsSettings.SqlExecutionMode != EnSqlExecutionMode.ResultsToFile || !AuxiliaryDocData.ResultsSettings.DiscardResultsForText)
			{
				if (AuxiliaryDocData.ResultsSettings.SqlExecutionMode == EnSqlExecutionMode.ResultsToGrid)
				{
					return AuxiliaryDocData.ResultsSettings.DiscardResultsForGrid;
				}

				return false;
			}

			return true;
		}
	}

	private bool ShouldOutputQuery
	{
		get
		{
			if (AuxiliaryDocData.SqlExecutionMode != EnSqlExecutionMode.ResultsToText && AuxiliaryDocData.SqlExecutionMode != EnSqlExecutionMode.ResultsToFile || !AuxiliaryDocData.ResultsSettings.OutputQueryForText)
			{
				if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToGrid)
				{
					return AuxiliaryDocData.ResultsSettings.OutputQueryForGrid;
				}

				return false;
			}

			return true;
		}
	}

	private int MaxCharsPerColumn
	{
		get
		{
			if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToText || AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToFile)
			{
				return AuxiliaryDocData.ResultsSettings.MaxCharsPerColumnForText;
			}

			return AuxiliaryDocData.ResultsSettings.MaxCharsPerColumnForGrid;
		}
	}

	private bool AutoSelectResultsTab
	{
		get
		{
			if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToText || AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToFile)
			{
				if (AuxiliaryDocData.ResultsSettings.DisplayResultInSeparateTabForText)
				{
					return AuxiliaryDocData.ResultsSettings.SwitchToResultsTabAfterQueryExecutesForText;
				}

				return false;
			}

			if (AuxiliaryDocData.ResultsSettings.DisplayResultInSeparateTabForGrid)
			{
				return AuxiliaryDocData.ResultsSettings.SwitchToResultsTabAfterQueryExecutesForGrid;
			}

			return false;
		}
	}

	public IVsTextView MessagesPaneTextView => _textMessagesPage.TextView.TextView;

	public IVsTextView TextResultsPaneTextView => _textResultsPage.TextView.TextView;

	private bool WithEstimatedShowplan => SqlExecutionOptions.WithEstimatedExecutionPlan;

	public DisplaySQLResultsControl(ResultWindowPane resultsGridPanel, ResultWindowPane messagePanel, ResultWindowPane textResultsPanel, ResultWindowPane statisticsPanel, ResultWindowPane executionPlanPanel, ResultWindowPane spatialPane, ISqlEditorWindowPane editorPane)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.DisplaySQLResultsControl", "", null);
		if (resultsGridPanel == null || messagePanel == null)
		{
			Exception ex = new ArgumentException("tabControl");
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		_resultsGridPane = resultsGridPanel;
		_messagePane = messagePanel;
		_statisticsPane = statisticsPanel;
		_executionPlanPane = executionPlanPanel;
		_textResultsPane = textResultsPanel;
		// _spatialResultsPane = spatialPane;
		SqlEditorPane = editorPane;
		Initialize(resultsGridPanel, messagePanel);
		AuxiliaryDocData.ResultSettingsChanged += OnResultSettingsChanged;
		AuxiliaryDocData.SqlExecutionModeChanged += OnSqlExecutionModeChanged;
		FontAndColorProviderGridResults.Instance.ColorChanged += OnGridColorChanged;
		FontAndColorProviderGridResults.Instance.FontChanged += OnGridFontChanged;
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool bDisposing)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.Dispose", "", null);
		CleanupGrids();
		UnhookFromEvents();
		if (_sp != null)
		{
			_sp = null;
		}

		if (_rawSP != null)
		{
			_rawSP = null;
		}

		if (bDisposing)
		{
			if (_BatchConsumer != null)
			{
				_BatchConsumer.Dispose();
				_BatchConsumer = null;
			}

			if (_resultRedirBatchConsumer != null)
			{
				_resultRedirBatchConsumer.Dispose();
				_resultRedirBatchConsumer = null;
			}

			if (_textResultsPage != null)
			{
				_textResultsPane.Clear();
				_textResultsPage.Dispose();
				_textResultsPage = null;
			}

			if (_textMessagesPage != null)
			{
				_messagePane.Clear();
				_textMessagesPage.Dispose();
				_textMessagesPage = null;
			}
		}

		if (_clientStatisticsControl != null)
		{
			_clientStatisticsControl = null;
		}

		if (_statisticsPage != null)
		{
			_statisticsPane?.Clear();
			_statisticsPage.Dispose();
			_statisticsPage = null;
		}
		/*
		if (_executionPlanPage != null)
		{
			_executionPlanPane.Clear();
			_executionPlanPage.Dispose();
			_executionPlanPage = null;
		}
		*/
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.Dispose", "calling base class");
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.Dispose", "returning");
	}

	public void ClearResultsTabs()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.ClearResultsTabs", "", null);
		ClearTabs();
	}

	public void SetSite(object sp)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetSite", "", null);
		if (_sp != null)
		{
			_sp = null;
		}

		_sp = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)sp);
		_rawSP = sp;
		OnHosted();
	}

	private void OnSqlExecutionModeChanged(object sender, AuxiliaryDocData.SqlExecutionModeChangedEventArgs sqlExecutionModeArgs)
	{
		EnSqlExecutionMode sqlExecutionMode = sqlExecutionModeArgs.SqlExecutionMode;
		ProcessSqlExecMode(sqlExecutionMode);
		ApplyResultSettingsToBatchConsumer(_BatchConsumer, AuxiliaryDocData.ResultsSettings);
	}

	private void OnResultSettingsChanged(object sender, AuxiliaryDocData.ResultsSettingsChangedEventArgs resultSettingsChangedArgs)
	{
		OnSqlExecutionModeChanged(sender, new AuxiliaryDocData.SqlExecutionModeChangedEventArgs(AuxiliaryDocData.SqlExecutionMode));
		_gridResultsPage.SetGridTabOptions(AuxiliaryDocData.ResultsSettings.IncludeColumnHeadersWhileSavingGridResults, AuxiliaryDocData.ResultsSettings.QuoteStringsContainingCommas);
		DefaultResultsDirectory = AuxiliaryDocData.QueryExecutor.QueryExecutionSettings.ExecutionResults.ResultsDirectory;
	}

	private void OnGridColorChanged(object sender, ColorChangedEventArgs args)
	{
		if (args.ItemName.Equals(FontAndColorProviderGridResults.GridCell, StringComparison.OrdinalIgnoreCase))
		{
			SetGridResultsColors(args.BkColor, args.FgColor);
		}
		else if (args.ItemName.Equals(FontAndColorProviderGridResults.SelectedCell, StringComparison.OrdinalIgnoreCase))
		{
			SetGridSelectedCellColor(args.BkColor);
		}
		else if (args.ItemName.Equals(FontAndColorProviderGridResults.SelectedCellInactive, StringComparison.OrdinalIgnoreCase))
		{
			SetGridInactiveSelectedCellColor(args.BkColor);
		}
		else if (args.ItemName.Equals(FontAndColorProviderGridResults.NullValueCell, StringComparison.OrdinalIgnoreCase))
		{
			SetGridNullValueColor(args.BkColor);
		}
		else if (args.ItemName.Equals(FontAndColorProviderGridResults.HeaderRow, StringComparison.OrdinalIgnoreCase))
		{
			SetHeaderRowColor(args.BkColor);
		}
	}

	private void OnGridFontChanged(object sender, FontChangedEventArgs args)
	{
		SetGridResultsFont(args.Font);
	}

	public bool PrepareForExecution(bool prepareForParse)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.PrepareForExecution", "prepareForParse = {0}", prepareForParse);
		(((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(SqlEditorPane.DocData)).QueryExecutor.ResultsHandler = BatchConsumer;
		ResultsWriter resultsWriter = null;
		if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToFile && !prepareForParse && !AuxiliaryDocData.ResultsSettings.DiscardResultsForText && !WithEstimatedShowplan)
		{
			StreamWriter textWriterForQueryResultsToFile = CommonUtils.GetTextWriterForQueryResultsToFile(xmlResults: false, ref _defResultsDirectory);
			if (textWriterForQueryResultsToFile == null)
			{
				return false;
			}

			resultsWriter = new FileStreamResultsWriter(textWriterForQueryResultsToFile);
		}

		Clear();
		PrepareTabs(prepareForParse);
		_textResultsPage.UndoEnabled = false;
		_textMessagesPage.UndoEnabled = false;
		if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToText || ShouldDiscardResults || WithEstimatedShowplan)
		{
			resultsWriter = _textResultsPage.ResultsWriter;
		}

		_resultsWriter = resultsWriter;
		_messagesWriter = _textMessagesPage.ResultsWriter;
		_errorsWriter = _textMessagesPage.ResultsWriter;
		return true;
	}

	public void SetGridResultsFont(Font f)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridResultsFont", "", null);
		if (f == null)
		{
			Exception ex = new ArgumentNullException("f");
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		_fontGridResults = f;
		_gridResultsPage.ApplyCurrentGridFont(_fontGridResults);
		_statisticsPage?.ApplyCurrentGridFont(_fontGridResults);
	}

	public void SetGridResultsColors(Color? bkColor, Color? fkColor)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridResultsColors", "bkColor = {0}, fkColor = {1}", bkColor, fkColor);
		if (bkColor.HasValue)
		{
			_bkGridColor = bkColor.Value;
		}
		else
		{
			_bkGridColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexWindowBkColor);
		}

		if (fkColor.HasValue)
		{
			_foreGridColor = fkColor.Value;
		}
		else
		{
			_foreGridColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexWindowTextColor);
		}

		if (_bkGridColor != Color.Empty && _foreGridColor != Color.Empty)
		{
			_gridResultsPage.ApplyCurrentGridColor(_bkGridColor, _foreGridColor);
			_statisticsPage?.ApplyCurrentGridColor(_bkGridColor, _foreGridColor);
		}
	}

	public void SetGridSelectedCellColor(Color? selectedCellColor)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridSelectedCellColor", "color = {0}", selectedCellColor);
		if (selectedCellColor.HasValue)
		{
			_selectedCellColor = selectedCellColor.Value;
		}
		else
		{
			_selectedCellColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexSelected);
		}

		if (_selectedCellColor != Color.Empty)
		{
			_gridResultsPage.ApplySelectedCellColor(_selectedCellColor);
			_statisticsPage?.ApplySelectedCellColor(_selectedCellColor);
		}
	}

	public void SetGridInactiveSelectedCellColor(Color? inactiveSelectedCellColor)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridInactiveSelectedCellColor", "color = {0}", inactiveSelectedCellColor);
		if (inactiveSelectedCellColor.HasValue)
		{
			_inactiveCellColor = inactiveSelectedCellColor.Value;
		}
		else
		{
			_inactiveCellColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexSelectedInactive);
		}

		if (_inactiveCellColor != Color.Empty)
		{
			_gridResultsPage.ApplyInactiveSelectedCellColor(_inactiveCellColor);
			_statisticsPage?.ApplyInactiveSelectedCellColor(_inactiveCellColor);
		}
	}

	public void SetGridNullValueColor(Color? nullValueCellColor)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridNullValueColor", "color = {0}", nullValueCellColor);
		if (nullValueCellColor.HasValue)
		{
			_nullValueCellColor = nullValueCellColor.Value;
		}
		else
		{
			_nullValueCellColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexNullCell);
		}

		if (_nullValueCellColor != Color.Empty)
		{
			_gridResultsPage.ApplyHighlightedCellColor(_nullValueCellColor);
		}
	}

	public void SetHeaderRowColor(Color? headerRowColor)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetHeaderRowColor", "color = {0}", headerRowColor);
		if (headerRowColor.HasValue)
		{
			_headerRowColor = headerRowColor.Value;
		}
		else
		{
			_headerRowColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexHeaderRow);
		}

		if (_headerRowColor != Color.Empty && _statisticsPage != null)
		{
			_statisticsPage.ApplyHighlightedCellColor(_headerRowColor);
		}
	}

	public void ActivateControl(EnPaneSelection selectPane)
	{
	}

	private void OnQEOLESQLErrorMessage(object sender, QEOLESQLErrorMessageEventArgs args)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnQEOLESQLErrorMessage", "", null);
		if (args.MessageType != EnQESQLScriptProcessingMessageType.Warning)
		{
			AddStringToErrors(args.DetailedMessage, args.Line, args.TextSpan, flush: false);
			AddStringToErrors(args.DescriptionMessage, args.Line, args.TextSpan, flush: true);
		}
		else
		{
			AddStringToMessages(args.DetailedMessage, flush: false);
			AddStringToMessages(args.DescriptionMessage, flush: true);
		}
	}

	protected virtual void OnHosted()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnHosted", "", null);
		_gridResultsPage.Initialize(_rawSP);
		_textResultsPage.Initialize(_rawSP);
		_textMessagesPage.Initialize(_rawSP);
	}

	private void Initialize(ResultWindowPane gridResultsPanel, ResultWindowPane messagePanel)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.Initialize", "", null);
		ProcessSqlExecMode(AuxiliaryDocData.SqlExecutionMode);
		ApplyResultSettingsToBatchConsumer(_BatchConsumer, AuxiliaryDocData.ResultsSettings);
		_ = DefaultResultsDirectory = AuxiliaryDocData.ResultsSettings.ResultsDirectory;
		_gridResultsPage = AllocateNewGridTabPage();
		_gridResultsPage.Name = "_gridResultsPage";
		_textResultsPage = new(_defResultsDirectory, xmlEditor: false)
		{
			Name = "_textResultsPage"
		};
		_textMessagesPage = new(_defResultsDirectory, xmlEditor: false)
		{
			Name = "_textMessagesPage"
		};
		AuxiliaryDocData auxillaryDocData = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(SqlEditorPane.DocData);
		RegisterToQueryExecutorEvents(auxillaryDocData.QueryExecutor);

		IVsFontAndColorStorage vsFontAndColorStorage = ((AsyncPackage)Controller.Instance.DdexPackage).GetService<SVsFontAndColorStorage, IVsFontAndColorStorage>();

		if (AbstractFontAndColorProvider.GetFontAndColorSettingsForCategory(VS.CLSID_FontAndColorsSqlResultsGridCategory,
			FontAndColorProviderGridResults.GridCell, vsFontAndColorStorage, out var categoryFont, out var foreColor,
			out var bkColor, readFont: true))
		{
			if (categoryFont != null)
			{
				SetGridResultsFont(categoryFont);
			}

			SetGridResultsColors(bkColor, foreColor);
		}

		if (AbstractFontAndColorProvider.GetFontAndColorSettingsForCategory(VS.CLSID_FontAndColorsSqlResultsGridCategory,
			FontAndColorProviderGridResults.SelectedCell, vsFontAndColorStorage, out _, out _,
			out bkColor, readFont: false))
		{
			SetGridSelectedCellColor(bkColor);
		}

		if (AbstractFontAndColorProvider.GetFontAndColorSettingsForCategory(VS.CLSID_FontAndColorsSqlResultsGridCategory,
			FontAndColorProviderGridResults.SelectedCellInactive, vsFontAndColorStorage, out _, out _,
			out bkColor, readFont: false))
		{
			SetGridInactiveSelectedCellColor(bkColor);
		}

		if (AbstractFontAndColorProvider.GetFontAndColorSettingsForCategory(VS.CLSID_FontAndColorsSqlResultsGridCategory,
			FontAndColorProviderGridResults.NullValueCell, vsFontAndColorStorage, out _, out _,
			out bkColor, readFont: false))
		{
			SetGridNullValueColor(bkColor);
		}

		if (AbstractFontAndColorProvider.GetFontAndColorSettingsForCategory(VS.CLSID_FontAndColorsSqlResultsGridCategory,
			FontAndColorProviderGridResults.HeaderRow, vsFontAndColorStorage, out _, out _,
			out bkColor, readFont: false))
		{
			SetHeaderRowColor(bkColor);
		}
	}

	public void AddStringToResults(string message, bool flush)
	{
		AddStringToTextWriterCommon(message, -1, null, ResultMessageType.Normal, _resultsWriter, flush, noCr: false);
		_bHasTextResults = true;
	}

	public void AddStringToMessages(string message, bool flush)
	{
		AddMessageToTextWriterCommon(message, -1, null, ResultMessageType.Normal, _messagesWriter, flush, noCr: false);
	}

	public void AddStringToErrors(string message, bool flush)
	{
		AddStringToErrors(message, -1, null, flush);
	}

	public void AddStringToErrors(string message, int line, ITextSpan textSpan, bool flush)
	{
		_hadErrorsDuringExecution = true;
		AddMessageToTextWriterCommon(message, line, textSpan, ResultMessageType.Error, _errorsWriter, flush, noCr: false);
	}

	public void AddResultSetSeparatorMsg()
	{
		AddStringToResults("", flush: true);
	}

	public void AddGridContainer(ResultSetAndGridContainer grid)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.AddGridContainer", "", null);

#pragma warning disable CS0618 // Type or member is obsolete
		ThreadHelper.Generic.Invoke(delegate
		{
			AddGridContainerInt(grid);
		});
#pragma warning restore CS0618 // Type or member is obsolete

		/*
		Task task = Task.Factory.StartNew(delegate
		{
			AddGridContainerInt(grid);
		},
		default, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
		*/
	}

	public void MarkAsCouldNotAddMoreGrids()
	{
		_totalNumberOfGrids = 10000001;
	}

	/*
	public void ProcessSpecialActionOnBatch(QESQLBatchSpecialActionEventArgs args)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSpecialActionOnBatch", "", null);
		IGraph[] graphs;
		object dataSource;
		try
		{
			GetExecutionPlanGraphs(args.DataReader, args.Action, out graphs, out dataSource);
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
			return;
		}

		Task task = Task.Factory.StartNew(delegate
		{
			ProcessSpecialActionOnBatchInt(args.Action, graphs, dataSource);
		},
		default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

		// ThreadHelper.Generic.Invoke(delegate
		// {
		//	ProcessSpecialActionOnBatchInt(args.Action, graphs, dataSource);
		// });
	}
	*/

	/*
	private void ProcessSpecialActionOnBatchInt(QESQLBatchSpecialAction action, IGraph[] graphs, object dataSource)
	{
		//IL_00bf: Expected O, but got Unknown
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSpecialActionOnBatchInt", "", null);
		try
		{
			if (_executionPlanPage == null)
			{
				_executionPlanPage = new(_defResultsDirectory)
				{
					Dock = DockStyle.Fill
				};
				_executionPlanPage.Initialize(_rawSP);
				((IObjectWithSite)_executionPlanPage.ExecutionPlanControl).SetSite(_executionPlanPane);
			}

			try
			{
				if (_executionPlanPage.ExecutionPlanControl.GraphPanelCount < C_MaxExecutionPlanControls)
				{
					_executionPlanPage.AddGraphs(graphs, dataSource);
				}
				else
				{
					_bExecutionPlanMaxCountExceeded = true;
				}
			}
			finally
			{
				if (_executionPlanPage.ExecutionPlanControl.GraphPanelCount == 0)
				{
					SqlEditorPane.ActivateMessageTab();
				}
			}
		}
		catch (NullReferenceException e)
		{
			Tracer.LogExCatch(GetType(), e);
		}
		catch (FbException val)
		{
			FbException e2 = val;
			Tracer.LogExCatch(GetType(), (Exception)(object)e2);
		}
		catch (ArgumentException e3)
		{
			Tracer.LogExCatch(GetType(), e3);
		}
		catch (OutOfMemoryException e4)
		{
			Tracer.LogExCatch(GetType(), e4);
		}
		catch (InvalidOperationException e5)
		{
			Tracer.LogExCatch(GetType(), e5);
		}
		catch (ApplicationException ex)
		{
			Tracer.LogExCatch(GetType(), ex);
			AddStringToErrors(ex.Message, flush: true);
		}
	}
	*/

	public void ProcessNewXml(string xmlString, bool cleanPreviousResults)
	{
		NotSupportedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}

	/*
	private void GetExecutionPlanGraphs(IDataReader dataReader, QESQLBatchSpecialAction batchSpecialAction, out IGraph[] graphs, out object dataSource)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		object executionPlanData = GetExecutionPlanData(dataReader, batchSpecialAction);
		ExecutionPlanType val = (ExecutionPlanType)((batchSpecialAction & QESQLBatchSpecialAction.ExpectActualExecutionPlan) != 0 ? 1 : 2);
		try
		{
			INodeBuilder val2 = NodeBuilderFactory.Create(executionPlanData, val);
			IGraph[] array = graphs = (IGraph[])(object)val2.Execute(executionPlanData);
			dataSource = executionPlanData;
		}
		catch (Exception ex)
		{
			string text = ex.Message;
			if (ex.InnerException != null && !text.Contains(ex.InnerException.Message))
			{
				text += "\r\n";
				text += ex.InnerException.Message;
			}

			ApplicationException ex = new(string.Format(CultureInfo.CurrentCulture, SharedResx.FailedToReadExecutionPlan, text), ex);
			Diag.Dug(ex);
			throw ex;
		}
	}

	private object GetExecutionPlanData(IDataReader dataReader, QESQLBatchSpecialAction batchSpecialAction)
	{
		// object obj = null;

		if ((batchSpecialAction & QESQLBatchSpecialAction.ExpectYukonXmlExecutionPlan) != 0)
		{
			if (!dataReader.Read())
			{
				InvalidOperationException ex = new(SharedResx.CannotFindDataForExecutionPlan);
				Diag.Dug(ex);
				throw ex;
			}

			return dataReader.GetString(0);
		}

		return dataReader;
	}
	*/

	private void AddMessageToTextWriterCommon(string message, int line, ITextSpan textSpan, ResultMessageType resultMessageType, ResultsWriter writer, bool flush, bool noCr)
	{
		AddStringToTextWriterCommon(message, line, textSpan, resultMessageType, writer, flush, noCr);
		if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToFile && _resultsWriter != null && _resultsWriter != writer)
		{
			_resultsWriter.AppendNormal(message, noCr);
		}
	}

	private void AddStringToTextWriterCommon(string message, int line, ITextSpan textSpan, ResultMessageType resultMessageType, ResultsWriter writer, bool flush, bool noCr)
	{
		if (message == null)
		{
			return;
		}

		switch (resultMessageType)
		{
			case ResultMessageType.Normal:
				writer.AppendNormal(message, noCr);
				break;
			case ResultMessageType.Warning:
				writer.AppendWarning(message, noCr);
				break;
			case ResultMessageType.Error:
				writer.AppendError(message, line, textSpan, noCr);
				break;
			default:
				writer.AppendNormal(message, noCr);
				break;
		}

		if (!flush)
		{
			return;
		}

		if (writer is FileStreamResultsWriter)
		{
			writer.Flush();
			return;
		}

		_bHasMessages = true;

#pragma warning disable CS0618 // Type or member is obsolete
		ThreadHelper.Generic.Invoke(delegate
		{
			FlushTextWritersInt(writer);
		});
#pragma warning restore CS0618 // Type or member is obsolete

		/*
 		Task task = Task.Factory.StartNew(delegate
		{
			FlushTextWritersInt(writer);
		},
		default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
		*/
	}

	private void Clear()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.Clear", "", null);
		CheckAndCloseTextWriters();
		_textResultsPage.Clear();
		_textMessagesPage.Clear();
		_bHasMessages = false;
		_hadErrorsDuringExecution = false;
		_bHasTextResults = false;
		_BatchConsumer?.Cleanup();

		_resultRedirBatchConsumer?.Cleanup();

		RemoveStatisticsPage();
		CleanupGrids();
	}

	public void RemoveStatisticsPage()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.RemoveStatisticsTab", "", null);
		if (_statisticsPage != null && _statisticsPane != null && _statisticsPane.Contains(_statisticsPage))
		{
			_statisticsPane.Remove(_statisticsPage);
		}
	}

	private void CleanupGrids()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.CleanupGrids", "", null);
		_totalNumberOfGrids = 0;
		_gridResultsPage?.Clear();
	}

	private void UnhookFromEvents()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.UnhookFromEvents", "", null);
		UnRegisterQueryExecutorEvents();
		AuxiliaryDocData.SqlExecutionModeChanged -= OnSqlExecutionModeChanged;
		AuxiliaryDocData.ResultSettingsChanged -= OnResultSettingsChanged;
		FontAndColorProviderGridResults.Instance.ColorChanged -= OnGridColorChanged;
		FontAndColorProviderGridResults.Instance.FontChanged -= OnGridFontChanged;
	}

	private void PrepareTabs(bool isParseOnly)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.PrepareTabs", "", null);
		ClearTabs();
		SqlEditorPane.CustomizeTabsForResultsSetting(isParseOnly);
		if (!_textResultsPane.Contains(_textResultsPage))
		{
			_textResultsPage.Dock = DockStyle.Fill;
			_textResultsPage.AutoSize = true;
			_textResultsPane.Add(_textResultsPage);
		}

		if (!_messagePane.Contains(_textMessagesPage))
		{
			_textMessagesPage.Dock = DockStyle.Fill;
			_textMessagesPage.AutoSize = true;
			_messagePane.Add(_textMessagesPage);
		}

		if (!_resultsGridPane.Contains(_gridResultsPage))
		{
			_gridResultsPage.Dock = DockStyle.Fill;
			_resultsGridPane.Add(_gridResultsPage);
		}

		_textResultsPage.TextReadOnly = false;
		_textMessagesPage.TextReadOnly = false;
	}

	private void OutputQueryIntoMessages(string strScript)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.OutputQueryIntoMessages", "", null);
		AddStringToMessages("/*------------------------", flush: false);
		AddStringToMessages(strScript, flush: false);
		AddStringToMessages("------------------------*/", flush: true);
	}

	private void FlushTextWritersInt(ResultsWriter textWriter)
	{
		if (textWriter != null)
		{
			textWriter.Flush();
			if (AuxiliaryDocData.ResultsSettings.ScrollResultsAsReceivedForText && AuxiliaryDocData.ResultsSettings.SqlExecutionMode == EnSqlExecutionMode.ResultsToText && textWriter == _textResultsPage.ResultsWriter)
			{
				_textResultsPage.ScrollTextViewToMaxScrollUnit();
			}
		}
	}

	private void AddGridContainerInt(ResultSetAndGridContainer cont)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.AddGridContainerInt", "", null);
		if (_totalNumberOfGrids == C_MaxGridResultSets)
		{
			MarkAsCouldNotAddMoreGrids();
			Exception ex = new QESQLBatchConsumerException(string.Format(CultureInfo.CurrentCulture, SharedResx.CanDisplayOnlyNGridResults, C_MaxGridResultSets), QESQLBatchConsumerException.ErrorType.CannotShowMoreResults);
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		_totalNumberOfGrids++;
		GridResultsPanel gridResultsPanel = null;
		if (AuxiliaryDocData.ResultsSettings.ShowAllGridsInTheSameTab || !AuxiliaryDocData.ResultsSettings.ShowAllGridsInTheSameTab && _gridResultsPage.NumberOfGrids == 0)
		{
			gridResultsPanel = _gridResultsPage;
		}

		gridResultsPanel.AddGridContainer(cont, _fontGridResults, _bkGridColor, _foreGridColor, _selectedCellColor, _inactiveCellColor);
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.AddGridContainerInt", "returning");
	}

	private void ProcessSqlExecMode(EnSqlExecutionMode mode)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSqlExecMode", "", null);
		if (!Enum.IsDefined(typeof(EnSqlExecutionMode), mode))
		{
			Exception ex = new ArgumentOutOfRangeException("mode");
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		if (_BatchConsumer != null)
		{
			_BatchConsumer.Dispose();
			_BatchConsumer = null;
		}

		switch (mode)
		{
			case EnSqlExecutionMode.ResultsToText:
			case EnSqlExecutionMode.ResultsToFile:
				_BatchConsumer = new ResultsToTextOrFileBatchConsumer(this);
				break;
			case EnSqlExecutionMode.ResultsToGrid:
				_BatchConsumer = new ResultsToGridBatchConsumer(this);
				break;
			default:
				_BatchConsumer = new ResultsToGridBatchConsumer(this);
				break;
		}
	}

	private void ApplyResultSettingsToBatchConsumer(AbstractQESQLBatchConsumer batchConsumer, IQueryExecutionResultsSettings resultsSettings)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.ApplyResSettingsToBatchConsumer", "", null);
		batchConsumer.MaxCharsPerColumn = MaxCharsPerColumn;
		batchConsumer.DiscardResults = ShouldDiscardResults;
		if (batchConsumer is ResultsToTextOrFileBatchConsumer obj)
		{
			obj.ColumnsDelimiter = resultsSettings.ColumnDelimiterForText;
			obj.PrintColumnHeaders = resultsSettings.PrintColumnHeadersForText;
			obj.RightAlignNumerics = resultsSettings.RightAlignNumericsForText;
		}
	}

	private GridResultsPanel AllocateNewGridTabPage()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.AllocateNewGridTabPage", "", null);
		GridResultsPanel gridResultsPanel = new(_defResultsDirectory)
		{
			Name = "GridResultsPanel",
			BackColor = SystemColors.Window
		};
		gridResultsPanel.ApplyHighlightedCellColor(_nullValueCellColor);
		if (_rawSP != null)
		{
			gridResultsPanel.Initialize(_rawSP);
		}

		gridResultsPanel.SetGridTabOptions(AuxiliaryDocData.ResultsSettings.IncludeColumnHeadersWhileSavingGridResults, AuxiliaryDocData.ResultsSettings.QuoteStringsContainingCommas);
		return gridResultsPanel;
	}

	private void ClearTabs()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.ClearTabs", "", null);
		CleanupGrids();
		/*
		if (_executionPlanPage != null)
		{
			_executionPlanPane.Remove(_executionPlanPage);
			_executionPlanPage.Dispose();
			_executionPlanPage = null;
		}
		*/
	}

	private void FlushAllTextWriters()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.FlushAllTextWriters", "", null);
		try
		{
			if (_resultsWriter != null)
			{
				FlushTextWritersInt(_resultsWriter);
			}
		}
		catch
		{
		}

		if (_messagesWriter != _resultsWriter && _messagesWriter != null)
		{
			try
			{
				_messagesWriter.Flush();
			}
			catch
			{
			}
		}

		if (_errorsWriter != _messagesWriter && _errorsWriter != _resultsWriter && _errorsWriter != null)
		{
			try
			{
				_errorsWriter.Flush();
			}
			catch
			{
			}
		}

		if (_messagesWriter != _textResultsPage.ResultsWriter && _errorsWriter != _textResultsPage.ResultsWriter)
		{
			try
			{
				_textResultsPage.ResultsWriter.Flush();
			}
			catch
			{
			}
		}
	}

	private void CheckAndCloseTextWriters()
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.CheckAndCloseTextWriters", "", null);
		if (_resultsWriter != null && _resultsWriter is FileStreamResultsWriter)
		{
			try
			{
				_resultsWriter.Close();
			}
			catch
			{
			}
		}

		if (_messagesWriter != null && _messagesWriter != _resultsWriter && _messagesWriter is FileStreamResultsWriter)
		{
			try
			{
				_messagesWriter.Close();
			}
			catch
			{
			}
		}

		if (_errorsWriter != null && _errorsWriter != _resultsWriter && _errorsWriter != _messagesWriter && _errorsWriter is FileStreamResultsWriter)
		{
			try
			{
				_errorsWriter.Close();
			}
			catch
			{
			}
		}

		_resultsWriter = _messagesWriter = _errorsWriter = null;
	}

	public static bool IsRunningOnUIThread()
	{
		return ThreadHelper.CheckAccess();
	}

	public void RegisterToQueryExecutorEvents(QueryExecutor queryExecutor)
	{
		if (_QueryExecutor == null)
		{
			_QueryExecutor = queryExecutor;
			_QueryExecutor.ScriptExecutionStarted += OnScriptExecutionStarted;
			_QueryExecutor.ScriptExecutionCompleted += OnSqlExecutionCompletedForSQLExec;
			_QueryExecutor.ScriptExecutionErrorMessage += OnQEOLESQLErrorMessage;
			_QueryExecutor.StatusChanged += OnQueryExecutorStatusChanged;
			_QueryExecutor.SqlCmdOutputRedirection += OnSqlCmdOutputRedirection;
			_QueryExecutor.SqlCmdMessageFromApp += OnSqlCmdMsgFromApp;
		}
	}

	private void UnRegisterQueryExecutorEvents()
	{
		if (_QueryExecutor != null)
		{
			_QueryExecutor.ScriptExecutionStarted -= OnScriptExecutionStarted;
			_QueryExecutor.ScriptExecutionCompleted -= OnSqlExecutionCompletedForSQLExec;
			_QueryExecutor.ScriptExecutionErrorMessage -= OnQEOLESQLErrorMessage;
			_QueryExecutor.StatusChanged -= OnQueryExecutorStatusChanged;
			_QueryExecutor.SqlCmdOutputRedirection -= OnSqlCmdOutputRedirection;
			_QueryExecutor.SqlCmdMessageFromApp -= OnSqlCmdMsgFromApp;
			_QueryExecutor = null;
		}
	}

	private bool OnScriptExecutionStarted(object sender, QueryExecutor.ScriptExecutionStartedEventArgs args)
	{
		if (AuxiliaryDocData.ClientStatisticsEnabled && !args.IsParseOnly)
		{
			FbConnection asSqlConnection = (FbConnection)args.Connection;
			if (args.Connection != null && (_clientStatisticsControl == null || _clearStatisticsControl))
			{
				_clientStatisticsControl = new StatisticsControl();
				StatisticsConnection node = new StatisticsConnection(asSqlConnection);
				_clientStatisticsControl.Add(node);
				_clearStatisticsControl = false;
			}
		}

		bool num = PrepareForExecution(args.IsParseOnly);
		if (num && ShouldOutputQuery && !args.IsParseOnly)
		{
			OutputQueryIntoMessages(args.QueryText);
		}

		// _bExecutionPlanMaxCountExceeded = false;
		return num;
	}

	private void OnSqlExecutionCompletedForSQLExec(object sender, ScriptExecutionCompletedEventArgs args)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlExecutionCompletedForSQLExec", "", null);
		if (!args.IsParseOnly && AuxiliaryDocData.ClientStatisticsEnabled && _clientStatisticsControl != null)
		{
			_clientStatisticsControl.RetrieveStatisticsIfNeeded();
		}

#pragma warning disable CS0618 // Type or member is obsolete
		ThreadHelper.Generic.Invoke(delegate
		{
			OnSqlExecutionCompletedInt(sender, args);
		});
#pragma warning restore CS0618 // Type or member is obsolete


		/*
		Task task = Task.Factory.StartNew(delegate
		{
			OnSqlExecutionCompletedInt(sender, args);
		},
		default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
		*/
	}

	private void OnSqlExecutionCompletedInt(object sender, ScriptExecutionCompletedEventArgs args)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlExecutionCompletedInt", "ExecResult = {0}", args.ExecutionResult);
		/*
		if (_executionPlanPage != null && !_executionPlanPane.Contains(_executionPlanPage))
		{
			if (_executionPlanPage.ExecutionPlanControl.GraphPanelCount > 0)
			{
				_executionPlanPane.Add(_executionPlanPage);
			}

			if (_bExecutionPlanMaxCountExceeded)
			{
				_textMessagesPage.ResultsWriter.AppendNormal(string.Format(CultureInfo.CurrentCulture, SharedResx.CanDisplayOnlyNExecutionPlanControls, C_MaxExecutionPlanControls), noCRLF: true);
			}
		}
		*/

		if (AuxiliaryDocData.ClientStatisticsEnabled && _clientStatisticsControl != null)
		{
			_statisticsPage = new StatisticsPanel(_defResultsDirectory);
			_statisticsPage.Initialize(_rawSP);
			_statisticsPane.Clear();
			_statisticsPage.Dock = DockStyle.Fill;
			_statisticsPage.Name = "ClientStatisticsPanel";
			_statisticsPane.Add(_statisticsPage);
			_statisticsPage.PopulateFromStatisticsControl(_clientStatisticsControl, _fontGridResults, _bkGridColor, _foreGridColor, _selectedCellColor, _inactiveCellColor, _headerRowColor);
		}

		if (SqlEditorPane != null)
		{
			if (_hadErrorsDuringExecution)
			{
				SqlEditorPane.ActivateMessageTab();
			}
			else if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToGrid)
			{
				if (_gridResultsPage.NumberOfGrids > 0)
				{
					SqlEditorPane.ActivateResultsTab();
				}
				else if (_gridResultsPage.NumberOfGrids == 0)
				{
					SqlEditorPane.IsResultsGridButtonVisible = false;
				}
			}
			else if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToText)
			{
				if (string.IsNullOrEmpty(_textResultsPage.TextView.TextBuffer.Text))
				{
					SqlEditorPane.IsTextResultsButtonVisible = false;
				}
				else
				{
					SqlEditorPane.ActivateTextResultsTab();
				}
			}
		}

		if (!AutoSelectResultsTab)
		{
			SqlEditorPane.ActivateCodeTab();
		}

		if (!_QueryExecutor.IsExecuting)
		{
			return;
		}

		try
		{
			if (!_hadErrorsDuringExecution)
			{
				_hadErrorsDuringExecution = (args.ExecutionResult & EnScriptExecutionResult.Failure) != 0;
			}

			if (AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToGrid && CouldNotShowSomeGridResults)
			{
				_textMessagesPage.ResultsWriter.AppendError(string.Format(CultureInfo.CurrentCulture, SharedResx.CanDisplayOnlyNGridResults, C_MaxGridResultSets));
				_bHasMessages = true;
				_hadErrorsDuringExecution = true;
			}

			if (!ShouldDiscardResults && !_bHasMessages && args.ExecutionResult == EnScriptExecutionResult.Success)
			{
				string text = _QueryExecutor.ConnectionStrategy.GetCustomQuerySuccessMessage();
				text ??= SharedResx.MsgCommandSuccess;

				_textMessagesPage.ResultsWriter.AppendNormal(text);
				if (!_bHasTextResults && AuxiliaryDocData.SqlExecutionMode == EnSqlExecutionMode.ResultsToFile && _resultsWriter != null)
				{
					_resultsWriter.AppendNormal(text);
				}
			}

			if (!ShouldDiscardResults && args.ExecutionResult == EnScriptExecutionResult.Cancel)
			{
				_textMessagesPage.ResultsWriter.AppendNormal(SharedResx.MsgQueryCancelled);
			}

			FlushAllTextWriters();
			CheckAndCloseTextWriters();
			_textResultsPage.UndoEnabled = true;
			_textMessagesPage.UndoEnabled = true;
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
		}
		finally
		{
			if (_BatchConsumer != null)
			{
				try
				{
					_BatchConsumer.CleanupAfterFinishingExecution();
				}
				catch (Exception e2)
				{
					Tracer.LogExCatch(GetType(), e2);
				}
			}
		}
	}

	private void OnQueryExecutorStatusChanged(object sender, QueryExecutor.StatusChangedEventArgs args)
	{
		if (args.Change == QueryExecutor.StatusType.Connection)
		{
			_clearStatisticsControl = true;
		}
	}

	private void OnSqlCmdOutputRedirection(object sender, QEOLESQLOutputRedirectionEventArgs args)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlCmdOutputRedirection", "", null);
		ResultsWriter resultsWriter = _textResultsPage.ResultsWriter;
		if (args.FullFileName != null && args.FullFileName != "stderr" && args.FullFileName != "stdout")
		{
			resultsWriter = new FileStreamResultsWriter(args.FullFileName, append: false);
		}

		if (args.OutputRedirectionCategory == EnQEOLESQLOutputCategory.Results)
		{
			if (_resultsWriter != null && _resultsWriter != _textResultsPage.ResultsWriter)
			{
				_resultsWriter.Flush();
			}

			if (_messagesWriter != null && _messagesWriter != _textMessagesPage.ResultsWriter && _messagesWriter != _resultsWriter)
			{
				_messagesWriter.Flush();
			}

			if (_resultsWriter != null && _resultsWriter != _textResultsPage.ResultsWriter && _resultsWriter != _errorsWriter)
			{
				_resultsWriter.Close();
			}

			if (_messagesWriter != null && _messagesWriter != _textMessagesPage.ResultsWriter && _messagesWriter != _resultsWriter && _messagesWriter != _errorsWriter)
			{
				_messagesWriter.Close();
			}

			_messagesWriter = _resultsWriter = resultsWriter;
		}
		else
		{
			if (args.OutputRedirectionCategory != EnQEOLESQLOutputCategory.Errors)
			{
				return;
			}

			if (_errorsWriter != null && _errorsWriter != _textResultsPage.ResultsWriter)
			{
				_errorsWriter.Flush();
				if (_errorsWriter != _resultsWriter && _errorsWriter != _messagesWriter)
				{
					_errorsWriter.Close();
				}
			}

			_errorsWriter = resultsWriter;
		}

		if (args.OutputRedirectionCategory == EnQEOLESQLOutputCategory.Results && args.BatchConsumer is ResultsToGridBatchConsumer)
		{
			if (_resultRedirBatchConsumer == null)
			{
				_resultRedirBatchConsumer = new ResultsToTextOrFileBatchConsumer(this);
				ApplyResultSettingsToBatchConsumer(_resultRedirBatchConsumer, AuxiliaryDocData.ResultsSettings);
			}

			args.BatchConsumer = _resultRedirBatchConsumer;
		}
	}

	private void OnSqlCmdMsgFromApp(object sender, QeSqlCmdMessageFromAppEventArgs args)
	{
		Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlCmdMsgFromApp", "", null);
		string message = args.Message;
		if (args.StdOut)
		{
			AddMessageToTextWriterCommon(message, -1, null, ResultMessageType.Normal, _messagesWriter, flush: true, noCr: true);
			return;
		}

		_hadErrorsDuringExecution = true;
		AddMessageToTextWriterCommon(message, -1, null, ResultMessageType.Error, _errorsWriter, flush: true, noCr: true);
	}
}
