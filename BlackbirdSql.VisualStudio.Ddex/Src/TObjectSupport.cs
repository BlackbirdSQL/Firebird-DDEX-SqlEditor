using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common;
using System.Reflection;

namespace BlackbirdSql.VisualStudio.Ddex
{
	internal class TObjectSupport : DataObjectSupport
	{
		#region · Constructors ·


		public TObjectSupport(string fileName, string path) : base(fileName, path)
		{
			Diag.Trace();
		}

		public TObjectSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
		{
			Diag.Trace();
		}

		public TObjectSupport(IVsDataConnection connection) : base("BlackbirdSql.VisualStudio.Ddex.TObjectSupport", typeof(TObjectSupport).Assembly)
		{
			Diag.Trace();
		}

		#endregion


	}
}
