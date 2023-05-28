
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using BlackbirdSql.Common.Properties;



namespace BlackbirdSql.Common.Providers
{

	// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	// [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]


	/// <summary>
	/// Pagiarized off of Microsoft.VisualStudio.Data.Providers.SqlServer.SqlTextEditorDocument.
	/// </summary>
	public class SqlTextEditorDocument : AbstractTextEditorDocument, IVsTextBufferProvider
	{

		private bool _IsNew;

		private string _NewTriggerTableOrView;

		private string _NewObjectType;

		private object[] _NewIdentifier;

		private static int S_NewTriggerCount;

		private static int S_NewStoredProcedureCount;

		private static int S_NewFunctionCount;




		public SqlTextEditorDocument(string newObjectSchema, string newTriggerTableOrView, string newObjectType, string newObjectTypeDisplayName,
			IVsDataViewHierarchy owningHierarchy) : base(owningHierarchy.CreateNewItem(), owningHierarchy)
		{
			_IsNew = true;
			_NewTriggerTableOrView = newTriggerTableOrView;
			_NewObjectType = newObjectType;
			_NewIdentifier = CreateNewIdentifier(newObjectSchema, newObjectType, owningHierarchy);

			Moniker = AbstractDataToolsDocument.BuildObjectMoniker(newObjectType, _NewIdentifier, owningHierarchy);
			Caption = BuildCaption(newObjectTypeDisplayName, _NewIdentifier, owningHierarchy);

			owningHierarchy.SetNewItemSaveName(OwningItemId, _NewIdentifier[2] as string);
		}

		public SqlTextEditorDocument(IVsDataExplorerNode node, int owningItemId, IVsDataViewHierarchy owningHierarchy)
			: base(owningItemId, owningHierarchy)
		{
			SqlMonikerHelper sqlMoniker = new(node);

			string prop = SqlMonikerHelper.GetNodeScriptProperty(node.Object).ToUpper();

			_Source = node.Object.Properties[prop];


			object[] arr = node.Object.Identifier.ToArray();

			int len = 0;

			for (int i = 0; i < arr.Length; i++)
			{
				if (arr[i] != null)
					len++;
			}

			object[] identifier = new object[len];

			for (int i = 0, j = 0; i < arr.Length; i++)
			{
				if (arr[i] != null)
				{
					identifier[j] = arr[i];
					j++;
				}
			}

			// Moniker = BuildObjectMoniker(name, array, owningHierarchy);
			Moniker = sqlMoniker.ToString();

			Caption = BuildCaption(OwningHierarchy.GetViewCommonNodeInfo(owningItemId).TypeDisplayName, identifier, owningHierarchy);
		}

