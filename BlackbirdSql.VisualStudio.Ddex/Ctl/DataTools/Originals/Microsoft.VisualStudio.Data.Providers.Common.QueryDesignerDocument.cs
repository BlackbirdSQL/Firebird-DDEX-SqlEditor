// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.QueryDesignerDocument
using System;
using Microsoft.VisualStudio.Data.Providers.Common;
using Microsoft.VisualStudio.Data.Providers.Common.Properties;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.DataTools.Interop;

internal class QueryDesignerDocument : DataToolsDocument
{
	private const string s_newMonikerFormat = "DataExplorer://{0}/Query{1}";

	private static int s_newQueryCount;

	private int _originalOwningItemId;

	protected override int WindowFrameAttributes => base.WindowFrameAttributes | 0x28;

	public QueryDesignerDocument(IVsDataViewHierarchy owningHierarchy)
		: base(string.Format(null, "DataExplorer://{0}/Query{1}", owningHierarchy.ExplorerConnection.DisplayName, ++s_newQueryCount), Guid.Empty, NativeMethods.GUID_Mode_QueryDesigner, typeof(IDTQueryDesignerFactory).GUID, owningHierarchy.CreateNewItem(), owningHierarchy)
	{
		base.Caption = string.Format(null, Resources.QueryDesignerDocument_NewQueryCaption, DataToolsDocument.BuildConnectionName(owningHierarchy.ExplorerConnection.Connection), s_newQueryCount);
		_originalOwningItemId = -1;
	}

	public QueryDesignerDocument(IVsDataObject obj, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: base(string.Format(null, DataToolsDocument.MonikerFormat, owningHierarchy.ExplorerConnection.DisplayName, obj.Type.Name, obj.Identifier.ToString()), Guid.Empty, NativeMethods.GUID_Mode_QueryDesigner, GetDesignerFactoryGuid(obj, owningHierarchy), owningHierarchy.CreateNewItem(), owningHierarchy)
	{
		base.Caption = string.Format(null, Resources.QueryDesignerDocument_RetrieveDataCaption, DataToolsDocument.BuildConnectionName(owningHierarchy.ExplorerConnection.Connection), obj.Identifier.ToString(DataObjectIdentifierFormat.ForDisplay));
		_originalOwningItemId = owningItemId;
	}

	public override void Show()
	{
		base.Show();
		if (base.UnderlyingDocument is IDTQueryDesigner)
		{
			base.Host.PostExecuteCommand(DataToolsCommands.ShowAddTableDialog);
		}
	}

	public override void Close()
	{
		base.OwningHierarchy.DiscardItem(base.OwningItemId);
		base.Close();
	}

	protected override void UpdateIsDirty()
	{
		base.IsDirty = false;
	}

	protected override IDTDocTool CreateUnderlyingDocument(object factory)
	{
		IDTDocTool result = null;
		if (factory is IDTQueryDesignerFactory iDTQueryDesignerFactory)
		{
			result = iDTQueryDesignerFactory.NewQuery(base.Moniker, base.ServiceProvider) as IDTDocTool;
		}
		if (factory is IDTTableDesignerFactory iDTTableDesignerFactory)
		{
			result = iDTTableDesignerFactory.BrowseTable(base.DSRef, base.ServiceProvider) as IDTDocTool;
		}
		if (factory is IDTViewDesignerFactory iDTViewDesignerFactory)
		{
			result = iDTViewDesignerFactory.BrowseView(base.DSRef, base.ServiceProvider) as IDTDocTool;
		}
		return result;
	}

	protected override object BuildDSRef()
	{
		IVsDataExplorerNode vsDataExplorerNode = base.OwningHierarchy.ExplorerConnection.FindNode(_originalOwningItemId);
		IVsDataObject vsDataObject = null;
		if (vsDataExplorerNode != null)
		{
			vsDataObject = vsDataExplorerNode.Object;
		}
		IVsDataObjectType type = null;
		if (vsDataObject != null)
		{
			type = vsDataObject.Type;
		}
		return BuildDSRef(type, vsDataObject.Identifier.ToArray());
	}

	private static Guid GetDesignerFactoryGuid(IVsDataObject obj, IVsDataViewHierarchy owningHierarchy)
	{
		Guid guid = Guid.Empty;
		if (!(owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectSupportModel)) is IVsDataObjectSupportModel vsDataObjectSupportModel))
		{
			throw new NotSupportedException();
		}
		if (vsDataObjectSupportModel.MappedTypes.ContainsKey("Table"))
		{
			foreach (IVsDataObjectType underlyingType in vsDataObjectSupportModel.MappedTypes["Table"].UnderlyingTypes)
			{
				if (underlyingType == obj.Type)
				{
					guid = typeof(IDTTableDesignerFactory).GUID;
					break;
				}
			}
		}
		if (guid == Guid.Empty && vsDataObjectSupportModel.MappedTypes.ContainsKey("View"))
		{
			foreach (IVsDataObjectType underlyingType2 in vsDataObjectSupportModel.MappedTypes["View"].UnderlyingTypes)
			{
				if (underlyingType2 == obj.Type)
				{
					guid = typeof(IDTViewDesignerFactory).GUID;
					break;
				}
			}
		}
		if (guid == Guid.Empty)
		{
			throw new NotSupportedException();
		}
		return guid;
	}
}
