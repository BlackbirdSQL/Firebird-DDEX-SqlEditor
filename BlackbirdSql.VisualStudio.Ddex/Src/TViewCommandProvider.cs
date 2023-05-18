// Not yet implemented

using System;
using System.ComponentModel.Design;

using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common.Commands;



namespace BlackbirdSql.VisualStudio.Ddex;



internal class TViewCommandProvider : UniversalCommandProvider
{
	private class ToggleMenuCommand : DataViewMenuCommand
	{
		private readonly ToggleEventHandler _Handler;

		public ToggleMenuCommand(int itemId, CommandID command, EventHandler statusHandler, ToggleEventHandler handler)
			: base(itemId, command, statusHandler, null)
		{
			_Handler = handler;
		}

		public override void Invoke(object arg)
		{
#pragma warning disable IDE0038 // Use pattern matching
			_Handler(arg is bool && (bool)arg);
#pragma warning restore IDE0038 // Use pattern matching
		}
	}

	private delegate void ToggleEventHandler(bool silent);





	protected override MenuCommand CreateSelectionCommand(CommandID commandId, object[] parameters)
	{
		MenuCommand command = null;
		if (commandId.Equals(StandardCommands.Copy))
		{
			bool flag = false;
			foreach (IVsDataExplorerNode selectedNode in Site.ExplorerConnection.SelectedNodes)
			{
				string text = (selectedNode.Object?.Type.Name);
				if (text != null && (text.Equals("StoredProcedureParameter", StringComparison.Ordinal)
					|| text.Equals("FunctionParameter", StringComparison.Ordinal) || text.Equals("AggregateParameter", StringComparison.Ordinal)))
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
				}, Site);
			}
		}

		command ??= base.CreateSelectionCommand(commandId, parameters);

		return command;
	}


}
