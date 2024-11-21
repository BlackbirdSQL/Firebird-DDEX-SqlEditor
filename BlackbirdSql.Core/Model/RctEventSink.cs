using System;
using System.Collections.Generic;
using System.Reflection;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//
//											RctEventSink Class
//
/// <summary>
/// Manages events for server explorer connection nodes.
/// There is only one place the Site can come from. However it is possible and does happen that
/// <see cref="IVsDataExplorerConnection.Connection"/> and Site do not reference the same object. So we
/// reference <see cref="IBsNativeDbLinkageParser"/> instances and <see cref="RctEventSink"/>s with the
/// <see cref="IVsDataExplorerConnection"/> root nodes instead.
/// The referencing to these nodes is robust. This is far less volatile than using
/// <see cref="IVsDataConnection"/>.
/// Our <see cref="RunningConnectionTable"/> still indexes by the equivalency key ConnectionUrl but we
/// control the SE root nodes' events and trigger tables by using the 
/// <see cref="IVsDataExplorerConnectionManager"/>'s objects as our references.
/// If anything happens in the SE's table, we will know about it in <see cref="RctEventSink"/>.
/// </summary>
// =========================================================================================================
internal class RctEventSink : IDisposable
{


	// ----------------------------------------------
	#region Constructors / Destructors - RctEventSink
	// ----------------------------------------------


	private RctEventSink(IVsDataExplorerConnection root, bool initialized = false)
	{
		_Root = root;
		_Initialized = initialized;
	}



	public void Dispose()
	{
		Dispose(true);
	}



	private void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		_Root.NodeChanged -= OnNodeChanged;
		_Root.NodeExpandedOrRefreshed -= OnNodeExpandedOrRefreshed;

		if (_ConnectionEventsAttached)
			_Root.Connection.StateChanged -= OnConnectionStateChanged;


		IBsDataViewSupport viewSupport = Reflect.GetFieldValue(_Root, "_viewSupport") as IBsDataViewSupport;

		viewSupport.OnCloseEvent -= OnNodeRemoving;

		// Evs.Debug(typeof(RctEventSink), "Dispose(bool)", $"Deregistered events for {_Root.SafeName()}.");

		_Root = null;
		_ConnectionEventsAttached = false;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - RctEventSink
	// =========================================================================================================


	private static readonly object _LockGlobal = new();

	private static int _EventCardinal = 0;

	private bool _Initialized = false;
	private IVsDataExplorerConnection _Root;
	private string _ConnectionString = null;
	private bool _ConnectionEventsAttached = false;

	private static IDictionary<IVsDataExplorerConnection, RctEventSink> _RctEventSinks = null;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - RctEventSink
	// =========================================================================================================


