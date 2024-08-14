// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyWindow.PropertyWindowManager

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Controls.Results;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
// using Microsoft.AnalysisServices.Graphing;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.Shared.Controls.PropertiesWindow;


public class PropertiesWindowManager : IDisposable
{
	private bool _Disposed;

	private Semaphore _SingleAsyncUpdate = new Semaphore(1, 1);

	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private TabbedEditorPane EditorPane { get; set; }

	private EditorUIControl TabbedEditorUiCtl { get; set; }

	public QueryManager QryMgr { get; private set; }

	public PropertiesWindowManager(TabbedEditorPane editorPane)
	{
		EditorPane = editorPane;
		QryMgr = editorPane.AuxDocData.QryMgr;
		TabbedEditorUiCtl = editorPane.TabbedEditorUiCtl;

		RegistererEventHandlers();
	}

	private void RegistererEventHandlers()
	{
		EditorUIControl editorUI = TabbedEditorUiCtl;
		editorUI.OnFocusHandler = (EventHandler)Delegate.Combine(editorUI.OnFocusHandler, new EventHandler(OnFocusReceived));
		QryMgr.ExecutionCompletedEventAsync += OnQueryExecutionCompletedAsync;
		QryMgr.StatusChangedEvent += OnConnectionChanged;
		TabbedEditorUiCtl.TabActivatedEvent += OnTabActivated;
		ResultsHandler displaySQLResultsControl = EditorPane.EnsureDisplayResultsControl();
		// displaySQLResultsControl.ExecutionPlanWindowPane.PanelAddedEvent += OnExecutionPlanPanelAdded;
		// displaySQLResultsControl.ExecutionPlanWindowPane.PanelRemovedEvent += OnExecutionPlanPanelRemoved;
	}

	private void UnRegisterEventHandlers()
	{
		EditorUIControl editorUI = TabbedEditorUiCtl;
		editorUI.OnFocusHandler = (EventHandler)Delegate.Remove(editorUI.OnFocusHandler, new EventHandler(OnFocusReceived));
		QryMgr.ExecutionCompletedEventAsync -= OnQueryExecutionCompletedAsync;
		QryMgr.StatusChangedEvent -= OnConnectionChanged;
		TabbedEditorUiCtl.TabActivatedEvent -= OnTabActivated;
		ResultsHandler displaySQLResultsControl = EditorPane.EnsureDisplayResultsControl();
		// displaySQLResultsControl.ExecutionPlanWindowPane.PanelAddedEvent -= OnExecutionPlanPanelAdded;
		// displaySQLResultsControl.ExecutionPlanWindowPane.PanelRemovedEvent -= OnExecutionPlanPanelRemoved;
	}

	private void OnTabActivated(object sender, EventArgs args)
	{
		EnsurePropertyWindowObjectIsLatest();
	}

	private void OnFocusReceived(object sender, EventArgs args)
	{
		EnsurePropertyWindowObjectIsLatest();
	}

	private async Task<bool> OnQueryExecutionCompletedAsync(object sender, ExecutionCompletedEventArgs args)
	{
		if (!args.Launched || args.SyncToken.IsCancellationRequested)
			return true;

		await RefreshPropertyWindowAsync();

		return await Cmd.AwaitableAsync(true);
	}

	private void OnConnectionChanged(object sender, QueryStateChangedEventArgs args)
	{
		if (args.IsStateConnecting || args.IsStateConnected)
		{
			RefreshPropertyWindow();
		}
	}

	/*
	private void OnExecutionPlanPanelAdded(object sender, ResultControlEventArgs args)
	{
		if (args.ResultsControl is ExecutionPlanPanel executionPlanPanel)
		{
			for (int i = 0; i < executionPlanPanel.ExecutionPlanCtl.GraphPanelCount; i++)
			{
				executionPlanPanel.ExecutionPlanCtl.GetGraphPanel(i).SelectionChangedEvent += OnGraphSelectionChanged;
			}
		}
	}

	private void OnExecutionPlanPanelRemoved(object sender, ResultControlEventArgs args)
	{
		if (args.ResultsControl is ExecutionPlanPanel executionPlanPanel)
		{
			for (int i = 0; i < executionPlanPanel.ExecutionPlanCtl.GraphPanelCount; i++)
			{
				executionPlanPanel.ExecutionPlanCtl.GetGraphPanel(i).SelectionChangedEvent -= OnGraphSelectionChanged;
			}
		}
	}
	*/




	/*
	private void OnGraphSelectionChanged(object sender, GraphEventArgs e)
	{
		IDisplay displayObject = e.DisplayObject;
		if (displayObject != null && displayObject.Selected && TabbedEditorUiCtl.ActiveTab == EditorPane.EditorTabExecutionPlan)
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
						return;
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



	private async Task RefreshPropertyWindowAsync()
	{
		lock (_LockLocal)
		{
			if (_Disposed || _SingleAsyncUpdate == null || !_SingleAsyncUpdate.WaitOne(0))
			{
				return;
			}
		}

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


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
		object propertiesWindowDisplayObject = QryMgr.Strategy.GetPropertiesWindowDisplayObject();
		if (propertiesWindowDisplayObject != null)
		{
			if (propertiesWindowDisplayObject is IBsConnectedPropertiesWindow propertyWindowQueryExecutorInitialize
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
