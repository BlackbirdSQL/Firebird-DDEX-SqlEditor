// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.ShowPlanControl
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing;
using BlackbirdSql.Common.Controls.Graphing.Enums;
using BlackbirdSql.Common.Controls.Graphing.Gram;
using BlackbirdSql.Common.Controls.Graphing.Interfaces;
using BlackbirdSql.Common.Controls.QueryExecution;
using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Enums;

using Microsoft.AnalysisServices.Graphing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;

using Tracer = BlackbirdSql.Core.Ctl.Diagnostics.Tracer;

namespace BlackbirdSql.Common.Controls.ResultsPane;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "UIThread compliance is performed by the class.")]

public class ExecutionPlanControl : UserControl, BlackbirdSql.Common.Controls.Interfaces.IBObjectWithSite, IOleCommandTarget
{
	private class DataBinding
	{
		public readonly object DataSource;

		public readonly int DataIndex;

		public DataBinding(object dataSource, int dataIndex)
		{
			DataSource = dataSource;
			DataIndex = dataIndex;
		}
	}

	private Dictionary<GraphPanel, DataBinding> dataBindings;

	// private const string TraceName = "ExecutionPlan";

	private System.IServiceProvider serviceProvider;

	private MultiControlPanel multiControlPanel;

	private GraphPanel currentGraphPanel;

	private MenuCommandsService _MenuService = new MenuCommandsService();

	private PageSettings cachedPageSettings;

	private PrinterSettings cachedPrinterSettings;

	private bool shouldResizePanels;

	private string sqlplanFile;

	public int GraphPanelCount => multiControlPanel.HostedControlsCount;

	public ExecutionPlanControl()
	{
		Initialize();
	}

