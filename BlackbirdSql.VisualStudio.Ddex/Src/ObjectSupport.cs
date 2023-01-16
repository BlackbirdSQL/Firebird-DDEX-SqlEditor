using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex
{
	internal class ObjectSupport : DataObjectSupport
	{
		#region · Constructors ·

		public ObjectSupport() : this(null)
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
