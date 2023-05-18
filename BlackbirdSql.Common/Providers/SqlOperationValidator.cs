
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common.Properties;
using Microsoft;
using System;

namespace BlackbirdSql.Common.Providers
{
	internal class SqlOperationValidator
	{
		public static bool BlockUnsupportedConnections(IVsDataConnection connection, Hostess host)
		{
			if (IsServerFutureVersion(connection))
			{
				host.ShowMessage(Resources.SqlConnectionSupport_UnsupportedFutureVersion);
				return true;
			}

			return false;
		}


		public static bool IsServerFutureVersion(IVsDataConnection connection)
		{
			IVsDataSourceVersionComparer vsDataSourceVersionComparer = connection.GetService(typeof(IVsDataSourceVersionComparer)) as IVsDataSourceVersionComparer;

			try { Assumes.Present(vsDataSourceVersionComparer); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			return vsDataSourceVersionComparer.CompareTo("11.00.0000") >= 0;
		}
	}
}
