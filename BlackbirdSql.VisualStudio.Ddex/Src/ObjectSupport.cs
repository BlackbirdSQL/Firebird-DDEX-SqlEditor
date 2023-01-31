using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common;
using System.Reflection;

namespace BlackbirdSql.VisualStudio.Ddex
{
	internal class ObjectSupport : DataObjectSupport
	{
		#region · Constructors ·


		public ObjectSupport(string fileName, string path) : base(fileName, path)
		{
			Diag.Trace();
		}

		public ObjectSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
		{
			Diag.Trace();
		}

		public ObjectSupport(IVsDataConnection connection) : base("BlackbirdSql.VisualStudio.Ddex.ObjectSupport", typeof(ObjectSupport).Assembly)
		{
			Diag.Trace();
		}

		#endregion


	}
}
