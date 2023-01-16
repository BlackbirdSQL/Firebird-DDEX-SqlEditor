using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlackbirdSql.Data.ServiceHub
{
	/// <summary>
	/// Configures app.config for EntityFramework.BlackbirdSql if the developer created an edmx using the Dddex data tool
	/// </summary>
	public class DbConfigurationEx : System.Data.Entity.DbConfiguration
	{
		public DbConfigurationEx()
		{
			if (ServiceProvider.ProviderServices != null)
				SetProviderServices(Common.SystemData.Invariant, (System.Data.Entity.Core.Common.DbProviderServices)ServiceProvider.ProviderServices);
		}
	}
}
