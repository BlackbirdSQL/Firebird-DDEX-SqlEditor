using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using EnvDTE;

using Microsoft;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using BlackbirdSql.Common.Providers;
using BlackbirdSql.Common.Properties;



namespace BlackbirdSql.Common.Commands
{

	// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	// [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
	// [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]


	/// <summary>
	/// This class is plagiarized from Microsoft.VisualStudio.Data.Providers.SqlServer.SqlViewTextObjectCommandProvider.
	/// It is unused but serves as a reference for future commands for the AbstractCommandProvider classes.
	/// </summary>
	internal class TemplateTextCommandProvider : DataViewCommandProvider
	{
		private const string S_dropTriggerSql8OrBelow = "IF EXISTS (SELECT id\r\n           FROM dbo.sysobjects\r\n           WHERE uid = USER_ID(N'{0}')\r\n               AND xtype = 'TR'\r\n               AND name = N'{1}')\r\n    DROP TRIGGER {2}";

		private const string S_dropTriggerSql9OrAbove = "IF EXISTS (SELECT t.object_id\r\n           FROM sys.triggers t\r\n               INNER JOIN sys.objects o\r\n               ON (t.parent_id <> 0 AND t.parent_id = o.object_id)\r\n           WHERE o.schema_id = SCHEMA_ID(N'{0}')\r\n               AND t.name = N'{1}')\r\n    DROP TRIGGER {2}";

		private const string S_dropStoredProcedureSql8OrBelow = "IF EXISTS (SELECT id\r\n           FROM dbo.sysobjects\r\n           WHERE uid = USER_ID(N'{0}')\r\n               AND xtype IN ('P','RF')\r\n               AND name = N'{1}')\r\n    DROP PROCEDURE {2}";

		private const string S_dropStoredProcedureSql9OrAbove = "IF EXISTS (SELECT object_id\r\n           FROM sys.procedures\r\n           WHERE schema_id = SCHEMA_ID(N'{0}')\r\n               AND name = N'{1}')\r\n    DROP PROCEDURE {2}";

		private const string S_dropFunctionSql8OrBelow = "IF EXISTS (SELECT id\r\n           FROM dbo.sysobjects\r\n           WHERE uid = USER_ID(N'{0}')\r\n               AND xtype IN ('FN','TF','IF')\r\n               AND name = N'{1}')\r\n    DROP FUNCTION {2}";

		private const string S_dropFunctionSql9OrAbove = "IF EXISTS (SELECT object_id\r\n           FROM sys.objects\r\n           WHERE schema_id = SCHEMA_ID(N'{0}')\r\n               AND type IN ('FN','FS','TF','FT','IF')\r\n               AND name = N'{1}')\r\n    DROP FUNCTION {2}";

		private const string S_dropAggregateSql = "IF EXISTS (SELECT object_id\r\n           FROM sys.objects\r\n           WHERE schema_id = SCHEMA_ID(N'{0}')\r\n               AND type = 'AF'\r\n               AND name = N'{1}')\r\n    DROP AGGREGATE {2}";

		private const string S_dropUserDefinedTypeSql = "IF EXISTS (SELECT user_type_id\r\n           FROM sys.assembly_types\r\n           WHERE schema_id = SCHEMA_ID(N'{0}')\r\n               AND name = N'{1}')\r\n    DROP TYPE {2}";

		private const string S_assemblyObjectFullName = "Assemblies/SqlAssembly[{0}]/{1}[{2}]";

		private Hostess _host;

		private bool IsCurrentlyExecuting => AbstractTextEditorDocument.IsExecuting(Host);

		private bool IsDebuggingSupported => SqlTextEditorDocument.IsDebuggingSupported(Host);

		private bool IsCurrentlyDebugging => SqlTextEditorDocument.IsDebugging(Host);

		private Hostess Host
		{
			get
			{
				_host ??= new(Site.ServiceProvider);

				return _host;
			}
		}

