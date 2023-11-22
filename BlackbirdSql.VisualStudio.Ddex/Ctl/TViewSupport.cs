// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Model;
using BlackbirdSql.VisualStudio.Ddex.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;

using FirebirdSql.Data.FirebirdClient;

using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TViewSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataViewSupport"/> and <see cref="IVsDataSupportImportResolver"/>
/// and <see cref="IVsDataViewIconProvider"/>interfaces.
/// Partly plagiarized off of Microsoft.VisualStudio.Data.Providers.SqlServer.SqlViewSupport.
/// </summary>
// =========================================================================================================
public class TViewSupport : DataViewSupport, IVsDataSupportImportResolver, IVsDataViewIconProvider
{
	private bool _Loaded = false;
	private bool _Initialized = false;
	private bool _Refreshing = false;

	private static string _IconName = null;
	private static string _IconPrefix = null;
	private static Icon _Icon = null;


	string IconPrefix
	{
		get
		{
			if (_IconPrefix == null)
			{
				string[] nsparts = GetType().FullName.Split('.');
				nsparts[^2] = "Resources";
				nsparts[^1] = "ViewSupport.";

				_IconPrefix = string.Join(".", nsparts);
			}

			return _IconPrefix;
		}
	}

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors
	// ---------------------------------------------------------------------------------


	public TViewSupport(string fileName, string path) : base(fileName, path)
	{
		// Tracer.Trace(GetType(), "TViewSupport.TViewSupport()", "fileName: {0}, path: {1}", fileName, path);
	}

	public TViewSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
	{
		// Tracer.Trace(GetType(), "TViewSupport.TViewSupport()", "resourceName: {0} assembly: {1}", resourceName, assembly.FullName);
	}



	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TViewSupport
	// =========================================================================================================


