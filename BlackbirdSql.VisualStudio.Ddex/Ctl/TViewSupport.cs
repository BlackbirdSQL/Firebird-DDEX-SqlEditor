// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.VisualStudio.Ddex.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;

using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


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
[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread")]
public class TViewSupport : DataViewSupport,
	IVsDataSupportImportResolver, IVsDataViewIconProvider, IBDataViewSupport
{
	private bool _Loaded = false;
	private bool _Initialized = false;
	private bool _LabelEditing = false;

	private static string _IconName = null;
	private static string _IconPrefix = null;
	private static Icon _Icon = null;

	private IVsUIHierarchyWindow _HierarchyWindow;




	public IVsUIHierarchyWindow HierarchyWindow
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			if (_HierarchyWindow == null)
			{
				IVsWindowFrame vsWindowFrame = Hostess.FindToolWindow(VSConstants.StandardToolWindows.ServerExplorer);

				if (vsWindowFrame != null)
				{
					Native.WrapComCall(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar));
					_HierarchyWindow = pvar as IVsUIHierarchyWindow;
				}
			}
			return _HierarchyWindow;
		}
	}



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

			ViewHierarchy.ExplorerConnection.NodeChanged -= OnNodeChanged;
			ViewHierarchy.ExplorerConnection.NodeExpandedOrRefreshed -= OnNodeExpandedOrRefreshed;
			ViewHierarchy.ExplorerConnection.NodeInserted -= OnNodeInserted;
			ViewHierarchy.ExplorerConnection.NodeRemoving -= OnNodeRemoving;

			RctManager.OnExplorerConnectionClose(this, ViewHierarchy.ExplorerConnection);

			IVsDataConnection site = ViewHierarchy.ExplorerConnection.Connection;


			if (site != null)
				site.StateChanged -= OnConnectionStateChanged;
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


	public void EditNodeLabel(IVsDataExplorerNode node)
	{
		ExpandItemWrapper(node, EXPANDFLAGS.EXPF_SelectItem);
		ExpandItemWrapper(node, EXPANDFLAGS.EXPF_EditItemLabel);
		_LabelEditing = true;
		while (_LabelEditing)
		{
			MSG msg = default;
			if (Native.GetMessage(ref msg, 0, 0, 0))
			{
				Native.TranslateMessage(ref msg);
				Native.DispatchMessage(ref msg);
			}
		}
	}


	public void StartEditNodeLabel(IVsDataExplorerNode node)
	{
		_LabelEditing = true;
	}

	public void CancelEditNodeLabel(IVsDataExplorerNode node)
	{
		_LabelEditing = false;
	}

	public void CommitEditNodeLabel(IVsDataExplorerNode node)
	{
		_LabelEditing = false;
	}

	private void ExpandItemWrapper(IVsDataExplorerNode node, EXPANDFLAGS expfFlag)
	{
		if (!(node == node.ExplorerConnection.ConnectionNode) && HierarchyWindow != null /* && synchronizeWithShell */)
		{
			Diag.ThrowIfNotOnUIThread();

			IVsUIHierarchy uiHierarchy = ViewHierarchy.ServiceProvider as IVsUIHierarchy;
			HierarchyWindow.ExpandItem(uiHierarchy, (uint)node.ItemId, expfFlag);
		}
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
			Diag.StackException(Resources.ExceptionImportResourceNotFound.FmtRes(name));
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

		if (ViewHierarchy == null || ViewHierarchy.ExplorerConnection == null
			|| ViewHierarchy.ExplorerConnection.Connection == null)
		{
			return;
		}

		IVsDataConnection site = ViewHierarchy.ExplorerConnection.Connection;

		if (site.State == DataConnectionState.Open)
			InitializeProperties();

		_Loaded = true;

		site.StateChanged += OnConnectionStateChanged;

		ViewHierarchy.ExplorerConnection.NodeChanged += OnNodeChanged;
		ViewHierarchy.ExplorerConnection.NodeExpandedOrRefreshed += OnNodeExpandedOrRefreshed;
		ViewHierarchy.ExplorerConnection.NodeInserted += OnNodeInserted;
		ViewHierarchy.ExplorerConnection.NodeRemoving += OnNodeRemoving;

		// Perform a single pass check to ensure this SE connection has had it's events
		// advised. This will occur if the SE was adding a new conection and we
		// registered it with the Rct before it was registered with Server Explorer.
		RctManager.RegisterUnadvisedConnection(ViewHierarchy.ExplorerConnection);

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
				Diag.StackException(Resources.ExceptionIconResourceNotFound.FmtRes(name));
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
				Diag.StackException(Resources.ExceptionIconResourceInvalid.FmtRes(name));
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
			if (ViewHierarchy == null || ViewHierarchy.ExplorerConnection == null
				|| ViewHierarchy.ExplorerConnection.Connection == null)
			{
				return;
			}

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
		// Tracer.Trace(GetType(), "OnConnectionStateChanged()", "Old: {0} new:{1}.", e.OldState, e.NewState);

		if (!_Initialized && e.NewState == DataConnectionState.Open)
			InitializeProperties();

		if (e.OldState == DataConnectionState.Open || e.NewState != DataConnectionState.Open
			|| !RctManager.Available || ViewHierarchy == null || ViewHierarchy.ExplorerConnection == null
			|| ViewHierarchy.ExplorerConnection.Connection == null)
		{
			return;
		}

		// Attempt linkage startup on a refresh.

		IVsDataConnection site = ViewHierarchy.ExplorerConnection.Connection;

		// We'll delay 25 cycles of 20ms each because this is a deadlock when
		// preregistering the taskhandler and a node requiring completed linkage tables is already expanded.
		LinkageParser parser = LinkageParser.EnsureInstance(site, typeof(DslProviderSchemaFactory));
		parser?.AsyncExecute(25, 20);
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


	/// <summary>
	/// Node renames occur here.
	/// </summary>
	private void OnNodeChanged(object sender, DataExplorerNodeEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnNodeChanged()");

		if (e.Node == null || (!e.Node.IsRefreshing && !e.Node.IsExpanding)
			|| e.Node.ExplorerConnection == null
			|| e.Node.ExplorerConnection.Connection == null
			|| e.Node.ExplorerConnection.ConnectionNode == null
			|| !e.Node.Equals(e.Node.ExplorerConnection.ConnectionNode))
		{
			return;
		}


		IVsDataConnection site = e.Node.ExplorerConnection.Connection;

		// Couple of caveats when refreshing.
		// We need to delete the LinkageParser if it exists but only if it's not loading.
		// If it's loading it will already be busy loading a refreshed instance so do nothing.
		// If it doesn't exist we can start linkage.
		if (e.Node.IsRefreshing)
		{
			LinkageParser parser = LinkageParser.GetInstance(site);

			if (parser != null)
			{
				// Not guaranteed but if all properties are the same it's 99% likely
				// this is a user refresh. For the 1% the linkage tables will be redundantly rebuilt.
				if (parser.Loaded &&
					CsbAgent.AreEquivalent(DataProtection.DecryptString(site.EncryptedConnectionString),
						parser.ConnectionString, CsbAgent.DescriberKeys))
				{
					LinkageParser.DisposeInstance(site, false);
				}
			}
			else
			{
				LinkageParser.EnsureLoaded(site, typeof(DslProviderSchemaFactory));
			}
		}
		else if (e.Node.IsExpanding)
		{
			if (site.State == DataConnectionState.Open && RctManager.Available)
			{
				LinkageParser parser = LinkageParser.EnsureInstance(site, typeof(DslProviderSchemaFactory));
				parser?.AsyncExecute(20, 20);
			}
		}

	}



	private void OnNodeExpandedOrRefreshed(object sender, DataExplorerNodeEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnNodeExpandedOrRefreshed");

		if (!e.Node.HasBeenExpanded || e.Node.ExplorerConnection == null
			|| e.Node.ExplorerConnection.Connection == null
			|| e.Node.ExplorerConnection.ConnectionNode == null
			|| e.Node.Equals(e.Node.ExplorerConnection.ConnectionNode))
		{
			return;
		}


		IVsDataConnection site = e.Node.ExplorerConnection.Connection;

		if (site.State != DataConnectionState.Open || !RctManager.Available)
			return;

		LinkageParser.EnsureLoaded(site, typeof(DslProviderSchemaFactory));
	}



	private void OnNodeInserted(object sender, DataExplorerNodeEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnNodeInserted()");
	}



	private void OnNodeRemoving(object sender, DataExplorerNodeEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnNodeRemoving()");
	}


	#endregion Events


}
