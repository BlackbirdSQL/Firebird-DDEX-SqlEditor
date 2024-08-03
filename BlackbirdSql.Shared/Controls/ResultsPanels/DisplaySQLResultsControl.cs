// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.DisplaySQLResultsControl

using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
// using BlackbirdSql.Shared.Controls.Graphing;
// using BlackbirdSql.Shared.Controls.Graphing.Enums;
// using BlackbirdSql.Shared.Controls.Graphing.Interfaces;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.IO;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Exceptions;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Model.QueryExecution;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
// using Microsoft.AnalysisServices.Graphing;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Controls.ResultsPanels;


public class DisplaySQLResultsControl : IBsQueryExecutionHandler, IBsExecutionHandler, IDisposable
{

	public DisplaySQLResultsControl(ResultWindowPane resultsGridPanel, ResultWindowPane messagePanel, ResultWindowPane textResultsPanel, ResultWindowPane statisticsPanel, /* ResultWindowPane executionPlanPanel,*/ ResultWindowPane textPlanPanel, ResultWindowPane spatialPane, IBsTabbedEditorWindowPane editorPane)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.DisplaySQLResultsControl", "", null);
		if (resultsGridPanel == null || messagePanel == null)
		{
			Exception ex = new ArgumentException("tabControl");
			Diag.ThrowException(ex);
		}

