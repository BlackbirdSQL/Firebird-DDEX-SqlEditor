//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using System.Drawing;
using Microsoft.VisualStudio.Data.Services;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TViewSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataViewSupport"/> and <see cref="IVsDataSupportImportResolver"/>
/// and <see cref="IVsDataViewIconProvider"/>interfaces.
/// </summary>
// =========================================================================================================
internal class TViewSupport : DataViewSupport, IVsDataSupportImportResolver, IVsDataViewIconProvider
{
	string _IconName = null;
	string _IconPrefix = null;
	Icon _Icon = null;


	string IconPrefix
	{
		get
		{
			if (_IconPrefix == null)
			{
				string[] nsparts = GetType().FullName.Split('.');
				nsparts[^1] = "Resources.ViewSupport.";

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
		// Diag.Trace();
	}

	public TViewSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
	{
		// Diag.Trace();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Event Implementations - TViewSupport
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




	#endregion Event Implementations





	// =========================================================================================================
	#region Method Implementations - TViewSupport
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service for the specified type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override object CreateService(Type serviceType)
	{
		// Diag.Trace();
		// TBC
		/*
		if (serviceType == typeof(IVsDataViewCommandProvider))
		{
			return new ViewDatabaseCommandProvider();
		}

		if (serviceType == typeof(IVsDataViewDocumentProvider))
		{
			return new ViewDocumentProvider();
		}
		*/


		object service = base.CreateService(serviceType);

		if (service == null)
			Diag.Trace(serviceType.FullName + " is not supported");
		else
			Diag.Trace(serviceType.FullName + " is indirectly supported");

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
			Diag.Dug(true, "Import resource not found: " + name);
			return null;
		}


		Type type = GetType();
		string resource = type.FullName + name[..^11] + "s.xml";

		// Diag.Trace(resource);

		return type.Assembly.GetManifestResourceStream(resource);
	}



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


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - TViewSupport
	// =========================================================================================================





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
				Diag.Dug(true, "Icon resource not found: " + name);
				return null;
			}

			Icon icon = new(GetType().Assembly.GetManifestResourceStream(name));
			if (icon != null)
			{
				_IconName = name;
				_Icon = icon;

				// OnIconsChanged(new DataViewNodeEventArgs(itemId));

				return icon;
			}
			else
			{
				Diag.Dug(true, "Icon resource invalid: " + name);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, "Icon not found: " + name);
		}

		return null;
	}


	#endregion Methods


}
