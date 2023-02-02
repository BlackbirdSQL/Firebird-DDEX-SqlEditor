// Not yet implemented

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Interop;
using Microsoft.VisualStudio.Data.Providers.Common;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell.Interop;


using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


internal class TViewDocumentProvider : DataViewDocumentProvider
{

	public override int FindNode(string documentMoniker, bool searchUnpopulatedChildren)
	{
		if (documentMoniker == null)
		{
			throw new ArgumentNullException("documentMoniker");
		}

		if (!documentMoniker.StartsWith("mssql:://", StringComparison.OrdinalIgnoreCase) && !documentMoniker.StartsWith("mssqlclr:://", StringComparison.OrdinalIgnoreCase))
		{
			return -1;
		}

		SqlObjectUrl url = new SqlObjectUrl(documentMoniker);
		if (documentMoniker.StartsWith("mssql:://", StringComparison.OrdinalIgnoreCase))
		{
			return FindTSqlNode(url, searchUnpopulatedChildren);
		}

		return FindSqlClrNode(url, searchUnpopulatedChildren);
	}

	public override bool IsSupported(int itemId)
	{
		IVsDataExplorerNode vsDataExplorerNode = base.Site.ExplorerConnection.FindNode(itemId);
		if (vsDataExplorerNode != null && vsDataExplorerNode.Object != null)
		{
			if (!vsDataExplorerNode.Object.Type.Name.Equals("Trigger", StringComparison.OrdinalIgnoreCase) && !vsDataExplorerNode.Object.Type.Name.Equals("ViewTrigger", StringComparison.OrdinalIgnoreCase) && !vsDataExplorerNode.Object.Type.Name.Equals("StoredProcedure", StringComparison.OrdinalIgnoreCase) && !vsDataExplorerNode.Object.Type.Name.Equals("Function", StringComparison.OrdinalIgnoreCase))
			{
				return vsDataExplorerNode.Object.Type.Name.Equals("SqlAssemblyFile", StringComparison.OrdinalIgnoreCase);
			}

			return true;
		}

		return false;
	}