		_ResultsGridPane = resultsGridPanel;
		_MessagePane = messagePanel;
		_StatisticsPane = statisticsPanel;
		//_ExecutionPlanPane = executionPlanPanel;
		_TextPlanPane = textPlanPanel;
		_TextResultsPane = textResultsPanel;
		_SpatialResultsPane = spatialPane;
		_ = _SpatialResultsPane; // Warn suppression;
		SqlEditorPane = editorPane;
		Initialize(resultsGridPanel, messagePanel);
		AuxDocData.LiveSettingsChangedEvent += OnLiveSettingsChanged;
		AuxDocData.SqlExecutionModeChangedEvent += OnSqlExecutionModeChanged;
		FontAndColorProviderGridResults.Instance.ColorChangedEvent += OnGridColorChanged;
		FontAndColorProviderGridResults.Instance.FontChangedEvent += OnGridFontChanged;
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool bDisposing)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.Dispose", "", null);
		CleanupGrids();
		UnhookFromEvents();
		if (_ServiceProvider != null)
		{
			_ServiceProvider = null;
		}

		if (_ObjServiceProvider != null)
		{
			_ObjServiceProvider = null;
		}

		if (bDisposing)
		{
			if (_BatchConsumer != null)
			{
				_BatchConsumer.Dispose();
				_BatchConsumer = null;
			}

			if (_ResultRedirBatchConsumer != null)
			{
				_ResultRedirBatchConsumer.Dispose();
				_ResultRedirBatchConsumer = null;
			}

			if (_TextResultsPage != null)
			{
				_TextResultsPane.Clear();
				_TextResultsPage.Dispose();
				_TextResultsPage = null;
			}

			if (_TextMessagesPage != null)
			{
				_MessagePane.Clear();
				_TextMessagesPage.Dispose();
				_TextMessagesPage = null;
			}
		}

		if (_ClientStatisticsCollection != null)
		{
			_ClientStatisticsCollection = null;
		}

		if (_StatisticsPage != null)
		{
			_StatisticsPane?.Clear();
			_StatisticsPage.Dispose();
			_StatisticsPage = null;
		}

		// RemoveExecutionPlanPage();
		RemoveTextPlanPage();
	}




	private enum EnResultMessageType
	{
		Normal,
		Warning,
		Error
	}

	private delegate void FlushXMLWriterOrEnsureXMLResultsTabDelegate(ShellBufferWriter writer);

	// private readonly object _thisObject = new object();
	private AbstractQESQLBatchConsumer _BatchConsumer;
	private ResultsToTextOrFileBatchConsumer _ResultRedirBatchConsumer;
	private StatisticsSnapshotCollection _ClientStatisticsCollection;
	private ServiceProvider _ServiceProvider;
	private object _ObjServiceProvider;
	private AbstractResultsWriter _ResultsWriter;
	private AbstractResultsWriter _PlanWriter;
	private AbstractResultsWriter _MessagesWriter;
	private AbstractResultsWriter _ErrorsWriter;
	private bool _HasMessages;
	private bool _HasTextResults;
	// private bool _HasTextPlan;
	private bool _HadExecutionErrors;
	protected int _GridCount;
	private AuxilliaryDocData _AuxDocData;
	private string _DefaultResultsDirectory = "";
	private const int C_MaxGridResultSets = 10000000;
	// private const int C_MaxExecutionPlanControls = 100;
	// private bool _ExecutionPlanMaxCountExceeded;
	private readonly ResultWindowPane _ResultsGridPane;
	private readonly ResultWindowPane _MessagePane;
	private readonly ResultWindowPane _TextResultsPane;
	private readonly ResultWindowPane _StatisticsPane;
	// private readonly ResultWindowPane _ExecutionPlanPane;
	private readonly ResultWindowPane _TextPlanPane;
	private readonly ResultWindowPane _SpatialResultsPane;
	private GridResultsPanel _GridResultsPage;
	private VSTextEditorPanel _TextResultsPage;
	private VSTextEditorPanel _TextMessagesPage;
	private StatisticsPanel _StatisticsPage;
	// private ExecutionPlanPanel _ExecutionPlanPage;
	private VSTextEditorPanel _TextPlanPage;
	private Font _FontGridResults = Control.DefaultFont;
	private Color _BkGridColor = SystemColors.Window;
	private Color _GridColor = SystemColors.WindowText;
	private Color _SelectedCellColor = SystemColors.Highlight;
	private Color _InactiveCellColor = SystemColors.InactiveCaption;
	private Color _NullValueCellColor = SystemColors.InactiveCaption;
	private Color _HeaderRowColor = SystemColors.InactiveCaption;
	private bool _ClearStatisticsCollection;
	private QueryManager _QryMgr;





	public AuxilliaryDocData AuxDocData
	{
		get
		{
			if (_AuxDocData == null && SqlEditorPane != null)
			{
				_AuxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(SqlEditorPane.DocData);
			}

			return _AuxDocData;
		}
	}


	public StatisticsSnapshotCollection ClientStatisticsCollection => _ClientStatisticsCollection;


	public EnSqlOutputMode SqlOutputMode => AuxDocData.SqlOutputMode;


	public IBsEditorTransientSettings LiveSettings => AuxDocData.LiveSettings;


	public string DefaultResultsDirectory
	{
		get
		{
			return _DefaultResultsDirectory;
		}
		set
		{
			if (value == null)
			{
				Exception ex = new ArgumentNullException("value");
				Diag.ThrowException(ex);
			}

			// Tracer.Trace(GetType(), "DisplaySQLResultsControl.DefaultResultsDirectory", "value = {0}", value);
			_DefaultResultsDirectory = value;
			if (_DefaultResultsDirectory != value)
			{
				if (_TextResultsPage != null)
				{
					_TextResultsPage.DefaultResultsDirectory = value;
				}

				if (_TextMessagesPage != null)
				{
					_TextMessagesPage.DefaultResultsDirectory = value;
				}

				if (_GridResultsPage != null)
				{
					_GridResultsPage.DefaultResultsDirectory = value;
				}

				if (_StatisticsPage != null)
				{
					_StatisticsPage.DefaultResultsDirectory = value;
				}

				/*
				if (_ExecutionPlanPage != null)
				{
					_ExecutionPlanPage.DefaultResultsDirectory = value;
				}
				*/
				if (_TextPlanPage != null)
				{
					_TextPlanPage.DefaultResultsDirectory = value;
				}
			}
		}
	}


	// public ResultWindowPane ExecutionPlanWindowPane => _ExecutionPlanPane;

	public ResultWindowPane TextPlanWindowPane => _TextPlanPane;

	public IBsQESQLBatchConsumer BatchConsumer => _BatchConsumer;

	public bool CanAddMoreGrids => _GridCount < C_MaxGridResultSets;

	private bool CouldNotShowSomeGridResults => _GridCount > C_MaxGridResultSets;

	public IBsTabbedEditorWindowPane SqlEditorPane { get; set; }

	private bool ShouldDiscardResults
	{
		get
		{
			if ((AuxDocData.LiveSettings.EditorResultsOutputMode != EnSqlOutputMode.ToText
				&& AuxDocData.LiveSettings.EditorResultsOutputMode != EnSqlOutputMode.ToFile)
				|| !AuxDocData.LiveSettings.EditorResultsTextDiscardResults)
			{
				if (AuxDocData.LiveSettings.EditorResultsOutputMode == EnSqlOutputMode.ToGrid)
				{
					return AuxDocData.LiveSettings.EditorResultsGridDiscardResults;
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
			if (AuxDocData.SqlOutputMode != EnSqlOutputMode.ToText && AuxDocData.SqlOutputMode != EnSqlOutputMode.ToFile
				|| !AuxDocData.LiveSettings.EditorResultsTextOutputQuery)
			{
				if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToGrid)
				{
					return AuxDocData.LiveSettings.EditorResultsGridOutputQuery;
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
			if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToText || AuxDocData.SqlOutputMode == EnSqlOutputMode.ToFile)
			{
				return AuxDocData.LiveSettings.EditorResultsTextMaxCharsPerColumnStd;
			}

			return AuxDocData.LiveSettings.EditorResultsGridMaxCharsPerColumnStd;
		}
	}

	private bool AutoSelectResultsTab
	{
		get
		{
			if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToText || AuxDocData.SqlOutputMode == EnSqlOutputMode.ToFile)
			{
				if (AuxDocData.LiveSettings.EditorResultsTextSeparateTabs)
				{
					return AuxDocData.LiveSettings.EditorResultsTextSwitchToResults;
				}

				return false;
			}

			if (AuxDocData.LiveSettings.EditorResultsGridSeparateTabs)
			{
				return AuxDocData.LiveSettings.EditorResultsGridSwitchToResults;
			}

			return false;
		}
	}

	public IVsTextView MessagesPaneTextView => _TextMessagesPage.TextViewCtl.TextView;

	public IVsTextView TextResultsPaneTextView => _TextResultsPage.TextViewCtl.TextView;

	public IVsTextView TextPlanPaneTextView => _TextPlanPage?.TextViewCtl.TextView;

	private bool PlanOnly => LiveSettings.ExecutionType == EnSqlExecutionType.PlanOnly;


	public void ClearResultsTabs()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ClearResultsTabs", "", null);
		ClearTabs();
	}

	public void SetSite(object sp)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetSite", "", null);

		Diag.ThrowIfNotOnUIThread();

		if (_ServiceProvider != null)
			_ServiceProvider = null;

		_ServiceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)sp);
		_ObjServiceProvider = sp;
		OnHosted();
	}

	private void OnSqlExecutionModeChanged(object sender, AuxilliaryDocData.SqlExecutionModeChangedEventArgs sqlExecutionModeArgs)
	{
		EnSqlOutputMode sqlExecutionMode = sqlExecutionModeArgs.SqlOutputMode;
		ProcessSqlExecMode(sqlExecutionMode);
		ApplyLiveSettingsToBatchConsumer(_BatchConsumer, AuxDocData.LiveSettings);
	}

	private void OnLiveSettingsChanged(object sender, AuxilliaryDocData.LiveSettingsChangedEventArgs liveSettingsChangedArgs)
	{
		OnSqlExecutionModeChanged(sender, new AuxilliaryDocData.SqlExecutionModeChangedEventArgs(AuxDocData.SqlOutputMode));
		_GridResultsPage.SetGridTabOptions(AuxDocData.LiveSettings.EditorResultsGridSaveIncludeHeaders,
			AuxDocData.LiveSettings.EditorResultsGridCsvQuoteStringsCommas);
		DefaultResultsDirectory = AuxDocData.LiveSettings.EditorResultsDirectory;
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
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.PrepareForExecution", "prepareForParse = {0}", prepareForParse);
		((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(SqlEditorPane.DocData).QryMgr.ResultsHandler = BatchConsumer;
		AbstractResultsWriter resultsWriter = null;
		if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToFile && !prepareForParse
			&& !AuxDocData.LiveSettings.EditorResultsTextDiscardResults && !PlanOnly)
		{
			StreamWriter textWriterForQueryResultsToFile = VS.GetTextWriterForQueryResultsToFile(xmlResults: false, ref _DefaultResultsDirectory);
			if (textWriterForQueryResultsToFile == null)
			{
				return false;
			}

			resultsWriter = new FileStreamResultsWriter(textWriterForQueryResultsToFile);
		}

		Clear();
		PrepareTabs(prepareForParse);
		_TextResultsPage.UndoEnabled = false;
		_TextMessagesPage.UndoEnabled = false;

		if (_TextPlanPage != null)
		{
			_TextPlanPage.UndoEnabled = false;
			_PlanWriter = _TextPlanPage.ResultsWriter;
		}

		if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToText || ShouldDiscardResults || PlanOnly)
		{
			resultsWriter = _TextResultsPage.ResultsWriter;
		}

		_ResultsWriter = resultsWriter;
		_MessagesWriter = _TextMessagesPage.ResultsWriter;
		_ErrorsWriter = _TextMessagesPage.ResultsWriter;

		return true;
	}

	public void SetGridResultsFont(Font f)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridResultsFont", "", null);
		if (f == null)
		{
			Exception ex = new ArgumentNullException("f");
			Diag.ThrowException(ex);
		}

		_FontGridResults = f;
		_GridResultsPage.ApplyCurrentGridFont(_FontGridResults);
		_StatisticsPage?.ApplyCurrentGridFont(_FontGridResults);
	}

	public void SetGridResultsColors(Color? bkColor, Color? fkColor)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridResultsColors", "bkColor = {0}, fkColor = {1}", bkColor, fkColor);
		if (bkColor.HasValue)
		{
			_BkGridColor = bkColor.Value;
		}
		else
		{
			_BkGridColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexWindowBkColor);
		}

		if (fkColor.HasValue)
		{
			_GridColor = fkColor.Value;
		}
		else
		{
			_GridColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexWindowTextColor);
		}

		if (_BkGridColor != Color.Empty && _GridColor != Color.Empty)
		{
			_GridResultsPage.ApplyCurrentGridColor(_BkGridColor, _GridColor);
			_StatisticsPage?.ApplyCurrentGridColor(_BkGridColor, _GridColor);
		}
	}

	public void SetGridSelectedCellColor(Color? selectedCellColor)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridSelectedCellColor", "color = {0}", selectedCellColor);
		if (selectedCellColor.HasValue)
		{
			_SelectedCellColor = selectedCellColor.Value;
		}
		else
		{
			_SelectedCellColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexSelected);
		}

		if (_SelectedCellColor != Color.Empty)
		{
			_GridResultsPage.ApplySelectedCellColor(_SelectedCellColor);
			_StatisticsPage?.ApplySelectedCellColor(_SelectedCellColor);
		}
	}

	public void SetGridInactiveSelectedCellColor(Color? inactiveSelectedCellColor)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridInactiveSelectedCellColor", "color = {0}", inactiveSelectedCellColor);
		if (inactiveSelectedCellColor.HasValue)
		{
			_InactiveCellColor = inactiveSelectedCellColor.Value;
		}
		else
		{
			_InactiveCellColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexSelectedInactive);
		}

		if (_InactiveCellColor != Color.Empty)
		{
			_GridResultsPage.ApplyInactiveSelectedCellColor(_InactiveCellColor);
			_StatisticsPage?.ApplyInactiveSelectedCellColor(_InactiveCellColor);
		}
	}

	public void SetGridNullValueColor(Color? nullValueCellColor)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetGridNullValueColor", "color = {0}", nullValueCellColor);
		if (nullValueCellColor.HasValue)
		{
			_NullValueCellColor = nullValueCellColor.Value;
		}
		else
		{
			_NullValueCellColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexNullCell);
		}

		if (_NullValueCellColor != Color.Empty)
		{
			_GridResultsPage.ApplyHighlightedCellColor(_NullValueCellColor);
		}
	}

	public void SetHeaderRowColor(Color? headerRowColor)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetHeaderRowColor", "color = {0}", headerRowColor);
		if (headerRowColor.HasValue)
		{
			_HeaderRowColor = headerRowColor.Value;
		}
		else
		{
			_HeaderRowColor = VsColorUtilities.GetShellColor(FontAndColorProviderGridResults.VsSysColorIndexHeaderRow);
		}

		if (_HeaderRowColor != Color.Empty && _StatisticsPage != null)
		{
			_StatisticsPage.ApplyHighlightedCellColor(_HeaderRowColor);
		}
	}

	public void ActivateControl(EnPaneSelection selectPane)
	{
	}

	private void OnErrorMessage(object sender, ErrorMessageEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnErrorMessage()", "", null);

		string msg = _HasMessages ? "\r\n" : "";

		msg += args.DetailedMessage;


		if (args.MessageType != EnQESQLScriptProcessingMessageType.Warning)
		{
			AddStringToErrors(msg + "\r\n\t" + args.DescriptionMessage, args.Line, args.TextSpan, true);
		}
		else
		{
			AddStringToInfoMessages(msg + "\r\n\t" + args.DescriptionMessage, true);
		}
	}

	protected virtual void OnHosted()
	{
		// Tracer.Trace(GetType(), "OnHosted()", "", null);
		_GridResultsPage.Initialize(_ObjServiceProvider);
		_TextResultsPage.Initialize(_ObjServiceProvider);
		_TextMessagesPage.Initialize(_ObjServiceProvider);
		_TextPlanPage?.Initialize(_ObjServiceProvider);
	}

	private void Initialize(ResultWindowPane gridResultsPanel, ResultWindowPane messagePanel)
	{
		// Tracer.Trace(GetType(), "Initialize()", "", null);
		ProcessSqlExecMode(AuxDocData.SqlOutputMode);
		ApplyLiveSettingsToBatchConsumer(_BatchConsumer, AuxDocData.LiveSettings);
		_ = DefaultResultsDirectory = AuxDocData.LiveSettings.EditorResultsDirectory;
		_GridResultsPage = AllocateNewGridTabPage();
		_GridResultsPage.Name = "_GridResultsPage";
		_TextResultsPage = new(_DefaultResultsDirectory, xmlEditor: false)
		{
			Name = "_TextResultsPage"
		};
		_TextMessagesPage = new(_DefaultResultsDirectory, xmlEditor: false)
		{
			Name = "_TextMessagesPage"
		};
		AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(SqlEditorPane.DocData);
		RegisterToQueryExecutorEvents(auxDocData.QryMgr);

		IVsFontAndColorStorage vsFontAndColorStorage = ApcManager.GetService<SVsFontAndColorStorage, IVsFontAndColorStorage>();

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
		AddStringToTextWriterCommon(message, -1, null, EnResultMessageType.Normal, _ResultsWriter, flush, false);
		_HasTextResults = true;
	}

	public void AddStringToPlan(string message, bool flush)
	{
		AddStringToTextWriterCommon(message, -1, null, EnResultMessageType.Normal, _PlanWriter, flush, false);
		// _HasTextPlan = true;
	}

	public void AddStringToInfoMessages(string message, bool flush)
	{
		AddMessageToTextWriterCommon(message, -1, null, EnResultMessageType.Normal, _MessagesWriter, flush, true);
	}

	public void AddStringToMessages(string message, bool flush)
	{
		AddMessageToTextWriterCommon(message, -1, null, EnResultMessageType.Normal, _MessagesWriter, flush, false);
	}

	public void AddStringToErrors(string message, bool flush)
	{
		AddStringToErrors(message, -1, null, flush);
	}

	public void AddStringToErrors(string message, int line, IBsTextSpan textSpan, bool flush)
	{
		_HadExecutionErrors = true;
		AddMessageToTextWriterCommon(message, line, textSpan, EnResultMessageType.Error, _ErrorsWriter, flush, true);
	}

	public void AddResultSetSeparatorMsg()
	{
		AddStringToResults("", true);
	}


	public void AddGridContainer(ResultSetAndGridContainer grid)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.AddGridContainer", "", null);


		ThreadHelper.Generic.Invoke(delegate
		{
			AddGridContainerInt(grid);
		});

	}

	public void MarkAsCouldNotAddMoreGrids()
	{
		_GridCount = C_MaxGridResultSets + 1;
	}


	public void ProcessBatchSpecialAction(BatchSpecialActionEventArgs args)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSpecialActionOnBatch", "", null);
		// IGraph[] graphs = null;
		// object dataSource = null;

		bool hasGraphs = false;

		object executionPlanData = GetExecutionPlanData(args.DataReader, args.Action);

		/*
		try
		{
			// GetExecutionPlanGraphs(args.DataReader, args.Batch, args.Action, out graphs, out dataSource);
			GetExecutionPlanGraphs(executionPlanData, args.Action, out graphs, out dataSource);
			hasGraphs = true;
		}
		catch (Exception e)
		{
			// Execution plan visualizer is not supported yet. Just log the exception
			// Tracer.Trace(GetType(), "ProcessSpecialActionOnBatch", "Execution plan visualizer under development. Exception: {0}", e.Message);
		}
		*/

		if (!hasGraphs)
		{
			ThreadHelper.Generic.Invoke(delegate
			{
				ProcessSpecialActionOnBatchInt(args.Action, executionPlanData);
			});
		}

	}

	/*
	private void ProcessSpecialActionOnBatchInt(EnQESQLBatchSpecialAction action, IGraph[] graphs, object dataSource)
	{
		//IL_00bf: Expected O, but got Unknown
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSpecialActionOnBatchInt", "", null);
		try
		{
			if (_ExecutionPlanPage == null)
			{
				_ExecutionPlanPage = new(_DefaultResultsDirectory)
				{
					Name = "_ExecutionPlanPage",
					Dock = DockStyle.Fill,
					AutoSize = true,
				};
				_ExecutionPlanPage.Initialize(_ObjServiceProvider);
				((IBsObjectWithSite)_ExecutionPlanPage.ExecutionPlanCtl).SetSite(_ExecutionPlanPane);
			}

			try
			{
				if (_ExecutionPlanPage.ExecutionPlanCtl.GraphPanelCount < C_MaxExecutionPlanControls)
				{
					_ExecutionPlanPage.AddGraphs(graphs, dataSource);
				}
				else
				{
					_ExecutionPlanMaxCountExceeded = true;
				}
			}
			finally
			{
				if (_ExecutionPlanPage.ExecutionPlanCtl.GraphPanelCount == 0)
				{
					SqlEditorPane.ActivateMessageTab();
				}
			}
		}
		catch (NullReferenceException e)
		{
			Diag.Dug(e);
		}
		catch (FbException val)
		{
			FbException e2 = val;
			Tracer.LogExCatch(GetType(), (Exception)(object)e2);
		}
		catch (ArgumentException e3)
		{
			Diag.Dug(e3);
		}
		catch (OutOfMemoryException e4)
		{
			Diag.Dug(e4);
		}
		catch (InvalidOperationException e5)
		{
			Tracer.LogExCatch(GetType(), e5);
		}
		catch (ApplicationException ex)
		{
			Diag.Dug(ex);
			AddStringToErrors(ex.Message, flush: true);
		}
	}
	*/


	private bool ProcessSpecialActionOnBatchInt(EnSpecialActions action, object dataSource)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSpecialActionOnBatchInt", "Text plan source: " + dataSource, null);

		try
		{
			if (_TextPlanPage == null)
			{
				_TextPlanPage = new(_DefaultResultsDirectory, xmlEditor: false)
				{
					Name = "_TextPlanPage",
					Dock = DockStyle.Fill,
					AutoSize = true,
					UndoEnabled = false,
					TextReadOnly = false
				};

				if (_ObjServiceProvider != null)
					_TextPlanPage.Initialize(_ObjServiceProvider);

				_PlanWriter = _TextPlanPage.ResultsWriter;
			}

			AddStringToPlan((string)dataSource, true);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		return true;
	}


	public void ProcessNewXml(string xmlString, bool cleanPreviousResults)
	{
		NotSupportedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}

	/* Experimental
	private void GetExecutionPlanGraphs(IDataReader dataReader, QESQLBatch batch, EnQESQLBatchSpecialAction batchSpecialAction, out IGraph[] graphs, out object dataSource)
	{
		object executionPlanData = GetExecutionPlanData(batch.Command, batchSpecialAction);
		EnExecutionPlanType val = (EnExecutionPlanType)((batchSpecialAction & EnQESQLBatchSpecialAction.ExpectActualExecutionPlan) != 0 ? 1 : 2);
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

			ApplicationException exo = new(ControlsResources.ExFailedToReadExecutionPlan.FmtRes(text), ex);
			Diag.Dug(exo);
			throw exo;
		}
	}
	protected void GetExecutionPlanGraphs(object executionPlanData, EnQESQLBatchSpecialAction batchSpecialAction, out IGraph[] graphs, out object dataSource)
	{
		EnExecutionPlanType val = (EnExecutionPlanType)((batchSpecialAction & EnQESQLBatchSpecialAction.ExpectActualExecutionPlan) != 0 ? 1 : 2);
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

			ApplicationException exo = new(ControlsResources.ExFailedToReadExecutionPlan.FmtRes(text), ex);
			throw exo;
		}
	}
	*/

	/* Experimental
	private object GetExecutionPlanData(IDbCommand command, EnQESQLBatchSpecialAction batchSpecialAction)
	{
		if (command is not FbCommand fbCommand)
			return null;

		return fbCommand.GetCommandPlan();
	}
	*/

	private object GetExecutionPlanData(IDataReader dataReader, EnSpecialActions batchSpecialAction)
	{
		string plan = null;

		if ((batchSpecialAction & EnSpecialActions.ExecutionPlansMask) != 0)
		{
			while (dataReader.Read())
			{
				if (plan == null)
					plan = "";
				else plan += "\n";

				plan += dataReader.GetString(0);
			}

			if (plan == null)
			{ 

				InvalidOperationException ex = new(ControlsResources.ExCannotFindDataForExecutionPlan);
				Diag.Dug(ex);
				throw ex;
			}


			return plan;
		}

		return dataReader;
	}

	private void AddMessageToTextWriterCommon(string message, int line, IBsTextSpan textSpan,
		EnResultMessageType resultMessageType, AbstractResultsWriter writer, bool flush, bool noCr)
	{
		AddStringToTextWriterCommon(message, line, textSpan, resultMessageType, writer, flush, noCr);
		if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToFile && _ResultsWriter != null && _ResultsWriter != writer)
		{
			_ResultsWriter.AppendNormal(message, noCr);
		}
	}

	private void AddStringToTextWriterCommon(string message, int line, IBsTextSpan textSpan,
		EnResultMessageType resultMessageType, AbstractResultsWriter writer, bool flush, bool noCr)
	{
		if (message == null)
		{
			return;
		}

		switch (resultMessageType)
		{
			case EnResultMessageType.Normal:
				writer.AppendNormal(message, noCr);
				break;
			case EnResultMessageType.Warning:
				writer.AppendWarning(message, noCr);
				break;
			case EnResultMessageType.Error:
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

		_HasMessages = true;

		ThreadHelper.Generic.Invoke(delegate
		{
			FlushTextWritersInt(writer);
		});

	}

	private void Clear()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.Clear", "", null);
		CheckAndCloseTextWriters();
		_TextResultsPage.Clear();
		_TextMessagesPage.Clear();
		_HasMessages = false;
		_HadExecutionErrors = false;
		_HasTextResults = false;
		// _HasTextPlan = false;

		_BatchConsumer?.Cleanup();

		_ResultRedirBatchConsumer?.Cleanup();

		RemoveStatisticsPage();
		// RemoveExecutionPlanPage();
		RemoveTextPlanPage();
		CleanupGrids();
	}

	public void RemoveStatisticsPage()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.RemoveStatisticsTab", "", null);
		if (_StatisticsPage != null && _StatisticsPane != null && _StatisticsPane.Contains(_StatisticsPage))
		{
			_StatisticsPane.Remove(_StatisticsPage);
		}
	}

	/*
	public void RemoveExecutionPlanPage()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.RemoveExecutionPlanTab", "", null);
		if (_ExecutionPlanPage != null)
		{
			if (_ExecutionPlanPane.Contains(_ExecutionPlanPage))
			{
				_ExecutionPlanPane.Clear();
				_ExecutionPlanPane.Remove(_ExecutionPlanPage);
			}
			_ExecutionPlanPage.Dispose();
			_ExecutionPlanPage = null;
		}
	}
	*/




	public void RemoveTextPlanPage()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.RemoveTextPlanTab", "", null);
		if (_TextPlanPage != null)
		{
			if (_TextPlanPane.Contains(_TextPlanPage))
			{
				_TextPlanPane.Clear();
				_TextPlanPane.Remove(_TextPlanPage);
			}
			_TextPlanPage.Dispose();
			_TextPlanPage = null;
			_PlanWriter = null;
		}
	}

	private void CleanupGrids()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.CleanupGrids", "", null);
		_GridCount = 0;
		_GridResultsPage?.Clear();
	}

	private void UnhookFromEvents()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.UnhookFromEvents", "", null);
		UnRegisterQueryExecutorEvents();
		AuxDocData.SqlExecutionModeChangedEvent -= OnSqlExecutionModeChanged;
		AuxDocData.LiveSettingsChangedEvent -= OnLiveSettingsChanged;
		FontAndColorProviderGridResults.Instance.ColorChangedEvent -= OnGridColorChanged;
		FontAndColorProviderGridResults.Instance.FontChangedEvent -= OnGridFontChanged;
	}

	private void PrepareTabs(bool isParseOnly)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.PrepareTabs", "", null);
		ClearTabs();

		SqlEditorPane.CustomizeTabsForResultsSetting(isParseOnly);
		if (!_TextResultsPane.Contains(_TextResultsPage))
		{
			_TextResultsPage.Dock = DockStyle.Fill;
			_TextResultsPage.AutoSize = true;
			_TextResultsPane.Add(_TextResultsPage);
		}

		if (!_MessagePane.Contains(_TextMessagesPage))
		{
			_TextMessagesPage.Dock = DockStyle.Fill;
			_TextMessagesPage.AutoSize = true;
			_MessagePane.Add(_TextMessagesPage);
		}

		if (!_ResultsGridPane.Contains(_GridResultsPage))
		{
			_GridResultsPage.Dock = DockStyle.Fill;
			_ResultsGridPane.Add(_GridResultsPage);
		}

		_TextResultsPage.TextReadOnly = false;
		_TextMessagesPage.TextReadOnly = false;
	}

	private void OutputQueryIntoMessages(string strScript)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OutputQueryIntoMessages", "", null);
		AddStringToMessages("/*------------------------", flush: false);
		AddStringToMessages(strScript, flush: false);
		AddStringToMessages("------------------------*/", flush: true);
	}

	private void FlushTextWritersInt(AbstractResultsWriter textWriter)
	{
		if (textWriter != null)
		{
			textWriter.Flush();
			if (AuxDocData.LiveSettings.EditorResultsTextScrollingResults && textWriter == _TextResultsPage.ResultsWriter)
			{
				if (!AuxDocData.LiveSettings.EditorResultsTextScrollingResults)
					_TextResultsPage.ScrollTextViewToTop();
				else
					_TextResultsPage.ScrollTextViewToMaxScrollUnit();
			}
		}
	}

	private void AddGridContainerInt(ResultSetAndGridContainer cont)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.AddGridContainerInt", "", null);
		if (_GridCount == C_MaxGridResultSets)
		{
			MarkAsCouldNotAddMoreGrids();
			Exception ex = new BatchConsumerException(ControlsResources.ExCanDisplayOnlyNGridResults.FmtRes(C_MaxGridResultSets), BatchConsumerException.EnErrorType.CannotShowMoreResults);
			Diag.ThrowException(ex);
		}

		_GridCount++;
		GridResultsPanel gridResultsPanel = null;

		if (AuxDocData.LiveSettings.EditorResultsGridSingleTab
			|| (!AuxDocData.LiveSettings.EditorResultsGridSingleTab && _GridResultsPage.NumberOfGrids == 0))
		{
			gridResultsPanel = _GridResultsPage;
		}

		gridResultsPanel?.AddGridContainer(cont, _FontGridResults, _BkGridColor, _GridColor, _SelectedCellColor, _InactiveCellColor);
	}

	private void ProcessSqlExecMode(EnSqlOutputMode mode)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSqlExecMode", "", null);
		if (!Enum.IsDefined(typeof(EnSqlOutputMode), mode))
		{
			Exception ex = new ArgumentOutOfRangeException("mode");
			Diag.ThrowException(ex);
		}

		if (_BatchConsumer != null)
		{
			_BatchConsumer.Dispose();
			_BatchConsumer = null;
		}

		switch (mode)
		{
			case EnSqlOutputMode.ToText:
			case EnSqlOutputMode.ToFile:
				_BatchConsumer = new ResultsToTextOrFileBatchConsumer(this);
				break;
			case EnSqlOutputMode.ToGrid:
				_BatchConsumer = new ResultsToGridBatchConsumer(this);
				break;
			default:
				_BatchConsumer = new ResultsToGridBatchConsumer(this);
				break;
		}
	}

	private void ApplyLiveSettingsToBatchConsumer(AbstractQESQLBatchConsumer batchConsumer, IBsEditorTransientSettings liveSettings)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ApplyResSettingsToBatchConsumer", "", null);
		batchConsumer.MaxCharsPerColumn = MaxCharsPerColumn;
		batchConsumer.DiscardResults = ShouldDiscardResults;
		if (batchConsumer is ResultsToTextOrFileBatchConsumer obj)
		{
			obj.ColumnsDelimiter = liveSettings.EditorResultsTextDelimiter;
			obj.PrintColumnHeaders = liveSettings.EditorResultsTextIncludeHeaders;
			obj.RightAlignNumerics = liveSettings.EditorResultsTextAlignRightNumerics;
		}
	}

	private GridResultsPanel AllocateNewGridTabPage()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.AllocateNewGridTabPage", "", null);
		GridResultsPanel gridResultsPanel = new(_DefaultResultsDirectory)
		{
			Name = "GridResultsPanel",
			BackColor = SystemColors.Window
		};
		gridResultsPanel.ApplyHighlightedCellColor(_NullValueCellColor);
		if (_ObjServiceProvider != null)
		{
			gridResultsPanel.Initialize(_ObjServiceProvider);
		}

		gridResultsPanel.SetGridTabOptions(AuxDocData.LiveSettings.EditorResultsGridSaveIncludeHeaders, AuxDocData.LiveSettings.EditorResultsGridCsvQuoteStringsCommas);
		return gridResultsPanel;
	}

	private void ClearTabs()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ClearTabs", "", null);
		CleanupGrids();

		/*
		if (_ExecutionPlanPage != null)
		{
			_ExecutionPlanPane.Remove(_ExecutionPlanPage);
			_ExecutionPlanPage.Dispose();
			_ExecutionPlanPage = null;
		}
		*/

		RemoveTextPlanPage();

	}

	private void FlushAllTextWriters()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.FlushAllTextWriters", "", null);
		try
		{
			if (_ResultsWriter != null)
			{
				FlushTextWritersInt(_ResultsWriter);
			}
		}
		catch
		{
		}

		if (_MessagesWriter != _ResultsWriter && _MessagesWriter != null)
		{
			try
			{
				_MessagesWriter.Flush();
			}
			catch
			{
			}
		}

		if (_ErrorsWriter != _MessagesWriter && _ErrorsWriter != _ResultsWriter && _ErrorsWriter != null)
		{
			try
			{
				_ErrorsWriter.Flush();
			}
			catch
			{
			}
		}

		if (_MessagesWriter != _TextResultsPage.ResultsWriter && _ErrorsWriter != _TextResultsPage.ResultsWriter)
		{
			try
			{
				_TextResultsPage.ResultsWriter.Flush();
			}
			catch
			{
			}
		}

		if (_PlanWriter != null)
		{
			try
			{
				_PlanWriter.Flush();
			}
			catch
			{
			}
		}
	}

	private void CheckAndCloseTextWriters()
	{
		// Tracer.Trace(GetType(), "CheckAndCloseTextWriters()");

		if (_ResultsWriter != null && _ResultsWriter is FileStreamResultsWriter)
		{
			try
			{
				_ResultsWriter.Close();
			}
			catch
			{
			}
		}

		if (_MessagesWriter != null && _MessagesWriter != _ResultsWriter && _MessagesWriter is FileStreamResultsWriter)
		{
			try
			{
				_MessagesWriter.Close();
			}
			catch
			{
			}
		}

		if (_ErrorsWriter != null && _ErrorsWriter != _ResultsWriter && _ErrorsWriter != _MessagesWriter && _ErrorsWriter is FileStreamResultsWriter)
		{
			try
			{
				_ErrorsWriter.Close();
			}
			catch
			{
			}
		}

		_ResultsWriter = _MessagesWriter = _ErrorsWriter = null;
	}


	public void RegisterToQueryExecutorEvents(QueryManager qryMgr)
	{
		if (_QryMgr == null)
		{
			_QryMgr = qryMgr;
			_QryMgr.ExecutionStartedEvent += OnQueryExecutionStarted;
			_QryMgr.ExecutionCompletedEvent += OnQueryExecutionCompleted;
			_QryMgr.ErrorMessageEvent += OnErrorMessage;
			// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
			_QryMgr.BatchScriptParsedEvent += OnBatchScriptParsed;
			_QryMgr.BatchDataLoadedEvent += OnBatchDataLoaded;
			_QryMgr.BatchStatementCompletedEvent += OnBatchStatementCompleted;
			_QryMgr.StatusChangedEvent += OnQueryManagerStatusChanged;
		}
	}

	private void UnRegisterQueryExecutorEvents()
	{
		if (_QryMgr != null)
		{
			_QryMgr.ExecutionStartedEvent -= OnQueryExecutionStarted;
			_QryMgr.ExecutionCompletedEvent -= OnQueryExecutionCompleted;
			_QryMgr.ErrorMessageEvent -= OnErrorMessage;
			// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
			_QryMgr.BatchScriptParsedEvent -= OnBatchScriptParsed;
			_QryMgr.BatchDataLoadedEvent -= OnBatchDataLoaded;
			_QryMgr.BatchStatementCompletedEvent -= OnBatchStatementCompleted;
			_QryMgr.StatusChangedEvent -= OnQueryManagerStatusChanged;
			_QryMgr = null;
		}
	}



	private bool OnQueryExecutionStarted(object sender, QueryExecutionStartedEventArgs args)
	{
		if (AuxDocData.ClientStatisticsEnabled && args.ExecutionType != EnSqlExecutionType.PlanOnly)
		{
			if (args.Connection != null && (_ClientStatisticsCollection == null || _ClearStatisticsCollection))
			{
				_ClientStatisticsCollection = new(args.Connection); 
				_ClearStatisticsCollection = false;
			}

			if (args.Connection != null)
				_ClientStatisticsCollection.LoadStatisticsSnapshotBase(_QryMgr);
		}

		bool hasSaveTo = PrepareForExecution(false);

		if (hasSaveTo && ShouldOutputQuery)
		{
			OutputQueryIntoMessages(args.QueryText);
		}

		// _ExecutionPlanMaxCountExceeded = false;
		return hasSaveTo;
	}



	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public virtual void OnBatchDataLoaded(object sender, QueryDataEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnQueryDataLoaded()");

		if (args.StatementAction == EnSqlStatementAction.ProcessQuery
			&& args.ExecutionType != EnSqlExecutionType.PlanOnly && args.WithClientStats)
		{
			_ClientStatisticsCollection.RetrieveStatisticsIfNeeded(args);
		}
	}



	public virtual void OnBatchScriptParsed(object sender, QueryDataEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnBatchScriptParsed()");
	}


	public virtual void OnBatchStatementCompleted(object sender, BatchStatementCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnStatementCompleted()");
	}

	private void OnQueryExecutionCompleted(object sender, QueryExecutionCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnQueryExecutionCompleted()");

		/* Moved to OnBatchDataLoaded(()
		if (!args.IsParseOnly && AuxDocData.ClientStatisticsEnabled && _ClientStatisticsCtl != null)
		{
			_ClientStatisticsCtl.RetrieveStatisticsIfNeeded(_QryMgr);
		}
		*/

		ThreadHelper.Generic.Invoke(delegate
		{
			OnQueryExecutionCompletedImpl(sender, args);
		});

	}

	private void OnQueryExecutionCompletedImpl(object sender, QueryExecutionCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnQueryExecutionCompletedImpl()", "ExecResult = {0}", args.ExecutionResult);

		if (args.SyncCancel)
			return;
		/*
		if (_ExecutionPlanPage != null && !_ExecutionPlanPane.Contains(_ExecutionPlanPage))
		{
			if (_ExecutionPlanPage.ExecutionPlanCtl.GraphPanelCount > 0)
			{
				try
				{
					_ExecutionPlanPane.Add(_ExecutionPlanPage);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}
			}

			if (_ExecutionPlanMaxCountExceeded)
			{
				_TextMessagesPage.ResultsWriter.AppendNormal(ControlsResources.ExCanDisplayOnlyNExecutionPlanControls.FmtRes(C_MaxExecutionPlanControls), noCRLF: true);
			}
		}
		*/

		if (_TextPlanPage != null && !_TextPlanPane.Contains(_TextPlanPage))
		{
			try
			{
				_TextPlanPane.Add(_TextPlanPage);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			Guid guid = new Guid(LibraryData.C_SqlTextPlanTabLogicalViewGuid);
			SqlEditorMessageTab tab;

			try
			{
				tab = SqlEditorPane.GetSqlEditorTab<SqlEditorMessageTab>(guid);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}


			try
			{
				SqlEditorPane.ConfigureTextViewForAutonomousFind(tab.CurrentFrame, TextPlanPaneTextView);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

		}


		if (AuxDocData.ClientStatisticsEnabled && args.ExecutionType != EnSqlExecutionType.PlanOnly && _ClientStatisticsCollection != null)
		{
			try
			{
				_StatisticsPage = new StatisticsPanel(_DefaultResultsDirectory);
				_StatisticsPage.Initialize(_ObjServiceProvider);
				_StatisticsPane.Clear();
				_StatisticsPage.Dock = DockStyle.Fill;
				_StatisticsPage.Name = "ClientStatisticsPanel";
				_StatisticsPane.Add(_StatisticsPage);
				_StatisticsPage.PopulateFromStatisticsCollection(_ClientStatisticsCollection, _FontGridResults, _BkGridColor, _GridColor, _SelectedCellColor, _InactiveCellColor, _HeaderRowColor);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

		if (SqlEditorPane != null)
		{
			if (_HadExecutionErrors)
			{
				try
				{
					SqlEditorPane.ActivateMessageTab();
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}
			}
			else if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToGrid)
			{
				if (_GridResultsPage.NumberOfGrids > 0)
				{
					try
					{
						SqlEditorPane.ActivateResultsTab();
					}
					catch (Exception ex)
					{
						Diag.Dug(ex);
						throw;
					}
				}
				else if (_GridResultsPage.NumberOfGrids == 0)
				{
					SqlEditorPane.IsResultsGridButtonVisible = false;
				}
			}
			else if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToText)
			{
				if (string.IsNullOrWhiteSpace(_TextResultsPage.TextViewCtl.TextBuffer.Text))
				{
					SqlEditorPane.IsTextResultsButtonVisible = false;
				}
				else
				{
					try
					{
						SqlEditorPane.ActivateTextResultsTab();
					}
					catch (Exception ex)
					{
						Diag.Dug(ex);
						throw;
					}
				}
			}
		}

		if (!AutoSelectResultsTab)
		{
			try
			{
				SqlEditorPane.ActivateCodeTab();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

		if (!_QryMgr.IsExecuting)
			return;

		try
		{
			if (!_HadExecutionErrors)
			{
				_HadExecutionErrors = (args.ExecutionResult & EnScriptExecutionResult.Failure) != 0;
			}

			if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToGrid && CouldNotShowSomeGridResults)
			{
				try
				{
					_TextMessagesPage.ResultsWriter.AppendError(ControlsResources.ExCanDisplayOnlyNGridResults.FmtRes(C_MaxGridResultSets));
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}

				_HasMessages = true;
				_HadExecutionErrors = true;
			}

			if (!ShouldDiscardResults && !_HasMessages && args.ExecutionResult == EnScriptExecutionResult.Success)
			{
				string text = _QryMgr.Strategy.GetCustomQuerySuccessMessage();

				text ??= ControlsResources.MsgCommandSuccess;

				try
				{
					_TextMessagesPage.ResultsWriter.AppendNormal(text);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}

				if (!_HasTextResults && AuxDocData.SqlOutputMode == EnSqlOutputMode.ToFile && _ResultsWriter != null)
				{
					try
					{
						_ResultsWriter.AppendNormal(text);
					}
					catch (Exception ex)
					{
						Diag.Dug(ex);
						throw;
					}
				}
			}

			if (!ShouldDiscardResults && args.ExecutionResult == EnScriptExecutionResult.Cancel)
			{
				try
				{
					_TextMessagesPage.ResultsWriter.AppendNormal(_QryMgr.GetUpdatedTransactionsStatus(true)
						? ControlsResources.MsgQueryCancelledRollback :ControlsResources.MsgQueryCancelled);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw;
				}
			}

			try
			{
				FlushAllTextWriters();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			try
			{
				CheckAndCloseTextWriters();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			_TextResultsPage.UndoEnabled = true;
			_TextMessagesPage.UndoEnabled = true;
		}
		catch (Exception e)
		{
			Diag.Dug(e);
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
					Diag.Dug(e2);
				}
			}
		}
	}

	private void OnQueryManagerStatusChanged(object sender, QueryStatusChangedEventArgs args)
	{
		if (args.StatusFlag == EnQueryStatusFlags.Connection)
		{
			_ClearStatisticsCollection = true;
		}
	}


}
