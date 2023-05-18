// Not yet implemented

using System;
using System.Globalization;
using System.IO;

using Microsoft;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell.Interop;


using BlackbirdSql.Common;
using BlackbirdSql.Common.Commands;
using BlackbirdSql.Common.Providers;




namespace BlackbirdSql.VisualStudio.Ddex;



/// <summary>
/// Partly plagiarized off of <see cref="Microsoft.VisualStudio.Data.Providers.SqlServer.SqlViewDocumentProvider"/>.
/// </summary>
internal class TViewDocumentProvider : DataViewDocumentProvider
{

	private Hostess _Host;

	protected Hostess Host
	{
		get
		{
			_Host ??= new(Site.ServiceProvider);

			return _Host;
		}
	}

	public override int FindNode(string documentMoniker, bool searchUnpopulatedChildren)
	{
		Diag.Trace("FindNode moniker: " + documentMoniker);
		if (documentMoniker == null)
		{
			ArgumentNullException ex = new("documentMoniker");
			Diag.Dug(ex);
			throw ex;
		}

		if (!documentMoniker.StartsWith(SqlMonikerHelper.Prefix, StringComparison.OrdinalIgnoreCase))
		{
			return -1;
		}

		SqlMonikerHelper url = new SqlMonikerHelper(documentMoniker);

		return FindSqlNode(url, searchUnpopulatedChildren);

	}

	public override bool IsSupported(int itemId)
	{
		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);

		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| @object.Type.Name == "Index" || @object.Type.Name == "ForeignKey")
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
					return true;
			}
			else if (@object.Type.Name.EndsWith("Trigger") || @object.Type.Name == "View"
				|| @object.Type.Name == "StoredProcedure" || @object.Type.Name == "Function")
			{
				return true;
			}

		}

		return false;

	}

	public override string GetMoniker(int itemId)
	{
		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
		SqlMonikerHelper sqlMoniker = new SqlMonikerHelper(node);

		return sqlMoniker.Moniker;
	}



	public override bool CanOpen(int itemId, Guid logicalView)
	{
		bool result = false;
		if (IsSupported(itemId))
		{
			IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
			result = AbstractCommandProvider.CanOpen(node);
		}

		return result;
	}

	public override IVsWindowFrame Open(int itemId, Guid logicalView, object existingDocumentData, bool doNotShowWindow)
	{
		IVsWindowFrame result = null;
		if (IsSupported(itemId))
		{
			IVsDataViewCommonNodeInfo viewCommonNodeInfo = Site.GetViewCommonNodeInfo(itemId);
			IVsDataViewCommandInfo vsDataViewCommandInfo = viewCommonNodeInfo.Commands[DataToolsCommands.OpenTextObject];
			UniversalCommandProvider commandProvider = Site.GetProviderImplementation<IVsDataViewCommandProvider>(
				vsDataViewCommandInfo.CommandProviderType) as UniversalCommandProvider;

			IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);



			SqlMonikerHelper sqlMoniker = new(node);
			string moniker = sqlMoniker.ToString();

			Diag.Trace("Opening node " + node.Name + ": " + moniker);

			Guid guid = new(DataToolsCommands.SqlEditorGuid);



			commandProvider.ActivateOrOpenVirtualFile(moniker, existingDocumentData, guid, doNotShowWindow);

		}

		return result;
	}


	private int FindSqlNode(SqlMonikerHelper urlMoniker, bool searchUnpopulatedChildren)
	{
		try
		{
			SqlMonikerHelper prefixMoniker = null;

			Site.PersistentProperties.TryGetValue("MkDocumentPrefix", out object value);

			if (value is string prefix)
			{
				prefixMoniker = new SqlMonikerHelper(prefix);
			}

			Diag.Trace(prefixMoniker.Server + ":" + urlMoniker.Server + ", " + prefixMoniker.Database + ":" + urlMoniker.Database);


			if (prefixMoniker == null || !string.Equals(prefixMoniker.Server, urlMoniker.Server, StringComparison.OrdinalIgnoreCase) || !string.Equals(prefixMoniker.Database, urlMoniker.Database, StringComparison.OrdinalIgnoreCase))
			{
				return -1;
			}


			IVsDataObject vsDataObject = null;
			IVsDataObjectStore vsDataObjectStore = Site.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectStore)) as IVsDataObjectStore;

			try { Assumes.Present(vsDataObjectStore); }
			catch (Exception ex) { Diag.Dug(ex); throw ex; }


			IVsDataObjectCollection vsDataObjectCollection = vsDataObjectStore.SelectObjects(urlMoniker.ObjectType);

			if (vsDataObjectCollection.Count > 0)
			{
				vsDataObject = vsDataObjectCollection[0];
			}

			object[] identifier = urlMoniker.GetIdentifier();

			return SqlViewNodeLocator.LocateTextObjectNode(Site.ExplorerConnection, Site.CurrentView.Name, urlMoniker.ObjectType, identifier, searchUnpopulatedChildren)?.ItemId ?? (-1);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}


}
