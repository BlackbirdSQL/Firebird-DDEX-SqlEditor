// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.QueryDesignerDocument
using System;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.DataTools.Interop;


namespace BlackbirdSql.Common.Ctl.DataTools;

/// <summary>
/// This roadblocks because key services in DataTools.Interop are protected.
/// </summary>
internal class QueryDesignerDocument : AbstractDataToolsDocument
{
	private const string C_NewMonikerFormat = "DataExplorer://{0}/Query{1}";

	private static int S_NewQueryCount;

	private readonly int _OriginalOwningItemId;

	protected override int WindowFrameAttributes => base.WindowFrameAttributes | 0x28;

	public QueryDesignerDocument(IVsDataViewHierarchy owningHierarchy)
		: base(C_NewMonikerFormat.FmtRes(owningHierarchy.ExplorerConnection.DisplayName, ++S_NewQueryCount),
			Guid.Empty, VS.CLSID_Mode_QueryDesigner, typeof(IDTQueryDesignerFactory).GUID,
			owningHierarchy.CreateNewItem(), owningHierarchy)
	{
		try
		{
			Caption = Resources.QueryDesignerDocument_NewQueryCaption.FmtRes(
				BuildConnectionName(owningHierarchy.ExplorerConnection.Connection), S_NewQueryCount);
			_OriginalOwningItemId = -1;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	public QueryDesignerDocument(IVsDataObject obj, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: base(string.Format(null, MonikerFormat, owningHierarchy.ExplorerConnection.DisplayName, obj.Type.Name,
			obj.Identifier.ToString()), Guid.Empty, VS.CLSID_Mode_QueryDesigner,
			GetDesignerFactoryGuid(obj, owningHierarchy), owningHierarchy.CreateNewItem(), owningHierarchy)
	{
		try
		{
			Caption = string.Format(null, Resources.QueryDesignerDocument_RetrieveDataCaption,
				BuildConnectionName(owningHierarchy.ExplorerConnection.Connection),
				obj.Identifier.ToString(DataObjectIdentifierFormat.ForDisplay));
			_OriginalOwningItemId = owningItemId;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

	}

	public override void Show()
	{
		Tracer.Trace(GetType(), "Show");
		base.Show();

		if (UnderlyingDocument is IDTQueryDesigner)
		{
			Tracer.Trace(GetType(), "Show", "Calling Hostess.PostExecuteCommand(CommandProperties.ShowAddTableDialog)");
			try
			{
				Host.PostExecuteCommand(CommandProperties.ShowAddTableDialog);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
		}
		else
		{
			ArgumentException ex = new("AbstractDataToolsDocument.UnderlyingDocument is not derived from IDTQueryDesigner");
			Diag.Dug(ex);
		}
	}

	public override void Close()
	{
		OwningHierarchy.DiscardItem(OwningItemId);
		base.Close();
	}

	protected override void UpdateIsDirty()
	{
		IsDirty = false;
	}

	protected override IDTDocTool CreateUnderlyingDocument(object factory)
	{
		IDTDocTool result = null;
		if (factory is IDTQueryDesignerFactory iDTQueryDesignerFactory)
		{
			result = iDTQueryDesignerFactory.NewQuery(Moniker, ServiceProvider) as IDTDocTool;
		}
		if (factory is IDTTableDesignerFactory iDTTableDesignerFactory)
		{
			result = iDTTableDesignerFactory.BrowseTable(DSRef, ServiceProvider) as IDTDocTool;
		}
		if (factory is IDTViewDesignerFactory iDTViewDesignerFactory)
		{
			result = iDTViewDesignerFactory.BrowseView(DSRef, ServiceProvider) as IDTDocTool;
		}
		return result;
	}

	protected override object BuildDSRef()
	{
		IVsDataExplorerNode vsDataExplorerNode = OwningHierarchy.ExplorerConnection.FindNode(_OriginalOwningItemId);
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
		if (owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectSupportModel))
			is not IVsDataObjectSupportModel vsDataObjectSupportModel)
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
