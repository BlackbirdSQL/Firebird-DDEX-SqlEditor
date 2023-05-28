using System;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.DataTools.Interop;
using BlackbirdSql.Common.Extensions;

namespace BlackbirdSql.Common.Providers
{
	internal class QueryDesignerDocument : ToolsDocument
	{
		private const string S_NewMonikerFormat = "DataExplorer://{0}/Query{1}";

		private static int S_NewQueryCount;

		private int _originalOwningItemId;

		protected override int WindowFrameAttributes => base.WindowFrameAttributes | 0x28;

		public QueryDesignerDocument(IVsDataViewHierarchy owningHierarchy)
			: base(string.Format(null, "DataExplorer://{0}/Query{1}", owningHierarchy.ExplorerConnection.DisplayName, ++S_NewQueryCount), Guid.Empty, NativeMethods.GUID_Mode_QueryDesigner, typeof(IDTQueryDesignerFactory).GUID, owningHierarchy.CreateNewItem(), owningHierarchy)
		{
			Diag.Dug();

			string connectionName = BuildConnectionName(owningHierarchy.ExplorerConnection.Connection);

			Diag.Dug();

			Caption = string.Format(null, Properties.Resources.QueryDesignerDocument_NewQueryCaption, connectionName, S_NewQueryCount);
			_originalOwningItemId = -1;

			Diag.Dug();
		}

		public QueryDesignerDocument(IVsDataObject obj, int owningItemId, IVsDataViewHierarchy owningHierarchy)
			: base(string.Format(null, MonikerFormat, owningHierarchy.ExplorerConnection.DisplayName, obj.Type.Name, obj.Identifier.ToString()), Guid.Empty, NativeMethods.GUID_Mode_QueryDesigner, GetDesignerFactoryGuid(obj, owningHierarchy), owningHierarchy.CreateNewItem(), owningHierarchy)
		{
			Diag.Dug();

			string connectionName = BuildConnectionName(owningHierarchy.ExplorerConnection.Connection);
			string dataObjectDisplayName = obj.Identifier.ToString(DataObjectIdentifierFormat.ForDisplay);

			Caption = string.Format(null, Properties.Resources.QueryDesignerDocument_RetrieveDataCaption, connectionName, dataObjectDisplayName);
			_originalOwningItemId = owningItemId;
		}

		public override void Show()
		{
			Diag.Dug();
			base.Show();
			Diag.Dug();
			if (UnderlyingDocument is IDTQueryDesigner)
			{
				Host.PostExecuteCommand(CommandProperties.ShowAddTableDialog);
			}
			Diag.Dug();
		}

		public override void Close()
		{
			Diag.Dug();
			OwningHierarchy.DiscardItem(OwningItemId);
			base.Close();
		}

		protected override void UpdateIsDirty()
		{
			Diag.Dug();
			IsDirty = false;
		}

		protected override IDTDocTool CreateUnderlyingDocument(object factory)
		{
			Diag.Dug();
			IDTDocTool result = null;
			IDTQueryDesignerFactory val = (IDTQueryDesignerFactory)(factory is IDTQueryDesignerFactory ? factory : null);
			if (val != null)
			{
				object obj = val.NewQuery(Moniker, (object)ServiceProvider);
				result = (IDTDocTool)(obj is IDTDocTool ? obj : null);
			}

			IDTTableDesignerFactory val2 = (IDTTableDesignerFactory)(factory is IDTTableDesignerFactory ? factory : null);
			if (val2 != null)
			{
				object obj2 = val2.BrowseTable(DSRef, (object)ServiceProvider);
				result = (IDTDocTool)(obj2 is IDTDocTool ? obj2 : null);
			}

			IDTViewDesignerFactory val3 = (IDTViewDesignerFactory)(factory is IDTViewDesignerFactory ? factory : null);
			if (val3 != null)
			{
				object obj3 = val3.BrowseView(DSRef, (object)ServiceProvider);
				result = (IDTDocTool)(obj3 is IDTDocTool ? obj3 : null);
			}

			return result;
		}

		protected override object BuildDSRef()
		{
			Diag.Dug();
			IVsDataExplorerNode vsDataExplorerNode = OwningHierarchy.ExplorerConnection.FindNode(_originalOwningItemId);
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
			Diag.Dug();
			Guid guid = Guid.Empty;
			IVsDataObjectSupportModel vsDataObjectSupportModel = owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectSupportModel)) as IVsDataObjectSupportModel;
			if (vsDataObjectSupportModel == null)
			{
				NotSupportedException ex = new();
				Diag.Dug(ex);
				throw ex;
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
				NotSupportedException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			return guid;
		}
	}
}
