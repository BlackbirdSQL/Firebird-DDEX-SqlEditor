// Not yet implemented

using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.VisualStudio.Ddex;


internal class DdexViewDatabaseCommandProvider : DataViewCommandProvider
{
	private class ToggleMenuCommand : DataViewMenuCommand
	{
		private ToggleEventHandler _handler;

		public ToggleMenuCommand(int itemId, CommandID command, EventHandler statusHandler, ToggleEventHandler handler)
			: base(itemId, command, statusHandler, null)
		{
			_handler = handler;
		}

		public override void Invoke(object arg)
		{
			_handler(arg is bool && (bool)arg);
		}
	}

	private delegate void ToggleEventHandler(bool silent);

	private bool _gotIsSql2005DebuggingSupported;

	private bool _isSql2005DebuggingSupported;

	private bool _gotCanDetachDatabase;

	private bool _canDetachDatabase;

	private Microsoft.VisualStudio.Data.Providers.Common.Host _host;

	private bool IsSql2005DebuggingSupported
	{
		get
		{
			if (!_gotIsSql2005DebuggingSupported)
			{
				IVsDataSourceVersionComparer vsDataSourceVersionComparer = base.Site.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceVersionComparer)) as IVsDataSourceVersionComparer;
				_isSql2005DebuggingSupported = vsDataSourceVersionComparer != null && vsDataSourceVersionComparer.CompareTo("9") >= 0 && SqlTextEditorDocument.IsDebuggingSupported(Host);
				_gotIsSql2005DebuggingSupported = true;
			}

