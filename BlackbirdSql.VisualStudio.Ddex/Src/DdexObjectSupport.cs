using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common;
using System.Reflection;

namespace BlackbirdSql.VisualStudio.Ddex
{
	internal class DdexObjectSupport : DataObjectSupport
	{
		#region · Constructors ·


		public DdexObjectSupport(string fileName, string path) : base(fileName, path)
		{
			Diag.Trace();
		}

		public DdexObjectSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
		{
			Diag.Trace();
		}

		public DdexObjectSupport(IVsDataConnection connection) : base("BlackbirdSql.VisualStudio.Ddex.DdexObjectSupport", typeof(DdexObjectSupport).Assembly)
		{
			Diag.Trace();
		}

		#endregion


	}
}