		protected override MenuCommand CreateCommand(int itemId, CommandID commandId, object[] parameters)
		{
			MenuCommand command = null;
			if (commandId.Equals(DataToolsCommands.GlobalNewTrigger) || commandId.Equals(DataToolsCommands.NewTrigger))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					// Conditional out - Hide command for Firebird
					MenuCommand menuCommand13 = command;
					bool visible9 = command.Enabled = false;
					menuCommand13.Visible = visible9;
				}, delegate
				{
					OnNewTrigger(Site.ExplorerConnection.FindNode(itemId));
				});
			}

			if (commandId.Equals(DataToolsCommands.GlobalNewStoredProcedure) || commandId.Equals(DataToolsCommands.NewStoredProcedure))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					// Conditional out - Hide command for Firebird
					MenuCommand menuCommand12 = command;
					bool visible8 = command.Enabled = false;
					menuCommand12.Visible = visible8;
				}, delegate
				{
					if (commandId.Equals(DataToolsCommands.GlobalNewStoredProcedure))
					{
						OnNewStoredProcedure(null);
					}
					else
					{
						OnNewStoredProcedure(Site.ExplorerConnection.FindNode(itemId));
					}
				});
			}

			if (commandId.Equals(DataToolsCommands.GlobalNewScalarFunction) || commandId.Equals(DataToolsCommands.NewScalarFunction))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					// Conditional out - Hide command for Firebird
					MenuCommand menuCommand11 = command;
					bool visible7 = command.Enabled = false;
					menuCommand11.Visible = visible7;
				}, delegate
				{
					if (commandId.Equals(DataToolsCommands.GlobalNewScalarFunction))
					{
						OnNewScalarFunction(null);
					}
					else
					{
						OnNewScalarFunction(Site.ExplorerConnection.FindNode(itemId));
					}
				});
			}

			if (commandId.Equals(DataToolsCommands.GlobalNewTableValuedFunction) || commandId.Equals(DataToolsCommands.NewTableValuedFunction))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					// Conditional out - Hide command for Firebird
					MenuCommand menuCommand10 = command;
					bool visible6 = command.Enabled = false;
					menuCommand10.Visible = visible6;
				}, delegate
				{
					if (commandId.Equals(DataToolsCommands.GlobalNewTableValuedFunction))
					{
						OnNewTableValuedFunction(null);
					}
					else
					{
						OnNewTableValuedFunction(Site.ExplorerConnection.FindNode(itemId));
					}
				});
			}

			if (commandId.Equals(DataToolsCommands.GlobalNewInlineTableValuedFunction) || commandId.Equals(DataToolsCommands.NewInlineTableValuedFunction))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					// Conditional out - Hide command for Firebird
					MenuCommand menuCommand9 = command;
					bool visible5 = command.Enabled = false;
					menuCommand9.Visible = visible5;
				}, delegate
				{
					if (commandId.Equals(DataToolsCommands.NewInlineTableValuedFunction))
					{
						OnNewInlineFunction(null);
					}
					else
					{
						OnNewInlineFunction(Site.ExplorerConnection.FindNode(itemId));
					}
				});
			}

			if (commandId.Equals(DataToolsCommands.OpenTextObject))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					MenuCommand menuCommand7 = command;
					bool visible4 = command.Enabled = CanOpen(Site.ExplorerConnection.FindNode(itemId));
					menuCommand7.Visible = visible4;
					if (command.Visible)
					{
						command.Properties["Text"] = Resources.SqlViewTextObjectCommandProvider_Open;
					}

					// Conditional out - Hide command for Firebird
					MenuCommand menuCommand8 = command;
					visible4 = command.Enabled = false;
					menuCommand8.Visible = visible4;
				}, delegate
				{
					OnOpen(Site.ExplorerConnection.FindNode(itemId));
				});
			}

			if (commandId.Equals(DataToolsCommands.ExecuteTextObject))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					IVsDataExplorerNode node2 = Site.ExplorerConnection.FindNode(itemId);
					MenuCommand menuCommand5 = command;
					bool visible3 = command.Enabled = CanExecute(node2);
					menuCommand5.Visible = visible3;
					if (command.Visible)
					{
						command.Enabled = !IsCurrentlyDebugging;
						if (!IsCurrentlyExecuting)
						{
							command.Properties.Remove("Text");
						}
						else
						{
							command.Properties["Text"] = Resources.SqlViewTextObjectCommandProvider_CancelExecution;
						}
					}

					// Conditional out - Hide command for Firebird
					MenuCommand menuCommand6 = command;
					visible3 = command.Enabled = false;
					menuCommand6.Visible = visible3;
				}, delegate
				{
					OnExecuteOrCancel(Site.ExplorerConnection.FindNode(itemId));
				});
			}

			if (commandId.Equals(DataToolsCommands.DebugTextObject) && IsDebuggingSupported)
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
					MenuCommand menuCommand4 = command;
					bool visible2 = command.Enabled = CanDebug(node);
					menuCommand4.Visible = visible2;
					if (command.Visible)
					{
						command.Enabled = EnableDebug(node);
						command.Properties["Text"] = GetDebugCommandText(Site.ExplorerConnection.FindNode(itemId));
					}
				}, delegate
				{
					OnDebug(Site.ExplorerConnection.FindNode(itemId));
				});
			}

			if (commandId.Equals(StandardCommands.Copy))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					MenuCommand menuCommand3 = command;
					bool visible = command.Enabled = false;
					menuCommand3.Visible = visible;
				}, delegate
				{
				});
			}

			if (commandId.Equals(StandardCommands.Delete))
			{
				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					MenuCommand menuCommand = command;
					bool enabled = command.Visible = CanDelete(Site.ExplorerConnection.FindNode(itemId));
					menuCommand.Enabled = enabled;
					// Conditional out - Hide command for Firebird
					MenuCommand menuCommand2 = command;
					enabled = command.Enabled = false;
					menuCommand2.Visible = enabled;
				}, delegate
				{
					OnDelete(Site.ExplorerConnection.FindNode(itemId));
				});
			}

			command ??= base.CreateCommand(itemId, commandId, parameters);

			return command;
		}

		internal static bool CanOpen(IVsDataExplorerNode node)
		{
			if (node != null && node.Object != null)
			{
				IVsDataObject @object = node.Object;
				if (IsSystemFunction(@object) || TypeNameIn(@object.Type.Name, "Trigger", "ViewTrigger", "StoredProcedure", "Function") && IsTransactSqlEncryptedObject(@object))
				{
					return false;
				}
			}

			return true;
		}

		internal IVsWindowFrame Open(IVsDataExplorerNode node, bool doNotShowWindow)
		{
			IVsWindowFrame vsWindowFrame;
			if (!node.Object.Type.Name.Equals("SqlAssemblyFile"))
			{
				if (!ImplementationIsSqlClr(node.Object)
					&& !node.Object.Type.Name.Equals("Aggregate", StringComparison.OrdinalIgnoreCase)
					&& !node.Object.Type.Name.Equals("UserDefinedType", StringComparison.OrdinalIgnoreCase))
				{
					vsWindowFrame = Host.ActivateDocumentIfOpen(node.DocumentMoniker, doNotShowWindow);
					if (vsWindowFrame == null)
					{
						if (SqlOperationValidator.BlockUnsupportedConnections(Site.ExplorerConnection.Connection, Host))
						{
							return null;
						}

						SqlTextEditorDocument sqlTextEditorDocument = new SqlTextEditorDocument(node.Object, node.ItemId, Site);
						if (!doNotShowWindow)
						{
							sqlTextEditorDocument.Show();
						}

						vsWindowFrame = sqlTextEditorDocument.WindowFrame;
					}
				}
				else
				{
					vsWindowFrame = OnOpenAssemblyObject(node, doNotShowWindow);
				}
			}
			else
			{
				vsWindowFrame = OnOpenAssemblyFile(node, doNotShowWindow);
			}

			return vsWindowFrame;
		}

		private void OnNewTrigger(IVsDataExplorerNode launchingNode)
		{
			// SSDTWrapper.Instance.NewTrigger(Site.ExplorerConnection.Connection);
		}

		private void OnNewStoredProcedure(IVsDataExplorerNode launchingNode)
		{
			// SSDTWrapper.Instance.NewStoreProcedure(Site.ExplorerConnection.Connection);
		}

		private void OnNewScalarFunction(IVsDataExplorerNode launchingNode)
		{
			// SSDTWrapper.Instance.NewScalarFunction(Site.ExplorerConnection.Connection);
		}

		private void OnNewTableValuedFunction(IVsDataExplorerNode launchingNode)
		{
			// SSDTWrapper.Instance.NewTableValueFunction(Site.ExplorerConnection.Connection);
		}

		private void OnNewInlineFunction(IVsDataExplorerNode launchingNode)
		{
			// SSDTWrapper.Instance.NewInlineFunction(Site.ExplorerConnection.Connection);
		}

		private void OnOpen(IVsDataExplorerNode node)
		{
			// SSDTWrapper.Instance.ViewCode(Site.ExplorerConnection.Connection, node.Object);
		}

		private IVsWindowFrame OnOpenAssemblyObject(IVsDataExplorerNode node, bool doNotShowWindow)
		{
			object[] array = new object[3];
			object[] array2 = node.Object.Identifier.ToArray();
			array[0] = array2[0];
			array[1] = node.Object.Properties["AssemblyName"];

			IVsDataObjectStore vsDataObjectStore = Site.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectStore)) as IVsDataObjectStore;

			try { Assumes.Present(vsDataObjectStore); }
			catch (Exception ex) { Diag.Dug(ex); throw; }


			object[] array3 = new object[array2.Length + 1];
			array2.CopyTo(array3, 0);
			array3[array2.Length] = "SqlAssemblyFile";

			IVsDataObject @object = vsDataObjectStore.GetObject(node.Object.Type.Name + "ExtProperty", array3, refresh: true);

			if (@object == null)
			{
				InvalidOperationException ex = new(Resources.SqlViewTextObjectCommandProvider_NoSourceAvailable);
				Diag.Dug(ex);
				throw ex;
			}

			array[2] = @object.Properties["Value"];

			return ActivateOrOpenAssemblyFile(array, doNotShowWindow);
		}

		private IVsWindowFrame OnOpenAssemblyFile(IVsDataExplorerNode node, bool doNotShowWindow)
		{
			return ActivateOrOpenAssemblyFile(node.Object.Identifier.ToArray(), doNotShowWindow);
		}

		private IVsWindowFrame ActivateOrOpenAssemblyFile(object[] fileIdentifier, bool doNotShowWindowFrame)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			string text = null;
			bool flag = false;

			IVsDataObjectStore vsDataObjectStore = Site.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectStore)) as IVsDataObjectStore;

			try { Assumes.Present(vsDataObjectStore); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			IVsDataObject @object = vsDataObjectStore.GetObject("SqlAssemblyExtProperty", new object[3]
			{
				fileIdentifier[0],
				fileIdentifier[1],
				"SqlAssemblyProjectRoot"
			});
			string text2 = null;
			if (@object != null)
			{
				text2 = @object.Properties["Value"] as string;
			}

			if (text2 != null)
			{
				text = Path.Combine(text2, fileIdentifier[2] as string);
				if (Host.IsDocumentInAProject(text))
				{
					flag = true;
				}
				else
				{
					text = null;
				}
			}

			if (text == null)
			{
				IVsDataObject object2 = vsDataObjectStore.GetObject(string.Empty, new object[0]);
				string text3 = object2.Properties["Name"] as string;
				text3 = text3.Replace("\\", ".");
				string text4 = fileIdentifier[0] as string;
				text4 = text4.Replace("\\", "{backslash}");
				text4 = text4.Replace("/", "{slash}");
				text4 = text4.Replace(":", "{colon}");
				text4 = text4.Replace("*", "{asterisk}");
				text4 = text4.Replace("?", "{questionmark}");
				text4 = text4.Replace("\"", "{quote}");
				text4 = text4.Replace("<", "{openbracket}");
				text4 = text4.Replace(">", "{closebracket}");
				text4 = text4.Replace("|", "{bar}");
				text = Host.ApplicationDataDirectory;
				text = Path.Combine(text, "AssemblyFiles");
				text = Path.Combine(text, text3);
				text = Path.Combine(text, text4);
				text = Path.Combine(text, fileIdentifier[1] as string);
				text = Path.Combine(text, fileIdentifier[2] as string);
				if (text.Length > 260)
				{
					IVsDataObject object3 = vsDataObjectStore.GetObject("Database", new object[1] { fileIdentifier[0] });
					int num = (int)object3.Properties["Id"];
					text = Host.ApplicationDataDirectory;
					text = Path.Combine(text, "AssemblyFiles");
					text = Path.Combine(text, text3);
					text = Path.Combine(text, num.ToString(CultureInfo.InvariantCulture));
					text = Path.Combine(text, fileIdentifier[1] as string);
					text = Path.Combine(text, fileIdentifier[2] as string);
					if (text.Length > 260)
					{
						NotSupportedException ex = new(Resources.SqlViewTextObjectCommandProvider_AssemblyFileNameTooLong);
						Diag.Dug(ex);
						throw ex;
					}
				}

				if (!Directory.Exists(Path.GetDirectoryName(text)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(text));
				}
			}

			IVsWindowFrame vsWindowFrame = Host.ActivateDocumentIfOpen(text, doNotShowWindowFrame);
			if (vsWindowFrame != null)
			{
				return vsWindowFrame;
			}

			if (!flag)
			{
				IVsDataObject object4 = vsDataObjectStore.GetObject("SqlAssemblyFile", fileIdentifier, refresh: true);

				if (object4 == null)
				{
					InvalidOperationException ex = new(Resources.SqlViewTextObjectCommandProvider_NoSourceAvailable);
					Diag.Dug(ex);
					throw ex;
				}

				if (File.Exists(text))
				{
					File.SetAttributes(text, FileAttributes.Normal);
				}

				FileStream fileStream = File.Open(text, FileMode.Create);
				using (fileStream)
				{
					byte[] array = object4.Properties["FileBytes"] as byte[];
					fileStream.Write(array, 0, array.Length);
				}

				File.SetAttributes(text, FileAttributes.ReadOnly);
			}

			vsWindowFrame = Host.OpenDocumentViaProject(text);
			if (!flag)
			{
				NativeMethods.WrapComCall(vsWindowFrame.GetProperty(-4004, out object pvar));
				IVsTextLines vsTextLines = pvar as IVsTextLines;
				if (vsTextLines == null)
				{
					(pvar as IVsTextBufferProvider)?.GetTextBuffer(out vsTextLines);
				}

				if (vsTextLines != null)
				{
					NativeMethods.WrapComCall(vsTextLines.GetStateFlags(out uint pdwReadOnlyFlags));
					pdwReadOnlyFlags |= 1u;
					NativeMethods.WrapComCall(vsTextLines.SetStateFlags(pdwReadOnlyFlags));
				}
			}

			if (vsWindowFrame != null && !doNotShowWindowFrame)
			{
				NativeMethods.WrapComCall(vsWindowFrame.Show());
			}

			return vsWindowFrame;
		}

		private static bool CanExecute(IVsDataExplorerNode node)
		{
			if (node != null)
			{
				IVsDataObject @object = node.Object;
				if (@object == null || !TypeNameIn(@object.Type.Name, "StoredProcedure", "Function"))
				{
					return false;
				}
			}

			return true;
		}

		private void OnExecuteOrCancel(IVsDataExplorerNode node)
		{
			// SSDTWrapper.Instance.Execute(Site.ExplorerConnection.Connection, node.Object);
		}

		private static bool CanDebug(IVsDataExplorerNode node)
		{
			if (node != null)
			{
				IVsDataObject @object = node.Object;
				if (@object == null || !TypeNameIn(@object.Type.Name, "StoredProcedure", "Function"))
				{
					return false;
				}

				if (IsSystemFunction(@object) || IsInlineFunction(@object) || TypeNameIn(@object.Type.Name, "StoredProcedure", "Function") && IsTransactSqlEncryptedObject(@object))
				{
					return false;
				}
			}

			return true;
		}

		private bool EnableDebug(IVsDataExplorerNode node)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			if (node != null && node.Object != null)
			{
				IVsDataObject @object = node.Object;
				if (ImplementationIsSqlClr(@object))
				{
					int num = Site.PersistentCommands[DataToolsCommands.AllowSqlClrDebugging];
					if ((num & 4) == 0)
					{
						return false;
					}
				}
			}

			if (Site.ServiceProvider != null)
			{
				if (Site.ServiceProvider.GetService(typeof(DTE)) is DTE dTE && dTE.Debugger != null
					&& (dTE.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode
					|| dTE.Debugger.CurrentMode == dbgDebugMode.dbgRunMode))
				{
					return false;
				}
			}

			if (!IsCurrentlyExecuting)
			{
				return !IsCurrentlyDebugging;
			}

			return false;
		}

		private static string GetDebugCommandText(IVsDataExplorerNode node)
		{
			string result = null;
			string name = node.Object.Type.Name;
			if (name.Equals("StoredProcedure", StringComparison.Ordinal))
			{
				result = Resources.SqlViewTextObjectCommandProvider_StepIntoStoredProcedure;
			}

			if (name.Equals("Function", StringComparison.Ordinal))
			{
				result = Resources.SqlViewTextObjectCommandProvider_StepIntoFunction;
			}

			return result;
		}

		private void OnDebug(IVsDataExplorerNode node)
		{
			if (PromptSaveDocument(node))
			{
				SqlTextEditorDocument.ExecuteOrCancel(Host, node, debug: true, Site);
			}
		}

		private bool PromptSaveDocument(IVsDataExplorerNode node)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			IVsWindowFrame vsWindowFrame = Host.ActivateDocumentIfOpen(node.DocumentMoniker, doNotShowWindowFrame: true);
			if (vsWindowFrame != null)
			{
				NativeMethods.WrapComCall(vsWindowFrame.GetProperty(-4004, out object pvar));
				SqlTextEditorDocument sqlTextEditorDocument = pvar as SqlTextEditorDocument;
				if (sqlTextEditorDocument.IsDirty)
				{
					string name = node.Object.Type.Name;
					string text = name;
					if (name.Equals("Trigger", StringComparison.Ordinal))
					{
						text = Resources.SqlViewNode_Type_Trigger;
					}

					if (name.Equals("ViewTrigger", StringComparison.Ordinal))
					{
						text = Resources.SqlViewNode_Type_ViewTrigger;
					}

					if (name.Equals("StoredProcedure", StringComparison.Ordinal))
					{
						text = Resources.SqlViewNode_Type_StoredProcedure;
					}

					if (name.Equals("Function", StringComparison.Ordinal))
					{
						text = Resources.SqlViewNode_Type_Function;
					}

					string identifierString = GetIdentifierString(node.Object);
					DialogResult dialogResult = Host.ShowQuestion(string.Format(null, Resources.SqlViewTextObjectCommandProvider_PromptSaveObject, text, identifierString), MessageBoxDefaultButton.Button1);
					if (dialogResult == DialogResult.No)
					{
						return false;
					}

					return sqlTextEditorDocument.Save(VSSAVEFLAGS.VSSAVE_Save);
				}
			}

			return true;
		}

		private static bool CanDelete(IVsDataExplorerNode node)
		{
			if (node != null && node.Object != null)
			{
				IVsDataSourceVersionComparer vsDataSourceVersionComparer = node.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceVersionComparer)) as IVsDataSourceVersionComparer;

				try { Assumes.Present(vsDataSourceVersionComparer); }
				catch (Exception ex) { Diag.Dug(ex); throw; }

				if (IsSystemFunction(node.Object) || IsTrigger(node.Object) && vsDataSourceVersionComparer.CompareTo("11") >= 0)
				{
					return false;
				}
			}

			return true;
		}

		private void OnDelete(IVsDataExplorerNode node)
		{
			// SSDTWrapper.Instance.Delete(Site.ExplorerConnection.Connection, node.Object);
		}

		private static bool IsSystemFunction(IVsDataObject obj)
		{
			if (TypeNameIn(obj.Type.Name, "Function"))
			{
				return (bool)obj.Properties["IsSystemObject"];
			}

			return false;
		}

		private static bool IsInlineFunction(IVsDataObject obj)
		{
			if (TypeNameIn(obj.Type.Name, "Function"))
			{
				return (int)obj.Properties["FunctionType"] == 3;
			}

			return false;
		}

		private static bool IsTrigger(IVsDataObject obj)
		{
			return TypeNameIn(obj.Type.Name, "Trigger");
		}


		private static bool TypeNameIn(string typeName, params string[] values)
		{
			foreach (string value in values)
			{
				if (typeName.Equals(value, StringComparison.Ordinal))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsTransactSqlEncryptedObject(IVsDataObject obj)
		{
			if (ImplementationIsTransactSql(obj))
			{
				return (bool)obj.Properties["IsEncrypted"];
			}

			return false;
		}

		private static bool ImplementationIsTransactSql(IVsDataObject obj)
		{
			int num = 1;
			if (obj.Properties.ContainsKey("ImplementationType"))
			{
				return (int)obj.Properties["ImplementationType"] == num;
			}

			return true;
		}

		private static bool ImplementationIsSqlClr(IVsDataObject obj)
		{
			int num = 2;
			if (obj.Properties.ContainsKey("ImplementationType"))
			{
				return (int)obj.Properties["ImplementationType"] == num;
			}

			return false;
		}

		private string GetIdentifierString(IVsDataObject obj)
		{
			string name = obj.Type.Name;
			object[] array = obj.Identifier.ToArray();
			IVsDataObjectIdentifierConverter vsDataObjectIdentifierConverter = Site.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectIdentifierConverter)) as IVsDataObjectIdentifierConverter;

			try { Assumes.Present(vsDataObjectIdentifierConverter); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			array[0] = null;
			if (name.Equals("Trigger", StringComparison.Ordinal) || name.Equals("ViewTrigger", StringComparison.Ordinal))
			{
				array = new object[3]
				{
					array[0],
					array[1],
					array[3]
				};
				return vsDataObjectIdentifierConverter.ConvertToString("Table", array);
			}

			return vsDataObjectIdentifierConverter.ConvertToString(name, array);
		}
	}
}
