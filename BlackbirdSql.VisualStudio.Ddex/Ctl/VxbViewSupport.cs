// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbViewSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataViewSupport"/> and <see cref="IVsDataSupportImportResolver"/>
/// and <see cref="IVsDataViewIconProvider"/>interfaces.
/// Partly plagiarized off of Microsoft.VisualStudio.Data.Providers.SqlServer.SqlViewSupport.
/// </summary>
// =========================================================================================================
public class VxbViewSupport : DataViewSupport, IBsDataViewSupport
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors
	// ---------------------------------------------------------------------------------


	public VxbViewSupport(string fileName, string path) : base(fileName, path)
	{
		// Evs.Trace(typeof(VxbViewSupport), ".ctor(string, string)", "fileName: {0}, path: {1}", fileName, path);
	}

	public VxbViewSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
	{
		// Evs.Trace(typeof(VxbViewSupport), ".ctor(string, Assembly)", "resourceName: {0} assembly: {1}", resourceName, assembly.FullName);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - VxbViewSupport
	// =========================================================================================================


	private bool _PropertiesInitialized = false;

	private static string _IconName = null;
	private static string _IconPrefix = null;
	private static Icon _Icon = null;

	private IBsDataViewSupport.CloseDelegate _OnCloseEvent;

	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - VxbViewSupport
	// =========================================================================================================


	private string IconPrefix
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


	/// <summary>
	/// Accessor to the <see cref="IBsDataViewSupport.CloseDelegate"/> event.
	/// </summary>
	event IBsDataViewSupport.CloseDelegate IBsDataViewSupport.OnCloseEvent
	{
		add { _OnCloseEvent += value; }
		remove { _OnCloseEvent -= value; }
	}



	#endregion Property Accessors





	// =========================================================================================================
	#region Method Implementations - VxbViewSupport
	// =========================================================================================================


	/// <summary>
	/// This override is important because it fires the <see cref="IVsDataExplorerConnection.NodeRemoving"/>
	/// event for the ConnectionNode when the ExplorerConnection is deleted.
	/// </summary>
	public override void Close()
	{
		_OnCloseEvent?.Invoke(this, new());
		base.Close();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service for the specified type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override object CreateService(Type serviceType)
	{
		Evs.Trace(GetType(), nameof(CreateService), $"serviceType: {serviceType.Name}");

		/*
		if (serviceType == typeof(IVsDataViewCommandProvider))
		{
			return new VbxViewCommandProvider();
		}

		if (serviceType == typeof(IVsDataViewDocumentProvider))
		{
			return new VbxViewDocumentProvider();
		}
		*/

		// Evs.Trace(serviceType.FullName + " is not supported");

		object service = base.CreateService(serviceType);

		// if (service == null)
		//	Tracer.Trace(GetType(), "CreateService()", serviceType.FullName + " is not supported");
		// else
		//	Tracer.Trace(GetType(), "CreateService()", serviceType.FullName + " is indirectly supported");

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
			Diag.StackException(Resources.ExceptionImportResourceNotFound.FmtRes(name));
			return null;
		}


		Type type = GetType();
		string resource = type.FullName + name[..^11] + ".xml";

		// Evs.Debug(GetType(), nameof(ImportSupportStream), $"Resource: {resource}");

		return type.Assembly.GetManifestResourceStream(resource);
	}



	public override void Initialize()
	{
		Evs.Trace(GetType(), nameof(Initialize));

		base.Initialize();

		if (ViewHierarchy == null || ViewHierarchy.ExplorerConnection == null || Connection == null)
		{
			// Evs.Trace(GetType(), nameof(Initialize), "Exiting. ExplorerConnection is null.");
			return;
		}

		// Evs.Trace(GetType(), nameof(Initialize));

		IVsDataConnection site = Connection;

		if (site.State == DataConnectionState.Open)
			InitializeProperties();

		// Perform a single pass check to ensure this SE connection has had it's events
		// advised. This will also occur if the SE was adding a new conection and we
		// registered it with the Rct before it was registered with Server Explorer.
		RctManager.NotifyInitializedServerExplorerModel(this, new(ViewHierarchy.ExplorerConnection.ConnectionNode));
	}


	public override Stream OpenSupportStream()
	{
		// Evs.Trace(GetType(), nameof(OpenSupportStream));

		return base.OpenSupportStream();
	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - VxbViewSupport
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

		// Evs.Trace((expanded ? "Expanded" : "Closed") + " icon requested id: " + itemId + ":" + node.Name + ":" + node.FullName);

		string name = null;
		string[] nodes = node.FullName.Split(SystemData.C_UnixFieldSeparator);

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
			// Evs.Trace((expanded ? "Expanded" : "Closed") + " icon not defined id: " + itemId + ":" + node.Name + ":" + node.FullName);
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
			// Evs.Trace("Loading Icon: " + name);

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
			if (!_PropertiesInitialized && ViewHierarchy.PersistentProperties != null)
			{
				Csb csa = new(ViewHierarchy.ExplorerConnection.DecryptedConnectionString(), true);

				ViewHierarchy.PersistentProperties["MkDocumentPrefix"] = csa.LiveDatasetMoniker;

				_PropertiesInitialized = true;
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
	#region Events - VxbViewSupport
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Occurs when any icons have changed in the data view.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public event EventHandler<DataViewNodeEventArgs> IconsChanged;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// SE IconsChanged Event.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected virtual void OnIconsChanged(DataViewNodeEventArgs e)
	{
		IconsChanged?.Invoke(this, e);
	}


	#endregion Events


}
