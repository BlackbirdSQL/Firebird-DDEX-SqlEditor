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
			Diag.Dug();
		}

		public ObjectSupport(IVsDataConnection connection) : base("BlackbirdSql.VisualStudio.Ddex.ObjectSupport", typeof(ObjectSupport).Assembly)
		{
			Diag.Dug();
		}

		#endregion


	}
}
