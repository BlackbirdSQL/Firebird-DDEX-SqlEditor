// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.DisplaySQLResultsControl

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Controls.Graphing;
using BlackbirdSql.Common.Controls.Graphing.Enums;
using BlackbirdSql.Common.Controls.Graphing.Interfaces;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.AnalysisServices.Graphing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Controls.ResultsPane;

[SuppressMessage("Usage", "VSTHRD001:Avoid legacy thread switching APIs")]
[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

public class DisplaySQLResultsControl : IBSqlQueryExecutionHandler, IBQueryExecutionHandler, IDisposable
{
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

	private StatisticsControl _ClientStatisticsCtl;

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

	private AuxiliaryDocData _AuxDocData;

	private string _DefaultResultsDirectory = "";

	private const int C_MaxGridResultSets = 10000000;

	private const int C_MaxExecutionPlanControls = 100;

	private bool _ExecutionPlanMaxCountExceeded;

	private readonly ResultWindowPane _ResultsGridPane;

	private readonly ResultWindowPane _MessagePane;

	private readonly ResultWindowPane _TextResultsPane;

	private readonly ResultWindowPane _StatisticsPane;

	private readonly ResultWindowPane _ExecutionPlanPane;

	private readonly ResultWindowPane _TextPlanPane;

	private readonly ResultWindowPane _SpatialResultsPane;

	private GridResultsPanel _GridResultsPage;

	private VSTextEditorPanel _TextResultsPage;

	private VSTextEditorPanel _TextMessagesPage;

	private StatisticsPanel _StatisticsPage;

	private ExecutionPlanPanel _ExecutionPlanPage;

	private VSTextEditorPanel _TextPlanPage;

	private Font _FontGridResults = Control.DefaultFont;

	private Color _BkGridColor = SystemColors.Window;

	private Color _GridColor = SystemColors.WindowText;

	private Color _SelectedCellColor = SystemColors.Highlight;

	private Color _InactiveCellColor = SystemColors.InactiveCaption;

	private Color _NullValueCellColor = SystemColors.InactiveCaption;

	private Color _HeaderRowColor = SystemColors.InactiveCaption;

	private bool _ClearStatisticsControl;

	private QueryManager _QryMgr;

	public AuxiliaryDocData AuxDocData
	{
		get
		{
			if (_AuxDocData == null && SqlEditorPane != null)
			{
				_AuxDocData = ((IBEditorPackage)Controller.DdexPackage).GetAuxiliaryDocData(SqlEditorPane.DocData);
			}

			return _AuxDocData;
		}
	}

	public EnSqlOutputMode SqlExecutionMode
	{
		get
		{
			return AuxDocData.SqlExecutionMode;
		}
		set
		{
			AuxDocData.SqlExecutionMode = value;
		}
	}

	public IBLiveUserSettings LiveSettings
	{
		get
		{
			return AuxDocData.LiveSettings;
		}
		set
		{
		}
	}

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
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
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

				if (_ExecutionPlanPage != null)
				{
					_ExecutionPlanPage.DefaultResultsDirectory = value;
				}
				if (_TextPlanPage != null)
				{
					_TextPlanPage.DefaultResultsDirectory = value;
				}
			}
		}
	}

	public QESQLCommandBuilder SqlExecutionOptions => AuxDocData.QryMgr.LiveSettings;

	public ResultWindowPane ExecutionPlanWindowPane => _ExecutionPlanPane;

	public ResultWindowPane TextPlanWindowPane => _TextPlanPane;

	public IBQESQLBatchConsumer BatchConsumer => _BatchConsumer;

	public bool CanAddMoreGrids => _GridCount < C_MaxGridResultSets;

	private bool CouldNotShowSomeGridResults => _GridCount > C_MaxGridResultSets;

	public IBSqlEditorWindowPane SqlEditorPane { get; set; }

	private bool ShouldDiscardResults
	{
		get
		{
			if (AuxDocData.LiveSettings.EditorResultsOutputMode != EnSqlOutputMode.ToText
				&& AuxDocData.LiveSettings.EditorResultsOutputMode != EnSqlOutputMode.ToFile
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
			if (AuxDocData.SqlExecutionMode != EnSqlOutputMode.ToText && AuxDocData.SqlExecutionMode != EnSqlOutputMode.ToFile || !AuxDocData.LiveSettings.EditorResultsTextOutputQuery)
			{
				if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToGrid)
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
			if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToText || AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToFile)
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
			if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToText || AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToFile)
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

	private bool WithEstimatedExecutionPlan => SqlExecutionOptions.WithEstimatedExecutionPlan;

	public DisplaySQLResultsControl(ResultWindowPane resultsGridPanel, ResultWindowPane messagePanel, ResultWindowPane textResultsPanel, ResultWindowPane statisticsPanel, ResultWindowPane executionPlanPanel, ResultWindowPane textPlanPanel, ResultWindowPane spatialPane, IBSqlEditorWindowPane editorPane)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.DisplaySQLResultsControl", "", null);
		if (resultsGridPanel == null || messagePanel == null)
		{
			Exception ex = new ArgumentException("tabControl");
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		_ResultsGridPane = resultsGridPanel;
		_MessagePane = messagePanel;
		_StatisticsPane = statisticsPanel;
		_ExecutionPlanPane = executionPlanPanel;
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

		if (_ClientStatisticsCtl != null)
		{
			_ClientStatisticsCtl = null;
		}

		if (_StatisticsPage != null)
		{
			_StatisticsPane?.Clear();
			_StatisticsPage.Dispose();
			_StatisticsPage = null;
		}

		RemoveExecutionPlanPage();
		RemoveTextPlanPage();
	}

	public void ClearResultsTabs()
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ClearResultsTabs", "", null);
		ClearTabs();
	}

	public void SetSite(object sp)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SetSite", "", null);

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (_ServiceProvider != null)
			_ServiceProvider = null;

		_ServiceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)sp);
		_ObjServiceProvider = sp;
		OnHosted();
	}

	private void OnSqlExecutionModeChanged(object sender, AuxiliaryDocData.SqlExecutionModeChangedEventArgs sqlExecutionModeArgs)
	{
		EnSqlOutputMode sqlExecutionMode = sqlExecutionModeArgs.SqlExecutionMode;
		ProcessSqlExecMode(sqlExecutionMode);
		ApplyLiveSettingsToBatchConsumer(_BatchConsumer, AuxDocData.LiveSettings);
	}

	private void OnLiveSettingsChanged(object sender, AuxiliaryDocData.LiveSettingsChangedEventArgs liveSettingsChangedArgs)
	{
		OnSqlExecutionModeChanged(sender, new AuxiliaryDocData.SqlExecutionModeChangedEventArgs(AuxDocData.SqlExecutionMode));
		_GridResultsPage.SetGridTabOptions(AuxDocData.LiveSettings.EditorResultsGridSaveIncludeHeaders,
			AuxDocData.LiveSettings.EditorResultsGridCsvQuoteStringsCommas);
		DefaultResultsDirectory = AuxDocData.QryMgr.LiveSettings.EditorResultsDirectory;
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
		(((IBEditorPackage)Controller.DdexPackage).GetAuxiliaryDocData(SqlEditorPane.DocData)).QryMgr.ResultsHandler = BatchConsumer;
		AbstractResultsWriter resultsWriter = null;
		if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToFile && !prepareForParse
			&& !AuxDocData.LiveSettings.EditorResultsTextDiscardResults && !WithEstimatedExecutionPlan)
		{
			StreamWriter textWriterForQueryResultsToFile = CommonUtils.GetTextWriterForQueryResultsToFile(xmlResults: false, ref _DefaultResultsDirectory);
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

		if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToText || ShouldDiscardResults || WithEstimatedExecutionPlan)
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
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
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

	private void OnQEOLESQLErrorMessage(object sender, QEOLESQLErrorMessageEventArgs args)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnQEOLESQLErrorMessage", "", null);
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
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnHosted", "", null);
		_GridResultsPage.Initialize(_ObjServiceProvider);
		_TextResultsPage.Initialize(_ObjServiceProvider);
		_TextMessagesPage.Initialize(_ObjServiceProvider);
		_TextPlanPage?.Initialize(_ObjServiceProvider);
	}

	private void Initialize(ResultWindowPane gridResultsPanel, ResultWindowPane messagePanel)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.Initialize", "", null);
		ProcessSqlExecMode(AuxDocData.SqlExecutionMode);
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
		AuxiliaryDocData auxDocData = ((IBEditorPackage)Controller.DdexPackage).GetAuxiliaryDocData(SqlEditorPane.DocData);
		RegisterToQueryExecutorEvents(auxDocData.QryMgr);

		IVsFontAndColorStorage vsFontAndColorStorage = Controller.GetService<SVsFontAndColorStorage, IVsFontAndColorStorage>();

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
		AddStringToTextWriterCommon(message, -1, null, EnResultMessageType.Normal, _ResultsWriter, flush, noCr: false);
		_HasTextResults = true;
	}

	public void AddStringToPlan(string message, bool flush)
	{
		AddStringToTextWriterCommon(message, -1, null, EnResultMessageType.Normal, _PlanWriter, flush, noCr: false);
		// _HasTextPlan = true;
	}

	public void AddStringToMessages(string message, bool flush)
	{
		AddMessageToTextWriterCommon(message, -1, null, EnResultMessageType.Normal, _MessagesWriter, flush, noCr: false);
	}

	public void AddStringToErrors(string message, bool flush)
	{
		AddStringToErrors(message, -1, null, flush);
	}

	public void AddStringToErrors(string message, int line, IBTextSpan textSpan, bool flush)
	{
		_HadExecutionErrors = true;
		AddMessageToTextWriterCommon(message, line, textSpan, EnResultMessageType.Error, _ErrorsWriter, flush, noCr: false);
	}

	public void AddResultSetSeparatorMsg()
	{
		AddStringToResults("", flush: true);
	}


	public void AddGridContainer(ResultSetAndGridContainer grid)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.AddGridContainer", "", null);


		ThreadHelper.Generic.Invoke(delegate
		{
			AddGridContainerInt(grid);
		});

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
		_GridCount = C_MaxGridResultSets + 1;
	}


	public void ProcessSpecialActionOnBatch(QESQLBatchSpecialActionEventArgs args)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSpecialActionOnBatch", "", null);
		IGraph[] graphs = null;
		object dataSource = null;

		bool hasGraphs = false;

		object executionPlanData = GetExecutionPlanData(args.DataReader, args.Action);

		try
		{
			// GetExecutionPlanGraphs(args.DataReader, args.Batch, args.Action, out graphs, out dataSource);
			GetExecutionPlanGraphs(executionPlanData, args.Action, out graphs, out dataSource);
			hasGraphs = true;
		}
		catch (Exception e)
		{
			// Execution plan visualizer is not supported yet. Just log the exception
			Tracer.Trace(GetType(), "ProcessSpecialActionOnBatch", "Execution plan visualizer under development. Exception: {0}", e.Message);
		}

		/*
		Task task = Task.Factory.StartNew(delegate
		{
			ProcessSpecialActionOnBatchInt(args.Action, graphs, dataSource);
		},
		default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
		*/

		if (!hasGraphs)
		{
			ThreadHelper.Generic.Invoke(delegate
			{
				ProcessSpecialActionOnBatchInt(args.Action, executionPlanData);
			});
			return;
		}
		ThreadHelper.Generic.Invoke(delegate
		{
			ProcessSpecialActionOnBatchInt(args.Action, graphs, dataSource);
		});
	}

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
				((IBObjectWithSite)_ExecutionPlanPage.ExecutionPlanCtl).SetSite(_ExecutionPlanPane);
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


	private void ProcessSpecialActionOnBatchInt(EnQESQLBatchSpecialAction action, object dataSource)
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
			Tracer.LogExCatch(GetType(), ex);
		}
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

			ApplicationException exo = new(string.Format(CultureInfo.CurrentCulture, ControlsResources.FailedToReadExecutionPlan, text), ex);
			Diag.Dug(exo);
			throw exo;
		}
	}
	*/
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

			ApplicationException exo = new(string.Format(CultureInfo.CurrentCulture, ControlsResources.FailedToReadExecutionPlan, text), ex);
			throw exo;
		}
	}

	/* Experimental
	private object GetExecutionPlanData(IDbCommand command, EnQESQLBatchSpecialAction batchSpecialAction)
	{
		if (command is not FbCommand fbCommand)
			return null;

		return fbCommand.GetCommandPlan();
	}
	*/

	private object GetExecutionPlanData(IDataReader dataReader, EnQESQLBatchSpecialAction batchSpecialAction)
	{
		if ((batchSpecialAction & EnQESQLBatchSpecialAction.ExpectYukonXmlExecutionPlan) != 0)
		{
			if (!dataReader.Read())
			{
				InvalidOperationException ex = new(ControlsResources.CannotFindDataForExecutionPlan);
				Diag.Dug(ex);
				throw ex;
			}

			return dataReader.GetString(0);
		}

		return dataReader;
	}

	private void AddMessageToTextWriterCommon(string message, int line, IBTextSpan textSpan, EnResultMessageType resultMessageType, AbstractResultsWriter writer, bool flush, bool noCr)
	{
		AddStringToTextWriterCommon(message, line, textSpan, resultMessageType, writer, flush, noCr);
		if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToFile && _ResultsWriter != null && _ResultsWriter != writer)
		{
			_ResultsWriter.AppendNormal(message, noCr);
		}
	}

	private void AddStringToTextWriterCommon(string message, int line, IBTextSpan textSpan, EnResultMessageType resultMessageType, AbstractResultsWriter writer, bool flush, bool noCr)
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
		RemoveExecutionPlanPage();
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
			if (AuxDocData.LiveSettings.EditorResultsTextScrollingResults)
			{
				if (AuxDocData.LiveSettings.EditorResultsOutputMode == EnSqlOutputMode.ToText && textWriter == _TextResultsPage.ResultsWriter)
				{
					_TextResultsPage.ScrollTextViewToMaxScrollUnit();
				}
			}
		}
	}

	private void AddGridContainerInt(ResultSetAndGridContainer cont)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.AddGridContainerInt", "", null);
		if (_GridCount == C_MaxGridResultSets)
		{
			MarkAsCouldNotAddMoreGrids();
			Exception ex = new QESQLBatchConsumerException(string.Format(CultureInfo.CurrentCulture, ControlsResources.CanDisplayOnlyNGridResults, C_MaxGridResultSets), QESQLBatchConsumerException.EnErrorType.CannotShowMoreResults);
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		_GridCount++;
		GridResultsPanel gridResultsPanel = null;
		if (AuxDocData.LiveSettings.EditorResultsGridSingleTab || !AuxDocData.LiveSettings.EditorResultsGridSingleTab && _GridResultsPage.NumberOfGrids == 0)
		{
			gridResultsPanel = _GridResultsPage;
		}

		gridResultsPanel.AddGridContainer(cont, _FontGridResults, _BkGridColor, _GridColor, _SelectedCellColor, _InactiveCellColor);
	}

	private void ProcessSqlExecMode(EnSqlOutputMode mode)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.ProcessSqlExecMode", "", null);
		if (!Enum.IsDefined(typeof(EnSqlOutputMode), mode))
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

	private void ApplyLiveSettingsToBatchConsumer(AbstractQESQLBatchConsumer batchConsumer, IBLiveUserSettings liveSettings)
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

		if (_ExecutionPlanPage != null)
		{
			_ExecutionPlanPane.Remove(_ExecutionPlanPage);
			_ExecutionPlanPage.Dispose();
			_ExecutionPlanPage = null;
		}

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
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.CheckAndCloseTextWriters", "", null);
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

	public static bool IsRunningOnUIThread()
	{
		return ThreadHelper.CheckAccess();
	}

	public void RegisterToQueryExecutorEvents(QueryManager qryMgr)
	{
		if (_QryMgr == null)
		{
			_QryMgr = qryMgr;
			_QryMgr.ScriptExecutionStartedEvent += OnScriptExecutionStarted;
			_QryMgr.ScriptExecutionCompletedEvent += OnSqlExecutionCompletedForSQLExec;
			_QryMgr.ScriptExecutionErrorMessageEvent += OnQEOLESQLErrorMessage;
			// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
			_QryMgr.DataLoadedEvent += OnSqlDataLoadedForSQLExec;
			_QryMgr.StatementCompletedEvent += OnSqlStatementCompletedForSQLExec;
			_QryMgr.StatusChangedEvent += OnQueryExecutorStatusChanged;
			_QryMgr.SqlCmdOutputRedirectionEvent += OnSqlCmdOutputRedirection;
			_QryMgr.SqlCmdMessageFromAppEvent += OnSqlCmdMsgFromApp;
		}
	}

	private void UnRegisterQueryExecutorEvents()
	{
		if (_QryMgr != null)
		{
			_QryMgr.ScriptExecutionStartedEvent -= OnScriptExecutionStarted;
			_QryMgr.ScriptExecutionCompletedEvent -= OnSqlExecutionCompletedForSQLExec;
			_QryMgr.ScriptExecutionErrorMessageEvent -= OnQEOLESQLErrorMessage;
			// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
			_QryMgr.DataLoadedEvent -= OnSqlDataLoadedForSQLExec;
			_QryMgr.StatementCompletedEvent -= OnSqlStatementCompletedForSQLExec;
			_QryMgr.StatusChangedEvent -= OnQueryExecutorStatusChanged;
			_QryMgr.SqlCmdOutputRedirectionEvent -= OnSqlCmdOutputRedirection;
			_QryMgr.SqlCmdMessageFromAppEvent -= OnSqlCmdMsgFromApp;
			_QryMgr = null;
		}
	}

	private bool OnScriptExecutionStarted(object sender, QueryManager.ScriptExecutionStartedEventArgs args)
	{
		if (AuxDocData.ClientStatisticsEnabled && !args.IsParseOnly)
		{
			FbConnection asSqlConnection = (FbConnection)args.Connection;
			if (args.Connection != null && (_ClientStatisticsCtl == null || _ClearStatisticsControl))
			{
				_ClientStatisticsCtl = new StatisticsControl();
				StatisticsConnection node = new (asSqlConnection);
				_ClientStatisticsCtl.Add(node);

				_ClearStatisticsControl = false;
			}

			if (args.Connection != null)
				_ClientStatisticsCtl.LoadStatisticsSnapshotBase(_QryMgr);
		}

		bool num = PrepareForExecution(args.IsParseOnly);
		if (num && ShouldOutputQuery && !args.IsParseOnly)
		{
			OutputQueryIntoMessages(args.QueryText);
		}

		// _ExecutionPlanMaxCountExceeded = false;
		return num;
	}

	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public virtual void OnSqlDataLoadedForSQLExec(object sender, QESQLDataLoadedEventArgs args)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlDataLoadedForSQLExec", "", null);
		if (!args.IsParseOnly && AuxDocData.ClientStatisticsEnabled && _ClientStatisticsCtl != null)
		{
			_ClientStatisticsCtl.RetrieveStatisticsIfNeeded(_QryMgr, args.RecordCount, args.RecordsAffected, args.ExecutionEndTime);
		}
	}


	public virtual void OnSqlStatementCompletedForSQLExec(object sender, QESQLStatementCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlExecutionCompletedForSQLExec", "", null);
	}

	private void OnSqlExecutionCompletedForSQLExec(object sender, ScriptExecutionCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlExecutionCompletedForSQLExec", "", null);

		/* Moved to OnSqlStatementCompletedForSQLExec(()
		if (!args.IsParseOnly && AuxDocData.ClientStatisticsEnabled && _ClientStatisticsCtl != null)
		{
			_ClientStatisticsCtl.RetrieveStatisticsIfNeeded(_QryMgr);
		}
		*/

		ThreadHelper.Generic.Invoke(delegate
		{
			OnSqlExecutionCompletedInt(sender, args);
		});


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
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlExecutionCompletedInt", "ExecResult = {0}", args.ExecutionResult);

		if (_ExecutionPlanPage != null && !_ExecutionPlanPane.Contains(_ExecutionPlanPage))
		{
			if (_ExecutionPlanPage.ExecutionPlanCtl.GraphPanelCount > 0)
			{
				_ExecutionPlanPane.Add(_ExecutionPlanPage);
			}

			if (_ExecutionPlanMaxCountExceeded)
			{
				_TextMessagesPage.ResultsWriter.AppendNormal(string.Format(CultureInfo.CurrentCulture, ControlsResources.CanDisplayOnlyNExecutionPlanControls, C_MaxExecutionPlanControls), noCRLF: true);
			}
		}

		if (_TextPlanPage != null && !_TextPlanPane.Contains(_TextPlanPage))
		{
			_TextPlanPane.Add(_TextPlanPage);

			Guid guid = new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid);
			SqlEditorMessageTab tab = SqlEditorPane.GetSqlEditorTab<SqlEditorMessageTab>(guid);

			SqlEditorPane.ConfigureTextViewForAutonomousFind(tab.CurrentFrame, TextPlanPaneTextView);

		}


		if (AuxDocData.ClientStatisticsEnabled && _ClientStatisticsCtl != null)
		{
			_StatisticsPage = new StatisticsPanel(_DefaultResultsDirectory);
			_StatisticsPage.Initialize(_ObjServiceProvider);
			_StatisticsPane.Clear();
			_StatisticsPage.Dock = DockStyle.Fill;
			_StatisticsPage.Name = "ClientStatisticsPanel";
			_StatisticsPane.Add(_StatisticsPage);
			_StatisticsPage.PopulateFromStatisticsControl(_ClientStatisticsCtl, _FontGridResults, _BkGridColor, _GridColor, _SelectedCellColor, _InactiveCellColor, _HeaderRowColor);
		}

		if (SqlEditorPane != null)
		{
			if (_HadExecutionErrors)
			{
				SqlEditorPane.ActivateMessageTab();
			}
			else if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToGrid)
			{
				if (_GridResultsPage.NumberOfGrids > 0)
				{
					SqlEditorPane.ActivateResultsTab();
				}
				else if (_GridResultsPage.NumberOfGrids == 0)
				{
					SqlEditorPane.IsResultsGridButtonVisible = false;
				}
			}
			else if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToText)
			{
				if (string.IsNullOrEmpty(_TextResultsPage.TextViewCtl.TextBuffer.Text))
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

		if (!_QryMgr.IsExecuting)
		{
			return;
		}

		try
		{
			if (!_HadExecutionErrors)
			{
				_HadExecutionErrors = (args.ExecutionResult & EnScriptExecutionResult.Failure) != 0;
			}

			if (AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToGrid && CouldNotShowSomeGridResults)
			{
				_TextMessagesPage.ResultsWriter.AppendError(string.Format(CultureInfo.CurrentCulture, ControlsResources.CanDisplayOnlyNGridResults, C_MaxGridResultSets));
				_HasMessages = true;
				_HadExecutionErrors = true;
			}

			if (!ShouldDiscardResults && !_HasMessages && args.ExecutionResult == EnScriptExecutionResult.Success)
			{
				string text = _QryMgr.ConnectionStrategy.GetCustomQuerySuccessMessage();
				text ??= ControlsResources.MsgCommandSuccess;

				_TextMessagesPage.ResultsWriter.AppendNormal(text);
				if (!_HasTextResults && AuxDocData.SqlExecutionMode == EnSqlOutputMode.ToFile && _ResultsWriter != null)
				{
					_ResultsWriter.AppendNormal(text);
				}
			}

			if (!ShouldDiscardResults && args.ExecutionResult == EnScriptExecutionResult.Cancel)
			{
				_TextMessagesPage.ResultsWriter.AppendNormal(ControlsResources.MsgQueryCancelled);
			}

			FlushAllTextWriters();
			CheckAndCloseTextWriters();
			_TextResultsPage.UndoEnabled = true;
			_TextMessagesPage.UndoEnabled = true;
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

	private void OnQueryExecutorStatusChanged(object sender, QueryManager.StatusChangedEventArgs args)
	{
		if (args.Change == QueryManager.EnStatusType.Connection)
		{
			_ClearStatisticsControl = true;
		}
	}

	private void OnSqlCmdOutputRedirection(object sender, QEOLESQLOutputRedirectionEventArgs args)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlCmdOutputRedirection", "", null);
		AbstractResultsWriter resultsWriter = _TextResultsPage.ResultsWriter;
		if (args.FullFileName != null && args.FullFileName != "stderr" && args.FullFileName != "stdout")
		{
			resultsWriter = new FileStreamResultsWriter(args.FullFileName, append: false);
		}

		if (args.OutputRedirectionCategory == EnQEOLESQLOutputCategory.Results)
		{
			if (_ResultsWriter != null && _ResultsWriter != _TextResultsPage.ResultsWriter)
			{
				_ResultsWriter.Flush();
			}

			if (_MessagesWriter != null && _MessagesWriter != _TextMessagesPage.ResultsWriter && _MessagesWriter != _ResultsWriter)
			{
				_MessagesWriter.Flush();
			}

			if (_ResultsWriter != null && _ResultsWriter != _TextResultsPage.ResultsWriter && _ResultsWriter != _ErrorsWriter)
			{
				_ResultsWriter.Close();
			}

			if (_MessagesWriter != null && _MessagesWriter != _TextMessagesPage.ResultsWriter && _MessagesWriter != _ResultsWriter && _MessagesWriter != _ErrorsWriter)
			{
				_MessagesWriter.Close();
			}

			_MessagesWriter = _ResultsWriter = resultsWriter;
		}
		else
		{
			if (args.OutputRedirectionCategory != EnQEOLESQLOutputCategory.Errors)
			{
				return;
			}

			if (_ErrorsWriter != null && _ErrorsWriter != _TextResultsPage.ResultsWriter)
			{
				_ErrorsWriter.Flush();
				if (_ErrorsWriter != _ResultsWriter && _ErrorsWriter != _MessagesWriter)
				{
					_ErrorsWriter.Close();
				}
			}

			_ErrorsWriter = resultsWriter;
		}

		if (args.OutputRedirectionCategory == EnQEOLESQLOutputCategory.Results && args.BatchConsumer is ResultsToGridBatchConsumer)
		{
			if (_ResultRedirBatchConsumer == null)
			{
				_ResultRedirBatchConsumer = new ResultsToTextOrFileBatchConsumer(this);
				ApplyLiveSettingsToBatchConsumer(_ResultRedirBatchConsumer, AuxDocData.LiveSettings);
			}

			args.BatchConsumer = _ResultRedirBatchConsumer;
		}
	}

	private void OnSqlCmdMsgFromApp(object sender, QeSqlCmdMessageFromAppEventArgs args)
	{
		// Tracer.Trace(GetType(), "DisplaySQLResultsControl.OnSqlCmdMsgFromApp", "", null);
		string message = args.Message;
		if (args.StdOut)
		{
			AddMessageToTextWriterCommon(message, -1, null, EnResultMessageType.Normal, _MessagesWriter, flush: true, noCr: true);
			return;
		}

		_HadExecutionErrors = true;
		AddMessageToTextWriterCommon(message, -1, null, EnResultMessageType.Error, _ErrorsWriter, flush: true, noCr: true);
	}
}