	private EnConnectionSource ConnectionSource => RctManager.ConnectionSource;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - RctEventSink
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attaches to a Server Explorer <see cref="IVsDataExplorerConnection"/>'s events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void NotifyInitializedServerExplorerModel(object sender, DataExplorerNodeEventArgs e)
	{
		if (ApcManager.SolutionClosing)
			return;


		if (_RctEventSinks != null && _RctEventSinks.TryGetValue(e.Node.ExplorerConnection, out RctEventSink eventSink))
		{
			if (!eventSink._Initialized)
			{
				Evs.Warning(typeof(RctEventSink), "NotifyInitializedServerExplorerModel()",
					$"Events already advised for node {e.Node.ExplorerConnection.SafeName()}.");
				return;
			}
		}
		else
		{
			eventSink = new RctEventSink(e.Node.ExplorerConnection, false);
		}

		eventSink.NotifyInitializedModel(sender, e);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attaches to a Server Explorer <see cref="IVsDataExplorerConnection"/>'s events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void InitializeServerExplorerModel(IVsDataExplorerConnection root)
	{
		if (ApcManager.IdeShutdownState)
			return;

		if (_RctEventSinks != null && _RctEventSinks.ContainsKey(root))
			return;

		// Evs.Trace(typeof(RctEventSink), nameof(InitializeServerExplorerModel), $"Connection Safename: {root.SafeName()}.");

		_RctEventSinks ??= new Dictionary<IVsDataExplorerConnection, RctEventSink>();
		_RctEventSinks[root] = new RctEventSink(root, true);

		try
		{
			// if gotViewSupport is true, Tviewsupport has already been initialized. Should always return false
			// bool gotViewSupport = (bool)Reflect.GetFieldValue(root, "_gotViewSupport");

			// Wake the connection node so we can get some basic info in the property window.
			// This will also cause AdviseEvents to be fired in VxbViewSupport.Initialize()
			// and also ensures VxbViewSupport.Close() will be called when an ExplorerConnection
			// node is deleted.
			// if (!gotViewSupport)

			Reflect.InvokeMethod(root, "InitializeModel", BindingFlags.Default, null, true);

			// Evs.Debug(typeof(RctEventSink), "InitializeServerExplorerModel()",
			//	$"Model; initialized for {root.SafeName()}.");
		}
		catch (Exception ex)
		{
			Diag.Expected(ex, $"Failed to initialize model for {root.SafeName()}.");
			_RctEventSinks.Remove(root);
		}
	}


	private void NotifyInitializedModel(object sender, DataExplorerNodeEventArgs e)
	{
		// Evs.Trace(typeof(RctEventSink), nameof(NotifyInitializedModel), "Performing full registration.");

		if (_Root.Connection == null)
			Diag.ThrowException(new ArgumentException($"The ExplorerConnection.Connection for Server Explorer node '{_Root.SafeName()}' is null. Aborting."));

		_ConnectionString = _Root.DecryptedConnectionString();

		if (_ConnectionString == null)
			Diag.ThrowException(new ArgumentNullException($"The Connection string for Server Explorer node '{_Root.SafeName()}' is null. Aborting."));


		try
		{
			_Root.NodeChanged += OnNodeChanged;
			_Root.NodeExpandedOrRefreshed += OnNodeExpandedOrRefreshed;


			_ConnectionEventsAttached = true;
			_Root.Connection.StateChanged += OnConnectionStateChanged;


			IBsDataViewSupport viewSupport = sender as IBsDataViewSupport;

			viewSupport.OnCloseEvent += OnNodeRemoving;

			// Evs.Debug(typeof(RctEventSink), nameof(NotifyInitializedModel), $"Registered events for {_Root.SafeName()}.");
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		if (!_Initialized)
		{
			_RctEventSinks ??= new Dictionary<IVsDataExplorerConnection, RctEventSink>();
			_RctEventSinks[_Root] = this;
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if an event passing an <see cref="DataExplorerNodeEventArgs"/> argument
	/// is a velid <see cref="IVsDataExplorerConnection.ConnectionNode"/> else returns
	/// false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool IsConnectionNodeEvent(DataExplorerNodeEventArgs e) =>
		!RctManager.InitializingExplorerModels && e.Node != null && e.Node.ExplorerConnection != null
		&& e.Node.ExplorerConnection.ConnectionNode != null
		&& e.Node.Equals(e.Node.ExplorerConnection.ConnectionNode);




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Detaches from a Server Explorer connection's events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void UnadviseServerExplorerEvents()
	{
		RctEventSink rctEventSink = null;

		if (_RctEventSinks == null || !_RctEventSinks.TryGetValue(_Root, out rctEventSink))
		{
			Diag.ThrowException(new ApplicationException($"RctEventSink not found for explorerConnection: {_Root.SafeName()}."));
		}

		// Evs.Trace(typeof(RctEventSink), nameof(UnadviseServerExplorerEvents), $"Unregistering events for {_Root.SafeName()}.");


		_RctEventSinks.Remove(_Root);
		rctEventSink.Dispose();

		if (_RctEventSinks.Count == 0)
			_RctEventSinks = null;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handling - RctEventSink
	// =========================================================================================================



	// -------------------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventsCardinal"/> counter when execution enters an external
	/// event handler to prevent recursion.
	/// </summary>
	/// <returns>
	/// Returns false if an event has already been entered else true if it is safe to enter.
	/// </returns>
	// -------------------------------------------------------------------------------------------
	public static bool EventEnter(bool test = false, bool force = false)
	{
		lock (_LockGlobal)
		{
			if (_EventCardinal != 0 && !force)
				return false;

			if (!test)
				_EventCardinal++;

			return true;
		}
	}



	// ---------------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventsCardinal"/> counter that was previously
	/// incremented by <see cref="EventEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------------
	public static void EventExit()
	{
		lock (_LockGlobal)
		{
			if (_EventCardinal == 0)
				Diag.Ex(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
			else
				_EventCardinal--;
		}
	}




	private void OnConnectionStateChanged(object sender, DataConnectionStateChangedEventArgs e)
	{
		if (ApcManager.IdeShutdownState || RctManager.InitializingExplorerModels)
			return;

		//Tracer.Trace(typeof(RctEventSink), "OnConnectionStateChanged()",
		//	"\n=======================================================================================================\nSender: {0}, Old state: {1}, new state:{2}.",
		//	sender.GetType().Name, e.OldState, e.NewState);


		if (sender is not IVsDataConnection site)
		{
			site = null;
			Diag.ThrowException(new ArgumentException($"Invalid sender type: {sender.GetType()}."));
		}

		if (!EventEnter())
			return;


		try
		{
#if DEBUG
			IVsDataExplorerConnection root = site.ExplorerConnection();

			if (!ReferenceEquals(root, _Root))
			{
				Diag.ThrowException(new ArgumentException(
					$"The root node passed in the event arguments does not match stored root node object for {_Root.SafeName()}."));
			}
#endif


			if (e.NewState != DataConnectionState.Open || e.OldState == DataConnectionState.Open)
				return;

			NativeDb.UnlockLoadedParser();

			string connectionString = site.DecryptedConnectionString();

			if(string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentNullException($"The new connection string for Server Explorer node {_Root.SafeName()} is null or empty. Aborting.");


			_ConnectionString = connectionString;
			_Root.EnsureLinkageLoadingAsyin();

		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}
		finally
		{
			EventExit();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Node renames occur here and a sanity check that the edmx wizard has not
	/// corrupted the root node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnNodeChanged(object sender, DataExplorerNodeEventArgs e)
	{
		if (ApcManager.SolutionClosing || !IsConnectionNodeEvent(e))
			return;


		// Check we're clear to enter an event.
		if (!EventEnter())
			return;

		// Evs.Trace(typeof(RctEventSink), nameof(OnNodeChanged),
		//	"\n=============================================================================================================");
		// Diag.Trace(typeof(RctEventSink), "OnNodeChanged()", e, ConnectionSource, ThreadHelper.CheckAccess(), _Initialized);


		try
		{
			IVsDataExplorerConnection root = e.Node.ExplorerConnection;

#if DEBUG
			if (!ReferenceEquals(root, _Root))
			{
				Diag.ThrowException(new ArgumentException(
					$"The root node passed in the event arguments does not match stored root node object for {_Root.SafeName()}."));
			}
#endif

			// -------------------------
			// Handle the linkage parser
			// -------------------------

			EnConnectionSource connectionSource = EnConnectionSource.Undefined;

			// Couple of caveats when refreshing.
			// We need to delete the LinkageParser if it exists but only if it's not loading.
			// If it's loading it will already be busy loading a refreshed instance so do nothing.
			// If it doesn't exist we can start linkage.
			if (e.Node.IsRefreshing)
			{
				IBsNativeDbLinkageParser parser = root.GetLinkageParser();

				if (parser != null)
				{
					// Not guaranteed but if all properties are the same it's 99% likely
					// this is a user refresh. For the 1% the linkage tables will be redundantly rebuilt.
					if (parser.Loaded && !parser.IsLockedLoaded
						&& Csb.AreEquivalent(root.DecryptedConnectionString(), parser.ConnectionString, Csb.DescriberKeys))
					{
						connectionSource = ConnectionSource;

						if (connectionSource != EnConnectionSource.EntityDataModel && connectionSource != EnConnectionSource.DataSource)
							root.DisposeLinkageParser(true);
					}
				}
				else
				{
					NativeDb.UnlockLoadedParser();

					// Evs.Debug(GetType(), "OnNodeChanged()",
					//	"\nParser == null. IsEdmConnectionSource. If not IsEdm then Calling EnsureLinkageLoadingAsyin().");

					connectionSource = ConnectionSource;

					if (connectionSource != EnConnectionSource.EntityDataModel)
						root.EnsureLinkageLoadingAsyin(10, 10);
				}
			}
			else if (e.Node.IsExpanding && !e.Node.HasBeenExpanded)
			{
				// Evs.Debug(typeof(RctEventSink), "OnNodeChanged()",
				//	"\nIsExpanding && !HasBeenExpanded. UnlockLoadedParser_ and calling EnsureLinkageLoadingAsyin().");

				NativeDb.UnlockLoadedParser();
				root.EnsureLinkageLoadingAsyin();
			}
			else
			{
				if (!_Initialized)
				{
					connectionSource = ConnectionSource;

					if (connectionSource == EnConnectionSource.ServerExplorer)
						root.EnsureLinkageLoadingAsyin();
				}
				// Evs.Debug(typeof(RctEventSink), "OnNodeChanged()",
				//	"\nNot IsRefreshing or (IsExpanding or !HasBeenExpanded). DOING NOTHING.");
			}

			_Initialized = true;


			// ----------------------------------------
			// Handle a possible connection node rename
			// ----------------------------------------

			// We're only interested in node value changes.
			if (e.Node.IsExpanding || e.Node.IsRefreshing || root.DisplayName == null
				|| root.EncryptedConnectionString == null)
			{
				//  Tracer.Trace(typeof(RctEventSink), "OnNodeChanged()",
				// 		"NO ValidateAndUpdateExplorerConnectionRename().\nIsExpanding or IsRefreshing or nulls found. NO FURTHER PROCESSING");
				return;
			}

			if (!RctManager.IsRegistered(root))
				return;

			Csb csa = new(root.DecryptedConnectionString());

			// If the ConnectionString contains any UIHierarchyMarshaler or DataSources ToolWindow identifiers we skip the
			// DatasetKey check because the Connection is earmarked for repair.
			if (!csa.ContainsKey(CoreConstants.C_KeyExEdmx) && !csa.ContainsKey(CoreConstants.C_KeyExEdmu)
					&& csa.ContainsKey(CoreConstants.C_KeyExDatasetKey))
			{
				// Check if node.Object exists before accessing it otherwise the root node will be initialized
				// with a call to VxbObjectSelectorRoot.SelectObjects.

				string datasetKey = csa.DatasetKey;

				// DatasetKey will be null if the edmx has corrupted the root node.
				if (!string.IsNullOrEmpty(datasetKey) && datasetKey == root.DisplayName)
				{
					// Evs.Debug(typeof(RctEventSink), "OnNodeChanged()", "No ValidateAndUpdateExplorerConnectionRename(). DisplayName IS OK.");
					return;
				}
			}
			else
			{
				if (connectionSource == EnConnectionSource.Undefined)
					connectionSource = ConnectionSource;

				if (connectionSource != EnConnectionSource.EntityDataModel && connectionSource != EnConnectionSource.DataSource)
					return;
			}

			// Evs.Debug(typeof(RctEventSink), "OnNodeChanged()", "Calling ValidateAndUpdateExplorerConnectionRename(). DisplayName ISSUE");

			RctManager.ValidateAndUpdateExplorerConnectionRename(root, root.DisplayName, csa);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}
		finally
		{
			EventExit();
		}
	}



	private void OnNodeExpandedOrRefreshed(object sender, DataExplorerNodeEventArgs e)
	{
		if (ApcManager.SolutionClosing || !IsConnectionNodeEvent(e))
			return;


		// Evs.Trace(typeof(RctEventSink), nameof(OnNodeExpandedOrRefreshed),
		//	"\n================================================================================================================\n"
		//	+ $"e.Node.HasBeenExpanded: {e.Node.HasBeenExpanded}, e.Node.IsExpanded: {e.Node.IsExpanded}, " +
		//	$"e.Node.IsExpanding: {e.Node.IsExpanding}, __EntityDataModelRenameState: {_EntityDataModelRenameState}.");


		if (!EventEnter())
			return;

		try
		{
			IVsDataExplorerConnection root = e.Node.ExplorerConnection;

#if DEBUG
			if (!ReferenceEquals(root, _Root))
			{
				Diag.ThrowException(new ArgumentException(
					$"The root node passed in the event arguments does not match stored root node object for {_Root.SafeName()}."));
			}
#endif

			if (!e.Node.HasBeenExpanded || root.Connection == null
				|| root.Connection.State != DataConnectionState.Open)
			{
				return;
			}


			// If the refresh is the result of the EDMX wizard making an illegal name
			// change, exit. We'll handle this in OnNodeChanged().
			if (ConnectionSource == EnConnectionSource.EntityDataModel || ConnectionSource == EnConnectionSource.DataSource)
				return;


			root.EnsureLinkageLoadingAsyin(10, 10);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}
		finally
		{
			EventExit();
		}

	}



	/// <summary>
	/// This event does not fire for the <see cref="IVsDataExplorerConnection.ConnectionNode"/>.
	/// To simulate the event on the root node we perform a basic initialization of the
	/// <see cref="IVsDataViewSupportModel"/> in the root node with a call to InitializeModel().
	/// This calls the the Initialize() method in our <see cref="IVsDataViewSupport"/>
	/// implementation. It is from their that we initiate an instance ofr RctEventSink for the
	/// root node. The Close() method in <see cref="IVsDataViewSupport"/> is the equivalent
	/// <see cref="IVsDataExplorerConnection.NodeRemoving"/> event for the ConnectionNode.
	/// </summary>
	private void OnNodeRemoving(object sender, EventArgs e)
	{
		if (ApcManager.IdeShutdownState)
			return;

		// Evs.Trace(typeof(RctEventSink), nameof(OnNodeRemoving),
		//	"\n=========================================================================================================");

		if (!EventEnter())
			return;

		try
		{
			IVsDataExplorerConnection root = _Root;

			UnadviseServerExplorerEvents();

			root.DisposeLinkageParser(false);

			// Convert the registered SE connnection to a Session connection.
			RctManager.UpdateRegisteredConnection(root.DecryptedConnectionString(),
				EnConnectionSource.Session, true);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
		}
		finally
		{
			EventExit();
		}
	}


#endregion Event handling

}