		public static string LoadText(IVsDataConnection connection, string typeName, object[] identifier)
		{

			string result = null;
			IVsDataCommand vsDataCommand = connection.GetService(typeof(IVsDataCommand)) as IVsDataCommand;

			try { Assumes.Present(vsDataCommand); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			string format = "SELECT c.[text]\r\nFROM [{0}].dbo.syscomments c\r\n\tINNER JOIN [{0}].dbo.sysobjects o ON c.id = o.id\r\nWHERE o.uid = USER_ID(N'{1}')\r\n\tAND o.name = N'{2}'\r\n\tAND c.number <= 1\r\nORDER BY c.colid";
			if (connection.GetService(typeof(IVsDataSourceVersionComparer)) is IVsDataSourceVersionComparer vsDataSourceVersionComparer
				&& vsDataSourceVersionComparer.CompareTo("9") >= 0)
			{
				format = "SELECT m.[definition]\r\nFROM [{0}].sys.sql_modules m\r\n\tINNER JOIN [{0}].sys.objects o\r\n\t\tON m.object_id = o.object_id\r\nWHERE o.schema_id = SCHEMA_ID(N'{1}')\r\n\tAND o.name = N'{2}'\r\n";
			}

			if (typeName.Equals("Trigger", StringComparison.Ordinal) || typeName.Equals("ViewTrigger", StringComparison.Ordinal))
			{
				identifier = new object[3]
				{
					identifier[0],
					identifier[1],
					identifier[3]
				};
			}

			IVsDataReader vsDataReader = vsDataCommand.Execute(string.Format(CultureInfo.InvariantCulture, format, identifier[0].ToString().Replace("]", "]]"), identifier[1].ToString().Replace("'", "''"), identifier[2].ToString().Replace("'", "''")));
			using (vsDataReader)
			{
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = false;
				while (vsDataReader.Read() && !vsDataReader.IsNullItem(0))
				{
					flag = true;
					stringBuilder.Append(vsDataReader.GetItem(0));
				}

				if (flag)
				{
					return stringBuilder.ToString();
				}

				return result;
			}
		}

		public static void ExecuteOrCancel(Hostess host, IVsDataExplorerNode node, bool debug, IVsDataViewHierarchy owningHierarchy)
		{
			if (SqlOperationValidator.BlockUnsupportedConnections(owningHierarchy.ExplorerConnection.Connection, host))
			{
				return;
			}

			int objectType = 0;
			if (node.Object.Type.Name.Equals("StoredProcedure", StringComparison.OrdinalIgnoreCase))
			{
				objectType = 65536;
			}

			if (node.Object.Type.Name.Equals("Function", StringComparison.OrdinalIgnoreCase))
			{
				switch ((int)node.Object.Properties["FunctionType"])
				{
					case 1:
						objectType = 262144;
						break;
					case 2:
						objectType = 524288;
						break;
					case 3:
						objectType = 1048576;
						break;
				}
			}

			AbstractTextEditorDocument.ExecuteOrCancel(host, node, debug, 1, objectType, node.Object.Identifier[1].ToString(), node.Object.Identifier[2].ToString(), owningHierarchy);
		}


		public override void Rename(string newMoniker, int attributes)
		{
			base.Rename(newMoniker, attributes);
			SqlMonikerHelper sqlMoniker = new SqlMonikerHelper(newMoniker);

			string typeDisplayName = Resources.ResourceManager.GetString("SqlViewNode_Caption_" + sqlMoniker.ObjectType);

			typeDisplayName ??= sqlMoniker.ObjectType;


			base.Caption = BuildCaption(typeDisplayName, new object[2] { sqlMoniker.Database, sqlMoniker.ObjectName }, base.OwningHierarchy);
		}

		public override bool Save(VSSAVEFLAGS saveFlags)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			string pbstrBuf;
			IVsTextLines ppTextBuffer;

			try
			{
				NativeMethods.WrapComCall(((IVsTextBufferProvider)this).GetTextBuffer(out ppTextBuffer));
				NativeMethods.WrapComCall(ppTextBuffer.GetLineCount(out var piLineCount));
				piLineCount--;
				NativeMethods.WrapComCall(ppTextBuffer.GetLengthOfLine(piLineCount, out var piLength));
				NativeMethods.WrapComCall(ppTextBuffer.GetLineText(0, 0, piLineCount, piLength, out pbstrBuf));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
			bool flag = false;
			string text = null;
			int index = 0;
			SkipCommentsAndWhiteSpace(pbstrBuf, ref index);
			int iPos = index;
			if (string.Compare(pbstrBuf, index, "CREATE", 0, 6, StringComparison.OrdinalIgnoreCase) == 0)
			{
				flag = true;
				index += "CREATE".Length;
			}
			else if (string.Compare(pbstrBuf, index, "ALTER", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
			{
				index += "ALTER".Length;
			}

			int num = index;
			SkipCommentsAndWhiteSpace(pbstrBuf, ref index);
			if (num == index)
			{
				NotSupportedException ex = new(Resources.SqlTextEditorDocument_UnsupportedStatementType);
				Diag.Dug(ex);
				throw ex;
			}

			if (string.Compare(pbstrBuf, index, "TRIGGER", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "Trigger";
				index += "TRIGGER".Length;
			}
			else if (string.Compare(pbstrBuf, index, "PROC", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "StoredProcedure";
				index += "PROC".Length;
				if (string.Compare(pbstrBuf, index, "EDURE", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
				{
					index += "EDURE".Length;
				}
			}
			else if (string.Compare(pbstrBuf, index, "FUNCTION", 0, 8, StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "Function";
				index += "FUNCTION".Length;
			}

			num = index;
			SkipCommentsAndWhiteSpace(pbstrBuf, ref index);
			if (num == index)
			{
				text = null;
			}

			if (text == null)
			{
				NotSupportedException ex = new(Resources.SqlTextEditorDocument_UnsupportedObjectType);
				Diag.Dug(ex);
				throw ex;
			}

			if (pbstrBuf != null)
			{
				(base.OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataCommand)) as IVsDataCommand)?.ExecuteWithoutResults(pbstrBuf);
			}

			if (flag)
			{
				if (ppTextBuffer is IVsTextStream vsTextStream)
				{
					IntPtr intPtr = Marshal.StringToCoTaskMemUni("ALTER");
					try
					{
						NativeMethods.WrapComCall(vsTextStream.ReplaceStream(iPos, "CREATE".Length, intPtr, "ALTER".Length));
					}
					finally
					{
						Marshal.FreeCoTaskMem(intPtr);
					}
				}
			}

			string text2 = null;
			object[] array = ParseIdentifier(pbstrBuf, ref index);
			if (text.Equals("StoredProcedure", StringComparison.Ordinal))
			{
				int num2 = 0;
				if (array.Length == 4)
				{
					num2 = (int)array[3];
					array = new object[3]
					{
						array[0],
						array[1],
						array[2]
					};
				}

				if (num2 > 1)
				{
					NotSupportedException ex = new(Resources.SqlTextEditorDocument_NumberedProceduresNotSupported);
					Diag.Dug(ex);
					throw ex;
				}
			}

			if (text.Equals("Trigger", StringComparison.Ordinal))
			{
				SkipCommentsAndWhiteSpace(pbstrBuf, ref index);
				index += 2;
				SkipCommentsAndWhiteSpace(pbstrBuf, ref index);
				num = index;
				if (string.Compare(pbstrBuf, index, "ALL", 0, 3, StringComparison.OrdinalIgnoreCase) == 0)
				{
					index += "ALL".Length;
					num = index;
					SkipCommentsAndWhiteSpace(pbstrBuf, ref index);
				}
				else if (string.Compare(pbstrBuf, index, "DATABASE", 0, 8, StringComparison.OrdinalIgnoreCase) == 0)
				{
					index += "DATABASE".Length;
					num = index;
					SkipCommentsAndWhiteSpace(pbstrBuf, ref index);
				}

				if (num < index)
				{
					NotSupportedException ex = new(Resources.SqlTextEditorDocument_DdlTriggersNotSupported);
					Diag.Dug(ex);
					throw ex;
				}

				object[] array2 = ParseIdentifier(pbstrBuf, ref index);
				text2 = array2[2] as string;
				array = new object[3]
				{
					array2[0],
					array2[1],
					array[2]
				};
			}

			IVsDataObjectStore vsDataObjectStore = OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectStore)) as IVsDataObjectStore;

			try { Assumes.Present(vsDataObjectStore); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			IVsDataObject vsDataObject;
			if (text2 != null)
			{
				object[] identifier = new object[4]
				{
					array[0],
					array[1],
					text2,
					array[2]
				};
				vsDataObject = vsDataObjectStore.GetObject(text, identifier, refresh: true);
				if (vsDataObject == null)
				{
					text = "ViewTrigger";
					vsDataObject = vsDataObjectStore.GetObject(text, identifier, refresh: true);
				}
			}
			else
			{
				vsDataObject = vsDataObjectStore.GetObject(text, array, refresh: true);
			}

			if (vsDataObject == null)
			{
				return false;
			}

			array = vsDataObject.Identifier.ToArray();
			if (array.Length == 4)
			{
				array = new object[3]
				{
					array[0],
					array[1],
					array[3]
				};
			}

			SqlMonikerHelper sqlMoniker = new(Moniker)
			{
				ObjectType = text,
				// ObjectSubType = objectSubType,
				// ObjectSchema = array[1] as string,
				ObjectName = array[2] as string
			};
			string text3 = sqlMoniker.ToString();
			array = vsDataObject.Identifier.ToArray();
			IVsDataExplorerNode vsDataExplorerNode = SqlViewNodeLocator.LocateTextObjectNode(OwningHierarchy.ExplorerConnection,
				OwningHierarchy.CurrentView.Name, text, array, false);
			if (vsDataExplorerNode == null)
			{
				int owningItemId = base.OwningItemId;
				int newItemId = base.OwningHierarchy.CreateNewItem();
				Host.RenameDocument(Moniker, newItemId, text3);
				OwningHierarchy.DiscardItem(owningItemId);
				string fullNameOfTextNodeParent = SqlViewNodeLocator.GetFullNameOfTextNodeParent(OwningHierarchy.ExplorerConnection,
					OwningHierarchy.CurrentView.Name, text, array);
				if (OwningHierarchy.DelayInsertItem(fullNameOfTextNodeParent, text, OwningItemId, array))
				{
					vsDataExplorerNode = OwningHierarchy.ExplorerConnection.FindNode(OwningItemId);
				}

				vsDataExplorerNode?.Select();
			}
			else
			{
				if (OwningItemId != vsDataExplorerNode.ItemId)
				{
					if (Host.ActivateDocumentIfOpen(vsDataExplorerNode.DocumentMoniker, doNotShowWindowFrame: true) != null)
					{
						int num3 = OwningHierarchy.CreateNewItem();
						object[] array3 = CreateNewIdentifier(array[1] as string, text, OwningHierarchy);
						string newDocumentMoniker = AbstractDataToolsDocument.BuildObjectMoniker(text, array3, OwningHierarchy);
						Host.RenameDocument(vsDataExplorerNode.DocumentMoniker, num3, newDocumentMoniker);
						OwningHierarchy.SetNewItemSaveName(num3, array3[2] as string);
					}

					int owningItemId2 = OwningItemId;
					Host.RenameDocument(Moniker, vsDataExplorerNode.ItemId, text3);
					IVsDataExplorerNode vsDataExplorerNode2 = OwningHierarchy.ExplorerConnection.FindNode(owningItemId2);
					if (!vsDataExplorerNode2.IsPlaced)
					{
						OwningHierarchy.DiscardItem(owningItemId2);
					}
				}

				vsDataExplorerNode.Refresh(noAsync: true);
			}

			IVsDataObjectChangeEventsBroker vsDataObjectChangeEventsBroker = OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectChangeEventsBroker)) as IVsDataObjectChangeEventsBroker;

			try { Assumes.Present(vsDataObjectChangeEventsBroker); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			if (flag)
				vsDataObjectChangeEventsBroker.RaiseObjectAdded(text, array);
			else
				vsDataObjectChangeEventsBroker.RaiseObjectChanged(text, array);

			try
			{
				NativeMethods.WrapComCall(ppTextBuffer.GetStateFlags(out uint pdwReadOnlyFlags));
				pdwReadOnlyFlags &= 0xFFFFFFFBu;
				NativeMethods.WrapComCall(ppTextBuffer.SetStateFlags(pdwReadOnlyFlags));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
			_IsNew = false;
			_NewTriggerTableOrView = null;
			_NewObjectType = null;
			_NewIdentifier = null;

			return true;
		}

		public override void Close()
		{
			IVsDataExplorerNode vsDataExplorerNode = null;
			if (!_IsNew)
			{
				vsDataExplorerNode = base.OwningHierarchy.ExplorerConnection.FindNode(base.OwningItemId);
			}

			if (vsDataExplorerNode == null || !vsDataExplorerNode.IsPlaced)
			{
				base.OwningHierarchy.DiscardItem(base.OwningItemId);
			}

			base.Close();
		}

		int IVsTextBufferProvider.GetTextBuffer(out IVsTextLines ppTextBuffer)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			return ((IVsTextBufferProvider)base.UnderlyingDocument.DocData).GetTextBuffer(out ppTextBuffer);
		}

		int IVsTextBufferProvider.LockTextBuffer(int fLock)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			return ((IVsTextBufferProvider)base.UnderlyingDocument.DocData).LockTextBuffer(fLock);
		}

		int IVsTextBufferProvider.SetTextBuffer(IVsTextLines pTextBuffer)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			return ((IVsTextBufferProvider)base.UnderlyingDocument.DocData).SetTextBuffer(pTextBuffer);
		}

		protected override string LoadText()
		{
			if (_Source != null)
				return (string)_Source;


			string text = null;
			if (_IsNew)
			{
				string format = null;
				if (_NewObjectType.Equals("Trigger", StringComparison.Ordinal))
				{
					format = "CREATE TRIGGER {1}\r\nON {0}\r\nFOR /* INSERT, UPDATE, DELETE */\r\nAS\r\n\t/* IF UPDATE() ... */\r\n";
				}
				else if (_NewObjectType.Equals("ViewTrigger", StringComparison.Ordinal))
				{
					format = "CREATE TRIGGER {1}\r\nON {0}\r\nINSTEAD OF /* INSERT, DELETE, UPDATE */\r\nAS\r\n\t/* IF UPDATE() ... */\r\n";
				}
				else if (_NewObjectType.Equals("StoredProcedure", StringComparison.Ordinal))
				{
					format = "CREATE PROCEDURE {0}\r\n\t/*\r\n\t(\r\n\t@parameter1 int = 5,\r\n\t@parameter2 datatype OUTPUT\r\n\t)\r\n\t*/\r\nAS\r\n\t/* SET NOCOUNT ON */\r\n\tRETURN\r\n";
				}
				else if (_NewObjectType.Equals("Function", StringComparison.Ordinal))
				{
					format = "CREATE FUNCTION {0}\r\n\t(\r\n\t/*\r\n\t@parameter1 int = 5,\r\n\t@parameter2 datatype\r\n\t*/\r\n\t)\r\nRETURNS /* datatype */\r\nAS\r\n\tBEGIN\r\n\tRETURN /* value */\r\n\tEND\r\n";
				}

				IVsDataObjectIdentifierConverter vsDataObjectIdentifierConverter = base.OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectIdentifierConverter)) as IVsDataObjectIdentifierConverter;

				try { Assumes.Present(vsDataObjectIdentifierConverter); }
				catch (Exception ex) { Diag.Dug(ex); throw; }

				string text3;
				if (_NewObjectType.Equals("Trigger", StringComparison.Ordinal) || _NewObjectType.Equals("ViewTrigger", StringComparison.Ordinal))
				{
					object[] identifier = new object[3]
					{
						null,
						_NewIdentifier[1],
						_NewTriggerTableOrView
					};
					string text2 = vsDataObjectIdentifierConverter.ConvertToString("Table", identifier);
					object[] identifier2 = new object[4]
					{
						null,
						null,
						null,
						_NewIdentifier[2]
					};
					text3 = vsDataObjectIdentifierConverter.ConvertToString(_NewObjectType, identifier2);
					text = string.Format(CultureInfo.InvariantCulture, format, text2, text3);
				}
				else
				{
					object[] identifier3 = new object[3]
					{
						null,
						_NewIdentifier[1],
						_NewIdentifier[2]
					};
					text3 = vsDataObjectIdentifierConverter.ConvertToString(_NewObjectType, identifier3);
					text = string.Format(CultureInfo.InvariantCulture, format, text3);
				}
			}
			else
			{
				IVsDataObject vsDataObject = null;
				IVsDataExplorerNode vsDataExplorerNode = base.OwningHierarchy.ExplorerConnection.FindNode(base.OwningItemId);
				if (vsDataExplorerNode != null)
				{
					vsDataObject = vsDataExplorerNode.Object;
				}

				if (vsDataObject != null)
				{
					text = LoadText(base.OwningHierarchy.ExplorerConnection.Connection, vsDataObject.Type.Name, vsDataObject.Identifier.ToArray());
					int index = 0;
					SkipCommentsAndWhiteSpace(text, ref index);
					if (index < text.Length)
					{
						text = text[..index] + "ALTER" + text[(index + "CREATE".Length)..];
						index += "ALTER".Length;
						SkipCommentsAndWhiteSpace(text, ref index);
						if (string.Compare(text, index, "TRIGGER", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
						{
							index += "TRIGGER".Length;
							SkipCommentsAndWhiteSpace(text, ref index);
							text = MaybeUpdateTriggerText(text, index, vsDataObject);
						}
					}
				}
			}

			return text;
		}


		private static object[] CreateNewIdentifier(string objectSchema, string objectType, IVsDataViewHierarchy owningHierarchy)
		{
			object[] result = null;
			if (objectType.Equals("Trigger", StringComparison.Ordinal) || objectType.Equals("ViewTrigger", StringComparison.Ordinal))
			{
				result = AbstractDataToolsDocument.BuildNewObjectIdentifier(objectSchema, "Table", "Trigger", ref S_NewTriggerCount, owningHierarchy);
			}

			if (objectType.Equals("StoredProcedure", StringComparison.Ordinal))
			{
				result = AbstractDataToolsDocument.BuildNewObjectIdentifier(objectSchema, objectType, objectType, ref S_NewStoredProcedureCount, owningHierarchy);
			}

			if (objectType.Equals("Function", StringComparison.Ordinal))
			{
				result = AbstractDataToolsDocument.BuildNewObjectIdentifier(objectSchema, objectType, objectType, ref S_NewFunctionCount, owningHierarchy);
			}

			return result;
		}

		private static string BuildCaption(string typeDisplayName, object[] identifier, IVsDataViewHierarchy owningHierarchy)
		{
			string caption = "";

			for (int i =0; i < identifier.Length; i++)
			{
				if (caption != "")
					caption += ".";
				caption += identifier[i];
			}

			return string.Format(null, Resources.ToolsDocument_Caption, BuildConnectionName(owningHierarchy.ExplorerConnection.Connection), typeDisplayName, caption);
		}

		private string MaybeUpdateTriggerText(string text, int index, IVsDataObject obj)
		{
			IVsDataObjectMemberComparer vsDataObjectMemberComparer = base.OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectMemberComparer)) as IVsDataObjectMemberComparer;

			try { Assumes.Present(vsDataObjectMemberComparer); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			IVsDataObjectIdentifierConverter vsDataObjectIdentifierConverter = base.OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectIdentifierConverter)) as IVsDataObjectIdentifierConverter;

