// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.DisplaySQLResultsControl

using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Controls.Dialogs;
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

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql.Shared.Controls.Results;


// =========================================================================================================
//										ResultsHandler Class
//
/// <summary>
/// Handler for results output panes.
/// </summary>
// =========================================================================================================
public class ResultsHandler : IBsQueryExecutionHandler, IBsExecutionHandler, IDisposable
{

	// ---------------------------------------------------
	#region Constructors / Destructors - ResultsHandler
	// ---------------------------------------------------


	/// <summary>
	/// Default .ctor.
	/// </summary>
	public ResultsHandler(ResultPane resultsGridPanel, ResultPane messagePanel, ResultPane textResultsPanel,
		ResultPane statisticsPanel, /* ResultWindowPane executionPlanPanel,*/ ResultPane textPlanPanel,
		ResultPane spatialPane, IBsTabbedEditorPane tabbedEditor)
	{
		// Evs.Trace(typeof(ResultsHandler), ".ctor");

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
		_TabbedEditor = tabbedEditor;

		Initialize(resultsGridPanel, messagePanel);

		AuxDocData.LiveSettingsChangedEvent += OnLiveSettingsChanged;
		AuxDocData.OutputModeChangedEvent += OnOutputModeChanged;
		FontAndColorProviderGridResults.Instance.ColorChangedEvent += OnGridColorChanged;
		FontAndColorProviderGridResults.Instance.FontChangedEvent += OnGridFontChanged;
	}



	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}



	private void Dispose(bool disposing)
	{
		// Evs.Trace(GetType(), "Dispose(bool)");
		CleanupGrids();
		UnhookEvents();

		_ServiceProvider = null;
		_ObjServiceProvider = null;

		if (disposing)
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

		_ClientStatisticsCollection = null;

		if (_StatisticsPage != null)
		{
			_StatisticsPane?.Clear();
			_StatisticsPage.Dispose();
			_StatisticsPage = null;
		}

		// RemoveExecutionPlanPage();
		RemoveTextPlanPage();
	}


	private void Initialize(ResultPane gridResultsPanel, ResultPane messagePanel)
	{
		// Evs.Trace(GetType(), nameof(Initialize));

		ConfigureOutputMode(AuxDocData.SqlOutputMode);
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

		HookEvents(QryMgr);

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


	#endregion Constructors / Destructors

	// private const int C_MaxExecutionPlanControls = 100;
	private const int C_MaxGridResultSets = 10000000;




	// =========================================================================================================
	#region Fields - ResultsHandler
	// =========================================================================================================


	private AuxilliaryDocData _AuxDocData;
	private AbstractBatchConsumer _BatchConsumer;
	private bool _ClearStatisticsCollection;
	private StatisticsSnapshotCollection _ClientStatisticsCollection;
	private string _DefaultResultsDirectory = "";
	// private bool _ExecutionPlanMaxCountExceeded;
	private Font _FontGridResults = Control.DefaultFont;
	protected int _GridCount;
	private bool _HadExecutionErrors;
	private bool _HasMessages;
	// private bool _HasTextPlan;
	private bool _HasTextResults;
	private object _ObjServiceProvider;
	private EnSqlOutputMode _OutputMode = EnSqlOutputMode.Undefined;
	private QueryManager _QryMgr;
	private TextOrFileBatchConsumer _ResultRedirBatchConsumer;
	private ServiceProvider _ServiceProvider;
	private readonly IBsTabbedEditorPane _TabbedEditor = null;

	private readonly ResultPane _ResultsGridPane;
	private readonly ResultPane _MessagePane;
	private readonly ResultPane _TextResultsPane;
	private readonly ResultPane _StatisticsPane;
	// private readonly ResultPane _ExecutionPlanPane;
	private readonly ResultPane _TextPlanPane;
	private readonly ResultPane _SpatialResultsPane;

	private AbstractResultsWriter _ErrorsWriter;
	private AbstractResultsWriter _MessagesWriter;
	private AbstractResultsWriter _PlanWriter;
	private AbstractResultsWriter _ResultsWriter;

	// private ExecutionPlanPanel _ExecutionPlanPage;
	private GridResultsPanel _GridResultsPage;
	private StatisticsPanel _StatisticsPage;
	private VSTextEditorPanel _TextMessagesPage;
	private VSTextEditorPanel _TextResultsPage;
	private VSTextEditorPanel _TextPlanPage;

	private Color _BkGridColor = SystemColors.Window;
	private Color _GridColor = SystemColors.WindowText;
	private Color _SelectedCellColor = SystemColors.Highlight;
	private Color _InactiveCellColor = SystemColors.InactiveCaption;
	private Color _NullValueCellColor = SystemColors.InactiveCaption;
	private Color _HeaderRowColor = SystemColors.InactiveCaption;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - ResultsHandler
	// =========================================================================================================


	public AuxilliaryDocData AuxDocData
	{
		get
		{
			if (_AuxDocData == null)
			{
				if (TabbedEditor == null)
					Diag.ThrowException(new ApplicationException("TabbedEditorPane is null"));
				if (TabbedEditor.DocData == null)
					Diag.ThrowException(new ApplicationException("TabbedEditorPane.DocData is null"));

				_AuxDocData = TabbedEditor.AuxDocData;

				if (_AuxDocData == null)
					Diag.ThrowException(new ApplicationException("AuxDocData is null"));
			}

			return _AuxDocData;
		}
	}


	public QueryManager QryMgr
	{
		get
		{
			if (AuxDocData.QryMgr == null)
				Diag.ThrowException(new ApplicationException("QryMgr is null"));

			return AuxDocData.QryMgr;
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

			// Evs.Trace(GetType(), "DefaultResultsDirectory", "value = {0}", value);
			_DefaultResultsDirectory = value;

			if (_DefaultResultsDirectory != value)
			{
				if (_TextResultsPage != null)
					_TextResultsPage.DefaultResultsDirectory = value;

				if (_TextMessagesPage != null)
					_TextMessagesPage.DefaultResultsDirectory = value;

				if (_GridResultsPage != null)
					_GridResultsPage.DefaultResultsDirectory = value;

				if (_StatisticsPage != null)
					_StatisticsPage.DefaultResultsDirectory = value;

				/*
				if (_ExecutionPlanPage != null)
					_ExecutionPlanPage.DefaultResultsDirectory = value;
				*/

				if (_TextPlanPage != null)
					_TextPlanPage.DefaultResultsDirectory = value;
			}
		}
	}


	// public ResultWindowPane ExecutionPlanWindowPane => _ExecutionPlanPane;

	public ResultPane TextPlanWindowPane => _TextPlanPane;

	public IBsQESQLBatchConsumer BatchConsumer => _BatchConsumer;

	public bool CanAddMoreGrids => _GridCount < C_MaxGridResultSets;

	private bool CouldNotShowSomeGridResults => _GridCount > C_MaxGridResultSets;

	public IBsTabbedEditorPane TabbedEditor => _TabbedEditor;


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


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - ResultsHandler
	// =========================================================================================================


	public void ClearResultsTabs()
	{
		// Evs.Trace(GetType(), "ClearResultsTabs", "", null);
		ClearTabs();
	}



	public static StreamWriter GetTextWriterForResultsToFile(bool xmlResults, ref string intialDirectory)
	{
		// Evs.Trace(typeof(VS), "GetTextWriterForQueryResultsToFile()");

		FileEncodingDialog fileEncodingDlg = new FileEncodingDialog();
		string text = Properties.Resources.SqlExportFromGridFilterTabDelimitted;

		if (xmlResults)
			text = Properties.Resources.SqlXMLFileFilter;


		text = text + "|" + Properties.Resources.SqlExportFromGridFilterAllFiles;
		string fileNameUsingSaveDialog = UnsafeCmd.GetFileNameUsingSaveDialog(Cmd.CreateVsFilterString(text), Properties.Resources.SaveResults, intialDirectory, fileEncodingDlg);

		if (fileNameUsingSaveDialog != null)
		{
			intialDirectory = Cmd.GetDirectoryName(fileNameUsingSaveDialog);
			return new StreamWriter(fileNameUsingSaveDialog, append: false, fileEncodingDlg.Encoding, 8192)
			{
				AutoFlush = false
			};
		}

		return null;
	}



	public void SetSite(object sp)
	{
		// Evs.Trace(GetType(), "SetSite", "", null);

		Diag.ThrowIfNotOnUIThread();

		if (_ServiceProvider != null)
			_ServiceProvider = null;

		_ServiceProvider = new ServiceProvider((IOleServiceProvider)sp);
		_ObjServiceProvider = sp;
		OnHosted();
	}



	public bool PrepareForExecution(bool prepareForParse)
	{
		// Evs.Trace(GetType(), "PrepareForExecution", "prepareForParse = {0}", prepareForParse);
		QryMgr.ResultsConsumer = BatchConsumer;
		AbstractResultsWriter resultsWriter = null;

		if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToFile && !prepareForParse
			&& !AuxDocData.LiveSettings.EditorResultsTextDiscardResults && !PlanOnly)
		{
			StreamWriter textWriterForQueryResultsToFile = GetTextWriterForResultsToFile(xmlResults: false, ref _DefaultResultsDirectory);
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
		// Evs.Trace(GetType(), "SetGridResultsFont", "", null);
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
		// Evs.Trace(GetType(), "SetGridResultsColors", "bkColor = {0}, fkColor = {1}", bkColor, fkColor);
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
		// Evs.Trace(GetType(), "SetGridSelectedCellColor", "color = {0}", selectedCellColor);
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
		// Evs.Trace(GetType(), "SetGridInactiveSelectedCellColor", "color = {0}", inactiveSelectedCellColor);
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
		// Evs.Trace(GetType(), "SetGridNullValueColor", "color = {0}", nullValueCellColor);
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
		// Evs.Trace(GetType(), "SetHeaderRowColor", "color = {0}", headerRowColor);
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
		// Evs.Trace(GetType(), nameof(OnErrorMessage), "", null);

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
		// Evs.Trace(GetType(), nameof(OnHosted), "", null);
		_GridResultsPage.Initialize(_ObjServiceProvider);
		_TextResultsPage.Initialize(_ObjServiceProvider);
		_TextMessagesPage.Initialize(_ObjServiceProvider);
		_TextPlanPage?.Initialize(_ObjServiceProvider);
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
		// Evs.Trace(GetType(), "AddGridContainer", "", null);


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
		// Evs.Trace(GetType(), "ProcessSpecialActionOnBatch", "", null);
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
			// Evs.Trace(GetType(), "ProcessSpecialActionOnBatch", "Execution plan visualizer under development. Exception: {0}", e.Message);
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
		// Evs.Trace(GetType(), "ProcessSpecialActionOnBatchInt", "", null);
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
					TabbedWindowPane.ActivateMessageTab();
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
		// Evs.Trace(GetType(), "ProcessSpecialActionOnBatchInt", "Text plan source: " + dataSource, null);

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
			return;

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
			return;

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
		// Evs.Trace(GetType(), "Clear", "", null);
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
		// Evs.Trace(GetType(), "RemoveStatisticsTab", "", null);
		if (_StatisticsPage != null && _StatisticsPane != null && _StatisticsPane.Contains(_StatisticsPage))
		{
			_StatisticsPane.Remove(_StatisticsPage);
		}
	}



	/*
	public void RemoveExecutionPlanPage()
	{
		// Evs.Trace(GetType(), "RemoveExecutionPlanTab", "", null);
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
		// Evs.Trace(GetType(), "RemoveTextPlanTab", "", null);
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
		// Evs.Trace(GetType(), "CleanupGrids", "", null);
		_GridCount = 0;
		_GridResultsPage?.Clear();
	}



	private void PrepareTabs(bool isParseOnly)
	{
		// Evs.Trace(GetType(), "PrepareTabs", "", null);
		ClearTabs();

		TabbedEditor.CustomizeTabsForResultsSetting(isParseOnly);
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
		// Evs.Trace(GetType(), "OutputQueryIntoMessages", "", null);
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
		// Evs.Trace(GetType(), "AddGridContainerInt", "", null);
		if (_GridCount == C_MaxGridResultSets)
		{
			MarkAsCouldNotAddMoreGrids();
			Exception ex = new ResultsException(ControlsResources.ExCanDisplayOnlyNGridResults.FmtRes(C_MaxGridResultSets));
			throw ex;
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



	private void ConfigureOutputMode(EnSqlOutputMode mode)
	{
		// Evs.Trace(GetType(), nameof(ConfigureOutputMode));

		if (_OutputMode == mode && _BatchConsumer != null)
			return;

		_OutputMode = mode;

		if (_BatchConsumer != null)
		{
			_BatchConsumer.Dispose();
			_BatchConsumer = null;
		}

		switch (mode)
		{
			case EnSqlOutputMode.ToText:
			case EnSqlOutputMode.ToFile:
				_BatchConsumer = new TextOrFileBatchConsumer(this);
				break;
			case EnSqlOutputMode.ToGrid:
				_BatchConsumer = new GridBatchConsumer(this);
				break;
			default:
				_BatchConsumer = new GridBatchConsumer(this);
				break;
		}
	}



	private void ApplyLiveSettingsToBatchConsumer(AbstractBatchConsumer batchConsumer, IBsEditorTransientSettings liveSettings)
	{
		// Evs.Trace(GetType(), nameof(ApplyLiveSettingsToBatchConsumer));

		batchConsumer.MaxCharsPerColumn = MaxCharsPerColumn;
		batchConsumer.DiscardResults = ShouldDiscardResults;
		if (batchConsumer is TextOrFileBatchConsumer obj)
		{
			obj.ColumnsDelimiter = liveSettings.EditorResultsTextDelimiter;
			obj.PrintColumnHeaders = liveSettings.EditorResultsTextIncludeHeaders;
			obj.RightAlignNumerics = liveSettings.EditorResultsTextAlignRightNumerics;
		}
	}

	private GridResultsPanel AllocateNewGridTabPage()
	{
		// Evs.Trace(GetType(), "AllocateNewGridTabPage", "", null);
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
		// Evs.Trace(GetType(), "ClearTabs", "", null);
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
		// Evs.Trace(GetType(), "FlushAllTextWriters", "", null);

		try
		{
			if (_ResultsWriter != null)
				FlushTextWritersInt(_ResultsWriter);
		}
		catch { }

		if (_MessagesWriter != _ResultsWriter && _MessagesWriter != null)
		{
			try { _MessagesWriter.Flush(); }
			catch { }
		}

		if (_ErrorsWriter != _MessagesWriter && _ErrorsWriter != _ResultsWriter
			&& _ErrorsWriter != null)
		{
			try { _ErrorsWriter.Flush(); }
			catch { }
		}

		if (_MessagesWriter != _TextResultsPage.ResultsWriter
			&& _ErrorsWriter != _TextResultsPage.ResultsWriter)
		{
			try { _TextResultsPage.ResultsWriter.Flush(); }
			catch { }
		}

		if (_PlanWriter != null)
		{
			try { _PlanWriter.Flush(); }
			catch { }
		}
	}



	private void CheckAndCloseTextWriters()
	{
		// Evs.Trace(GetType(), nameof(CheckAndCloseTextWriters));

		if (_ResultsWriter != null && _ResultsWriter is FileStreamResultsWriter)
		{
			try { _ResultsWriter.Close(); }
			catch { }
		}

		if (_MessagesWriter != null && _MessagesWriter != _ResultsWriter
			&& _MessagesWriter is FileStreamResultsWriter)
		{
			try { _MessagesWriter.Close(); }
			catch { }
		}

		if (_ErrorsWriter != null && _ErrorsWriter != _ResultsWriter
			&& _ErrorsWriter != _MessagesWriter && _ErrorsWriter is FileStreamResultsWriter)
		{
			try { _ErrorsWriter.Close(); }
			catch { }
		}

		_ResultsWriter = _MessagesWriter = _ErrorsWriter = null;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - ResultsHandler
	// =========================================================================================================


	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public virtual void OnBatchDataLoaded(object sender, QueryDataEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnQueryDataLoaded));

		if (args.StatementAction == EnSqlStatementAction.ProcessQuery
			&& args.ExecutionType != EnSqlExecutionType.PlanOnly && args.WithClientStats)
		{
			_ClientStatisticsCollection.RetrieveStatisticsIfNeeded(args);
		}
	}



	public virtual void OnBatchScriptParsed(object sender, QueryDataEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnBatchScriptParsed));
	}


	public virtual void OnBatchStatementCompleted(object sender, BatchStatementCompletedEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnStatementCompleted));
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



	private void OnLiveSettingsChanged(object sender, LiveSettingsEventArgs args)
	{
		OnOutputModeChanged(sender, args);
		_GridResultsPage?.SetGridTabOptions(args.LiveSettings.EditorResultsGridSaveIncludeHeaders,
			args.LiveSettings.EditorResultsGridCsvQuoteStringsCommas);
		DefaultResultsDirectory = args.LiveSettings.EditorResultsDirectory;
	}



	private void OnOutputModeChanged(object sender, LiveSettingsEventArgs args)
	{
		ConfigureOutputMode(args.LiveSettings.EditorResultsOutputMode);
		ApplyLiveSettingsToBatchConsumer(_BatchConsumer, args.LiveSettings);
	}



	private async Task<bool> OnQueryExecutionCompletedAsync(object sender, ExecutionCompletedEventArgs args)
	{
		// Evs.Trace(GetType(), nameof(OnQueryExecutionCompletedAsync));

		/* Moved to OnBatchDataLoaded(()
		if (!args.IsParseOnly && AuxDocData.ClientStatisticsEnabled && _ClientStatisticsCtl != null)
		{
			_ClientStatisticsCtl.RetrieveStatisticsIfNeeded(_QryMgr);
		}
		*/
		if (args.CancelToken.Cancelled() || !args.Launched)
			return true;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		if (args.CancelToken.Cancelled() || TabbedEditor == null)
			return true;
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

		bool result = true;

		try
		{

			if (_TextPlanPage != null && !_TextPlanPane.Contains(_TextPlanPage))
			{
				_TextPlanPane.Add(_TextPlanPage);

				Guid guid = new Guid(LibraryData.C_TextPlanTabGuid);
				EditorMessageTab tab;

				tab = TabbedEditor.GetEditorTab<EditorMessageTab>(guid);


				TabbedEditor.ConfigureTextViewForAutonomousFind(tab.CurrentFrame, TextPlanPaneTextView);
			}


			if (AuxDocData.ClientStatisticsEnabled && args.ExecutionType != EnSqlExecutionType.PlanOnly && _ClientStatisticsCollection != null)
			{
				_StatisticsPage = new StatisticsPanel(_DefaultResultsDirectory);
				_StatisticsPage.Initialize(_ObjServiceProvider);
				_StatisticsPane.Clear();
				_StatisticsPage.Dock = DockStyle.Fill;
				_StatisticsPage.Name = "ClientStatisticsPanel";
				_StatisticsPane.Add(_StatisticsPage);
				_StatisticsPage.PopulateFromStatisticsCollection(_ClientStatisticsCollection, _FontGridResults,
				_BkGridColor, _GridColor, _SelectedCellColor, _InactiveCellColor, _HeaderRowColor);
			}

			if (_HadExecutionErrors)
			{
				TabbedEditor.ActivateMessageTab();
			}
			else if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToGrid)
			{
				if (_GridResultsPage.NumberOfGrids > 0)
					TabbedEditor.ActivateResultsTab();
				else if (_GridResultsPage.NumberOfGrids == 0)
					TabbedEditor.IsButtonVisibleResultsGrid = false;
			}
			else if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToText)
			{
				if (string.IsNullOrWhiteSpace(_TextResultsPage.TextViewCtl.TextBuffer.Text))
					TabbedEditor.IsButtonVisibleTextResults = false;
				else
					TabbedEditor.ActivateTextResultsTab();
			}

			if (!AutoSelectResultsTab)
				TabbedEditor.ActivateCodeTab();


			// if (!_QryMgr.IsExecuting)
			//	return;

			if (!_HadExecutionErrors)
				_HadExecutionErrors = (args.ExecutionResult & EnScriptExecutionResult.Failure) != 0;


			if (AuxDocData.SqlOutputMode == EnSqlOutputMode.ToGrid && CouldNotShowSomeGridResults)
			{
				_TextMessagesPage.ResultsWriter.AppendError(ControlsResources.ExCanDisplayOnlyNGridResults.FmtRes(C_MaxGridResultSets));

				_HasMessages = true;
				_HadExecutionErrors = true;
			}

			if (!ShouldDiscardResults && !_HasMessages && args.ExecutionResult == EnScriptExecutionResult.Success)
			{
				string text = _QryMgr.Strategy.GetCustomQuerySuccessMessage();

				text ??= ControlsResources.MsgCommandSuccess;

				_TextMessagesPage.ResultsWriter.AppendNormal(text);

				if (!_HasTextResults && AuxDocData.SqlOutputMode == EnSqlOutputMode.ToFile && _ResultsWriter != null)
				{
					_ResultsWriter.AppendNormal(text);
				}
			}

			if (!ShouldDiscardResults && args.ExecutionResult == EnScriptExecutionResult.Cancel)
			{
				_TextMessagesPage.ResultsWriter.AppendNormal(_QryMgr.LiveTransactions
					? ControlsResources.MsgQueryCancelledRollback : ControlsResources.MsgQueryCancelled);
			}

			FlushAllTextWriters();
			CheckAndCloseTextWriters();

			_TextResultsPage.UndoEnabled = true;
			_TextMessagesPage.UndoEnabled = true;
		}
		catch (Exception e)
		{
			result = false;
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
					result = false;
					Diag.Dug(e2);
				}
			}
		}

		args.Result &= result;

		return result;

	}

	private async Task<bool> OnQueryExecutionStartedAsync(object sender, ExecutionStartedEventArgs args)
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


		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


		bool hasSaveTo = PrepareForExecution(false);

		if (hasSaveTo && ShouldOutputQuery)
		{
			OutputQueryIntoMessages(args.QueryText);
		}

		// _ExecutionPlanMaxCountExceeded = false;
		return hasSaveTo;
	}



	private void OnQueryManagerStatusChanged(object sender, QueryStateChangedEventArgs args)
	{
		if (args.DatabaseChanged)
			_ClearStatisticsCollection = true;
	}


	private void HookEvents(QueryManager qryMgr)
	{
		if (_QryMgr == null)
		{
			_QryMgr = qryMgr;
			_QryMgr.ExecutionStartedEventAsync += OnQueryExecutionStartedAsync;
			_QryMgr.ExecutionCompletedEventAsync += OnQueryExecutionCompletedAsync;
			_QryMgr.ErrorMessageEvent += OnErrorMessage;
			// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
			_QryMgr.BatchScriptParsedEvent += OnBatchScriptParsed;
			_QryMgr.BatchDataLoadedEvent += OnBatchDataLoaded;
			_QryMgr.BatchStatementCompletedEvent += OnBatchStatementCompleted;
			_QryMgr.StatusChangedEvent += OnQueryManagerStatusChanged;
		}
	}

	private void UnhookEvents()
	{
		// Evs.Trace(GetType(), "UnhookFromEvents", "", null);

		if (_QryMgr != null)
		{
			_QryMgr.ExecutionStartedEventAsync -= OnQueryExecutionStartedAsync;
			_QryMgr.ExecutionCompletedEventAsync -= OnQueryExecutionCompletedAsync;
			_QryMgr.ErrorMessageEvent -= OnErrorMessage;
			// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
			_QryMgr.BatchScriptParsedEvent -= OnBatchScriptParsed;
			_QryMgr.BatchDataLoadedEvent -= OnBatchDataLoaded;
			_QryMgr.BatchStatementCompletedEvent -= OnBatchStatementCompleted;
			_QryMgr.StatusChangedEvent -= OnQueryManagerStatusChanged;
			_QryMgr = null;
		}

		AuxDocData.OutputModeChangedEvent -= OnOutputModeChanged;
		AuxDocData.LiveSettingsChangedEvent -= OnLiveSettingsChanged;
		FontAndColorProviderGridResults.Instance.ColorChangedEvent -= OnGridColorChanged;
		FontAndColorProviderGridResults.Instance.FontChangedEvent -= OnGridFontChanged;
	}


	#endregion Event Handling

}