	public override string GetMoniker(int itemId)
	{
		string result = null;
		IVsDataExplorerNode vsDataExplorerNode = base.Site.ExplorerConnection.FindNode(itemId);
		if (vsDataExplorerNode != null && vsDataExplorerNode.Object != null)
		{
			string name = vsDataExplorerNode.Object.Type.Name;
			if (name.Equals("Trigger", StringComparison.Ordinal) || name.Equals("ViewTrigger", StringComparison.Ordinal) || name.Equals("StoredProcedure", StringComparison.Ordinal) || name.Equals("Function", StringComparison.Ordinal))
			{
				int objectSubType = -1;
				if (name.Equals("Function", StringComparison.Ordinal))
				{
					objectSubType = (int)vsDataExplorerNode.Object.Properties["FunctionType"];
				}

				object[] array = vsDataExplorerNode.Object.Identifier.ToArray();
				if (name.Equals("Trigger", StringComparison.Ordinal) || name.Equals("ViewTrigger", StringComparison.Ordinal))
				{
					array = new object[3]
					{
						array[0],
						array[1],
						array[3]
					};
				}

				result = SqlDataToolsDocument.BuildObjectMoniker(name, objectSubType, array, base.Site);
			}
			else if (name.Equals("SqlAssemblyFile", StringComparison.Ordinal))
			{
				SqlObjectUrl sqlObjectUrl = new SqlObjectUrl();
				string text = vsDataExplorerNode.Object.Name;
				IVsDataObjectStore vsDataObjectStore = base.Site.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectStore)) as IVsDataObjectStore;
				IVsDataObject @object = vsDataObjectStore.GetObject("SqlAssemblyExtProperty", new object[3]
				{
					vsDataExplorerNode.Parent.Object.Identifier[0],
					vsDataExplorerNode.Parent.Object.Identifier[1],
					"SqlAssemblyProjectRoot"
				});
				if (@object != null)
				{
					string path = @object.Properties["Value"] as string;
					text = Path.Combine(path, text);
				}

				sqlObjectUrl.SqlClrFileName = text;
				result = sqlObjectUrl.ToString();
			}
		}

		return result;
	}

	public override bool CanOpen(int itemId, Guid logicalView)
	{
		bool result = false;
		if (IsSupported(itemId))
		{
			IVsDataExplorerNode node = base.Site.ExplorerConnection.FindNode(itemId);
			result = SqlViewTextObjectCommandProvider.CanOpen(node);
		}

		return result;
	}

	public override IVsWindowFrame Open(int itemId, Guid logicalView, object existingDocumentData, bool doNotShowWindow)
	{
		IVsWindowFrame result = null;
		if (IsSupported(itemId))
		{
			SqlViewTextObjectCommandProvider sqlViewTextObjectCommandProvider = null;
			IVsDataViewCommonNodeInfo viewCommonNodeInfo = base.Site.GetViewCommonNodeInfo(itemId);
			IVsDataViewCommandInfo vsDataViewCommandInfo = viewCommonNodeInfo.Commands[DataToolsCommands.OpenTextObject];
			sqlViewTextObjectCommandProvider = base.Site.GetProviderImplementation<IVsDataViewCommandProvider>(vsDataViewCommandInfo.CommandProviderType) as SqlViewTextObjectCommandProvider;
			IVsDataExplorerNode node = base.Site.ExplorerConnection.FindNode(itemId);
			result = sqlViewTextObjectCommandProvider.Open(node, doNotShowWindow: true);
		}

		return result;
	}

	private int FindTSqlNode(SqlObjectUrl url, bool searchUnpopulatedChildren)
	{
		SqlObjectUrl sqlObjectUrl = null;
		string text = null;
		object value = null;
		base.Site.PersistentProperties.TryGetValue("MkDocumentPrefix", out value);
		text = value as string;
		if (text != null)
		{
			sqlObjectUrl = new SqlObjectUrl(text);
		}

		if (sqlObjectUrl == null || !string.Equals(sqlObjectUrl.Server, url.Server, StringComparison.OrdinalIgnoreCase) || !string.Equals(sqlObjectUrl.Database, url.Database, StringComparison.OrdinalIgnoreCase))
		{
			return -1;
		}

		if (url.ObjectId > 0)
		{
			url.ResolveUsing(base.Site.ExplorerConnection.Connection);
		}

		object[] array = new object[3] { url.Database, url.ObjectSchema, url.ObjectName };
		if (url.ObjectType.Equals("Trigger", StringComparison.Ordinal) || url.ObjectType.Equals("ViewTrigger", StringComparison.Ordinal))
		{
			array = new object[4]
			{
				array[0],
				array[1],
				null,
				array[2]
			};
			IVsDataObject vsDataObject = null;
			IVsDataObjectStore vsDataObjectStore = base.Site.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectStore)) as IVsDataObjectStore;
			IVsDataObjectCollection vsDataObjectCollection = vsDataObjectStore.SelectObjects("Trigger", array);
			if (vsDataObjectCollection.Count == 0)
			{
				vsDataObjectCollection = vsDataObjectStore.SelectObjects("ViewTrigger", array);
			}

			if (vsDataObjectCollection.Count > 0)
			{
				vsDataObject = vsDataObjectCollection[0];
			}

			if (vsDataObject != null)
			{
				array[2] = vsDataObject.Identifier[2];
			}
		}

		return SqlViewNodeLocator.LocateTextObjectNode(base.Site.ExplorerConnection, base.Site.CurrentView.Name, url.ObjectType, url.ObjectSubType, array, searchUnpopulatedChildren)?.ItemId ?? (-1);
	}

	private int FindSqlClrNode(SqlObjectUrl url, bool searchUnpopulatedChildren)
	{
		SqlObjectUrl sqlObjectUrl = null;
		string text = null;
		object value = null;
		base.Site.PersistentProperties.TryGetValue("MkDocumentPrefix", out value);
		text = value as string;
		if (text != null)
		{
			sqlObjectUrl = new SqlObjectUrl(text);
		}

		if (sqlObjectUrl == null)
		{
			return -1;
		}

		NativeMethods.IDTInternalRunManager service = Host.GetService<NativeMethods.IDTInternalRunManager>(NativeMethods.SID_SDTInternalRunManager);
		IVsDataConnection debuggingConnection = service.DebuggingConnection;
		if (debuggingConnection == null)
		{
			return -1;
		}

		IServiceProvider serviceProvider = debuggingConnection as IServiceProvider;
		object service2 = serviceProvider.GetService(typeof(IVsDataSourceInformation));
		IVsDataSourceInformation val = (IVsDataSourceInformation)((service2 is IVsDataSourceInformation) ? service2 : null);
		url.Server = val.GetValue("DataSourceName") as string;
		if (sqlObjectUrl == null || !string.Equals(sqlObjectUrl.Server, url.Server, StringComparison.OrdinalIgnoreCase))
		{
			return -1;
		}

		SqlDebugServices sqlDebugServices = serviceProvider.GetService(typeof(ISqlDebugServices)) as SqlDebugServices;
		if (sqlDebugServices != null)
		{
			IList<string[]> candidateIdentifiersOfAssemblyFile = sqlDebugServices.GetCandidateIdentifiersOfAssemblyFile(url.SqlClrFileName);
			if (candidateIdentifiersOfAssemblyFile != null)
			{
				foreach (string[] item in candidateIdentifiersOfAssemblyFile)
				{
					if (string.Equals(item[0], sqlObjectUrl.Database, StringComparison.Ordinal))
					{
						url.Database = sqlObjectUrl.Database;
						url.ObjectType = "SqlAssembly";
						url.ObjectName = item[1];
						url.SqlClrFileName = item[2];
						break;
					}
				}
			}
		}

		if (url.Database == null)
		{
			return -1;
		}

		IVsDataExplorerNode vsDataExplorerNode = base.Site.ExplorerConnection.FindNode(string.Format(CultureInfo.InvariantCulture, "Assemblies/SqlAssembly[{0}]", url.ObjectName.Replace("]", "]]").Replace("/", "//")), searchUnpopulatedChildren);
		if (vsDataExplorerNode != null)
		{
			vsDataExplorerNode = vsDataExplorerNode.Children.Find("SqlAssemblyFile", new object[3] { url.Database, url.ObjectName, url.SqlClrFileName });
		}

		return vsDataExplorerNode?.ItemId ?? (-1);
	}
}
