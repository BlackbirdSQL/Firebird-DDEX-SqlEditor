#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Controls.ResultsPane;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using Microsoft.VisualStudio.Shell;

namespace BlackbirdSql.Common.Controls.PropertiesWindow;


public class PropertyWindowManager : IDisposable
{
	private bool _disposed;

	private Semaphore _singleAsyncUpdate = new Semaphore(1, 1);

	private readonly object _LocalLock = new object();

	public SqlEditorTabbedEditorPane EditorPane { get; private set; }

	public SqlEditorTabbedEditorUI EditorUI { get; private set; }

	public QueryExecutor QueryExecutor { get; private set; }

	public PropertyWindowManager(SqlEditorTabbedEditorPane editorPane)
	{
		EditorPane = editorPane;
		AuxiliaryDocData auxillaryDocData = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(editorPane.DocData);
		QueryExecutor = auxillaryDocData.QueryExecutor;
		EditorUI = editorPane.TabbedEditorUI;
		RegistererEventHandlers();
	}

	private void RegistererEventHandlers()
	{
		SqlEditorTabbedEditorUI editorUI = EditorUI;
		editorUI.OnFocus = (EventHandler)Delegate.Combine(editorUI.OnFocus, new EventHandler(OnFocusReceived));
		QueryExecutor.ScriptExecutionCompleted += OnScriptExecutionCompleted;
		QueryExecutor.StatusChanged += OnConnectionChanged;
		EditorUI.TabActivated += OnTabActivated;
		DisplaySQLResultsControl displaySQLResultsControl = EditorPane.EnsureDisplayResultsControl();
		displaySQLResultsControl.ExecutionPlanWindowPane.PanelAdded += OnExecutionPlanPanelAdded;
		displaySQLResultsControl.ExecutionPlanWindowPane.PanelRemoved += OnExecutionPlanPanelRemoved;
	}

	private void UnRegisterEventHandlers()
	{
		SqlEditorTabbedEditorUI editorUI = EditorUI;
		editorUI.OnFocus = (EventHandler)Delegate.Remove(editorUI.OnFocus, new EventHandler(OnFocusReceived));
		QueryExecutor.ScriptExecutionCompleted -= OnScriptExecutionCompleted;
		QueryExecutor.StatusChanged -= OnConnectionChanged;
		EditorUI.TabActivated -= OnTabActivated;
		DisplaySQLResultsControl displaySQLResultsControl = EditorPane.EnsureDisplayResultsControl();
		displaySQLResultsControl.ExecutionPlanWindowPane.PanelAdded -= OnExecutionPlanPanelAdded;
		displaySQLResultsControl.ExecutionPlanWindowPane.PanelRemoved -= OnExecutionPlanPanelRemoved;
	}

	private void OnTabActivated(object sender, EventArgs args)
	{
		EnsurePropertyWindowObjectIsLatest();
	}

	private void OnFocusReceived(object sender, EventArgs args)
	{
		EnsurePropertyWindowObjectIsLatest();
	}

	private void OnScriptExecutionCompleted(object sender, EventArgs args)
	{
		RefreshPropertyWindow();
	}

	private void OnConnectionChanged(object sender, QueryExecutor.StatusChangedEventArgs args)
	{
		if (args.Change == QueryExecutor.StatusType.Connection || args.Change == QueryExecutor.StatusType.Connected)
		{
			RefreshPropertyWindow();
		}
	}

	private void OnExecutionPlanPanelAdded(object sender, ResultControlEventArgs args)
	{
		NotImplementedException ex = new("ExecutionPlanPanel");
		Diag.Dug(ex);
		throw ex;

		/*
		ExecutionPlanPanel executionPlanPanel = args.ResultsControl as ExecutionPlanPanel;
		if (executionPlanPanel != null)
		{
			for (int i = 0; i < executionPlanPanel.ExecutionPlanControl.GraphPanelCount; i++)
			{
				executionPlanPanel.ExecutionPlanControl.GetGraphPanel(i).add_SelectionChanged(new GraphEventHandler(OnGraphSelectionChanged));
			}
		}
		*/
	}