	public GraphPanel GetGraphPanel(int index)
	{
		if (index < 0 || index >= GraphPanelCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return multiControlPanel.GetHostedControl(index) as GraphPanel;
	}

	public GraphControl GetGraphControl(int index)
	{
		if (index < 0 || index >= GraphPanelCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return (multiControlPanel.GetHostedControl(index) as GraphPanel).GraphControl;
	}

	public void LoadXml(string xmlFile)
	{
		sqlplanFile = xmlFile;
		try
		{
			string dataSource;
			StreamReader streamReader;
			using (streamReader = new StreamReader(xmlFile, Encoding.Default))
			{
				dataSource = streamReader.ReadLine();
			}
			multiControlPanel.Clear();
			currentGraphPanel = null;
			INodeBuilder nodeBuilder = NodeBuilderFactory.Create(dataSource, EnExecutionPlanType.Unknown);
			using (streamReader = new StreamReader(xmlFile, Encoding.Default))
			{
				dataSource = streamReader.ReadToEnd();
			}
			IGraph[] array = nodeBuilder.Execute(dataSource);
			IGraph[] graphs = array;
			AddGraphs(graphs, dataSource);
		}
		catch (Exception innerException)
		{
			throw new ApplicationException(string.Format(CultureInfo.CurrentCulture, ControlsResources.FailedToLoadExecutionPlanFile, xmlFile), innerException);
		}
	}

	public void SaveXml(string xmlFile)
	{
		if (GraphPanelCount == 0)
		{
			throw new InvalidOperationException();
		}
		if (dataBindings == null || !dataBindings.TryGetValue(GetGraphPanel(0), out DataBinding value))
		{
			throw new InvalidOperationException();
		}
		using StreamWriter streamWriter = new StreamWriter(xmlFile);
		streamWriter.Write(value.DataSource);
	}

	public void AddGraphs(IGraph[] graphs, object dataSource)
	{
		multiControlPanel.SuspendLayout();
		int num = 0;
		foreach (IGraph graph in graphs)
		{
			if (!graph.Nodes.GetEnumerator().MoveNext())
			{
				continue;
			}
			GraphPanel graphPanel = CreateGraphPanel();
			if (graphPanel == null)
			{
				continue;
			}
			graphPanel.SetGraph(graph);
			if (dataSource != null)
			{
				dataBindings ??= new Dictionary<GraphPanel, DataBinding>();
				dataBindings.Add(graphPanel, new DataBinding(dataSource, num++));
			}
		}
		UpdateAllPanelDescriptions();
		shouldResizePanels = true;
		if (multiControlPanel.Height > 0 && base.Parent != null && base.Parent.Parent != null)
		{
			shouldResizePanels = false;
			multiControlPanel.ResizeControlsToPreferredHeight(limitMaxControlHeightToClientArea: true);
		}
		multiControlPanel.ResumeLayout();
		if (currentGraphPanel == null && GraphPanelCount > 0)
		{
			GetGraphPanel(0).Activate();
		}
	}

	void BlackbirdSql.Common.Controls.Interfaces.IBObjectWithSite.SetSite(System.IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
		OnHosted();
	}

	int IOleCommandTarget.QueryStatus(ref Guid guidGroup, uint cmdID, OLECMD[] oleCmd, IntPtr oleText)
	{
		CommandID commandID = new CommandID(guidGroup, (int)oleCmd[0].cmdID);
		MenuCommand menuCommand = _MenuService.FindCommand(commandID);
		if (menuCommand != null)
		{
			if (guidGroup.Equals(VSConstants.GUID_VSStandardCommandSet97))
			{
				int iD = commandID.ID;
				if (iD == 27 || iD == 227)
				{
					bool visible = (menuCommand.Supported = true);
					menuCommand.Visible = visible;
					oleCmd[0].cmdf = (uint)menuCommand.OleStatus;
					return 0;
				}
			}
			else if (guidGroup.Equals(LibraryData.CLSID_CommandSet))
			{
				bool enabled = false;
				if (dataBindings != null && dataBindings.TryGetValue(GetGraphPanel(0), out DataBinding value))
				{
					enabled = value.DataSource is not IDataReader;
				}
				bool visible = (menuCommand.Supported = true);
				menuCommand.Visible = visible;
				switch ((uint)commandID.ID)
				{
				case 790u:
					menuCommand.Enabled = true;
					break;
				case 785u:
				case 791u:
					menuCommand.Enabled = enabled;
					break;
				case 788u:
					menuCommand.Enabled = enabled;
					break;
				case 789u:
					menuCommand.Enabled = true;
					menuCommand.Visible = currentGraphPanel.DescriptionCtl.HasMissingIndex;
					break;
				}
				oleCmd[0].cmdf = (uint)menuCommand.OleStatus;
				return 0;
			}
		}
		return -2147221248;
	}

	int IOleCommandTarget.Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr vIn, IntPtr vOut)
	{
		CommandID commandID = new CommandID(guidGroup, (int)nCmdId);
		MenuCommand menuCommand = _MenuService.FindCommand(commandID);
		if (menuCommand != null)
		{
			Tracer.Trace(GetType(), "Exec", "{0}:{1}", commandID.Guid, commandID.ID);
			try
			{
				menuCommand.Invoke();
			}
			catch (Exception e)
			{
				Tracer.LogExCatch(GetType(), e);
				Cmd.ShowExceptionInDialog(string.Empty, e);
			}
			return 0;
		}
		return -2147221248;
	}

	protected override void Dispose(bool disposing)
	{
		Font font = null;
		if (disposing)
		{
			if (multiControlPanel != null)
			{
				for (int i = 0; i < GraphPanelCount; i++)
				{
					GetGraphPanel(i).IsActiveChangedEvent -= OnGraphActiveChanged;
				}
				multiControlPanel.Dispose();
				multiControlPanel = null;
			}
			serviceProvider = null;
			if (_MenuService != null)
			{
				_MenuService.Dispose();
				_MenuService = null;
			}
			cachedPageSettings = null;
			cachedPrinterSettings = null;
			currentGraphPanel = null;
			if (Font != base.Parent.Font)
			{
				font = Font;
				Font = base.Parent.Font;
			}
		}
		base.Dispose(disposing);
		font?.Dispose();
	}

	protected virtual void OnHosted()
	{
		Font = GetExecutionPlanFont();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		if (multiControlPanel != null)
		{
			multiControlPanel.HostedControlsMinSize = 5 * Font.Height;
			for (int i = 0; i < GraphPanelCount; i++)
			{
				GetGraphPanel(i).PerformLayout();
			}
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (shouldResizePanels)
		{
			shouldResizePanels = false;
			multiControlPanel.SuspendLayout();
			multiControlPanel.ResizeControlsToPreferredHeight(limitMaxControlHeightToClientArea: true);
			multiControlPanel.ResumeLayout();
			multiControlPanel.PerformLayout();
		}
	}

	private void OnExecutionPlanXml(object sender, EventArgs a)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		string text = string.Empty;
		try
		{
			string showPlanXml = GetShowPlanXml();
			string pszItemName = string.Format(CultureInfo.InvariantCulture, "{0}.xml", ControlsResources.ExecutionPlan);
			if (!string.IsNullOrEmpty(sqlplanFile))
			{
				string name = new System.IO.FileInfo(sqlplanFile).Name;
				if (name.EndsWith(".sqlplan", StringComparison.OrdinalIgnoreCase))
				{
					pszItemName = string.Format(CultureInfo.InvariantCulture, "{0}.xml", name[..^8]);
				}
			}
			int num = 0;
			do
			{
				string tempFileName = Path.GetTempFileName();
				text = string.Format(CultureInfo.InvariantCulture, "{0}.xml", tempFileName);
				File.Delete(tempFileName);
				num++;
			}
			while (File.Exists(text) && num < 100);
			if (num == 100)
			{
				throw new InvalidOperationException(ControlsResources.ErrCannotCreateTempFile);
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(showPlanXml);
			xmlDocument.Save(text);
			if (Package.GetGlobalService(typeof(IVsExternalFilesManager)) is not IVsExternalFilesManager obj)
			{
				Exception ex = new InvalidOperationException(ControlsResources.ErrCannotGetExternalFilesManager);
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}
			Native.ThrowOnFailure(obj.GetExternalFilesProject(out IVsProject ppProject));
			VSADDRESULT[] pResult = new VSADDRESULT[1];
			Native.ThrowOnFailure(ppProject.AddItem(4294967294u, VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE, pszItemName, 1u, new string[1] { text }, IntPtr.Zero, pResult));
			if (Package.GetGlobalService(typeof(IVsMonitorSelection)) is not IVsMonitorSelection)
			{
				Exception ex2 = new InvalidOperationException();
				Tracer.LogExThrow(GetType(), ex2);
				throw ex2;
			}
		}
		finally
		{
			if (!string.IsNullOrEmpty(text) && File.Exists(text))
			{
				File.Delete(text);
			}
		}
	}

	internal void OnMissingIndexDetails(object sender, EventArgs a)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (!currentGraphPanel.DescriptionCtl.HasMissingIndex)
			return;

		string missingIndexDatabase = currentGraphPanel.DescriptionCtl.MissingIndexDatabase;
		string missingIndexImpact = currentGraphPanel.DescriptionCtl.MissingIndexImpact;
		string missingIndexQueryText = currentGraphPanel.DescriptionCtl.MissingIndexQueryText;
		if (Package.GetGlobalService(typeof(IVsMonitorSelection)) is IVsMonitorSelection vsMonitorSelection)
		{
			vsMonitorSelection.GetCurrentElementValue(2u, out object pvarValue);
			if (pvarValue == null)
			{
				Exception ex = new NullReferenceException("DocumentFrame");
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}
			IVsWindowFrame obj = (IVsWindowFrame)pvarValue;
			Native.ThrowOnFailure(obj.GetProperty(-3004, out object pvar));
			string text = pvar.ToString();
			text = text.Replace("*", "");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, ControlsResources.MissingIndexDetailsTitle, text, missingIndexImpact));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("/*");
			stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "USE {0}", missingIndexDatabase));
			stringBuilder.AppendLine("GO");
			stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}", missingIndexQueryText));
			stringBuilder.AppendLine("GO");
			stringBuilder.AppendLine("*/");
			Cmd.OpenNewMiscellaneousSqlFile(new ServiceProvider(Controller.OleServiceProvider), stringBuilder.ToString());
		}
	}

	private void OnSave(object sender, EventArgs a)
	{
		try
		{
			string showPlanXml = GetShowPlanXml();
			Stream stream = null;
			StreamWriter streamWriter = null;
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = ControlsResources.ExecutionPlanFilter,
				RestoreDirectory = true
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK && (stream = saveFileDialog.OpenFile()) != null)
			{
				try
				{
					streamWriter = new StreamWriter(stream);
					streamWriter.Write(showPlanXml);
					return;
				}
				finally
				{
					streamWriter?.Close();
					stream.Close();
				}
			}
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
			Cmd.ShowExceptionInDialog(ControlsResources.ErrorWhileSavingExecutionPlan, e);
		}
	}

	private string GetShowPlanXml()
	{
		if (!string.IsNullOrEmpty(sqlplanFile))
		{
			return GetSqlPlanFileXml();
		}
		return GetExecutionPlanXml();
	}

	private string GetExecutionPlanXml()
	{
		ArrayList arrayList = new ArrayList();
		List<StmtBlockType> list = new List<StmtBlockType>();
		object obj = null;
		bool flag = false;
		string version = string.Empty;
		string build = string.Empty;
		for (int i = 0; i < GraphPanelCount; i++)
		{
			if (dataBindings == null || !dataBindings.TryGetValue(GetGraphPanel(i), out DataBinding value))
			{
				throw new InvalidOperationException();
			}
			IXmlBatchParser xmlBatchParser = NodeBuilderFactory.Create(value.DataSource, EnExecutionPlanType.Unknown) as IXmlBatchParser;
			if (!flag)
			{
				using (StringReader textReader = new StringReader(xmlBatchParser.GetSingleStatementXml(value.DataSource, value.DataIndex)))
				{
					ExecutionPlanXML obj2 = new XmlSerializer(typeof(ExecutionPlanXML)).Deserialize(textReader) as ExecutionPlanXML;
					version = obj2.Version;
					build = obj2.Build;
				}
				flag = true;
			}
			StmtBlockType singleStatementObject = xmlBatchParser.GetSingleStatementObject(value.DataSource, value.DataIndex);
			if (value.DataSource != obj)
			{
				if (obj != null)
				{
					arrayList.Add(list);
					list = new List<StmtBlockType>();
				}
				obj = value.DataSource;
			}
			list.Add(singleStatementObject);
		}
		arrayList.Add(list);
		ExecutionPlanXML showPlanXML = new ExecutionPlanXML
		{
			Version = version,
			Build = build,
			BatchSequence = new StmtBlockType[arrayList.Count][]
		};
		for (int j = 0; j < arrayList.Count; j++)
		{
			showPlanXML.BatchSequence[j] = (arrayList[j] as List<StmtBlockType>).ToArray();
		}
		StringBuilder stringBuilder = new StringBuilder();
		StringWriter textWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		new XmlSerializer(typeof(ExecutionPlanXML)).Serialize(textWriter, showPlanXML);
		return stringBuilder.ToString();
	}

	private string GetSqlPlanFileXml()
	{
		if (dataBindings == null || !dataBindings.TryGetValue(currentGraphPanel, out DataBinding value))
		{
			throw new InvalidOperationException();
		}
		if (value != null)
		{
			return value.DataSource.ToString();
		}
		return string.Empty;
	}

	private void OnZoomIn(object sender, EventArgs a)
	{
		currentGraphPanel.GraphControl.ZoomIn();
	}

	private void OnZoomOut(object sender, EventArgs a)
	{
		currentGraphPanel.GraphControl.ZoomOut();
	}

	private void OnZoomCustom(object sender, EventArgs a)
	{
		currentGraphPanel.GraphControl.ShowCustomZoomDialog();
	}

	private void OnZoomToFit(object sender, EventArgs a)
	{
		currentGraphPanel.GraphControl.ScaleToFit();
	}

	internal void OnPrint(object sender, EventArgs e)
	{
		Tracer.Trace(GetType(), "OnPrint", "", null);
		if (currentGraphPanel == null)
		{
			return;
		}
		try
		{
			using PrintDialog printDialog = new PrintDialog();
			printDialog.Document = currentGraphPanel.GraphControl.GetPrintDocument();
			if (cachedPageSettings != null)
			{
				printDialog.Document.DefaultPageSettings = cachedPageSettings;
			}
			if (cachedPrinterSettings != null)
			{
				printDialog.Document.PrinterSettings = cachedPrinterSettings;
			}
			if (printDialog.ShowDialog(this) == DialogResult.OK)
			{
				printDialog.Document.Print();
			}
		}
		catch (Exception e2)
		{
			Tracer.LogExCatch(GetType(), e2);
			Cmd.ShowExceptionInDialog(ControlsResources.ErrUnableToPrintResults, e2);
		}
	}

	internal void OnPrintPreview(object sender, EventArgs e)
	{
		Tracer.Trace(GetType(), "OnPrintPreview", "", null);
		if (currentGraphPanel == null)
		{
			return;
		}
		try
		{
			using PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
			printPreviewDialog.Document = currentGraphPanel.GraphControl.GetPrintDocument();
			if (cachedPageSettings != null)
			{
				printPreviewDialog.Document.DefaultPageSettings = cachedPageSettings;
			}
			if (cachedPrinterSettings != null)
			{
				printPreviewDialog.Document.PrinterSettings = cachedPrinterSettings;
			}
			printPreviewDialog.ShowDialog(this);
		}
		catch (Exception e2)
		{
			Tracer.LogExCatch(GetType(), e2);
			Cmd.ShowExceptionInDialog(ControlsResources.ErrUnableToPrintPreview, e2);
		}
	}

	internal void OnPrintPageSetup(object sender, EventArgs e)
	{
		Tracer.Trace(GetType(), "OnPrintPageSetup", "", null);
		try
		{
			using PageSetupDialog pageSetupDialog = new PageSetupDialog();
			cachedPageSettings ??= new PageSettings();
			cachedPrinterSettings ??= new PrinterSettings();
			pageSetupDialog.PageSettings = cachedPageSettings;
			pageSetupDialog.PrinterSettings = cachedPrinterSettings;
			pageSetupDialog.AllowPrinter = true;
			pageSetupDialog.ShowDialog(this);
		}
		catch (Exception e2)
		{
			Tracer.LogExCatch(GetType(), e2);
			Cmd.ShowExceptionInDialog(ControlsResources.ErrUnableToPageSetup, e2);
		}
	}

	private void Initialize()
	{
		multiControlPanel = new()
		{
			Name = "MultiControlPanel",
			Dock = DockStyle.Fill,
			HostedControlsMinSize = 5 * Font.Height
		};
		base.Controls.Add(multiControlPanel);
		InitializeMenuCommands();
	}

	private void InitializeMenuCommands()
	{
		Guid clsid = LibraryData.CLSID_CommandSet;
		MenuCommand[] cmds = new MenuCommand[10]
		{
			new MenuCommand(OnSave, new CommandID(clsid, (int)EnCommandSet.CmdIdExecutionPlanSave)),
			new MenuCommand(OnExecutionPlanXml, new CommandID(clsid, (int)EnCommandSet.CmdIdExecutionPlanXml)),
			new MenuCommand(OnMissingIndexDetails, new CommandID(clsid, (int)EnCommandSet.CmdIdExecutionPlanMissingIndex)),
			new MenuCommand(OnZoomIn, new CommandID(clsid, (int)EnCommandSet.CmdIdExecutionPlanZoomIn)),
			new MenuCommand(OnZoomOut, new CommandID(clsid, (int)EnCommandSet.CmdIdExecutionPlanZoomOut)),
			new MenuCommand(OnZoomCustom, new CommandID(clsid, (int)EnCommandSet.CmdIdExecutionPlanZoomCustom)),
			new MenuCommand(OnZoomToFit, new CommandID(clsid, (int)EnCommandSet.CmdIdExecutionPlanZoomToFit)),
			new MenuCommand(OnPrintPreview, new CommandID(clsid, (int)EnCommandSet.CmdIdPrintPreview)),
			new MenuCommand(OnPrint, new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Print)),
			new MenuCommand(OnPrintPageSetup, new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.PageSetup))
		};
		_MenuService.AddRange(cmds);
	}

	private new object GetService(Type serviceType)
	{
		return serviceProvider.GetService(serviceType);
	}

	private GraphPanel CreateGraphPanel()
	{
		GraphPanel graphPanel;
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			graphPanel = new GraphPanel();
		}
		graphPanel.Name = "GraphPanel";
		graphPanel.IsActiveChangedEvent += OnGraphActiveChanged;
		multiControlPanel.AddControl(graphPanel);
		return graphPanel;
	}

	private void UpdateAllPanelDescriptions()
	{
		double totalCost = GetTotalCost();
		bool flag = IsClusteredMode();
		for (int i = 0; i < GraphPanelCount; i++)
		{
			if (i == 0 && flag)
			{
				SetGraphPanelDescription(GetGraphPanel(i), flag, i + 1, totalCost);
			}
			else
			{
				SetGraphPanelDescription(GetGraphPanel(i), i + 1, totalCost);
			}
		}
	}

	private double GetTotalCost()
	{
		double num = 0.0;
		for (int i = 0; i < GraphPanelCount; i++)
		{
			num += GetGraphPanel(i).GraphControl.Cost;
		}
		return num;
	}

	private void SetGraphPanelDescription(GraphPanel panel, bool isClusteredMode, int queryNumber, double totalCost)
	{
		if (isClusteredMode)
		{
			panel.DescriptionCtl.IsClusteredMode = isClusteredMode;
			panel.DescriptionCtl.ClusteredMode = ControlsResources.ClusteredMode;
		}
		int num = ((totalCost > 0.0) ? ((int)Math.Round(panel.GraphControl.Cost * 100.0 / totalCost)) : ((int)Math.Round(100.0 / (double)GraphPanelCount)));
		panel.Title = string.Format(CultureInfo.CurrentCulture, ControlsResources.QueryCostFormat, queryNumber, num);
		SetOptionalMissingIndex(panel);
	}

	private void SetGraphPanelDescription(GraphPanel panel, int queryNumber, double totalCost)
	{
		SetGraphPanelDescription(panel, isClusteredMode: false, queryNumber, totalCost);
	}

	private bool IsClusteredMode()
	{
		if (dataBindings != null && dataBindings.TryGetValue(GetGraphPanel(0), out DataBinding value) && value.DataSource is not IDataReader && NodeBuilderFactory.Create(value.DataSource, EnExecutionPlanType.Unknown) is IXmlBatchParser xmlBatchParser)
		{
			using StringReader textReader = new StringReader(xmlBatchParser.GetSingleStatementXml(value.DataSource, 0));
			return (new XmlSerializer(typeof(ExecutionPlanXML)).Deserialize(textReader) as ExecutionPlanXML).ClusteredMode;
		}
		return false;
	}

	private void OnGraphActiveChanged(object sender, EventArgs e)
	{
		if (sender is GraphPanel graphPanel && graphPanel != currentGraphPanel)
		{
			currentGraphPanel?.Deactivate();
			currentGraphPanel = graphPanel;
		}
	}

	private Font GetExecutionPlanFont()
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		IVsFontAndColorStorage vsFontAndColorStorage = (IVsFontAndColorStorage)GetService(typeof(SVsFontAndColorStorage));
		if (AbstractFontAndColorProvider.GetFontAndColorSettingsForCategory(VS.CLSID_FontAndColorsSqlResultsExecutionPlanCategory, FontAndColorProviderExecutionPlan.Text, vsFontAndColorStorage, out var categoryFont, out var _, out var _, readFont: true))
		{
			return categoryFont;
		}
		return new Font("Courier New", 10f);
	}

	private void SetOptionalMissingIndex(GraphPanel panel)
	{
		if (dataBindings == null || !dataBindings.TryGetValue(panel, out DataBinding value))
		{
			throw new InvalidOperationException();
		}
		if (value.DataSource is IDataReader)
		{
			return;
		}
		string xml = value.DataSource.ToString();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
		xmlNamespaceManager.AddNamespace("shp", LibraryData.C_ShowPlanNamespace);
		XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("descendant::shp:MissingIndexes", xmlNamespaceManager);
		if (xmlNode == null)
		{
			return;
		}
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("descendant::shp:MissingIndexGroup", xmlNamespaceManager) ?? throw new NullReferenceException("MissingIndexGroup");
		XmlNode obj = xmlNode2.FirstChild ?? throw new NullReferenceException("MissingIndex");
		string value2 = obj.Attributes["Database"].Value;
		string value3 = obj.Attributes["Schema"].Value;
		string value4 = obj.Attributes["Table"].Value;
		string text = string.Empty;
		string text2 = string.Empty;
		foreach (XmlNode item in obj.SelectNodes("shp:ColumnGroup", xmlNamespaceManager))
		{
			foreach (XmlNode childNode in item.ChildNodes)
			{
				string value5 = childNode.Attributes["Name"].Value;
				if (string.Compare(item.Attributes["Usage"].Value, "INCLUDE", StringComparison.Ordinal) != 0)
				{
					text = ((!(text == string.Empty)) ? string.Format(CultureInfo.InvariantCulture, "{0},{1}", text, value5) : value5);
				}
				else
				{
					text2 = ((!(text2 == string.Empty)) ? string.Format(CultureInfo.InvariantCulture, "{0},{1}", text2, value5) : value5);
				}
			}
		}
		string text3 = string.Format(CultureInfo.InvariantCulture, "CREATE NONCLUSTERED INDEX [<Name of Missing Index, sysname,>]\r\nON {0}.{1} ({2})\r\n", value3, value4, text);
		if (!string.IsNullOrEmpty(text2))
		{
			text3 += string.Format(CultureInfo.InvariantCulture, "INCLUDE ({0})", text2);
		}
		string value6 = xmlNode2.Attributes["Impact"].Value;
		string caption = string.Format(CultureInfo.InvariantCulture, ControlsResources.MissingIndexFormat, value6, text3);
		panel.DescriptionCtl.SetOptionalMissingIndex(caption, text3, value6, value2);
	}
}
