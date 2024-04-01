// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyWindow.PropertyWindowManager

using System;
using System.Collections;
using System.Threading;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Controls.ResultsPanels;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
// using Microsoft.AnalysisServices.Graphing;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.Common.Controls.PropertiesWindow;

public class PropertiesWindowManager : IDisposable
{
	private bool _Disposed;

	private Semaphore _SingleAsyncUpdate = new Semaphore(1, 1);

	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private TabbedEditorWindowPane EditorPane { get; set; }

	private TabbedEditorUIControl TabbedEditorUiCtl { get; set; }

	public QueryManager QryMgr { get; private set; }

	public PropertiesWindowManager(TabbedEditorWindowPane editorPane)
	{
		EditorPane = editorPane;
		AuxilliaryDocData auxDocData = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(editorPane.DocData);
		QryMgr = auxDocData.QryMgr;
		TabbedEditorUiCtl = editorPane.TabbedEditorUiCtl;
		RegistererEventHandlers();
	}

	private void RegistererEventHandlers()
	{
		TabbedEditorUIControl editorUI = TabbedEditorUiCtl;
		editorUI.OnFocusHandler = (EventHandler)Delegate.Combine(editorUI.OnFocusHandler, new EventHandler(OnFocusReceived));
		QryMgr.ScriptExecutionCompletedEvent += OnScriptExecutionCompleted;
		QryMgr.StatusChangedEvent += OnConnectionChanged;
		TabbedEditorUiCtl.TabActivatedEvent += OnTabActivated;
		DisplaySQLResultsControl displaySQLResultsControl = EditorPane.EnsureDisplayResultsControl();
		displaySQLResultsControl.ExecutionPlanWindowPane.PanelAddedEvent += OnExecutionPlanPanelAdded;
		displaySQLResultsControl.ExecutionPlanWindowPane.PanelRemovedEvent += OnExecutionPlanPanelRemoved;
	}

	private void UnRegisterEventHandlers()
	{
		TabbedEditorUIControl editorUI = TabbedEditorUiCtl;
		editorUI.OnFocusHandler = (EventHandler)Delegate.Remove(editorUI.OnFocusHandler, new EventHandler(OnFocusReceived));
		QryMgr.ScriptExecutionCompletedEvent -= OnScriptExecutionCompleted;
		QryMgr.StatusChangedEvent -= OnConnectionChanged;
		TabbedEditorUiCtl.TabActivatedEvent -= OnTabActivated;
		DisplaySQLResultsControl displaySQLResultsControl = EditorPane.EnsureDisplayResultsControl();
		displaySQLResultsControl.ExecutionPlanWindowPane.PanelAddedEvent -= OnExecutionPlanPanelAdded;
		displaySQLResultsControl.ExecutionPlanWindowPane.PanelRemovedEvent -= OnExecutionPlanPanelRemoved;
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

	private void OnConnectionChanged(object sender, QueryManager.StatusChangedEventArgs args)
	{
		if (args.Change == QueryManager.EnStatusType.Connection || args.Change == QueryManager.EnStatusType.Connected)
		{
			RefreshPropertyWindow();
		}
	}


	private void OnExecutionPlanPanelAdded(object sender, ResultControlEventArgs args)
	{
		/*
		if (args.ResultsControl is ExecutionPlanPanel executionPlanPanel)
		{
			for (int i = 0; i < executionPlanPanel.ExecutionPlanCtl.GraphPanelCount; i++)
			{
				executionPlanPanel.ExecutionPlanCtl.GetGraphPanel(i).SelectionChangedEvent += OnGraphSelectionChanged;
			}
		}
		*/
	}

	private void OnExecutionPlanPanelRemoved(object sender, ResultControlEventArgs args)
	{
		/*
		if (args.ResultsControl is ExecutionPlanPanel executionPlanPanel)
		{
			for (int i = 0; i < executionPlanPanel.ExecutionPlanCtl.GraphPanelCount; i++)
			{
				executionPlanPanel.ExecutionPlanCtl.GetGraphPanel(i).SelectionChangedEvent -= OnGraphSelectionChanged;
			}
		}
		*/
	}




	/*
	private void OnGraphSelectionChanged(object sender, GraphEventArgs e)
	{
		IDisplay displayObject = e.DisplayObject;
		if (displayObject != null && displayObject.Selected && TabbedEditorUiCtl.ActiveTab == EditorPane.GetSqlExecutionPlanTab())
		{
			ArrayList arrayList = new(1)
			{
				displayObject
			};
			TabbedEditorUiCtl.ActiveTab.PropertyWindowSelectedObjects = arrayList;
		}
	}
	*/

	private void RefreshPropertyWindow()
	{
		lock (_LockLocal)
		{
			if (_Disposed || _SingleAsyncUpdate == null || !_SingleAsyncUpdate.WaitOne(0))
			{
				return;
			}
		}

		((Action)delegate
		{
			ThreadHelper.Generic.Invoke(delegate
			{
				lock (_LockLocal)
				{
					if (_Disposed || _SingleAsyncUpdate == null)
					{
						return;
					}
					_SingleAsyncUpdate.Release();
				}
				ICollection propertyWindowObjects = GetPropertyWindowObjects();
				if (propertyWindowObjects != null && TabbedEditorUiCtl.ActiveTab != null)
				{
					TabbedEditorUiCtl.ActiveTab.PropertyWindowSelectedObjects = propertyWindowObjects;
				}
			});
		}).BeginInvoke(null, null);

	}

	private void EnsurePropertyWindowObjectIsLatest()
	{
		ArrayList propertyWindowObjects = GetPropertyWindowObjects();
		if (propertyWindowObjects != null && propertyWindowObjects.Count > 0)
		{
			if (TabbedEditorUiCtl.ActiveTab.PropertyWindowSelectedObjects is not ArrayList arrayList
				|| !arrayList[0].Equals(propertyWindowObjects[0]))
			{
				RefreshPropertyWindow();
			}
		}
	}

	private ArrayList GetPropertyWindowObjects()
	{
		ArrayList arrayList = [];
		object propertiesWindowDisplayObject = QryMgr.ConnectionStrategy.GetPropertiesWindowDisplayObject();
		if (propertiesWindowDisplayObject != null)
		{
			if (propertiesWindowDisplayObject is IBPropertyWindowQueryManagerInitialize propertyWindowQueryExecutorInitialize
				&& !propertyWindowQueryExecutorInitialize.IsInitialized())
			{
				propertyWindowQueryExecutorInitialize.Initialize(QryMgr);
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
		lock (_LockLocal)
		{
			if (!_Disposed)
			{
				if (disposing)
				{
					_SingleAsyncUpdate.Dispose();
					_SingleAsyncUpdate = null;
					UnRegisterEventHandlers();
				}

				_Disposed = true;
			}
		}
	}
}