			try { Assumes.Present(vsDataObjectIdentifierConverter); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			object[] array = ParseIdentifier(text, ref index, out int[] partIndexes);
			if (array[1] != null && vsDataObjectMemberComparer.Compare("Table", obj.Identifier.ToArray(), 1, array[1]) != 0)
			{
				string text2 = vsDataObjectIdentifierConverter.ConvertToString("Table", new object[3]
				{
					null,
					null,
					obj.Identifier[1]
				});
				int num = text2.Length - (partIndexes[3] - partIndexes[2]);
				text = text[..partIndexes[2]] + text2 + text[partIndexes[3]..];
				index += num;
			}

			SkipCommentsAndWhiteSpace(text, ref index);
			index += 2;
			SkipCommentsAndWhiteSpace(text, ref index);
			object[] array2 = ParseIdentifier(text, ref index, out int[] partIndexes2);
			if (array2[1] != null && vsDataObjectMemberComparer.Compare("Table", obj.Identifier.ToArray(), 1, array2[1]) != 0)
			{
				string text3 = vsDataObjectIdentifierConverter.ConvertToString("Table", new object[3]
				{
					null,
					null,
					obj.Identifier[1]
				});
				int num2 = text3.Length - (partIndexes2[3] - partIndexes2[2]);
				text = text[..partIndexes2[2]] + text3 + text[partIndexes2[3]..];
				index += num2;
				partIndexes2[4] += num2;
				partIndexes2[5] += num2;
			}

			if (array2[2] != null && vsDataObjectMemberComparer.Compare("Table", obj.Identifier.ToArray(), 2, array2[2]) != 0)
			{
				string text4 = vsDataObjectIdentifierConverter.ConvertToString("Table", new object[3]
				{
					null,
					null,
					obj.Identifier[2]
				});
				text = text[..partIndexes2[4]] + text4 + text[partIndexes2[5]..];
			}

			return text;
		}