			return _isSql2005DebuggingSupported;
		}
	}

	private bool CanEnableApplicationDebugging
	{
		get
		{
			bool result = false;
			IVsDataConnectionProperties connectionProperties = GetConnectionProperties();
			if ((bool)connectionProperties["Integrated Security"])
			{
				result = true;
				if (base.Site.ExplorerConnection.Connection.State == DataConnectionState.Open)
				{
					IVsDataCommand vsDataCommand = base.Site.ExplorerConnection.Connection.GetService(typeof(IVsDataCommand)) as IVsDataCommand;
					IVsDataReader vsDataReader = vsDataCommand.Execute("SELECT CONVERT(bit, IS_SRVROLEMEMBER('sysadmin'))");
					using (vsDataReader)
					{
						vsDataReader.Read();
						return (bool)vsDataReader.GetItem(0);
					}
				}
			}

			return result;
		}
	}

	private bool EnableApplicationDebugging
	{
		get
		{
			int num = base.Site.PersistentCommands[SqlDataToolsCommands.ApplicationDebugging];
			if (num == 0)
			{
				num = 3;
				base.Site.PersistentCommands[SqlDataToolsCommands.ApplicationDebugging] = num;
			}

			return (num & 4) != 0;
		}
	}

	private bool AllowSqlClrDebugging
	{
		get
		{
			int num = base.Site.PersistentCommands[SqlDataToolsCommands.AllowSqlClrDebugging];
			if (num == 0)
			{
				num = 3;
				string text = null;
				IVsDataConnectionProperties connectionProperties = GetConnectionProperties();
				text = connectionProperties["Data Source"] as string;
				IVsDataExplorerConnectionManager service = Host.GetService<IVsDataExplorerConnectionManager>();
				foreach (IVsDataExplorerConnection value in service.Connections.Values)
				{
					if (value.Provider != base.Site.ExplorerConnection.Provider || value == base.Site.ExplorerConnection)
					{
						continue;
					}

					connectionProperties.Parse(DataProtection.DecryptString(value.EncryptedConnectionString));
					if (string.Equals(text, connectionProperties["Data Source"] as string, StringComparison.OrdinalIgnoreCase))
					{
						IVsDataViewHierarchy otherViewHierarchy = base.Site.GetOtherViewHierarchy(value);
						int num2 = otherViewHierarchy.PersistentCommands[SqlDataToolsCommands.AllowSqlClrDebugging];
						if (((uint)num2 & (true ? 1u : 0u)) != 0)
						{
							num = num2;
							break;
						}
					}
				}

				base.Site.PersistentCommands[SqlDataToolsCommands.AllowSqlClrDebugging] = num;
			}

			return (num & 4) != 0;
		}
	}

	private bool CanDetachDatabase
	{
		get
		{
			if (!_gotCanDetachDatabase)
			{
				IVsDataSourceVersionComparer vsDataSourceVersionComparer = base.Site.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceVersionComparer)) as IVsDataSourceVersionComparer;
				if (vsDataSourceVersionComparer != null && vsDataSourceVersionComparer.CompareTo("9") >= 0)
				{
					IVsDataSourceInformation vsDataSourceInformation = base.Site.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
					if (vsDataSourceInformation != null && vsDataSourceInformation["DeskTopDataSource"] is bool && (bool)vsDataSourceInformation["DeskTopDataSource"] && base.Site.ExplorerConnection.Source == NativeMethods.GUID_MicrosoftSqlServerFileDataSource)
					{
						_canDetachDatabase = true;
					}
				}

				_gotCanDetachDatabase = true;
			}

			return _canDetachDatabase;
		}
	}

	private Microsoft.VisualStudio.Data.Providers.Common.Host Host
	{
		get
		{
			if (_host == null)
			{
				_host = new Microsoft.VisualStudio.Data.Providers.Common.Host(base.Site.ServiceProvider);
			}

			return _host;
		}
	}

	private bool IsExpressSku
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			AppId val = new AppId((IServiceProvider)Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
			return val.get_IsExpressSku();
		}
	}

	protected override MenuCommand CreateCommand(int itemId, CommandID commandId, object[] parameters)
	{
		MenuCommand command = null;
		if (commandId.Equals(SqlDataToolsCommands.ApplicationDebugging))
		{
			command = new ToggleMenuCommand(itemId, commandId, delegate
			{
				command.Supported = IsSql2005DebuggingSupported;
				MenuCommand menuCommand5 = command;
				bool visible4 = (command.Enabled = command.Supported);
				menuCommand5.Visible = visible4;
				command.Checked = command.Visible && EnableApplicationDebugging;
			}, delegate (bool silent)
			{
				SetEnableApplicationDebugging(!EnableApplicationDebugging, silent);
			});
		}

		if (commandId.Equals(SqlDataToolsCommands.AllowSqlClrDebugging))
		{
			command = new ToggleMenuCommand(itemId, commandId, delegate
			{
				command.Supported = IsSql2005DebuggingSupported;
				MenuCommand menuCommand4 = command;
				bool visible3 = (command.Enabled = command.Supported);
				menuCommand4.Visible = visible3;
				command.Checked = command.Visible && AllowSqlClrDebugging;
			}, delegate (bool silent)
			{
				SetAllowSqlClrDebugging(!AllowSqlClrDebugging, silent);
			});
		}

		if (commandId.Equals(SqlDataToolsCommands.EndApplicationDebugging))
		{
			command = new DataViewMenuCommand(itemId, commandId, delegate
			{
				SqlTextEditorDocument.EndAllDebug();
			});
		}

		if (commandId.Equals(SqlDataToolsCommands.DetachDatabase))
		{
			command = new DataViewMenuCommand(itemId, commandId, delegate
			{
				MenuCommand menuCommand3 = command;
				bool visible2 = (command.Enabled = CanDetachDatabase);
				menuCommand3.Visible = visible2;
				if (command.Visible)
				{
					command.Enabled = base.Site.ExplorerConnection.Connection.State == DataConnectionState.Open;
				}
			}, delegate
			{
				OnDetachDatabase();
			});
		}

		if (commandId.Equals(SqlDataToolsCommands.BrowseIntoSSDT))
		{
			command = new DataViewMenuCommand(itemId, commandId, delegate
			{
				MenuCommand menuCommand = command;
				bool visible = (command.Enabled = false);
				menuCommand.Visible = visible;
				if (!IsExpressSku && SSDTWrapper.Instance.SupportSSTDCommand(base.Site.ExplorerConnection.Connection))
				{
					MenuCommand menuCommand2 = command;
					visible = (command.Enabled = true);
					menuCommand2.Visible = visible;
				}
			}, delegate
			{
				SSDTWrapper.Instance.Browse(base.Site.ExplorerConnection.Connection);
			});
		}

		if (command == null)
		{
			command = base.CreateCommand(itemId, commandId, parameters);
		}

		return command;
	}

	protected override MenuCommand CreateSelectionCommand(CommandID commandId, object[] parameters)
	{
		MenuCommand command = null;
		if (commandId.Equals(StandardCommands.Copy))
		{
			bool flag = false;
			foreach (IVsDataExplorerNode selectedNode in base.Site.ExplorerConnection.SelectedNodes)
			{
				string text = ((selectedNode.Object != null) ? selectedNode.Object.Type.Name : null);
				if (text != null && (text.Equals("StoredProcedureParameter", StringComparison.Ordinal) || text.Equals("FunctionParameter", StringComparison.Ordinal) || text.Equals("AggregateParameter", StringComparison.Ordinal)))
				{
					flag = true;
					break;
				}
			}

			if (flag)
			{
				command = new DataViewSelectionMenuCommand(commandId, delegate
				{
					MenuCommand menuCommand = command;
					bool visible = (command.Enabled = false);
					menuCommand.Visible = visible;
				}, delegate
				{
				}, base.Site);
			}
		}

		if (command == null)
		{
			command = base.CreateSelectionCommand(commandId, parameters);
		}

		return command;
	}

	private void SetEnableApplicationDebugging(bool value, bool silent)
	{
		IVsDataSourceInformation dsi = base.Site.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
		if (SqlOperationValidator.IsServerSqlAzure(dsi))
		{
			if (!silent)
			{
				throw new InvalidOperationException(Resources.SqlAzure_OperationBlockMessage);
			}

			return;
		}

		if (SqlOperationValidator.IsServerFutureVersion(base.Site.ExplorerConnection.Connection))
		{
			if (!silent)
			{
				throw new InvalidOperationException(Resources.SqlConnectionSupport_UnsupportedFutureVersion);
			}

			return;
		}

		if (!EnableApplicationDebugging && value && !CanEnableApplicationDebugging)
		{
			if (!silent)
			{
				Exception ex = new InvalidOperationException(Resources.SqlViewDatabaseCommandProvider_CannotEnableApplicationDebugging);
				ex.HelpLink = "vdt.dataview.appdebugging";
				throw ex;
			}

			return;
		}

		if (!value)
		{
			base.Site.PersistentCommands[SqlDataToolsCommands.ApplicationDebugging] &= -5;
			return;
		}

		base.Site.PersistentCommands[SqlDataToolsCommands.ApplicationDebugging] |= 4;
		string text = null;
		IVsDataConnectionProperties connectionProperties = GetConnectionProperties();
		text = connectionProperties["Data Source"] as string;
		IVsDataExplorerConnectionManager service = Host.GetService<IVsDataExplorerConnectionManager>();
		foreach (IVsDataExplorerConnection value2 in service.Connections.Values)
		{
			if (!(value2.Provider != base.Site.ExplorerConnection.Provider) && value2 != base.Site.ExplorerConnection)
			{
				connectionProperties.Parse(DataProtection.DecryptString(value2.EncryptedConnectionString));
				if (string.Equals(text, connectionProperties["Data Source"] as string, StringComparison.OrdinalIgnoreCase))
				{
					IVsDataViewHierarchy otherViewHierarchy = base.Site.GetOtherViewHierarchy(value2);
					otherViewHierarchy.PersistentCommands[SqlDataToolsCommands.ApplicationDebugging] = 3;
				}
			}
		}
	}

	private void SetAllowSqlClrDebugging(bool value, bool silent)
	{
		IVsDataSourceInformation dsi = base.Site.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
		if (SqlOperationValidator.IsServerSqlAzure(dsi))
		{
			if (!silent)
			{
				throw new InvalidOperationException(Resources.SqlAzure_OperationBlockMessage);
			}

			return;
		}

		if (SqlOperationValidator.IsServerFutureVersion(base.Site.ExplorerConnection.Connection))
		{
			if (!silent)
			{
				throw new InvalidOperationException(Resources.SqlConnectionSupport_UnsupportedFutureVersion);
			}

			return;
		}

		if (!AllowSqlClrDebugging && value && !silent)
		{
			DialogResult dialogResult = Host.ShowQuestion(Resources.SqlViewDatabaseCommandProvider_WarnBeforeAllowSqlClrDebugging, "vdt.dataview.allowsqlclrdebugging");
			if (dialogResult == DialogResult.No)
			{
				return;
			}
		}

		if (!value)
		{
			base.Site.PersistentCommands[SqlDataToolsCommands.AllowSqlClrDebugging] &= -5;
		}
		else
		{
			base.Site.PersistentCommands[SqlDataToolsCommands.AllowSqlClrDebugging] |= 4;
		}

		string text = null;
		IVsDataConnectionProperties connectionProperties = GetConnectionProperties();
		text = connectionProperties["Data Source"] as string;
		IVsDataExplorerConnectionManager service = Host.GetService<IVsDataExplorerConnectionManager>();
		foreach (IVsDataExplorerConnection value2 in service.Connections.Values)
		{
			if (!(value2.Provider != base.Site.ExplorerConnection.Provider) && value2 != base.Site.ExplorerConnection)
			{
				connectionProperties.Parse(DataProtection.DecryptString(value2.EncryptedConnectionString));
				if (string.Equals(text, connectionProperties["Data Source"] as string, StringComparison.OrdinalIgnoreCase))
				{
					IVsDataViewHierarchy otherViewHierarchy = base.Site.GetOtherViewHierarchy(value2);
					otherViewHierarchy.PersistentCommands[SqlDataToolsCommands.AllowSqlClrDebugging] = base.Site.PersistentCommands[SqlDataToolsCommands.AllowSqlClrDebugging];
				}
			}
		}
	}

	private void OnDetachDatabase()
	{
		string text = GetConnectionProperties()["AttachDbFilename"] as string;
		if (text != null)
		{
			IVsTrackProjectDocuments3 vsTrackProjectDocuments = Host.TryGetService<SVsTrackProjectDocuments, IVsTrackProjectDocuments3>();
			if (vsTrackProjectDocuments != null)
			{
				NativeMethods.WrapComCall(vsTrackProjectDocuments.HandsOffFiles(7u, 1, new string[1] { text }));
			}
		}
	}

	private IVsDataConnectionProperties GetConnectionProperties()
	{
		IVsDataConnectionProperties vsDataConnectionProperties = null;
		IVsDataProviderManager service = Host.GetService<IVsDataProviderManager>();
		IVsDataProvider vsDataProvider = service.Providers[base.Site.ExplorerConnection.Provider];
		vsDataConnectionProperties = vsDataProvider.CreateObject<IVsDataConnectionProperties>(base.Site.ExplorerConnection.Source);
		vsDataConnectionProperties.Parse(DataProtection.DecryptString(base.Site.ExplorerConnection.EncryptedConnectionString));
		return vsDataConnectionProperties;
	}
}