	private void OnExecutionPlanPanelRemoved(object sender, ResultControlEventArgs args)
	{
		NotImplementedException ex = new("OnExecutionPlanPanelRemoved: ExecutionPlanPanel");
		Diag.Dug(ex);
		throw ex;

		/*
		ExecutionPlanPanel executionPlanPanel = args.ResultsControl as ExecutionPlanPanel;
		if (executionPlanPanel != null)
		{
			for (int i = 0; i < executionPlanPanel.ExecutionPlanControl.GraphPanelCount; i++)
			{
				executionPlanPanel.ExecutionPlanControl.GetGraphPanel(i).remove_SelectionChanged(new GraphEventHandler(OnGraphSelectionChanged));
			}
		}
		*/
	}

	/*
	private void OnGraphSelectionChanged(object sender, GraphEventArgs e)
	{
		IDisplay displayObject = e.DisplayObject;
		if (displayObject != null && displayObject.Selected && EditorUI.ActiveTab == EditorPane.GetSqlExecutionPlanTab())
		{
			ArrayList arrayList = new(1)
			{
				displayObject
			};
			EditorUI.ActiveTab.PropertyWindowSelectedObjects = arrayList;
		}
	}
	*/

	private void RefreshPropertyWindow()
	{
		lock (_LocalLock)
		{
			if (_disposed || _singleAsyncUpdate == null || !_singleAsyncUpdate.WaitOne(0))
			{
				return;
			}
		}

		((Action)delegate
		{
#pragma warning disable CS0618 // Type or member is obsolete
			ThreadHelper.Generic.Invoke(delegate
			{
				lock (_LocalLock)
				{
					if (_disposed || _singleAsyncUpdate == null)
					{
						return;
					}
					_singleAsyncUpdate.Release();
				}
				ICollection propertyWindowObjects = GetPropertyWindowObjects();
				if (propertyWindowObjects != null && EditorUI.ActiveTab != null)
				{
					EditorUI.ActiveTab.PropertyWindowSelectedObjects = propertyWindowObjects;
				}
			});
#pragma warning restore CS0618 // Type or member is obsolete
		}).BeginInvoke(null, null);

		/*
		_ = ((Action)delegate
		{
			Task task = Task.Factory.StartNew(delegate
			{
				lock (_LocalLock)
				{
					if (_disposed || _singleAsyncUpdate == null)
					{
						return;
					}

					_singleAsyncUpdate.Release();
				}

				ICollection propertyWindowObjects = GetPropertyWindowObjects();
				if (propertyWindowObjects != null && EditorUI.ActiveTab != null)
				{
					EditorUI.ActiveTab.PropertyWindowSelectedObjects = propertyWindowObjects;
				}
			},
			default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

		}).BeginInvoke(null, null);
		*/
	}

	private void EnsurePropertyWindowObjectIsLatest()
	{
		ArrayList propertyWindowObjects = GetPropertyWindowObjects();
		if (propertyWindowObjects != null && propertyWindowObjects.Count > 0)
		{
			if (EditorUI.ActiveTab.PropertyWindowSelectedObjects is not ArrayList arrayList
				|| !arrayList[0].Equals(propertyWindowObjects[0]))
			{
				RefreshPropertyWindow();
			}
		}
	}

	private ArrayList GetPropertyWindowObjects()
	{
		ArrayList arrayList = new ArrayList();
		object propertiesWindowDisplayObject = QueryExecutor.ConnectionStrategy.GetPropertiesWindowDisplayObject();
		if (propertiesWindowDisplayObject != null)
		{
			if (propertiesWindowDisplayObject is IPropertyWindowQueryExecutorInitialize propertyWindowQueryExecutorInitialize
				&& !propertyWindowQueryExecutorInitialize.IsInitialized())
			{
				propertyWindowQueryExecutorInitialize.Initialize(QueryExecutor);
			}

			arrayList.Add(propertiesWindowDisplayObject);
		}

		return arrayList;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		lock (_LocalLock)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_singleAsyncUpdate.Dispose();
					_singleAsyncUpdate = null;
					UnRegisterEventHandlers();
				}

				_disposed = true;
			}
		}
	}
}
