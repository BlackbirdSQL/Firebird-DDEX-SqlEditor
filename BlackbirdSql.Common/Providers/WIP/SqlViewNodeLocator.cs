
using System;
using System.Globalization;

using Microsoft;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.Common.Providers
{
	internal static class SqlViewNodeLocator
	{
		public static IVsDataExplorerNode LocateTableNode(IVsDataExplorerConnection explorerConnection, string view, object[] identifier)
		{
			IVsDataExplorerNode vsDataExplorerNode;
			string fullNameOfTableNodeParent = GetFullNameOfTableNodeParent(view, identifier);
			vsDataExplorerNode = explorerConnection.FindNode(fullNameOfTableNodeParent);
			if (vsDataExplorerNode == null || !vsDataExplorerNode.HasBeenExpanded)
			{
				return null;
			}

			return vsDataExplorerNode.Children.Find("Table", identifier);
		}

		public static string GetFullNameOfTableNodeParent(string view, object[] identifier)
		{
			string result = null;
			if (view.Equals("Classic View", StringComparison.Ordinal))
			{
				result = "Tables";
			}

			if (view.Equals("By Object Type", StringComparison.Ordinal))
			{
				result = "Tables/UserTables";
			}

			if (view.Equals("By Schema", StringComparison.Ordinal))
			{
				string text = identifier[1] as string;
				result = string.Format(CultureInfo.InvariantCulture, "Schemas/Schema[{0}]/Tables", text.Replace("]", "]]").Replace("/", "//"));
			}

			return result;
		}

		public static IVsDataExplorerNode LocateViewNode(IVsDataExplorerConnection explorerConnection, string view, object[] identifier)
		{
			IVsDataExplorerNode vsDataExplorerNode;
			string fullNameOfViewNodeParent = GetFullNameOfViewNodeParent(view, identifier);
			vsDataExplorerNode = explorerConnection.FindNode(fullNameOfViewNodeParent);
			if (vsDataExplorerNode == null || !vsDataExplorerNode.HasBeenExpanded)
			{
				return null;
			}

			return vsDataExplorerNode.Children.Find("View", identifier);
		}

		public static string GetFullNameOfViewNodeParent(string view, object[] identifier)
		{
			string result = null;
			if (view.Equals("Classic View", StringComparison.Ordinal))
			{
				result = "Views";
			}

			if (view.Equals("By Object Type", StringComparison.Ordinal))
			{
				result = "Views/UserViews";
			}

			if (view.Equals("By Schema", StringComparison.Ordinal))
			{
				string text = identifier[1] as string;
				result = string.Format(CultureInfo.InvariantCulture, "Schemas/Schema[{0}]/Views", text.Replace("]", "]]").Replace("/", "//"));
			}

			return result;
		}

		public static IVsDataExplorerNode LocateTextObjectNode(IVsDataExplorerConnection explorerConnection, string view, string typeName, object[] identifier, bool searchUnpopulatedChildren)
		{
			IVsDataExplorerNode vsDataExplorerNode;
			string fullNameOfTextNodeParent = GetFullNameOfTextNodeParent(explorerConnection, view, typeName, identifier);
			vsDataExplorerNode = explorerConnection.FindNode(fullNameOfTextNodeParent, searchUnpopulatedChildren);
			if (vsDataExplorerNode == null || (!vsDataExplorerNode.HasBeenExpanded && !searchUnpopulatedChildren))
			{
				return null;
			}

			return vsDataExplorerNode.Children.Find(typeName, identifier);
		}

		public static string GetFullNameOfTextNodeParent(IVsDataExplorerConnection explorerConnection, string view, string typeName, object[] identifier)
		{
			string text = string.Empty;
			bool flag = view.Equals("Classic View", StringComparison.Ordinal);
			bool flag2 = view.Equals("By Object Type", StringComparison.Ordinal);
			bool flag3 = view.Equals("By Schema", StringComparison.Ordinal);
			IVsDataObjectIdentifierConverter vsDataObjectIdentifierConverter = explorerConnection.Connection.GetService(typeof(IVsDataObjectIdentifierConverter)) as IVsDataObjectIdentifierConverter;

			try { Assumes.Present(vsDataObjectIdentifierConverter); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			if (flag3)
			{
				string text2 = vsDataObjectIdentifierConverter.ConvertToString("Schema", new object[2]
				{
					identifier[0],
					identifier[1]
				});
				text += string.Format(CultureInfo.InvariantCulture, "Schemas/Schema[{0}]/", text2.Replace("]", "]]").Replace("/", "//"));
			}

			if (typeName.Equals("Trigger", StringComparison.Ordinal))
			{
				if (flag2)
				{
					text += "Tables/User";
				}

				string text3 = vsDataObjectIdentifierConverter.ConvertToString("Table", new object[3]
				{
					identifier[0],
					identifier[1],
					identifier[2]
				});
				text += string.Format(CultureInfo.InvariantCulture, "Tables/Table[{0}]", text3.Replace("]", "]]").Replace("/", "//"));
				if (flag2 || flag3)
				{
					text += "/Triggers";
				}
			}
			else if (typeName.Equals("ViewTrigger", StringComparison.Ordinal))
			{
				if (flag2)
				{
					text += "Views/User";
				}

				string text4 = vsDataObjectIdentifierConverter.ConvertToString("View", new object[3]
				{
					identifier[0],
					identifier[1],
					identifier[2]
				});
				text += string.Format(CultureInfo.InvariantCulture, "Views/View[{0}]", text4.Replace("]", "]]").Replace("/", "//"));
				if (flag2 || flag3)
				{
					text += "/Triggers";
				}
			}
			else if (typeName.Equals("StoredProcedure", StringComparison.Ordinal))
			{
				text += "StoredProcedures";
				if (flag2)
				{
					text += "/UserStoredProcedures";
				}
			}
			else if (typeName.Equals("Function", StringComparison.Ordinal))
			{
				text += "Functions";
				if (flag2 || flag3)
				{
					text += "/ScalarValuedFunctions";
				}
			}
			else if (typeName.Equals("Aggregate", StringComparison.Ordinal))
			{
				text += "Functions";
				if (flag2 || flag3)
				{
					text += "/AggregateFunctions";
				}
			}
			else if (typeName.Equals("UserDefinedType", StringComparison.Ordinal))
			{
				if (flag)
				{
					text += "UserDefined";
				}

				text += "Types";
				if (flag2 || flag3)
				{
					text += "/UserDefinedTypes";
				}
			}

			return text;
		}
	}
}
