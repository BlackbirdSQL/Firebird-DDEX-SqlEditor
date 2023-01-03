using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data.Framework;

using BlackbirdSql.Common;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services;


/*
 * This experiment eventually fell flat in ToolsDocument.CreateUnderlyingDocument()
 * We're missing a trick on instantiating the COM object.
*/

namespace BlackbirdSql.VisualStudio.Ddex.Extensions
{
	[Guid(DataToolsCommands.UniversalQueryCommandProviderGuid)]
	internal class UniversalQueryCommandProvider : DataViewCommandProvider
	{

		protected override MenuCommand CreateCommand(int itemId, CommandID commandId, object[] parameters)
		{
			Diag.Dug();

			MenuCommand command = null;

			if (/*commandId.Equals(DataToolsCommands.GlobalNewQuery) ||*/ commandId.Equals(DataToolsCommands.NewQuery))
			{
				int qualityMetric = 262144;
				if (parameters != null && parameters[0] is int)
				{
					qualityMetric = (int)parameters[0];
				}

				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					OnNewQuery(itemId, qualityMetric);
				});
			}
			else if (commandId.Equals(DataToolsCommands.RetrieveData))
			{
				string textResource = null;
				if (parameters != null)
				{
					textResource = parameters[0] as string;
				}

				command = new DataViewMenuCommand(itemId, commandId, delegate
				{
					MenuCommand menuCommand = command;
					bool visible = (command.Enabled = base.Site.ExplorerConnection.FindNode(itemId).CanCopy);
					menuCommand.Visible = visible;
					if (!command.Properties.Contains("GotText"))
					{
						string retrieveDataText = GetRetrieveDataText(textResource);
						if (retrieveDataText != null)
						{
							command.Properties["Text"] = retrieveDataText;
						}

						command.Properties["GotText"] = true;
					}
				}, delegate
				{
					OnRetrieveData(base.Site.ExplorerConnection.FindNode(itemId));
				});
			}
			else
			{
				command = base.CreateCommand(itemId, commandId, parameters);
			}

			return command;
		}

		private void OnNewQuery(int itemId, int qualityMetricProvider)
		{
			Host host = new Host(base.Site.ServiceProvider);

			Diag.Dug();


			QueryDesignerDocument document = new QueryDesignerDocument(base.Site);
			Diag.Dug();
			document.Show();
			Diag.Dug();
			host.QueryDesignerProviderTelemetry(qualityMetricProvider);

			Diag.Dug();
		}

		private string GetRetrieveDataText(string resourceId)
		{
			string result = null;
			if (resourceId != null)
			{
				Host host = new Host(base.Site.ServiceProvider);

				IVsDataProvider vsDataProvider = host.GetService<IVsDataProviderManager>().Providers[base.Site.ExplorerConnection.Provider];

				result = vsDataProvider.GetString(resourceId);
			}

			return result;
		}

		private void OnRetrieveData(IVsDataExplorerNode node)
		{
			Host host = new Host(base.Site.ServiceProvider);

			if (!ToolsDocument.ActivateIfOpen(host, node))
			{
				new QueryDesignerDocument(node.Object, node.ItemId, base.Site).Show();
			}
		}
	}
}