		private static object[] ParseIdentifier(string text, ref int index)
		{
			return ParseIdentifier(text, ref index, out _);
		}

		private static object[] ParseIdentifier(string text, ref int index, out int[] partIndexes)
		{
			object[] array = new object[3];
			partIndexes = new int[6];
			for (int i = 0; i < 3; i++)
			{
				char c = '\0';
				if (text[index] == '[')
				{
					c = ']';
				}
				else if (text[index] == '"')
				{
					c = '"';
				}

				int num2 = -1;
				int num;
				if (c != 0)
				{
					num = ++index;
					while (text[index] != c || text[index + 1] == c)
					{
						if (text[index] == c)
						{
							index++;
						}

						index++;
					}

					array[2] = text[num..index];
					array[2] = (array[2] as string).Replace(new string(c, 2), c.ToString());
					partIndexes[4] = num - 1;
					partIndexes[5] = index + 1;
					index++;
					num2 = index;
					SkipCommentsAndWhiteSpace(text, ref num2);
				}
				else
				{
					num = index;
					num2 = index;
					while (text[index] != '.' && text[index] != ';' && text[index] != '(')
					{
						SkipCommentsAndWhiteSpace(text, ref num2);
						if (num2 > index)
						{
							break;
						}

						index++;
						num2 = index;
					}

					array[2] = text[num..index];
					partIndexes[4] = num;
					partIndexes[5] = index;
				}

				if (text[num2] == '.')
				{
					array[0] = array[1];
					partIndexes[0] = partIndexes[2];
					partIndexes[1] = partIndexes[3];
					array[1] = array[2];
					partIndexes[2] = partIndexes[4];
					partIndexes[3] = partIndexes[5];
					index = num2 + 1;
					SkipCommentsAndWhiteSpace(text, ref index);
					continue;
				}

				if (text[num2] != ';')
				{
					break;
				}

				index = num2 + 1;
				SkipCommentsAndWhiteSpace(text, ref index);
				num = index;
				num2 = index;
				while (text[index] != '(')
				{
					SkipCommentsAndWhiteSpace(text, ref num2);
					if (num2 > index)
					{
						break;
					}

					index++;
					num2 = index;
				}

				array = new object[4]
				{
					array[0],
					array[1],
					array[2],
					int.Parse(text[num..index ], CultureInfo.InvariantCulture)
				};
				partIndexes = new int[8]
				{
					partIndexes[0],
					partIndexes[1],
					partIndexes[2],
					partIndexes[3],
					partIndexes[4],
					partIndexes[5],
					num,
					index
				};
				break;
			}

			return array;
		}

		private static void SkipCommentsAndWhiteSpace(string text, ref int index)
		{
			while (index < text.Length)
			{
				while (index < text.Length && (char.IsControl(text[index]) || char.IsWhiteSpace(text[index])))
				{
					index++;
				}

				if (index < text.Length - 1 && text[index] == '-' && text[index + 1] == '-')
				{
					index += 2;
					while (index < text.Length && text[index] != '\n')
					{
						index++;
					}

					index++;
					continue;
				}

				if (index < text.Length - 1 && text[index] == '/' && text[index + 1] == '*')
				{
					index += 2;
					while (index < text.Length - 1 && (text[index] != '*' || text[index + 1] != '/'))
					{
						index++;
					}

					index += 2;
					continue;
				}

				break;
			}
		}
	}
}