	public override void Close()
	{
		// Tracer.Trace(GetType(), "Close()");

		if (_Loaded && ViewHierarchy != null && ViewHierarchy.ExplorerConnection != null)
		{
			// Tracer.Trace(GetType(), "Close()", "Disassociating from ExplorerConnection.");

			ViewHierarchy.ExplorerConnection.NodeExpandedOrRefreshed -= OnNodeStateChanged;
			ViewHierarchy.ExplorerConnection.NodeRemoving -= OnNodeRemoving;
			ViewHierarchy.ExplorerConnection.NodeChanged -= OnNodeChanged;

			IVsDataConnection connection = ViewHierarchy.ExplorerConnection.Connection;

			if (connection != null)
			{
				// Tracer.Trace(GetType(), "Close()", "Disposing of site objects");

				connection.StateChanged -= OnConnectionStateChanged;

				// Perform a cleanup of linkage tables.
				LinkageParser.DisposeInstance(connection);
			}
		}

		base.Close();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service for the specified type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override object CreateService(Type serviceType)
	{
		// Tracer.Trace(GetType(), "CreateService()", "serviceType: {0}", serviceType.Name);


		// If it's asking for a visibility provider it means the tree must be opening so it's
		// our opportunity to async start the parser if it's not up already.
		/*
		if (serviceType == typeof(IVsDataViewVisibilityProvider))
		{

			IVsDataConnection connection = ViewHierarchy.ExplorerConnection.Connection;
			if (connection.State == DataConnectionState.Open)
			{
				LinkageParser parser = LinkageParser.EnsureInstance(connection, typeof(DslProviderSchemaFactory));
				parser?.AsyncExecute();
			}
		}
		*/
		/*
		if (serviceType == typeof(IVsDataViewCommandProvider))
		{
			return new TViewCommandProvider();
		}

		if (serviceType == typeof(IVsDataViewDocumentProvider))
		{
			return new TViewDocumentProvider();
		}
		*/

		// Diag.Trace(serviceType.FullName + " is not supported");

		object service = base.CreateService(serviceType);

		// if (service == null)
		//	Diag.Trace(serviceType.FullName + " is not supported");
		// else
		//	Diag.Trace(serviceType.FullName + " is indirectly supported");

		return service;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Imports and returns a stream of data support XML that is identified with a specified
	/// pseudo name.
	/// </summary>
	/// <param name="name">The pseudo name of a stream to import.</param>
	/// <returns>
	/// An open stream containing the data support XML to be imported, or null if there
	/// is no stream found with this pseudo name.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public Stream ImportSupportStream(string name)
	{
		if (name == null)
		{
			ArgumentNullException ex = new("name");
			Diag.Dug(ex);
			throw ex;
		}

		if (!name.EndsWith("Definitions"))
		{
			Diag.Stack(Resources.ExceptionImportResourceNotFound.FmtRes(name));
			return null;
		}


		Type type = GetType();
		string resource = type.FullName + name[..^11] + ".xml";

		// Diag.Trace(resource);

		return type.Assembly.GetManifestResourceStream(resource);
	}



	public override void Initialize()
	{
		// Tracer.Trace(GetType(), "Initialize()");

		base.Initialize();

		IVsDataConnection site = ViewHierarchy.ExplorerConnection.Connection;

		if (site.State == DataConnectionState.Open)
			InitializeProperties();

		_Loaded = true;

		site.StateChanged += OnConnectionStateChanged;

		ViewHierarchy.ExplorerConnection.NodeExpandedOrRefreshed += OnNodeStateChanged;
		ViewHierarchy.ExplorerConnection.NodeRemoving += OnNodeRemoving;
		ViewHierarchy.ExplorerConnection.NodeChanged += OnNodeChanged;

	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - TViewSupport
	// =========================================================================================================




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves the icon displayed in Server Explorer when the specified node is closed.
	/// </summary>
	/// <param name="itemId">
	/// A numerical identifier of the node for which you want to get the icon.
	/// </param>
	/// <returns>
	/// An System.Drawing.Icon object representing the node's icon.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public Icon GetIcon(int itemId)
	{
		Icon icon;

		try
		{
			icon = GetIcon(false, itemId);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		return icon;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves the icon displayed when the specified node is expanded.
	/// </summary>
	/// <param name="itemId">
	/// A numerical identifier of the node for which you want to get the icon.
	/// </param>
	/// <returns>
	///	An System.Drawing.Icon object representing the node's icon.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public Icon GetExpandedIcon(int itemId)
	{
		Icon icon;

		try
		{
			icon = GetIcon(true, itemId);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		return icon;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves the icon displayed.
	/// </summary>
	/// <param name="expanded">
	/// Boolean indicating whether or not the node is expanded or closed.
	/// </param>
	/// <param name="itemId">
	/// A numerical identifier of the node for which you want to get the icon.
	/// </param>
	/// <returns>
	///	An System.Drawing.Icon object representing the node's icon else null.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public Icon GetIcon(bool expanded, int itemId)
	{
		IVsDataExplorerNode node = ViewHierarchy.ExplorerConnection.FindNode(itemId);

		if (node == null)
			return null;

		// Diag.Trace((expanded ? "Expanded" : "Closed") + " icon requested id: " + itemId + ":" + node.Name + ":" + node.FullName);

		string name = null;
		string[] nodes = node.FullName.Split('/');

		switch (nodes.Length)
		{
			case 2:
				name = "Database" + node.Name;
				break;

			case 3:
				if (node.Name.IndexOf("[") == -1)
					name = "Folder" + node.Name;
				break;

			case 4:
				if (node.Name.IndexOf("[") == -1)
				{
					if (node.Name.EndsWith("Empty"))
						name = "FolderEmpty";
					else if (nodes[2].IndexOf("System") == 0)
						name = "FolderSystem";
					else
						name = "FolderUser";
				}
				break;

			case 5:
				if (node.Name.IndexOf("[") == -1)
				{
					if (node.Name.EndsWith("Empty"))
						name = "FolderEmpty";
					else if (nodes[2].IndexOf("System") == 0)
						name = "FolderSystem";
					else
						name = "FolderUser";
				}
				break;

			default:
				break;
		}

		if (name == null)
		{
			// Diag.Trace((expanded ? "Expanded" : "Closed") + " icon not defined id: " + itemId + ":" + node.Name + ":" + node.FullName);
			return null;
		}

		if (expanded && name != "FolderEmpty")
			name += "Open";
		name = IconPrefix + name + ".ico";

		if (_IconName != null && name == _IconName)
		{
			// Expand columns folder on first expand of table folder

			if (expanded && node.Name == "ColumnFolder")
			{
				_ = Task.Run(() => ExpandNodeAsync(itemId));
			}


			// OnIconsChanged(new DataViewNodeEventArgs(itemId));
			// return (Icon)_Icon.Clone();

			return _Icon;
		}

		try
		{
			// Diag.Trace("Loading Icon: " + name);

			Stream stream = GetType().Assembly.GetManifestResourceStream(name);
			if (stream == null)
			{
				Diag.Stack(Resources.ExceptionIconResourceNotFound.FmtRes(name));
				return null;
			}

			Icon icon = new(GetType().Assembly.GetManifestResourceStream(name));
			if (icon != null)
			{
				_IconName = name;
				_Icon = icon;

				// OnIconsChanged(new DataViewNodeEventArgs(itemId));

				// Expand columns folder on first expand of table folder

				if (expanded && node.Name == "ColumnFolder")
				{
					_ = Task.Run(() => ExpandNodeAsync(itemId));
				}



				return icon;
			}
			else
			{
				Diag.Stack(Resources.ExceptionIconResourceInvalid.FmtRes(name));
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, Resources.ExceptionIconResourceNotFound.FmtRes(name));
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Expands an SE node asynchronously
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected async Task<bool> ExpandNodeAsync(int itemId)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		IVsDataExplorerNode node = ViewHierarchy.ExplorerConnection.FindNode(itemId);

		if (node == null)
			return false;

		node.Expand();

		return true;
	}



	private void InitializeProperties()
	{
		try
		{
			IVsDataConnection connection = ViewHierarchy.ExplorerConnection.Connection;
			if (connection != null && connection.State == DataConnectionState.Open)
			{
				_Initialized = true;

				// IVsDataSourceInformation vsDataSourceInformation = connection.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
				// ViewHierarchy.PersistentProperties["BackendType"] = vsDataSourceInformation["BackendType"];

				// MonikerAgent sqlMoniker = new(ViewHierarchy.ExplorerConnection.ConnectionNode);
				// ViewHierarchy.PersistentProperties["MkDocumentPrefix"] = sqlMoniker.ToDocumentMoniker(true);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}

	#endregion Methods




	// =========================================================================================================
	#region Events - TViewSupport
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Occurs when any icons have changed in the data view.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public event EventHandler<DataViewNodeEventArgs> IconsChanged;



	private void OnConnectionStateChanged(object sender, DataConnectionStateChangedEventArgs e)
	{
		if (!_Initialized && e.NewState == DataConnectionState.Open)
			InitializeProperties();

		// Tracer.Trace(GetType(), "OnNodeChanged()", "Old: {0} new:{1}.", e.OldState, e.NewState);

		if (!_Refreshing || e.OldState == DataConnectionState.Open || e.NewState != DataConnectionState.Open
			|| ViewHierarchy == null || ViewHierarchy.ExplorerConnection == null
			|| ViewHierarchy.ExplorerConnection.Connection == null)
		{
			return;
		}

		_Refreshing = false;

		// Attempt linkage startup on a refresh.
		IVsDataConnection site = null;
		object lockedProviderObject = null;

		try
		{
			site = ViewHierarchy.ExplorerConnection.Connection;
			lockedProviderObject = site.GetLockedProviderObject();

			if (lockedProviderObject == null || lockedProviderObject is not FbConnection connection)
			{
				NotImplementedException ex = new("ViewHierarchy.ExplorerConnection.Connection.GetLockedProviderObject()");
				throw ex;
			}

			// We'll delay 25 cycles of 20ms each because this is a deadlock when
			// preregistering the taskhandler and a node requiring completed linkage tables is already expanded.
			LinkageParser parser = LinkageParser.EnsureInstance(connection, typeof(DslProviderSchemaFactory));
			parser?.AsyncExecute(20, 50);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			if (lockedProviderObject != null)
				site.UnlockProviderObject();
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// SE IconsChanged Event.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected virtual void OnIconsChanged(DataViewNodeEventArgs e)
	{
		IconsChanged?.Invoke(this, e);
	}



	private void OnNodeChanged(object sender, DataExplorerNodeEventArgs e)
	{
		IVsDataConnection site = null;
		object lockedProviderObject = null;

		try
		{
			if (e.Node == null || ((!e.Node.IsRefreshing || _Refreshing) && !e.Node.IsExpanding)
				|| e.Node.ExplorerConnection == null
				|| e.Node.ExplorerConnection.Connection == null
				|| e.Node.ExplorerConnection.ConnectionNode == null
				|| !e.Node.Equals(e.Node.ExplorerConnection.ConnectionNode))
			{
				return;
			}

			string str = $"ItemId: {e.Node.ItemId}, NodeType: {e.Node}, " +
				$"Name: {e.Node.Name} HasBeenExpanded: {e.Node.HasBeenExpanded}, " +
				$"IsExpandable: {e.Node.IsExpandable}, IsExpanding: {e.Node.IsExpanding}, " +
				$"IsRefreshing: {e.Node.IsRefreshing}, IsDiscarded: {e.Node.IsDiscarded}, " +
				$"IsExpanded: {e.Node.IsExpanded}, IsPlaced: {e.Node.IsPlaced}, IsVisible: {e.Node.IsVisible}";

			if (e.Node.Object != null)
			{
				str += $", Object.Name: {e.Node.Object.Name}, Object.Type: {e.Node.Object.Type.Name}, " +
					$"Object.IsDeleted: {e.Node.Object.IsDeleted}.";
			}

			// Tracer.Trace(GetType(), "OnNodeChanged()", str);

			site = ViewHierarchy.ExplorerConnection.Connection;
			lockedProviderObject = site.GetLockedProviderObject();

			if (lockedProviderObject == null || lockedProviderObject is not FbConnection connection)
			{
				NotImplementedException ex = new("ViewHierarchy.ExplorerConnection.Connection.GetLockedProviderObject()");
				throw ex;
			}


			if (e.Node.IsRefreshing && !_Refreshing)
			{
				if (connection != null)
					LinkageParser.DisposeInstance(connection);
				_Refreshing = true;

			}
			else if (e.Node.IsExpanding)
			{
				if (site.State == DataConnectionState.Open)
				{
					LinkageParser parser = LinkageParser.EnsureInstance(connection, typeof(DslProviderSchemaFactory));
					parser?.AsyncExecute();
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			if (site != null && lockedProviderObject != null)
				site.UnlockProviderObject();
		}

	}



	private void OnNodeRemoving(object sender, DataExplorerNodeEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnNodeRemoving()", "Node: {0}.", e.Node != null ? e.Node.ToString() : "null");
	}



	private void OnNodeStateChanged(object sender, DataExplorerNodeEventArgs e)
	{
		try
		{
			if (e.Node == null || e.Node.ExplorerConnection == null
				|| e.Node.ExplorerConnection.Connection == null
				|| e.Node.ExplorerConnection.ConnectionNode == null
				|| !e.Node.Equals(e.Node.ExplorerConnection.ConnectionNode))
			{
				return;
			}

			string str = $"ItemId: {e.Node.ItemId}, NodeType: {e.Node}, " +
				$"Name: {e.Node.Name} HasBeenExpanded: {e.Node.HasBeenExpanded}, " +
				$"IsExpandable: {e.Node.IsExpandable}, IsExpanding: {e.Node.IsExpanding}, " +
				$"IsRefreshing: {e.Node.IsRefreshing}, IsDiscarded: {e.Node.IsDiscarded}, " +
				$"IsExpanded: {e.Node.IsExpanded}, IsPlaced: {e.Node.IsPlaced}, IsVisible: {e.Node.IsVisible}";

			if (e.Node.Object != null)
			{
				str += $", Object.Name: {e.Node.Object.Name}, Object.Type: {e.Node.Object.Type.Name}, " +
					$"Object.IsDeleted: {e.Node.Object.IsDeleted}.";
			}
			// Tracer.Trace(GetType(), "OnNodeStateChanged()", str);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}



	#endregion Events


}
